namespace Mars.Seem.Tree
{
    public class PreferredLogLength
    {
        public float PreferredLogLengthInMeters { get; init; }

        protected PreferredLogLength()
        {
            this.PreferredLogLengthInMeters = Constant.Bucking.DefaultLongLogLengthInM;
        }

        protected PreferredLogLength(PreferredLogLength other)
        {
            this.PreferredLogLengthInMeters = other.PreferredLogLengthInMeters;
        }

        public float GetPreferredTrim()
        {
            if (this.PreferredLogLengthInMeters > Constant.Bucking.ScribnerShortLogLengthInM)
            {
                return Constant.Bucking.ScribnerTrimLongLogInM;
            }
            return Constant.Bucking.ScribnerTrimShortLogInM;
        }
    }
}
