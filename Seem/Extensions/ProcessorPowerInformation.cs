namespace Mars.Seem.Extensions
{
    public class ProcessorPowerInformation
    {
        public int[] CurrentIdleState { get; private init; }
        public int[] CurrentMHz { get; private init; }
        public int[] MaxMHz { get; private init; }
        public int[] MaxIdleState { get; private init; }
        public int[] MHzLimit { get; private init; }

        internal ProcessorPowerInformation(NativeMethods.PROCESSOR_POWER_INFORMATION[] nativeInfo)
        {
            this.CurrentIdleState = new int[nativeInfo.Length];
            this.CurrentMHz = new int[nativeInfo.Length];
            this.MaxMHz = new int[nativeInfo.Length];
            this.MaxIdleState = new int[nativeInfo.Length];
            this.MHzLimit = new int[nativeInfo.Length];

            for (int thread = 0; thread < nativeInfo.Length; ++thread)
            {
                this.CurrentIdleState[thread] = (int)nativeInfo[thread].CurrentIdleState;
                this.CurrentMHz[thread] = (int)nativeInfo[thread].CurrentMhz;
                this.MaxMHz[thread] = (int)nativeInfo[thread].MaxMhz;
                this.MaxIdleState[thread] = (int)nativeInfo[thread].MaxIdleState;
                this.MHzLimit[thread] = (int)nativeInfo[thread].MhzLimit;
            }
        }

        public int Threads
        {
            get { return this.CurrentIdleState.Length; }
        }

        public float GetPerformanceFrequencyInGHz()
        {
            return 0.001F * this.MaxMHz[0];
        }
    }
}
