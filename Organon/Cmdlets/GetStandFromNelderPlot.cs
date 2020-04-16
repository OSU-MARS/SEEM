using Osu.Cof.Ferm.Organon;
using Osu.Cof.Ferm.Data;
using System;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "StandFromNelderPlot")]
    public class GetStandFromNelderPlot : Cmdlet
    {
        [Parameter()]
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

        public GetStandFromNelderPlot()
        {
            this.SiteIndex = 130.0F;
            this.Trees = null;
            this.XlsxSheet = "1";
        }

        protected override void ProcessRecord()
        {
            NelderPlot plot = new NelderPlot(this.Xlsx, this.XlsxSheet);
            OrganonStand stand;
            if (this.Trees.HasValue)
            {
                stand = plot.ToStand(this.SiteIndex, this.Trees.Value);
            }
            else
            {
                stand = plot.ToStand(this.SiteIndex);
            }
            this.WriteObject(stand);
        }
    }
}
