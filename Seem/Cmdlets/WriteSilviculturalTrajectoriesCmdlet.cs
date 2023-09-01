using Mars.Seem.Heuristics;
using Mars.Seem.Silviculture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    public class WriteSilviculturalTrajectoriesCmdlet : WriteCmdlet
    {
        [Parameter(HelpMessage = "Include columns with heuristic parameter values (for heuristics with parameters) in output file.")]
        public SwitchParameter HeuristicParameters { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public List<SilviculturalSpace>? Trajectories { get; set; }

        public WriteSilviculturalTrajectoriesCmdlet()
        {
            this.HeuristicParameters = false;
            this.Trajectories = null;
        }

        protected override string GetCsvHeaderForSilviculturalCoordinate()
        {
            string? maybeHeuristicParametersWithTrailingComma = null;
            if (this.HeuristicParameters)
            {
                Debug.Assert((this.Trajectories != null) && (this.Trajectories.Count > 0));
                if (this.Trajectories[0] is HeuristicStandTrajectories heuristicTrajectories)
                {
                    if (this.Trajectories.Count > 1)
                    {
                        throw new NotSupportedException("Writing heuristic parameters is not supported for multiple runs. Turn off -" + nameof(this.HeuristicParameters) + " or write each element of -" + nameof(this.Trajectories) + " individually");
                    }

                    HeuristicParameters? firstHeuristicParameters = heuristicTrajectories.GetParameters(Constant.HeuristicDefault.CoordinateIndex);
                    if (firstHeuristicParameters != null)
                    {
                        maybeHeuristicParametersWithTrailingComma = firstHeuristicParameters.GetCsvHeader() + ",";
                    }
                }
            }
            return "stand," + maybeHeuristicParametersWithTrailingComma + "thin1,thin2,thin3,rotation,financialScenario";
        }

        protected static int GetMaxCoordinateIndex(SilviculturalSpace silviculturalSpace)
        {
            Debug.Assert(silviculturalSpace != null);
            return silviculturalSpace.CoordinatesEvaluated.Count;
        }

        [MemberNotNull(nameof(WriteSilviculturalTrajectoriesCmdlet.Trajectories))]
        protected void ValidateParameters()
        {
            Debug.Assert((this.Trajectories != null) && (this.Trajectories.Count > 0));

            for (int trajectoryIndex = 0; trajectoryIndex < this.Trajectories.Count; ++trajectoryIndex)
            {
                SilviculturalSpace silviculturalSpace = this.Trajectories[trajectoryIndex];
                if (silviculturalSpace.CoordinatesEvaluated.Count < 1)
                {
                    throw new ParameterOutOfRangeException(nameof(this.Trajectories), "-" + nameof(this.Trajectories) + " contains an empty set of stand trajectories. At least one run must be present in each set of results.");
                }
            }
        }
    }
}
