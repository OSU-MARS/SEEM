using Mars.Seem.Optimization;
using System;

namespace Mars.Seem.Cmdlets
{
    public class WriteStandTrajectoryContext
    {
        // global settings invariant across all stands
        public float DiameterClassSize { get; private init; }
        public FinancialScenarios FinancialScenarios { get; private init; }
        public bool HarvestsOnly { get; private init; }
        public float MaximumDiameter { get; private init; }
        public bool NoCarbon { get; private init; }
        public bool NoEquipmentProductivity { get; private init; }
        public bool NoFinancial { get; private init; }
        public bool NoHarvestCosts { get; private init; }
        public bool NoTimberSorts { get; private init; }
        public bool NoTreeGrowth { get; private init; }

        // per stand settings
        public int EndOfRotationPeriodIndex { get; set; }
        public int FinancialIndex { get; set; }
        public string LinePrefix { get; set; }

        public WriteStandTrajectoryContext(FinancialScenarios financialScenarios, bool harvestsOnly, bool noTreeGrowth, bool noFinancial, bool noCarbon, bool noHarvestCosts, bool noTimberSorts, bool noEquipmentProductivity, float diameterClassSize, float maximumDiameter)
        {
            this.DiameterClassSize = diameterClassSize;
            this.FinancialScenarios = financialScenarios;
            this.HarvestsOnly = harvestsOnly;
            this.MaximumDiameter = maximumDiameter;
            this.NoCarbon = noCarbon;
            this.NoEquipmentProductivity = noEquipmentProductivity;
            this.NoFinancial = noFinancial;
            this.NoHarvestCosts = noHarvestCosts;
            this.NoTimberSorts = noTimberSorts;
            this.NoTreeGrowth = noTreeGrowth;

            this.EndOfRotationPeriodIndex = -1;
            this.FinancialIndex = -1;
            this.LinePrefix = String.Empty;
        }
    }
}
