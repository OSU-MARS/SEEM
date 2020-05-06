namespace Osu.Cof.Ferm.Organon
{
    public interface IHarvest
    {
        int Period { get; }

        float EvaluateTreeSelection(OrganonStandTrajectory trajectory);
    }
}
