using System;

namespace Osu.Cof.Organon.Test
{
    public class TreeLifeAndDeath
    {
        public float[] TotalDbhGrowthInInches { get; private set; }
        public float[] TotalDeadExpansionFactor { get; private set; }
        public float[] TotalHeightGrowthInFeet { get; private set; }

        public TreeLifeAndDeath(int treeRecordCount)
        {
            this.TotalDbhGrowthInInches = new float[treeRecordCount];
            this.TotalDeadExpansionFactor = new float[treeRecordCount];
            this.TotalHeightGrowthInFeet = new float[treeRecordCount];
        }

        public void Accumulate(float[] dbhInInchesAtStartOfCycle, float[] dbhInInchesAtEndOfCycle,
                               float[] heightInFeetAtStartOfCycle, float[] heightInFeetAtEndOfCycle,
                               float[] deadExpansionFactorAtEndOfCycle)
        {
            // TODO: remove workaround for hard coded Organon tree data arrays of length 2000 by changing < to !=
            int treeRecords = this.TotalDbhGrowthInInches.Length;
            if (dbhInInchesAtStartOfCycle.Length < treeRecords)
            {
                throw new ArgumentOutOfRangeException(nameof(dbhInInchesAtStartOfCycle));
            }
            if (dbhInInchesAtEndOfCycle.Length < treeRecords)
            {
                throw new ArgumentOutOfRangeException(nameof(dbhInInchesAtEndOfCycle));
            }
            if (heightInFeetAtStartOfCycle.Length < treeRecords)
            {
                throw new ArgumentOutOfRangeException(nameof(heightInFeetAtStartOfCycle));
            }
            if (heightInFeetAtEndOfCycle.Length < treeRecords)
            {
                throw new ArgumentOutOfRangeException(nameof(heightInFeetAtEndOfCycle));
            }
            if (deadExpansionFactorAtEndOfCycle.Length < treeRecords)
            {
                throw new ArgumentOutOfRangeException(nameof(deadExpansionFactorAtEndOfCycle));
            }

            for (int treeIndex = 0; treeIndex < treeRecords; ++treeIndex)
            {
                this.TotalDbhGrowthInInches[treeIndex] += (dbhInInchesAtEndOfCycle[treeIndex] - dbhInInchesAtStartOfCycle[treeIndex]);
                this.TotalHeightGrowthInFeet[treeIndex] += (heightInFeetAtEndOfCycle[treeIndex] - heightInFeetAtStartOfCycle[treeIndex]);
                this.TotalDeadExpansionFactor[treeIndex] += deadExpansionFactorAtEndOfCycle[treeIndex];
            }
        }
    }
}
