namespace Osu.Cof.Ferm.Organon
{
    public interface IHarvest
    {
        int Period { get; set; }

        IHarvest Clone();
        float EvaluateTreeSelection(OrganonStandTrajectory trajectory);
    }
}
