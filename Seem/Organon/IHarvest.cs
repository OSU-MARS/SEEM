namespace Osu.Cof.Ferm.Organon
{
    public interface IHarvest
    {
        int Period { get; }

        IHarvest Clone();
        float EvaluateTreeSelection(OrganonStandTrajectory trajectory);
    }
}
