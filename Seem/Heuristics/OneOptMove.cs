namespace Osu.Cof.Ferm.Heuristics
{
    public class OneOptMove
    {
        public int HarvestPeriod { get; set; }
        public int TreeIndex { get; set; }

        public OneOptMove(int treeIndex, int harvestPeriod)
        {
            this.HarvestPeriod = harvestPeriod;
            this.TreeIndex = treeIndex;
        }
    }
}
