namespace Mars.Seem.Tree
{
    public class TreeSpeciesMerchantableVolumeForPeriod
    {
        // merchantable log volume by grade, m³/ha
        public float Cubic2Saw { get; set; }
        public float Cubic3Saw { get; set; }
        public float Cubic4Saw { get; set; }

        // merchantable log density by grade, logs/ha
        public float Logs2Saw { get; set; }
        public float Logs3Saw { get; set; }
        public float Logs4Saw { get; set; }

        // merchantable log volume by grade, Scribner MBF/ha
        public float Scribner2Saw { get; set; }
        public float Scribner3Saw { get; set; }
        public float Scribner4Saw { get; set; }

        public TreeSpeciesMerchantableVolumeForPeriod()
        {
            this.Cubic2Saw = 0.0F;
            this.Cubic3Saw = 0.0F;
            this.Cubic4Saw = 0.0F;
            this.Logs2Saw = 0.0F;
            this.Logs3Saw = 0.0F;
            this.Logs4Saw = 0.0F;
            this.Scribner2Saw = 0.0F;
            this.Scribner3Saw = 0.0F;
            this.Scribner4Saw = 0.0F;
        }

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
