using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.Intrinsics;

namespace Osu.Cof.Organon.Test
{
    [TestClass]
    public class Mathematics
    {
        [TestMethod]
        public void Exp()
        {
            float accuracy = 4E-6F; // error of up to 4E-6 expected for 4th order polynomial
            float precision = 5E-7F; // numerical error

            //                              0       1      2      3      4      5      6       7      8     9     10    11    12     13    14    15    16    17    18    19
            float[] values = new float[] { -10.0F, -5.0F, -4.0F, -3.0F, -2.0F, -1.5F, -1.33F, -1.0F, -0.5F, 0.0F, 0.5F, 1.0F, 1.33F, 1.5F, 2.0F, 2.5F, 3.0F, 4.0F, 5.0F, 10.0F };
            for (int index = 0; index < values.Length; ++index)
            {
                float value = values[index];
                float exp = MathV.Exp(value);
                float exp10 = MathV.Exp10(value);
                float exp2 = MathV.Exp2(value);

                double expError = 1.0 - exp / Math.Exp(value);
                double exp10Error = 1.0 - exp10 / Math.Pow(10.0, value);
                double exp2Error = 1.0 - exp2 / Math.Pow(2.0, value);

                double tolerance = accuracy * Math.Abs(value) + precision;
                Assert.IsTrue(Math.Abs(expError) < tolerance);
                Assert.IsTrue(Math.Abs(exp10Error) < tolerance);
                Assert.IsTrue(Math.Abs(exp2Error) < tolerance);
            }

            for (int quadIndex = 0; quadIndex < values.Length; quadIndex += 4)
            {
                Vector128<float> value = Vector128.Create(values[quadIndex], values[quadIndex + 1], values[quadIndex + 2], values[quadIndex + 3]);
                Vector128<float> exp = MathV.Exp(value);
                Vector128<float> exp10 = MathV.Exp10(value);
                Vector128<float> exp2 = MathV.Exp2(value);

                for (int scalarIndex = 0; scalarIndex < 4; ++scalarIndex)
                {
                    float scalarValue = value.GetElement(scalarIndex);
                    float scalarExp = exp.GetElement(scalarIndex);
                    float scalarExp10 = exp10.GetElement(scalarIndex);
                    float scalarExp2 = exp2.GetElement(scalarIndex);

                    double expError = 1.0 - scalarExp / Math.Exp(scalarValue);
                    double exp10Error = 1.0 - scalarExp10 / Math.Pow(10.0, scalarValue);
                    double exp2Error = 1.0 - scalarExp2 / Math.Pow(2.0, scalarValue);

                    double tolerance = accuracy * Math.Abs(scalarValue) + precision;
                    Assert.IsTrue(Math.Abs(expError) < tolerance);
                    Assert.IsTrue(Math.Abs(exp10Error) < tolerance);
                    Assert.IsTrue(Math.Abs(exp2Error) < tolerance);
                }
            }
        }

        [TestMethod]
        public void Log()
        {
            float accuracy = 2E-6F; // error of up to 2E-6 expected for 5th order polynomial
            float precision = 5E-7F; // numerical error

            //                             0       1           2     3     4     5     6                   7      8      9        10        11
            float[] values = new float[] { 1E-30F, 0.0000099F, 0.1F, 0.5F, 1.0F, 2.0F, 2.718281828459045F, 10.0F, 17.0F, 2.22E3F, 3.336E6F, 30E30F };
            for (int index = 0; index < values.Length; ++index)
            {
                float value = values[index];
                float ln = MathV.Ln(value);
                float log10 = MathV.Log10(value);
                float log2 = MathV.Log2(value);

                if (value != 1.0F)
                {
                    double lnError = 1.0 - ln / Math.Log(value);
                    double log10Error = 1.0 - log10 / Math.Log10(value);
                    double log2Error = 1.0 - log2 / Math.Log2(value);

                    double tolerance = accuracy * value + precision;
                    Assert.IsTrue(Math.Abs(lnError) < tolerance);
                    Assert.IsTrue(Math.Abs(log10Error) < tolerance);
                    Assert.IsTrue(Math.Abs(log2Error) < tolerance);
                }
                else
                {
                    Assert.IsTrue(ln == 0.0F);
                    Assert.IsTrue(log10 == 0.0F);
                    Assert.IsTrue(log2 == 0.0F);
                }
            }

            for (int quadIndex = 0; quadIndex < values.Length; quadIndex += 4)
            {
                Vector128<float> value = Vector128.Create(values[quadIndex], values[quadIndex + 1], values[quadIndex + 2], values[quadIndex + 3]);
                Vector128<float> ln = MathV.Ln(value);
                Vector128<float> log10 = MathV.Log10(value);
                Vector128<float> log2 = MathV.Log2(value);

                for (int scalarIndex = 0; scalarIndex < 4; ++scalarIndex)
                {
                    float scalarValue = value.GetElement(scalarIndex);
                    float scalarLn = ln.GetElement(scalarIndex);
                    float scalarLog10 = log10.GetElement(scalarIndex);
                    float scalarLog2 = log2.GetElement(scalarIndex);
                    if (scalarValue != 1.0F)
                    {
                        double lnError = 1.0 - scalarLn / Math.Log(scalarValue);
                        double log10Error = 1.0 - scalarLog10 / Math.Log10(scalarValue);
                        double log2Error = 1.0 - scalarLog2 / Math.Log2(scalarValue);

                        double tolerance = accuracy * scalarValue + precision;
                        Assert.IsTrue(Math.Abs(lnError) < tolerance);
                        Assert.IsTrue(Math.Abs(log10Error) < tolerance);
                        Assert.IsTrue(Math.Abs(log2Error) < tolerance);
                    }
                    else
                    {
                        Assert.IsTrue(scalarLn == 0.0F);
                        Assert.IsTrue(scalarLog10 == 0.0F);
                        Assert.IsTrue(scalarLog2 == 0.0F);
                    }
                }
            }
        }

        [TestMethod]
        public void Pow()
        {
            float accuracy = 4E-6F; // error of up to 2E-6 expected for 5th order polynomial
            float precision = 1.2E-6F; // numerical error

            float[] x = new float[] { 0.0F, 0.0001F, 0.1F,  0.1F,   1.0F,  1.0F, 1.3F, 2.0F, 1E6F };
            float[] y = new float[] { 0.1F, 0.0001F, 1.0F, -0.25F, 80.0F, -1.0F, 0.6F, 2.0F, 2.22F };

            for (int index = 0; index < x.Length; ++index)
            {
                float power = MathV.Pow(x[index], y[index]);

                if (x[index] != 0.0F)
                {
                    double powError = 1.0 - power / Math.Pow(x[index], y[index]);
                    double tolerance = accuracy * Math.Abs(Math.Pow(x[index], y[index])) + precision;
                    Assert.IsTrue(Math.Abs(powError) < tolerance);
                }
                else
                {
                    Assert.IsTrue(power == 0.0F);
                }
            }
        }
    }
}
