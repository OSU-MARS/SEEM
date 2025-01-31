using Mars.Seem.Extensions;
using Mars.Seem.Tree;
using System;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    public class GetTrajectoryCmdlet : Cmdlet
    {
        [Parameter]
        public SimdInstructions Simd { get; set; }

        [Parameter]
        [ValidateRange(1, 128)]
        public int Threads { get; set; }

        [Parameter]
        [ValidateNotNull]
        public TreeScaling TreeVolume { get; set; }

        protected GetTrajectoryCmdlet()
        {
            this.Simd = SimdInstructionsExtensions.GetDefault();
            this.Threads = Environment.ProcessorCount / 2;
            this.TreeVolume = TreeScaling.Default;
        }
    }
}
