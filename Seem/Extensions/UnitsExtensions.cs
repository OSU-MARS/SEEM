using System;

namespace Mars.Seem.Extensions
{
    internal static class UnitsExtensions
    {
        public static (float diameterToCentimetersMultiplier, float heightToMetersMultiplier, float hectareExpansionFactorMultiplier) GetConversionToMetric(this Units units)
        {
            return units switch
            {
                Units.English => (Constant.CentimetersPerInch, Constant.MetersPerFoot, Constant.AcresPerHectare),
                Units.Metric => (1.0F, 1.0F, 1.0F),
                _ => throw new NotSupportedException("Unhandled units " + units + "."),
            };
        }
    }
}
