using System;

namespace Osu.Cof.Ferm.Silviculture
{
    public class LongLogHarvest
    {
        public float FellerBuncherGrappleSwingYarderProcessorLoaderCost { get; set; } // $/ha
        public float FellerBuncherGrappleYoaderProcessorLoaderCost { get; set; } // $/ha
        public float GrappleSwingYarderOverweightFirstLogs { get; set; } // logs/ha
        public float GrappleYoaderOverweightFirstLogs { get; set; } // logs/ha
        public float MinimumCost { get; set; } // $/ha
        public HarvestEquipmentProductionRates Productivity { get; private init; }
        public float TrackedHarvesterGrappleYoaderLoaderCost { get; set; } // $/ha
        public float TrackedHarvesterGrappleSwingYarderLoaderCost { get; set; } // $/ha
        public float WheeledHarvesterGrappleYoaderLoaderCost { get; set; } // $/ha
        public float WheeledHarvesterGrappleSwingYarderLoaderCost { get; set; } // $/ha

        public LongLogHarvest()
        {
            this.FellerBuncherGrappleSwingYarderProcessorLoaderCost = Single.NaN;
            this.FellerBuncherGrappleYoaderProcessorLoaderCost = Single.NaN;
            this.GrappleSwingYarderOverweightFirstLogs = 0.0F;
            this.GrappleYoaderOverweightFirstLogs = 0.0F;
            this.MinimumCost = Single.NaN;
            this.Productivity = new();
            this.TrackedHarvesterGrappleSwingYarderLoaderCost = Single.NaN;
            this.TrackedHarvesterGrappleYoaderLoaderCost = Single.NaN;
            this.WheeledHarvesterGrappleSwingYarderLoaderCost = Single.NaN;
            this.WheeledHarvesterGrappleYoaderLoaderCost = Single.NaN;
        }

        public class HarvestEquipmentProductionRates
        {
            public float FellerBuncher { get; set; } // m³/PMh
            public float GrappleSwingYarder { get; set; } // m³/PMh
            public float GrappleYoader { get; set; } // m³/PMh
            public float ProcessorWithGrappleSwingYarder { get; set; } // m³/PMh
            public float ProcessorWithGrappleYoader { get; set; } // m³/PMh
            public float TrackedHarvester { get; set; } // m³/PMh
            public float WheeledHarvester { get; set; } // m³/PMh

            public HarvestEquipmentProductionRates()
            {
                this.FellerBuncher = Single.NaN;
                this.GrappleSwingYarder = Single.NaN;
                this.GrappleYoader = Single.NaN;
                this.ProcessorWithGrappleSwingYarder = Single.NaN;
                this.ProcessorWithGrappleYoader = Single.NaN;
                this.WheeledHarvester = Single.NaN;
            }
        }
    }
}
