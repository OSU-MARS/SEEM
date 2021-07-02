using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "FinancialScenarios")]
    public class GetFinancialScenarios : Cmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string? Xlsx { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        public string XlsxSheet { get; set; }

        public GetFinancialScenarios()
        {
            this.Xlsx = null;
            this.XlsxSheet = "Sheet1";
        }

        protected override void ProcessRecord()
        {
            FinancialScenarios financialScenarios = new();
            financialScenarios.Read(this.Xlsx!, this.XlsxSheet);

            this.WriteObject(financialScenarios);
        }
    }
}
