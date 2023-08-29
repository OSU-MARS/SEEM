using Mars.Seem.Heuristics;
using Mars.Seem.Optimization;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Mars.Seem.Output
{
    public class WriteSilviculturalCoordinateContext
    {
        // per run (heuristic optimization or group of stands)/per coordinate settings
        private FinancialScenarios? financialScenarios;
        private SilviculturalCoordinate? silviculturalCoordinate;
        private SilviculturalSpace? silviculturalSpace;

        protected StandTrajectory? HighTrajectoryNullable { get; set; }

        // global settings invariant across all coordinates
        public bool HeuristicParameters { get; private init; }

        // per coordinate settings
        public int FirstThinPeriod { get; protected set; }
        public int SecondThinPeriod { get; protected set; }
        public int ThirdThinPeriod { get; protected set; }
        public int EndOfRotationPeriod { get; protected set; }
        public int FinancialIndex { get; protected set; }

        public WriteSilviculturalCoordinateContext(bool heuristicParameters)
        {
            this.financialScenarios = null;
            this.silviculturalCoordinate = null;
            this.silviculturalSpace = null;
            this.HighTrajectoryNullable = null;

            this.HeuristicParameters = heuristicParameters;

            this.FirstThinPeriod = Constant.NoHarvestPeriod;
            this.SecondThinPeriod = Constant.NoHarvestPeriod;
            this.ThirdThinPeriod = Constant.NoHarvestPeriod;
            this.EndOfRotationPeriod = -1;
            this.FinancialIndex = -1;
        }

        public FinancialScenarios FinancialScenarios
        {
            get
            {
                if (this.financialScenarios == null)
                {
                    throw new InvalidOperationException("Financial scenarios have not been specified.  Either set them at construction time or call " + nameof(this.SetSilviculturalCoordinate) + "() before accessing the " + nameof(FinancialScenarios) + " property.");
                }
                return this.financialScenarios;
            }
            init 
            { 
                this.financialScenarios = value; 
            }
        }

        public StandTrajectory HighTrajectory
        {
            get
            {
                if (this.HighTrajectoryNullable == null)
                {
                    throw new InvalidOperationException("Silvicultural coordinate has not been specified.  Call " + nameof(this.SetSilviculturalCoordinate) + "() and " + nameof(this.SetSilviculturalCoordinate) + " before calling " + nameof(this.HighTrajectory) + ".");
                }

                return this.HighTrajectoryNullable;
            }
        }

        public string GetCsvPrefixForSilviculturalCoordinate()
        {
            string? maybeHeuristicParametersWithTrailingComma = null;
            if (this.HeuristicParameters)
            {
                if (this.silviculturalSpace == null)
                {
                    throw new InvalidOperationException("Silvicultural space has not been specified.  Call " + nameof(this.SetSilviculturalSpace) + "() before calling " + nameof(this.GetCsvPrefixForSilviculturalCoordinate) + ".");
                }
                if (this.silviculturalCoordinate == null)
                {
                    throw new InvalidOperationException("Silvicultural coordinate has not been specified, leaving heuristic parameter index unknown.  Call " + nameof(this.SetSilviculturalCoordinate) + "() and " + nameof(this.SetSilviculturalCoordinate) + " before calling " + nameof(this.GetCsvPrefixForSilviculturalCoordinate) + ".");
                }

                if (this.silviculturalSpace is HeuristicStandTrajectories heuristicTrajectories)
                {
                    maybeHeuristicParametersWithTrailingComma = heuristicTrajectories.GetParameters(this.silviculturalCoordinate.ParameterIndex).GetCsvValues() + ",";
                }
            }

            (int firstThinAge, int secondThinAge, int thirdThinAge, int rotationAge) = this.GetHarvestAges();
            return this.HighTrajectoryNullable.Name + "," +
                   maybeHeuristicParametersWithTrailingComma +
                   (firstThinAge != Constant.NoHarvestPeriod ? firstThinAge.ToString(CultureInfo.InvariantCulture) : null) + "," +
                   (secondThinAge != Constant.NoHarvestPeriod ? secondThinAge.ToString(CultureInfo.InvariantCulture) : null) + "," +
                   (thirdThinAge != Constant.NoHarvestPeriod ? thirdThinAge.ToString(CultureInfo.InvariantCulture) : null) + "," +
                   rotationAge.ToString(CultureInfo.InvariantCulture) + "," +
                   this.FinancialScenarios.Name[this.FinancialIndex];
        }

        [MemberNotNull(nameof(WriteSilviculturalCoordinateContext.HighTrajectoryNullable))]
        public (int firstThinAge, int secondThinAge, int thirdThinAge, int rotationAge) GetHarvestAges()
        {
            if (this.HighTrajectoryNullable == null)
            {
                throw new InvalidOperationException("Silvicultural coordinate has not been specified.  Call " + nameof(this.SetSilviculturalCoordinate) + "() and " + nameof(this.SetSilviculturalCoordinate) + " before calling " + nameof(this.GetHarvestAges) + ".");
            }

            int firstThinAge = Constant.NoHarvestPeriod;
            if (this.FirstThinPeriod != Constant.NoHarvestPeriod)
            {
                firstThinAge = this.HighTrajectoryNullable.GetEndOfPeriodAge(this.FirstThinPeriod);
            }

            int secondThinAge = Constant.NoHarvestPeriod;
            if (this.SecondThinPeriod != Constant.NoHarvestPeriod)
            {
                secondThinAge = this.HighTrajectoryNullable.GetEndOfPeriodAge(this.SecondThinPeriod);
            }

            int thirdThinAge = Constant.NoHarvestPeriod;
            if (this.ThirdThinPeriod != Constant.NoHarvestPeriod)
            {
                thirdThinAge = this.HighTrajectoryNullable.GetEndOfPeriodAge(this.ThirdThinPeriod);
            }

            int rotationAge = this.HighTrajectoryNullable.GetEndOfPeriodAge(EndOfRotationPeriod);

            return (firstThinAge, secondThinAge, thirdThinAge, rotationAge);
        }

        public void SetSilviculturalSpace(SilviculturalSpace silviculturalSpace)
        {
            this.financialScenarios = silviculturalSpace.FinancialScenarios;
            this.silviculturalSpace = silviculturalSpace;
        }

        public void SetSilviculturalCoordinate(int coordinateIndex)
        {
            if (this.silviculturalSpace == null)
            {
                throw new InvalidOperationException("Silvicultural space has not been specified.  Call " + nameof(this.SetSilviculturalSpace) + "() before calling " + nameof(this.SetSilviculturalCoordinate) + ".");
            }

            this.SetSilviculturalCoordinate(this.silviculturalSpace.CoordinatesEvaluated[coordinateIndex]);
        }

        public void SetSilviculturalCoordinate(SilviculturalCoordinate coordinate)
        {
            if (this.silviculturalSpace == null)
            {
                throw new InvalidOperationException("Silvicultural space has not been specified.  Call " + nameof(this.SetSilviculturalSpace) + "() before calling " + nameof(this.SetSilviculturalCoordinate) + ".");
            }

            this.silviculturalCoordinate = coordinate;
            this.HighTrajectoryNullable = this.silviculturalSpace.GetHighTrajectory(coordinate);

            this.FirstThinPeriod = this.silviculturalSpace.FirstThinPeriods[coordinate.FirstThinPeriodIndex];
            this.SecondThinPeriod = this.silviculturalSpace.SecondThinPeriods[coordinate.SecondThinPeriodIndex];
            this.ThirdThinPeriod = this.silviculturalSpace.ThirdThinPeriods[coordinate.ThirdThinPeriodIndex];
            this.EndOfRotationPeriod = this.silviculturalSpace.RotationLengths[coordinate.RotationIndex];
            this.FinancialIndex = coordinate.FinancialIndex;
        }
    }
}
