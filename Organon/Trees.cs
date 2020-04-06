using System;

namespace Osu.Cof.Organon
{
    public class Trees
    {
        public float[] CrownRatio { get; private set; }

        /// <summary>
        /// DBH in inches.
        /// </summary>
        public float[] Dbh { get; private set; }

        /// <summary>
        /// DBH in inches at the most recent simulation step. 
        /// </summary>
        public float[] DbhGrowth { get; private set; }

        // accumulated expansion factor of dead trees from mortality chipping
        public float[] DeadExpansionFactor { get; private set; }

        /// <summary>
        /// Height in feet.
        /// </summary>
        public float[] Height { get; private set; }

        /// <summary>
        /// Height growth in feet at the most recent simulation step. 
        /// </summary>
        public float[] HeightGrowth { get; private set; }

        public float[] LiveExpansionFactor { get; private set; }

        public FiaCode[] Species { get; private set; }

        // trees' tag numbers, if specified
        public int[] Tag { get; private set; }

        public int TreeRecordCount { get; private set; }

        public Trees(int treeRecordCount)
        {
            // ensure array lengths are an exact multiple of the SIMD width
            int treeArrayLength = Constant.SimdWidthInSingles * (int)MathF.Ceiling((float)treeRecordCount / (float)Constant.SimdWidthInSingles);

            this.CrownRatio = new float[treeArrayLength];
            this.Dbh = new float[treeArrayLength];
            this.DbhGrowth = new float[treeArrayLength];
            this.DeadExpansionFactor = new float[treeArrayLength];
            this.LiveExpansionFactor = new float[treeArrayLength];
            this.Height = new float[treeArrayLength];
            this.HeightGrowth = new float[treeArrayLength];
            this.Species = new FiaCode[treeArrayLength];
            this.Tag = new int[treeArrayLength];
            this.TreeRecordCount = treeRecordCount;
        }

        public Trees(Trees other)
            : this(other.TreeRecordCount)
        {
            other.CrownRatio.CopyTo(this.CrownRatio, 0);
            other.Dbh.CopyTo(this.Dbh, 0);
            other.DbhGrowth.CopyTo(this.DbhGrowth, 0);
            other.DeadExpansionFactor.CopyTo(this.DeadExpansionFactor, 0);
            other.LiveExpansionFactor.CopyTo(this.LiveExpansionFactor, 0);
            other.Height.CopyTo(this.Height, 0);
            other.HeightGrowth.CopyTo(this.HeightGrowth, 0);
            other.Species.CopyTo(this.Species, 0);
            other.Tag.CopyTo(this.Tag, 0);
        }

        public float GetBasalArea(int treeIndex)
        {
            float dbhInInches = this.Dbh[treeIndex];
            float liveExpansionFactor = this.LiveExpansionFactor[treeIndex];
            return Constant.ForestersEnglish * dbhInInches * dbhInInches * liveExpansionFactor;
        }
    }
}
