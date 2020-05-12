using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "Harvest")]
    public class WriteHarvest : Cmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string CsvFile;

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public List<HeuristicSolutionDistribution> Runs { get; set; }

        protected override void ProcessRecord()
        {
            using FileStream stream = new FileStream(this.CsvFile, FileMode.Create, FileAccess.Write, FileShare.Read);
            using StreamWriter writer = new StreamWriter(stream);

            StringBuilder line = new StringBuilder("period");
            // harvest volume headers
            for (int runIndex = 0; runIndex < this.Runs.Count; ++runIndex)
            {
                OrganonStandTrajectory bestTrajectory = this.Runs[runIndex].BestSolution.BestTrajectory;
                line.Append("," + bestTrajectory.Name + "harvest");
            }
            // standing volume headers
            int maxPlanningPeriod = 0;
            for (int runIndex = 0; runIndex < this.Runs.Count; ++runIndex)
            {
                OrganonStandTrajectory bestTrajectory = this.Runs[runIndex].BestSolution.BestTrajectory;
                line.Append("," + bestTrajectory.Name + "standing");
                maxPlanningPeriod = Math.Max(maxPlanningPeriod, bestTrajectory.StandingVolumeByPeriod.Length);
            }
            writer.WriteLine(line);

            for (int periodIndex = 0; periodIndex < maxPlanningPeriod; ++periodIndex)
            {
                line.Clear();
                line.Append(periodIndex);

                for (int runIndex = 0; runIndex < this.Runs.Count; ++runIndex)
                {
                    line.Append(",");

                    Heuristic heuristic = this.Runs[runIndex].BestSolution;
                    if (heuristic.BestTrajectory.HarvestVolumesByPeriod.Length > periodIndex)
                    {
                        double harvestVolume = heuristic.BestTrajectory.HarvestVolumesByPeriod[periodIndex];
                        if (heuristic.BestTrajectory.VolumeUnits == VolumeUnits.ScribnerBoardFeetPerAcre)
                        {
                            harvestVolume *= 0.001;
                        }
                        line.Append(harvestVolume.ToString(CultureInfo.InvariantCulture));
                    }
                }

                for (int runIndex = 0; runIndex < this.Runs.Count; ++runIndex)
                {
                    line.Append(",");

                    Heuristic heuristic = this.Runs[runIndex].BestSolution;
                    if (heuristic.BestTrajectory.StandingVolumeByPeriod.Length > periodIndex)
                    {
                        double standingVolume = heuristic.BestTrajectory.StandingVolumeByPeriod[periodIndex];
                        if (heuristic.BestTrajectory.VolumeUnits == VolumeUnits.ScribnerBoardFeetPerAcre)
                        {
                            standingVolume *= 0.001;
                        }
                        line.Append(standingVolume.ToString(CultureInfo.InvariantCulture));
                    }
                }

                writer.WriteLine(line);
            }
        }
    }
}
