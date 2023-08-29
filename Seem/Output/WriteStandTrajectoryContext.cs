using Mars.Seem.Optimization;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;

namespace Mars.Seem.Output
{
    public class WriteStandTrajectoryContext : WriteSilviculturalCoordinateContext
    {
        // global settings invariant across all stands
        public float DiameterClassSize { get; private init; }
        public bool HarvestsOnly { get; private init; }
        public float MaximumDiameter { get; private init; }
        public bool NoCarbon { get; private init; }
        public bool NoEquipmentProductivity { get; private init; }
        public bool NoFinancial { get; private init; }
        public bool NoHarvestCosts { get; private init; }
        public bool NoTimberSorts { get; private init; }
        public bool NoTreeGrowth { get; private init; }
        public int? StartYear { get; init; }

        public WriteStandTrajectoryContext(bool harvestsOnly, bool heuristicParameters, bool noTreeGrowth, bool noFinancial, bool noCarbon, bool noHarvestCosts, bool noTimberSorts, bool noEquipmentProductivity, float diameterClassSize, float maximumDiameter)
            : base(heuristicParameters)
        {
            this.DiameterClassSize = diameterClassSize;
            this.HarvestsOnly = harvestsOnly;
            this.MaximumDiameter = maximumDiameter;
            this.NoCarbon = noCarbon;
            this.NoEquipmentProductivity = noEquipmentProductivity;
            this.NoFinancial = noFinancial;
            this.NoHarvestCosts = noHarvestCosts;
            this.NoTimberSorts = noTimberSorts;
            this.NoTreeGrowth = noTreeGrowth;
            this.StartYear = null;
        }

        public int GetPeriodsToWrite(IList<SilviculturalSpace> silviculturalSpaces)
        {
            int periodsToWrite = 0;
            for (int spaceIndex = 0; spaceIndex < silviculturalSpaces.Count; ++spaceIndex)
            {
                SilviculturalSpace silviculturalSpace = silviculturalSpaces[spaceIndex];

                for (int coordinateIndex = 0; coordinateIndex < silviculturalSpace.CoordinatesEvaluated.Count; ++coordinateIndex)
                {
                    SilviculturalCoordinate coordinate = silviculturalSpace.CoordinatesEvaluated[coordinateIndex];
                    StandTrajectory highTrajectory = silviculturalSpace.GetHighTrajectory(coordinate);
                    periodsToWrite += this.GetPeriodsToWrite(highTrajectory, silviculturalSpace.FinancialScenarios.Count);
                }
            }

            return periodsToWrite;
        }

        public int GetPeriodsToWrite(IList<StandTrajectory> trajectories)
        {
            int periodsToWrite = 0;
            for (int trajectoryIndex = 0; trajectoryIndex < trajectories.Count; ++trajectoryIndex)
            {
                periodsToWrite += this.GetPeriodsToWrite(trajectories[trajectoryIndex]);
            }

            return periodsToWrite;
        }

        public int GetPeriodsToWrite(StandTrajectory trajectory)
        {
            return this.GetPeriodsToWrite(trajectory, FinancialScenarios.Count);
        }

        private int GetPeriodsToWrite(StandTrajectory trajectory, int financialScenarioCount)
        {
            int periodsToWrite;
            if (this.HarvestsOnly)
            {
                int harvests = 0;
                for (int harvestIndex = 0; harvestIndex < trajectory.Treatments.Harvests.Count; ++harvestIndex)
                {
                    ++harvests;

                    Harvest harvest = trajectory.Treatments.Harvests[harvestIndex];
                    if (harvest.Period == EndOfRotationPeriod)
                    {
                        return harvests; // thin scheduled in same period as end of rotation
                    }
                }

                periodsToWrite = ++harvests; // add one for regeneration harvest
            }
            else
            {
                periodsToWrite = trajectory.StandByPeriod.Length;
            }

            periodsToWrite *= financialScenarioCount;
            return periodsToWrite;
        }

        public void SetStandTrajectoryCoordinate(StandTrajectory trajectory, int financialIndex)
        {
            this.HighTrajectoryNullable = trajectory;
            
            this.FirstThinPeriod = trajectory.GetFirstThinPeriod();
            this.SecondThinPeriod = trajectory.GetSecondThinPeriod();
            this.ThirdThinPeriod = trajectory.GetThirdThinPeriod();
            this.EndOfRotationPeriod = trajectory.PlanningPeriods - 1;
            this.FinancialIndex = financialIndex;
        }
    }
}
