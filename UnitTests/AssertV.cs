using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Osu.Cof.Ferm.Test
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
    }
}
