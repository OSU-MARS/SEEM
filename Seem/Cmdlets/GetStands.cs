using Mars.Seem.Data;
using System;
using System.IO;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "Stands")]
    public class GetStands : GetStandsCmdlet
    {
        [Parameter(HelpMessage = $"Sheet (tab) within spreadsheet to load stands from. Default is \"stands\". Ignored if -{nameof(GetStands.File)} does not indicate a spreadsheet.")]
        [ValidateNotNullOrWhiteSpace]
        public string StandsSheet { get; set; }

        [Parameter(HelpMessage = $"File to load stands from if -{nameof(GetStands.File)} is not a spreadsheet.")]
        [ValidateNotNullOrWhiteSpace]
        public string StandsFile { get; set; }

        public GetStands()
        {
            this.StandsFile = String.Empty;
            this.StandsSheet = "stands";
        }

        protected override void ProcessRecord()
        {
            StandList stands = new(this.Model);

            // read all stands and trees defined in spreadsheet
            string extension = Path.GetExtension(this.File);
            switch (extension)
            {
                case Constant.File.FeatherExtension:
                    stands.ReadTreesFromArrowAndStandsFromSpreadsheet(this.File, this.StandsFile, this.StandsSheet);
                    break;
                case Constant.File.XlsxExtension:
                    stands.ReadTreesAndStandsFromSpreadsheet(this.File, this.StandsSheet, this.TreesSheet);
                    break;
                default:
                    throw new NotSupportedException($"File extension {extension} is not supported.");
            }

            this.WriteObject(stands);
        }
    }
}
