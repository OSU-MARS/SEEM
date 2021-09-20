using Osu.Cof.Ferm.Organon;

namespace Osu.Cof.Ferm.Silviculture
{
    public abstract class Harvest
    {
        public int Period { get; set; }

        public abstract Harvest Clone();
        public abstract float EvaluateTreeSelection(OrganonStandTrajectory trajectory);
        public abstract bool TryCopyFrom(Harvest other);
    }
}
