using Mars.Seem.Tree;
using System;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    public class GetStandsCmdlet : Cmdlet
    {
        [Parameter]
        public TreeModel Model { get; set; }

        [Parameter(HelpMessage = $"Sheet (tab) within spreadsheet to load measure trees from. Default is \"trees\". Ignored if -{nameof(GetStandsCmdlet.File)} does not indicate a spreadsheet.")]
        [ValidateNotNullOrWhiteSpace]
        public string TreesSheet { get; set; }

        [Parameter(Mandatory = true, HelpMessage = $"Spreadsheet (.xlsx) or Arrow flat file (.feather) containing the individual tree list for the stands to be loaded. If a spreadsheet, can contain other sheets besides the tree list indicated by -{nameof(GetStandsCmdlet.TreesSheet)} with stand parameters or other information.")]
        [ValidateNotNullOrWhiteSpace]
        public string File { get; set; }

        public GetStandsCmdlet() 
        {
            this.File = String.Empty;
            this.Model = TreeModel.OrganonNwo;
            this.TreesSheet = "trees";
        }
    }
}
