using System;
using System.Globalization;

namespace Mars.Seem.Heuristics
{
    public class TabuParameters : HeuristicParameters
    {
        public int EscapeAfter { get; set; }
        public int EscapeDistance { get; set; }
        public int Iterations { get; set; }
        //public int Jump { get; set; }
        public int MaximumTenure { get; set; }
        public TabuTenure Tenure { get; set; }

        public TabuParameters()
        {
            this.EscapeAfter = Int32.MaxValue;
            this.EscapeDistance = 2;
            this.Iterations = 100;
            //this.Jump = 1;
            this.Tenure = Constant.TabuDefault.Tenure;
        }

        public TabuParameters(int treeCount)
            : this()
        {
            this.EscapeAfter = (int)(Constant.TabuDefault.EscapeAfter * treeCount);
            this.EscapeDistance = Math.Max((int)(Constant.TabuDefault.EscapeBy * treeCount), 2);
            this.Iterations = treeCount;
            this.MaximumTenure = (int)(Constant.TabuDefault.MaximumTenureRatio * treeCount);
        }

        public override string GetCsvHeader()
        {
            // for now, don't log iterations
            return base.GetCsvHeader() + ",perturbation,escape after,escape distance,tenure method,max tenure";
        }

        public override string GetCsvValues()
        {
            // for now don't log iterations
            return base.GetCsvValues() + "," +
                   this.MinimumConstructionGreediness.ToString(CultureInfo.InvariantCulture) + "," +
                   this.EscapeAfter.ToString(CultureInfo.InvariantCulture) + "," +
                   this.EscapeDistance.ToString(CultureInfo.InvariantCulture) + "," +
                   this.Tenure.ToString() + "," +
                   this.MaximumTenure.ToString(CultureInfo.InvariantCulture);
        }
    }
}
