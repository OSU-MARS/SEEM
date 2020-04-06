using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Osu.Cof.Organon
{
    internal static class AvxExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector128<float> BroadcastScalarToVector128(float value)
        {
            return Avx.BroadcastScalarToVector128(&value);
        }
    }
}
