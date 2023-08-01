using Mars.Seem.Tree;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    public class GetStandCmdlet : Cmdlet
    {
        [Parameter]
        public TreeModel Model { get; set; }

        [Parameter(HelpMessage = "Sheet (tab) within spreadsheet to load measure trees from.")]
        [ValidateNotNullOrEmpty]
        public string TreesSheet { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Spreadsheet to load measure trees and, if applicable, stand definitions from.")]
        [ValidateNotNullOrEmpty]
        public string? Xlsx { get; set; }

        public GetStandCmdlet() 
        {
            this.Model = TreeModel.OrganonNwo;
            this.TreesSheet = "trees";
            this.Xlsx = null;
        }
    }
}
