using Mars.Seem.Extensions;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "MerchantableLogs")]
    public class WriteMerchantableLogs : WriteSilviculturalTrajectories
    {
        [Parameter(HelpMessage = "Write logs only for trees which are eligible for harvest based on the stand's thinning prescription rather than for all trees at all model timesteps.")]
        public SwitchParameter Harvestable { get; set; }

        [Parameter(HelpMessage = "Write a histogram of log sizes across trees rather than logs for each individual tree. Histogram bins with zero counts are omitted to reduce file size.")]
        public SwitchParameter Histogram { get; set; }
        [Parameter]
        [ValidateRange(0.0F, 1.0F)]
        public float HistogramBinSize { get; set; } // m³

        [Parameter]
        [ValidateRange(0.0F, 20.0F)]
        public float MinimumLogVolume { get; set; } // m³

        [Parameter(HelpMessage = "Suppress repeated logging of a stand trajectory if it occurs across multiple combinations of rotation length and financial scenario. This can reduce file sizes substantially when optimiation prefers one stand trajectory across multiple rotation lengths and financial scenarios.")]
        public SwitchParameter SuppressIdentical { get; set; }

        public WriteMerchantableLogs()
        {
            this.Harvestable = false;
            this.Histogram = false;
            this.HistogramBinSize = 0.01F; // m³
            this.MinimumLogVolume = 0.0F; // log all logs by default
            this.SuppressIdentical = false;
        }

        protected override void ProcessRecord()
        {
            // this.Histogram is a switch
            if (this.HistogramBinSize <= 0.0F)
            {
                throw new ParameterOutOfRangeException(nameof(this.HistogramBinSize));
            }
            // this.MinimumLogVolume checked by PowerShell
            // this.SuppressIdentical is a switch
            this.ValidateParameters();

            // write header
            using StreamWriter writer = this.GetWriter();
            bool resultsSpecified = this.Trajectories != null;
            if (this.ShouldWriteHeader())
            {
                string heuristicAndPositionHeader = this.GetCsvHeaderForSilviculturalCoordinate() + ",standAge";
                if (this.Histogram)
                {
                    writer.WriteLine(heuristicAndPositionHeader + ",cubic,thinCount,regenCount");
                }
                else
                {
                    writer.WriteLine(heuristicAndPositionHeader + ",species,plot,tag,DBH,height,EF,log,gradeThin,topDibThin,cubicThin,gradeRegen,topDibRegen,cubicRegen");
                }
            }

            // write data
            int firstRegenerationHarvestPeriodAcrossAllTrajectories = this.Trajectories!.RotationLengths.Min(); // VS 16.11.7: nullability checking doesn't see [MemberNotNull] on ValidateParameters() call above
            HashSet<StandTrajectory> knownTrajectories = new();
            int maxCoordinateIndex = this.GetMaxCoordinateIndex();
            long estimatedBytesSinceLastFileLength = 0;
            long knownFileSizeInBytes = 0;
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            List<int> regenHistogram = new();
            List<int> thinHistogram = new();       
            for (int coordinateIndex = 0; coordinateIndex < maxCoordinateIndex; ++ coordinateIndex)
            {
                StandTrajectory highTrajectory = this.GetHighTrajectoryAndPositionPrefix(coordinateIndex, out string linePrefix);
                if (this.SuppressIdentical)
                {
                    // for now, identification of unique trajectories relies on reference equality
                    // This amounts to an assumption heuristics don't generate duplicate duplicate prescriptions. This is likely a good
                    // approximation but unlikely to be true in general.
                    if (knownTrajectories.Contains(highTrajectory))
                    {
                        continue; // skip trajectory since it's already been logged
                    }
                    knownTrajectories.Add(highTrajectory);
                }

                int lastThinningPeriod = Constant.RegenerationHarvestIfEligible;
                List<int> thinningPeriods = highTrajectory.Treatments.GetThinningPeriods();
                if (this.Harvestable && (thinningPeriods.Count > 0))
                {
                    lastThinningPeriod = thinningPeriods.Max();
                }

                int firstRegenerationHarvestPeriod = Math.Max(lastThinningPeriod, firstRegenerationHarvestPeriodAcrossAllTrajectories); // see below
                for (int period = 0; period < highTrajectory.PlanningPeriods; ++period)
                {
                    bool isThinningPeriod = thinningPeriods.Contains(period);
                    if (this.Harvestable && (isThinningPeriod == false) && (period < firstRegenerationHarvestPeriod))
                    {
                        // no data to write for unthinned periods before the first regeneration harvest which applies to this trajectory
                        // Checks are made against both the last thinning period for this trajectory and the earliest regeneration harvest
                        // available across all trajectories since this trajectory's last thinning period might be later than the earliest
                        // included regeneration harvest.
                        continue;
                    }

                    Stand? stand;
                    if (this.Harvestable && isThinningPeriod)
                    {
                        // have to get tree sizes from previous period as trees have been removed by end of this period
                        stand = highTrajectory.StandByPeriod[period - 1];
                    }
                    else
                    {
                        // log trees at end of timestep by default
                        stand = highTrajectory.StandByPeriod[period];
                    }
                    if (stand == null)
                    {
                        throw new InvalidOperationException("Stand has not been simulated for period " + period + ".");
                    }
                    string linePrefixForStandAge = linePrefix + "," + highTrajectory.GetEndOfPeriodAge(period).ToString(CultureInfo.InvariantCulture);

                    if (this.Histogram)
                    {
                        // reset histograms if they're in used
                        // If needed, per species histograms can be supported.
                        regenHistogram.Clear();
                        thinHistogram.Clear();
                    }

                    foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
                    {
                        string linePrefixForStandAgeAndSpecies = linePrefixForStandAge + "," + treesOfSpecies.Species.ToFourLetterCode();
                        IndividualTreeSelection treeSelectionForSpecies = highTrajectory.TreeSelectionBySpecies[treesOfSpecies.Species];

                        highTrajectory.TreeVolume.TryGetForwarderVolumeTable(treesOfSpecies.Species, out TreeSpeciesMerchantableVolumeTable? forwarderVolumeTable);
                        highTrajectory.TreeVolume.TryGetLongLogVolumeTable(treesOfSpecies.Species, out TreeSpeciesMerchantableVolumeTable? longLogVolumeTable);
                        if ((forwarderVolumeTable == null) && (longLogVolumeTable == null))
                        {
                            continue; // not a merchantable species, so no logs to buck
                        }
                        if ((forwarderVolumeTable == null) || (longLogVolumeTable == null))
                        {
                            throw new NotSupportedException("Either the forwarder or long log volume table is null but the other is not. This is unexpected for species where long logs are produced and short log species are not currently supported.");
                        }
                        if (forwarderVolumeTable.MaximumLogs < longLogVolumeTable.MaximumLogs)
                        {
                            throw new NotSupportedException(forwarderVolumeTable.MaximumLogs + " logs are buckable from " + treesOfSpecies.Species + " in period " + period + ", which is more than the " + longLogVolumeTable.MaximumLogs + " logs buckable in regeneration harvest.");
                        }

                        for (int compactedTreeIndex = 0; compactedTreeIndex < treesOfSpecies.Count; ++compactedTreeIndex)
                        {
                            if (this.Harvestable)
                            {
                                // if appropriate, skip this tree
                                int uncompactedTreeIndex = treesOfSpecies.UncompactedIndex[compactedTreeIndex];
                                int harvestPeriod = treeSelectionForSpecies[uncompactedTreeIndex];
                                if (isThinningPeriod)
                                {
                                    if (harvestPeriod != period)
                                    {
                                        // tree shouldn't be logged because it is not selected for thinning in this period
                                        continue;
                                    }
                                }
                                else
                                {
                                    if (harvestPeriod != Constant.RegenerationHarvestIfEligible)
                                    {
                                        // tree should be logged in periods eligible for regeneration harvest because it is selected for thinning
                                        continue;
                                    }
                                }
                            }

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
                                                       dbhInCm.ToString(Constant.Default.DiameterInCmFormat, CultureInfo.InvariantCulture) + "," +
                                                       heightInM.ToString(Constant.Default.HeightInMFormat, CultureInfo.InvariantCulture) + "," +
                                                       expansionFactorPerHa.ToString(CultureInfo.InvariantCulture);

                            int regenDiameterClass = longLogVolumeTable.ToDiameterIndex(dbhInCm);
                            int regenHeightClass = longLogVolumeTable.ToHeightIndex(heightInM);
                            (int regenLogs2S, int regenLogs3S) = longLogVolumeTable.GetLogCounts(regenDiameterClass, regenHeightClass);

                            int thinDiameterClass = forwarderVolumeTable.ToDiameterIndex(dbhInCm);
                            int thinHeightClass = forwarderVolumeTable.ToHeightIndex(heightInM);
                            (int thinLogs2S, int thinLogs3S) = forwarderVolumeTable.GetLogCounts(thinDiameterClass, thinHeightClass);

                            for (int logIndex = 0; logIndex < forwarderVolumeTable.MaximumLogs; ++logIndex)
                            {
                                float thinLogVolume = forwarderVolumeTable.LogCubic[thinDiameterClass, thinHeightClass, logIndex];
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
                                float thinTopDiameter = forwarderVolumeTable.LogTopDiameterInCentimeters[regenDiameterClass, regenHeightClass, logIndex];

                                string? regenGrade = null;
                                string? regenLogVolume = null;
                                float regenLogVolumeAsFloat = 0.0F;
                                string? regenTopDiameter = null;
                                if (longLogVolumeTable.MaximumLogs > logIndex)
                                {
                                    regenLogVolumeAsFloat = longLogVolumeTable.LogCubic[regenDiameterClass, regenHeightClass, logIndex];
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

                                        regenLogVolume = regenLogVolumeAsFloat.ToString(Constant.Default.LogVolumeFormat, CultureInfo.InvariantCulture);

                                        float topDiameterInCm = longLogVolumeTable.LogTopDiameterInCentimeters[regenDiameterClass, regenHeightClass, logIndex];
                                        Debug.Assert(topDiameterInCm > 0.0F);
                                        regenTopDiameter = topDiameterInCm.ToString(Constant.Default.DiameterInCmFormat, CultureInfo.InvariantCulture);
                                    }
                                    else if (thinLogVolume < this.MinimumLogVolume)
                                    {
                                        // since thinning log is below threshold there is no regen log, so log shouldn't be written to file
                                        continue;
                                    }
                                }
                                else if (thinLogVolume < this.MinimumLogVolume)
                                {
                                    // since thinning log is below threshold there is no regen log, so log shouldn't be written to file
                                    continue;
                                }

                                if (this.Histogram)
                                {
                                    // accumulate thinning and regen logs into histogram
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
                                    // write log to file
                                    string line = linePrefixForTree + "," +
                                        logIndex.ToString(CultureInfo.InvariantCulture) + "," +
                                        thinGrade + "," +
                                        thinTopDiameter.ToString(Constant.Default.DiameterInCmFormat, CultureInfo.InvariantCulture) + "," +
                                        thinLogVolume.ToString(Constant.Default.LogVolumeFormat, CultureInfo.InvariantCulture) + "," +
                                        regenGrade + "," +
                                        regenTopDiameter + "," +
                                        regenLogVolume;
                                    writer.WriteLine(line);
                                    estimatedBytesSinceLastFileLength += line.Length + Environment.NewLine.Length;
                                }
                            }
                        }
                    }

                    if (this.Histogram)
                    {
                        // write histogram to file
                        // for now, assume histogram bin sizes have granularity no finer than 0.01 m³. More digits can be added to the volume
                        // format if needed.
                        string logVolumeFormat = "0.00"; // Constant.Default.DiameterInCmFormat

                        int maxVolumeClass = Math.Max(regenHistogram.Count, thinHistogram.Count);
                        for (int volumeIndex = 0; volumeIndex < maxVolumeClass; ++volumeIndex)
                        {
                            float volumeClass = this.HistogramBinSize * volumeIndex;
                            int thinCount = thinHistogram.Count > volumeIndex ? thinHistogram[volumeIndex] : 0;
                            int regenCount = regenHistogram.Count > volumeIndex ? regenHistogram[volumeIndex] : 0;
                            if ((thinCount > 0) || (regenCount > 0))
                            {
                                string line = linePrefixForStandAge + "," +
                                    volumeClass.ToString(logVolumeFormat, CultureInfo.InvariantCulture) + "," +
                                    thinCount.ToString(CultureInfo.InvariantCulture) + "," +
                                    regenCount.ToString(CultureInfo.InvariantCulture);
                                writer.WriteLine(line);
                                estimatedBytesSinceLastFileLength += line.Length + Environment.NewLine.Length;
                            }
                        }
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
                    this.WriteWarning("Write-MechantableLogs: File size limit of " + this.LimitGB.ToString(Constant.Default.FileSizeLimitFormat) + " GB exceeded.");
                    break;
                }
            }
        }
    }
}
