using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Avx512 = System.Runtime.Intrinsics.X86.Avx10v1.V512;

namespace Mars.Seem.Extensions
{
    public class MathAvx10
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Exp(Vector256<float> power)
        {
            return MathAvx10.Exp2(Avx10v1.Multiply(Vector256.Create(Constant.Math.ExpToExp2), power));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<float> Exp(Vector512<float> power)
        {
            return MathAvx10.Exp2(Avx512.Multiply(Vector512.Create(Constant.Math.ExpToExp2), power));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Exp2(Vector256<float> power)
        {
            Debug.Assert(Avx10v1.MoveMask(Avx512.And(Avx10v1.CompareGreaterThan(power, Vector256.Create(Constant.Math.FloatExp2MaximumPower)), Avx10v1.CompareOrdered(power, power))) == 0);

            Vector256<float> integerPowerAsFloat = Avx512.VL.RoundScale(power, Constant.Avx512.RoundToZero);
            Vector256<float> fractionalPower = Avx10v1.Subtract(power, integerPowerAsFloat);
            Vector256<float> fractionSquared = Avx10v1.Multiply(fractionalPower, fractionalPower);

            Vector256<float> c1 = Vector256.Create(Constant.Math.ExpC1);
            Vector256<float> a = Avx10v1.Add(fractionalPower, Avx10v1.Multiply(c1, Avx10v1.Multiply(fractionSquared, fractionalPower)));
            Vector256<float> c2 = Vector256.Create(Constant.Math.ExpC2);
            Vector256<float> c3 = Vector256.Create(Constant.Math.ExpC3);
            Vector256<float> b = Avx10v1.Add(c3, Avx10v1.Multiply(c2, fractionSquared));
            Vector256<float> fractionalInterpolant = Avx10v1.Divide(Avx10v1.Add(b, a), Avx10v1.Subtract(b, a));

            Vector256<int> integerPower = Avx10v1.ConvertToVector256Int32(integerPowerAsFloat); // res = 2^intPart
            Vector256<int> integerExponent = Avx10v1.ShiftLeftLogical(integerPower, 23);
            Vector256<float> exponent = Avx10v1.Add(integerExponent, fractionalInterpolant.AsInt32()).AsSingle();

            Vector256<float> zeroMask = Avx10v1.CompareLessThan(power, Vector256.Create(-Constant.Math.FloatExp2MaximumPower));
            exponent = Vector256.ConditionalSelect(zeroMask, Vector256<float>.Zero, exponent);
            return exponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<float> Exp2(Vector512<float> power)
        {
            Debug.Assert(Avx512DQ.And(Avx512.CompareGreaterThan(power, Vector512.Create(Constant.Math.FloatExp2MaximumPower)), Avx512.CompareOrdered(power, power)).ExtractMostSignificantBits() == 0);

            Vector512<float> integerPowerAsFloat = Avx512F.RoundScale(power, Constant.Avx512.RoundToZero);
            Vector512<float> fractionalPower = Avx512.Subtract(power, integerPowerAsFloat);
            Vector512<float> fractionSquared = Avx512.Multiply(fractionalPower, fractionalPower);

            Vector512<float> c1 = Vector512.Create(Constant.Math.ExpC1);
            Vector512<float> a = Avx512.Add(fractionalPower, Avx512.Multiply(c1, Avx512.Multiply(fractionSquared, fractionalPower)));
            Vector512<float> c2 = Vector512.Create(Constant.Math.ExpC2);
            Vector512<float> c3 = Vector512.Create(Constant.Math.ExpC3);
            Vector512<float> b = Avx512.Add(c3, Avx512.Multiply(c2, fractionSquared));
            Vector512<float> fractionalInterpolant = Avx512.Divide(Avx512.Add(b, a), Avx512.Subtract(b, a));

            Vector512<int> integerPower = Avx512.ConvertToVector512Int32(integerPowerAsFloat); // res = 2^intPart
            Vector512<int> integerExponent = Avx512.ShiftLeftLogical(integerPower, 23);
            Vector512<float> exponent = Avx512.Add(integerExponent, fractionalInterpolant.AsInt32()).AsSingle();

            Vector512<float> zeroMask = Avx512.CompareLessThan(power, Vector512.Create(-Constant.Math.FloatExp2MaximumPower));
            exponent = Vector512.ConditionalSelect(zeroMask, Vector512<float>.Zero, exponent);
            return exponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Exp10(Vector256<float> power)
        {
            return MathAvx10.Exp2(Avx10v1.Multiply(Vector256.Create(Constant.Math.Exp10ToExp2), power));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<float> Exp10(Vector512<float> power)
        {
            return MathAvx10.Exp2(Avx512.Multiply(Vector512.Create(Constant.Math.Exp10ToExp2), power));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Ln(Vector256<float> value)
        {
            return Avx10v1.Multiply(Vector256.Create(Constant.Math.Log2ToNaturalLog), MathAvx10.Log2(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<float> Ln(Vector512<float> value)
        {
            return Avx512.Multiply(Vector512.Create(Constant.Math.Log2ToNaturalLog), MathAvx10.Log2(value));
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

            Vector256<float> mantissa = Avx512.VL.GetMantissa(value, Constant.Simd256x8.Mantissa12Sign);
            Vector256<float> x = Avx10v1.Subtract(mantissa, Vector256.Create(Constant.Math.One));
            Vector256<float> polynomial = Avx10v1.Multiply(beta1, x);
            Vector256<float> x2 = Avx10v1.Multiply(x, x);
            polynomial = Avx10v1.Add(polynomial, Avx10v1.Multiply(beta2, x2));
            Vector256<float> x3 = Avx10v1.Multiply(x2, x);
            polynomial = Avx10v1.Add(polynomial, Avx10v1.Multiply(beta3, x3));
            Vector256<float> x4 = Avx10v1.Multiply(x3, x);
            polynomial = Avx10v1.Add(polynomial, Avx10v1.Multiply(beta4, x4));
            Vector256<float> x5 = Avx10v1.Multiply(x4, x);
            polynomial = Avx10v1.Add(polynomial, Avx10v1.Multiply(beta5, x5));

            // form logarithm
            Vector256<float> exponent = Avx512.VL.GetExponent(value);
            return Avx10v1.Add(exponent, polynomial);
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
            Vector512<float> x = Avx512.Subtract(mantissa, Vector512.Create(Constant.Math.One));
            Vector512<float> polynomial = Avx512.Multiply(beta1, x);
            Vector512<float> x2 = Avx512.Multiply(x, x);
            polynomial = Avx512.Add(polynomial, Avx512.Multiply(beta2, x2));
            Vector512<float> x3 = Avx512.Multiply(x2, x);
            polynomial = Avx512.Add(polynomial, Avx512.Multiply(beta3, x3));
            Vector512<float> x4 = Avx512.Multiply(x3, x);
            polynomial = Avx512.Add(polynomial, Avx512.Multiply(beta4, x4));
            Vector512<float> x5 = Avx512.Multiply(x4, x);
            polynomial = Avx512.Add(polynomial, Avx512.Multiply(beta5, x5));

            // form logarithm
            Vector512<float> exponent = Avx512F.GetExponent(value);
            return Avx512.Add(exponent, polynomial);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Log10(Vector256<float> value)
        {
            return Avx10v1.Multiply(Vector256.Create(Constant.Math.Log2ToLog10), MathAvx10.Log2(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<float> Log10(Vector512<float> value)
        {
            return Avx512.Multiply(Vector512.Create(Constant.Math.Log2ToLog10), MathAvx10.Log2(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> MaskExp(Vector256<float> power, Vector256<float> exponentMask)
        {
            // uses lower 8 bits of mask
            Vector256<float> restrictedPower = Avx10v1.BlendVariable(power, Vector256.Create(1.0F), exponentMask);
            Vector256<float> exponent = Avx10v1.BlendVariable(MathAvx10.Exp(restrictedPower), Vector256<float>.Zero, exponentMask);
            return exponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<float> MaskExp(Vector512<float> power, Vector512<float> exponentMask)
        {
            // uses lower 8 bits of mask
            Vector512<float> restrictedPower = Avx512.BlendVariable(power, Vector512.Create(1.0F), exponentMask);
            Vector512<float> exponent = Avx512.BlendVariable(MathAvx10.Exp(restrictedPower), Vector512<float>.Zero, exponentMask);
            return exponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Pow(Vector256<float> x, Vector256<float> y)
        {
            return MathAvx10.Exp2(Avx10v1.Multiply(MathAvx10.Log2(x), y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<float> Pow(Vector512<float> x, Vector512<float> y)
        {
            return MathAvx10.Exp2(Avx512.Multiply(MathAvx10.Log2(x), y));
        }
    }
}