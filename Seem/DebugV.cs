using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Osu.Cof.Ferm
{
    internal static class DebugV
    {
        [Conditional("DEBUG")]
        public static void Assert(Vector128<float> condition)
        {
            Debug.Assert(Avx.MoveMask(condition) == Constant.Simd128x4.MaskAllTrue);
        }

        [Conditional("DEBUG")]
        public static void Assert(Vector128<int> condition)
        {
            Debug.Assert(Avx.MoveMask(condition.AsSingle()) == Constant.Simd128x4.MaskAllTrue);
        }

        [Conditional("DEBUG")]
        public static void Assert(Vector128<float> condition, string message)
        {
            Debug.Assert(Avx.MoveMask(condition) == Constant.Simd128x4.MaskAllTrue, message);
        }
    }
}
