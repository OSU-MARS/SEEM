using Mars.Seem.Heuristics;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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

        protected string GetCsvHeaderForSilviculturalCoordinate()
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

        protected string GetCsvPrefixForCoordinate(SilviculturalSpace silviculturalSpace, SilviculturalCoordinate coordinate)
        {
            StandTrajectory highTrajectory = silviculturalSpace.GetHighTrajectory(coordinate);
            int firstThinPeriod = silviculturalSpace.FirstThinPeriods[coordinate.FirstThinPeriodIndex];
            int secondThinPeriod = silviculturalSpace.SecondThinPeriods[coordinate.SecondThinPeriodIndex];
            int thirdThinPeriod = silviculturalSpace.ThirdThinPeriods[coordinate.ThirdThinPeriodIndex];
            int endOfRotationPeriod = silviculturalSpace.RotationLengths[coordinate.RotationIndex];

            string? maybeHeuristicParametersWithTrailingComma = null;
            if (this.HeuristicParameters && (silviculturalSpace is HeuristicStandTrajectories heuristicTrajectories))
            {
                maybeHeuristicParametersWithTrailingComma = heuristicTrajectories.GetParameters(coordinate.ParameterIndex).GetCsvValues() + ",";
            }
            string? firstThinAge = firstThinPeriod != Constant.NoHarvestPeriod ? highTrajectory.GetEndOfPeriodAge(firstThinPeriod).ToString(CultureInfo.InvariantCulture) : null;
            string? secondThinAge = secondThinPeriod != Constant.NoHarvestPeriod ? highTrajectory.GetEndOfPeriodAge(secondThinPeriod).ToString(CultureInfo.InvariantCulture) : null;
            string? thirdThinAge = thirdThinPeriod != Constant.NoHarvestPeriod ? highTrajectory.GetEndOfPeriodAge(thirdThinPeriod).ToString(CultureInfo.InvariantCulture) : null;
            string rotationLength = highTrajectory.GetEndOfPeriodAge(endOfRotationPeriod).ToString(CultureInfo.InvariantCulture);
            string financialScenario = silviculturalSpace.FinancialScenarios.Name[coordinate.FinancialIndex];

            return highTrajectory.Name + "," +
                   maybeHeuristicParametersWithTrailingComma +
                   firstThinAge + "," +
                   secondThinAge + "," +
                   thirdThinAge + "," +
                   rotationLength + "," +
                   financialScenario;
        }

        protected StandTrajectory GetHighTrajectoryAndPositionPrefix(SilviculturalSpace silviculturalSpace, int evaluatedCoordinateIndex, out string linePrefix)
        {
            Debug.Assert(silviculturalSpace != null);
            SilviculturalCoordinate coordinate = silviculturalSpace.CoordinatesEvaluated[evaluatedCoordinateIndex];
            linePrefix = this.GetCsvPrefixForCoordinate(silviculturalSpace, coordinate);
            return silviculturalSpace.GetHighTrajectory(coordinate);
        }

        protected StandTrajectory GetHighTrajectoryAndPositionPrefix(SilviculturalSpace silviculturalSpace, int evaluatedCoordinateIndex, out string linePrefix, out int endOfRotationPeriodIndex, out int financialIndex)
        {
            SilviculturalCoordinate coordinate = silviculturalSpace.CoordinatesEvaluated[evaluatedCoordinateIndex];
            linePrefix = this.GetCsvPrefixForCoordinate(silviculturalSpace, coordinate);
            endOfRotationPeriodIndex = coordinate.RotationIndex;
            financialIndex = coordinate.FinancialIndex;
            return silviculturalSpace.GetHighTrajectory(coordinate);
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
