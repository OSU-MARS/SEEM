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
            return Avx.BroadcastScalarToVector128(&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector256<float> BroadcastScalarToVector256(float value)
        {
            return Avx.BroadcastScalarToVector256(&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<int> Set128(int value)
        {
            // AVX version of Avx2.BroadcastScalarToVector128(&value)
            // Same code as https://github.com/dotnet/runtime/blob/master/src/libraries/System.Private.CoreLib/src/System/Runtime/Intrinsics/Vector128.cs
            // Vector128.Create(int) without the CPU dispatch and signal to compiler that VEX can be used.
            Vector128<int> value128 = Vector128.CreateScalarUnsafe(value); // reinterpet cast without upper zeroing
            return Avx.Shuffle(value128, Constant.Simd128x4.Broadcast0toAll);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<int> Set256(int value)
        {
            // AVX version of Avx2.BroadcastScalarToVector256(&value)
            Vector128<int> value128 = AvxExtensions.Set128(value); // reinterpet cast without upper zeroing
            return Avx.InsertVector128(value128.ToVector256Unsafe(), value128, Constant.Simd256x8.InsertUpper128);
        }
    }
}
