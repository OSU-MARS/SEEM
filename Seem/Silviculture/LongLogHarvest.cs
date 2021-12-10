using System;

namespace Mars.Seem.Silviculture
{
    public class LongLogHarvest : HarvestFinancialValue
    {
        public float ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleSwingYarder { get; set; } // m²/ha
        public float ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleYoader { get; set; } // m²/ha
        public float ChainsawBasalAreaPerHaWithTrackedHarvester { get; set; } // m²/ha
        public float ChainsawBasalAreaPerHaWithWheeledHarvester { get; set; } // m²/ha
        public ChainsawCrewType ChainsawCrewWithFellerBuncherAndGrappleSwingYarder { get; set; }
        public ChainsawCrewType ChainsawCrewWithFellerBuncherAndGrappleYoader { get; set; }
        public ChainsawCrewType ChainsawCrewWithTrackedHarvester { get; set; }
        public ChainsawCrewType ChainsawCrewWithWheeledHarvester { get; set; }
        public float ChainsawCubicVolumePerHaWithFellerBuncherAndGrappleSwingYarder { get; set; }
        public float ChainsawCubicVolumePerHaWithFellerBuncherAndGrappleYoader { get; set; }
        public float ChainsawCubicVolumePerHaWithTrackedHarvester { get; set; }
        public float ChainsawCubicVolumePerHaWithWheeledHarvester { get; set; }
        public float ChainsawPMhPerHaWithFellerBuncherAndGrappleSwingYarder { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha
        public float ChainsawPMhPerHaWithFellerBuncherAndGrappleYoader { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha
        public float ChainsawPMhPerHaWithTrackedHarvester { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha
        public float ChainsawPMhPerHaWithWheeledHarvester { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha

        public float FellerBuncherGrappleSwingYarderProcessorLoaderCostPerHa { get; set; } // US$/ha
        public float FellerBuncherGrappleYoaderProcessorLoaderCostPerHa { get; set; } // US$/ha
        public float FellerBuncherPMhPerHa { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha
        public float GrappleSwingYarderOverweightFirstLogsPerHa { get; set; } // logs/ha
        public float GrappleSwingYarderPMhPerHectare { get; set; } // PMh₀/ha
        public float GrappleYoaderOverweightFirstLogsPerHa { get; set; } // logs/ha
        public float GrappleYoaderPMhPerHectare { get; set; } // PMh₀/ha

        public float LoadedWeightPerHa { get; set; } // m³/ha
        // TODO: merge into base class
        public float MerchantableCubicVolumePerHa { get; set; } // m³/ha
        public float MinimumSystemCostPerHa { get; set; } // US$/ha

        public float ProcessorPMhPerHaWithGrappleSwingYarder { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha
        public float ProcessorPMhPerHaWithGrappleYoader { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha

        public HarvestEquipmentProductivity Productivity { get; private init; }

        public float TrackedHarvesterGrappleYoaderLoaderCostPerHa { get; set; } // US$/ha
        public float TrackedHarvesterGrappleSwingYarderLoaderCostPerHa { get; set; } // US$/ha
        public float TrackedHarvesterPMhPerHa { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha
        public float WheeledHarvesterGrappleYoaderLoaderCostPerHa { get; set; } // US$/ha
        public float WheeledHarvesterGrappleSwingYarderLoaderCostPerHa { get; set; } // US$/ha
        public float WheeledHarvesterPMhPerHa { get; set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha

        // yarded weight with feller buncher (plus chainsaw) should be highest as bark loss is lowest
        // yarded weight with harvesters depends on bark loss from processing by the harvester versus chainsaw bucking
        public float YardedWeightPerHaWithFellerBuncher { get; set; } // kg/ha
        public float YardedWeightPerHaWithTrackedHarvester { get; set; } // kg/ha
        public float YardedWeightPerHaWithWheeledHarvester { get; set; } // kg/ha

        public LongLogHarvest()
        {
            this.ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleSwingYarder = 0.0F;
            this.ChainsawBasalAreaPerHaWithFellerBuncherAndGrappleYoader = 0.0F;
            this.ChainsawBasalAreaPerHaWithTrackedHarvester = 0.0F;
            this.ChainsawBasalAreaPerHaWithWheeledHarvester = 0.0F;
            this.ChainsawCrewWithFellerBuncherAndGrappleSwingYarder = ChainsawCrewType.None;
            this.ChainsawCrewWithFellerBuncherAndGrappleYoader = ChainsawCrewType.None;
            this.ChainsawCrewWithTrackedHarvester = ChainsawCrewType.None;
            this.ChainsawCrewWithWheeledHarvester = ChainsawCrewType.None;
            this.ChainsawCubicVolumePerHaWithFellerBuncherAndGrappleSwingYarder = 0.0F;
            this.ChainsawCubicVolumePerHaWithFellerBuncherAndGrappleYoader = 0.0F;
            this.ChainsawCubicVolumePerHaWithTrackedHarvester = 0.0F;
            this.ChainsawCubicVolumePerHaWithWheeledHarvester = 0.0F;
            this.ChainsawPMhPerHaWithFellerBuncherAndGrappleSwingYarder = 0.0F;
            this.ChainsawPMhPerHaWithFellerBuncherAndGrappleYoader = 0.0F;
            this.ChainsawPMhPerHaWithTrackedHarvester = 0.0F;
            this.ChainsawPMhPerHaWithWheeledHarvester = 0.0F;

            this.FellerBuncherGrappleSwingYarderProcessorLoaderCostPerHa = Single.NaN;
            this.FellerBuncherGrappleYoaderProcessorLoaderCostPerHa = Single.NaN;
            this.FellerBuncherPMhPerHa = 0.0F;
            this.GrappleSwingYarderOverweightFirstLogsPerHa = 0.0F;
            this.GrappleSwingYarderPMhPerHectare = 0.0F;
            this.GrappleYoaderOverweightFirstLogsPerHa = 0.0F;
            this.GrappleYoaderPMhPerHectare = 0.0F;

            this.LoadedWeightPerHa = 0.0F;
            this.MerchantableCubicVolumePerHa = 0.0F;
            this.MinimumSystemCostPerHa = Single.NaN;

            this.ProcessorPMhPerHaWithGrappleSwingYarder = 0.0F;
            this.ProcessorPMhPerHaWithGrappleYoader = 0.0F;

            this.Productivity = new();

            this.TrackedHarvesterGrappleSwingYarderLoaderCostPerHa = Single.NaN;
            this.TrackedHarvesterGrappleYoaderLoaderCostPerHa = Single.NaN;
            this.TrackedHarvesterPMhPerHa = 0.0F;
            this.WheeledHarvesterGrappleSwingYarderLoaderCostPerHa = Single.NaN;
            this.WheeledHarvesterGrappleYoaderLoaderCostPerHa = Single.NaN;
            this.WheeledHarvesterPMhPerHa = 0.0F;

            this.YardedWeightPerHaWithFellerBuncher = 0.0F;
            this.YardedWeightPerHaWithTrackedHarvester = 0.0F;
            this.YardedWeightPerHaWithWheeledHarvester = 0.0F;
        }

        public HarvestSystem GetMinimumCostHarvestSystem()
        {
            if (this.FellerBuncherGrappleSwingYarderProcessorLoaderCostPerHa == this.MinimumSystemCostPerHa)
            {
                return HarvestSystem.FellerBuncherGrappleSwingYarderProcessorLoader;
            }
            if (this.FellerBuncherGrappleYoaderProcessorLoaderCostPerHa == this.MinimumSystemCostPerHa)
            {
                return HarvestSystem.FellerBuncherGrappleYoaderProcessorLoader;
            }
            if (this.TrackedHarvesterGrappleSwingYarderLoaderCostPerHa == this.MinimumSystemCostPerHa)
            {
                return HarvestSystem.TrackedHarvesterGrappleSwingYarderLoader;
            }
            if (this.TrackedHarvesterGrappleYoaderLoaderCostPerHa == this.MinimumSystemCostPerHa)
            {
                return HarvestSystem.TrackedHarvesterGrappleYoaderLoader;
            }
            if (this.WheeledHarvesterGrappleSwingYarderLoaderCostPerHa == this.MinimumSystemCostPerHa)
            {
                return HarvestSystem.WheeledHarvesterGrappleSwingYarderLoader;
            }
            if (this.WheeledHarvesterGrappleYoaderLoaderCostPerHa == this.MinimumSystemCostPerHa)
            {
                return HarvestSystem.WheeledHarvesterGrappleYoaderLoader;
            }

            throw new InvalidOperationException("Minimum harvest cost per hectare does not match any harvest system.");
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
