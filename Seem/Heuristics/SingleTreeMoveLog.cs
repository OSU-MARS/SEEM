using Mars.Seem.Silviculture;
using System.Collections.Generic;
using System.Globalization;

namespace Mars.Seem.Heuristics
{
    public class SingleTreeMoveLog : HeuristicMoveLog
    {
        // if needed, also track tree's plot ID by move
        public List<int> TreeIDByMove { get; protected init; }

        public SingleTreeMoveLog(int moveCapacity)
            : base(moveCapacity)
        {
            this.TreeIDByMove = new List<int>();
        }

        public override string GetCsvHeader(string prefix)
        {
            return prefix + "TreeEvaluated";
        }

        public override string GetCsvValues(StandTrajectoryCoordinate coordinate, int moveNumber)
        {
            return this.TreeIDByMove[moveNumber].ToString(CultureInfo.InvariantCulture);
        }

        public bool TryAddMove(int uncompactedTreeIndex)
        {
            if (this.TreeIDByMove.Count < this.MoveCapacity)
            {
                this.TreeIDByMove.Add(uncompactedTreeIndex);
                return true;
            }

            return false;
        }
    }
}
