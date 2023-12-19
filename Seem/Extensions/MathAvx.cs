using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Mars.Seem.Extensions
{
    public class MathAvx
    {
        // TODO: test if inlining Vector.Create() calls reduces runtime
        private static readonly Vector128<int> FloatExponentMask128;
        private static readonly Vector256<int> FloatExponentMask256;
        private static readonly Vector128<int> FloatMantissaMask128;
        private static readonly Vector256<int> FloatMantissaMask256;
        private static readonly Vector128<int> FloatMantissaZero128;
        private static readonly Vector256<int> FloatMantissaZero256;

        static MathAvx()
        {
            MathAvx.FloatExponentMask128 = Vector128.Create(Constant.Math.FloatExponentMask);
            MathAvx.FloatExponentMask256 = Vector256.Create(Constant.Math.FloatExponentMask);
            MathAvx.FloatMantissaMask128 = Vector128.Create(Constant.Math.FloatMantissaMask);
            MathAvx.FloatMantissaMask256 = Vector256.Create(Constant.Math.FloatMantissaMask);
            MathAvx.FloatMantissaZero128 = Vector128.Create(Constant.Math.FloatMantissaZero);
            MathAvx.FloatMantissaZero256 = Vector256.Create(Constant.Math.FloatMantissaZero);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> Exp(Vector128<float> power)
        {
            return MathAvx.Exp2(Avx.Multiply(Vector128.Create(Constant.Math.ExpToExp2), power));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Exp(Vector256<float> power)
        {
            return MathAvx.Exp2(Avx.Multiply(Vector256.Create(Constant.Math.ExpToExp2), power));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> Exp2(Vector128<float> power)
        {
            Debug.Assert(Avx.MoveMask(Avx.And(Avx.CompareGreaterThan(power, Vector128.Create(Constant.Math.FloatExp2MaximumPower)), Avx.CompareOrdered(power, power))) == 0);

            Vector128<float> integerPowerAsFloat = Avx.RoundToZero(power);
            Vector128<float> fractionalPower = Avx.Subtract(power, integerPowerAsFloat);
            Vector128<float> fractionSquared = Avx.Multiply(fractionalPower, fractionalPower);

            Vector128<float> c1 = Vector128.Create(Constant.Math.ExpC1);
            Vector128<float> a = Avx.Add(fractionalPower, Avx.Multiply(c1, Avx.Multiply(fractionSquared, fractionalPower)));
            Vector128<float> c2 = Vector128.Create(Constant.Math.ExpC2);
            Vector128<float> c3 = Vector128.Create(Constant.Math.ExpC3);
            Vector128<float> b = Avx.Add(c3, Avx.Multiply(c2, fractionSquared));
            Vector128<float> fractionalInterpolant = Avx.Divide(Avx.Add(b, a), Avx.Subtract(b, a));

            Vector128<int> integerPower = Avx.ConvertToVector128Int32(integerPowerAsFloat); // res = 2^intPart
            Vector128<int> integerExponent = Avx.ShiftLeftLogical(integerPower, 23);
            Vector128<float> exponent = Avx.Add(integerExponent, fractionalInterpolant.AsInt32()).AsSingle();

            Vector128<float> zeroMask = Avx.CompareLessThan(power, Vector128.Create(-Constant.Math.FloatExp2MaximumPower));
            if (Avx.MoveMask(zeroMask) != Constant.Simd128x4.MaskAllFalse)
            {
                exponent = Avx.BlendVariable(exponent, Vector128<float>.Zero, zeroMask);
            }
            return exponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Exp2(Vector256<float> power)
        {
            Debug.Assert(Avx.MoveMask(Avx.And(Avx.CompareGreaterThan(power, Vector256.Create(Constant.Math.FloatExp2MaximumPower)), Avx.CompareOrdered(power, power))) == 0);

            Vector256<float> integerPowerAsFloat = Avx.RoundToZero(power);
            Vector256<float> fractionalPower = Avx.Subtract(power, integerPowerAsFloat);
            Vector256<float> fractionSquared = Avx.Multiply(fractionalPower, fractionalPower);

            Vector256<float> c1 = Vector256.Create(Constant.Math.ExpC1);
            Vector256<float> a = Avx.Add(fractionalPower, Avx.Multiply(c1, Avx.Multiply(fractionSquared, fractionalPower)));
            Vector256<float> c2 = Vector256.Create(Constant.Math.ExpC2);
            Vector256<float> c3 = Vector256.Create(Constant.Math.ExpC3);
            Vector256<float> b = Avx.Add(c3, Avx.Multiply(c2, fractionSquared));
            Vector256<float> fractionalInterpolant = Avx.Divide(Avx.Add(b, a), Avx.Subtract(b, a));

            Vector256<int> integerPower = Avx.ConvertToVector256Int32(integerPowerAsFloat); // res = 2^intPart
            Vector256<int> integerExponent = Avx2.ShiftLeftLogical(integerPower, 23);
            Vector256<float> exponent = Avx2.Add(integerExponent, fractionalInterpolant.AsInt32()).AsSingle();

            Vector256<float> zeroMask = Avx.CompareLessThan(power, Vector256.Create(-Constant.Math.FloatExp2MaximumPower));
            if (Avx.MoveMask(zeroMask) != Constant.Simd256x8.MaskAllFalse)
            {
                exponent = Avx.BlendVariable(exponent, Vector256<float>.Zero, zeroMask);
            }
            return exponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> Exp10(Vector128<float> power)
        {
            return MathAvx.Exp2(Avx.Multiply(Vector128.Create(Constant.Math.Exp10ToExp2), power));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Exp10(Vector256<float> power)
        {
            return MathAvx.Exp2(Avx.Multiply(Vector256.Create(Constant.Math.Exp10ToExp2), power));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> Ln(Vector128<float> value)
        {
            return Avx.Multiply(Vector128.Create(Constant.Math.Log2ToNaturalLog), MathAvx.Log2(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Ln(Vector256<float> value)
        {
            return Avx.Multiply(Vector256.Create(Constant.Math.Log2ToNaturalLog), MathAvx.Log2(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> Log2(Vector128<float> value)
        {
            // split value into exponent and mantissa parts
            Vector128<float> one = Vector128.Create(Constant.Math.One);

            Vector128<int> integerValue = value.AsInt32();
            Vector128<float> exponent = Avx.ConvertToVector128Single(Avx.Subtract(Avx.ShiftRightLogical(Avx.And(integerValue, MathAvx.FloatExponentMask128),
                                                                                                        Constant.Math.FloatMantissaBits),
                                                                                  MathAvx.FloatMantissaZero128));
            Vector128<float> mantissa = Avx.Or(Avx.And(integerValue, MathAvx.FloatMantissaMask128).AsSingle(), one);

            // evaluate mantissa polynomial
            Vector128<float> beta1 = Vector128.Create(Constant.Math.Log2Beta1);
            Vector128<float> beta2 = Vector128.Create(Constant.Math.Log2Beta2);
            Vector128<float> beta3 = Vector128.Create(Constant.Math.Log2Beta3);
            Vector128<float> beta4 = Vector128.Create(Constant.Math.Log2Beta4);
            Vector128<float> beta5 = Vector128.Create(Constant.Math.Log2Beta5);

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
        public static Vector256<float> Log2(Vector256<float> value)
        {
            // split value into exponent and mantissa parts
            Vector256<float> one = Vector256.Create(Constant.Math.One);

            Vector256<int> integerValue = value.AsInt32();
            Vector256<float> exponent = Avx.ConvertToVector256Single(Avx2.Subtract(Avx2.ShiftRightLogical(Avx2.And(integerValue, MathAvx.FloatExponentMask256),
                                                                                                          Constant.Math.FloatMantissaBits),
                                                                                   MathAvx.FloatMantissaZero256));
            Vector256<float> mantissa = Avx.Or(Avx2.And(integerValue, MathAvx.FloatMantissaMask256).AsSingle(), one);

            // evaluate mantissa polynomial
            Vector256<float> beta1 = Vector256.Create(Constant.Math.Log2Beta1);
            Vector256<float> beta2 = Vector256.Create(Constant.Math.Log2Beta2);
            Vector256<float> beta3 = Vector256.Create(Constant.Math.Log2Beta3);
            Vector256<float> beta4 = Vector256.Create(Constant.Math.Log2Beta4);
            Vector256<float> beta5 = Vector256.Create(Constant.Math.Log2Beta5);

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
        public static Vector128<float> Log10(Vector128<float> value)
        {
            return Avx.Multiply(Vector128.Create(Constant.Math.Log2ToLog10), MathAvx.Log2(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Log10(Vector256<float> value)
        {
            return Avx.Multiply(Vector256.Create(Constant.Math.Log2ToLog10), MathAvx.Log2(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> MaskExp(Vector128<float> power, Vector128<float> exponentMask)
        {
            // if corresponding bit in mask is set then 1.0 is used instead of power in restrictedPower to avoid math errors
            // Uses lower 4 bits of mask.
            Vector128<float> restrictedPower = Avx.BlendVariable(power, Vector128.Create(1.0F), exponentMask);
            // if corresponding bit in mask is set then 0.0 is returned
            Vector128<float> exponent = Avx.BlendVariable(MathAvx.Exp(restrictedPower), Vector128<float>.Zero, exponentMask);
            return exponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> MaskExp(Vector256<float> power, Vector256<float> exponentMask)
        {
            // uses lower 8 bits of mask
            Vector256<float> restrictedPower = Avx.BlendVariable(power, Vector256.Create(1.0F), exponentMask);
            Vector256<float> exponent = Avx.BlendVariable(MathAvx.Exp(restrictedPower), Vector256<float>.Zero, exponentMask);
            return exponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> Pow(Vector128<float> x, Vector128<float> y)
        {
            return MathAvx.Exp2(Avx.Multiply(MathAvx.Log2(x), y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<float> Pow(Vector256<float> x, Vector256<float> y)
        {
            return MathAvx.Exp2(Avx.Multiply(MathAvx.Log2(x), y));
        }
    }
}