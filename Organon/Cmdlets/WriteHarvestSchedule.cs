using Osu.Cof.Ferm.Heuristics;
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
    public class WriteHarvestSchedule : Cmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string CsvFile;

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public List<Heuristic> Heuristics { get; set; }

        protected override void ProcessRecord()
        {
            if (this.Heuristics.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Heuristics));
            }

            // for now, assume all heuristics are from the same stand with the same tree ordering and ingrowth pattern
            SortedDictionary<FiaCode, int[]> treeSelection0BySpecies = this.Heuristics[0].BestTrajectory.IndividualTreeSelectionBySpecies;
            for (int heuristicIndex = 1; heuristicIndex < this.Heuristics.Count; ++heuristicIndex)
            {
                SortedDictionary<FiaCode, int[]> treeSelectionNBySpecies = this.Heuristics[heuristicIndex].BestTrajectory.IndividualTreeSelectionBySpecies;
                if (SortedDictionaryExtensions.ValueLengthsIdentical(treeSelection0BySpecies, treeSelectionNBySpecies) == false)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.Heuristics));
                }
            }

            using FileStream stream = new FileStream(this.CsvFile, FileMode.Create, FileAccess.Write, FileShare.Read);
            using StreamWriter writer = new StreamWriter(stream);

            StringBuilder line = new StringBuilder("tree");
            for (int heuristicIndex = 0; heuristicIndex < this.Heuristics.Count; ++heuristicIndex)
            {
                Heuristic heuristic = this.Heuristics[heuristicIndex];
                line.Append("," + heuristic.GetColumnName());
            }
            writer.WriteLine(line);

            int previousSpeciesCount = 0;
            foreach (KeyValuePair<FiaCode, int[]> treeSelection0ForSpecies in treeSelection0BySpecies)
            {
                Trees treesOfSpecies = this.Heuristics[0].BestTrajectory.StandByPeriod[0].TreesBySpecies[treeSelection0ForSpecies.Key];
                Debug.Assert(treesOfSpecies.Count <= treeSelection0ForSpecies.Value.Length);
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    line.Clear();
                    // for now, best guess of using tree tag or index as unique identifier
                    line.Append(treesOfSpecies.Tag[treeIndex] == 0 ? previousSpeciesCount + treeIndex : treesOfSpecies.Tag[treeIndex]);

                    for (int heuristicIndex = 0; heuristicIndex < this.Heuristics.Count; ++heuristicIndex)
                    {
                        int[] treeSelectionN = this.Heuristics[heuristicIndex].BestTrajectory.IndividualTreeSelectionBySpecies[treeSelection0ForSpecies.Key];
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
