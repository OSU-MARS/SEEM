using Mars.Seem.Optimization;

namespace Mars.Seem.Silviculture
{
    public class StandTrajectoryArrayElement
    {
        public OptimizationObjectiveDistribution Distribution { get; set; }
        public SilviculturalPrescriptionPool Pool { get; set; }

        public StandTrajectoryArrayElement(int poolCapacity)
        {
            this.Distribution = new OptimizationObjectiveDistribution();
            this.Pool = new SilviculturalPrescriptionPool(poolCapacity);
        }
    }
}
