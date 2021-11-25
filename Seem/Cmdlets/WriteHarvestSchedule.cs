using Osu.Cof.Ferm.Silviculture;
using Osu.Cof.Ferm.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "HarvestSchedule")]
    public class WriteHarvestSchedule : WriteTrajectoriesCmdlet
    {
        protected override void ProcessRecord()
        {
            this.ValidateParameters();
            Debug.Assert(this.Trajectories != null);

            using StreamWriter writer = this.GetWriter();
            if (this.ShouldWriteHeader())
            {
                writer.WriteLine(WriteTrajectoriesCmdlet.GetHeuristicAndPositionCsvHeader(this.Trajectories) + ",plot,tag,lowSelection,highSelection,highThin1dbh,highThin1height,highThin1cr,highThin1ef,highThin1cubic,highThin2dbh,highThin2height,highThin2cr,highThin2ef,highThin2cubic,highThin3dbh,highThin3height,highThin3cr,highThin3ef,highThin3cubic,highRegenDbh,highRegenHeight,highRegenCR,highRegenEF,highRegenCubic");
            }

            int maxCoordinateIndex = this.GetMaxCoordinateIndex();
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            for (int coordinateIndex = 0; coordinateIndex < maxCoordinateIndex; ++coordinateIndex)
            {
                StandTrajectory highTrajectory = this.GetHighTrajectoryAndPositionPrefix(coordinateIndex, out string linePrefix, out int endOfRotationPeriod, out int financialIndex);
                int firstThinPeriod = highTrajectory.GetFirstThinPeriod();
                int periodBeforeFirstThin = firstThinPeriod - 1;
                if (periodBeforeFirstThin < 0)
                {
                    periodBeforeFirstThin = highTrajectory.PlanningPeriods - 1;
                }
                int secondThinPeriod = highTrajectory.GetSecondThinPeriod();
                int periodBeforeSecondThin = secondThinPeriod - 1;
                if (periodBeforeSecondThin < 0)
                {
                    periodBeforeSecondThin = highTrajectory.PlanningPeriods - 1;
                }
                int thirdThinPeriod = highTrajectory.GetThirdThinPeriod();
                int periodBeforeThirdThin = thirdThinPeriod - 1;
                if (periodBeforeThirdThin < 0)
                {
                    periodBeforeThirdThin = highTrajectory.PlanningPeriods - 1;
                }

                Stand? highStandBeforeFirstThin = highTrajectory.StandByPeriod[periodBeforeFirstThin];
                Stand? highStandBeforeSecondThin = highTrajectory.StandByPeriod[periodBeforeSecondThin];
                Stand? highStandBeforeThirdThin = highTrajectory.StandByPeriod[periodBeforeThirdThin];
                Stand? highStandAtEnd = highTrajectory.StandByPeriod[^1];
                if ((highStandBeforeFirstThin == null) || (highStandBeforeSecondThin == null) || (highStandBeforeThirdThin == null) || (highStandAtEnd == null))
                {
                    throw new ParameterOutOfRangeException(nameof(this.Trajectories), "High trajectory in run has not been fully simulated. Did the heuristic perform at least one move?");
                }
                Units regenUnits = highStandAtEnd.GetUnits();
                if ((highStandBeforeFirstThin.GetUnits() != regenUnits) || (highStandBeforeSecondThin.GetUnits() != regenUnits))
                {
                    throw new NotSupportedException("Units differ between simulation periods.");
                }

                StandTrajectoryCoordinate coordinate = this.Trajectories.CoordinatesEvaluated[coordinateIndex];
                StandTrajectory lowTrajectory = this.Trajectories[coordinate].Pool.Low.Trajectory ?? throw new InvalidOperationException("Low trajectory is null.");

                int previousSpeciesCount = 0;
                foreach (KeyValuePair<FiaCode, IndividualTreeSelection> highTreeSelectionForSpecies in highTrajectory.TreeSelectionBySpecies)
                {
                    Trees highTreesBeforeFirstThin = highStandBeforeFirstThin.TreesBySpecies[highTreeSelectionForSpecies.Key];
                    Trees highTreesBeforeSecondThin = highStandBeforeSecondThin.TreesBySpecies[highTreeSelectionForSpecies.Key];
                    Trees highTreesBeforeThirdThin = highStandBeforeThirdThin.TreesBySpecies[highTreeSelectionForSpecies.Key];
                    Trees highTreesAtRegen = highStandAtEnd.TreesBySpecies[highTreeSelectionForSpecies.Key];
                    WriteHarvestSchedule.GetMetricConversions(highTreesBeforeFirstThin.Units, out float areaConversionFactor, out float dbhConversionFactor, out float heightConversionFactor);

                    TreeSpeciesVolumeTable regenVolumeTable = highTrajectory.TreeVolume.RegenerationHarvest.VolumeBySpecies[highTreeSelectionForSpecies.Key];
                    TreeSpeciesVolumeTable thinVolumeTable = highTrajectory.TreeVolume.Thinning.VolumeBySpecies[highTreeSelectionForSpecies.Key];

                    // uncompactedTreeIndex: tree index in periods before thinned trees are removed
                    // compactedTreeIndex: index of retained trees in periods after thinning
                    IndividualTreeSelection lowTreeSelection = lowTrajectory.TreeSelectionBySpecies[highTreeSelectionForSpecies.Key];
                    IndividualTreeSelection highTreeSelection = highTreeSelectionForSpecies.Value;
                    Debug.Assert(highTreesBeforeFirstThin.Capacity == highTreeSelection.Capacity);
                    int secondThinCompactedTreeIndex = 0;
                    int thirdThinCompactedTreeIndex = 0;
                    int regenCompactedTreeIndex = 0;
                    for (int uncompactedTreeIndex = 0; uncompactedTreeIndex < highTreesBeforeFirstThin.Count; ++uncompactedTreeIndex)
                    {
                        // TODO: replace FiaVolume calls with single-tree version of highTrajectory.Financial.ScaledVolumeThinning.GetHarvestedScribnerVolume();
                        string? highFirstThinDbhInCm = null;
                        string? highFirstThinHeightInM = null;
                        string? highFirstThinCrownRatio = null;
                        string? highFirstThinExpansionFactorPerHa = null;
                        string? highFirstThinCubicM3 = null;
                        // properties before first thin are undefined if no thinning occurred
                        if (highTrajectory.GetFirstThinPeriod() != Constant.NoThinPeriod)
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
                        if ((highTrajectory.GetSecondThinPeriod() != Constant.NoThinPeriod) && (isRemovedInFirstThin == false))
                        {
                            Debug.Assert(highTreesBeforeSecondThin.Tag[secondThinCompactedTreeIndex] == highTreesBeforeFirstThin.Tag[uncompactedTreeIndex]);

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
                        if ((highTrajectory.GetThirdThinPeriod() != Constant.NoThinPeriod) && (isRemovedInFirstOrSecondThin == false))
                        {
                            Debug.Assert(highTreesBeforeThirdThin.Tag[thirdThinCompactedTreeIndex] == highTreesBeforeFirstThin.Tag[uncompactedTreeIndex]);

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
                        bool isThinned = highTreeSelection[uncompactedTreeIndex] != Constant.NoHarvestPeriod;
                        if (isThinned == false)
                        {
                            Debug.Assert(highTreesAtRegen.Tag[regenCompactedTreeIndex] == highTreesBeforeFirstThin.Tag[uncompactedTreeIndex]);

                            float regenDbhInCm = dbhConversionFactor * highTreesAtRegen.Dbh[uncompactedTreeIndex];
                            float regenHeightInM = heightConversionFactor * highTreesAtRegen.Height[uncompactedTreeIndex];
                            highRegenDbhInCm = regenDbhInCm.ToString("0.00", CultureInfo.InvariantCulture);
                            highRegenHeightInM = regenHeightInM.ToString("0.00", CultureInfo.InvariantCulture);
                            highRegenCrownRatio = highTreesAtRegen.CrownRatio[regenCompactedTreeIndex].ToString("0.000", CultureInfo.InvariantCulture);
                            highRegenExpansionFactorPerHa = (areaConversionFactor * highTreesAtRegen.LiveExpansionFactor[regenCompactedTreeIndex]).ToString("0.000", CultureInfo.InvariantCulture);
                            highRegenCubicM3 = thinVolumeTable.GetCubicVolume(regenDbhInCm, regenHeightInM).ToString("0.00", CultureInfo.InvariantCulture);
                            ++regenCompactedTreeIndex; // only need to increment on retained trees, OK to increment here as not referenced below
                        }

                        // for now, make best guess of using tree tag or index as unique identifier 
                        int tag = highTreesBeforeFirstThin.Tag[uncompactedTreeIndex] < 0 ? previousSpeciesCount + uncompactedTreeIndex : highTreesBeforeFirstThin.Tag[uncompactedTreeIndex];
                        writer.WriteLine(linePrefix + "," +
                                         highTreesBeforeFirstThin.Plot[uncompactedTreeIndex].ToString(CultureInfo.InvariantCulture) + "," +
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
