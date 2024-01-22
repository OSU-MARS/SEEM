using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Mars.Seem.Extensions
{
    public class MathAvx10
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Exp(Vector256<float> power)
        {
            return MathAvx10.Exp2(Avx512F.Multiply(Vector256.Create(Constant.Math.ExpToExp2), power));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<float> Exp(Vector512<float> power)
        {
            return MathAvx10.Exp2(Avx512F.Multiply(Vector512.Create(Constant.Math.ExpToExp2), power));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Exp2(Vector256<float> power)
        {
            Debug.Assert(Avx512F.MoveMask(Avx512F.And(Avx512F.CompareGreaterThan(power, Vector256.Create(Constant.Math.FloatExp2MaximumPower)), Avx512F.CompareOrdered(power, power))) == 0);

            Vector256<float> integerPowerAsFloat = Avx512F.VL.RoundScale(power, Constant.Avx512.RoundToZero);
            Vector256<float> fractionalPower = Avx512F.Subtract(power, integerPowerAsFloat);
            Vector256<float> fractionSquared = Avx512F.Multiply(fractionalPower, fractionalPower);

            Vector256<float> c1 = Vector256.Create(Constant.Math.ExpC1);
            Vector256<float> a = Avx512F.Add(fractionalPower, Avx512F.Multiply(c1, Avx512F.Multiply(fractionSquared, fractionalPower)));
            Vector256<float> c2 = Vector256.Create(Constant.Math.ExpC2);
            Vector256<float> c3 = Vector256.Create(Constant.Math.ExpC3);
            Vector256<float> b = Avx512F.Add(c3, Avx512F.Multiply(c2, fractionSquared));
            Vector256<float> fractionalInterpolant = Avx512F.Divide(Avx512F.Add(b, a), Avx512F.Subtract(b, a));

            Vector256<int> integerPower = Avx512F.ConvertToVector256Int32(integerPowerAsFloat); // res = 2^intPart
            Vector256<int> integerExponent = Avx512F.ShiftLeftLogical(integerPower, 23);
            Vector256<float> exponent = Avx512F.Add(integerExponent, fractionalInterpolant.AsInt32()).AsSingle();

            Vector256<float> zeroMask = Avx512F.CompareLessThan(power, Vector256.Create(-Constant.Math.FloatExp2MaximumPower));
            exponent = Vector256.ConditionalSelect(zeroMask, Vector256<float>.Zero, exponent);
            return exponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<float> Exp2(Vector512<float> power)
        {
            Debug.Assert(Avx512DQ.And(Avx512F.CompareGreaterThan(power, Vector512.Create(Constant.Math.FloatExp2MaximumPower)), Avx512F.CompareOrdered(power, power)).ExtractMostSignificantBits() == 0);

            Vector512<float> integerPowerAsFloat = Avx512F.RoundScale(power, Constant.Avx512.RoundToZero);
            Vector512<float> fractionalPower = Avx512F.Subtract(power, integerPowerAsFloat);
            Vector512<float> fractionSquared = Avx512F.Multiply(fractionalPower, fractionalPower);

            Vector512<float> c1 = Vector512.Create(Constant.Math.ExpC1);
            Vector512<float> a = Avx512F.Add(fractionalPower, Avx512F.Multiply(c1, Avx512F.Multiply(fractionSquared, fractionalPower)));
            Vector512<float> c2 = Vector512.Create(Constant.Math.ExpC2);
            Vector512<float> c3 = Vector512.Create(Constant.Math.ExpC3);
            Vector512<float> b = Avx512F.Add(c3, Avx512F.Multiply(c2, fractionSquared));
            Vector512<float> fractionalInterpolant = Avx512F.Divide(Avx512F.Add(b, a), Avx512F.Subtract(b, a));

            Vector512<int> integerPower = Avx512F.ConvertToVector512Int32(integerPowerAsFloat); // res = 2^intPart
            Vector512<int> integerExponent = Avx512F.ShiftLeftLogical(integerPower, 23);
            Vector512<float> exponent = Avx512F.Add(integerExponent, fractionalInterpolant.AsInt32()).AsSingle();

            Vector512<float> zeroMask = Avx512F.CompareLessThan(power, Vector512.Create(-Constant.Math.FloatExp2MaximumPower));
            exponent = Vector512.ConditionalSelect(zeroMask, Vector512<float>.Zero, exponent);
            return exponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Exp10(Vector256<float> power)
        {
            return MathAvx10.Exp2(Avx512F.Multiply(Vector256.Create(Constant.Math.Exp10ToExp2), power));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<float> Exp10(Vector512<float> power)
        {
            return MathAvx10.Exp2(Avx512F.Multiply(Vector512.Create(Constant.Math.Exp10ToExp2), power));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Ln(Vector256<float> value)
        {
            return Avx512F.Multiply(Vector256.Create(Constant.Math.Log2ToNaturalLog), MathAvx10.Log2(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<float> Ln(Vector512<float> value)
        {
            return Avx512F.Multiply(Vector512.Create(Constant.Math.Log2ToNaturalLog), MathAvx10.Log2(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Log2(Vector256<float> value)
        {
            // evaluate mantissa polynomial
            Vector256<float> beta1 = Vector256.Create(Constant.Math.Log2Beta1);
            Vector256<float> beta2 = Vector256.Create(Constant.Math.Log2Beta2);
            Vector256<float> beta3 = Vector256.Create(Constant.Math.Log2Beta3);
            Vector256<float> beta4 = Vector256.Create(Constant.Math.Log2Beta4);
            Vector256<float> beta5 = Vector256.Create(Constant.Math.Log2Beta5);

            Vector256<float> mantissa = Avx512F.VL.GetMantissa(value, Constant.Simd256x8.Mantissa12Sign);
            Vector256<float> x = Avx512F.Subtract(mantissa, Vector256.Create(Constant.Math.One));
            Vector256<float> polynomial = Avx512F.Multiply(beta1, x);
            Vector256<float> x2 = Avx512F.Multiply(x, x);
            polynomial = Avx512F.Add(polynomial, Avx512F.Multiply(beta2, x2));
            Vector256<float> x3 = Avx512F.Multiply(x2, x);
            polynomial = Avx512F.Add(polynomial, Avx512F.Multiply(beta3, x3));
            Vector256<float> x4 = Avx512F.Multiply(x3, x);
            polynomial = Avx512F.Add(polynomial, Avx512F.Multiply(beta4, x4));
            Vector256<float> x5 = Avx512F.Multiply(x4, x);
            polynomial = Avx512F.Add(polynomial, Avx512F.Multiply(beta5, x5));

            // form logarithm
            Vector256<float> exponent = Avx512F.VL.GetExponent(value);
            return Avx512F.Add(exponent, polynomial);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<float> Log2(Vector512<float> value)
        {
            // evaluate mantissa polynomial
            Vector512<float> beta1 = Vector512.Create(Constant.Math.Log2Beta1);
            Vector512<float> beta2 = Vector512.Create(Constant.Math.Log2Beta2);
            Vector512<float> beta3 = Vector512.Create(Constant.Math.Log2Beta3);
            Vector512<float> beta4 = Vector512.Create(Constant.Math.Log2Beta4);
            Vector512<float> beta5 = Vector512.Create(Constant.Math.Log2Beta5);

            Vector512<float> mantissa = Avx512F.GetMantissa(value, Constant.Simd256x8.Mantissa12Sign);
            Vector512<float> x = Avx512F.Subtract(mantissa, Vector512.Create(Constant.Math.One));
            Vector512<float> polynomial = Avx512F.Multiply(beta1, x);
            Vector512<float> x2 = Avx512F.Multiply(x, x);
            polynomial = Avx512F.Add(polynomial, Avx512F.Multiply(beta2, x2));
            Vector512<float> x3 = Avx512F.Multiply(x2, x);
            polynomial = Avx512F.Add(polynomial, Avx512F.Multiply(beta3, x3));
            Vector512<float> x4 = Avx512F.Multiply(x3, x);
            polynomial = Avx512F.Add(polynomial, Avx512F.Multiply(beta4, x4));
            Vector512<float> x5 = Avx512F.Multiply(x4, x);
            polynomial = Avx512F.Add(polynomial, Avx512F.Multiply(beta5, x5));

            // form logarithm
            Vector512<float> exponent = Avx512F.GetExponent(value);
            return Avx512F.Add(exponent, polynomial);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Log10(Vector256<float> value)
        {
            return Avx512F.Multiply(Vector256.Create(Constant.Math.Log2ToLog10), MathAvx10.Log2(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<float> Log10(Vector512<float> value)
        {
            return Avx512F.Multiply(Vector512.Create(Constant.Math.Log2ToLog10), MathAvx10.Log2(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> MaskExp(Vector256<float> power, Vector256<float> exponentMask)
        {
            // uses lower 8 bits of mask
            Vector256<float> restrictedPower = Avx512F.BlendVariable(power, Vector256.Create(1.0F), exponentMask);
            Vector256<float> exponent = Avx512F.BlendVariable(MathAvx10.Exp(restrictedPower), Vector256<float>.Zero, exponentMask);
            return exponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<float> MaskExp(Vector512<float> power, Vector512<float> exponentMask)
        {
            // uses lower 8 bits of mask
            Vector512<float> restrictedPower = Avx512F.BlendVariable(power, Vector512.Create(1.0F), exponentMask);
            Vector512<float> exponent = Avx512F.BlendVariable(MathAvx10.Exp(restrictedPower), Vector512<float>.Zero, exponentMask);
            return exponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Pow(Vector256<float> x, Vector256<float> y)
        {
            return MathAvx10.Exp2(Avx512F.Multiply(MathAvx10.Log2(x), y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<float> Pow(Vector512<float> x, Vector512<float> y)
        {
            return MathAvx10.Exp2(Avx512F.Multiply(MathAvx10.Log2(x), y));
        }
    }
}