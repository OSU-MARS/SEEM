using System;
using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Osu.Cof.Ferm
{
    public class FiaVolume
    {
        /// <summary>
        /// Find cubic volume of tree.
        /// </summary>
        /// <param name="trees">Trees in stand.</param>
        /// <param name="treeIndex">Tree.</param>
        /// <returns>Tree's volume including top and stump in ft³.</returns>
        public static float GetCubicFeet(Trees trees, int treeIndex)
        {
            if (trees.Units != Units.English)
            {
                throw new NotSupportedException();
            }

            float dbhInInches = trees.Dbh[treeIndex];
            if (dbhInInches < Constant.InchesPerCentimeter * Constant.Bucking.MinimumScalingDiameter4Saw)
            {
                return 0.0F;
            }

            float logDbhInInches = MathV.Log10(dbhInInches);
            float heightInFeet = trees.Height[treeIndex];
            float logHeightInFeet = MathV.Log10(heightInFeet);
            float cvtsl = trees.Species switch
            {
                // Waddell K, Campbell K, Kuegler O, Christensen G. 2014. FIA Volume Equation documentation updated on 9-19-2014:
                //   Volume estimation for PNW Databases NIMS and FIADB. https://ww3.arb.ca.gov/cc/capandtrade/offsets/copupdatereferences/qm_volume_equations_pnw_updated_091914.pdf
                // Equation 1: western Oregon and Washington (Brackett 1973)
                FiaCode.PseudotsugaMenziesii => -3.21809F + 0.04948F * logHeightInFeet * logDbhInInches - 0.15664F * logDbhInInches * logDbhInInches +
                                                 2.02132F * logDbhInInches + 1.63408F * logHeightInFeet - 0.16184F * logHeightInFeet * logHeightInFeet,
                // FIA Equation 6: all of Oregon and California (Chambers 1979)
                FiaCode.TsugaHeterophylla => -2.72170F + 2.00857F * logDbhInInches + 1.08620F * logHeightInFeet - 0.00568F * dbhInInches,
                _ => throw Trees.CreateUnhandledSpeciesException(trees.Species)
            };

            return MathV.Exp10(cvtsl);
        }

        /// <summary>
        /// Get cubic volume to a 10 centimeter (four inch) top.
        /// </summary>
        /// <param name="trees">Trees in stand.</param>
        /// <param name="compactedTreeIndex">Tree.</param>
        /// <returns>Tree's volume in m³.</returns>
        public static float GetMerchantableCubicFeet(Trees trees, int compactedTreeIndex)
        {
            if (trees.Units != Units.English)
            {
                throw new NotSupportedException();
            }

            float dbhInInches = trees.Dbh[compactedTreeIndex];
            if (dbhInInches < Constant.InchesPerCentimeter * Constant.Bucking.MinimumScalingDiameter4Saw)
            {
                // CV4 regression goes negative, unsurprisingly, for trees less than four inches in diameter
                return 0.0F;
            }

            float cvts = FiaVolume.GetCubicFeet(trees, compactedTreeIndex);
            if (cvts <= 0.0)
            {
                return 0.0F;
            }

            float basalAreaInSquareFeet = Constant.ForestersEnglish * dbhInInches * dbhInInches;
            float tarif = trees.Species switch
            {
                // Waddell K, Campbell K, Kuegler O, Christensen G. 2014. FIA Volume Equation documentation updated on 9-19-2014:
                //   Volume estimation for PNW Databases NIMS and FIADB. https://ww3.arb.ca.gov/cc/capandtrade/offsets/copupdatereferences/qm_volume_equations_pnw_updated_091914.pdf
                // Douglas-fir and western hemlock use the same tarif and CV4 regressions
                // FIA Equation 1: western Oregon and Washington (Brackett 1973)
                // FIA Equation 6: all of Oregon and California (Chambers 1979)
                FiaCode.PseudotsugaMenziesii or 
                FiaCode.TsugaHeterophylla =>
                    0.912733F * cvts / (1.033F * (1.0F + 1.382937F * MathV.Exp(-4.015292F * dbhInInches / 10.0F)) * (basalAreaInSquareFeet + 0.087266F) - 0.174533F),
                _ => throw Trees.CreateUnhandledSpeciesException(trees.Species)
            };

            return tarif * (basalAreaInSquareFeet - 0.087266F) / 0.912733F;
        }

        public static unsafe float GetScribnerBoardFeetPerAcre(Trees trees)
        {
            // for now, assume all trees are of the same species
            if (trees.Species != FiaCode.PseudotsugaMenziesii)
            {
                throw new NotSupportedException();
            }
            if (trees.Units != Units.English)
            {
                throw new NotSupportedException();
            }

            // Douglas-fir
            #if DEBUG
            Vector128<float> v6p8 = AvxExtensions.BroadcastScalarToVector128(6.8F);
            Vector128<float> v10k = AvxExtensions.BroadcastScalarToVector128(10.0F * 1000.0F);
            #endif

            // constants
            Vector128<float> forestersEnglish = AvxExtensions.BroadcastScalarToVector128(Constant.ForestersEnglish);
            Vector128<float> one = AvxExtensions.BroadcastScalarToVector128(1.0F);
            Vector128<float> six = AvxExtensions.BroadcastScalarToVector128(6.0F);

            Vector128<float> vm3p21809 = AvxExtensions.BroadcastScalarToVector128(-3.21809F); // b4
            Vector128<float> v0p04948 = AvxExtensions.BroadcastScalarToVector128(0.04948F);
            Vector128<float> vm0p15664 = AvxExtensions.BroadcastScalarToVector128(-0.15664F);
            Vector128<float> v2p02132 = AvxExtensions.BroadcastScalarToVector128(2.02132F);
            Vector128<float> v1p63408 = AvxExtensions.BroadcastScalarToVector128(1.63408F);
            Vector128<float> vm0p16184 = AvxExtensions.BroadcastScalarToVector128(-0.16184F);
            Vector128<float> v1p033 = AvxExtensions.BroadcastScalarToVector128(1.033F);
            Vector128<float> v1p382937 = AvxExtensions.BroadcastScalarToVector128(1.382937F);
            Vector128<float> vm0p4015292 = AvxExtensions.BroadcastScalarToVector128(-0.4015292F);
            Vector128<float> v0p087266 = AvxExtensions.BroadcastScalarToVector128(0.087266F);
            Vector128<float> vm0p174533 = AvxExtensions.BroadcastScalarToVector128(-0.174533F);

            Vector128<float> vm0p6896598794 = AvxExtensions.BroadcastScalarToVector128(-0.6896598794F); // rc6-rs632
            Vector128<float> v0p993 = AvxExtensions.BroadcastScalarToVector128(0.993F);
            Vector128<float> v0p174439 = AvxExtensions.BroadcastScalarToVector128(0.174439F);
            Vector128<float> v0p117594 = AvxExtensions.BroadcastScalarToVector128(0.117594F);
            Vector128<float> vm8p210585 = AvxExtensions.BroadcastScalarToVector128(-8.210585F);
            Vector128<float> v0p236693 = AvxExtensions.BroadcastScalarToVector128(0.236693F);
            Vector128<float> v0p00001345 = AvxExtensions.BroadcastScalarToVector128(0.00001345F);
            Vector128<float> v0p00001937 = AvxExtensions.BroadcastScalarToVector128(0.00001937F);
            Vector128<float> v1p001491 = AvxExtensions.BroadcastScalarToVector128(1.001491F);
            Vector128<float> vm6p924097 = AvxExtensions.BroadcastScalarToVector128(-6.924097F);
            Vector128<float> v0p912733 = AvxExtensions.BroadcastScalarToVector128(0.912733F);
            Vector128<float> v0p00001351 = AvxExtensions.BroadcastScalarToVector128(0.00001351F);

            fixed (float* dbh = &trees.Dbh[0], expansionFactors = &trees.LiveExpansionFactor[0], height = &trees.Height[0])
            {
                Vector128<float> standBoardFeetPerAcre = Vector128<float>.Zero;
                for (int treeIndex = 0; treeIndex < trees.Count; treeIndex += Constant.Simd128x4.Width)
                {
                    Vector128<float> dbhInInches = Avx.LoadVector128(dbh + treeIndex);
                    Vector128<float> heightInFeet = Avx.LoadVector128(height + treeIndex);

                    Vector128<float> logDbhInInches = MathV.Log10(dbhInInches);
                    Vector128<float> logHeightInFeet = MathV.Log10(heightInFeet);
                    // FiaCode.PseudotsugaMenziesii => -3.21809F + 0.04948F * logHeightInFeet * logDbhInInches - 0.15664F * logDbhInInches * logDbhInInches +
                    //                                  2.02132F * logDbhInInches + 1.63408F * logHeightInFeet - 0.16184F * logHeightInFeet * logHeightInFeet,
                    Vector128<float> cvtsl = Avx.Add(vm3p21809, Avx.Multiply(v0p04948, Avx.Multiply(logHeightInFeet, logDbhInInches)));
                                     cvtsl = Avx.Add(cvtsl, Avx.Multiply(vm0p15664, Avx.Multiply(logDbhInInches, logDbhInInches)));
                                     cvtsl = Avx.Add(cvtsl, Avx.Multiply(v2p02132, logDbhInInches));
                                     cvtsl = Avx.Add(cvtsl, Avx.Multiply(v1p63408, logHeightInFeet));
                                     cvtsl = Avx.Add(cvtsl, Avx.Multiply(vm0p16184, Avx.Multiply(logHeightInFeet, logHeightInFeet)));
                    Vector128<float> cubicFeet = MathV.Exp10(cvtsl);

                    Vector128<float> dbhSquared = Avx.Multiply(dbhInInches, dbhInInches); // could be consolidated by merging other scaling constants with Forester's constant for basal area
                    Vector128<float> basalAreaInSquareFeet = Avx.Multiply(forestersEnglish, dbhSquared);
                    // b4 = cubicFeet / (1.033F * (1.0F + 1.382937F * MathV.Exp(-4.015292F * dbhInInches / 10.0F)) * (basalAreaInSquareFeet + 0.087266F) - 0.174533F);
                    Vector128<float> b4 = Avx.Divide(cubicFeet, Avx.Add(Avx.Multiply(v1p033, 
                                                                                     Avx.Multiply(Avx.Add(one, Avx.Multiply(v1p382937, 
                                                                                                                            MathV.Exp(Avx.Multiply(vm0p4015292, 
                                                                                                                                                   dbhInInches)))),
                                                                                                  Avx.Add(basalAreaInSquareFeet, v0p087266))),
                                                                        vm0p174533));
                    Vector128<float> cv4 = Avx.Multiply(b4, Avx.Subtract(basalAreaInSquareFeet, v0p087266));

                    // conversion to Scribner volumes for 32 foot trees
                    // Waddell 2014:32
                    // rc6 = 0.993F * (1.0F - MathF.Pow(0.62F, dbhInInches - 6.0F));
                    Vector128<float> rc6 = Avx.Multiply(v0p993, Avx.Subtract(one, MathV.Exp(Avx.Multiply(vm0p6896598794, Avx.Subtract(dbhInInches, six))))); // log2(0.62) = -0.6896598794
                    Vector128<float> cv6 = Avx.Multiply(rc6, cv4);
                    Vector128<float> logB4 = MathV.Log10(b4);
                    // float rs616 = MathF.Pow(10.0F, 0.174439F + 0.117594F * logDbhInInches * logB4 - 8.210585F / (dbhInInches * dbhInInches) + 0.236693F * logB4 - 0.00001345F * b4 * b4 - 0.00001937F * dbhInInches * dbhInInches);
                    Vector128<float> rs616l = Avx.Add(v0p174439, Avx.Multiply(v0p117594, Avx.Multiply(logDbhInInches, logB4)));
                                     rs616l = Avx.Add(rs616l, Avx.Divide(vm8p210585, dbhSquared));
                                     rs616l = Avx.Add(rs616l, Avx.Multiply(v0p236693, logB4));
                                     rs616l = Avx.Subtract(rs616l, Avx.Multiply(v0p00001345, Avx.Multiply(b4, b4)));
                                     rs616l = Avx.Subtract(rs616l, Avx.Multiply(v0p00001937, dbhSquared));
                    Vector128<float> rs616 = MathV.Exp10(rs616l);
                    Vector128<float> sv616 = Avx.Multiply(rs616, cv6); // Scribner board foot volume to a 6 inch top for 16 foot logs
                    // float rs632 = 1.001491F - 6.924097F / tarif + 0.00001351F * dbhInInches * dbhInInches;
                    Vector128<float> rs632 = Avx.Add(v1p001491, Avx.Divide(vm6p924097, Avx.Multiply(v0p912733, b4)));
                                     rs632 = Avx.Add(rs632, Avx.Multiply(v0p00001351, dbhSquared));
                    Vector128<float> zeroVolumeMask = Avx.CompareLessThanOrEqual(dbhInInches, six);
                    Vector128<float> sv632 = Avx.Multiply(rs632, sv616); // Scribner board foot volume to a 6 inch top for 32 foot logs
                                     sv632 = Avx.BlendVariable(sv632, Vector128<float>.Zero, zeroVolumeMask);

                    #if DEBUG
                    DebugV.Assert(Avx.CompareGreaterThanOrEqual(Avx.BlendVariable(rc6, Vector128<float>.Zero, zeroVolumeMask), Vector128<float>.Zero));
                    DebugV.Assert(Avx.CompareLessThanOrEqual(rc6, one));
                    DebugV.Assert(Avx.CompareGreaterThanOrEqual(Avx.BlendVariable(rs616, one, zeroVolumeMask), one));
                    DebugV.Assert(Avx.CompareLessThanOrEqual(Avx.BlendVariable(rs616, Vector128<float>.Zero, zeroVolumeMask), v6p8));
                    DebugV.Assert(Avx.CompareGreaterThanOrEqual(Avx.BlendVariable(rs632, Vector128<float>.Zero, zeroVolumeMask), Vector128<float>.Zero));
                    DebugV.Assert(Avx.CompareLessThanOrEqual(Avx.BlendVariable(rs632, Vector128<float>.Zero, zeroVolumeMask), one));
                    DebugV.Assert(Avx.CompareGreaterThanOrEqual(Avx.BlendVariable(sv632, Vector128<float>.Zero, zeroVolumeMask), Vector128<float>.Zero));
                    DebugV.Assert(Avx.CompareLessThanOrEqual(Avx.BlendVariable(sv632, Vector128<float>.Zero, zeroVolumeMask), v10k));
                    #endif

                    Vector128<float> expansionFactor = Avx.LoadVector128(expansionFactors + treeIndex);
                    standBoardFeetPerAcre = Avx.Add(standBoardFeetPerAcre, Avx.Multiply(expansionFactor, sv632));
                }

                standBoardFeetPerAcre = Avx.HorizontalAdd(standBoardFeetPerAcre, standBoardFeetPerAcre);
                standBoardFeetPerAcre = Avx.HorizontalAdd(standBoardFeetPerAcre, standBoardFeetPerAcre);
                return standBoardFeetPerAcre.ToScalar();
            }
        }

        /// <summary>
        /// Get Scribner board foot volume for 32 foot logs to a six inch top.
        /// </summary>
        /// <param name="trees">Trees in stand.</param>
        /// <param name="compactedTreeIndex">Tree.</param>
        /// <returns>Tree's volume in Scribner board feet</returns>
        public static float GetScribnerBoardFeet(Trees trees, int compactedTreeIndex)
        {
            if (trees.Units != Units.English)
            {
                throw new NotSupportedException();
            }

            // repeat code of GetCubicFeet() as this provides about a 6% speedup
            float dbhInInches = trees.Dbh[compactedTreeIndex];
            if (dbhInInches < 6.0F)
            {
                return 0.0F;
            }

            float logDbhInInches = MathV.Log10(dbhInInches);
            float heightInFeet = trees.Height[compactedTreeIndex];
            float logHeightInFeet = MathV.Log10(heightInFeet);
            float cvtsl = trees.Species switch
            {
                // Waddell K, Campbell K, Kuegler O, Christensen G. 2014. FIA Volume Equation documentation updated on 9-19-2014:
                //   Volume estimation for PNW Databases NIMS and FIADB. https://ww3.arb.ca.gov/cc/capandtrade/offsets/copupdatereferences/qm_volume_equations_pnw_updated_091914.pdf
                // Equation 1: western Oregon and Washington (Brackett 1973)
                FiaCode.PseudotsugaMenziesii => -3.21809F + 0.04948F * logHeightInFeet * logDbhInInches - 0.15664F * logDbhInInches * logDbhInInches +
                                                 2.02132F * logDbhInInches + 1.63408F * logHeightInFeet - 0.16184F * logHeightInFeet * logHeightInFeet,
                // FIA Equation 6: all of Oregon and California (Chambers 1979)
                FiaCode.TsugaHeterophylla => -2.72170F + 2.00857F * logDbhInInches + 1.08620F * logHeightInFeet - 0.00568F * dbhInInches,
                _ => throw Trees.CreateUnhandledSpeciesException(trees.Species)
            };
            float cubicFeet = MathV.Exp10(cvtsl);
            
            float basalAreaInSquareFeet = Constant.ForestersEnglish * dbhInInches * dbhInInches;
            float tarif = trees.Species switch
            {
                // Waddell K, Campbell K, Kuegler O, Christensen G. 2014. FIA Volume Equation documentation updated on 9-19-2014:
                //   Volume estimation for PNW Databases NIMS and FIADB. https://ww3.arb.ca.gov/cc/capandtrade/offsets/copupdatereferences/qm_volume_equations_pnw_updated_091914.pdf
                // Douglas-fir and western hemlock use the same tarif and CV4 regressions
                // FIA Equation 1: western Oregon and Washington (Brackett 1973)
                // FIA Equation 6: all of Oregon and California (Chambers 1979)
                FiaCode.PseudotsugaMenziesii or 
                FiaCode.TsugaHeterophylla =>
                    0.912733F * cubicFeet / (1.033F * (1.0F + 1.382937F * MathV.Exp(-4.015292F * dbhInInches / 10.0F)) * (basalAreaInSquareFeet + 0.087266F) - 0.174533F),
                _ => throw Trees.CreateUnhandledSpeciesException(trees.Species)
            };
            float cv4 = tarif * (basalAreaInSquareFeet - 0.087266F) / 0.912733F;

            // conversion to Scribner volumes for 32 foot trees
            // Waddell 2014:32
            // float rc6 = 0.993F * (1.0F - MathF.Pow(0.62F, dbhInInches - 6.0F));
            float rc6 = 0.993F * (1.0F - MathV.Exp(-0.6896598794F * (dbhInInches - 6.0F))); // log2(0.62) = -0.6896598794
            float cv6 = rc6 * cv4;
            float b4 = tarif / 0.912733F;
            float logB4 = MathV.Log10(b4);
            // float rs616 = MathF.Pow(10.0F, 0.174439F + 0.117594F * logDbhInInches * logB4 - 8.210585F / (dbhInInches * dbhInInches) + 0.236693F * logB4 - 0.00001345F * b4 * b4 - 0.00001937F * dbhInInches * dbhInInches);
            float rs616 = MathV.Exp10(0.174439F + 0.117594F * logDbhInInches * logB4 - 8.210585F / (dbhInInches * dbhInInches) + 0.236693F * logB4 - 0.00001345F * b4 * b4 - 0.00001937F * dbhInInches * dbhInInches);
            float sv616 = rs616 * cv6; // Scribner board foot volume to a 6 inch top for 16 foot logs
            float rs632 = 1.001491F - 6.924097F / tarif + 0.00001351F * dbhInInches * dbhInInches;
            float sv632 = rs632 * sv616; // Scribner board foot volume to a 6 inch top for 32 foot logs

            Debug.Assert(rc6 >= 0.0F);
            Debug.Assert(rc6 <= 1.0F);
            Debug.Assert(rs616 >= 1.0F);
            Debug.Assert(rs616 <= 7.0F);
            Debug.Assert(rs632 >= 0.0F);
            Debug.Assert(rs632 <= 1.0F);
            Debug.Assert(sv632 >= 0.0F);
            Debug.Assert(sv632 <= 10.0 * 1000.0F);
            return sv632;
        }
    }
}
