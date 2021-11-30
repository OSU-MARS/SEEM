using Mars.Seem.Heuristics;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    public class WriteTrajectoriesCmdlet : WriteCmdlet
    {
        [Parameter(HelpMessage = "Include columns with heuristic parameter values (for heuristics with parameters) in output file.")]
        public SwitchParameter HeuristicParameters { get; set; }

        [Parameter]
        [ValidateNotNull]
        public StandTrajectories? Trajectories { get; set; }

        public WriteTrajectoriesCmdlet()
        {
            this.HeuristicParameters = false;
        }

        protected string GetCsvHeaderForCoordinate()
        {
            string? maybeHeuristicParametersWithTrailingComma = null;
            if (this.HeuristicParameters && (this.Trajectories is HeuristicStandTrajectories heuristicTrajectories))
            {
                HeuristicParameters? firstHeuristicParameters = heuristicTrajectories.GetParameters(Constant.HeuristicDefault.CoordinateIndex);
                if (firstHeuristicParameters != null)
                {
                    maybeHeuristicParametersWithTrailingComma = firstHeuristicParameters.GetCsvHeader() + ",";
                }
            }
            return "stand," + maybeHeuristicParametersWithTrailingComma + "thin1,thin2,thin3,rotation,financialScenario";
        }

        [MemberNotNull(nameof(WriteTrajectoriesCmdlet.Trajectories))]
        protected string GetCsvPrefixForCoordinate(StandTrajectoryCoordinate coordinate)
        {
            Debug.Assert(this.Trajectories != null);
            StandTrajectory? highTrajectory = this.Trajectories[coordinate].Pool.High.Trajectory;
            if (highTrajectory == null)
            {
                throw new NotSupportedException("Prescription pool is missing a high trajectory.");
            }

            int firstThinPeriod = this.Trajectories.FirstThinPeriods[coordinate.FirstThinPeriodIndex];
            int secondThinPeriod = this.Trajectories.SecondThinPeriods[coordinate.SecondThinPeriodIndex];
            int thirdThinPeriod = this.Trajectories.ThirdThinPeriods[coordinate.ThirdThinPeriodIndex];
            int endOfRotationPeriod = this.Trajectories.RotationLengths[coordinate.RotationIndex];

            string? maybeHeuristicParametersWithTrailingComma = null;
            if (this.HeuristicParameters && (this.Trajectories is HeuristicStandTrajectories heuristicTrajectories))
            {
                maybeHeuristicParametersWithTrailingComma = heuristicTrajectories.GetParameters(coordinate.ParameterIndex).GetCsvValues() + ",";
            }
            string? firstThinAge = firstThinPeriod != Constant.NoThinPeriod ? highTrajectory.GetEndOfPeriodAge(firstThinPeriod).ToString(CultureInfo.InvariantCulture) : null;
            string? secondThinAge = secondThinPeriod != Constant.NoThinPeriod ? highTrajectory.GetEndOfPeriodAge(secondThinPeriod).ToString(CultureInfo.InvariantCulture) : null;
            string? thirdThinAge = thirdThinPeriod != Constant.NoThinPeriod ? highTrajectory.GetEndOfPeriodAge(thirdThinPeriod).ToString(CultureInfo.InvariantCulture) : null;
            string rotationLength = highTrajectory.GetEndOfPeriodAge(endOfRotationPeriod).ToString(CultureInfo.InvariantCulture);
            string financialScenario = this.Trajectories.FinancialScenarios.Name[coordinate.FinancialIndex];

            return highTrajectory.Name + "," +
                   maybeHeuristicParametersWithTrailingComma +
                   firstThinAge + "," +
                   secondThinAge + "," +
                   thirdThinAge + "," +
                   rotationLength + "," +
                   financialScenario;
        }

        [MemberNotNull(nameof(WriteTrajectoriesCmdlet.Trajectories))]
        protected StandTrajectory GetHighTrajectoryAndPositionPrefix(int evaluatedCoordinateIndex, out string linePrefix)
        {
            Debug.Assert(this.Trajectories != null);
            StandTrajectoryCoordinate coordinate = this.Trajectories.CoordinatesEvaluated[evaluatedCoordinateIndex];
            linePrefix = this.GetCsvPrefixForCoordinate(coordinate);

            SilviculturalPrescriptionPool prescriptions = this.Trajectories[coordinate].Pool;
            StandTrajectory? highTrajectory = prescriptions.High.Trajectory;
            if (highTrajectory == null)
            {
                throw new NotSupportedException("Precription pool at position " + evaluatedCoordinateIndex + " is missing a high trajectory.");
            }

            return highTrajectory;
        }

        protected StandTrajectory GetHighTrajectoryAndPositionPrefix(int evaluatedCoordinateIndex, out string linePrefix, out int endOfRotationPeriod, out int financialIndex)
        {
            StandTrajectory highTrajectory = this.GetHighTrajectoryAndPositionPrefix(evaluatedCoordinateIndex, out linePrefix);

            StandTrajectoryCoordinate coordinate = this.Trajectories.CoordinatesEvaluated[evaluatedCoordinateIndex];
            endOfRotationPeriod = this.Trajectories.RotationLengths[coordinate.RotationIndex];
            financialIndex = coordinate.FinancialIndex;

            return highTrajectory;
        }

        protected int GetMaxCoordinateIndex()
        {
            Debug.Assert(this.Trajectories != null);
            return this.Trajectories.CoordinatesEvaluated.Count;
        }

        [MemberNotNull(nameof(WriteTrajectoriesCmdlet.Trajectories))]
        protected void ValidateParameters()
        {
            if (this.Trajectories == null)
            {
                throw new ParameterOutOfRangeException(nameof(this.Trajectories));
            }
            if (this.Trajectories.CoordinatesEvaluated.Count < 1)
            {
                throw new ParameterOutOfRangeException(nameof(this.Trajectories), "-" + nameof(this.Trajectories) + " is empty. At least one run must be present.");
            }
        }
    }
}
