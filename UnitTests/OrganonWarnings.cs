namespace Osu.Cof.Organon.Test
{
    public class OrganonWarnings
    {
        public int[] StandWarnings { get; private set; }
        public int[] TreeWarnings { get; private set; }

        public OrganonWarnings(int treeCount)
        {
            this.StandWarnings = new int[9];
            this.TreeWarnings = new int[treeCount];
        }
    }
}
