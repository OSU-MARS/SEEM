using Osu.Cof.Ferm.Extensions;
using Osu.Cof.Ferm.Tree;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
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
                writer.WriteLine("logLength,species,height,DBH,heightDiameterRatio,logs,logs2S,logs3S,logs4S,cubic2S,cubic3S,cubic4S,cubic,scribner2S,scribner3S,scribner4S,scribner,fellingDiameter15,fellingDiameter30,maxFeedRollerDiameter");
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
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            foreach (FiaCode species in scaledVolume.VolumeBySpecies.Keys)
            {
                if (species != FiaCode.PseudotsugaMenziesii)
                {
                    throw new NotSupportedException("Felling diameter estimation not implemented for " + species + ".");
                }

                string speciesPrefix = String.Concat(logLengthAsString, ",", FiaCodeExtensions.ToFourLetterCode(species));
                TreeSpeciesVolumeTable volumeTable = scaledVolume.VolumeBySpecies[species];
                for (int heightIndex = 0; heightIndex < volumeTable.HeightClasses; ++heightIndex)
                {
                    // for now, use fixed 40% crown ratio in estimating bark thickness with Maguire and Hann 1990 regressions
                    // Height:diameter ratio could potentially be used as a modifier, with higher ratio trees having shorter crowns.
                    float heightInM = volumeTable.GetHeight(heightIndex);
                    float heightToCrownBaseInM = (1.0F - 0.4F) * heightInM;
                    string speciesAndHeightPrefix = speciesPrefix + "," + heightInM.ToString(CultureInfo.InvariantCulture);
                    for (int dbhIndex = 0; dbhIndex < volumeTable.DiameterClasses; ++dbhIndex)
                    {
                        float dbhInCm = volumeTable.GetDiameter(dbhIndex);

                        // diameter and bark regressions behaved incorrectly on trees too small to be merchantable as sawtimber below
                        // breast height
                        // These trees are threfore excluded from checking of felling cut diameter and diameter at minimum feed roller height.
                        // Feed roller diameter estimation is also incorrect for trees with unusually low height-diameter ratios. So these
                        // trees are, for now, also excluded.
                        float fellingDiameter15 = dbhInCm;
                        if ((dbhInCm > 0.0F) && (heightInM > PoudelRegressions.MinimumKozakHeightInM))
                        {
                            // Since
                            //   1) Maguire and Hann 1990's lower height limit for predicting diameter outside bark is one foot
                            //   2) The accurate fitting range of Curtis and Arney 1977 doesn't extend to DBHes large enough to be useful
                            //      in sizing processor heads.
                            // estimate felling diameter at 15 cm from Kozak 2004 form with Poudel et al. 2018's diameter inside bark
                            // coefficients and add bark thickness at one foot from Maguire and Hann 1990.
                            // Constant.Bucking.StumpHeight <= PoudelRegressions.MinimumKozakHeightInM is asserted above
                            float candidateFellingDiameter15 = PoudelRegressions.GetDouglasFirDiameterInsideBark(dbhInCm, heightInM, 0.15F) + DouglasFir.GetDoubleBarkThickness(dbhInCm, heightInM, heightToCrownBaseInM, Constant.MetersPerFoot);
                            Debug.Assert((candidateFellingDiameter15 > dbhInCm) || (dbhInCm < Constant.Bucking.MinimumScalingDiameter4Saw) || (heightInM < Constant.Bucking.MinimumLogLength4SawInM));
                            if (candidateFellingDiameter15 > dbhInCm)
                            {
                                fellingDiameter15 = candidateFellingDiameter15;
                            }
                            //fellingDiameter15 = DouglasFir.GetDiameterOutsideBark(dbhInCm, heightInM, heightToCrownBaseInM, 0.5F * Constant.MetersPerFoot);
                            //Debug.Assert(fellingDiameter15 > dbhInCm);
                        }
                        float fellingDiameter30 = dbhInCm;
                        if ((dbhInCm > 0.0F) && (heightInM > Constant.DbhHeightInM))
                        {
                            // Constant.Bucking.StumpHeight <= PoudelRegressions.MinimumKozakHeightInM is asserted above
                            // float candidateFellingDiameter30 = PoudelRegressions.GetDouglasFirDiameterInsideBark(dbhInCm, heightInM, Constant.MetersPerFoot) + DouglasFir.GetDoubleBarkThickness(dbhInCm, heightInM, heightToCrownBaseInM, Constant.MetersPerFoot);
                            // Debug.Assert((candidateFellingDiameter30 > dbhInCm) || (dbhInCm < Constant.Bucking.MinimumScalingDiameter4Saw) || (heightInM < Constant.Bucking.MinimumLogLength4Saw));
                            //if (candidateFellingDiameter30 > dbhInCm)
                            //{
                            //    fellingDiameter30 = candidateFellingDiameter30;
                            //}
                            fellingDiameter30 = DouglasFir.GetDiameterOutsideBark(dbhInCm, heightInM, heightToCrownBaseInM, Constant.MetersPerFoot);
                            // don't check fellingDiameter15 > fellingDiameter30 because regressions aren't necessarily consistent
                            Debug.Assert(fellingDiameter30 > dbhInCm);
                        }
                        float maxFeedRollerDiameter = dbhInCm;
                        if ((dbhInCm > 0.0F) && (heightInM > Constant.DbhHeightInM))
                        {
                            // minimumFeedRollerHeight <= PoudelRegressions.MinimumKozakHeightInM is asserted above
                            // maxFeedRollerDiameter = PoudelRegressions.GetDouglasFirDiameterInsideBark(dbhInCm, heightInM, minimumFeedRollerHeight) + DouglasFir.GetDoubleBarkThickness(dbhInCm, heightInM, heightToCrownBaseInM, minimumFeedRollerHeight);
                            // Debug.Assert((maxFeedRollerDiameter > dbhInCm) && ((maxFeedRollerDiameter < fellingDiameter30) || (dbhInCm < Constant.Bucking.MinimumScalingDiameter4Saw) || (heightInM < Constant.Bucking.MinimumLogLength4Saw) || (heightDiameterRatio < 25.0F)));
                            // minimumFeedRollerHeight <= Constant.DbhHeightInM is asserted above
                            maxFeedRollerDiameter = DouglasFir.GetDiameterOutsideBark(dbhInCm, heightInM, heightToCrownBaseInM, minimumFeedRollerHeight);
                            Debug.Assert((fellingDiameter30 >= maxFeedRollerDiameter) && (maxFeedRollerDiameter > dbhInCm));
                        }

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
                        writer.WriteLine(speciesAndHeightPrefix + "," + 
                                         dbhInCm.ToString(CultureInfo.InvariantCulture) + "," +
                                         heightDiameterRatio.ToString(CultureInfo.InvariantCulture) + "," +
                                         logs.ToString(CultureInfo.InvariantCulture) + "," +
                                         logs2saw.ToString(CultureInfo.InvariantCulture) + "," +
                                         logs3saw.ToString(CultureInfo.InvariantCulture) + "," +
                                         logs4saw.ToString(CultureInfo.InvariantCulture) + "," +
                                         cubic2saw.ToString(CultureInfo.InvariantCulture) + "," + 
                                         cubic3saw.ToString(CultureInfo.InvariantCulture) + "," + 
                                         cubic4saw.ToString(CultureInfo.InvariantCulture) + "," +
                                         cubicTotal.ToString(CultureInfo.InvariantCulture) + "," +
                                         scribner2saw.ToString(CultureInfo.InvariantCulture) + "," + 
                                         scribner3saw.ToString(CultureInfo.InvariantCulture) + "," + 
                                         scribner4saw.ToString(CultureInfo.InvariantCulture) + "," +
                                         scribnerTotal.ToString(CultureInfo.InvariantCulture) + "," +
                                         fellingDiameter15.ToString(CultureInfo.InvariantCulture) + "," +
                                         fellingDiameter30.ToString(CultureInfo.InvariantCulture) + "," +
                                         maxFeedRollerDiameter.ToString(CultureInfo.InvariantCulture));
                    }
                }

                if (writer.BaseStream.Length > maxFileSizeInBytes)
                {
                    this.WriteWarning("Write-VolumeTable: File size limit of " + this.LimitGB.ToString("0.00") + " GB exceeded.");
                    break;
                }
            }
        }
    }
}