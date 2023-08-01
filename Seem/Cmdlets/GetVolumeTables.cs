using Mars.Seem.Tree;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "VolumeTables")]
    public class GetVolumeTables : Cmdlet
    {
        protected override void ProcessRecord() 
        {
            //TreeVolume volumeTables = new(finalHarvestRange, thinningRange);
            //this.WriteObject(volumeTables);
        }
    }
}
