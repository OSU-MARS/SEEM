using System;

namespace Mars.Seem.Extensions
{
    internal static class UnitsExtensions
    {
        public static float GetBasalAreaConversionToMetric(this Units units)
        {
            return units switch
            {
                Units.English => Constant.AcresPerHectare * Constant.SquareMetersPerSquareFoot, // ft²/ac * ac/ha * m²/ft² = m²/ha
                Units.Metric => 1.0F,
                _ => throw new NotSupportedException("Unhandled units " + units + "."),
            };
        }

        public static (float diameterToCentimetersMultiplier, float heightToMetersMultiplier, float hectareExpansionFactorMultiplier) GetConversionToMetric(this Units units)
        {
            return units switch
            {
                Units.English => (Constant.CentimetersPerInch, Constant.MetersPerFoot, Constant.AcresPerHectare),
                Units.Metric => (1.0F, 1.0F, 1.0F),
                _ => throw new NotSupportedException("Unhandled units " + units + "."),
            };
        }

        public static float GetDbhConversionToMetric(this Units units)
        {
            return units switch
            {
                Units.English => Constant.CentimetersPerInch,
                Units.Metric => 1.0F,
                _ => throw new NotSupportedException("Unhandled units " + units + "."),
            };
        }
    }
}
