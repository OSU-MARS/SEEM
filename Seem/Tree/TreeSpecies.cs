using System.Collections.Generic;

namespace Mars.Seem.Tree
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
