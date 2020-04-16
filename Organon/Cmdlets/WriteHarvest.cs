using Osu.Cof.Ferm.Heuristics;
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
        public List<Heuristic> Heuristics { get; set; }

        protected override void ProcessRecord()
        {
            using FileStream stream = new FileStream(this.CsvFile, FileMode.Create, FileAccess.Write, FileShare.Read);
            using StreamWriter writer = new StreamWriter(stream);

            StringBuilder line = new StringBuilder("period");
            // harvest volume headers
            for (int heuristicIndex = 0; heuristicIndex < this.Heuristics.Count; ++heuristicIndex)
            {
                Heuristic heuristic = this.Heuristics[heuristicIndex];
                line.Append("," + heuristic.GetColumnName() + "H");
            }
            // standing volume headers
            int maxPlanningPeriod = 0;
            for (int heuristicIndex = 0; heuristicIndex < this.Heuristics.Count; ++heuristicIndex)
            {
                Heuristic heuristic = this.Heuristics[heuristicIndex];
                line.Append("," + heuristic.GetColumnName() + "S");
                maxPlanningPeriod = Math.Max(maxPlanningPeriod, heuristic.BestTrajectory.StandingVolumeByPeriod.Length);
            }
            writer.WriteLine(line);

            for (int periodIndex = 0; periodIndex < maxPlanningPeriod; ++periodIndex)
            {
                line.Clear();
                line.Append(periodIndex);

                for (int heuristicIndex = 0; heuristicIndex < this.Heuristics.Count; ++heuristicIndex)
                {
                    line.Append(",");

                    Heuristic heuristic = this.Heuristics[heuristicIndex];
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

                for (int heuristicIndex = 0; heuristicIndex < this.Heuristics.Count; ++heuristicIndex)
                {
                    line.Append(",");

                    Heuristic heuristic = this.Heuristics[heuristicIndex];
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
