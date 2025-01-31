using System;
using System.Runtime.Intrinsics.X86;

namespace Mars.Seem.Extensions
{
    internal static class SimdInstructionsExtensions
    {
        public static SimdInstructions GetDefault()
        {
            // Avx10v1.V512.IsSupported | Avx512F.IsSupported is not currently checked as code changes and recompilation are needed
            if (Avx10v1.IsSupported | Avx512F.VL.IsSupported) // keep in sync with IsSupported()
            {
                return SimdInstructions.Avx10;
            }
            if (Avx.IsSupported)
            {
                return SimdInstructions.Avx;
            }

            return SimdInstructions.Vex128;
        }

        public static int GetWidth(this SimdInstructions instructions)
        {
            return instructions switch
            {
                SimdInstructions.Avx or
                SimdInstructions.Avx10 => 256,
                SimdInstructions.Avx512 => 512,
                SimdInstructions.Vex128 => 128,
                _ => throw new NotSupportedException("Unhandled instruction set " + instructions + "."),
            };
        }

        public static int GetWidth32(this SimdInstructions instructions)
        {
            return instructions.GetWidth() / 32;
        }

        public static bool IsSupported(SimdInstructions instructions)
        {
            // keep in sync with GetDefault()
            // Zen 4 and 5 implement AVX10/256 (and AVX10/512) but lack AVX10 CPUID flags and .NET 9 codegen requires at least the 256 bit versions of
            // GetMantissa(), GetExponent(), and RoundScale() be called through Avx512.VL rather than Avx10v1.
            return instructions switch
            {
                SimdInstructions.Avx or SimdInstructions.Vex128 => Fma.IsSupported,
                SimdInstructions.Avx10 => Avx10v1.IsSupported | Avx512F.VL.IsSupported,
                SimdInstructions.Avx512 => Avx10v1.V512.IsSupported | Avx512F.IsSupported,
                _ => throw new NotSupportedException("Unhandled instruction set " + instructions + ".")
            };
        }
    }
}
