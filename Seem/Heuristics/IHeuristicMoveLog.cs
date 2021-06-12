namespace Osu.Cof.Ferm.Heuristics
{
    public interface IHeuristicMoveLog
    {
        int LengthInMoves { get; }

        string GetCsvHeader(string prefix);
        string GetCsvValues(HeuristicResultPosition position, int move);
    }
}
