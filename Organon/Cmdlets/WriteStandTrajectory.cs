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
    [Cmdlet(VerbsCommunications.Write, "StandTrajectory")]
    public class WriteStandTrajectory : WriteCmdlet
    {
        [Parameter]
        [ValidateNotNull]
        public List<HeuristicSolutionDistribution> Runs { get; set; }

        [Parameter]
        [ValidateNotNull]
        public TimberValue TimberValue { get; set; }

        [Parameter]
        [ValidateNotNull]
        public List<OrganonStandTrajectory> Trajectories { get; set; }

        public WriteStandTrajectory()
        {
            this.Runs = null;
            this.TimberValue = new TimberValue();
            this.Trajectories = null;
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
            if ((this.Runs != null) && (this.Runs.Count < 1))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Runs));
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

                HeuristicParameters heuristicParametersForHeader = runsSpecified ? this.Runs[0].HighestHeuristicParameters : this.Trajectories[0].Heuristic.GetParameters();
                if (heuristicParametersForHeader != null)
                {
                    string heuristicParameters = heuristicParametersForHeader.GetCsvHeader();
                    if (String.IsNullOrEmpty(heuristicParameters) == false)
                    {
                        // TODO: if needed, check if heuristics have different parameters
                        line.Append("," + heuristicParameters);
                    }
                }

                line.Append(",thin age,rotation,stand age,sim year,SDI,QMD,Htop,TPA,BA,standing,harvested,BA removed,BA intensity,TPA decrease,LEV");
                writer.WriteLine(line);
            }

            // rows for periods
            int maxIndex = runsSpecified ? this.Runs.Count : this.Trajectories.Count;
            for (int runOrTrajectoryIndex = 0; runOrTrajectoryIndex < maxIndex; ++runOrTrajectoryIndex)
            {
                OrganonStandTrajectory bestTrajectory;
                HeuristicParameters heuristicParameters = null;
                int moves = -1;
                int runs = -1;
                string runtimeInSeconds = "-1";
                if (runsSpecified)
                {
                    HeuristicSolutionDistribution distribution = this.Runs[runOrTrajectoryIndex];
                    bestTrajectory = distribution.HighestSolution.BestTrajectory;
                    heuristicParameters = distribution.HighestHeuristicParameters;
                    moves = distribution.TotalMoves;
                    runs = distribution.TotalRuns;
                    runtimeInSeconds = distribution.TotalCoreSeconds.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);
                }
                else
                {
                    bestTrajectory = this.Trajectories[runOrTrajectoryIndex];
                    if (bestTrajectory.Heuristic != null)
                    {
                        heuristicParameters = bestTrajectory.Heuristic.GetParameters();
                    }
                }
                if (bestTrajectory.VolumeUnits != VolumeUnits.ScribnerBoardFeetPerAcre)
                {
                    throw new NotSupportedException();
                }

                string heuristicNameAndParameters = "none";
                if (bestTrajectory.Heuristic != null)
                {
                    heuristicNameAndParameters = bestTrajectory.Heuristic.GetName();
                }
                if (heuristicParameters != null)
                {
                    string parameterString = heuristicParameters.GetCsvValues();
                    if (String.IsNullOrEmpty(parameterString) == false)
                    {
                        heuristicNameAndParameters += "," + parameterString;
                    }
                }

                int thinAge = bestTrajectory.GetFirstHarvestAge();
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

                for (int periodIndex = 0; periodIndex < bestTrajectory.PlanningPeriods; ++periodIndex)
                {
                    line.Clear();

                    // get density and volumes
                    OrganonStandDensity density = bestTrajectory.DensityByPeriod[periodIndex];
                    float standingVolume = volumeUnitMultiplier * bestTrajectory.StandingVolumeByPeriod[periodIndex];
                    float harvestMbfPerAcre = 0.0F;
                    float basalAreaRemoved = 0.0F;
                    float basalAreaIntensity = 0.0F;
                    if (bestTrajectory.HarvestVolumesByPeriod.Length > periodIndex)
                    {
                        harvestMbfPerAcre = volumeUnitMultiplier * bestTrajectory.HarvestVolumesByPeriod[periodIndex];
                        basalAreaRemoved = bestTrajectory.BasalAreaRemoved[periodIndex];
                        if (periodIndex > 0)
                        {
                            basalAreaIntensity = basalAreaRemoved / bestTrajectory.DensityByPeriod[periodIndex - 1].BasalAreaPerAcre;
                        }
                    }

                    float tpaDecrease = 0.0F;
                    if (periodIndex > 0)
                    {
                        OrganonStandDensity previousDensity = bestTrajectory.DensityByPeriod[periodIndex - 1];
                        OrganonStandDensity currentDensity = bestTrajectory.DensityByPeriod[periodIndex];
                        if ((currentDensity == null) || (previousDensity == null))
                        {
                            throw new ArgumentOutOfRangeException(null, "Stand density information is missing. Did the heuristic perform at least one fully simulated move?");
                        }
                        tpaDecrease = 1.0F - currentDensity.TreesPerAcre / previousDensity.TreesPerAcre;
                    }

                    Stand stand = bestTrajectory.StandByPeriod[periodIndex];
                    float quadraticMeanDiameter = stand.GetQuadraticMeanDiameter();
                    float reinekeStandDensityIndex = density.TreesPerAcre * MathF.Pow(0.1F * quadraticMeanDiameter, Constant.ReinekeExponent);
                    float topHeight = stand.GetTopHeight();

                    // LEV
                    float landExpectationValue;
                    int periodsFromPresent = Math.Max(periodIndex - 1, 0);
                    if (harvestMbfPerAcre > 0.0F)
                    {
                        float thinningPresentValue = this.TimberValue.GetPresentValueOfThinScribner(bestTrajectory.HarvestVolumesByPeriod[periodIndex], thinAge);
                        float presentToFutureConversionFactor = MathF.Pow(1.0F + this.TimberValue.DiscountRate, rotationLength);
                        float thinningFutureValue = presentToFutureConversionFactor * thinningPresentValue;
                        landExpectationValue = thinningFutureValue / (presentToFutureConversionFactor - 1.0F);
                    }
                    else
                    {
                        float firstRotationPresentValue = this.TimberValue.GetPresentValueOfRegenerationHarvestScribner(bestTrajectory.StandingVolumeByPeriod[periodIndex], rotationLength) - this.TimberValue.ReforestationCostPerAcre;
                        landExpectationValue = this.TimberValue.FirstRotationToLandExpectationValue(firstRotationPresentValue, rotationLength);
                    }

                    int simulationYear = bestTrajectory.PeriodLengthInYears * periodIndex;
                    line.Append(trajectoryName);
                    if (runsSpecified)
                    {
                        line.Append("," + runs + "," + moves + "," + runtimeInSeconds);
                    }
                    line.Append("," + heuristicNameAndParameters);
                    Debug.Assert((harvestMbfPerAcre == 0.0F && basalAreaRemoved == 0.0F) || (harvestMbfPerAcre > 0.0F && basalAreaRemoved > 0.0F));
                    line.Append("," + thinAge.ToString(CultureInfo.InvariantCulture) + "," +
                                rotationLength.ToString(CultureInfo.InvariantCulture) + "," +
                                (bestTrajectory.PeriodZeroAgeInYears + simulationYear).ToString(CultureInfo.InvariantCulture) + "," +
                                simulationYear.ToString(CultureInfo.InvariantCulture) + "," +
                                reinekeStandDensityIndex.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                quadraticMeanDiameter.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                topHeight.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                density.TreesPerAcre.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                density.BasalAreaPerAcre.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                standingVolume.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                harvestMbfPerAcre.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                basalAreaRemoved.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                basalAreaIntensity.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                tpaDecrease.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                landExpectationValue.ToString("0", CultureInfo.InvariantCulture)); ;
                    writer.WriteLine(line);
                }
            }
        }
    }
}
