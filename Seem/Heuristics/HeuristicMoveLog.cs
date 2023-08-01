using Mars.Seem.Silviculture;

namespace Mars.Seem.Heuristics
{
    public abstract class HeuristicMoveLog
    {
        public int MoveCapacity { get; private init; }

        protected HeuristicMoveLog(int moveCapacity)
        {
            this.MoveCapacity = moveCapacity;
        }

        public abstract string GetCsvHeader(string prefix);
        public abstract string GetCsvValues(SilviculturalCoordinate coordinate, int moveNumber);

        public virtual int GetMoveNumberWithDefaulting(SilviculturalCoordinate coordinate, int moveIndex)
        {
            return moveIndex;
        }
    }
}
