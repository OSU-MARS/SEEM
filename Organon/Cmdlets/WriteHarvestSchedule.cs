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
        public DataFormat Format { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public List<HeuristicSolutionDistribution> Runs { get; set; }

        public WriteHarvestSchedule()
        {
            this.Format = DataFormat.Long;
        }

        protected override void ProcessRecord()
        {
            if (this.Runs.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Runs));
            }

            using StreamWriter writer = this.GetWriter(); 
            switch (this.Format)
            {
                case DataFormat.Long:
                    this.WriteLongFormat(writer);
                    break;
                case DataFormat.Wide:
                    this.WriteWideFormat(writer);
                    break;
                default:
                    throw new NotSupportedException(String.Format("Unhandled data format {0}.", this.Format));
            }
        }

        private void WriteLongFormat(StreamWriter writer)
        {
            StringBuilder line = new StringBuilder();
            if (this.Append == false)
            {
                line.Append("stand,heuristic," + this.Runs[0].HighestHeuristicParameters.GetCsvHeader() + ",thin age,rotation,tree,lowest selection,highest selection,DBH,height,expansion factor");
                writer.WriteLine(line);
            }

            for (int runIndex = 0; runIndex < this.Runs.Count; ++runIndex)
            {
                HeuristicSolutionDistribution distribution = this.Runs[runIndex];
                OrganonStandTrajectory highestTrajectoryN = distribution.HighestSolution.BestTrajectory;
                int periodBeforeHarvest = highestTrajectoryN.GetFirstHarvestPeriod() - 1;
                if (periodBeforeHarvest < 0)
                {
                    periodBeforeHarvest = highestTrajectoryN.PlanningPeriods - 1;
                }
                string linePrefix = highestTrajectoryN.Name + "," + highestTrajectoryN.Heuristic.GetName() + "," + distribution.HighestHeuristicParameters.GetCsvValues() + "," + highestTrajectoryN.GetFirstHarvestAge() + "," + highestTrajectoryN.GetRotationLength();

                OrganonStandTrajectory lowestTrajectoryN = distribution.LowestSolution.BestTrajectory;
                int previousSpeciesCount = 0;
                foreach (KeyValuePair<FiaCode, int[]> highestTreeSelectionNForSpecies in highestTrajectoryN.IndividualTreeSelectionBySpecies)
                {
                    Trees treesOfSpecies = highestTrajectoryN.StandByPeriod[periodBeforeHarvest].TreesBySpecies[highestTreeSelectionNForSpecies.Key];

                    int[] lowestTreeSelectionN = lowestTrajectoryN.IndividualTreeSelectionBySpecies[highestTreeSelectionNForSpecies.Key];
                    int[] highestTreeSelectionN = highestTreeSelectionNForSpecies.Value;
                    Debug.Assert(treesOfSpecies.Capacity == highestTreeSelectionN.Length);
                    for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                    {
                        line.Clear();

                        // for now, make best guess of using tree tag or index as unique identifier
                        int tree = treesOfSpecies.Tag[treeIndex] < 0 ? previousSpeciesCount + treeIndex : treesOfSpecies.Tag[treeIndex];
                        line.Append(linePrefix + "," + tree + "," +
                                    lowestTreeSelectionN[treeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                    highestTreeSelectionN[treeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                    treesOfSpecies.Dbh[treeIndex].ToString("0.00", CultureInfo.InvariantCulture) + "," + 
                                    treesOfSpecies.Height[treeIndex].ToString("0.00", CultureInfo.InvariantCulture) + "," + 
                                    treesOfSpecies.LiveExpansionFactor[treeIndex].ToString("0.000", CultureInfo.InvariantCulture));

                        writer.WriteLine(line);
                    }

                    previousSpeciesCount += highestTreeSelectionN.Length;
                }
            }
        }

        private void WriteWideFormat(StreamWriter writer)
        {
            // for now, assume all heuristics are from the same stand with the same tree ordering and ingrowth pattern
            SortedDictionary<FiaCode, int[]> treeSelection0BySpecies = this.Runs[0].HighestSolution.BestTrajectory.IndividualTreeSelectionBySpecies;
            for (int heuristicIndex = 1; heuristicIndex < this.Runs.Count; ++heuristicIndex)
            {
                SortedDictionary<FiaCode, int[]> treeSelectionBySpecies = this.Runs[heuristicIndex].HighestSolution.BestTrajectory.IndividualTreeSelectionBySpecies;
                if (SortedDictionaryExtensions.ValueLengthsIdentical(treeSelection0BySpecies, treeSelectionBySpecies) == false)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.Runs));
                }
            }

            StringBuilder line = new StringBuilder();
            if (this.ShouldWriteHeader())
            {
                line.Append("tree");
                for (int runIndex = 0; runIndex < this.Runs.Count; ++runIndex)
                {
                    OrganonStandTrajectory bestTrajectory = this.Runs[runIndex].HighestSolution.BestTrajectory;
                    line.Append("," + bestTrajectory.Name + bestTrajectory.Heuristic + bestTrajectory.GetFirstHarvestAge() + "." + bestTrajectory.GetRotationLength());
                }
                writer.WriteLine(line);
            }

            int previousSpeciesCount = 0;
            foreach (KeyValuePair<FiaCode, int[]> treeSelection0ForSpecies in treeSelection0BySpecies)
            {
                OrganonStandTrajectory bestTrajectory0 = this.Runs[0].HighestSolution.BestTrajectory;
                Trees treesOfSpecies = bestTrajectory0.StandByPeriod[0].TreesBySpecies[treeSelection0ForSpecies.Key];
                Debug.Assert(treesOfSpecies.Count <= treeSelection0ForSpecies.Value.Length);
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    line.Clear();
                    // for now, make best guess of using tree tag or index as unique identifier
                    line.Append(treesOfSpecies.Tag[treeIndex] == 0 ? previousSpeciesCount + treeIndex : treesOfSpecies.Tag[treeIndex]);

                    for (int runIndex = 0; runIndex < this.Runs.Count; ++runIndex)
                    {
                        OrganonStandTrajectory bestTrajectoryN = this.Runs[runIndex].HighestSolution.BestTrajectory;
                        int[] treeSelectionN = bestTrajectoryN.IndividualTreeSelectionBySpecies[treeSelection0ForSpecies.Key];
                        int harvestPeriod = treeSelectionN[treeIndex];
                        line.Append("," + harvestPeriod.ToString(CultureInfo.InvariantCulture));
                    }

                    writer.WriteLine(line);
                }

                previousSpeciesCount += treesOfSpecies.Count;
            }
        }
    }
}
