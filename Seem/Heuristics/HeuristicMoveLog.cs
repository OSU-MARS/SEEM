namespace Osu.Cof.Ferm.Heuristics
{
    public abstract class HeuristicMoveLog
    {
        public int MoveCapacity { get; private init; }

        protected HeuristicMoveLog(int moveCapacity)
        {
            this.MoveCapacity = moveCapacity;
        }

        public abstract string GetCsvHeader(string prefix);
        public abstract string GetCsvValues(HeuristicResultPosition position, int moveNumber);

        public virtual int GetMoveNumberWithDefaulting(HeuristicResultPosition position, int moveIndex)
        {
            return moveIndex;
        }
    }
}
