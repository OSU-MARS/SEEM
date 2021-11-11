using System;

namespace Osu.Cof.Ferm.Silviculture
{
    public class LongLogHarvest
    {
        public float ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleSwingYarder { get; set; } // m²/ha
        public float ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleYoader { get; set; } // m²/ha
        public float ChainsawBasalAreaPerHaWithTrackedHarvester { get; set; } // m²/ha
        public float ChainsawBasalAreaPerHaWithWheeledHarvester { get; set; } // m²/ha
        public float ChainsawPMhPerHaWithFellerBuncherAndGrappleSwingYarder { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha
        public float ChainsawPMhPerHaWithFellerBuncherAndGrappleYoader { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha
        public float ChainsawPMhPerHaTrackedHarvester { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha
        public float ChainsawPMhPerHaWithWheeledHarvester { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha

        public float FellerBuncherGrappleSwingYarderProcessorLoaderCostPerHa { get; set; } // US$/ha
        public float FellerBuncherGrappleYoaderProcessorLoaderCostPerHa { get; set; } // US$/ha
        public float FellerBuncherPMhPerHa { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha
        public float GrappleSwingYarderOverweightFirstLogsPerHa { get; set; } // logs/ha
        public float GrappleYoaderOverweightFirstLogsPerHa { get; set; } // logs/ha

        public float MinimumCostPerHa { get; set; } // US$/ha

        public float ProcessorPMhPerHaWithGrappleSwingYarder { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha
        public float ProcessorPMhPerHaWithGrappleYoader { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha

        public HarvestEquipmentProductivity Productivity { get; private init; }

        public float TotalLoadedWeightPerHa { get; set; } // m³/ha
        public float TotalMerchantableCubicVolumePerHa { get; set; } // m³/ha
        public float TotalYardedWeightPerHaWithFellerBuncher { get; set; } // kg/ha
        public float TotalYardedWeightPerHaWithTrackedHarvester { get; set; } // kg/ha
        public float TotalYardedWeightPerHaWithWheeledHarvester { get; set; } // kg/ha

        public float TrackedHarvesterGrappleYoaderLoaderCostPerHa { get; set; } // US$/ha
        public float TrackedHarvesterGrappleSwingYarderLoaderCostPerHa { get; set; } // US$/ha
        public float TrackedHarvesterPMhPerHa { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha
        public float WheeledHarvesterGrappleYoaderLoaderCostPerHa { get; set; } // US$/ha
        public float WheeledHarvesterGrappleSwingYarderLoaderCostPerHa { get; set; } // US$/ha
        public float WheeledHarvesterPMhPerHa { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha

        public LongLogHarvest()
        {
            this.ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleSwingYarder = 0.0F;
            this.ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleYoader = 0.0F;
            this.ChainsawBasalAreaPerHaWithTrackedHarvester = 0.0F;
            this.ChainsawBasalAreaPerHaWithWheeledHarvester = 0.0F;
            this.ChainsawPMhPerHaWithFellerBuncherAndGrappleSwingYarder = 0.0F;
            this.ChainsawPMhPerHaWithFellerBuncherAndGrappleYoader = 0.0F;
            this.ChainsawPMhPerHaTrackedHarvester = 0.0F;
            this.ChainsawPMhPerHaWithWheeledHarvester = 0.0F;

            this.FellerBuncherGrappleSwingYarderProcessorLoaderCostPerHa = Single.NaN;
            this.FellerBuncherGrappleYoaderProcessorLoaderCostPerHa = Single.NaN;
            this.FellerBuncherPMhPerHa = 0.0F;
            this.GrappleSwingYarderOverweightFirstLogsPerHa = 0.0F;
            this.GrappleYoaderOverweightFirstLogsPerHa = 0.0F;
            
            this.MinimumCostPerHa = Single.NaN;

            this.ProcessorPMhPerHaWithGrappleSwingYarder = 0.0F;
            this.ProcessorPMhPerHaWithGrappleYoader = 0.0F;

            this.Productivity = new();

            this.TotalLoadedWeightPerHa = 0.0F;
            this.TotalMerchantableCubicVolumePerHa = 0.0F;
            this.TotalYardedWeightPerHaWithFellerBuncher = 0.0F;
            this.TotalYardedWeightPerHaWithTrackedHarvester = 0.0F;
            this.TotalYardedWeightPerHaWithWheeledHarvester = 0.0F;

            this.TrackedHarvesterGrappleSwingYarderLoaderCostPerHa = Single.NaN;
            this.TrackedHarvesterGrappleYoaderLoaderCostPerHa = Single.NaN;
            this.TrackedHarvesterPMhPerHa = 0.0F;
            this.WheeledHarvesterGrappleSwingYarderLoaderCostPerHa = Single.NaN;
            this.WheeledHarvesterGrappleYoaderLoaderCostPerHa = Single.NaN;
            this.WheeledHarvesterPMhPerHa = 0.0F;
        }

        public class HarvestEquipmentProductivity
        {
            // TODO: chainsaw
            public float FellerBuncher { get; set; } // m³/PMh₀
            public float GrappleSwingYarder { get; set; } // m³/PMh₀
            public float GrappleYoader { get; set; } // m³/PMh₀
            // log loader productivity is specified
            public float ProcessorWithGrappleSwingYarder { get; set; } // m³/PMh₀
            public float ProcessorWithGrappleYoader { get; set; } // m³/PMh₀
            public float TrackedHarvester { get; set; } // m³/PMh₀
            public float WheeledHarvester { get; set; } // m³/PMh₀

            public HarvestEquipmentProductivity()
            {
                this.FellerBuncher = Single.NaN;
                this.GrappleSwingYarder = Single.NaN;
                this.GrappleYoader = Single.NaN;
                this.ProcessorWithGrappleSwingYarder = Single.NaN;
                this.ProcessorWithGrappleYoader = Single.NaN;
                this.TrackedHarvester = Single.NaN;
                this.WheeledHarvester = Single.NaN;
            }
        }
    }
}
