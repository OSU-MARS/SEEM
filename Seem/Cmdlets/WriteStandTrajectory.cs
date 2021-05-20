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
        [ValidateRange(0.1F, 100.0F)]
        public float DiameterClassSize { get; set; } // cm

        [Parameter]
        [ValidateRange(1.0F, 1000.0F)]
        public float MaximumDiameter { get; set; } // cm

        [Parameter]
        [ValidateNotNull]
        public HeuristicResultSet? Results { get; set; }

        [Parameter]
        [ValidateNotNull]
        public List<OrganonStandTrajectory>? Trajectories { get; set; }

        public WriteStandTrajectory()
        {
            this.DiameterClassSize = Constant.Bucking.DiameterClassSizeInCentimeters;
            this.MaximumDiameter = Constant.Bucking.DefaultMaximumDiameterInCentimeters;
            this.Results = null;
            this.Trajectories = null;
        }

        protected OrganonStandTrajectory GetHighestTrajectoryAndLinePrefix(int runOrTrajectoryIndex, out StringBuilder linePrefix)
        {
            OrganonStandTrajectory highestTrajectory;
            HeuristicParameters? heuristicParameters = null;
            if (this.Results != null)
            {
                HeuristicSolutionPool solution = this.Results.Solutions[runOrTrajectoryIndex];
                if (solution.Highest == null)
                {
                    throw new NotSupportedException("Run " + runOrTrajectoryIndex + " is missing a highest solution.");
                }
                highestTrajectory = solution.Highest.BestTrajectory;
                heuristicParameters = this.Results.Distributions[runOrTrajectoryIndex].HeuristicParameters;
            }
            else
            {
                highestTrajectory = this.Trajectories![runOrTrajectoryIndex];
                if (highestTrajectory.Heuristic != null)
                {
                    heuristicParameters = highestTrajectory.Heuristic.GetParameters();
                }
            }

            string heuristicName = "none";
            if (highestTrajectory.Heuristic != null)
            {
                heuristicName = highestTrajectory.Heuristic.GetName();
            }
            string? heuristicParameterString = null;
            if (heuristicParameters != null)
            {
                heuristicParameterString = heuristicParameters.GetCsvValues();
            }

            string? trajectoryName = highestTrajectory.Name;
            if (trajectoryName == null)
            {
                trajectoryName = runOrTrajectoryIndex.ToString(CultureInfo.InvariantCulture);
            }

            linePrefix = new(trajectoryName + "," + heuristicName + ",");
            if (heuristicParameterString != null)
            {
                linePrefix.Append(heuristicParameterString + ",");
            }
            linePrefix.Append(WriteCmdlet.GetRateAndAgeCsvValues(highestTrajectory));

            return highestTrajectory;
        }

        protected override void ProcessRecord()
        {
            this.ValidateParameters();

            using StreamWriter writer = this.GetWriter();

            // header
            // TODO: check for mixed units and support TBH
            // TODO: snags per acre or hectare, live and dead QMD?
            bool runsSpecified = this.Results != null;
            if (this.ShouldWriteHeader())
            {
                StringBuilder line = new("stand,heuristic");

                HeuristicParameters? heuristicParametersForHeader = null;
                if (runsSpecified)
                {
                    heuristicParametersForHeader = this.Results!.Distributions[0].HeuristicParameters;
                }
                else if(this.Trajectories![0].Heuristic != null)
                {
                    heuristicParametersForHeader = this.Trajectories[0].Heuristic!.GetParameters();
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

                line.Append("," + WriteCmdlet.RateAndAgeCsvHeader + ",stand age,TPH,QMD,Htop,BA,SDI,SPH,snag QMD,standing CMH,harvest CMH,standing MBFH,harvest MBFH,BA removed,BA intensity,TPH decrease,NPV,LEV,standing 2S CMH,standing 3S CMH,standing 4S CMH,harvest 2S CMH,harvest 3S CMH,harvest 4S CMH,standing 2S MBFH,standing 3S MBFH,standing 4S MBFH,harvest 2S MBFH,harvest 3S MBFH,harvest 4S MBFH,NPV 2S, NPV 3S,NPV 4S,live biomass");
                writer.WriteLine(line);
            }

            // rows for periods
            int maxIndex = runsSpecified ? this.Results!.Count : this.Trajectories!.Count;
            for (int runOrTrajectoryIndex = 0; runOrTrajectoryIndex < maxIndex; ++runOrTrajectoryIndex)
            {
                OrganonStandTrajectory highestTrajectory = this.GetHighestTrajectoryAndLinePrefix(runOrTrajectoryIndex, out StringBuilder linePrefix);
                string coreTimeInSeconds = "";
                if (runsSpecified)
                {
                    HeuristicDistribution distribution = this.Results!.Distributions[runOrTrajectoryIndex];
                    coreTimeInSeconds = distribution.TotalCoreSeconds.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);
                }

                Units trajectoryUnits = highestTrajectory.GetUnits();
                if (trajectoryUnits != Units.English)
                {
                    throw new NotSupportedException("Expected Organon stand trajectory with English Units.");
                }
                highestTrajectory.GetGradedVolumes(out StandGradedVolume gradedVolumeStanding, out StandGradedVolume gradedVolumeHarvested);

                SnagLogTable snagsAndLogs = new(highestTrajectory, this.MaximumDiameter, this.DiameterClassSize);
                
                float totalThinNetPresentValue = 0.0F;
                for (int period = 0; period < highestTrajectory.PlanningPeriods; ++period)
                {
                    // get density and volumes
                    float standingVolumeCubic = gradedVolumeStanding.GetCubicTotal(period); // m³/ha
                    float harvestVolumeCubic = gradedVolumeHarvested.GetCubicTotal(period); // m³/ha
                    float standingVolumeScribner = highestTrajectory.StandingVolume.ScribnerTotal[period]; // MBF/ha
                    float harvestVolumeScribner = highestTrajectory.ThinningVolume.ScribnerTotal[period]; // MBF/ha

                    float basalAreaRemoved = Constant.AcresPerHectare * Constant.MetersPerFoot * Constant.MetersPerFoot * highestTrajectory.BasalAreaRemoved[period]; // m²/acre
                    float basalAreaIntensity = 0.0F;
                    if (period > 0)
                    {
                        basalAreaIntensity = basalAreaRemoved / highestTrajectory.DensityByPeriod[period - 1].BasalAreaPerAcre;
                    }
                    Debug.Assert((harvestVolumeScribner == 0.0F && basalAreaRemoved == 0.0F) || (harvestVolumeScribner > 0.0F && basalAreaRemoved > 0.0F));

                    float treesPerAcreDecrease = 0.0F;
                    if (period > 0)
                    {
                        OrganonStandDensity previousDensity = highestTrajectory.DensityByPeriod[period - 1];
                        OrganonStandDensity currentDensity = highestTrajectory.DensityByPeriod[period];
                        if ((currentDensity == null) || (previousDensity == null))
                        {
                            throw new ParameterOutOfRangeException(null, "Stand density information is missing. Did the heuristic perform at least one fully simulated move?");
                        }
                        treesPerAcreDecrease = 1.0F - currentDensity.TreesPerAcre / previousDensity.TreesPerAcre;
                    }

                    OrganonStand stand = highestTrajectory.StandByPeriod[period] ?? throw new NotSupportedException("Stand information missing for period " + period + ".");
                    OrganonStandDensity density = highestTrajectory.DensityByPeriod[period];
                    float quadraticMeanDiameterInCm = stand.GetQuadraticMeanDiameterInCentimeters(); // 1/(10 in * 2.54 cm/in) = 0.03937008
                    float topHeightInM = stand.GetTopHeightInMeters();
                    float reinekeStandDensityIndex = Constant.AcresPerHectare * density.TreesPerAcre * MathF.Pow(0.03937008F * quadraticMeanDiameterInCm, Constant.ReinekeExponent);

                    float treesPerHectare = Constant.AcresPerHectare * density.TreesPerAcre;
                    float basalAreaPerHectare = Constant.AcresPerHectare * Constant.MetersPerFoot * Constant.MetersPerFoot * density.BasalAreaPerAcre;
                    float treesPerHectareDecrease = Constant.AcresPerHectare * treesPerAcreDecrease;

                    // NPV and LEV
                    float thinNetPresentValue = highestTrajectory.ThinningVolume.NetPresentValue[period];
                    totalThinNetPresentValue += thinNetPresentValue;
                    float standingNetPresentValue = highestTrajectory.StandingVolume.NetPresentValue[period];
                    float reforestationNetPresentValue = highestTrajectory.TimberValue.GetNetPresentReforestationValue(highestTrajectory.PlantingDensityInTreesPerHectare);
                    float periodNetPresentValue = totalThinNetPresentValue + standingNetPresentValue + reforestationNetPresentValue;

                    float presentToFutureConversionFactor = MathF.Pow(1.0F + highestTrajectory.TimberValue.DiscountRate, highestTrajectory.GetRotationLength());
                    float landExpectationValue = presentToFutureConversionFactor * periodNetPresentValue / (presentToFutureConversionFactor - 1.0F);

                    // pond NPV by grade
                    float netPresentValue2Saw = gradedVolumeStanding.NetPresentValue2Saw[period] + gradedVolumeHarvested.NetPresentValue2Saw[period];
                    float netPresentValue3Saw = gradedVolumeStanding.NetPresentValue3Saw[period] + gradedVolumeHarvested.NetPresentValue3Saw[period];
                    float netPresentValue4Saw = gradedVolumeStanding.NetPresentValue4Saw[period] + gradedVolumeHarvested.NetPresentValue4Saw[period];

                    // biomass
                    float liveBiomass = 0.001F * stand.GetLiveBiomass(); // Mg/ha

                    writer.WriteLine(linePrefix + "," +
                                     stand.AgeInYears.ToString(CultureInfo.InvariantCulture) + "," +
                                     treesPerHectare.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                     quadraticMeanDiameterInCm.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                     topHeightInM.ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                     basalAreaPerHectare.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                     reinekeStandDensityIndex.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                     snagsAndLogs.SnagsPerHectareByPeriod[period].ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                     snagsAndLogs.SnagQmdInCentimetersByPeriod[period].ToString("0.00", CultureInfo.InvariantCulture) + "," +
                                     standingVolumeCubic.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     harvestVolumeCubic.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     standingVolumeScribner.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     harvestVolumeScribner.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     basalAreaRemoved.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                     basalAreaIntensity.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     treesPerHectareDecrease.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     periodNetPresentValue.ToString("0", CultureInfo.InvariantCulture) + "," +
                                     landExpectationValue.ToString("0", CultureInfo.InvariantCulture) + "," +
                                     gradedVolumeStanding.Cubic2Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     gradedVolumeStanding.Cubic3Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     gradedVolumeStanding.Cubic4Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     gradedVolumeHarvested.Cubic2Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     gradedVolumeHarvested.Cubic3Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     gradedVolumeHarvested.Cubic4Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     gradedVolumeStanding.Scribner2Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     gradedVolumeStanding.Scribner3Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     gradedVolumeStanding.Scribner4Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     gradedVolumeHarvested.Scribner2Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     gradedVolumeHarvested.Scribner3Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     gradedVolumeHarvested.Scribner4Saw[period].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     netPresentValue2Saw.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     netPresentValue3Saw.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     netPresentValue4Saw.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                     liveBiomass.ToString("0.00", CultureInfo.InvariantCulture));
                }
            }
        }

        protected void ValidateParameters()
        {
            if ((this.Results == null) && (this.Trajectories == null))
            {
                throw new ParameterOutOfRangeException(nameof(this.Trajectories), "Niether of " + nameof(this.Results) + " or " + nameof(this.Trajectories) + " are specified. Specify one or the other.");
            }
            if ((this.Results != null) && (this.Trajectories != null))
            {
                throw new ParameterOutOfRangeException(nameof(this.Trajectories), "Both " + nameof(this.Results) + " and " + nameof(this.Trajectories) + " are specified. Specify one or the other.");
            }
            if ((this.Results != null) && (this.Results.Count < 1))
            {
                throw new ParameterOutOfRangeException(nameof(this.Results), nameof(this.Results) + " is empty. At least one run must be present.");
            }
            if ((this.Trajectories != null) && (this.Trajectories.Count < 1))
            {
                throw new ParameterOutOfRangeException(nameof(this.Trajectories), nameof(this.Trajectories) + " is empty. At least one run must be present.");
            }
        }
    }
}
