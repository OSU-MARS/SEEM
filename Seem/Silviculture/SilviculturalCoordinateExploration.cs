using Mars.Seem.Optimization;

namespace Mars.Seem.Silviculture
{
    public class SilviculturalCoordinateExploration
    {
        public OptimizationObjectiveDistribution Distribution { get; set; }
        public SilviculturalPrescriptionPool Pool { get; set; }

        public SilviculturalCoordinateExploration(int poolCapacity)
        {
            this.Distribution = new OptimizationObjectiveDistribution();
            this.Pool = new SilviculturalPrescriptionPool(poolCapacity);
        }
    }
}
