namespace Osu.Cof.Organon.Test
{
    public class OrganonErrorsAndWarnings
    {
        public int[] StandErrors { get; private set; }
        public int[] StandWarnings { get; private set; }

        public int[,] TreeErrors { get; private set; }
        public int[] TreeWarnings { get; private set; }

        public OrganonErrorsAndWarnings(int treeCount)
        {
            this.StandErrors = new int[35];
            this.StandWarnings = new int[9];

            // TODO: change to treeCount once Execute2.EDIT() is no longer hard coded to 2000
            this.TreeErrors = new int[2000, 6];
            this.TreeWarnings = new int[2000];
        }
    }
}
