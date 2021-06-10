using Osu.Cof.Ferm.Heuristics;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    public class WriteHeuristicResultsCmdlet : WriteCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public HeuristicResults? Results { get; set; }
    }
}
