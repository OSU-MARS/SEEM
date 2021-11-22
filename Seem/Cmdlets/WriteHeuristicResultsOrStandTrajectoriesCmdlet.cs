using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Management.Automation;
using System.Text;

namespace Osu.Cof.Ferm.Cmdlets
{
    public class WriteHeuristicResultsOrStandTrajectoriesCmdlet : WriteHeuristicResultsCmdlet
    {
        [Parameter]
        [ValidateNotNull]
        public FinancialScenarios Financial { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        public List<OrganonStandTrajectory>? Trajectories { get; set; }

        public WriteHeuristicResultsOrStandTrajectoriesCmdlet()
        {
            this.Financial = FinancialScenarios.Default;
            this.Trajectories = null;
        }

        protected OrganonStandTrajectory GetHighestTrajectoryAndLinePrefix(int positionIndex, out StringBuilder linePrefix, out int endOfRotationPeriod, out int financialIndex)
        {
            OrganonStandTrajectory highTrajectory;
            string financialScenarioName;
            int firstThinAgeAsInteger;
            int secondThinAgeAsInteger;
            int thirdThinAgeAsInteger;
            if (this.Results != null)
            {
                // position index in list of positions evaluated
                HeuristicResultPosition position = this.Results.PositionsEvaluated[positionIndex];
                HeuristicSolutionPool solutionPool = this.Results[position].Pool;
                if (solutionPool.High == null)
                {
                    throw new NotSupportedException("Result at position " + positionIndex + " is missing a high solution.");
                }
                highTrajectory = solutionPool.High.GetBestTrajectoryWithDefaulting(position);
                endOfRotationPeriod = this.Results.RotationLengths[position.RotationIndex];
                financialIndex = position.FinancialIndex;
                financialScenarioName = this.Results.FinancialScenarios.Name[financialIndex];

                // since both a position and trajectory are available in this case, check stand entries for consistency in debug builds
                // If a trajectory's thins don't match the position it's assigned to then the trajectory isn't valid for the position.
                firstThinAgeAsInteger = this.Results.FirstThinPeriods[position.FirstThinPeriodIndex];
                if (firstThinAgeAsInteger != Constant.NoThinPeriod)
                {
                    firstThinAgeAsInteger = highTrajectory.GetEndOfPeriodAge(firstThinAgeAsInteger);
                }
                secondThinAgeAsInteger = this.Results.SecondThinPeriods[position.SecondThinPeriodIndex];
                if (secondThinAgeAsInteger != Constant.NoThinPeriod)
                {
                    secondThinAgeAsInteger = highTrajectory.GetEndOfPeriodAge(secondThinAgeAsInteger);
                }
                thirdThinAgeAsInteger = this.Results.ThirdThinPeriods[position.ThirdThinPeriodIndex];
                if (thirdThinAgeAsInteger != Constant.NoThinPeriod)
                {
                    thirdThinAgeAsInteger = highTrajectory.GetEndOfPeriodAge(thirdThinAgeAsInteger);
                }
                Debug.Assert(highTrajectory.GetFirstThinAge() == firstThinAgeAsInteger);
                Debug.Assert(highTrajectory.GetSecondThinAge() == secondThinAgeAsInteger);
                Debug.Assert(highTrajectory.GetThirdThinAge() == thirdThinAgeAsInteger);
                Debug.Assert(highTrajectory.PlanningPeriods > endOfRotationPeriod); // trajectory periods beyond the end of rotation indicate simulation reuse but a trajectory should extend to at least the rotation length
            }
            else
            {
                // positionIndex = trajectoryIndex * this.FinancialCount + financialIndex
                Debug.Assert((this.Financial.Count > 0) && (this.Trajectories != null) && (this.Trajectories.Count > 0));
                int trajectoryIndex = positionIndex / this.Financial.Count;
                financialIndex = positionIndex - trajectoryIndex * this.Financial.Count;

                highTrajectory = this.Trajectories![trajectoryIndex];
                endOfRotationPeriod = highTrajectory.PlanningPeriods - 1;
                financialScenarioName = this.Financial.Name[financialIndex];
                firstThinAgeAsInteger = highTrajectory.GetFirstThinAge();
                secondThinAgeAsInteger = highTrajectory.GetSecondThinAge();
                thirdThinAgeAsInteger = highTrajectory.GetThirdThinAge();
            }

            HeuristicParameters? heuristicParameters = null;
            if (highTrajectory.Heuristic != null)
            {
                heuristicParameters = highTrajectory.Heuristic.GetParameters();
            }

            string heuristicName = "none";
            if (highTrajectory.Heuristic != null)
            {
                heuristicName = highTrajectory.Heuristic.GetName();
            }
            string? heuristicParameterString = null;
            if (heuristicParameters != null)
            {
                heuristicParameterString = heuristicParameters.GetCsvValues();
            }

            string? trajectoryName = highTrajectory.Name;
            if (trajectoryName == null)
            {
                trajectoryName = positionIndex.ToString(CultureInfo.InvariantCulture);
            }

            linePrefix = new(trajectoryName + "," + heuristicName + ",");
            if (heuristicParameterString != null)
            {
                linePrefix.Append(heuristicParameterString + ",");
            }

            string? firstThinAge = null;
            if (firstThinAgeAsInteger != Constant.NoThinPeriod)
            {
                firstThinAge = firstThinAgeAsInteger.ToString(CultureInfo.InvariantCulture);
                Debug.Assert(Constant.NoThinPeriod < firstThinAgeAsInteger);
            }
            string? secondThinAge = null;
            if (secondThinAgeAsInteger != Constant.NoThinPeriod)
            {
                secondThinAge = secondThinAgeAsInteger.ToString(CultureInfo.InvariantCulture);
                Debug.Assert((Constant.NoThinPeriod < firstThinAgeAsInteger) && (firstThinAgeAsInteger < secondThinAgeAsInteger));
            }
            string? thirdThinAge = null;
            if (thirdThinAgeAsInteger != Constant.NoThinPeriod)
            {
                thirdThinAge = thirdThinAgeAsInteger.ToString(CultureInfo.InvariantCulture);
                Debug.Assert((Constant.NoThinPeriod < firstThinAgeAsInteger) && (firstThinAgeAsInteger < secondThinAgeAsInteger) && (secondThinAgeAsInteger < thirdThinAgeAsInteger));
            }

            int rotationLengthAsInteger = highTrajectory.GetEndOfPeriodAge(endOfRotationPeriod);
            string rotationLength = rotationLengthAsInteger.ToString(CultureInfo.InvariantCulture);
            Debug.Assert((firstThinAgeAsInteger < rotationLengthAsInteger) && (secondThinAgeAsInteger < rotationLengthAsInteger) && (thirdThinAgeAsInteger < rotationLengthAsInteger));

            linePrefix.Append(firstThinAge + "," + secondThinAge + "," + thirdThinAge + "," + rotationLength + "," + financialScenarioName);
            return highTrajectory;
        }

        protected int GetMaxPositionIndex()
        {
            if (this.Results != null)
            {
                return this.Results!.PositionsEvaluated.Count;
            }

            Debug.Assert(this.Trajectories != null);
            return this.Trajectories.Count * this.Financial.Count;
        }

        protected void ValidateParameters()
        {
            if (this.Results == null)
            {
                // no results specified so at least one trajectory must be specified.
                if (this.Trajectories == null)
                {
                    throw new ParameterOutOfRangeException(nameof(this.Trajectories), "One of -" + nameof(this.Results) + " or -" + nameof(this.Trajectories) + " must be specified.");
                }
                if (this.Trajectories.Count < 1)
                {
                    throw new ParameterOutOfRangeException(nameof(this.Trajectories), "-" + nameof(this.Trajectories) + " is empty. If -" + nameof(this.Results) + " is not specified both -" + nameof(this.Trajectories) + " and -" + nameof(this.Financial) + " must be specified, containing at least one run and at least one financial scenario.");
                }
                if (this.Financial.Count < 1)
                {
                    throw new ParameterOutOfRangeException(nameof(this.Financial), "-" + nameof(this.Financial) + " is empty. If -" + nameof(this.Results) + " is not specified both -" + nameof(this.Trajectories) + " and -" + nameof(this.Financial) + " must be specified, containing at least one run and at least one financial scenario.");
                }
            }
            else
            {
                // results are specified so trajectories must be specified and there must be something in the results
                if (this.Trajectories != null)
                {
                    throw new ParameterOutOfRangeException(nameof(this.Trajectories), "Both -" + nameof(this.Results) + " and -" + nameof(this.Trajectories) + " are specified. Specify one or the other.");
                }
                else if (this.Results.PositionsEvaluated.Count < 1)
                {
                    throw new ParameterOutOfRangeException(nameof(this.Results), "-" + nameof(this.Results) + " is empty. At least one run must be present.");
                }
            }
        }
    }
}
