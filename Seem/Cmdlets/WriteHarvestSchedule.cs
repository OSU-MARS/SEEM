using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "HarvestSchedule")]
    public class WriteHarvestSchedule : WriteCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public HeuristicResultSet? Results { get; set; }

        protected override void ProcessRecord()
        {
            Debug.Assert(this.Results != null);
            if (this.Results.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.Results));
            }

            using StreamWriter writer = this.GetWriter();
            if (this.ShouldWriteHeader())
            {
                HeuristicParameters? highestHeuristicParameters = this.Results.Distributions[0].HeuristicParameters;
                if (highestHeuristicParameters == null)
                {
                    throw new NotSupportedException("Cannot generate schedule header because first run has no heuristic parameters.");
                }
                writer.WriteLine("stand,heuristic," + highestHeuristicParameters.GetCsvHeader() + "," + WriteCmdlet.RateAndAgeCsvHeader + ",tree,lowest selection,highest selection,highest first thin DBH,highest first thin height,highest first thin CR,highest first thin EF,highest first thin BF,highest second thin DBH,highest second thin height,highest second thin CR,highest second thin EF,highest second thin BF,highest third thin DBH,highest third thin height,highest third thin CR,highest third thin EF,highest third thin BF,highest final DBH,highest final height,highest final CR,highest final EF,highest final BF");
            }

            for (int resultIndex = 0; resultIndex < this.Results!.Count; ++resultIndex)
            {
                HeuristicDistribution distribution = this.Results.Distributions[resultIndex];
                if (distribution.HeuristicParameters == null)
                {
                    throw new NotSupportedException("Result " + resultIndex + " is missing heuristic parameters.");
                }
                HeuristicSolutionPool solution = this.Results.Solutions[resultIndex];
                if ((solution.Highest == null) ||
                    (solution.Highest.BestTrajectory == null) ||
                    (solution.Highest.BestTrajectory.Heuristic == null) ||
                    (solution.Lowest == null))
                {
                    throw new NotSupportedException("Result " + resultIndex + " is missing a highest solution, lowest solution, highest heuristic trajectory, or back link from highest trajectory to is generating heuristic.");
                }

                OrganonStandTrajectory highestTrajectory = solution.Highest.BestTrajectory;
                int firstThinPeriod = highestTrajectory.GetFirstThinPeriod();
                int periodBeforeFirstThin = firstThinPeriod - 1;
                if (periodBeforeFirstThin < 0)
                {
                    periodBeforeFirstThin = highestTrajectory.PlanningPeriods - 1;
                }
                int secondThinPeriod = highestTrajectory.GetSecondThinPeriod();
                int periodBeforeSecondThin = secondThinPeriod - 1;
                if (periodBeforeSecondThin < 0)
                {
                    periodBeforeSecondThin = highestTrajectory.PlanningPeriods - 1;
                }
                int thirdThinPeriod = highestTrajectory.GetThirdThinPeriod();
                int periodBeforeThirdThin = thirdThinPeriod - 1;
                if (periodBeforeThirdThin < 0)
                {
                    periodBeforeThirdThin = highestTrajectory.PlanningPeriods - 1;
                }

                float discountRate = this.Results.DiscountRates[distribution.DiscountRateIndex];
                string linePrefix = highestTrajectory.Name + "," + 
                    highestTrajectory.Heuristic.GetName() + "," + 
                    distribution.HeuristicParameters.GetCsvValues() + "," +
                    WriteCmdlet.GetRateAndAgeCsvValues(highestTrajectory, discountRate);

                Stand? highestStandNbeforeFirstThin = highestTrajectory.StandByPeriod[periodBeforeFirstThin];
                Stand? highestStandNbeforeSecondThin = highestTrajectory.StandByPeriod[periodBeforeSecondThin];
                Stand? highestStandNbeforeThirdThin = highestTrajectory.StandByPeriod[periodBeforeThirdThin];
                Stand? highestStandNatEnd = highestTrajectory.StandByPeriod[^1];
                if ((highestStandNbeforeFirstThin == null) || (highestStandNbeforeSecondThin == null) || (highestStandNbeforeThirdThin == null) || (highestStandNatEnd == null))
                {
                    throw new ParameterOutOfRangeException(nameof(this.Results), "Highest stand in run has not been fully simulated. Did the heuristic perform at least one move?");
                }
                Units finalUnits = highestStandNatEnd.GetUnits();
                if ((highestStandNbeforeFirstThin.GetUnits() != finalUnits) || (highestStandNbeforeSecondThin.GetUnits() != finalUnits))
                {
                    throw new NotSupportedException("Units differ between simulation periods.");
                }

                OrganonStandTrajectory lowestTrajectory = solution.Lowest.BestTrajectory;
                int previousSpeciesCount = 0;
                foreach (KeyValuePair<FiaCode, int[]> highestTreeSelectionNForSpecies in highestTrajectory.IndividualTreeSelectionBySpecies)
                {
                    Trees highestTreesBeforeFirstThin = highestStandNbeforeFirstThin.TreesBySpecies[highestTreeSelectionNForSpecies.Key];
                    Trees highestTreesBeforeSecondThin = highestStandNbeforeSecondThin.TreesBySpecies[highestTreeSelectionNForSpecies.Key];
                    Trees highestTreesBeforeThirdThin = highestStandNbeforeThirdThin.TreesBySpecies[highestTreeSelectionNForSpecies.Key];
                    Trees highestTreesAtFinal = highestStandNatEnd.TreesBySpecies[highestTreeSelectionNForSpecies.Key];
                    WriteHarvestSchedule.GetDimensionConversions(highestTreesBeforeFirstThin.Units, Units.Metric, out float areaConversionFactor, out float dbhConversionFactor, out float heightConversionFactor);

                    // uncompactedTreeIndex: tree index in periods before thinned trees are removed
                    // compactedTreeIndex: index of retained trees in periods after thinning
                    int[] lowestTreeSelectionN = lowestTrajectory.IndividualTreeSelectionBySpecies[highestTreeSelectionNForSpecies.Key];
                    int[] highestTreeSelectionN = highestTreeSelectionNForSpecies.Value;
                    Debug.Assert(highestTreesBeforeFirstThin.Capacity == highestTreeSelectionN.Length);
                    int secondThinCompactedTreeIndex = 0;
                    int thirdThinCompactedTreeIndex = 0;
                    int finalCompactedTreeIndex = 0;
                    for (int uncompactedTreeIndex = 0; uncompactedTreeIndex < highestTreesBeforeFirstThin.Count; ++uncompactedTreeIndex)
                    {
                        float highestThinBoardFeet = FiaVolume.GetScribnerBoardFeet(highestTreesBeforeFirstThin, uncompactedTreeIndex);

                        string? highestFirstThinDbh = null;
                        string? highestFirstThinHeight = null;
                        string? highestFirstThinCrownRatio = null;
                        string? highestFirstThinExpansionFactor = null;
                        string? highestFirstThinBoardFeet = null;
                        // properties before first thin are undefined if no thinning occurred
                        if (highestTrajectory.GetFirstThinPeriod() != Constant.NoThinPeriod)
                        {
                            highestFirstThinDbh = (dbhConversionFactor * highestTreesBeforeFirstThin.Dbh[uncompactedTreeIndex]).ToString("0.00", CultureInfo.InvariantCulture);
                            highestFirstThinHeight = (heightConversionFactor * highestTreesBeforeFirstThin.Height[uncompactedTreeIndex]).ToString("0.00", CultureInfo.InvariantCulture);
                            highestFirstThinCrownRatio = highestTreesBeforeFirstThin.CrownRatio[uncompactedTreeIndex].ToString("0.000", CultureInfo.InvariantCulture);
                            highestFirstThinExpansionFactor = highestTreesBeforeFirstThin.LiveExpansionFactor[uncompactedTreeIndex].ToString("0.000", CultureInfo.InvariantCulture);
                            highestFirstThinBoardFeet = FiaVolume.GetScribnerBoardFeet(highestTreesBeforeFirstThin, uncompactedTreeIndex).ToString("0.00", CultureInfo.InvariantCulture);
                        }

                        string? highestSecondThinDbh = null;
                        string? highestSecondThinHeight = null;
                        string? highestSecondThinCrownRatio = null;
                        string? highestSecondThinExpansionFactor = null;
                        string? highestSecondThinBoardFeet = null;
                        // properties before second thin are undefined if no thinning, only a first thin, or if tree was removed in first thin
                        bool isRemovedInFirstThin = highestTreeSelectionN[uncompactedTreeIndex] == firstThinPeriod;
                        if ((highestTrajectory.GetSecondThinPeriod() != Constant.NoThinPeriod) && (isRemovedInFirstThin == false))
                        {
                            Debug.Assert(highestTreesBeforeSecondThin.Tag[secondThinCompactedTreeIndex] == highestTreesBeforeFirstThin.Tag[uncompactedTreeIndex]);
                            highestSecondThinDbh = (dbhConversionFactor * highestTreesBeforeSecondThin.Dbh[secondThinCompactedTreeIndex]).ToString("0.00", CultureInfo.InvariantCulture);
                            highestSecondThinHeight = (heightConversionFactor * highestTreesBeforeSecondThin.Height[secondThinCompactedTreeIndex]).ToString("0.00", CultureInfo.InvariantCulture);
                            highestSecondThinCrownRatio = highestTreesBeforeSecondThin.CrownRatio[secondThinCompactedTreeIndex].ToString("0.000", CultureInfo.InvariantCulture);
                            highestSecondThinExpansionFactor = highestTreesBeforeSecondThin.LiveExpansionFactor[secondThinCompactedTreeIndex].ToString("0.000", CultureInfo.InvariantCulture);
                            highestSecondThinBoardFeet = FiaVolume.GetScribnerBoardFeet(highestTreesBeforeSecondThin, secondThinCompactedTreeIndex).ToString("0.00", CultureInfo.InvariantCulture);
                            ++secondThinCompactedTreeIndex; // only need to increment on retained trees, OK to increment here as not referenced below
                        }

                        string? highestThirdThinDbh = null;
                        string? highestThirdThinHeight = null;
                        string? highestThirdThinCrownRatio = null;
                        string? highestThirdThinExpansionFactor = null;
                        string? highestThirdThinBoardFeet = null;
                        // properties before Third thin are undefined if no thinning, only a first thin, or if tree was removed in first thin
                        bool isRemovedInFirstOrSecondThin = isRemovedInFirstThin || (highestTreeSelectionN[uncompactedTreeIndex] == secondThinPeriod);
                        if ((highestTrajectory.GetThirdThinPeriod() != Constant.NoThinPeriod) && (isRemovedInFirstOrSecondThin == false))
                        {
                            Debug.Assert(highestTreesBeforeThirdThin.Tag[thirdThinCompactedTreeIndex] == highestTreesBeforeFirstThin.Tag[uncompactedTreeIndex]);
                            highestThirdThinDbh = (dbhConversionFactor * highestTreesBeforeThirdThin.Dbh[thirdThinCompactedTreeIndex]).ToString("0.00", CultureInfo.InvariantCulture);
                            highestThirdThinHeight = (heightConversionFactor * highestTreesBeforeThirdThin.Height[thirdThinCompactedTreeIndex]).ToString("0.00", CultureInfo.InvariantCulture);
                            highestThirdThinCrownRatio = highestTreesBeforeThirdThin.CrownRatio[thirdThinCompactedTreeIndex].ToString("0.000", CultureInfo.InvariantCulture);
                            highestThirdThinExpansionFactor = highestTreesBeforeThirdThin.LiveExpansionFactor[thirdThinCompactedTreeIndex].ToString("0.000", CultureInfo.InvariantCulture);
                            highestThirdThinBoardFeet = FiaVolume.GetScribnerBoardFeet(highestTreesBeforeThirdThin, thirdThinCompactedTreeIndex).ToString("0.00", CultureInfo.InvariantCulture);
                            ++thirdThinCompactedTreeIndex; // only need to increment on retained trees, OK to increment here as not referenced below
                        }

                        string? highestFinalDbh = null;
                        string? highestFinalHeight = null;
                        string? highestFinalCrownRatio = null;
                        string? highestFinalExpansionFactor = null;
                        string? highestFinalBoardFeet = null;
                        bool isThinned = highestTreeSelectionN[uncompactedTreeIndex] != Constant.NoHarvestPeriod;
                        if (isThinned == false)
                        {
                            Debug.Assert(highestTreesAtFinal.Tag[finalCompactedTreeIndex] == highestTreesBeforeFirstThin.Tag[uncompactedTreeIndex]);
                            highestFinalDbh = (dbhConversionFactor * highestTreesAtFinal.Dbh[finalCompactedTreeIndex]).ToString("0.00", CultureInfo.InvariantCulture);
                            highestFinalHeight = (heightConversionFactor * highestTreesAtFinal.Height[finalCompactedTreeIndex]).ToString("0.00", CultureInfo.InvariantCulture);
                            highestFinalCrownRatio = highestTreesAtFinal.CrownRatio[finalCompactedTreeIndex].ToString("0.000", CultureInfo.InvariantCulture);
                            highestFinalExpansionFactor = highestTreesAtFinal.LiveExpansionFactor[finalCompactedTreeIndex].ToString("0.000", CultureInfo.InvariantCulture);
                            highestFinalBoardFeet = FiaVolume.GetScribnerBoardFeet(highestTreesAtFinal, finalCompactedTreeIndex).ToString("0.00", CultureInfo.InvariantCulture);
                            ++finalCompactedTreeIndex; // only need to increment on retained trees, OK to increment here as not referenced below
                        }

                        // for now, make best guess of using tree tag or index as unique identifier
                        int treeID = highestTreesBeforeFirstThin.Tag[uncompactedTreeIndex] < 0 ? previousSpeciesCount + uncompactedTreeIndex : highestTreesBeforeFirstThin.Tag[uncompactedTreeIndex];
                        writer.WriteLine(linePrefix + "," + treeID + "," +
                                         lowestTreeSelectionN[uncompactedTreeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                         highestTreeSelectionN[uncompactedTreeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                         highestFirstThinDbh + "," +
                                         highestFirstThinHeight + "," +
                                         highestFirstThinCrownRatio + "," +
                                         highestFirstThinExpansionFactor + "," +
                                         highestFirstThinBoardFeet + "," +
                                         highestSecondThinDbh + "," +
                                         highestSecondThinHeight + "," +
                                         highestSecondThinCrownRatio + "," +
                                         highestSecondThinExpansionFactor + "," +
                                         highestSecondThinBoardFeet + "," +
                                         highestThirdThinDbh + "," +
                                         highestThirdThinHeight + "," +
                                         highestThirdThinCrownRatio + "," +
                                         highestThirdThinExpansionFactor + "," +
                                         highestThirdThinBoardFeet + "," +
                                         highestFinalDbh + "," +
                                         highestFinalHeight + "," +
                                         highestFinalCrownRatio + "," +
                                         highestFinalExpansionFactor + "," +
                                         highestFinalBoardFeet);
                    }

                    previousSpeciesCount += highestTreeSelectionN.Length;
                }
            }
        }
    }
}
