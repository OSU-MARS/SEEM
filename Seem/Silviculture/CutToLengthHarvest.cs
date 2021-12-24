using System;

namespace Mars.Seem.Silviculture
{
    public class CutToLengthHarvest : HarvestFinancialValue
    {
        public float ChainsawBasalAreaPerHaWithWheeledHarvester { get; set; } // m²/ha
        public ChainsawCrewType ChainsawCrewWithWheeledHarvester { get; set; }
        public float ChainsawCubicVolumePerHaWithWheeledHarvester { get; set; } // merchantable m³/ha
        public float ChainsawPMhPerHaWithWheeledHarvester { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha

        public float ForwardedWeightPeHa { get; set; } // kg/ha
        public float ForwarderCostPerHa { get; set; } // US$/ha
        public float ForwarderPMhPerHa { get; set; } // accumulated in delay free minutes/ha and then converted to PMh₀/ha

        // TODO: merge into base class
        public float MerchantableCubicVolumePerHa { get; set; } // m³/ha
        public float MinimumSystemCostPerHa { get; set; } // US$/ha
        public HarvestEquipmentProductivity Productivity { get; private init; }

        public float WheeledHarvesterCostPerHa { get; set; } // US$/ha
        public float WheeledHarvesterPMhPerHa { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha

        public CutToLengthHarvest()
        {
            this.ChainsawBasalAreaPerHaWithWheeledHarvester = 0.0F;
            this.ChainsawCrewWithWheeledHarvester = ChainsawCrewType.None;
            this.ChainsawCubicVolumePerHaWithWheeledHarvester = 0.0F;
            this.ChainsawPMhPerHaWithWheeledHarvester = 0.0F;

            this.ForwardedWeightPeHa = 0.0F;
            this.ForwarderCostPerHa = 0.0F;
            this.ForwarderPMhPerHa = 0.0F;

            this.MerchantableCubicVolumePerHa = 0.0F;
            this.MinimumSystemCostPerHa = Single.NaN;
            this.Productivity = new();
            
            this.WheeledHarvesterCostPerHa = Single.NaN;
            this.WheeledHarvesterPMhPerHa = 0.0F;
        }

        public class HarvestEquipmentProductivity
        {
            public float Forwarder { get; set; } // m³/PMh₀
            public ForwarderLoadingMethod ForwardingMethod { get; set; }
            public float WheeledHarvester { get; set; } // m³/PMh₀

            public HarvestEquipmentProductivity()
            {
                this.Forwarder = Single.NaN;
                this.ForwardingMethod = ForwarderLoadingMethod.None;
                this.WheeledHarvester = Single.NaN;
            }
        }
    }
}
