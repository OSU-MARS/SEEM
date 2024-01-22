using System;
using System.Runtime.Intrinsics.X86;

namespace Mars.Seem.Extensions
{
    internal static class SimdInstructionsExtensions
    {
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
            return instructions switch
            {
                SimdInstructions.Avx or SimdInstructions.Vex128 => Fma.IsSupported,
                SimdInstructions.Avx10 => Avx512F.VL.IsSupported, // absent explicit support for AVX10/256 in .NET 8
                SimdInstructions.Avx512 => Avx512F.IsSupported,
                _ => throw new NotSupportedException("Unhandled instruction set " + instructions + "."),
            };
        }
    }
}
