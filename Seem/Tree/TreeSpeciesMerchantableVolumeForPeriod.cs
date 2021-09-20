namespace Osu.Cof.Ferm.Tree
{
    public class TreeSpeciesMerchantableVolumeForPeriod
    {
        // 
        public float Cubic2Saw { get; set; }
        public float Cubic3Saw { get; set; }
        public float Cubic4Saw { get; set; }

        public float Logs2Saw { get; set; }
        public float Logs3Saw { get; set; }
        public float Logs4Saw { get; set; }

        public float Scribner2Saw { get; set; }
        public float Scribner3Saw { get; set; }
        public float Scribner4Saw { get; set; }

        public void ConvertToMbf()
        {
            // convert from board feet to MBF
            this.Scribner2Saw *= 0.001F;
            this.Scribner3Saw *= 0.001F;
            this.Scribner4Saw *= 0.001F;
        }

        public void Multiply(float value)
        {
            this.Cubic2Saw *= value;
            this.Cubic3Saw *= value;
            this.Cubic4Saw *= value;

            this.Logs2Saw *= value;
            this.Logs3Saw *= value;
            this.Logs4Saw *= value;

            this.Scribner2Saw *= value;
            this.Scribner3Saw *= value;
            this.Scribner4Saw *= value;
        }
    }
}
