using System;

namespace Osu.Cof.Ferm.Silviculture
{
    public class CutToLengthHarvest : HarvestFinancialValue
    {
        //public float ChainsawBasalAreaPerHaWithTrackedHarvester { get; set; } // m²/ha
        public float ChainsawBasalAreaPerHaWithWheeledHarvester { get; set; } // m²/ha
        //public float ChainsawPMhPerHaTrackedHarvester { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha
        public float ChainsawPMhPerHaWithWheeledHarvester { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha

        public float ForwardedWeightPeHa { get; set; } // kg/ha
        public float ForwarderCostPerHa { get; set; } // US$/ha
        public float ForwarderPMhPerHa { get; set; } // accumulated in delay free minutes/ha and then converted to PMh₀/ha

        // TODO: merge into base class
        public float MerchantableCubicVolumePerHa { get; set; } // m³/ha
        public float MinimumSystemCostPerHa { get; set; } // US$/ha
        public HarvestEquipmentProductivity Productivity { get; private init; }

        //public float TrackedHarvesterCostPerHa { get; set; } // US$/ha
        //public float TrackedHarvesterPMhPerHa { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha
        public float WheeledHarvesterCostPerHa { get; set; } // US$/ha
        public float WheeledHarvesterPMhPerHa { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha

        public CutToLengthHarvest()
        {
            //this.ChainsawBasalAreaPerHaWithTrackedHarvester = 0.0F;
            this.ChainsawBasalAreaPerHaWithWheeledHarvester = 0.0F;
            //this.ChainsawPMhPerHaTrackedHarvester = 0.0F;
            this.ChainsawPMhPerHaWithWheeledHarvester = 0.0F;

            this.ForwardedWeightPeHa = 0.0F;
            this.ForwarderCostPerHa = 0.0F;
            this.ForwarderPMhPerHa = 0.0F;

            this.MerchantableCubicVolumePerHa = 0.0F;
            this.MinimumSystemCostPerHa = Single.NaN;
            this.Productivity = new();
            
            //this.TrackedHarvesterCostPerHa = Single.NaN;
            //this.TrackedHarvesterPMhPerHa = 0.0F;
            this.WheeledHarvesterCostPerHa = Single.NaN;
            this.WheeledHarvesterPMhPerHa = 0.0F;
        }

        public class HarvestEquipmentProductivity
        {
            public float Forwarder { get; set; } // m³/PMh₀
            //public float TrackedHarvester { get; set; } // m³/PMh₀
            public float WheeledHarvester { get; set; } // m³/PMh₀

            public HarvestEquipmentProductivity()
            {
                this.Forwarder = Single.NaN;
                //this.TrackedHarvester = Single.NaN;
                this.WheeledHarvester = Single.NaN;
            }
        }
    }
}
