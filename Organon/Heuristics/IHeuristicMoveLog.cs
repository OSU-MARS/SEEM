namespace Osu.Cof.Ferm.Heuristics
{
    public interface IHeuristicMoveLog
    {
        string GetCsvHeader(string prefix);
        string GetCsvValues(int move);
    }
}
