using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "HarvestSchedule")]
    public class WriteHarvestSchedule : WriteTrajectoriesCmdlet
    {
        [Parameter(HelpMessage = "Suppress repeated logging of a stand trajectory if it occurs across multiple financial scenarios. This can reduce file sizes substantially when optimiation prefers one stand trajectory across multiple financial scenarios.")]
        public SwitchParameter SuppressIdentical { get; set; }

        public WriteHarvestSchedule()
        {
            this.SuppressIdentical = false;
        }

        protected override void ProcessRecord()
        {
            this.ValidateParameters();
            Debug.Assert(this.Trajectories != null);

            // write header
            using StreamWriter writer = this.GetWriter();
            if (this.ShouldWriteHeader())
            {
                writer.WriteLine(this.GetCsvHeaderForCoordinate() + ",plot,tag,lowSelection,highSelection,highThin1dbh,highThin1height,highThin1cr,highThin1ef,highThin1cubic,highThin2dbh,highThin2height,highThin2cr,highThin2ef,highThin2cubic,highThin3dbh,highThin3height,highThin3cr,highThin3ef,highThin3cubic,highRegenDbh,highRegenHeight,highRegenCR,highRegenEF,highRegenCubic");
            }

            // write data
            Dictionary<int, HashSet<StandTrajectory>> knownTrajectoriesByEndOfRotationPeriod = new();
            int maxCoordinateIndex = this.GetMaxCoordinateIndex();
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            for (int coordinateIndex = 0; coordinateIndex < maxCoordinateIndex; ++coordinateIndex)
            {
                StandTrajectory highTrajectory = this.GetHighTrajectoryAndPositionPrefix(coordinateIndex, out string linePrefix, out int endOfRotationPeriod, out int financialIndex);
                if (this.SuppressIdentical)
                {
                    // skip writing trajectory if it's already been written for this rotation length
                    // Tree DBH, height, expansion factor, and volume vary with timing of regeneration harvest so stand trajectories spanning
                    // multiple rotation lengths must be written once for each rotation length.
                    if (knownTrajectoriesByEndOfRotationPeriod.TryGetValue(endOfRotationPeriod, out HashSet<StandTrajectory>? knownTrajectoriesForRotation) == false)
                    {
                        knownTrajectoriesForRotation = new();
                        knownTrajectoriesByEndOfRotationPeriod.Add(endOfRotationPeriod, knownTrajectoriesForRotation);
                    }
                    if (knownTrajectoriesForRotation.Contains(highTrajectory)) // reference equality for now
                    {
                        continue; // skip trajectory since it's already been logged
                    }
                    knownTrajectoriesForRotation.Add(highTrajectory);
                }

                StandTrajectoryCoordinate coordinate = this.Trajectories.CoordinatesEvaluated[coordinateIndex];
                Stand? highStandBeforeFirstThin = null;
                int firstThinPeriod = this.Trajectories.FirstThinPeriods[coordinate.FirstThinPeriodIndex];
                if (firstThinPeriod != Constant.NoThinPeriod)
                {
                    highStandBeforeFirstThin = highTrajectory.StandByPeriod[firstThinPeriod - 1];
                    if (highStandBeforeFirstThin == null)
                    {
                        throw new ParameterOutOfRangeException(nameof(this.Trajectories), "High trajectory in run has not been fully simulated as stand is null at first thinning. Did the heuristic perform at least one move?");
                    }
                }

                Stand? highStandBeforeSecondThin = null;
                int secondThinPeriod = this.Trajectories.SecondThinPeriods[coordinate.SecondThinPeriodIndex];
                if (secondThinPeriod != Constant.NoThinPeriod)
                {
                    highStandBeforeSecondThin = highTrajectory.StandByPeriod[secondThinPeriod - 1];
                    if (highStandBeforeSecondThin == null)
                    {
                        throw new ParameterOutOfRangeException(nameof(this.Trajectories), "High trajectory in run has not been fully simulated as stand is null at second thinning. Did the heuristic perform at least one move?");
                    }
                }

                Stand? highStandBeforeThirdThin = null;
                int thirdThinPeriod = this.Trajectories.ThirdThinPeriods[coordinate.ThirdThinPeriodIndex];
                if (thirdThinPeriod != Constant.NoThinPeriod)
                {
                    highStandBeforeThirdThin = highTrajectory.StandByPeriod[thirdThinPeriod - 1];
                    if (highStandBeforeThirdThin == null)
                    {
                        throw new ParameterOutOfRangeException(nameof(this.Trajectories), "High trajectory in run has not been fully simulated as stand is null at third thinning. Did the heuristic perform at least one move?");
                    }
                }

                Stand? highStandAtEnd = highTrajectory.StandByPeriod[endOfRotationPeriod];
                if (highStandAtEnd == null)
                {
                    throw new ParameterOutOfRangeException(nameof(this.Trajectories), "High trajectory in run has not been fully simulated as stand is null at end of rotation. Did the heuristic perform at least one move?");
                }

                Units regenUnits = highStandAtEnd.GetUnits();
                if (((highStandBeforeFirstThin != null) && (highStandBeforeFirstThin.GetUnits() != regenUnits)) || 
                    ((highStandBeforeSecondThin != null) && (highStandBeforeSecondThin.GetUnits() != regenUnits)) ||
                    ((highStandBeforeThirdThin != null) && (highStandBeforeThirdThin.GetUnits() != regenUnits)))
                {
                    throw new NotSupportedException("Units differ between simulation periods.");
                }
                WriteHarvestSchedule.GetMetricConversions(regenUnits, out float areaConversionFactor, out float dbhConversionFactor, out float heightConversionFactor);

                StandTrajectory lowTrajectory = this.Trajectories[coordinate].Pool.Low.Trajectory ?? throw new InvalidOperationException("Low trajectory is null.");

                int previousSpeciesCount = 0;
                foreach (KeyValuePair<FiaCode, IndividualTreeSelection> highTreeSelectionForSpecies in highTrajectory.TreeSelectionBySpecies)
                {
                    Trees? highTreesBeforeFirstThin = highStandBeforeFirstThin?.TreesBySpecies[highTreeSelectionForSpecies.Key];
                    Trees? highTreesBeforeSecondThin = highStandBeforeSecondThin?.TreesBySpecies[highTreeSelectionForSpecies.Key];
                    Trees? highTreesBeforeThirdThin = highStandBeforeThirdThin?.TreesBySpecies[highTreeSelectionForSpecies.Key];
                    Trees highTreesAtRegen = highStandAtEnd.TreesBySpecies[highTreeSelectionForSpecies.Key];

                    IndividualTreeSelection highTreeSelection = highTreeSelectionForSpecies.Value;
                    int uncompactedTreeCount = highTreesAtRegen.Count;
                    if (highTreesBeforeFirstThin != null)
                    {
                        Debug.Assert(highTreesBeforeFirstThin.Capacity == highTreeSelection.Capacity);
                        uncompactedTreeCount = highTreesBeforeFirstThin.Count;
                    }

                    TreeSpeciesVolumeTable regenVolumeTable = highTrajectory.TreeVolume.RegenerationHarvest.VolumeBySpecies[highTreeSelectionForSpecies.Key];
                    TreeSpeciesVolumeTable thinVolumeTable = highTrajectory.TreeVolume.Thinning.VolumeBySpecies[highTreeSelectionForSpecies.Key];

                    // uncompactedTreeIndex: tree index in periods before thinned trees are removed
                    // compactedTreeIndex: index of retained trees in periods after thinning
                    IndividualTreeSelection lowTreeSelection = lowTrajectory.TreeSelectionBySpecies[highTreeSelectionForSpecies.Key];
                    int secondThinCompactedTreeIndex = 0;
                    int thirdThinCompactedTreeIndex = 0;
                    int regenCompactedTreeIndex = 0;
                    for (int uncompactedTreeIndex = 0; uncompactedTreeIndex < uncompactedTreeCount; ++uncompactedTreeIndex)
                    {
                        // TODO: replace FiaVolume calls with single-tree version of highTrajectory.Financial.ScaledVolumeThinning.GetHarvestedScribnerVolume();
                        string? highFirstThinDbhInCm = null;
                        string? highFirstThinHeightInM = null;
                        string? highFirstThinCrownRatio = null;
                        string? highFirstThinExpansionFactorPerHa = null;
                        string? highFirstThinCubicM3 = null;
                        // properties before first thin are undefined if no thinning occurred
                        if (highTreesBeforeFirstThin != null)
                        {
                            float firstThinDbhInCm = dbhConversionFactor * highTreesBeforeFirstThin.Dbh[uncompactedTreeIndex];
                            float firstThinHeightInM = heightConversionFactor * highTreesBeforeFirstThin.Height[uncompactedTreeIndex];
                            highFirstThinDbhInCm = firstThinDbhInCm.ToString("0.00", CultureInfo.InvariantCulture);
                            highFirstThinHeightInM = firstThinHeightInM.ToString("0.00", CultureInfo.InvariantCulture);
                            highFirstThinCrownRatio = highTreesBeforeFirstThin.CrownRatio[uncompactedTreeIndex].ToString("0.000", CultureInfo.InvariantCulture);
                            highFirstThinExpansionFactorPerHa = (areaConversionFactor * highTreesBeforeFirstThin.LiveExpansionFactor[uncompactedTreeIndex]).ToString("0.000", CultureInfo.InvariantCulture);
                            highFirstThinCubicM3 = thinVolumeTable.GetCubicVolume(firstThinDbhInCm, firstThinHeightInM).ToString("0.00", CultureInfo.InvariantCulture);
                        }

                        string? highSecondThinDbhInCm = null;
                        string? highSecondThinHeightInM = null;
                        string? highSecondThinCrownRatio = null;
                        string? highSecondThinExpansionFactorPerHa = null;
                        string? highSecondThinCubicM3 = null;
                        // properties before second thin are undefined if no thinning, only a first thin, or if tree was removed in first thin
                        bool isRemovedInFirstThin = highTreeSelection[uncompactedTreeIndex] == firstThinPeriod;
                        if ((highTreesBeforeSecondThin != null) && (isRemovedInFirstThin == false))
                        {
                            Debug.Assert(highTreesBeforeSecondThin.Tag[secondThinCompactedTreeIndex] == highTreesBeforeFirstThin!.Tag[uncompactedTreeIndex]);

                            float secondThinDbhInCm = dbhConversionFactor * highTreesBeforeSecondThin.Dbh[uncompactedTreeIndex];
                            float secondThinHeightInM = heightConversionFactor * highTreesBeforeSecondThin.Height[uncompactedTreeIndex];
                            highSecondThinDbhInCm = secondThinDbhInCm.ToString("0.00", CultureInfo.InvariantCulture);
                            highSecondThinHeightInM = secondThinHeightInM.ToString("0.00", CultureInfo.InvariantCulture);
                            highSecondThinCrownRatio = highTreesBeforeSecondThin.CrownRatio[secondThinCompactedTreeIndex].ToString("0.000", CultureInfo.InvariantCulture);
                            highSecondThinExpansionFactorPerHa = (areaConversionFactor * highTreesBeforeSecondThin.LiveExpansionFactor[secondThinCompactedTreeIndex]).ToString("0.000", CultureInfo.InvariantCulture);
                            highSecondThinCubicM3 = thinVolumeTable.GetCubicVolume(secondThinDbhInCm, secondThinHeightInM).ToString("0.00", CultureInfo.InvariantCulture);
                            ++secondThinCompactedTreeIndex; // only need to increment on retained trees, OK to increment here as not referenced below
                        }

                        string? highThirdThinDbhInCm = null;
                        string? highThirdThinHeightInM = null;
                        string? highThirdThinCrownRatio = null;
                        string? highThirdThinExpansionFactorPerHa = null;
                        string? highThirdThinCubicM3 = null;
                        // properties before Third thin are undefined if no thinning, only a first thin, or if tree was removed in first thin
                        bool isRemovedInFirstOrSecondThin = isRemovedInFirstThin || (highTreeSelection[uncompactedTreeIndex] == secondThinPeriod);
                        if ((highTreesBeforeThirdThin != null) && (isRemovedInFirstOrSecondThin == false))
                        {
                            Debug.Assert(highTreesBeforeThirdThin.Tag[thirdThinCompactedTreeIndex] == highTreesBeforeFirstThin!.Tag[uncompactedTreeIndex]);

                            float thirdThinDbhInCm = dbhConversionFactor * highTreesBeforeThirdThin.Dbh[uncompactedTreeIndex];
                            float thirdThinHeightInM = heightConversionFactor * highTreesBeforeThirdThin.Height[uncompactedTreeIndex];
                            highThirdThinDbhInCm = thirdThinDbhInCm.ToString("0.00", CultureInfo.InvariantCulture);
                            highThirdThinHeightInM = thirdThinHeightInM.ToString("0.00", CultureInfo.InvariantCulture);
                            highThirdThinCrownRatio = highTreesBeforeThirdThin.CrownRatio[thirdThinCompactedTreeIndex].ToString("0.000", CultureInfo.InvariantCulture);
                            highThirdThinExpansionFactorPerHa = (areaConversionFactor * highTreesBeforeThirdThin.LiveExpansionFactor[thirdThinCompactedTreeIndex]).ToString("0.000", CultureInfo.InvariantCulture);
                            highThirdThinCubicM3 = thinVolumeTable.GetCubicVolume(thirdThinDbhInCm, thirdThinHeightInM).ToString("0.00", CultureInfo.InvariantCulture);
                            ++thirdThinCompactedTreeIndex; // only need to increment on retained trees, OK to increment here as not referenced below
                        }

                        string? highRegenDbhInCm = null;
                        string? highRegenHeightInM = null;
                        string? highRegenCrownRatio = null;
                        string? highRegenExpansionFactorPerHa = null;
                        string? highRegenCubicM3 = null;
                        bool isTreeThinned = highTreeSelection[uncompactedTreeIndex] != Constant.RegenerationHarvestPeriod;
                        if (isTreeThinned == false)
                        {
                            Debug.Assert((highTreesBeforeFirstThin == null) || (highTreesAtRegen.Tag[regenCompactedTreeIndex] == highTreesBeforeFirstThin.Tag[uncompactedTreeIndex]));

                            float regenDbhInCm = dbhConversionFactor * highTreesAtRegen.Dbh[uncompactedTreeIndex];
                            float regenHeightInM = heightConversionFactor * highTreesAtRegen.Height[uncompactedTreeIndex];
                            highRegenDbhInCm = regenDbhInCm.ToString("0.00", CultureInfo.InvariantCulture);
                            highRegenHeightInM = regenHeightInM.ToString("0.00", CultureInfo.InvariantCulture);
                            highRegenCrownRatio = highTreesAtRegen.CrownRatio[regenCompactedTreeIndex].ToString("0.000", CultureInfo.InvariantCulture);
                            highRegenExpansionFactorPerHa = (areaConversionFactor * highTreesAtRegen.LiveExpansionFactor[regenCompactedTreeIndex]).ToString("0.000", CultureInfo.InvariantCulture);
                            highRegenCubicM3 = thinVolumeTable.GetCubicVolume(regenDbhInCm, regenHeightInM).ToString("0.00", CultureInfo.InvariantCulture);
                            ++regenCompactedTreeIndex; // only need to increment on retained trees, OK to increment here as not referenced below
                        }

                        int plot;
                        int tag;
                        if (highTreesBeforeFirstThin != null)
                        {
                            plot = highTreesBeforeFirstThin.Plot[uncompactedTreeIndex];
                            tag = highTreesBeforeFirstThin.Tag[uncompactedTreeIndex];
                        }
                        else
                        {
                            plot = highTreesAtRegen.Plot[uncompactedTreeIndex];
                            tag = highTreesAtRegen.Tag[uncompactedTreeIndex];
                        }
                        writer.WriteLine(linePrefix + "," +
                                         plot.ToString(CultureInfo.InvariantCulture) + "," +
                                         tag.ToString(CultureInfo.InvariantCulture) + "," +
                                         lowTreeSelection[uncompactedTreeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                         highTreeSelection[uncompactedTreeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                         highFirstThinDbhInCm + "," +
                                         highFirstThinHeightInM + "," +
                                         highFirstThinCrownRatio + "," +
                                         highFirstThinExpansionFactorPerHa + "," +
                                         highFirstThinCubicM3 + "," +
                                         highSecondThinDbhInCm + "," +
                                         highSecondThinHeightInM + "," +
                                         highSecondThinCrownRatio + "," +
                                         highSecondThinExpansionFactorPerHa + "," +
                                         highSecondThinCubicM3 + "," +
                                         highThirdThinDbhInCm + "," +
                                         highThirdThinHeightInM + "," +
                                         highThirdThinCrownRatio + "," +
                                         highThirdThinExpansionFactorPerHa + "," +
                                         highThirdThinCubicM3 + "," +
                                         highRegenDbhInCm + "," +
                                         highRegenHeightInM + "," +
                                         highRegenCrownRatio + "," +
                                         highRegenExpansionFactorPerHa + "," +
                                         highRegenCubicM3);
                    }

                    previousSpeciesCount += highTreeSelection.Count;
                }

                if (writer.BaseStream.Length > maxFileSizeInBytes)
                {
                    this.WriteWarning("Write-HarvestSchedule: File size limit of " + this.LimitGB.ToString("0.00") + " GB exceeded.");
                    break;
                }

            }
        }
    }
}
