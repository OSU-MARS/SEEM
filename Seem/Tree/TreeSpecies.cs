using System.Collections.Generic;

namespace Osu.Cof.Ferm.Tree
{
    public static class TreeSpecies
    {
        public static SortedList<FiaCode, TreeSpeciesProperties> Properties { get; private set; }

        static TreeSpecies()
        {
            TreeSpecies.Properties = new() { { FiaCode.PseudotsugaMenziesii, DouglasFir.Properties } };
        }
    }
}
