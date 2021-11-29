using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mars.Seem.Tree
{
    public class StandMerchantableVolume
    {
        // merchantable cubic volume, m³/ha, by period
        public float[] Cubic2Saw { get; private init; }
        public float[] Cubic3Saw { get; private init; }
        public float[] Cubic4Saw { get; private init; }

        // log density, logs/ha, by period
        public float[] Logs2Saw { get; private init; }
        public float[] Logs3Saw { get; private init; }
        public float[] Logs4Saw { get; private init; }

        // Scribner board foot volume, MBF/ha, by period
        public float[] Scribner2Saw { get; private init; }
        public float[] Scribner3Saw { get; private init; }
        public float[] Scribner4Saw { get; private init; }

        public StandMerchantableVolume(SortedList<FiaCode, TreeSpeciesMerchantableVolume> volumeBySpecies)
        {
            if (volumeBySpecies.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(volumeBySpecies));
            }
            
            int planningPeriods = volumeBySpecies.Values[0].Cubic2Saw.Length;
            this.Cubic2Saw = new float[planningPeriods];
            this.Cubic3Saw = new float[planningPeriods];
            this.Cubic4Saw = new float[planningPeriods];
            this.Logs2Saw = new float[planningPeriods];
            this.Logs3Saw = new float[planningPeriods];
            this.Logs4Saw = new float[planningPeriods];
            this.Scribner2Saw = new float[planningPeriods];
            this.Scribner3Saw = new float[planningPeriods];
            this.Scribner4Saw = new float[planningPeriods];

            foreach (TreeSpeciesMerchantableVolume volumeForSpecies in volumeBySpecies.Values)
            {
                for (int periodIndex = 0; periodIndex < planningPeriods; ++periodIndex)
                {
                    Debug.Assert(volumeForSpecies.IsCalculated(periodIndex));

                    this.Cubic2Saw[periodIndex] += volumeForSpecies.Cubic2Saw[periodIndex];
                    this.Cubic3Saw[periodIndex] += volumeForSpecies.Cubic3Saw[periodIndex];
                    this.Cubic4Saw[periodIndex] += volumeForSpecies.Cubic4Saw[periodIndex];
                    this.Logs2Saw[periodIndex] += volumeForSpecies.Logs2Saw[periodIndex];
                    this.Logs3Saw[periodIndex] += volumeForSpecies.Logs3Saw[periodIndex];
                    this.Logs4Saw[periodIndex] += volumeForSpecies.Logs4Saw[periodIndex];
                    this.Scribner2Saw[periodIndex] += volumeForSpecies.Scribner2Saw[periodIndex];
                    this.Scribner3Saw[periodIndex] += volumeForSpecies.Scribner3Saw[periodIndex];
                    this.Scribner4Saw[periodIndex] += volumeForSpecies.Scribner4Saw[periodIndex];
                }
            }
        }

        public float GetCubicTotal(int periodIndex)
        {
            return this.Cubic2Saw[periodIndex] + this.Cubic3Saw[periodIndex] + this.Cubic4Saw[periodIndex];
        }

        public float GetScribnerTotal(int periodIndex)
        {
            return this.Scribner2Saw[periodIndex] + this.Scribner3Saw[periodIndex] + this.Scribner4Saw[periodIndex];
        }
    }
}
