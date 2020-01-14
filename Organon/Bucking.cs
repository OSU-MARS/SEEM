namespace Osu.Cof.Organon
{
    public class Bucking
    {
        /// Merchantable log length (feet)?
        public float PreferredLogLength { get; set; }
        /// Stump cut height (feet?).
        public float StumpCutHeight { get; set; }
        /// Merchantable top diameter (inches).
        public float TopDiameter { get; set; }
        /// Log trim allowance (inches).
        public float Trim { get; set; }

        public Bucking()
        {
            this.PreferredLogLength = 40.0F;
            this.StumpCutHeight = 0.5F;
            this.TopDiameter = 6.0F;
            this.Trim = 12.0F; // TODO: unclear if this is total or per log end 6.0F?
        }
    }
}
