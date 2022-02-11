using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Mars.Seem.Extensions
{
    public class MathV
    {
        private const float Exp10ToExp2 = 3.321928094887362F; // log2(10)
        private const float Exp2Beta1 = 0.6931176823F;
        private const float Exp2Beta2 = 0.2402214511F;
        private const float Exp2Beta3 = 0.0559562089F;
        private const float Exp2Beta4 = 0.0096779180F;
        private const float ExpToExp2 = 1.442695040888963F; // log2(e)

        public const float ExpToZeroThreshold = -16.2F; // truncate exponents less than 1E-7 to zero

        private const int FloatExponentMask = 0x7F800000;
        private const int FloatMantissaBits = 23;
        private const int FloatMantissaZero = 127;
        private const int FloatMantissaMask = 0x007FFFFF;
        private const float FloatMaximumPower = 127.0F;

        private const float Log2Beta1 = 1.441814292091611F;
        private const float Log2Beta2 = -0.708440969761796F;
        private const float Log2Beta3 = 0.414281442395441F;
        private const float Log2Beta4 = -0.192544768195605F;
        private const float Log2Beta5 = 0.044890234549254F;
        private const float Log2ToNaturalLog = 0.693147180559945F;
        private const float Log2ToLog10 = 0.301029995663981F;

        private const float One = 1.0F;
        private const int OneAsInt = 0x3f800000; // 0x3f800000 = 1.0F

        // memory accesses for Avx2.BroadcastScalarToVector128() are awkward
        // Therefore, hoist shifts for SIMD 128 and 256 bit assembly and accept increased coefficient load bandwidth.
        private static readonly Vector128<int> FloatExponentMask128;
        private static readonly Vector256<int> FloatExponentMask256;
        private static readonly Vector128<int> FloatMantissaMask128;
        private static readonly Vector256<int> FloatMantissaMask256;
        private static readonly Vector128<int> FloatMantissaZero128;
        private static readonly Vector256<int> FloatMantissaZero256;

        static MathV()
        {
            MathV.FloatExponentMask128 = AvxExtensions.BroadcastScalarToVector128(MathV.FloatExponentMask);
            MathV.FloatExponentMask256 = AvxExtensions.BroadcastScalarToVector256(MathV.FloatExponentMask);
            MathV.FloatMantissaMask128 = AvxExtensions.BroadcastScalarToVector128(MathV.FloatMantissaMask);
            MathV.FloatMantissaMask256 = AvxExtensions.BroadcastScalarToVector256(MathV.FloatMantissaMask);
            MathV.FloatMantissaZero128 = AvxExtensions.BroadcastScalarToVector128(MathV.FloatMantissaZero);
            MathV.FloatMantissaZero256 = AvxExtensions.BroadcastScalarToVector256(MathV.FloatMantissaZero);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Exp(float power)
        {
            return MathV.Exp2(MathV.ExpToExp2 * power);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector128<float> Exp(Vector128<float> power)
        {
            return MathV.Exp2(Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(MathV.ExpToExp2), power));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector256<float> Exp(Vector256<float> power)
        {
            return MathV.Exp2(Avx.Multiply(AvxExtensions.BroadcastScalarToVector256(MathV.ExpToExp2), power));
        }

        /// <summary>
        /// Base 2 exponent using IEEE 754 decomposition and 4th order polynomial approximation.
        /// </summary>
        /// <param name="power">Power of 2 to calculate.</param>
        /// <returns>2^(power)</returns>
        /// <remarks>Relative error <5 ppm. ~1.4x faster than MathF.Exp().</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Exp2(float power)
        {
            if (MathF.Abs(power) > MathV.FloatMaximumPower)
            {
                throw new ArgumentOutOfRangeException(nameof(power));
            }

            // R for polynomial generation
            // exponent2 = data.frame(x = seq(-0.536, 0.536, by = 0.001))
            // exponent2$exp = 2 ^ (exponent2$x)
            // exponentFit = lm(exp ~x + I(x ^ 2) + I(x ^ 3) + I(x ^ 4) + I(x ^ 5), data = exponent2)
            float integerPart = MathF.Round(power);
            float x = power - integerPart; // fractional part
            float integerExponent = BitConverter.Int32BitsToSingle(((int)integerPart + MathV.FloatMantissaZero) << MathV.FloatMantissaBits);
            float fractionalExponent = MathV.One + MathV.Exp2Beta1 * x + MathV.Exp2Beta2 * x * x + MathV.Exp2Beta3 * x * x * x + MathV.Exp2Beta4 * x * x * x * x;
            return integerExponent * fractionalExponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector128<float> Exp2(Vector128<float> power)
        {
            Debug.Assert(Avx.MoveMask(Avx.And(Avx.CompareGreaterThan(power, AvxExtensions.BroadcastScalarToVector128(MathV.FloatMaximumPower)), Avx.CompareOrdered(power, power))) == 0);

            byte zeroMask = (byte)Avx.MoveMask(Avx.CompareLessThan(power, AvxExtensions.BroadcastScalarToVector128(-MathV.FloatMaximumPower)));
            Vector128<float> integerPart = Avx.RoundToNearestInteger(power);
            Vector128<float> integerExponent = Avx.ShiftLeftLogical(Avx.Add(Avx.ConvertToVector128Int32(integerPart), MathV.FloatMantissaZero128), MathV.FloatMantissaBits).AsSingle();

            // evaluate polynomial
            Vector128<float> beta1 = AvxExtensions.BroadcastScalarToVector128(MathV.Exp2Beta1);
            Vector128<float> beta2 = AvxExtensions.BroadcastScalarToVector128(MathV.Exp2Beta2);
            Vector128<float> beta3 = AvxExtensions.BroadcastScalarToVector128(MathV.Exp2Beta3);
            Vector128<float> beta4 = AvxExtensions.BroadcastScalarToVector128(MathV.Exp2Beta4);

            Vector128<float> x = Avx.Subtract(power, integerPart); // fractional part
            Vector128<float> fractionalExponent = AvxExtensions.BroadcastScalarToVector128(MathV.One);
            fractionalExponent = Avx.Add(fractionalExponent, Avx.Multiply(beta1, x));
            Vector128<float> x2 = Avx.Multiply(x, x);
            fractionalExponent = Avx.Add(fractionalExponent, Avx.Multiply(beta2, x2));
            Vector128<float> x3 = Avx.Multiply(x2, x);
            fractionalExponent = Avx.Add(fractionalExponent, Avx.Multiply(beta3, x3));
            Vector128<float> x4 = Avx.Multiply(x3, x);
            fractionalExponent = Avx.Add(fractionalExponent, Avx.Multiply(beta4, x4));

            // form exponent
            Vector128<float> exponent = Avx.Multiply(integerExponent, fractionalExponent);

            // suppress exponent overflows by truncating values less than 2^-127 to zero
            if (zeroMask != 0)
            {
                exponent = Avx.Blend(exponent, Vector128<float>.Zero, zeroMask);
            }
            return exponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector256<float> Exp2(Vector256<float> power)
        {
            Debug.Assert(Avx.MoveMask(Avx.And(Avx.CompareGreaterThan(power, AvxExtensions.BroadcastScalarToVector256(MathV.FloatMaximumPower)), Avx.CompareOrdered(power, power))) == 0);

            byte zeroMask = (byte)Avx.MoveMask(Avx.CompareLessThan(power, AvxExtensions.BroadcastScalarToVector256(-MathV.FloatMaximumPower)));
            Vector256<float> integerPart = Avx.RoundToNearestInteger(power);
            Vector256<float> integerExponent = Avx2.ShiftLeftLogical(Avx2.Add(Avx.ConvertToVector256Int32(integerPart), MathV.FloatMantissaZero256), MathV.FloatMantissaBits).AsSingle();

            // evaluate polynomial
            Vector256<float> beta1 = AvxExtensions.BroadcastScalarToVector256(MathV.Exp2Beta1);
            Vector256<float> beta2 = AvxExtensions.BroadcastScalarToVector256(MathV.Exp2Beta2);
            Vector256<float> beta3 = AvxExtensions.BroadcastScalarToVector256(MathV.Exp2Beta3);
            Vector256<float> beta4 = AvxExtensions.BroadcastScalarToVector256(MathV.Exp2Beta4);

            Vector256<float> x = Avx.Subtract(power, integerPart); // fractional part
            Vector256<float> fractionalExponent = AvxExtensions.BroadcastScalarToVector256(MathV.One);
            fractionalExponent = Avx.Add(fractionalExponent, Avx.Multiply(beta1, x));
            Vector256<float> x2 = Avx.Multiply(x, x);
            fractionalExponent = Avx.Add(fractionalExponent, Avx.Multiply(beta2, x2));
            Vector256<float> x3 = Avx.Multiply(x2, x);
            fractionalExponent = Avx.Add(fractionalExponent, Avx.Multiply(beta3, x3));
            Vector256<float> x4 = Avx.Multiply(x3, x);
            fractionalExponent = Avx.Add(fractionalExponent, Avx.Multiply(beta4, x4));

            // form exponent
            Vector256<float> exponent = Avx.Multiply(integerExponent, fractionalExponent);

            // suppress exponent overflows by truncating values less than 2^-127 to zero
            if (zeroMask != 0)
            {
                exponent = Avx.Blend(exponent, Vector256<float>.Zero, zeroMask);
            }
            return exponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Exp10(float power)
        {
            return MathV.Exp2(MathV.Exp10ToExp2 * power);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector128<float> Exp10(Vector128<float> power)
        {
            return MathV.Exp2(Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(MathV.Exp10ToExp2), power));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector256<float> Exp10(Vector256<float> power)
        {
            return MathV.Exp2(Avx.Multiply(AvxExtensions.BroadcastScalarToVector256(MathV.Exp10ToExp2), power));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ExpWithTruncationToZero(float power)
        {
            if (power < MathV.ExpToZeroThreshold)
            {
                return 0.0F;
            }
            return MathV.Exp(power);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Ln(float value)
        {
            return MathV.Log2ToNaturalLog * MathV.Log2(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector128<float> Ln(Vector128<float> value)
        {
            return Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(MathV.Log2ToNaturalLog), MathV.Log2(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector256<float> Ln(Vector256<float> value)
        {
            return Avx.Multiply(AvxExtensions.BroadcastScalarToVector256(MathV.Log2ToNaturalLog), MathV.Log2(value));
        }

        /// <summary>
        /// Base 2 logarithm using IEEE 754 decompostion and 5th order approximation
        /// </summary>
        /// <param name="value">Value to take logarithm of.</param>
        /// <returns>Base 2 logarithm of value.</returns>
        /// <remarks>Relative error <2 ppm. ~1.4x faster than MathF.Log()</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Log2(float value)
        {
            // R for polynomial generation: fitting against x - 1 improves overall accuracy minimizes error for small logarithms of values near 1
            // logBase2 = data.frame(x = seq(1, 2.041, by = 0.001))
            // logBase2$log = log2(logBase2$x)
            // logFit = lm(log ~0 + I(x - 1) + I((x - 1) ^ 2) + I((x - 1) ^ 3) + I((x - 1) ^ 4) + I((x - 1) ^ 5), data = logBase2, weights = seq(3, 1, length.out = nrow(logBase2)))
            int integerValue = BitConverter.SingleToInt32Bits(value);
            float exponent = (float)(((integerValue & MathV.FloatExponentMask) >> MathV.FloatMantissaBits) - MathV.FloatMantissaZero);
            float mantissa = BitConverter.Int32BitsToSingle((integerValue & MathV.FloatMantissaMask) | MathV.OneAsInt);
            float x = mantissa - 1;
            float polynomial = MathV.Log2Beta1 * x + MathV.Log2Beta2 * x * x + MathV.Log2Beta3 * x * x * x + MathV.Log2Beta4 * x * x * x * x + MathV.Log2Beta5 * x * x * x * x * x;
            return exponent + polynomial;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector128<float> Log2(Vector128<float> value)
        {
            // split value into exponent and mantissa parts
            Vector128<float> one = AvxExtensions.BroadcastScalarToVector128(MathV.One);

            Vector128<int> integerValue = value.AsInt32();
            Vector128<float> exponent = Avx.ConvertToVector128Single(Avx.Subtract(Avx.ShiftRightLogical(Avx.And(integerValue, MathV.FloatExponentMask128),
                                                                                                        MathV.FloatMantissaBits),
                                                                                  MathV.FloatMantissaZero128));
            Vector128<float> mantissa = Avx.Or(Avx.And(integerValue, MathV.FloatMantissaMask128).AsSingle(), one);

            // evaluate mantissa polynomial
            Vector128<float> beta1 = AvxExtensions.BroadcastScalarToVector128(MathV.Log2Beta1);
            Vector128<float> beta2 = AvxExtensions.BroadcastScalarToVector128(MathV.Log2Beta2);
            Vector128<float> beta3 = AvxExtensions.BroadcastScalarToVector128(MathV.Log2Beta3);
            Vector128<float> beta4 = AvxExtensions.BroadcastScalarToVector128(MathV.Log2Beta4);
            Vector128<float> beta5 = AvxExtensions.BroadcastScalarToVector128(MathV.Log2Beta5);

            Vector128<float> x = Avx.Subtract(mantissa, one);
            Vector128<float> polynomial = Avx.Multiply(beta1, x);
            Vector128<float> x2 = Avx.Multiply(x, x);
            polynomial = Avx.Add(polynomial, Avx.Multiply(beta2, x2));
            Vector128<float> x3 = Avx.Multiply(x2, x);
            polynomial = Avx.Add(polynomial, Avx.Multiply(beta3, x3));
            Vector128<float> x4 = Avx.Multiply(x3, x);
            polynomial = Avx.Add(polynomial, Avx.Multiply(beta4, x4));
            Vector128<float> x5 = Avx.Multiply(x4, x);
            polynomial = Avx.Add(polynomial, Avx.Multiply(beta5, x5));

            // form logarithm
            return Avx.Add(exponent, polynomial);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector256<float> Log2(Vector256<float> value)
        {
            // split value into exponent and mantissa parts
            Vector256<float> one = AvxExtensions.BroadcastScalarToVector256(MathV.One);

            Vector256<int> integerValue = value.AsInt32();
            Vector256<float> exponent = Avx.ConvertToVector256Single(Avx2.Subtract(Avx2.ShiftRightLogical(Avx2.And(integerValue, MathV.FloatExponentMask256),
                                                                                                          MathV.FloatMantissaBits),
                                                                                   MathV.FloatMantissaZero256));
            Vector256<float> mantissa = Avx.Or(Avx2.And(integerValue, MathV.FloatMantissaMask256).AsSingle(), one);

            // evaluate mantissa polynomial
            Vector256<float> beta1 = AvxExtensions.BroadcastScalarToVector256(MathV.Log2Beta1);
            Vector256<float> beta2 = AvxExtensions.BroadcastScalarToVector256(MathV.Log2Beta2);
            Vector256<float> beta3 = AvxExtensions.BroadcastScalarToVector256(MathV.Log2Beta3);
            Vector256<float> beta4 = AvxExtensions.BroadcastScalarToVector256(MathV.Log2Beta4);
            Vector256<float> beta5 = AvxExtensions.BroadcastScalarToVector256(MathV.Log2Beta5);

            Vector256<float> x = Avx.Subtract(mantissa, one);
            Vector256<float> polynomial = Avx.Multiply(beta1, x);
            Vector256<float> x2 = Avx.Multiply(x, x);
            polynomial = Avx.Add(polynomial, Avx.Multiply(beta2, x2));
            Vector256<float> x3 = Avx.Multiply(x2, x);
            polynomial = Avx.Add(polynomial, Avx.Multiply(beta3, x3));
            Vector256<float> x4 = Avx.Multiply(x3, x);
            polynomial = Avx.Add(polynomial, Avx.Multiply(beta4, x4));
            Vector256<float> x5 = Avx.Multiply(x4, x);
            polynomial = Avx.Add(polynomial, Avx.Multiply(beta5, x5));

            // form logarithm
            return Avx.Add(exponent, polynomial);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Log10(float value)
        {
            return MathV.Log2ToLog10 * MathV.Log2(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector128<float> Log10(Vector128<float> value)
        {
            return Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(MathV.Log2ToLog10), MathV.Log2(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static Vector256<float> Log10(Vector256<float> value)
        {
            return Avx.Multiply(AvxExtensions.BroadcastScalarToVector256(MathV.Log2ToLog10), MathV.Log2(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> MaskExp(Vector128<float> power, byte exponentMask)
        {
            // if corresponding bit in mask is set then 1.0 is used instead of power in restrictedPower to avoid math errors
            // Uses lower 4 bits of mask.
            Vector128<float> restrictedPower = Avx.Blend(power, AvxExtensions.BroadcastScalarToVector128(1.0F), exponentMask);
            // if corresponding bit in mask is set then 0.0 is returned
            Vector128<float> exponent = Avx.Blend(MathV.Exp(restrictedPower), Vector128<float>.Zero, exponentMask);
            return exponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> MaskExp(Vector256<float> power, byte exponentMask)
        {
            // uses lower 8 bits of mask
            Vector256<float> restrictedPower = Avx.Blend(power, AvxExtensions.BroadcastScalarToVector256(1.0F), exponentMask);
            Vector256<float> exponent = Avx.Blend(MathV.Exp(restrictedPower), Vector256<float>.Zero, exponentMask);
            return exponent;
        }

        /// <summary>
        /// Faster but restricted implementation of MathF.Pow().
        /// </summary>
        /// <param name="x">Base. Must be positive and nonzero.</param>
        /// <param name="y">Power.</param>
        /// <returns>x^y</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Pow(float x, float y)
        {
            if (x < 0.0F)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }
            if (x == 0.0F)
            {
                if (y <= 0.0F)
                {
                    throw new ArgumentOutOfRangeException(nameof(y));
                }
                return 0.0F;
            }
            return MathV.Exp2(MathV.Log2(x) * y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> Pow(Vector128<float> x, Vector128<float> y)
        {
            return MathV.Exp2(Avx.Multiply(MathV.Log2(x), y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Pow(Vector256<float> x, Vector256<float> y)
        {
            return MathV.Exp2(Avx.Multiply(MathV.Log2(x), y));
        }
    }
}