using System;

namespace Mars.Seem.Extensions
{
    internal static class UnitsExtensions
    {
        public static (float diameterToCentimetersMultiplier, float heightToMetersMultiplier, float hectareExpansionFactorMultiplier) GetConversionToMetric(this Units units)
        {
            switch (units)
            {
                case Units.English:
                    return (Constant.CentimetersPerInch, Constant.MetersPerFoot,  Constant.AcresPerHectare);
                case Units.Metric:
                    return (1.0F, 1.0F, 1.0F);
                default:
                    throw new NotSupportedException("Unhandled units " + units + ".");
            }
        }
    }
}
