using Mars.Seem.Tree;
using System.Collections.Generic;

namespace Mars.Seem.Silviculture
{
    public class LogTable
    {
        public int DiameterClasses { get; private init; }
        public float DiameterClassSizeInCentimeters { get; private init; }

        public float[] LogQmdInCentimetersByPeriod { get; private init; } // average across all species
        public float[] LogsPerHectareByPeriod { get; private init; } // total across all species
        public SortedList<FiaCode, float[,]> LogsPerHectareBySpeciesAndDiameterClass { get; private init; } // diameter class in cm

        public float MaximumDiameterInCentimeters { get; private init; }
        public int Periods { get; private init; }

        public LogTable(int periods, float maximumDiameterInCm, float diameterClassSizeInCm)
        {
            this.DiameterClasses = (int)(maximumDiameterInCm / diameterClassSizeInCm) + 1;
            this.DiameterClassSizeInCentimeters = diameterClassSizeInCm;
            this.LogsPerHectareBySpeciesAndDiameterClass = new();
            this.MaximumDiameterInCentimeters = maximumDiameterInCm;
            this.Periods = periods;

            this.LogQmdInCentimetersByPeriod = new float[periods];
            this.LogsPerHectareByPeriod = new float[periods];
        }

        public float GetDiameter(int diameterClassIndex)
        {
            return this.DiameterClassSizeInCentimeters * (diameterClassIndex + 0.5F);
        }
    }
}
