using Mars.Seem.Organon;
using Mars.Seem.Data;
using Mars.Seem.Tree;
using System;
using System.Management.Automation;
using System.Collections.Generic;
using Mars.Seem.Silviculture;
using Mars.Seem.Optimization;
using Mars.Seem.Extensions;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "StandsFromPlots")]
    public class GetStandsFromPlots : Cmdlet
    {
        [Parameter(HelpMessage = "Stand age in years, if even aged.")]
        [ValidateRange(0, 10000)]
        public int[] Ages { get; set; } // PowerShell 7 can't translate from an int[] declared in PowerShell to IList<int>

        [Parameter(Mandatory = true, HelpMessage = "Stand area in hectares.")]
        [ValidateRange(0, 1000)]
        public float Area { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        public string[] ExcludeSpecies { get; set; }
        [Parameter(HelpMessage = "Expansion factor in units of trees per hectare.")]
        [ValidateRange(0.1F, Constant.Maximum.ExpansionFactorPerHa)]
        public float[]? ExpansionFactorPerHa { get; set; }

        [Parameter(HelpMessage = "Financial scenario to use in calculating value of a measured stand trajectory. Not used when a single stand is returned.")]
        [ValidateNotNull]
        public FinancialScenarios Financial { get; set; }
        [Parameter(HelpMessage = "Mean tethered forwarding distance in stand, in meters.")]
        [ValidateRange(0.0F, 1000.0F)]
        public float ForwardingTethered { get; set; }
        [Parameter(HelpMessage = "Mean untethered forwarding distance in stand, in meters.")]
        [ValidateRange(0.0F, 2000.0F)]
        public float ForwardingUntethered { get; set; }
        [Parameter(HelpMessage = "Mean forwarding distance on road between top of corridor and unloading point (roadside, landing, or hot load into mule train), in meters.")]
        [ValidateRange(0.0F, 2500.0F)]
        public float ForwardingRoad { get; set; }

        [Parameter]
        [ValidateRange(1.37F, Constant.Maximum.SiteIndexInM)]
        public float HemlockSiteIndexInM { get; set; }
        [Parameter]
        public ImputationMethod Imputation { get; set; }
        [Parameter]
        public SwitchParameter IncludeSpacingAndReplicateInTag { get; set; }

        [Parameter]
        public TreeModel Model { get; set; }

        [Parameter(HelpMessage = "Replanting density after regeneration harvest in seedlings per hectare.")]
        [ValidateRange(1.0F, Constant.Maximum.PlantingDensityInTreesPerHectare)]
        public float? PlantingDensityPerHa { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public List<int>? Plots { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 200.0F)]
        public float SlopeInPercent { get; set; }

        [Parameter]
        [ValidateRange(1.37F, Constant.Maximum.SiteIndexInM)]
        public float SiteIndexInM { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int Trees { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string? Xlsx { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        public string XlsxSheet { get; set; }

        public GetStandsFromPlots()
        {
            this.Ages = Array.Empty<int>();
            this.Area = Constant.HarvestCost.DefaultHarvestUnitSizeInHa;
            this.ExcludeSpecies = Array.Empty<string>();
            this.ExpansionFactorPerHa = null;
            this.Financial = FinancialScenarios.Default;
            this.ForwardingTethered = Constant.HarvestCost.DefaultForwardingDistanceInStandTethered;
            this.ForwardingUntethered = Constant.HarvestCost.DefaultForwardingDistanceInStandUntethered;
            this.ForwardingRoad = Constant.HarvestCost.DefaultForwardingDistanceOnRoad;
            this.HemlockSiteIndexInM = Constant.Default.WesternHemlockSiteIndexInM;
            this.Imputation = ImputationMethod.None;
            this.IncludeSpacingAndReplicateInTag = false;
            this.Model = TreeModel.OrganonNwo;
            this.SlopeInPercent = Constant.HarvestCost.DefaultSlopeInPercent;
            this.SiteIndexInM = Constant.Default.DouglasFirSiteIndexInM; 
            this.Trees = Int32.MaxValue;
            this.Xlsx = null;
            this.XlsxSheet = "1";
        }

        private Stand CreateStand(PlotsWithHeight plot, OrganonConfiguration configuration, int age)
        {
            OrganonStand stand = plot.ToOrganonStand(configuration, age, this.SiteIndexInM, this.HemlockSiteIndexInM, this.Trees, this.Imputation);

            stand.AreaInHa = this.Area;
            stand.SetCorridorLength(this.ForwardingTethered, this.ForwardingUntethered);
            stand.ForwardingDistanceOnRoad = this.ForwardingRoad;
            if (this.PlantingDensityPerHa.HasValue)
            {
                stand.PlantingDensityInTreesPerHectare = this.PlantingDensityPerHa.Value;
            }
            stand.SlopeInPercent = this.SlopeInPercent;

            return stand;
        }

        protected override void ProcessRecord()
        {
            // read plot data
            PlotsWithHeight plot;
            if (this.ExpansionFactorPerHa != null)
            {
                if (this.ExpansionFactorPerHa.Length == 1)
                {
                    plot = new PlotsWithHeight(this.Plots!, this.ExpansionFactorPerHa[0]);
                }
                else
                {
                    if (this.Ages.Length != this.ExpansionFactorPerHa.Length)
                    {
                        throw new ParameterOutOfRangeException(nameof(this.Ages) + " or " + nameof(this.ExpansionFactorPerHa));
                    }

                    SortedList<int, float> expansionFactorByAge = new();
                    for (int ageIndex = 0; ageIndex < this.Ages.Length; ++ageIndex)
                    {
                        expansionFactorByAge.Add(this.Ages[ageIndex], this.ExpansionFactorPerHa[ageIndex]);
                    }

                    plot = new PlotsWithHeight(this.Plots!, this.ExpansionFactorPerHa[0])
                    {
                        ExpansionFactorPerHaByAge = expansionFactorByAge
                    };
                }
            }
            else
            {
                plot = new PlotsWithHeight(this.Plots!);
            }

            if (this.ExcludeSpecies.Length > 0)
            {
                plot.ExcludeSpecies = new FiaCode[this.ExcludeSpecies.Length];
                for (int index = 0; index < this.ExcludeSpecies.Length; ++index)
                {
                    plot.ExcludeSpecies[index] = FiaCodeExtensions.Parse(this.ExcludeSpecies[index]);
                }
            }
            plot.IncludeSpacingAndReplicateInTag = this.IncludeSpacingAndReplicateInTag;
            plot.Read(this.Xlsx!, this.XlsxSheet);

            IList<int> ages = this.Ages;
            if (ages.Count == 0)
            {
                ages = plot.Ages;
                if (ages.Count == 0)
                {
                    throw new ParameterOutOfRangeException("Plots", "Plots have no measurement ages.");
                }
            }

            // create stands
            OrganonConfiguration configuration = new(OrganonVariant.Create(this.Model));
            Stand initialStand = this.CreateStand(plot, configuration, ages[0]);
            if (ages.Count == 1)
            {
                this.WriteObject(initialStand);
                return;
            }

            if (this.Financial.Count != 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.Financial));
            }

            MeasuredStandTrajectory trajectory = new(initialStand, TreeVolume.Default, ages);
            for (int ageIndex = 1; ageIndex < ages.Count; ++ageIndex)
            {
                Stand stand = this.CreateStand(plot, configuration, ages[ageIndex]);
                trajectory.DensityByPeriod[ageIndex] = new StandDensity(stand);
                trajectory.StandByPeriod[ageIndex] = stand;
            }

            int planningPeriods = ages.Count - 1;
            float landExpectationValue = this.Financial.GetLandExpectationValue(trajectory, Constant.HeuristicDefault.CoordinateIndex, planningPeriods);

            StandTrajectories trajectories = new(new List<int>() { Constant.NoThinPeriod }, new List<int>() { planningPeriods }, this.Financial);
            StandTrajectoryCoordinate currentCoordinate = new();
            trajectories.AssimilateIntoCoordinate(trajectory, landExpectationValue, currentCoordinate, new PrescriptionPerformanceCounters());
            trajectories.AddEvaluatedPosition(currentCoordinate);
            this.WriteObject(trajectories);
        }
    }
}
