using System.Collections.Generic;
using System.Globalization;

namespace Osu.Cof.Ferm.Heuristics
{
    public class SingleTreeMoveLog : IHeuristicMoveLog
    {
        public List<int> TreeIDByMove { get; protected set; }

        public SingleTreeMoveLog()
        {
            this.TreeIDByMove = new List<int>();
        }

        public int Count
        {
            get { return this.TreeIDByMove.Count; }
        }

        public string GetCsvHeader(string prefix)
        {
            return prefix + "tree evaluated";
        }

        public string GetCsvValues(int move)
        {
            return this.TreeIDByMove[move].ToString(CultureInfo.InvariantCulture);
        }
    }
}
