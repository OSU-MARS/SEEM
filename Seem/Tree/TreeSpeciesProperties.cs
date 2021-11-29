using System;
using System.Diagnostics;

namespace Mars.Seem.Tree
{
    public struct TreeSpeciesProperties
    {
        public float BarkDensity { get; private init; } // kg/m³, green
        public float BarkFraction { get; private init; } // fraction of total stem volume
        public float BarkFractionAfterHarvester { get; private init; } // fraction of total stem volume
        public float BarkFractionAfterYardingAndProcessing { get; private init; } // fraction of total stem volume after from processing head and all of yarding distance
        public float BarkFractionAtMidspanAfterHarvester { get; private init; } // fraction of total stem volume after loss from processing head and half of yarding distance
        public float BarkFractionAtMidspanWithoutHarvester { get; private init; } // fraction of total stem volume after half of cable yarding loss only (no losses from felling or chainsaw bucking)
        public float ProcessingBarkLoss { get; private init; } // fraction of bark
        public float StemDensity { get; private init; } // kg/m³ with all bark present
        public float StemDensityAfterHarvester { get; private init; } // kg/m³ with only processing bark loss
        public float StemDensityAfterYardingAndProcessing { get; private init; } // kg/m³ with only processing and all of yarding bark loss
        public float StemDensityAtMidspanAfterHarvester { get; private init; } // kg/m³ with processing and half of yarding bark loss
        public float StemDensityAtMidspanWithoutHarvester { get; private init; } // kg/m³ with half of yarding bark loss
        public float WoodDensity { get; private init; } // kg/m³, green
        public float YardingBarkLoss { get; private init; } // fraction of bark

        public TreeSpeciesProperties(float woodDensity, float barkFraction, float barkDensity, float processingBarkLoss, float yardingBarkLoss)
        {
            if ((woodDensity <= 50.0F) || (woodDensity > 1500.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(woodDensity));
            }
            if ((barkFraction <= 0.0F) || (barkFraction >= 0.3F))
            {
                throw new ArgumentOutOfRangeException(nameof(barkFraction));
            }
            if ((barkDensity <= 50.0F) || (barkDensity > 1500.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(barkDensity));
            }
            if ((processingBarkLoss < 0.0F) || (processingBarkLoss > 0.5F))
            {
                throw new ArgumentOutOfRangeException(nameof(processingBarkLoss));
            }
            if ((yardingBarkLoss < 0.0F) || (yardingBarkLoss > 0.5F))
            {
                throw new ArgumentOutOfRangeException(nameof(yardingBarkLoss));
            }

            // specified properties
            this.BarkDensity = barkDensity;
            this.BarkFraction = barkFraction;
            this.ProcessingBarkLoss = processingBarkLoss;
            this.WoodDensity = woodDensity;
            this.YardingBarkLoss = yardingBarkLoss;

            // unmodified properties
            this.StemDensity = (1.0F - this.BarkFraction) * this.WoodDensity + this.BarkFraction * this.BarkDensity;

            // after felled and bucked by harvester in cut to length system
            this.BarkFractionAfterHarvester = (1.0F - this.ProcessingBarkLoss) * this.BarkFraction;
            this.StemDensityAfterHarvester = (1.0F - this.BarkFractionAfterHarvester) * this.WoodDensity + this.BarkFractionAfterHarvester * this.BarkDensity;

            // after felled and bucked by harvester in whole tree yarding
            this.BarkFractionAtMidspanAfterHarvester = (1.0F - this.ProcessingBarkLoss) * (1.0F - 0.5F * this.YardingBarkLoss) * this.BarkFraction;
            this.StemDensityAtMidspanAfterHarvester = (1.0F - this.BarkFractionAtMidspanAfterHarvester) * this.WoodDensity + this.BarkFractionAtMidspanAfterHarvester * this.BarkDensity;

            // after felled by chainsaw or feller-buncher in whole tree yarding or bucked to length by chainsaw
            this.BarkFractionAtMidspanWithoutHarvester = (1.0F - 0.5F * this.YardingBarkLoss) * this.BarkFraction;
            this.StemDensityAtMidspanWithoutHarvester = (1.0F - this.BarkFractionAtMidspanWithoutHarvester) * this.WoodDensity + this.BarkFractionAtMidspanWithoutHarvester * this.BarkDensity;

            // after either 1) harvesting and yarding or 2) yarding and processing
            this.BarkFractionAfterYardingAndProcessing = (1.0F - this.ProcessingBarkLoss) * (1.0F - this.YardingBarkLoss) * this.BarkFraction;
            this.StemDensityAfterYardingAndProcessing = (1.0F - this.BarkFractionAfterYardingAndProcessing) * this.WoodDensity + this.BarkFractionAfterYardingAndProcessing * this.BarkDensity;

            // sanity checks, assuming bark density is greater than wood density
            Debug.Assert((this.BarkFractionAfterYardingAndProcessing > 0.0F) && 
                         (this.BarkFractionAfterHarvester > this.BarkFractionAfterYardingAndProcessing) && 
                         (this.BarkFractionAtMidspanWithoutHarvester > this.BarkFractionAfterHarvester) && 
                         (this.BarkFractionAtMidspanWithoutHarvester < this.BarkFraction) &&
                         (this.StemDensityAfterYardingAndProcessing > this.WoodDensity) &&
                         (this.StemDensityAfterHarvester > this.StemDensityAfterYardingAndProcessing) && 
                         (this.StemDensityAtMidspanWithoutHarvester > this.StemDensityAfterHarvester) &&
                         (this.StemDensity > this.StemDensityAtMidspanWithoutHarvester) && 
                         (this.StemDensity < this.BarkDensity));
        }
    }
}
