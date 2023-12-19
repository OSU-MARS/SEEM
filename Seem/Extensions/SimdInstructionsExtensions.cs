using System;
using System.Runtime.Intrinsics.X86;

namespace Mars.Seem.Extensions
{
    internal static class SimdInstructionsExtensions
    {
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
