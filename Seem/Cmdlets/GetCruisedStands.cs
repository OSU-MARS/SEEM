using Mars.Seem.Data;
using System.Diagnostics;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "CruisedStands")]
    public class GetCruisedStands : GetStandCmdlet
    {
        [Parameter(HelpMessage = "Sheet (tab) within spreadsheet to load stands from.")]
        [ValidateNotNullOrEmpty]
        public string StandsSheet { get; set; }

        public GetCruisedStands()
        {
            this.StandsSheet = "stands";
        }

        protected override void ProcessRecord()
        {
            Debug.Assert(this.Xlsx != null);

            // read all stands and trees defined in spreadsheet
            CruisedStands cruiseRecords = new(this.Model);
            cruiseRecords.Read(this.Xlsx, this.StandsSheet, this.TreesSheet);

            this.WriteObject(cruiseRecords);
        }
    }
}
