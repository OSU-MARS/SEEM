using Mars.Seem.Extensions;
using Mars.Seem.Tree;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "VolumeTable")]
    public class WriteVolumeTable : WriteCmdlet
    {
        [Parameter]
        [ValidateNotNull]
        public TreeVolume TreeVolume { get; set; }

        public WriteVolumeTable()
        {
            this.TreeVolume = TreeVolume.Default;
        }

        protected override void ProcessRecord()
        {
            using StreamWriter writer = this.GetWriter();

            // header
            if (this.ShouldWriteHeader())
            {
                writer.WriteLine("logLength,species,height,DBH,heightDiameterRatio,logs,logs2S,logs3S,logs4S,cubic2S,cubic3S,cubic4S,cubic,scribner2S,scribner3S,scribner4S,scribner,fellingDiameter15,fellingDiameter30,maxFeedRollerDiameter,neiloidHeight,neiloidCubic");
            }

            this.WriteScaledVolume(writer, this.TreeVolume.RegenerationHarvest);
            this.WriteScaledVolume(writer, this.TreeVolume.Thinning);
        }

        private void WriteScaledVolume(StreamWriter writer, ScaledVolume scaledVolume)
        {
            float minimumFeedRollerHeight = Constant.Bucking.DefaultStumpHeightInM + Constant.Bucking.ProcessingHeadFeedRollerHeightInM;
            //Debug.Assert(Constant.Bucking.StumpHeight <= PoudelRegressions.MinimumKozakHeightInM);
            //Debug.Assert(minimumFeedRollerHeight <= PoudelRegressions.MinimumKozakHeightInM);
            Debug.Assert(minimumFeedRollerHeight <= Constant.DbhHeightInM);

            string logLengthAsString = scaledVolume.PreferredLogLengthInMeters.ToString(CultureInfo.InvariantCulture);
            long estimatedBytesSinceLastFileLength = 0;
            long knownFileSizeInBytes = 0;
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            foreach (FiaCode species in scaledVolume.VolumeBySpecies.Keys)
            {
                Func<float, float, float, float, float> getDiameterOutsideBark = species switch
                {
                    FiaCode.PseudotsugaMenziesii => DouglasFir.GetDiameterOutsideBark,
                    FiaCode.TsugaHeterophylla => (float dbhInCm, float heightInM, float _, float evaluationHeightInM) =>
                    {
                        return WesternHemlock.GetDiameterOutsideBark(dbhInCm, heightInM, evaluationHeightInM);
                    },
                    _ => throw new NotSupportedException("Unhandled species " + species + ".")
                };
                Func<float, float, float> getNeiloidHeight = species switch
                {
                    FiaCode.PseudotsugaMenziesii => DouglasFir.GetNeiloidHeight,
                    FiaCode.TsugaHeterophylla => WesternHemlock.GetNeiloidHeight,
                    _ => throw new NotSupportedException("Unhandled species " + species + ".")
                };

                string speciesPrefix = String.Concat(logLengthAsString, ",", FiaCodeExtensions.ToFourLetterCode(species));
                TreeSpeciesVolumeTable volumeTable = scaledVolume.VolumeBySpecies[species];
                for (int heightIndex = 0; heightIndex < volumeTable.HeightClasses; ++heightIndex)
                {
                    // for now, use fixed 40% crown ratio in estimating bark thickness with Maguire and Hann 1990 regressions
                    // Height:diameter ratio could potentially be used as a modifier, with higher ratio trees having shorter crowns.
                    float heightInM = volumeTable.GetHeight(heightIndex);
                    float heightToCrownBaseInM = (1.0F - 0.4F) * heightInM;
                    string speciesAndHeightPrefix = speciesPrefix + "," + heightInM.ToString(Constant.Default.HeightInMFormat, CultureInfo.InvariantCulture);
                    for (int dbhIndex = 0; dbhIndex < volumeTable.DiameterClasses; ++dbhIndex)
                    {
                        float dbhInCm = volumeTable.GetDiameter(dbhIndex);

                        float heightDiameterRatio = heightInM / (0.01F * dbhInCm);
                        int logs2saw = volumeTable.Logs2Saw[dbhIndex, heightIndex];
                        int logs3saw = volumeTable.Logs3Saw[dbhIndex, heightIndex];
                        int logs4saw = volumeTable.Logs4Saw[dbhIndex, heightIndex];
                        int logs = logs2saw + logs3saw + logs4saw;
                        float cubic2saw = volumeTable.Cubic2Saw[dbhIndex, heightIndex];
                        float cubic3saw = volumeTable.Cubic3Saw[dbhIndex, heightIndex];
                        float cubic4saw = volumeTable.Cubic4Saw[dbhIndex, heightIndex];
                        float cubicTotal = cubic2saw + cubic3saw + cubic4saw;
                        float scribner2saw = volumeTable.Scribner2Saw[dbhIndex, heightIndex];
                        float scribner3saw = volumeTable.Scribner3Saw[dbhIndex, heightIndex];
                        float scribner4saw = volumeTable.Scribner4Saw[dbhIndex, heightIndex];
                        float scribnerTotal = scribner2saw + scribner3saw + scribner4saw;

                        // below breast height, diameter and bark regressions behaved incorrectly on trees too small to be merchantable as sawtimber
                        // These trees are therefore excluded from checking of felling cut diameter and diameter at minimum feed roller height.
                        // Feed roller diameter estimation is also incorrect for trees with unusually low height-diameter ratios. So these
                        // trees are, for now, also excluded.
                        float fellingDiameterAt15cm = dbhInCm;
                        float fellingDiameterAt30cm = dbhInCm;
                        float maxFeedRollerDiameterInCm = dbhInCm;
                        float neiloidHeightInM = 0.0F;
                        if ((dbhInCm > 0.0F) && (heightInM > Constant.DbhHeightInM))
                        {
                            fellingDiameterAt15cm = getDiameterOutsideBark(dbhInCm, heightInM, heightToCrownBaseInM, 0.5F * Constant.MetersPerFoot);
                            fellingDiameterAt30cm = getDiameterOutsideBark(dbhInCm, heightInM, heightToCrownBaseInM, Constant.MetersPerFoot);
                            maxFeedRollerDiameterInCm = getDiameterOutsideBark(dbhInCm, heightInM, heightToCrownBaseInM, minimumFeedRollerHeight);
                            neiloidHeightInM = getNeiloidHeight(dbhInCm, heightInM);

                            // don't check fellingDiameter15 > fellingDiameter30 because regressions aren't necessarily consistent
                            Debug.Assert(fellingDiameterAt15cm > dbhInCm);
                            Debug.Assert(fellingDiameterAt30cm > dbhInCm);
                            Debug.Assert((fellingDiameterAt30cm >= maxFeedRollerDiameterInCm) && (maxFeedRollerDiameterInCm > dbhInCm));
                        }
                        float unscaledNeiloidVolumeInM3 = volumeTable.UnscaledNeiloidCubic[dbhIndex, heightIndex];

                        string line = speciesAndHeightPrefix + "," +
                            dbhInCm.ToString(Constant.Default.DiameterInCmFormat, CultureInfo.InvariantCulture) + "," +
                            heightDiameterRatio.ToString(CultureInfo.InvariantCulture) + "," +
                            logs.ToString(CultureInfo.InvariantCulture) + "," +
                            logs2saw.ToString(CultureInfo.InvariantCulture) + "," +
                            logs3saw.ToString(CultureInfo.InvariantCulture) + "," +
                            logs4saw.ToString(CultureInfo.InvariantCulture) + "," +
                            cubic2saw.ToString(Constant.Default.CubicVolumeFormat, CultureInfo.InvariantCulture) + "," +
                            cubic3saw.ToString(Constant.Default.CubicVolumeFormat, CultureInfo.InvariantCulture) + "," +
                            cubic4saw.ToString(Constant.Default.CubicVolumeFormat, CultureInfo.InvariantCulture) + "," +
                            cubicTotal.ToString(CultureInfo.InvariantCulture) + "," +
                            scribner2saw.ToString(CultureInfo.InvariantCulture) + "," +
                            scribner3saw.ToString(CultureInfo.InvariantCulture) + "," +
                            scribner4saw.ToString(CultureInfo.InvariantCulture) + "," +
                            scribnerTotal.ToString(CultureInfo.InvariantCulture) + "," +
                            fellingDiameterAt15cm.ToString(Constant.Default.DiameterInCmFormat, CultureInfo.InvariantCulture) + "," +
                            fellingDiameterAt30cm.ToString(Constant.Default.DiameterInCmFormat, CultureInfo.InvariantCulture) + "," +
                            maxFeedRollerDiameterInCm.ToString(Constant.Default.DiameterInCmFormat, CultureInfo.InvariantCulture) + "," +
                            neiloidHeightInM.ToString(Constant.Default.HeightInMFormat, CultureInfo.InvariantCulture) + "," +
                            unscaledNeiloidVolumeInM3.ToString(Constant.Default.LogVolumeFormat, CultureInfo.InvariantCulture);
                        writer.WriteLine(line);
                        estimatedBytesSinceLastFileLength += line.Length + Environment.NewLine.Length;
                    }
                }

                if (estimatedBytesSinceLastFileLength > WriteCmdlet.StreamLengthSynchronizationInterval)
                {
                    // see remarks on WriteCmdlet.StreamLengthSynchronizationInterval
                    knownFileSizeInBytes = writer.BaseStream.Length;
                    estimatedBytesSinceLastFileLength = 0;
                }
                if (knownFileSizeInBytes + estimatedBytesSinceLastFileLength > maxFileSizeInBytes)
                {
                    this.WriteWarning("Write-VolumeTable: File size limit of " + this.LimitGB.ToString(Constant.Default.FileSizeLimitFormat) + " GB exceeded.");
                    break;
                }
            }
        }
    }
}