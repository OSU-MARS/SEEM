using Osu.Cof.Ferm.Extensions;
using Osu.Cof.Ferm.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "MerchantableLogs")]
    public class WriteMerchantableLogs : WriteStandTrajectory
    {
        [Parameter]
        public SwitchParameter Histogram { get; set; }
        [Parameter]
        [ValidateRange(0.0F, 1.0F)]
        public float HistogramBinSize { get; set; } // m³

        [Parameter]
        [ValidateRange(0.0F, 20.0F)]
        public float MinimumLogVolume { get; set; } // m³

        public WriteMerchantableLogs()
        {
            this.Histogram = false;
            this.HistogramBinSize = 0.01F; // m³
            this.MinimumLogVolume = 0.0F; // log all logs by default
        }

        protected override void ProcessRecord()
        {
            // this.Histogram is a switch
            if (this.HistogramBinSize <= 0.0F)
            {
                throw new ParameterOutOfRangeException(nameof(this.HistogramBinSize));
            }
            // this.MinimumLogVolume checked by PowerShell
            this.ValidateParameters();

            using StreamWriter writer = this.GetWriter();

            // header
            bool resultsSpecified = this.Trajectories != null;
            if (this.ShouldWriteHeader())
            {
                string heuristicAndPositionHeader = WriteTrajectoriesCmdlet.GetHeuristicAndPositionCsvHeader(this.Trajectories!) + ",standAge";
                if (this.Histogram)
                {
                    writer.WriteLine(heuristicAndPositionHeader + ",cubic,thinCount,regenCount");
                }
                else
                {
                    writer.WriteLine(heuristicAndPositionHeader + ",species,plot,tag,DBH,height,EF,log,gradeThin,topDibThin,cubicThin,gradeRegen,topDibRegen,cubicRegen");
                }
            }

            // data
            int maxCoordinateIndex = this.GetMaxCoordinateIndex();
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            List<int> regenHistogram = new();
            List<int> thinHistogram = new();
            for (int coordinateIndex = 0; coordinateIndex < maxCoordinateIndex; ++coordinateIndex)
            {
                StandTrajectory highTrajectory = this.GetHighTrajectoryAndPositionPrefix(coordinateIndex, out string linePrefix);

                for (int periodIndex = 0; periodIndex < highTrajectory.PlanningPeriods; ++periodIndex)
                {
                    Stand stand = highTrajectory.StandByPeriod[periodIndex] ?? throw new InvalidOperationException("Stand has not been simulated for period " + periodIndex + ".");
                    string linePrefixForStandAge = linePrefix + "," + highTrajectory.GetEndOfPeriodAge(periodIndex).ToString(CultureInfo.InvariantCulture);

                    regenHistogram.Clear();
                    thinHistogram.Clear();

                    foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
                    {
                        string linePrefixForStandAgeAndSpecies = linePrefixForStandAge + "," + treesOfSpecies.Species.ToFourLetterCode();

                        TreeSpeciesVolumeTable regenVolume = highTrajectory.TreeVolume.RegenerationHarvest.VolumeBySpecies[treesOfSpecies.Species];
                        TreeSpeciesVolumeTable thinningVolume = highTrajectory.TreeVolume.Thinning.VolumeBySpecies[treesOfSpecies.Species];
                        if (thinningVolume.MaximumLogs < regenVolume.MaximumLogs)
                        {
                            throw new NotSupportedException();
                        }

                        for (int compactedTreeIndex = 0; compactedTreeIndex < treesOfSpecies.Count; ++compactedTreeIndex)
                        {
                            float dbhInCm = treesOfSpecies.Dbh[compactedTreeIndex];
                            float heightInM = treesOfSpecies.Height[compactedTreeIndex];
                            float expansionFactorPerHa = treesOfSpecies.LiveExpansionFactor[compactedTreeIndex];
                            if (treesOfSpecies.Units == Units.English)
                            {
                                dbhInCm *= Constant.CentimetersPerInch;
                                heightInM *= Constant.MetersPerFoot;
                                expansionFactorPerHa *= Constant.AcresPerHectare;
                            }

                            string linePrefixForTree = linePrefixForStandAgeAndSpecies + "," +
                                                       treesOfSpecies.Plot[compactedTreeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                                       treesOfSpecies.Tag[compactedTreeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                                       dbhInCm.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                                       heightInM.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                                       expansionFactorPerHa.ToString(CultureInfo.InvariantCulture);

                            int regenDiameterClass = regenVolume.ToDiameterIndex(dbhInCm);
                            int regenHeightClass = regenVolume.ToHeightIndex(heightInM);
                            int regenLogs2S = regenVolume.Logs2Saw[regenDiameterClass, regenHeightClass];
                            int regenLogs3S = regenVolume.Logs3Saw[regenDiameterClass, regenHeightClass];

                            int thinDiameterClass = thinningVolume.ToDiameterIndex(dbhInCm);
                            int thinHeightClass = thinningVolume.ToHeightIndex(heightInM);
                            int thinLogs2S = thinningVolume.Logs2Saw[thinDiameterClass, thinHeightClass];
                            int thinLogs3S = thinningVolume.Logs3Saw[thinDiameterClass, thinHeightClass];

                            for (int logIndex = 0; logIndex < thinningVolume.MaximumLogs; ++logIndex)
                            {
                                float thinLogVolume = thinningVolume.LogCubic[thinDiameterClass, thinHeightClass, logIndex];
                                if (thinLogVolume <= 0.0F)
                                {
                                    continue;
                                }

                                string thinGrade = "4S";
                                if (logIndex < thinLogs2S + thinLogs3S)
                                {
                                    thinGrade = "3S";
                                    if (logIndex < thinLogs2S)
                                    {
                                        thinGrade = "2S";
                                    }
                                }
                                float thinTopDiameter = thinningVolume.LogTopDiameter[regenDiameterClass, regenHeightClass, logIndex];

                                string? regenGrade = null;
                                string? regenLogVolume = null;
                                float regenLogVolumeAsFloat = 0.0F;
                                string? regenTopDiameter = null;
                                if (regenVolume.MaximumLogs > logIndex)
                                {
                                    regenLogVolumeAsFloat = regenVolume.LogCubic[regenDiameterClass, regenHeightClass, logIndex];
                                    if (regenLogVolumeAsFloat > 0.0F)
                                    {
                                        if ((thinLogVolume < this.MinimumLogVolume) && (regenLogVolumeAsFloat < this.MinimumLogVolume))
                                        {
                                            // log is below both thinning and regen threshold so shouldn't be written to file
                                            continue;
                                        }

                                        regenGrade = "4S";
                                        if (logIndex < regenLogs2S + regenLogs3S)
                                        {
                                            regenGrade = "3S";
                                            if (logIndex < regenLogs2S)
                                            {
                                                regenGrade = "2S";
                                            }
                                        }

                                        regenLogVolume = regenLogVolumeAsFloat.ToString("0.000", CultureInfo.InvariantCulture);

                                        float topDiameter = regenVolume.LogTopDiameter[regenDiameterClass, regenHeightClass, logIndex];
                                        Debug.Assert(topDiameter > 0.0F);
                                        regenTopDiameter = topDiameter.ToString("0.0", CultureInfo.InvariantCulture);
                                    }
                                    else if (thinLogVolume < this.MinimumLogVolume)
                                    {
                                        // thinning log is below threshold there is no regen log, so log shouldn't be written to file
                                        continue;
                                    }
                                }
                                else if (thinLogVolume < this.MinimumLogVolume)
                                {
                                    // thinning log is below threshold there is no regen log, so log shouldn't be written to file
                                    continue;
                                }

                                if (this.Histogram)
                                {
                                    int thinIndex = (int)(thinLogVolume / this.HistogramBinSize + 0.5F);
                                    if (thinHistogram.Count <= thinIndex)
                                    {
                                        thinHistogram.Extend(thinIndex);
                                    }
                                    ++thinHistogram[thinIndex];

                                    if (regenLogVolumeAsFloat > 0.0F)
                                    {
                                        int regenIndex = (int)(regenLogVolumeAsFloat / this.HistogramBinSize + 0.5F);
                                        if (regenHistogram.Count <= regenIndex)
                                        {
                                            regenHistogram.Extend(regenIndex);
                                        }
                                        ++regenHistogram[regenIndex];
                                    }
                                }
                                else
                                {
                                    writer.WriteLine(linePrefixForTree + "," +
                                                     logIndex.ToString(CultureInfo.InvariantCulture) + "," +
                                                     thinGrade + "," +
                                                     thinTopDiameter.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                                     thinLogVolume.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                                     regenGrade + "," +
                                                     regenTopDiameter + "," +
                                                     regenLogVolume);
                                }
                            }
                        }
                    }

                    if (this.Histogram)
                    {
                        // for now, assume histogram bin sizes have granularity no finer than 0.01 m³
                        // More digits can be added to the volume format if needed.
                        string logVolumeFormat = "0.00";

                        int maxVolumeClass = Math.Max(regenHistogram.Count, thinHistogram.Count);
                        for (int volumeIndex = 0; volumeIndex < maxVolumeClass; ++volumeIndex)
                        {
                            float volumeClass = this.HistogramBinSize * volumeIndex;
                            int thinCount = thinHistogram.Count > volumeIndex ? thinHistogram[volumeIndex] : 0;
                            int regenCount = regenHistogram.Count > volumeIndex ? regenHistogram[volumeIndex] : 0;
                            if ((thinCount > 0) || (regenCount > 0))
                            {
                                writer.WriteLine(linePrefixForStandAge + "," +
                                                 volumeClass.ToString(logVolumeFormat, CultureInfo.InvariantCulture) + "," +
                                                 thinCount.ToString(CultureInfo.InvariantCulture) + "," +
                                                 regenCount.ToString(CultureInfo.InvariantCulture));
                            }
                        }
                    }
                }
                if (writer.BaseStream.Length > maxFileSizeInBytes)
                {
                    this.WriteWarning("Write-MechantableLogs: File size limit of " + this.LimitGB.ToString("0.00") + " GB exceeded.");
                    break;
                }
            }
        }
    }
}
