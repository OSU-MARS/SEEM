using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Mars.Seem.Test
{
    public static class AssertV
    {
        public static void IsTrue(Vector128<float> comparison)
        {
            Assert.IsTrue(Avx.MoveMask(comparison) == Constant.Simd128x4.MaskAllTrue);
        }

        public static void IsTrue(Vector128<int> comparison)
        {
            AssertV.IsTrue(Avx.ConvertToVector128Single(comparison));
        }

        public static void IsTrue(Vector256<float> comparison)
        {
            Assert.IsTrue(Avx.MoveMask(comparison) == Constant.Simd256x8.MaskAllTrue);
        }

        public static void IsTrue(Vector256<int> comparison)
        {
            AssertV.IsTrue(Avx.ConvertToVector256Single(comparison));
        }

        // no _mm512_cmpeq_epi32_mask() in .NET 9
        public static void IsTrue(Vector512<int> comparison)
        {
            Assert.IsTrue((Avx.MoveMask(comparison.GetLower().AsSingle()) & Avx.MoveMask(comparison.GetUpper().AsSingle())) == Constant.Simd256x8.MaskAllTrue);
        }
    }
}
