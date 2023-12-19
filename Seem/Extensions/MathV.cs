using System;
using System.Runtime.CompilerServices;

namespace Mars.Seem.Extensions
{
    public class MathV
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Exp(float power)
        {
            return MathV.Exp2(Constant.Math.ExpToExp2 * power);
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
            if (power <= -Constant.Math.FloatExp2MaximumPower)
            {
                return 0.0F; // truncate values less than 5.877472e-39 down to zero
            }
            ArgumentOutOfRangeException.ThrowIfGreaterThan(power, Constant.Math.FloatExp2MaximumPower);

            // fast_exp() by @jenkas in https://stackoverflow.com/questions/479705/reinterpret-cast-in-c-sharp
            // ExpC4 is corrected in this code as @jenkas used a value of log2(e) that's low by 0.013 ppm.
            int integerPower = (int)power;
            float fractionalPower = power - integerPower;
            float fractionSquared = fractionalPower * fractionalPower;
            float a = fractionalPower + Constant.Math.ExpC1 * fractionSquared * fractionalPower;
            float b = Constant.Math.ExpC3 + Constant.Math.ExpC2 * fractionSquared;
            float fractionalInterpolant = (b + a) / (b - a);
            int exponentAsInt = BitConverter.SingleToInt32Bits(fractionalInterpolant) + (integerPower << 23); // res + 2^(intPart)
            float exponent = BitConverter.Int32BitsToSingle(exponentAsInt);
            return exponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Exp10(float power)
        {
            return MathV.Exp2(Constant.Math.Exp10ToExp2 * power);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Ln(float value)
        {
            return Constant.Math.Log2ToNaturalLog * MathV.Log2(value);
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
            float exponent = (float)(((integerValue & Constant.Math.FloatExponentMask) >> Constant.Math.FloatMantissaBits) - Constant.Math.FloatMantissaZero);
            float mantissa = BitConverter.Int32BitsToSingle((integerValue & Constant.Math.FloatMantissaMask) | Constant.Math.OneAsInt);
            float x = mantissa - 1;
            float polynomial = Constant.Math.Log2Beta1 * x + Constant.Math.Log2Beta2 * x * x + Constant.Math.Log2Beta3 * x * x * x + Constant.Math.Log2Beta4 * x * x * x * x + Constant.Math.Log2Beta5 * x * x * x * x * x;
            return exponent + polynomial;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Log10(float value)
        {
            return Constant.Math.Log2ToLog10 * MathV.Log2(value);
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
            ArgumentOutOfRangeException.ThrowIfLessThan(x, 0.0F);
            if (x == 0.0F)
            {
                ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(y, 0.0F);
                return 0.0F;
            }
            return MathV.Exp2(MathV.Log2(x) * y);
        }
    }
}
