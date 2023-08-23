using Mars.Seem.Optimization;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;

namespace Mars.Seem.Cmdlets
{
    public class WriteStandTrajectoryContext
    {
        // per run (heuristic optimization or group of stands) settings
        private FinancialScenarios? financialScenarios;

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

        // per trajectory settings
        public int EndOfRotationPeriodIndex { get; private set; }
        public int FinancialIndex { get; private set; }
        public string LinePrefix { get; private set; }

        public WriteStandTrajectoryContext(bool harvestsOnly, bool noTreeGrowth, bool noFinancial, bool noCarbon, bool noHarvestCosts, bool noTimberSorts, bool noEquipmentProductivity, float diameterClassSize, float maximumDiameter)
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

            this.financialScenarios = null;

            this.EndOfRotationPeriodIndex = -1;
            this.FinancialIndex = -1;
            this.LinePrefix = String.Empty;
        }

        public FinancialScenarios FinancialScenarios 
        { 
            get 
            { 
                if (this.financialScenarios == null)
                {
                    throw new InvalidOperationException("Financial scenarios have not been specified. Either set them at construction time or call " + nameof(this.SetSilviculturalCoordinate) + "() before accessing the " + nameof(this.FinancialScenarios) + " property.");
                }
                return this.financialScenarios;
            }
            init { this.financialScenarios = value; }
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
            return this.GetPeriodsToWrite(trajectory, this.FinancialScenarios.Count);
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
                    if (harvest.Period == this.EndOfRotationPeriodIndex)
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

        public void SetSilviculturalSpace(SilviculturalSpace silviculturalSpace)
        {
            this.financialScenarios = silviculturalSpace.FinancialScenarios;
        }

        public void SetSilviculturalCoordinate(string linePrefix, int financialIndex, int endOfRotationPeriod)
        {
            this.EndOfRotationPeriodIndex = endOfRotationPeriod;
            this.FinancialIndex = financialIndex;
            this.LinePrefix = linePrefix;
        }
    }
}
