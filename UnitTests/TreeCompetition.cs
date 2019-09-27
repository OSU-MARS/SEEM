namespace Osu.Cof.Organon.Test
{
    // see Stats.SSTATS()
    internal class TreeCompetition
    {
        // BALL
        public float[] LargeTreeBasalAreaLarger { get; private set; }
        // CCFLL
        public float[] LargeTreeCrownCompetition { get; private set; }

        // BAL
        public float[] SmallTreeBasalAreaLarger { get; private set; }
        // CCFL
        public float[] SmallTreeCrownCompetition { get; private set; }

        public TreeCompetition()
        {
            this.LargeTreeBasalAreaLarger = new float[51];
            this.LargeTreeCrownCompetition = new float[51];
            this.SmallTreeBasalAreaLarger = new float[500];
            this.SmallTreeCrownCompetition = new float[500];
        }
    }
}
