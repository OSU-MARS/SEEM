using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Mars.Seem.Extensions
{
    internal static class AvxExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector128<float> BroadcastScalarToVector128(float value)
        {
            // could implement this with Avx.BroadcastScalarToVector128(&value) (_mm256_broadcast_ps) but vbroadcastf128 makes a memory thunk
            Vector128<float> value128 = Vector128.CreateScalarUnsafe(value);
            return Avx.Shuffle(value128, value128, Constant.Simd128x4.Broadcast0toAll);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<int> BroadcastScalarToVector128(int value)
        {
            // AVX version of Avx2.BroadcastScalarToVector128(int) (_mm_broadcastd_epi32())
            // Same code as https://github.com/dotnet/runtime/blob/master/src/libraries/System.Private.CoreLib/src/System/Runtime/Intrinsics/Vector128.cs
            // Vector128.Create(int) without the CPU dispatch and signal to compiler that VEX can be used.
            Vector128<int> value128 = Vector128.CreateScalarUnsafe(value); // reinterpet cast without upper zeroing
            return Avx.Shuffle(value128, Constant.Simd128x4.Broadcast0toAll);
        }
    }
}
