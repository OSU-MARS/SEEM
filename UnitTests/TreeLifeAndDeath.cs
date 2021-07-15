using Osu.Cof.Ferm.Organon;
using Osu.Cof.Ferm.Tree;
using System.Collections.Generic;

namespace Osu.Cof.Ferm.Test
{
    public class TreeLifeAndDeath
    {
        public Dictionary<FiaCode, float[]> TotalDbhGrowthInInches { get; private set; }
        public Dictionary<FiaCode, float[]> TotalDeadExpansionFactor { get; private set; }
        public Dictionary<FiaCode, float[]> TotalHeightGrowthInFeet { get; private set; }

        public TreeLifeAndDeath()
        {
            this.TotalDbhGrowthInInches = new Dictionary<FiaCode, float[]>();
            this.TotalDeadExpansionFactor = new Dictionary<FiaCode, float[]>();
            this.TotalHeightGrowthInFeet = new Dictionary<FiaCode, float[]>();
        }

        public void AccumulateGrowthAndMortality(OrganonStand stand)
        {
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                float[] totalDbhGrowthInInches = this.TotalDbhGrowthInInches.GetOrAdd(treesOfSpecies.Species, treesOfSpecies.Capacity);
                float[] totalHeightGrowthInFeet = this.TotalHeightGrowthInFeet.GetOrAdd(treesOfSpecies.Species, treesOfSpecies.Capacity);
                float[] totalDeadExpansionFactor = this.TotalDeadExpansionFactor.GetOrAdd(treesOfSpecies.Species, treesOfSpecies.Capacity);
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    totalDbhGrowthInInches[treeIndex] += treesOfSpecies.DbhGrowth[treeIndex];
                    totalHeightGrowthInFeet[treeIndex] += treesOfSpecies.HeightGrowth[treeIndex];
                    totalDeadExpansionFactor[treeIndex] += treesOfSpecies.DeadExpansionFactor[treeIndex];
                }
            }
        }
    }
}
