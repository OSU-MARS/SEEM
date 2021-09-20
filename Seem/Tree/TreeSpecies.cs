using System.Collections.Generic;

namespace Osu.Cof.Ferm.Tree
{
    public static class TreeSpecies
    {
        public static SortedList<FiaCode, TreeSpeciesProperties> Properties { get; private set; }

        static TreeSpecies()
        {
            // Miles PD, Smith BW. 2009. Specific gravity and other properties of wood and bark for 156 tree species found in North
            //   America (No. NRS-RN-38). Northern Research Station, US Forest Service. https://doi.org/10.2737/NRS-RN-38
            TreeSpecies.Properties = new();
            TreeSpecies.Properties.Add(FiaCode.PseudotsugaMenziesii, new()
            {
                BarkDensity = 833.0F,
                BarkFraction = 0.176F,
                BarkFractionRemainingAfterProcessing = 0.12F,
                WoodDensity = 609F
            });
        }
    }
}
