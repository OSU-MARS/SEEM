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
    [Cmdlet(VerbsCommunications.Write, "StandTrajectory")]
    public class WriteStandTrajectory : WriteCmdlet
    {
        [Parameter]
        [ValidateNotNull]
        public List<HeuristicSolutionDistribution> Runs { get; set; }

        [Parameter]
        public TimberValue TimberValue { get; set; }

        [Parameter]
        [ValidateNotNull]
        public List<OrganonStandTrajectory> Trajectories { get; set; }

        public WriteStandTrajectory()
        {
            this.TimberValue = new TimberValue();
        }

        protected override void ProcessRecord()
        {
            if ((this.Runs == null) && (this.Trajectories == null))
            {
                throw new ArgumentOutOfRangeException();
            }
            if ((this.Runs != null) && (this.Trajectories != null))
            {
                throw new ArgumentOutOfRangeException();
            }

            using StreamWriter writer = this.GetWriter();

            // header
            // TODO: check for mixed units and support TBH
            // TODO: snags per acre or hectare, live and dead QMD?
            bool runsSpecified = this.Runs != null;
            StringBuilder line = new StringBuilder();
            if (this.ShouldWriteHeader())
            {
                line.Append("stand");
                if (runsSpecified)
                {
                    line.Append(",runs,total moves,runtime");
                }
                line.Append(",heuristic");
                if (runsSpecified)
                {
                    line.Append(",default selection probability");
                }
                line.Append(",thin age,rotation,stand age,sim year,TPA,BA,standing,harvested,BA removed,NPV");
                writer.WriteLine(line);
            }

            // rows for periods
            int maxIndex = runsSpecified ? this.Runs.Count : this.Trajectories.Count;
            for (int runOrTrajectoryIndex = 0; runOrTrajectoryIndex < maxIndex; ++runOrTrajectoryIndex)
            {
                OrganonStandTrajectory bestTrajectory;
                float defaultSelectionProbability = -1.0F;
                int moves = -1;
                int runs = -1;
                string runtimeInSeconds = "-1";
                if (runsSpecified)
                {
                    HeuristicSolutionDistribution distribution = this.Runs[runOrTrajectoryIndex];
                    bestTrajectory = distribution.BestSolution.BestTrajectory;
                    defaultSelectionProbability = distribution.DefaultSelectionProbability;
                    moves = distribution.TotalMoves;
                    runs = distribution.TotalRuns;
                    runtimeInSeconds = distribution.TotalCoreSeconds.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);
                }
                else
                {
                    bestTrajectory = this.Trajectories[runOrTrajectoryIndex];
                }
                if (bestTrajectory.VolumeUnits != VolumeUnits.ScribnerBoardFeetPerAcre)
                {
                    throw new NotSupportedException();
                }

                string heuristic = bestTrajectory.Heuristic != null ? bestTrajectory.Heuristic.GetName() : "none";
                int thinYear = bestTrajectory.GetHarvestYear();
                int rotationLength = bestTrajectory.GetRotationLength();

                string trajectoryName = bestTrajectory.Name;
                if (trajectoryName == null)
                {
                    trajectoryName = runOrTrajectoryIndex.ToString(CultureInfo.InvariantCulture);
                }
                float volumeUnitMultiplier = 1.0F;
                if (bestTrajectory.VolumeUnits == VolumeUnits.ScribnerBoardFeetPerAcre)
                {
                    volumeUnitMultiplier = 0.001F;
                }

                int initialStandAge = bestTrajectory.GetInitialStandAge();
                for (int periodIndex = 0; periodIndex < bestTrajectory.PlanningPeriods; ++periodIndex)
                {
                    line.Clear();

                    // get density and volumes
                    OrganonStandDensity density = bestTrajectory.DensityByPeriod[periodIndex];
                    float standingVolume = volumeUnitMultiplier * bestTrajectory.StandingVolumeByPeriod[periodIndex];
                    float harvestMbfPerAcre = 0.0F;
                    float basalAreaRemoved = 0.0F;
                    if (bestTrajectory.HarvestVolumesByPeriod.Length > periodIndex)
                    {
                        harvestMbfPerAcre = volumeUnitMultiplier * bestTrajectory.HarvestVolumesByPeriod[periodIndex];
                        basalAreaRemoved = bestTrajectory.BasalAreaRemoved[periodIndex];
                    }

                    // NPV
                    float netPresentValue = 0.0F;
                    int periodsFromPresent = Math.Max(periodIndex - 1, 0);
                    if (harvestMbfPerAcre > 0.0F)
                    {
                        netPresentValue = this.TimberValue.GetPresentValueOfThinScribner(bestTrajectory.HarvestVolumesByPeriod[periodIndex], periodsFromPresent, bestTrajectory.PeriodLengthInYears);
                    }
                    else
                    {
                        netPresentValue = this.TimberValue.GetPresentValueOfFinalHarvestScribner(bestTrajectory.StandingVolumeByPeriod[periodIndex], periodsFromPresent, bestTrajectory.PeriodLengthInYears);
                    }

                    int simulationYear = bestTrajectory.PeriodLengthInYears * periodIndex;
                    line.Append(trajectoryName);
                    if (runsSpecified)
                    {
                        line.Append("," + runs + "," + moves + "," + runtimeInSeconds);
                    }
                    line.Append("," + heuristic + "," +
                                defaultSelectionProbability.ToString(Constant.DefaultSelectionFormat, CultureInfo.InvariantCulture) + "," +
                                thinYear.ToString(CultureInfo.InvariantCulture) + "," +
                                rotationLength.ToString(CultureInfo.InvariantCulture) + "," +
                                (initialStandAge + simulationYear).ToString(CultureInfo.InvariantCulture) + "," +
                                simulationYear.ToString(CultureInfo.InvariantCulture) + "," +
                                density.TreesPerAcre.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                density.BasalAreaPerAcre.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                standingVolume.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                harvestMbfPerAcre.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                basalAreaRemoved.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                netPresentValue.ToString("0", CultureInfo.InvariantCulture)); ;
                    writer.WriteLine(line);
                }
            }
        }
    }
}
