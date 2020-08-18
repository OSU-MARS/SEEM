namespace Osu.Cof.Ferm.Heuristics
{
    public interface IHeuristicMoveLog
    {
        int Count { get; }

        string GetCsvHeader(string prefix);
        string GetCsvValues(int move);
    }
}
