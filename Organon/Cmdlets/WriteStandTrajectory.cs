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
            this.TimberValue = TimberValue.Default;
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

                HeuristicParameters heuristicParametersForHeader = null;
                if (runsSpecified)
                {
                    heuristicParametersForHeader = this.Runs[0].HighestHeuristicParameters;
                }
                else if(this.Trajectories[0].Heuristic != null)
                {
                    heuristicParametersForHeader = this.Trajectories[0].Heuristic.GetParameters();
                }

                if (heuristicParametersForHeader != null)
                {
                    string heuristicParameters = heuristicParametersForHeader.GetCsvHeader();
                    if (String.IsNullOrEmpty(heuristicParameters) == false)
                    {
                        // TODO: if needed, check if heuristics have different parameters
                        line.Append("," + heuristicParameters);
                    }
                }

                line.Append(",thin age,rotation,stand age,sim year,SDI,QMD,Htop,TPH,BA,standing CMH,standing MBFH,harvest CMH,harvest MBFH,BA removed,BA intensity,TPH decrease,LEV");
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

                this.GetDimensionConversions(Units.English, Units.Metric, out float areaConversionFactor, out float dbhConversionFactor, out float heightConversionFactor);
                this.GetBasalAreaConversion(Units.English, Units.Metric, out float basalAreaConversionFactor);
                for (int periodIndex = 0; periodIndex < bestTrajectory.PlanningPeriods; ++periodIndex)
                {
                    line.Clear();

                    // get density and volumes
                    float standingVolumeCubic = bestTrajectory.StandingVolume.Cubic[periodIndex]; // m³/ha
                    float standingVolumeScribner = bestTrajectory.StandingVolume.Scribner[periodIndex]; // MBF/ha
                    float harvestVolumeCubic = bestTrajectory.ThinningVolume.Cubic[periodIndex]; // m³/ha
                    float harvestVolumeScribner = bestTrajectory.ThinningVolume.Scribner[periodIndex]; // MBF/ha
                    float basalAreaRemoved = bestTrajectory.BasalAreaRemoved[periodIndex]; // ft²/acre
                    float basalAreaIntensity = 0.0F;
                    if (periodIndex > 0)
                    {
                        basalAreaIntensity = basalAreaRemoved / bestTrajectory.DensityByPeriod[periodIndex - 1].BasalAreaPerAcre;
                    }

                    float treesPerAcreDecrease = 0.0F;
                    if (periodIndex > 0)
                    {
                        OrganonStandDensity previousDensity = bestTrajectory.DensityByPeriod[periodIndex - 1];
                        OrganonStandDensity currentDensity = bestTrajectory.DensityByPeriod[periodIndex];
                        if ((currentDensity == null) || (previousDensity == null))
                        {
                            throw new ArgumentOutOfRangeException(null, "Stand density information is missing. Did the heuristic perform at least one fully simulated move?");
                        }
                        treesPerAcreDecrease = 1.0F - currentDensity.TreesPerAcre / previousDensity.TreesPerAcre;
                    }

                    Stand stand = bestTrajectory.StandByPeriod[periodIndex];
                    OrganonStandDensity density = bestTrajectory.DensityByPeriod[periodIndex];
                    float quadraticMeanDiameter = stand.GetQuadraticMeanDiameter(); // leave in inches for Reineke SDI
                    float reinekeStandDensityIndex = areaConversionFactor * density.TreesPerAcre * MathF.Pow(0.1F * quadraticMeanDiameter, Constant.ReinekeExponent);
                    quadraticMeanDiameter *= dbhConversionFactor;
                    float topHeight = heightConversionFactor * stand.GetTopHeight();

                    float treesPerUnitArea = areaConversionFactor * density.TreesPerAcre;
                    float basalAreaPerUnitArea = basalAreaConversionFactor * density.BasalAreaPerAcre;
                    basalAreaRemoved *= basalAreaConversionFactor;
                    float treesPerUnitAreaDecrease = areaConversionFactor * treesPerAcreDecrease;

                    // LEV
                    float landExpectationValue;
                    int periodsFromPresent = Math.Max(periodIndex - 1, 0);
                    if (harvestVolumeScribner > 0.0F)
                    {
                        float thinningNetPresentValue = bestTrajectory.ThinningVolume.NetPresentValue[periodIndex];
                        float presentToFutureConversionFactor = MathF.Pow(1.0F + this.TimberValue.DiscountRate, rotationLength);
                        float thinningFutureValue = presentToFutureConversionFactor * thinningNetPresentValue;
                        landExpectationValue = thinningFutureValue / (presentToFutureConversionFactor - 1.0F);
                    }
                    else
                    {
                        float finalHarvestNetPresentValue = bestTrajectory.StandingVolume.NetPresentValue[periodIndex] - this.TimberValue.ReforestationCostPerHectare;
                        landExpectationValue = this.TimberValue.ToLandExpectationValue(finalHarvestNetPresentValue, rotationLength);
                    }

                    int simulationYear = bestTrajectory.PeriodLengthInYears * periodIndex;
                    line.Append(trajectoryName);
                    if (runsSpecified)
                    {
                        line.Append("," + runs + "," + moves + "," + runtimeInSeconds);
                    }
                    line.Append("," + heuristicNameAndParameters);
                    Debug.Assert((harvestVolumeScribner == 0.0F && basalAreaRemoved == 0.0F) || (harvestVolumeScribner > 0.0F && basalAreaRemoved > 0.0F));
                    line.Append("," + thinAge.ToString(CultureInfo.InvariantCulture) + "," +
                                rotationLength.ToString(CultureInfo.InvariantCulture) + "," +
                                (bestTrajectory.PeriodZeroAgeInYears + simulationYear).ToString(CultureInfo.InvariantCulture) + "," +
                                simulationYear.ToString(CultureInfo.InvariantCulture) + "," +
                                reinekeStandDensityIndex.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                quadraticMeanDiameter.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                topHeight.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                treesPerUnitArea.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                basalAreaPerUnitArea.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                standingVolumeCubic.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                standingVolumeScribner.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                harvestVolumeCubic.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                harvestVolumeScribner.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                basalAreaRemoved.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                basalAreaIntensity.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                treesPerUnitAreaDecrease.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                landExpectationValue.ToString("0", CultureInfo.InvariantCulture)); ;
                    writer.WriteLine(line);
                }
            }
        }
    }
}
