using Osu.Cof.Ferm.Organon;
using System;

namespace Osu.Cof.Ferm.Test
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

        public void AccumulateGrowthAndMortality(OrganonStand stand)
        {
            int treeRecords = this.TotalDbhGrowthInInches.Length;
            if (stand.TreeRecordCount != treeRecords)
            {
                throw new ArgumentOutOfRangeException(nameof(stand));
            }

            for (int treeIndex = 0; treeIndex < treeRecords; ++treeIndex)
            {
                this.TotalDbhGrowthInInches[treeIndex] += stand.DbhGrowth[treeIndex];
                this.TotalHeightGrowthInFeet[treeIndex] += stand.HeightGrowth[treeIndex];
                this.TotalDeadExpansionFactor[treeIndex] += stand.DeadExpansionFactor[treeIndex];
            }
        }
    }
}
