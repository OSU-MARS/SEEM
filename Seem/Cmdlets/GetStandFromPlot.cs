using Osu.Cof.Ferm.Organon;
using Osu.Cof.Ferm.Data;
using Osu.Cof.Ferm.Tree;
using System;
using System.Management.Automation;
using System.Collections.Generic;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "StandFromPlot")]
    public class GetStandFromPlot : Cmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateRange(0, 1000)]
        public int Age { get; set; }

        [Parameter]
        [ValidateRange(0.1F, Constant.Maximum.ExpansionFactorPerHa)]
        public float? ExpansionFactorPerHa { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 1000.0F)]
        public float ForwardingTethered { get; set; }
        [Parameter]
        [ValidateRange(0.0F, 2000.0F)]
        public float ForwardingUntethered { get; set; }
        [Parameter]
        [ValidateRange(0.0F, 2500.0F)]
        public float ForwardingRoad { get; set; }

        [Parameter]
        public TreeModel Model { get; set; }

        [Parameter]
        [ValidateRange(1.0F, Constant.Maximum.PlantingDensityInTreesPerHectare)]
        public float? PlantingDensityPerHa { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public List<int>? Plots { get; set; }

        [Parameter]
        [ValidateRange(0.0F, 200.0F)]
        public float SlopeInPercent { get; set; }

        [Parameter]
        [ValidateRange(1.0F, Constant.Maximum.SiteIndexInM)]
        public float SiteIndexInM { get; set; }

        [Parameter]
        [ValidateRange(1, Int32.MaxValue)]
        public int? Trees { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string? Xlsx { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        public string XlsxSheet { get; set; }

        public GetStandFromPlot()
        {
            this.ExpansionFactorPerHa = null;
            this.ForwardingTethered = Constant.HarvestCost.DefaultForwardingDistanceInStandTethered;
            this.ForwardingUntethered = Constant.HarvestCost.DefaultForwardingDistanceInStandUntethered;
            this.ForwardingRoad = Constant.HarvestCost.DefaultForwardingDistanceOnRoad;
            this.Model = TreeModel.OrganonNwo;
            this.SlopeInPercent = Constant.HarvestCost.DefaultSlopeInPercent;
            this.SiteIndexInM = 130.0F;
            this.Trees = null;
            this.Xlsx = null;
            this.XlsxSheet = "1";
        }

        protected override void ProcessRecord()
        {
            PlotsWithHeight plot;
            if (this.ExpansionFactorPerHa.HasValue)
            {
                plot = new PlotsWithHeight(this.Plots!, this.ExpansionFactorPerHa.Value);
            }
            else
            {
                plot = new PlotsWithHeight(this.Plots!);
            }
            plot.Read(this.Xlsx!, this.XlsxSheet);

            OrganonConfiguration configuration = new(OrganonVariant.Create(this.Model));
            OrganonStand stand;
            if (this.Trees.HasValue)
            {
                stand = plot.ToOrganonStand(configuration, this.Age, this.SiteIndexInM, this.Trees.Value);
            }
            else
            {
                stand = plot.ToOrganonStand(configuration, this.Age, this.SiteIndexInM);
            }

            stand.SetCorridorLength(this.ForwardingTethered, this.ForwardingUntethered);
            stand.ForwardingDistanceOnRoad = this.ForwardingRoad;
            if (this.PlantingDensityPerHa.HasValue)
            {
                stand.PlantingDensityInTreesPerHectare = this.PlantingDensityPerHa.Value;
            }
            stand.SlopeInPercent = this.SlopeInPercent;

            this.WriteObject(stand);
        }
    }
}
