using Osu.Cof.Ferm.Organon;
using Osu.Cof.Ferm.Data;
using System;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "StandFromPlot")]
    public class GetStandFromPlot : Cmdlet
    {
        [Parameter]
        public TreeModel Model { get; set; }

        [Parameter]
        [ValidateRange(0.0F, Constant.Maximum.SiteIndexInFeet)]
        public float SiteIndex { get; set; }

        [Parameter]
        [ValidateRange(0, Int32.MaxValue)]
        public Nullable<int> Trees { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Xlsx { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        public string XlsxSheet { get; set; }

        public GetStandFromPlot()
        {
            this.Model = TreeModel.OrganonNwo;
            this.SiteIndex = 130.0F;
            this.Trees = null;
            this.XlsxSheet = "1";
        }

        protected override void ProcessRecord()
        {
            PlotWithHeight plot = new PlotWithHeight();
            plot.Read(this.Xlsx, this.XlsxSheet);

            OrganonConfiguration configuration = new OrganonConfiguration(OrganonVariant.Create(this.Model));
            OrganonStand stand;
            if (this.Trees.HasValue)
            {
                stand = plot.ToOrganonStand(configuration, this.SiteIndex, this.Trees.Value);
            }
            else
            {
                stand = plot.ToOrganonStand(configuration, this.SiteIndex);
            }
            this.WriteObject(stand);
        }
    }
}
