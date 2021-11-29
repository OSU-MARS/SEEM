namespace Mars.Seem.Heuristics
{
    public class OneOptMove
    {
        public int ThinPeriod { get; set; }
        public int TreeIndex { get; set; }

        public OneOptMove(int treeIndex, int harvestPeriod)
        {
            this.ThinPeriod = harvestPeriod;
            this.TreeIndex = treeIndex;
        }
    }
}
