using Mars.Seem.Organon;

namespace Mars.Seem.Silviculture
{
    public abstract class Harvest
    {
        public int Period { get; set; }

        public abstract Harvest Clone();
        public abstract float EvaluateTreeSelection(OrganonStandTrajectory trajectory);
        public abstract bool TryCopyFrom(Harvest other);
    }
}
