using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "HarvestSchedule")]
    public class WriteHarvestSchedule : WriteCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public List<HeuristicSolutionDistribution>? Runs { get; set; }

        protected override void ProcessRecord()
        {
            if (this.Runs!.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.Runs));
            }

            using StreamWriter writer = this.GetWriter();
            StringBuilder line = new StringBuilder();
            if (this.Append == false)
            {
                HeuristicParameters? highestHeuristicParameters = this.Runs![0].HighestHeuristicParameters;
                if (highestHeuristicParameters == null)
                {
                    throw new NotSupportedException("Cannot generate schedule header because first run has no heuristic parameters.");
                }
                line.Append("stand,heuristic," + highestHeuristicParameters.GetCsvHeader() + ",discount rate,first thin,second thin,rotation,tree,lowest selection,highest selection,highest first thin DBH,highest first thin height,highest first thin CR,highest first thin EF,highest first thin BF,highest second thin DBH,highest second thin height,highest second thin CR,highest second thin EF,highest second thin BF,highest final DBH,highest final height,highest final CR,highest final EF,highest final BF");
                writer.WriteLine(line);
            }

            for (int runIndex = 0; runIndex < this.Runs!.Count; ++runIndex)
            {
                HeuristicSolutionDistribution distribution = this.Runs[runIndex];
                if ((distribution.HighestHeuristicParameters == null) ||
                    (distribution.HighestSolution == null) ||
                    (distribution.HighestSolution.BestTrajectory == null) ||
                    (distribution.HighestSolution.BestTrajectory.Heuristic == null) ||
                    (distribution.LowestSolution == null))
                {
                    throw new NotSupportedException("Run " + runIndex + " is missing a highest solution, lowest solution, highest solution parameters, highest heuristic trajectory, or back link from highest trajectory to is generating heuristic.");
                }

                OrganonStandTrajectory highestTrajectoryN = distribution.HighestSolution.BestTrajectory;
                int firstThinPeriod = highestTrajectoryN.GetFirstHarvestPeriod();
                int periodBeforeFirstThin = firstThinPeriod - 1;
                if (periodBeforeFirstThin < 0)
                {
                    periodBeforeFirstThin = highestTrajectoryN.PlanningPeriods - 1;
                }
                int secondThinPeriod = highestTrajectoryN.GetSecondHarvestPeriod();
                int periodBeforeSecondThin = secondThinPeriod - 1;
                if (periodBeforeSecondThin < 0)
                {
                    periodBeforeSecondThin = highestTrajectoryN.PlanningPeriods - 1;
                }

                int firstThinAge = highestTrajectoryN.GetFirstHarvestAge();
                string? firstThinAgeString = firstThinAge != -1 ? firstThinAge.ToString(CultureInfo.InvariantCulture) : null;
                int secondThinAge = highestTrajectoryN.GetSecondHarvestAge();
                string? secondThinAgeString = secondThinAge != -1 ? secondThinAge.ToString(CultureInfo.InvariantCulture) : null;

                string linePrefix = highestTrajectoryN.Name + "," + highestTrajectoryN.Heuristic.GetName() + "," + 
                    distribution.HighestHeuristicParameters.GetCsvValues() + "," + 
                    highestTrajectoryN.TimberValue.DiscountRate.ToString(CultureInfo.InvariantCulture) + "," +
                    firstThinAgeString + "," +
                    secondThinAgeString + "," +
                    highestTrajectoryN.GetRotationLength().ToString(CultureInfo.InvariantCulture);

                Stand? highestStandNbeforeFirstThin = highestTrajectoryN.StandByPeriod[periodBeforeFirstThin];
                Stand? highestStandNbeforeSecondThin = highestTrajectoryN.StandByPeriod[periodBeforeSecondThin];
                Stand? highestStandNatEnd = highestTrajectoryN.StandByPeriod[^1];
                if ((highestStandNbeforeFirstThin == null) || (highestStandNbeforeSecondThin == null) || (highestStandNatEnd == null))
                {
                    throw new ParameterOutOfRangeException(nameof(this.Runs), "Highest stand in run has not been fully simulated. Did the heuristic perform at least one move?");
                }
                Units finalUnits = highestStandNatEnd.GetUnits();
                if ((highestStandNbeforeFirstThin.GetUnits() != finalUnits) || (highestStandNbeforeSecondThin.GetUnits() != finalUnits))
                {
                    throw new NotSupportedException("Units differ between simulation periods.");
                }

                OrganonStandTrajectory lowestTrajectoryN = distribution.LowestSolution.BestTrajectory;
                int previousSpeciesCount = 0;
                foreach (KeyValuePair<FiaCode, int[]> highestTreeSelectionNForSpecies in highestTrajectoryN.IndividualTreeSelectionBySpecies)
                {
                    Trees highestTreesBeforeFirstThin = highestStandNbeforeFirstThin.TreesBySpecies[highestTreeSelectionNForSpecies.Key];
                    Trees highestTreesBeforeSecondThin = highestStandNbeforeSecondThin.TreesBySpecies[highestTreeSelectionNForSpecies.Key];
                    Trees highestTreesAtFinal = highestStandNatEnd.TreesBySpecies[highestTreeSelectionNForSpecies.Key];
                    WriteHarvestSchedule.GetDimensionConversions(highestTreesBeforeFirstThin.Units, Units.Metric, out float areaConversionFactor, out float dbhConversionFactor, out float heightConversionFactor);

                    // uncompactedTreeIndex: tree index in periods before thinned trees are removed
                    // compactedTreeIndex: index of retained trees in periods after thinning
                    int[] lowestTreeSelectionN = lowestTrajectoryN.IndividualTreeSelectionBySpecies[highestTreeSelectionNForSpecies.Key];
                    int[] highestTreeSelectionN = highestTreeSelectionNForSpecies.Value;
                    Debug.Assert(highestTreesBeforeFirstThin.Capacity == highestTreeSelectionN.Length);
                    int secondThinCompactedTreeIndex = 0;
                    int finalCompactedTreeIndex = 0;
                    for (int uncompactedTreeIndex = 0; uncompactedTreeIndex < highestTreesBeforeFirstThin.Count; ++uncompactedTreeIndex)
                    {
                        line.Clear();

                        float highestThinBoardFeet = FiaVolume.GetScribnerBoardFeet(highestTreesBeforeFirstThin, uncompactedTreeIndex);

                        string? highestFirstThinDbh = null;
                        string? highestFirstThinHeight = null;
                        string? highestFirstThinCrownRatio = null;
                        string? highestFirstThinExpansionFactor = null;
                        string? highestFirstThinBoardFeet = null;
                        // properties before first thin are undefined if no thinning occurred
                        if (firstThinAge > 0)
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
                        if ((secondThinAge > 0) && (isRemovedInFirstThin == false))
                        {
                            Debug.Assert(highestTreesBeforeSecondThin.Tag[secondThinCompactedTreeIndex] == highestTreesBeforeFirstThin.Tag[uncompactedTreeIndex]);
                            highestSecondThinDbh = (dbhConversionFactor * highestTreesBeforeSecondThin.Dbh[secondThinCompactedTreeIndex]).ToString("0.00", CultureInfo.InvariantCulture);
                            highestSecondThinHeight = (heightConversionFactor * highestTreesBeforeSecondThin.Height[secondThinCompactedTreeIndex]).ToString("0.00", CultureInfo.InvariantCulture);
                            highestSecondThinCrownRatio = highestTreesBeforeSecondThin.CrownRatio[secondThinCompactedTreeIndex].ToString("0.000", CultureInfo.InvariantCulture);
                            highestSecondThinExpansionFactor = highestTreesBeforeSecondThin.LiveExpansionFactor[secondThinCompactedTreeIndex].ToString("0.000", CultureInfo.InvariantCulture);
                            highestSecondThinBoardFeet = FiaVolume.GetScribnerBoardFeet(highestTreesBeforeSecondThin, secondThinCompactedTreeIndex).ToString("0.00", CultureInfo.InvariantCulture);
                            ++secondThinCompactedTreeIndex; // only need to increment on retained trees, OK to increment here as not referenced below
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
                        line.Append(linePrefix + "," + treeID + "," +
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
                                    highestFinalDbh + "," +
                                    highestFinalHeight + "," +
                                    highestFinalCrownRatio + "," +
                                    highestFinalExpansionFactor + "," +
                                    highestFinalBoardFeet);

                        writer.WriteLine(line);
                    }

                    previousSpeciesCount += highestTreeSelectionN.Length;
                }
            }
        }
    }
}
