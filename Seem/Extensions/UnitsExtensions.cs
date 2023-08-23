using System;

namespace Mars.Seem.Extensions
{
    internal static class UnitsExtensions
    {
        public static (float diameterToCentimetersMultiplier, float heightToMetersMultiplier, float hectareExpansionFactorMultiplier) GetConversionToMetric(this Units units)
        {
            float diameterToCentimetersMultiplier;
            float heightToMetersMultiplier;
            float hectareExpansionFactorMultiplier;
            switch (units)
            {
                case Units.English:
                    diameterToCentimetersMultiplier = Constant.CentimetersPerInch;
                    heightToMetersMultiplier = Constant.MetersPerFoot;
                    hectareExpansionFactorMultiplier = Constant.AcresPerHectare;
                    break;
                case Units.Metric:
                    diameterToCentimetersMultiplier = 1.0F;
                    heightToMetersMultiplier = 1.0F;
                    hectareExpansionFactorMultiplier = 1.0F;
                    break;
                default:
                    throw new NotSupportedException("Unhandled units " + units + ".");
            }

            return (diameterToCentimetersMultiplier, heightToMetersMultiplier, hectareExpansionFactorMultiplier);
        }
    }
}
