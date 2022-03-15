using System.Diagnostics;

namespace Mars.Seem.Tree
{
    public class StandDensity
    {
        public float BasalAreaPerHa { get; protected set; } // m²/ha
        public float TreesPerHa { get; protected set; }

        public StandDensity()
        {
            this.BasalAreaPerHa = 0.0F;
            this.TreesPerHa = 0.0F;
        }

        public StandDensity(Stand stand)
        {
            Debug.Assert(stand.GetUnits() == Units.English);

            float basalAreaPerAcre = 0.0F;
            float treesPerAcre = 0.0F;
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    float expansionFactor = treesOfSpecies.LiveExpansionFactor[treeIndex];
                    if (expansionFactor <= 0.0F)
                    {
                        continue;
                    }

                    treesPerAcre += expansionFactor;
                    basalAreaPerAcre += treesOfSpecies.GetBasalArea(treeIndex);
                }
            }

            this.BasalAreaPerHa = Constant.AcresPerHectare * Constant.SquareMetersPerSquareFoot * basalAreaPerAcre;
            this.TreesPerHa = Constant.AcresPerHectare * treesPerAcre;
        }

        public StandDensity(StandDensity other)
        {
            this.BasalAreaPerHa = other.BasalAreaPerHa;
            this.TreesPerHa = other.TreesPerHa;
        }
    }
}
