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
    public class WriteHarvestSchedule : WriteSilviculturalTrajectoriesCmdlet
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

            // write header
            using StreamWriter writer = this.CreateCsvWriter();
            if (this.ShouldWriteCsvHeader())
            {
                writer.WriteLine(this.GetCsvHeaderForSilviculturalCoordinate() + ",plot,tag,lowSelection,highSelection,highThin1dbh,highThin1height,highThin1cr,highThin1ef,highThin1cubic,highThin2dbh,highThin2height,highThin2cr,highThin2ef,highThin2cubic,highThin3dbh,highThin3height,highThin3cr,highThin3ef,highThin3cubic,highRegenDbh,highRegenHeight,highRegenCR,highRegenEF,highRegenCubic");
            }

            // write data
            long estimatedBytesSinceLastFileLength = 0;
            long knownFileSizeInBytes = 0;
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            for (int trajectoryIndex = 0; trajectoryIndex < this.Trajectories.Count; ++trajectoryIndex)
            {
                SilviculturalSpace silviculturalSpace = this.Trajectories[trajectoryIndex];

                Dictionary<int, HashSet<StandTrajectory>> knownTrajectoriesByEndOfRotationPeriod = new();
                int maxCoordinateIndex = WriteSilviculturalTrajectoriesCmdlet.GetMaxCoordinateIndex(silviculturalSpace);
                for (int coordinateIndex = 0; coordinateIndex < maxCoordinateIndex; ++coordinateIndex)
                {
                    StandTrajectory highTrajectory = this.GetHighTrajectoryAndPositionPrefix(silviculturalSpace, coordinateIndex, out string linePrefix, out int endOfRotationPeriodIndex, out int financialIndex);
                    if (this.SuppressIdentical)
                    {
                        // skip writing trajectory if it's already been written for this rotation length
                        // Tree DBH, height, expansion factor, and volume vary with timing of regeneration harvest so stand trajectories spanning
                        // multiple rotation lengths must be written once for each rotation length.
                        if (knownTrajectoriesByEndOfRotationPeriod.TryGetValue(endOfRotationPeriodIndex, out HashSet<StandTrajectory>? knownTrajectoriesForRotation) == false)
                        {
                            knownTrajectoriesForRotation = new();
                            knownTrajectoriesByEndOfRotationPeriod.Add(endOfRotationPeriodIndex, knownTrajectoriesForRotation);
                        }
                        if (knownTrajectoriesForRotation.Contains(highTrajectory)) // reference equality for now
                        {
                            continue; // skip trajectory since it's already been logged
                        }
                        knownTrajectoriesForRotation.Add(highTrajectory);
                    }

                    SilviculturalCoordinate coordinate = silviculturalSpace.CoordinatesEvaluated[coordinateIndex];
                    Stand? highStandBeforeFirstThin = null;
                    int firstThinPeriod = silviculturalSpace.FirstThinPeriods[coordinate.FirstThinPeriodIndex];
                    if (firstThinPeriod != Constant.NoHarvestPeriod)
                    {
                        highStandBeforeFirstThin = highTrajectory.StandByPeriod[firstThinPeriod - 1];
                        if (highStandBeforeFirstThin == null)
                        {
                            throw new ParameterOutOfRangeException(nameof(this.Trajectories), "High trajectory in run " + trajectoryIndex + " has not been fully simulated as stand is null at first thinning. Did the heuristic perform at least one move?");
                        }
                    }

                    Stand? highStandBeforeSecondThin = null;
                    int secondThinPeriod = silviculturalSpace.SecondThinPeriods[coordinate.SecondThinPeriodIndex];
                    if (secondThinPeriod != Constant.NoHarvestPeriod)
                    {
                        highStandBeforeSecondThin = highTrajectory.StandByPeriod[secondThinPeriod - 1];
                        if (highStandBeforeSecondThin == null)
                        {
                            throw new ParameterOutOfRangeException(nameof(this.Trajectories), "High trajectory in run " + trajectoryIndex + " has not been fully simulated as stand is null at second thinning. Did the heuristic perform at least one move?");
                        }
                    }

                    Stand? highStandBeforeThirdThin = null;
                    int thirdThinPeriod = silviculturalSpace.ThirdThinPeriods[coordinate.ThirdThinPeriodIndex];
                    if (thirdThinPeriod != Constant.NoHarvestPeriod)
                    {
                        highStandBeforeThirdThin = highTrajectory.StandByPeriod[thirdThinPeriod - 1];
                        if (highStandBeforeThirdThin == null)
                        {
                            throw new ParameterOutOfRangeException(nameof(this.Trajectories), "High trajectory in run " + trajectoryIndex + " has not been fully simulated as stand is null at third thinning. Did the heuristic perform at least one move?");
                        }
                    }

                    Stand? highStandAtEnd = highTrajectory.StandByPeriod[endOfRotationPeriodIndex];
                    if (highStandAtEnd == null)
                    {
                        throw new ParameterOutOfRangeException(nameof(this.Trajectories), "High trajectory in run " + trajectoryIndex + " has not been fully simulated as stand is null at end of rotation. Did the heuristic perform at least one move?");
                    }

                    Units regenUnits = highStandAtEnd.GetUnits();
                    if (((highStandBeforeFirstThin != null) && (highStandBeforeFirstThin.GetUnits() != regenUnits)) ||
                        ((highStandBeforeSecondThin != null) && (highStandBeforeSecondThin.GetUnits() != regenUnits)) ||
                        ((highStandBeforeThirdThin != null) && (highStandBeforeThirdThin.GetUnits() != regenUnits)))
                    {
                        throw new NotSupportedException("Units differ between simulation periods.");
                    }
                    WriteHarvestSchedule.GetMetricConversions(regenUnits, out float areaConversionFactor, out float dbhConversionFactor, out float heightConversionFactor);

                    StandTrajectory lowTrajectory = silviculturalSpace[coordinate].Pool.Low.Trajectory ?? throw new InvalidOperationException("Low trajectory is null.");

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

                        highTrajectory.TreeScaling.TryGetForwarderVolumeTable(highTreeSelectionForSpecies.Key, out TreeSpeciesMerchantableVolumeTable? forwarderVolumeTable);
                        highTrajectory.TreeScaling.TryGetLongLogVolumeTable(highTreeSelectionForSpecies.Key, out TreeSpeciesMerchantableVolumeTable? longLogVolumeTable);

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
                                highFirstThinDbhInCm = firstThinDbhInCm.ToString(Constant.Default.DiameterInCmFormat, CultureInfo.InvariantCulture);
                                highFirstThinHeightInM = firstThinHeightInM.ToString(Constant.Default.HeightInMFormat, CultureInfo.InvariantCulture);
                                highFirstThinCrownRatio = highTreesBeforeFirstThin.CrownRatio[uncompactedTreeIndex].ToString(Constant.Default.CrownRatioFormat, CultureInfo.InvariantCulture);
                                highFirstThinExpansionFactorPerHa = (areaConversionFactor * highTreesBeforeFirstThin.LiveExpansionFactor[uncompactedTreeIndex]).ToString(Constant.Default.ExpansionFactorFormat, CultureInfo.InvariantCulture);
                                highFirstThinCubicM3 = forwarderVolumeTable?.GetCubicVolumeOfMerchantableWood(firstThinDbhInCm, firstThinHeightInM).ToString(Constant.Default.CubicVolumeFormat, CultureInfo.InvariantCulture);
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
                                highSecondThinDbhInCm = secondThinDbhInCm.ToString(Constant.Default.DiameterInCmFormat, CultureInfo.InvariantCulture);
                                highSecondThinHeightInM = secondThinHeightInM.ToString(Constant.Default.HeightInMFormat, CultureInfo.InvariantCulture);
                                highSecondThinCrownRatio = highTreesBeforeSecondThin.CrownRatio[secondThinCompactedTreeIndex].ToString(Constant.Default.CrownRatioFormat, CultureInfo.InvariantCulture);
                                highSecondThinExpansionFactorPerHa = (areaConversionFactor * highTreesBeforeSecondThin.LiveExpansionFactor[secondThinCompactedTreeIndex]).ToString(Constant.Default.ExpansionFactorFormat, CultureInfo.InvariantCulture);
                                highSecondThinCubicM3 = forwarderVolumeTable?.GetCubicVolumeOfMerchantableWood(secondThinDbhInCm, secondThinHeightInM).ToString(Constant.Default.CubicVolumeFormat, CultureInfo.InvariantCulture);
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
                                highThirdThinDbhInCm = thirdThinDbhInCm.ToString(Constant.Default.DiameterInCmFormat, CultureInfo.InvariantCulture);
                                highThirdThinHeightInM = thirdThinHeightInM.ToString(Constant.Default.HeightInMFormat, CultureInfo.InvariantCulture);
                                highThirdThinCrownRatio = highTreesBeforeThirdThin.CrownRatio[thirdThinCompactedTreeIndex].ToString(Constant.Default.CrownRatioFormat, CultureInfo.InvariantCulture);
                                highThirdThinExpansionFactorPerHa = (areaConversionFactor * highTreesBeforeThirdThin.LiveExpansionFactor[thirdThinCompactedTreeIndex]).ToString(Constant.Default.ExpansionFactorFormat, CultureInfo.InvariantCulture);
                                highThirdThinCubicM3 = forwarderVolumeTable?.GetCubicVolumeOfMerchantableWood(thirdThinDbhInCm, thirdThinHeightInM).ToString(Constant.Default.CubicVolumeFormat, CultureInfo.InvariantCulture);
                                ++thirdThinCompactedTreeIndex; // only need to increment on retained trees, OK to increment here as not referenced below
                            }

                            string? highRegenDbhInCm = null;
                            string? highRegenHeightInM = null;
                            string? highRegenCrownRatio = null;
                            string? highRegenExpansionFactorPerHa = null;
                            string? highRegenCubicM3 = null;
                            bool isTreeThinned = highTreeSelection[uncompactedTreeIndex] != Constant.RegenerationHarvestIfEligible;
                            if (isTreeThinned == false)
                            {
                                Debug.Assert((highTreesBeforeFirstThin == null) || (highTreesAtRegen.Tag[regenCompactedTreeIndex] == highTreesBeforeFirstThin.Tag[uncompactedTreeIndex]));

                                float regenDbhInCm = dbhConversionFactor * highTreesAtRegen.Dbh[uncompactedTreeIndex];
                                float regenHeightInM = heightConversionFactor * highTreesAtRegen.Height[uncompactedTreeIndex];
                                highRegenDbhInCm = regenDbhInCm.ToString(Constant.Default.DiameterInCmFormat, CultureInfo.InvariantCulture);
                                highRegenHeightInM = regenHeightInM.ToString(Constant.Default.HeightInMFormat, CultureInfo.InvariantCulture);
                                highRegenCrownRatio = highTreesAtRegen.CrownRatio[regenCompactedTreeIndex].ToString(Constant.Default.CrownRatioFormat, CultureInfo.InvariantCulture);
                                highRegenExpansionFactorPerHa = (areaConversionFactor * highTreesAtRegen.LiveExpansionFactor[regenCompactedTreeIndex]).ToString(Constant.Default.ExpansionFactorFormat, CultureInfo.InvariantCulture);
                                highRegenCubicM3 = longLogVolumeTable?.GetCubicVolumeOfMerchantableWood(regenDbhInCm, regenHeightInM).ToString(Constant.Default.CubicVolumeFormat, CultureInfo.InvariantCulture);
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

                            string line = linePrefix + "," +
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
                                highRegenCubicM3;
                            writer.WriteLine(line);
                            estimatedBytesSinceLastFileLength += line.Length + Environment.NewLine.Length;
                        }

                        previousSpeciesCount += highTreeSelection.Count;
                    }

                    if (estimatedBytesSinceLastFileLength > WriteCmdlet.StreamLengthSynchronizationInterval)
                    {
                        // see remarks on WriteCmdlet.StreamLengthSynchronizationInterval
                        knownFileSizeInBytes = writer.BaseStream.Length;
                        estimatedBytesSinceLastFileLength = 0;
                    }
                    if (knownFileSizeInBytes + estimatedBytesSinceLastFileLength > maxFileSizeInBytes)
                    {
                        this.WriteWarning("Write-HarvestSchedule: File size limit of " + this.LimitGB.ToString(Constant.Default.FileSizeLimitFormat) + " GB exceeded.");
                        break;
                    }
                }
            }
        }
    }
}
