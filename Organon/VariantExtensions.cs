using System;

namespace Osu.Cof.Organon
{
    internal static class VariantExtensions
    {
        public static NotSupportedException CreateUnhandledVariantException(Variant variant)
        {
            return new NotSupportedException(String.Format("Unhandled Organon variant {0}.", variant));
        }
    }
}
