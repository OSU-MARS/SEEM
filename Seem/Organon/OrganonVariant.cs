using Mars.Seem.Extensions;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Mars.Seem.Organon
{
    public abstract class OrganonVariant
    {
        private readonly SortedList<FiaCode, OrganonCrownCoefficients> crownCoefficients;
        private readonly SortedList<FiaCode, OrganonHeightCoefficients> heightCoefficients;

        public float OldTreeAgeThreshold { get; private init; }
        public Simd Simd { get; init; }
        public int TimeStepInYears { get; private init; }
        public TreeModel TreeModel { get; private init; }

        protected OrganonVariant(TreeModel treeModel, float oldTreeAgeThreshold)
        {
            this.crownCoefficients = new();
            this.heightCoefficients = new();
            this.OldTreeAgeThreshold = oldTreeAgeThreshold;
            this.Simd = Simd.Width128;
            this.TimeStepInYears = treeModel == TreeModel.OrganonRap ? 1 : 5;
            this.TreeModel = treeModel;
        }

        // VEX 128 with quads of strata: 3.0x speedup from scalar
        public unsafe void AddCrownCompetitionByHeight(Trees trees, float[] crownCompetitionByHeight)
        {
            switch (this.Simd)
            {
                case Simd.Width128:
                    this.AddCrownCompetitionByHeight128(trees, crownCompetitionByHeight);
                    break;
                case Simd.Width256:
                    this.AddCrownCompetitionByHeight256(trees, crownCompetitionByHeight);
                    break;
                default:
                    throw new NotSupportedException("Unhandled SIMD " + this.Simd + ".");
            };
        }

        // VEX 128 with quads of trees: 2.1x speedup from scalar
        //private override unsafe void AddCrownCompetitionByHeight128(Trees trees, float[] crownCompetitionByHeight)
        //{
        //    // coefficients for maximum crown width
        //    FiaCode species = trees.Species;
        //    Vector128<float> mcwB0;
        //    Vector128<float> mcwB1;
        //    Vector128<float> mcwB2;
        //    Vector128<float> dbhLimitForMaxCrownWidth;
        //    switch (species)
        //    {
        //        // Paine and Hann(1982) FRL Research Paper 46
        //        case FiaCode.PseudotsugaMenziesii:
        //            mcwB0 = AvxExtensions.BroadcastScalarToVector128(4.6198F);
        //            mcwB1 = AvxExtensions.BroadcastScalarToVector128(1.8426F);
        //            mcwB2 = AvxExtensions.BroadcastScalarToVector128(-0.011311F);
        //            dbhLimitForMaxCrownWidth = AvxExtensions.BroadcastScalarToVector128(81.45F);
        //            break;
        //        // GF Coefficients from Paine and Hann(1982) FRL Research Paper 46
        //        case FiaCode.AbiesGrandis:
        //            mcwB0 = AvxExtensions.BroadcastScalarToVector128(6.1880F);
        //            mcwB1 = AvxExtensions.BroadcastScalarToVector128(1.0069F);
        //            mcwB2 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            dbhLimitForMaxCrownWidth = AvxExtensions.BroadcastScalarToVector128(999.99F);
        //            break;
        //        // Johnson(2002) Willamette Industries Report
        //        case FiaCode.TsugaHeterophylla:
        //            mcwB0 = AvxExtensions.BroadcastScalarToVector128(4.3586F);
        //            mcwB1 = AvxExtensions.BroadcastScalarToVector128(1.57458F);
        //            mcwB2 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            dbhLimitForMaxCrownWidth = AvxExtensions.BroadcastScalarToVector128(76.70F);
        //            break;
        //        // Smith(1966) Proc. 6th World Forestry Conference
        //        case FiaCode.ThujaPlicata:
        //            mcwB0 = AvxExtensions.BroadcastScalarToVector128(4.0F);
        //            mcwB1 = AvxExtensions.BroadcastScalarToVector128(1.65F);
        //            mcwB2 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            dbhLimitForMaxCrownWidth = AvxExtensions.BroadcastScalarToVector128(999.99F);
        //            break;
        //        // WH of Paine and Hann(1982) FRL Research Paper 46
        //        case FiaCode.TaxusBrevifolia:
        //            mcwB0 = AvxExtensions.BroadcastScalarToVector128(4.5652F);
        //            mcwB1 = AvxExtensions.BroadcastScalarToVector128(1.4147F);
        //            mcwB2 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            dbhLimitForMaxCrownWidth = AvxExtensions.BroadcastScalarToVector128(999.99F);
        //            break;
        //        // Paine and Hann(1982) FRL Research Paper 46
        //        case FiaCode.ArbutusMenziesii:
        //            mcwB0 = AvxExtensions.BroadcastScalarToVector128(3.4298629F);
        //            mcwB1 = AvxExtensions.BroadcastScalarToVector128(1.3532302F);
        //            mcwB2 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            dbhLimitForMaxCrownWidth = AvxExtensions.BroadcastScalarToVector128(999.99F);
        //            break;
        //        // Ek(1974) School of Natural Res., U.Wisc., Forestry Res. Notes.
        //        case FiaCode.AcerMacrophyllum:
        //            mcwB0 = AvxExtensions.BroadcastScalarToVector128(4.0953F);
        //            mcwB1 = AvxExtensions.BroadcastScalarToVector128(2.3849F);
        //            mcwB2 = AvxExtensions.BroadcastScalarToVector128(-0.0102651F);
        //            dbhLimitForMaxCrownWidth = AvxExtensions.BroadcastScalarToVector128(102.53F);
        //            break;
        //        // WO Coefficients from Paine and Hann (1982) FRL Research Paper 46
        //        case FiaCode.QuercusGarryana:
        //            mcwB0 = AvxExtensions.BroadcastScalarToVector128(3.0785639F);
        //            mcwB1 = AvxExtensions.BroadcastScalarToVector128(1.9242211F);
        //            mcwB2 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            dbhLimitForMaxCrownWidth = AvxExtensions.BroadcastScalarToVector128(999.99F);
        //            break;
        //        // Smith(1966) Proc. 6th World Forestry Conference
        //        case FiaCode.AlnusRubra:
        //            mcwB0 = AvxExtensions.BroadcastScalarToVector128(8.0F);
        //            mcwB1 = AvxExtensions.BroadcastScalarToVector128(1.53F);
        //            mcwB2 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            dbhLimitForMaxCrownWidth = AvxExtensions.BroadcastScalarToVector128(999.99F);
        //            break;
        //        // GC of Paine and Hann(1982) FRL Research Paper 46
        //        case FiaCode.CornusNuttallii:
        //        case FiaCode.Salix:
        //            mcwB0 = AvxExtensions.BroadcastScalarToVector128(2.9793895F);
        //            mcwB1 = AvxExtensions.BroadcastScalarToVector128(1.5512443F);
        //            mcwB2 = AvxExtensions.BroadcastScalarToVector128(-0.01416129F);
        //            dbhLimitForMaxCrownWidth = AvxExtensions.BroadcastScalarToVector128(54.77F);
        //            break;
        //        default:
        //            throw Trees.CreateUnhandledSpeciesException(species);
        //    }

        //    // coefficients for height to largest crown width
        //    Vector128<float> hlcwB1;
        //    switch (species)
        //    {
        //        // Hann(1999) FS 45: 217-225
        //        case FiaCode.PseudotsugaMenziesii:
        //            hlcwB1 = AvxExtensions.BroadcastScalarToVector128(0.062000F);
        //            break;
        //        // Hann and Hanus(2001) FRL Research Contribution 34
        //        case FiaCode.AbiesGrandis:
        //            hlcwB1 = AvxExtensions.BroadcastScalarToVector128(0.028454F);
        //            break;
        //        // Marshall, Johnson, and Hann(2003) CJFR 33: 2059-2066
        //        case FiaCode.TsugaHeterophylla:
        //            hlcwB1 = AvxExtensions.BroadcastScalarToVector128(0.355270F);
        //            break;
        //        // WH of Hann and Hanus(2001) FRL Research Contribution 34
        //        case FiaCode.ThujaPlicata:
        //        case FiaCode.TaxusBrevifolia:
        //            hlcwB1 = AvxExtensions.BroadcastScalarToVector128(0.209806F);
        //            break;
        //        // Hann and Hanus(2001) FRL Research Contribution 34
        //        case FiaCode.ArbutusMenziesii:
        //        case FiaCode.AcerMacrophyllum:
        //        case FiaCode.QuercusGarryana:
        //        case FiaCode.AlnusRubra:
        //        case FiaCode.CornusNuttallii:
        //        case FiaCode.Salix:
        //            hlcwB1 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            break;
        //        default:
        //            throw Trees.CreateUnhandledSpeciesException(species);
        //    }

        //    Vector128<float> lcwB1;
        //    Vector128<float> lcwB2;
        //    Vector128<float> lcwB3;
        //    switch (species)
        //    {
        //        // Hann(1997) FRL Research Contribution 17
        //        case FiaCode.PseudotsugaMenziesii:
        //            lcwB1 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            lcwB2 = AvxExtensions.BroadcastScalarToVector128(0.00436324F);
        //            lcwB3 = AvxExtensions.BroadcastScalarToVector128(0.6020020F);
        //            break;
        //        // Hann(1997) FRL Research Contribution 17
        //        case FiaCode.AbiesGrandis:
        //            lcwB1 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            lcwB2 = AvxExtensions.BroadcastScalarToVector128(0.00308402F);
        //            lcwB3 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            break;
        //        // Johnson(2002) Willamette Industries Report
        //        case FiaCode.TsugaHeterophylla:
        //            lcwB1 = AvxExtensions.BroadcastScalarToVector128(0.105590F);
        //            lcwB2 = AvxExtensions.BroadcastScalarToVector128(0.0035662F);
        //            lcwB3 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            break;
        //        // IC of Hann(1997) FRL Research Contribution 17
        //        case FiaCode.ThujaPlicata:
        //            lcwB1 = AvxExtensions.BroadcastScalarToVector128(-0.2513890F);
        //            lcwB2 = AvxExtensions.BroadcastScalarToVector128(0.006925120F);
        //            lcwB3 = AvxExtensions.BroadcastScalarToVector128(0.985922F);
        //            break;
        //        // WH of Hann(1997) FRL Research Contribution 17
        //        case FiaCode.TaxusBrevifolia:
        //            lcwB1 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            lcwB2 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            lcwB3 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            break;
        //        // Hann(1997) FRL Research Contribution 17
        //        case FiaCode.ArbutusMenziesii:
        //            lcwB1 = AvxExtensions.BroadcastScalarToVector128(0.118621F);
        //            lcwB2 = AvxExtensions.BroadcastScalarToVector128(0.00384872F);
        //            lcwB3 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            break;
        //        // Hann(1997) FRL Research Contribution 17
        //        case FiaCode.AcerMacrophyllum:
        //            lcwB1 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            lcwB2 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            lcwB3 = AvxExtensions.BroadcastScalarToVector128(1.470180F);
        //            break;
        //        // Hann(1997) FRL Research Contribution 17
        //        case FiaCode.QuercusGarryana:
        //            lcwB1 = AvxExtensions.BroadcastScalarToVector128(0.3648110F);
        //            lcwB2 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            lcwB3 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            break;
        //        // Hann(1997) FRL Research Contribution 17
        //        case FiaCode.AlnusRubra:
        //            lcwB1 = AvxExtensions.BroadcastScalarToVector128(0.3227140F);
        //            lcwB2 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            lcwB3 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            break;
        //        // GC of Hann(1997) FRL Research Contribution 17
        //        case FiaCode.CornusNuttallii:
        //        case FiaCode.Salix:
        //            lcwB1 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            lcwB2 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            lcwB3 = AvxExtensions.BroadcastScalarToVector128(1.61440F);
        //            break;
        //        default:
        //            throw Trees.CreateUnhandledSpeciesException(species);
        //    }

        //    // coefficients for crown width
        //    Vector128<float> cwB1;
        //    Vector128<float> cwB2;
        //    Vector128<float> cwB3;
        //    Vector128<float> cwMaxHeightDiameterRatio = AvxExtensions.BroadcastScalarToVector128(Single.MaxValue);
        //    switch (species)
        //    {
        //        // DF Coefficients from Hann(1999) FS 45: 217-225
        //        case FiaCode.PseudotsugaMenziesii:
        //            cwB1 = AvxExtensions.BroadcastScalarToVector128(0.929973F);
        //            cwB2 = AvxExtensions.BroadcastScalarToVector128(-0.135212F);
        //            cwB3 = AvxExtensions.BroadcastScalarToVector128(-0.0157579F);
        //            cwMaxHeightDiameterRatio = AvxExtensions.BroadcastScalarToVector128(50.0F); // BUGBUG: Fortran code divides feet by inches?
        //            break;
        //        // GF Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
        //        case FiaCode.AbiesGrandis:
        //            cwB1 = AvxExtensions.BroadcastScalarToVector128(0.999291F);
        //            cwB2 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            cwB3 = AvxExtensions.BroadcastScalarToVector128(-0.0314603F);
        //            cwMaxHeightDiameterRatio = AvxExtensions.BroadcastScalarToVector128(31.0F); // BUGBUG: Fortran code divides feet by inches?
        //            break;
        //        // Marshall, Johnson, and Hann(2003) CJFR 33: 2059-2066
        //        case FiaCode.TsugaHeterophylla:
        //            cwB1 = AvxExtensions.BroadcastScalarToVector128(0.461782F);
        //            cwB2 = AvxExtensions.BroadcastScalarToVector128(0.552011F);
        //            cwB3 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            break;
        //        // Hann and Hanus(2001) FRL Research Contribution 34
        //        case FiaCode.ThujaPlicata:
        //        case FiaCode.TaxusBrevifolia:
        //            cwB1 = AvxExtensions.BroadcastScalarToVector128(0.629785F);
        //            cwB2 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            cwB3 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            break;
        //        // Hann and Hanus(2001) FRL Research Contribution 34
        //        case FiaCode.ArbutusMenziesii:
        //        case FiaCode.AcerMacrophyllum:
        //        case FiaCode.QuercusGarryana:
        //        case FiaCode.AlnusRubra:
        //        case FiaCode.CornusNuttallii:
        //        case FiaCode.Salix:
        //            cwB1 = AvxExtensions.BroadcastScalarToVector128(0.5F);
        //            cwB2 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            cwB3 = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //            break;
        //        default:
        //            throw Trees.CreateUnhandledSpeciesException(species);
        //    }

        //    Vector128<float> zero = AvxExtensions.BroadcastScalarToVector128(0.0F);
        //    Vector128<float> one = AvxExtensions.BroadcastScalarToVector128(1.0F);
        //    Vector128<float> v4p5 = AvxExtensions.BroadcastScalarToVector128(4.5F);
        //    Vector128<float> vCrownCompetitionConstantEnglish = AvxExtensions.BroadcastScalarToVector128(Constant.CrownCompetionConstantEnglish);
        //    Vector128<float> strataThickness = AvxExtensions.BroadcastScalarToVector128(crownCompetitionByHeight[^1] / Constant.HeightStrataAsFloat);
        //    fixed (float* dbh = &trees.Dbh[0], heights = &trees.Height[0], crownRatios = &trees.CrownRatio[0], expansionFactors = &trees.LiveExpansionFactor[0])
        //    {
        //        for (int treeIndex = 0; treeIndex < trees.Count; treeIndex += Constant.Simd128x4.Width)
        //        {
        //            Vector128<float> dbhInInches = Avx.LoadVector128(dbh + treeIndex);
        //            Vector128<float> heightInFeet = Avx.LoadVector128(heights + treeIndex);
        //            // DebugV.Assert(Avx.CompareGreaterThanOrEqual(heightInFeet, v4p5)); // Fortran case for maxCrownWidth = heightInFeet / 4.5F * mcwB0; removed

        //            // maximum crown width = B0 + B1 * dbh + B2 * dbh * dbh
        //            Vector128<float> dbhForMaxCrownWidth = Avx.Min(dbhInInches, dbhLimitForMaxCrownWidth);
        //            Vector128<float> maxCrownWidth = Avx.Add(mcwB0, Avx.Multiply(mcwB1, dbhForMaxCrownWidth));
        //            maxCrownWidth = Avx.Add(maxCrownWidth, Avx.Multiply(mcwB2, Avx.Multiply(dbhForMaxCrownWidth, dbhForMaxCrownWidth)));

        //            // height to crown base and largest crown width
        //            Vector128<float> crownRatio = Avx.LoadVector128(crownRatios + treeIndex);
        //            Vector128<float> crownLengthInFeet = Avx.Multiply(crownRatio, heightInFeet);
        //            Vector128<float> crownWidthMultiplierPower = Avx.Add(lcwB1, Avx.Multiply(lcwB2, crownLengthInFeet));
        //            crownWidthMultiplierPower = Avx.Add(crownWidthMultiplierPower, Avx.Multiply(lcwB3, Avx.Divide(dbhInInches, heightInFeet)));
        //            Vector128<float> crownWidthMultiplier = MathV.Pow(crownRatio, crownWidthMultiplierPower);
        //            Vector128<float> largestCrownWidth = Avx.Multiply(crownWidthMultiplier, maxCrownWidth);
        //            Vector128<float> heightToLargestCrownWidth = Avx.Subtract(heightInFeet, Avx.Multiply(Avx.Subtract(one, hlcwB1), crownLengthInFeet));
        //            Vector128<float> heightToCrownBaseInFeet = Avx.Subtract(heightInFeet, crownLengthInFeet);
        //            Vector128<float> cwB3heightDiameterRatio = Avx.Multiply(cwB3, Avx.Min(Avx.Divide(heightInFeet, dbhInInches), cwMaxHeightDiameterRatio));
        //            Vector128<float> crownBaseAboveLargestCrownWidth = Avx.CompareGreaterThan(heightToCrownBaseInFeet, heightToLargestCrownWidth);
        //            int crownBaseAboveLargestCrownWidthMask = Avx.MoveMask(crownBaseAboveLargestCrownWidth);
        //            if (crownBaseAboveLargestCrownWidthMask != 0)
        //            {
        //                Vector128<float> relativePosition = Avx.Divide(Avx.Subtract(heightInFeet, heightToCrownBaseInFeet), Avx.Subtract(heightInFeet, heightToLargestCrownWidth));
        //                //largestCrownWidth = this.GetCrownWidth(species, heightToLargestCrownWidth, largestCrownWidth, heightInFeet, dbhInInches, heightToCrownBaseInFeet);
        //                Vector128<float> crownWidthMultiplierAtCrownBase = MathV.Pow(relativePosition, Avx.Add(Avx.Add(cwB1, Avx.Multiply(cwB2, Avx.Sqrt(relativePosition))), cwB3heightDiameterRatio));
        //                Vector128<float> crownWidthAtCrownBase = Avx.Multiply(crownWidthMultiplierAtCrownBase, largestCrownWidth);
        //                largestCrownWidth = Avx.Blend(largestCrownWidth, crownWidthAtCrownBase, (byte)crownBaseAboveLargestCrownWidthMask);
        //                heightToLargestCrownWidth = Avx.Blend(heightToLargestCrownWidth, heightToCrownBaseInFeet, (byte)crownBaseAboveLargestCrownWidthMask);
        //            }

        //            // crown competition factor by strata
        //            Vector128<float> expansionFactor = Avx.LoadVector128(expansionFactors + treeIndex);
        //            Vector128<float> ccfExpansionFactor = Avx.Multiply(vCrownCompetitionConstantEnglish, expansionFactor);
        //            Vector128<float> crownCompetitionFactor = Avx.Multiply(ccfExpansionFactor, Avx.Multiply(largestCrownWidth, largestCrownWidth));
        //            Vector128<float> crownWidthEvaluationHeight = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(0.5F), strataThickness);
        //            for (int strataIndex = 0; strataIndex < crownCompetitionByHeight.Length - 1; ++strataIndex)
        //            {
        //                int evaluationHeightBelowTreetop = Avx.MoveMask(Avx.CompareLessThan(crownWidthEvaluationHeight, heightInFeet));
        //                if (evaluationHeightBelowTreetop == 0)
        //                {
        //                    // trees contribute no crown competition factor to strata above their height
        //                    break;
        //                }

        //                // find crown width and CCF for trees where strata is above their maximum crown width
        //                int evaluationHeightBelowWidestCrown = Avx.MoveMask(Avx.CompareLessThan(crownWidthEvaluationHeight, heightToLargestCrownWidth));
        //                if (evaluationHeightBelowWidestCrown != 0)
        //                {
        //                    Vector128<float> relativePosition = Avx.Divide(Avx.Subtract(heightInFeet, crownWidthEvaluationHeight), Avx.Subtract(heightInFeet, heightToLargestCrownWidth));
        //                    crownWidthMultiplier = MathV.Pow(relativePosition, Avx.Add(Avx.Add(cwB1, Avx.Multiply(cwB2, Avx.Sqrt(relativePosition))), cwB3heightDiameterRatio));
        //                    Vector128<float> crownWidthInStrata = Avx.Multiply(crownWidthMultiplier, largestCrownWidth);
        //                    Vector128<float> crownCompetitionFactorInStrata = Avx.Multiply(ccfExpansionFactor, Avx.Multiply(crownWidthInStrata, crownWidthInStrata));
        //                    crownCompetitionFactor = Avx.Blend(crownCompetitionFactor, crownCompetitionFactorInStrata, (byte)evaluationHeightBelowWidestCrown);
        //                }

        //                // zero out CCF for any trees shorter than this strata
        //                // No tail zeroing needed for last quad of trees as unused tree records have zero expansion factors and zero ccfExpansionFactor.
        //                crownCompetitionFactor = Avx.Blend(zero, crownCompetitionFactor, (byte)evaluationHeightBelowTreetop);

        //                Vector128<float> strataCrownCompetionFactorSum = Avx.HorizontalAdd(crownCompetitionFactor, crownCompetitionFactor);
        //                strataCrownCompetionFactorSum = Avx.HorizontalAdd(strataCrownCompetionFactorSum, strataCrownCompetionFactorSum);
        //                crownCompetitionByHeight[strataIndex] += strataCrownCompetionFactorSum.ToScalar();

        //                // move up to next strata
        //                crownWidthEvaluationHeight = Avx.Add(crownWidthEvaluationHeight, strataThickness);
        //            }
        //        }
        //    }
        //}

        private unsafe void AddCrownCompetitionByHeight128(Trees trees, float[] crownCompetitionByHeight)
        {
            OrganonCrownCoefficients crown = this.GetOrCreateCrownCoefficients(trees.Species);
            fixed (float* pinnedCrownCompetitionByHeight = &crownCompetitionByHeight[0])
            {
                for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
                {
                    float expansionFactor = trees.LiveExpansionFactor[treeIndex];
                    if (expansionFactor <= 0.0F)
                    {
                        continue;
                    }

                    float dbhInInches = trees.Dbh[treeIndex];
                    float heightInFeet = trees.Height[treeIndex];
                    float crownRatio = trees.CrownRatio[treeIndex];
                    float crownLengthInFeet = crownRatio * heightInFeet;

                    // maximum crown width
                    // Subset of code in GetMaximumCrownWidth().
                    Debug.Assert(heightInFeet >= 4.5F);
                    float dbhForMaxCrownWidth = MathF.Min(dbhInInches, crown.DbhLimitForMaxCrownWidth);
                    float maxCrownWidth;
                    if (crown.McwK == 1.0F)
                    {
                        // exponent is 1 for all species besides red alder in RAP variant, so no need to call MathV.Exp() and Pow()
                        maxCrownWidth = crown.McwB0 + crown.McwB1 * dbhForMaxCrownWidth + crown.McwB2 * dbhForMaxCrownWidth * dbhForMaxCrownWidth;
                    }
                    else
                    {
                        maxCrownWidth = crown.McwB0 + crown.McwB1 * MathV.Pow(dbhForMaxCrownWidth, crown.McwK) + crown.McwB2 * dbhForMaxCrownWidth * dbhForMaxCrownWidth;
                    }

                    // height to crown base and largest crown width
                    float largestCrownWidth = crown.LcwB0 * maxCrownWidth * MathV.Pow(crownRatio, crown.LcwB1 + crown.LcwB2 * crownLengthInFeet + crown.LcwB3 * dbhInInches / heightInFeet);
                    float heightToLargestCrownWidth;
                    if (crown.HlcwB2 == 0.0F)
                    {
                        // exponent is 1 for all species besides red alder in RAP variant, so no need to call MathV.Exp() and Pow()
                        heightToLargestCrownWidth = heightInFeet - (1.0F - crown.HlcwB1) * crownLengthInFeet;
                    }
                    else
                    {
                        heightToLargestCrownWidth = heightInFeet - (1.0F - crown.HlcwB1 * MathV.Exp(MathF.Pow(crown.HlcwB2 * (1.0F - heightInFeet / 140.0F), 3))) * crownLengthInFeet;
                    }
                    float heightToCrownBaseInFeet = heightInFeet - crownLengthInFeet;
                    float cwB3heightDiameterRatio = crown.CWb3 * MathF.Min(heightInFeet / dbhInInches, crown.CWMaxHeightDiameterRatio);
                    if (heightToCrownBaseInFeet > heightToLargestCrownWidth)
                    {
                        float relativePosition = (heightInFeet - heightToCrownBaseInFeet) / (heightInFeet - heightToLargestCrownWidth);
                        largestCrownWidth *= MathV.Pow(relativePosition, crown.CWb1 + crown.CWb2 * MathF.Sqrt(relativePosition) + cwB3heightDiameterRatio);
                        heightToLargestCrownWidth = heightToCrownBaseInFeet;
                    }

                    // crown competition factor by strata
                    float ccfExpansionFactor = 0.001803F * expansionFactor;
                    Vector128<float> ccfExpansionFactor128 = AvxExtensions.BroadcastScalarToVector128(ccfExpansionFactor);
                    Vector128<float> crownCompetitionFactor = AvxExtensions.BroadcastScalarToVector128(ccfExpansionFactor * largestCrownWidth * largestCrownWidth);
                    Vector128<float> cwB1_128 = AvxExtensions.BroadcastScalarToVector128(crown.CWb1);
                    Vector128<float> cwB2_128 = AvxExtensions.BroadcastScalarToVector128(crown.CWb2);
                    Vector128<float> cwB3heightDiameterRatio128 = AvxExtensions.BroadcastScalarToVector128(cwB3heightDiameterRatio);
                    Vector128<float> heightInFeet128 = AvxExtensions.BroadcastScalarToVector128(heightInFeet);
                    Vector128<float> heightToLargestCrownWidth128 = AvxExtensions.BroadcastScalarToVector128(heightToLargestCrownWidth);
                    Vector128<float> largestCrownWidth128 = AvxExtensions.BroadcastScalarToVector128(largestCrownWidth);
                    Vector128<float> strataHeightIncrement = AvxExtensions.BroadcastScalarToVector128(4.0F * crownCompetitionByHeight[^1] / Constant.OrganonHeightStrata);
                    Vector128<float> strataHeight = Avx.Multiply(Vector128.Create(0.25F, 0.50F, 0.75F, 1.0F), strataHeightIncrement); // find CCF at top of strata as in Fortran
                    for (int strataIndex = 0; strataIndex < crownCompetitionByHeight.Length - 2; strataIndex += Constant.Simd128x4.Width)
                    {
                        int strataBelowTreeHeightMask = Avx.MoveMask(Avx.CompareLessThan(strataHeight, heightInFeet128));
                        if (strataBelowTreeHeightMask == 0)
                        {
                            // tree contributes no crown competition factor above its height
                            break;
                        }

                        // find crown width and lowered CCFs for any strata above height of largest crown width
                        int strataAboveLargestCrownMask = Avx.MoveMask(Avx.CompareGreaterThan(strataHeight, heightToLargestCrownWidth128));
                        if (strataAboveLargestCrownMask != 0)
                        {
                            // very slightly faster to divide than to precompute denominator reciprocal
                            Vector128<float> relativePosition = Avx.Divide(Avx.Subtract(heightInFeet128, strataHeight), Avx.Subtract(heightInFeet128, heightToLargestCrownWidth128));
                            Vector128<float> largestWidthMultiplier = MathV.Pow(relativePosition, Avx.Add(cwB1_128, Avx.Add(Avx.Multiply(cwB2_128, Avx.Sqrt(relativePosition)), cwB3heightDiameterRatio128)));
                            Vector128<float> crownWidthInStrata = Avx.Multiply(largestCrownWidth128, largestWidthMultiplier);
                            Vector128<float> crownCompetitionFactorInStrata = Avx.Multiply(ccfExpansionFactor128, Avx.Multiply(crownWidthInStrata, crownWidthInStrata));
                            crownCompetitionFactor = Avx.Blend(crownCompetitionFactor, crownCompetitionFactorInStrata, (byte)strataAboveLargestCrownMask);
                        }

                        // zero any elements above tree height
                        crownCompetitionFactor = Avx.Blend(Vector128<float>.Zero, crownCompetitionFactor, (byte)strataBelowTreeHeightMask);

                        // accumulate CCF
                        Vector128<float> crownCompetitionByHeight128 = Avx.LoadVector128(pinnedCrownCompetitionByHeight + strataIndex);
                        crownCompetitionByHeight128 = Avx.Add(crownCompetitionByHeight128, crownCompetitionFactor);
                        Avx.Store(pinnedCrownCompetitionByHeight + strataIndex, crownCompetitionByHeight128);

                        // move upwards to next quad of strata
                        strataHeight = Avx.Add(strataHeight, strataHeightIncrement);
                    }
                }
            }

            // reference scalar implementation
            //    for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
            //    {
            //        float expansionFactor = trees.LiveExpansionFactor[treeIndex];
            //        if (expansionFactor <= 0.0F)
            //        {
            //            continue;
            //        }

            //        float dbhInInches = trees.Dbh[treeIndex];
            //        float heightInFeet = trees.Height[treeIndex];
            //        float crownRatio = trees.CrownRatio[treeIndex];
            //        float crownLengthInFeet = crownRatio * heightInFeet;

            //        // maximum crown width
            //        float dbhForMaxCrownWidth = MathF.Min(dbhInInches, dbhLimitForMaxCrownWidth);
            //        float maxCrownWidth;
            //        if (heightInFeet < 4.5F)
            //        {
            //            maxCrownWidth = heightInFeet / 4.5F * mcwB0;
            //        }
            //        else
            //        {
            //            maxCrownWidth = mcwB0 + mcwB1 * dbhForMaxCrownWidth + mcwB2 * dbhForMaxCrownWidth * dbhForMaxCrownWidth;
            //        }

            //        // height to crown base and largest crown width
            //        float largestCrownWidth = maxCrownWidth * MathV.Pow(crownRatio, lcwB1 + lcwB2 * crownLengthInFeet + lcwB3 * dbhInInches / heightInFeet);
            //        float heightToLargestCrownWidth = heightInFeet - (1.0F - hlcwB1) * crownLengthInFeet;
            //        float heightToCrownBaseInFeet = heightInFeet - crownLengthInFeet;
            //        float cwB3heightDiameterRatio = cwB3 * MathF.Min(heightInFeet / dbhInInches, cwMaxHeightDiameterRatio);
            //        if (heightToCrownBaseInFeet > heightToLargestCrownWidth)
            //        {
            //            float relativePosition = (heightInFeet - heightToCrownBaseInFeet) / (heightInFeet - heightToLargestCrownWidth);
            //            largestCrownWidth *= MathV.Pow(relativePosition, cwB1 + cwB2 * MathF.Sqrt(relativePosition) + cwB3heightDiameterRatio);
            //            heightToLargestCrownWidth = heightToCrownBaseInFeet;
            //        }

            //        // crown competition factor by strata
            //        float ccfExpansionFactor = 0.001803F * expansionFactor;
            //        float crownCompetitionFactor = ccfExpansionFactor * largestCrownWidth * largestCrownWidth;
            //        float strataThickness = crownCompetitionByHeight[^1] / Constant.HeightStrataAsFloat;
            //        for (int strataIndex = 0; strataIndex < crownCompetitionByHeight.Length - 1; ++strataIndex)
            //        {
            //            float crownWidthEvaluationHeight = strataThickness * ((float)strataIndex + 0.5F);
            //            if (crownWidthEvaluationHeight > heightInFeet)
            //            {
            //                // tree contributes no crown competition factor above its height
            //                break;
            //            }

            //            if (crownWidthEvaluationHeight > heightToLargestCrownWidth)
            //            {
            //                float relativePosition = (heightInFeet - crownWidthEvaluationHeight) / (heightInFeet - heightToLargestCrownWidth);
            //                float crownWidthInStrata = largestCrownWidth * MathV.Pow(relativePosition, cwB1 + cwB2 * MathF.Sqrt(relativePosition) + cwB3heightDiameterRatio);
            //                // crownWidth = this.GetCrownWidth(species, heightToLargestCrownWidth, largestCrownWidth, heightInFeet, dbhInInches, relativeHeight);
            //                crownCompetitionFactor = ccfExpansionFactor * crownWidthInStrata * crownWidthInStrata;
            //            }
            //            crownCompetitionByHeight[strataIndex] += crownCompetitionFactor;
            //        }
            //    }
            //}
        }

        private unsafe void AddCrownCompetitionByHeight256(Trees trees, float[] crownCompetitionByHeight)
        {
            OrganonCrownCoefficients crown = this.GetOrCreateCrownCoefficients(trees.Species);
            fixed (float* pinnedCrownCompetitionByHeight = &crownCompetitionByHeight[0])
            {
                for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
                {
                    float expansionFactor = trees.LiveExpansionFactor[treeIndex];
                    if (expansionFactor <= 0.0F)
                    {
                        continue;
                    }

                    float dbhInInches = trees.Dbh[treeIndex];
                    float heightInFeet = trees.Height[treeIndex];
                    float crownRatio = trees.CrownRatio[treeIndex];
                    float crownLengthInFeet = crownRatio * heightInFeet;

                    // maximum crown width
                    // Subset of code in GetMaximumCrownWidth().
                    Debug.Assert(heightInFeet >= 4.5F);
                    float dbhForMaxCrownWidth = MathF.Min(dbhInInches, crown.DbhLimitForMaxCrownWidth);
                    float maxCrownWidth;
                    if (crown.McwK == 1.0F)
                    {
                        // exponent is 1 for all species besides red alder in RAP variant, so no need to call MathV.Exp() and Pow()
                        maxCrownWidth = crown.McwB0 + crown.McwB1 * dbhForMaxCrownWidth + crown.McwB2 * dbhForMaxCrownWidth * dbhForMaxCrownWidth;
                    }
                    else
                    {
                        maxCrownWidth = crown.McwB0 + crown.McwB1 * MathV.Pow(dbhForMaxCrownWidth, crown.McwK) + crown.McwB2 * dbhForMaxCrownWidth * dbhForMaxCrownWidth;
                    }

                    // height to crown base and largest crown width
                    float largestCrownWidth = crown.LcwB0 * maxCrownWidth * MathV.Pow(crownRatio, crown.LcwB1 + crown.LcwB2 * crownLengthInFeet + crown.LcwB3 * dbhInInches / heightInFeet);
                    float heightToLargestCrownWidth;
                    if (crown.HlcwB2 == 0.0F)
                    {
                        // exponent is 1 for all species besides red alder in RAP variant, so no need to call MathV.Exp() and Pow()
                        heightToLargestCrownWidth = heightInFeet - (1.0F - crown.HlcwB1) * crownLengthInFeet;
                    }
                    else
                    {
                        heightToLargestCrownWidth = heightInFeet - (1.0F - crown.HlcwB1 * MathV.Exp(MathF.Pow(crown.HlcwB2 * (1.0F - heightInFeet / 140.0F), 3))) * crownLengthInFeet;
                    }
                    float heightToCrownBaseInFeet = heightInFeet - crownLengthInFeet;
                    float cwB3heightDiameterRatio = crown.CWb3 * MathF.Min(heightInFeet / dbhInInches, crown.CWMaxHeightDiameterRatio);
                    if (heightToCrownBaseInFeet > heightToLargestCrownWidth)
                    {
                        float relativePosition = (heightInFeet - heightToCrownBaseInFeet) / (heightInFeet - heightToLargestCrownWidth);
                        largestCrownWidth *= MathV.Pow(relativePosition, crown.CWb1 + crown.CWb2 * MathF.Sqrt(relativePosition) + cwB3heightDiameterRatio);
                        heightToLargestCrownWidth = heightToCrownBaseInFeet;
                    }

                    // crown competition factor by strata
                    float ccfExpansionFactor = 0.001803F * expansionFactor;
                    Vector256<float> ccfExpansionFactor256 = AvxExtensions.BroadcastScalarToVector256(ccfExpansionFactor);
                    Vector256<float> crownCompetitionFactor = AvxExtensions.BroadcastScalarToVector256(ccfExpansionFactor * largestCrownWidth * largestCrownWidth);
                    Vector256<float> cwB1_256 = AvxExtensions.BroadcastScalarToVector256(crown.CWb1);
                    Vector256<float> cwB2_256 = AvxExtensions.BroadcastScalarToVector256(crown.CWb2);
                    Vector256<float> cwB3heightDiameterRatio256 = AvxExtensions.BroadcastScalarToVector256(cwB3heightDiameterRatio);
                    Vector256<float> heightInFeet256 = AvxExtensions.BroadcastScalarToVector256(heightInFeet);
                    Vector256<float> heightToLargestCrownWidth256 = AvxExtensions.BroadcastScalarToVector256(heightToLargestCrownWidth);
                    Vector256<float> largestCrownWidth256 = AvxExtensions.BroadcastScalarToVector256(largestCrownWidth);
                    Vector256<float> strataHeightIncrement = AvxExtensions.BroadcastScalarToVector256(8.0F * crownCompetitionByHeight[^1] / Constant.OrganonHeightStrata);
                    Vector256<float> strataHeight = Avx.Multiply(Vector256.Create(0.125F, 0.250F, 0.375F, 0.500F, 0.625F, 0.750F, 0.875F, 1.00F), strataHeightIncrement); // find CCF at top of strata as in Fortran
                    for (int strataIndex = 0; strataIndex < crownCompetitionByHeight.Length - 2; strataIndex += Constant.Simd256x8.Width)
                    {
                        int strataBelowTreeHeightMask = Avx.MoveMask(Avx.CompareLessThan(strataHeight, heightInFeet256));
                        if (strataBelowTreeHeightMask == 0)
                        {
                            // tree contributes no crown competition factor above its height
                            break;
                        }

                        // find crown width and lowered CCFs for any strata above height of largest crown width
                        int strataAboveLargestCrownMask = Avx.MoveMask(Avx.CompareGreaterThan(strataHeight, heightToLargestCrownWidth256));
                        if (strataAboveLargestCrownMask != 0)
                        {
                            // very slightly faster to divide than to precompute denominator reciprocal
                            Vector256<float> relativePosition = Avx.Divide(Avx.Subtract(heightInFeet256, strataHeight), Avx.Subtract(heightInFeet256, heightToLargestCrownWidth256));
                            Vector256<float> largestWidthMultiplier = MathV.Pow(relativePosition, Avx.Add(cwB1_256, Avx.Add(Avx.Multiply(cwB2_256, Avx.Sqrt(relativePosition)), cwB3heightDiameterRatio256)));
                            Vector256<float> crownWidthInStrata = Avx.Multiply(largestCrownWidth256, largestWidthMultiplier);
                            Vector256<float> crownCompetitionFactorInStrata = Avx.Multiply(ccfExpansionFactor256, Avx.Multiply(crownWidthInStrata, crownWidthInStrata));
                            crownCompetitionFactor = Avx.Blend(crownCompetitionFactor, crownCompetitionFactorInStrata, (byte)strataAboveLargestCrownMask);
                        }

                        // zero any elements above tree height
                        crownCompetitionFactor = Avx.Blend(Vector256<float>.Zero, crownCompetitionFactor, (byte)strataBelowTreeHeightMask);

                        // accumulate CCF
                        Vector256<float> crownCompetitionByHeight256 = Avx.LoadVector256(pinnedCrownCompetitionByHeight + strataIndex);
                        crownCompetitionByHeight256 = Avx.Add(crownCompetitionByHeight256, crownCompetitionFactor);
                        Avx.Store(pinnedCrownCompetitionByHeight + strataIndex, crownCompetitionByHeight256);

                        // move upwards to next quad of strata
                        strataHeight = Avx.Add(strataHeight, strataHeightIncrement);
                    }
                }
            }
        }

        public static OrganonVariant Create(TreeModel treeModel)
        {
            return treeModel switch
            {
                TreeModel.OrganonNwo => new OrganonVariantNwo(),
                TreeModel.OrganonSwo => new OrganonVariantSwo(),
                TreeModel.OrganonSmc => new OrganonVariantSmc(),
                TreeModel.OrganonRap => new OrganonVariantRap(),
                _ => throw OrganonVariant.CreateUnhandledModelException(treeModel),
            };
        }

        protected abstract OrganonCrownCoefficients CreateCrownCoefficients(FiaCode species);

        protected abstract OrganonHeightCoefficients CreateHeightCoefficients(FiaCode species);

        public static NotSupportedException CreateUnhandledModelException(TreeModel treeModel)
        {
            return new NotSupportedException(String.Format("Unhandled model {0}.", treeModel));
        }

        protected static float GetCrownCompetitionFactorByHeight(float height, float[] crownCompetitionByHeight)
        {
            if (height >= crownCompetitionByHeight[^1])
            {
                return 0.0F;
            }

            Debug.Assert(crownCompetitionByHeight.Length == Constant.OrganonHeightStrata + 1);
            int strataIndex = (int)(Constant.OrganonHeightStrata * height / crownCompetitionByHeight[^1]);
            if (strataIndex >= crownCompetitionByHeight.Length)
            {
                Debug.Fail("Strata index should not be greater than the length of the crown competition array. This case was previously checked for.");
                return 0.0F;
            }
            return crownCompetitionByHeight[strataIndex + 1];
        }

        protected static unsafe Vector128<float> GetCrownCompetitionFactorByHeight(Vector128<float> height, float[] crownCompetitionByHeight)
        {
            // this is called during GrowHeight() with grown height but before crown competition has been recomputed for new heights
            // As a result, indices well beyond the end of the crown competition array can be generated and must be clamped. If needed, the code 
            // here can be made slightly more efficient by adding a guard strata whose competition factor is always zero and vectorizing the
            // compare and clamp.
            Debug.Assert(crownCompetitionByHeight[^1] > 4.5F);
            Vector128<float> strataIndexAsFloat = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128((float)Constant.OrganonHeightStrata / crownCompetitionByHeight[^1]), height);
            Vector128<int> strataIndex = Avx.ConvertToVector128Int32WithTruncation(strataIndexAsFloat);
            DebugV.Assert(Avx.CompareGreaterThan(strataIndex, AvxExtensions.Set128(-1))); // no integer >=
            DebugV.Assert(Avx.CompareLessThan(strataIndex, AvxExtensions.Set128(2 * Constant.OrganonHeightStrata))); // factor of 2 empirically fitted to tests, likely fragile

            // AVX implementation of Avx2.GatherVector128(&crownCompetitionByHeight[0], strataIndex, 1)
            float crownCompetitionFactor0 = 0.0F;
            int crownCompetitionIndex0 = strataIndex.ToScalar();
            if (crownCompetitionIndex0 < Constant.OrganonHeightStrata)
            {
                crownCompetitionFactor0 = crownCompetitionByHeight[crownCompetitionIndex0];
            }
            float crownCompetitionFactor1 = 0.0F;
            int crownCompetitionIndex1 = Avx.Shuffle(strataIndex, Constant.Simd128x4.ShuffleRotateLower1).ToScalar();
            if (crownCompetitionIndex1 < Constant.OrganonHeightStrata)
            {
                crownCompetitionFactor1 = crownCompetitionByHeight[crownCompetitionIndex1];
            }
            float crownCompetitionFactor2 = 0.0F;
            int crownCompetitionIndex2 = Avx.Shuffle(strataIndex, Constant.Simd128x4.ShuffleRotateLower2).ToScalar();
            if (crownCompetitionIndex2 < Constant.OrganonHeightStrata)
            {
                crownCompetitionFactor2 = crownCompetitionByHeight[crownCompetitionIndex2];
            }
            float crownCompetitionFactor3 = 0.0F;
            int crownCompetitionIndex3 = Avx.Shuffle(strataIndex, Constant.Simd128x4.ShuffleRotateLower3).ToScalar();
            if (crownCompetitionIndex3 < Constant.OrganonHeightStrata)
            {
                crownCompetitionFactor3 = crownCompetitionByHeight[crownCompetitionIndex3];
            }

            return Vector128.Create(crownCompetitionFactor0, crownCompetitionFactor1, crownCompetitionFactor2, crownCompetitionFactor3);
        }

        protected static unsafe Vector256<float> GetCrownCompetitionFactorByHeight(Vector256<float> height, float[] crownCompetitionByHeight)
        {
            // this is called during GrowHeight() with grown height but before crown competition has been recomputed for new heights
            // As a result, indices well beyond the end of the crown competition array can be generated and must be clamped. If needed, the code 
            // here can be made slightly more efficient by adding a guard strata whose competition factor is always zero and vectorizing the
            // compare and clamp.
            Debug.Assert(crownCompetitionByHeight[^1] > 4.5F);
            Vector256<float> strataIndexAsFloat = Avx.Multiply(AvxExtensions.BroadcastScalarToVector256((float)Constant.OrganonHeightStrata / crownCompetitionByHeight[^1]), height);
            Vector256<int> strataIndex = Avx.ConvertToVector256Int32WithTruncation(strataIndexAsFloat);
            DebugV.Assert(Avx2.CompareGreaterThan(strataIndex, AvxExtensions.Set256(-1))); // no integer >=
            DebugV.Assert(Avx2.CompareGreaterThan(AvxExtensions.Set256(2 * Constant.OrganonHeightStrata), strataIndex)); // factor of 2 empirically fitted to tests, likely fragile

            fixed (float* crownCompetition = crownCompetitionByHeight)
            {
                Vector256<float> crownCompetitionFactor = Avx2.GatherVector256(crownCompetition, strataIndex, sizeof(float));
                return crownCompetitionFactor;
            }
        }

        protected static float GetCrownRatioAdjustment(float crownRatio)
        {
            if (crownRatio > 0.11F)
            {
                return 1.0F; // accurate within 0.05%
            }

            // slowdowns typically measured with fifth order polynomial approximation in Douglas-fir benchmark
            // This appears associated with trees falling under the if statement above.
            return 1.0F - MathV.Exp(-(25.0F * 25.0F * crownRatio * crownRatio));
        }

        protected static Vector128<float> GetCrownRatioAdjustment(Vector128<float> crownRatio)
        {
            Vector128<float> crownRatioAdjustment = AvxExtensions.BroadcastScalarToVector128(1.0F);
            int exponentMask = Avx.MoveMask(Avx.CompareLessThan(crownRatio, AvxExtensions.BroadcastScalarToVector128(0.11F)));
            if (exponentMask != 0)
            {
                Vector128<float> power = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128(-25.0F * 25.0F), Avx.Multiply(crownRatio, crownRatio));
                Vector128<float> exponent = MathV.MaskExp(power, (byte)exponentMask);
                crownRatioAdjustment = Avx.Subtract(crownRatioAdjustment, exponent);
                DebugV.Assert(Avx.CompareGreaterThanOrEqual(crownRatioAdjustment, Vector128<float>.Zero));
                DebugV.Assert(Avx.CompareLessThanOrEqual(crownRatioAdjustment, AvxExtensions.BroadcastScalarToVector128(1.0F)));
            }
            return crownRatioAdjustment;
        }

        protected static Vector256<float> GetCrownRatioAdjustment(Vector256<float> crownRatio)
        {
            Vector256<float> crownRatioAdjustment = AvxExtensions.BroadcastScalarToVector256(1.0F);
            int exponentMask = Avx.MoveMask(Avx.CompareLessThan(crownRatio, AvxExtensions.BroadcastScalarToVector256(0.11F)));
            if (exponentMask != 0)
            {
                Vector256<float> power = Avx.Multiply(AvxExtensions.BroadcastScalarToVector256(-25.0F * 25.0F), Avx.Multiply(crownRatio, crownRatio));
                Vector256<float> exponent = MathV.MaskExp(power, (byte)exponentMask);
                crownRatioAdjustment = Avx.Subtract(crownRatioAdjustment, exponent);
            }
            return crownRatioAdjustment;
        }

        public int GetEndYear(int simulationStep)
        {
            return this.TimeStepInYears * (simulationStep + 1);
        }

        public abstract float GetGrowthEffectiveAge(OrganonConfiguration configuration, OrganonStand stand, Trees trees, int treeIndex, out float potentialHeightGrowth);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="organonStand">Stand tree is a part of.</param>
        /// <param name="species">Tree's species.</param>
        /// <param name="heightInFeet">Tree height (feet).</param>
        /// <param name="dbhInInches">Tree's diameter at breast height (inches)</param>
        /// <param name="CCFL">Crown competition factor from trees larger than the tree being evaluated.</param>
        /// <param name="basalAreaPerHa">Stand basal area.</param>
        /// <param name="oldGrowthIndex">Mostly applied in SWO. Only used for Pacific madrone in NWO and SMC. Not used in RAP.</param>
        /// <returns>Height to crown base (feet).</returns>
        public virtual float GetHeightToCrownBase(OrganonStand organonStand, FiaCode species, float heightInFeet, float dbhInInches, float CCFL, StandDensity standDensity, float oldGrowthIndex)
        {
            OrganonCrownCoefficients crown = this.GetOrCreateCrownCoefficients(species);
            float siteIndexFromDbh = this.GetSiteIndex(organonStand, species) - 4.5F;

            // this line of code also appears in GrowCrown() but needs to be called on its own during tree initialization
            float basalAreaPerAcre = Constant.HectaresPerAcre * Constant.SquareFeetPerSquareMeter * standDensity.BasalAreaPerHa;
            float heightToCrownBase = heightInFeet / (1.0F + MathV.Exp(crown.HcbB0 + crown.HcbB1 * heightInFeet + crown.HcbB2 * CCFL + crown.HcbB3 * MathV.Ln(basalAreaPerAcre) + crown.HcbB4 * (dbhInInches / heightInFeet) + crown.HcbB5 * siteIndexFromDbh + crown.HcbB6 * oldGrowthIndex * oldGrowthIndex));
            Debug.Assert((heightInFeet > 0) && (heightToCrownBase >= 0.0F) && (heightToCrownBase <= heightInFeet));
            return heightToCrownBase;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="species">Tree's species.</param>
        /// <param name="dbhInInches">Tree's diameter at breast height (inches).</param>
        /// <param name="heightInFeet">Tree's height (feet).</param>
        /// <returns>Estimated maximum crown width (in feet, presumably).</returns>
        public float GetMaximumCrownWidth(FiaCode species, float dbhInInches, float heightInFeet)
        {
            OrganonCrownCoefficients crown = this.GetOrCreateCrownCoefficients(species);
            if (heightInFeet < 4.501F)
            {
                return heightInFeet / 4.5F * crown.McwB0;
            }

            float dbhForMaxCrownWidth = dbhInInches;
            if (dbhInInches > crown.DbhLimitForMaxCrownWidth)
            {
                dbhForMaxCrownWidth = crown.DbhLimitForMaxCrownWidth;
            }

            float maxCrownWidth;
            if (crown.McwK == 1.0F)
            {
                // exponent is 1 for all species besides red alder in RAP variant, so no need to call MathV.Exp() and Pow()
                maxCrownWidth = crown.McwB0 + crown.McwB1 * dbhForMaxCrownWidth + crown.McwB2 * dbhForMaxCrownWidth * dbhForMaxCrownWidth;
            }
            else
            {
                maxCrownWidth = crown.McwB0 + crown.McwB1 * MathV.Pow(dbhForMaxCrownWidth, crown.McwK) + crown.McwB2 * dbhForMaxCrownWidth * dbhForMaxCrownWidth;
            }
            return maxCrownWidth;
        }

        protected OrganonCrownCoefficients GetOrCreateCrownCoefficients(FiaCode species)
        {
            if (this.crownCoefficients.TryGetValue(species, out OrganonCrownCoefficients? crownCoefficients) == false)
            {
                lock (this.crownCoefficients)
                {
                    if (this.crownCoefficients.TryGetValue(species, out crownCoefficients) == false)
                    {
                        crownCoefficients = this.CreateCrownCoefficients(species);
                        this.crownCoefficients.Add(species, crownCoefficients);
                    }
                }
            }
            return crownCoefficients;
        }

        public OrganonHeightCoefficients GetOrCreateHeightCoefficients(FiaCode species)
        {
            if (this.heightCoefficients.TryGetValue(species, out OrganonHeightCoefficients? heightCoefficients) == false)
            {
                lock (this.heightCoefficients)
                {
                    if (this.heightCoefficients.TryGetValue(species, out heightCoefficients) == false)
                    {
                        heightCoefficients = this.CreateHeightCoefficients(species);
                        this.heightCoefficients.Add(species, heightCoefficients);
                    }
                }
            }
            return heightCoefficients;
        }

        protected virtual float GetSiteIndex(OrganonStand stand, FiaCode species)
        {
            if (species == FiaCode.TsugaHeterophylla)
            {
                return stand.HemlockSiteIndexInFeet;
            }
            return stand.SiteIndexInFeet;
        }

        // RAP and SWO use this default implementation
        public virtual void GrowCrown(OrganonStand stand, Trees trees, OrganonStandDensity densityAfterGrowth, float oldGrowthIndicator, float nwoSmcCrownRatioMultiplier)
        {
            OrganonCrownCoefficients crown = this.GetOrCreateCrownCoefficients(trees.Species);
            for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
            {
                float endDbhInInches = trees.Dbh[treeIndex];
                float endHeightInFeet = trees.Height[treeIndex];

                // get height to crown base at start of period
                float startDbhInInches = endDbhInInches - trees.DbhGrowth[treeIndex]; // diameter at end of step
                float startHeightInFeet = endHeightInFeet - trees.HeightGrowth[treeIndex]; // height at beginning of step
                Debug.Assert(startDbhInInches >= 0.0F);
                Debug.Assert(startHeightInFeet >= 0.0F);

                // crown ratio is still the starting crown ratio at this point, so it would be expected no start crown ratio would be needed
                // However, Fortran code has quite curious behavior where it runs the equivalent of the commented out code below to calculate its internal
                // expectation of a tree's crown ratio. This seems very likely to be a defect.
                // float startCcfl = densityBeforeGrowth.GetCrownCompetitionFactorLarger(startDbh);
                // float startHeightToCrownBase = variant.GetHeightToCrownBase(species, startHeight, startDbh, startCcfl, densityBeforeGrowth.BasalAreaPerAcre, SI_1, SI_2, OG1);
                // float startCrownRatio = 1.0F - startHeightToCrownBase / startHeight;
                // if (variant.TreeModel == TreeModel.OrganonNwo)
                // {
                //     startCrownRatio = calibrationBySpecies[species].CrownRatio * (1.0F - startHeightToCrownBase / startHeight);
                // }
                //startHeightToCrownBase = (1.0F - startCrownRatio) * startHeight;
                float startCrownRatio = trees.CrownRatio[treeIndex];
                float startHeightToCrownBase = (1.0F - startCrownRatio) * startHeightInFeet;

                // get height to crown base at end of period
                float endCcfl = densityAfterGrowth.GetCrownCompetitionFactorLarger(endDbhInInches);
                float endHeightToCrownBase = this.GetHeightToCrownBase(stand, trees.Species, endHeightInFeet, endDbhInInches, endCcfl, densityAfterGrowth, oldGrowthIndicator);
                float heightToCrownBaseRatio = crown.MhcbB0 - crown.MhcbB1 * MathV.Exp(crown.MhcbB2 * MathV.Pow(endCcfl / 100.0F, crown.MhcbB3));
                if (heightToCrownBaseRatio > crown.HeightToCrownBaseRatioLimit)
                {
                    heightToCrownBaseRatio = crown.HeightToCrownBaseRatioLimit;
                }
                float endMaxHeightToCrownBase = heightToCrownBaseRatio * endHeightInFeet;
                float endCrownRatio = 1.0F - endHeightToCrownBase / endHeightInFeet; // NWO overrides so NWO multiplier isn't needed here
                endHeightToCrownBase = (1.0F - endCrownRatio) * endHeightInFeet;

                // crown recession = increase in height of crown base
                float crownRecession = endHeightToCrownBase - startHeightToCrownBase;
                if (crownRecession < 0.0F)
                {
                    crownRecession = 0.0F;
                }
                Debug.Assert(crownRecession >= 0.0F); // catch NaNs

                // update tree's crown ratio
                float alternateHeightToCrownBase1 = (1.0F - trees.CrownRatio[treeIndex]) * startHeightInFeet;
                float alternateHeightToCrownBase2 = alternateHeightToCrownBase1 + crownRecession;
                if (alternateHeightToCrownBase1 >= endMaxHeightToCrownBase)
                {
                    trees.CrownRatio[treeIndex] = 1.0F - alternateHeightToCrownBase1 / endHeightInFeet;
                }
                else if (alternateHeightToCrownBase2 >= endMaxHeightToCrownBase)
                {
                    trees.CrownRatio[treeIndex] = 1.0F - endMaxHeightToCrownBase / endHeightInFeet;
                }
                else
                {
                    trees.CrownRatio[treeIndex] = 1.0F - alternateHeightToCrownBase2 / endHeightInFeet;
                }
                Debug.Assert((trees.CrownRatio[treeIndex] >= 0.0F) && (trees.CrownRatio[treeIndex] <= 1.0F));
            }
        }

        public abstract void GrowDiameter(Trees trees, float growthMultiplier, float siteIndexFromDbh, OrganonStandDensity densityBeforeGrowth);

        // scalar reference source
        //public virtual int GrowHeightBigSix(OrganonConfiguration configuration, OrganonStand stand, Trees trees, float[] crownCompetitionByHeight)
        //{
        //    float P1 = 1.0F;
        //    float P2;
        //    float P3;
        //    // float P4 = 0.5F; // sqrt()
        //    float P5;
        //    float P7 = 0.0F;
        //    float P8;
        //    switch (trees.Species)
        //    {
        //        // Hann, Marshall, and Hanus(2006) FRL Research Contribution ??
        //        case FiaCode.PseudotsugaMenziesii:
        //            P1 = 0.655258886F;
        //            P2 = -0.006322913F;
        //            P3 = -0.039409636F;
        //            P5 = 0.597617316F;
        //            P7 = 0.631643636F;
        //            P8 = 1.010018427F;
        //            break;
        //        // Ritchie and Hann(1990) FRL Research Paper 54
        //        case FiaCode.AbiesGrandis:
        //            P2 = -0.0328142F;
        //            P3 = -0.0127851F;
        //            // P4 = 1.0F;
        //            P5 = 6.19784F;
        //            P8 = 1.01F;
        //            break;
        //        // Johnson(2002) Willamette Industries Report
        //        case FiaCode.TsugaHeterophylla:
        //            P2 = -0.0384415F;
        //            P3 = -0.0144139F;
        //            P5 = 1.04409F;
        //            P8 = 1.03F;
        //            break;
        //        default:
        //            throw Trees.CreateUnhandledSpeciesException(trees.Species);
        //    }

        //    int oldTreeRecordCount = 0;
        //    DouglasFir.SiteConstants psmeSite = trees.Species == FiaCode.TsugaHeterophylla ? null : new DouglasFir.SiteConstants(stand.SiteIndex); // also used for grand fir
        //    WesternHemlock.SiteConstants tsheSite = trees.Species == FiaCode.TsugaHeterophylla ? new WesternHemlock.SiteConstants(stand.HemlockSiteIndex) : null;
        //    for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
        //    {
        //        if (trees.LiveExpansionFactor[treeIndex] <= 0.0F)
        //        {
        //            trees.HeightGrowth[treeIndex] = 0.0F;
        //            continue;
        //        }

        //        // inline version of GetGrowthEffectiveAge()
        //        float height = trees.Height[treeIndex];
        //        float growthEffectiveAge;
        //        float potentialHeightGrowth;
        //        if (trees.Species == FiaCode.TsugaHeterophylla)
        //        {
        //            growthEffectiveAge = WesternHemlock.GetFlewellingGrowthEffectiveAge(tsheSite, this.TimeStepInYears, height, out potentialHeightGrowth);
        //        }
        //        else
        //        {
        //            growthEffectiveAge = DouglasFir.GetBrucePsmeAbgrGrowthEffectiveAge(psmeSite, this.TimeStepInYears, height, out potentialHeightGrowth);
        //        }
        //        float crownCompetitionIncrement = this.GetCrownCompetitionFactorByHeight(height, crownCompetitionByHeight);
        //        float sqrtCrownCompetitionIncrement = MathF.Sqrt(crownCompetitionIncrement);
        //        float crownCompetitionIncrementToP4 = sqrtCrownCompetitionIncrement;
        //        if (trees.Species == FiaCode.AbiesGrandis)
        //        {
        //            crownCompetitionIncrementToP4 = crownCompetitionIncrement;
        //        }

        //        float crownRatio = trees.CrownRatio[treeIndex];
        //        float proportionBelowCrown = 1.0F - crownRatio;
        //        float B0 = P1 * MathV.Exp(P2 * crownCompetitionIncrement);
        //        float B1 = MathV.Exp(P3 * crownCompetitionIncrementToP4); // exp(P3 * sqrt(CCI)) for PSME and THSE, exp(P3 * CCI) for ABGR
        //        float FCR = -P5 * proportionBelowCrown * proportionBelowCrown * MathV.Exp(P7 * sqrtCrownCompetitionIncrement); // P7 is 0.0 for ABGR and TSHE -> exp() = 1.0
        //        float MODIFER = P8 * (B0 + (B1 - B0) * MathF.Exp(FCR));
        //        float CRADJ = OrganonGrowth.GetCrownRatioAdjustment(crownRatio);
        //        float heightGrowth = potentialHeightGrowth * MODIFER * CRADJ;
        //        Debug.Assert(heightGrowth > 0.0F);
        //        trees.HeightGrowth[treeIndex] = heightGrowth;

        //        if (growthEffectiveAge > configuration.Variant.OldTreeAgeThreshold)
        //        {
        //            ++oldTreeRecordCount;
        //        }
        //    }
        //    return oldTreeRecordCount;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="stand"></param>
        /// <param name="trees"></param>
        /// <param name="crownCompetitionByHeight">Percent stand level crown closure by height.</param>
        /// <returns>Height growth in feet.</param>
        public virtual int GrowHeightBigSix(OrganonConfiguration configuration, OrganonStand stand, Trees trees, float[] crownCompetitionByHeight)
        {
            return this.Simd switch
            {
                Simd.Width128 => this.GrowHeightBigSix128(configuration, stand, trees, crownCompetitionByHeight),
                Simd.Width256 => this.GrowHeightBigSix256(configuration, stand, trees, crownCompetitionByHeight),
                _ => throw new NotSupportedException("Unhandled SIMD " + this.Simd + ".")
            };
        }

        private unsafe int GrowHeightBigSix128(OrganonConfiguration configuration, OrganonStand stand, Trees trees, float[] crownCompetitionByHeight)
        {
            OrganonHeightCoefficients heightCoefficients = this.GetOrCreateHeightCoefficients(trees.Species);

            // equivalent of this.GetGrowthEffectiveAge()
            DouglasFir.SiteConstants? psmeSite = null;
            WesternHemlock.SiteConstants? tsheSite = null;
            if (trees.Species == FiaCode.TsugaHeterophylla)
            {
                tsheSite = new WesternHemlock.SiteConstants(stand.HemlockSiteIndexInFeet);
            }
            else
            {
                psmeSite = new DouglasFir.SiteConstants(stand.SiteIndexInFeet); // also used for grand fir
            }

            Vector128<float> P1 = AvxExtensions.BroadcastScalarToVector128(heightCoefficients.P1);
            Vector128<float> P2 = AvxExtensions.BroadcastScalarToVector128(heightCoefficients.P2);
            Vector128<float> P3 = AvxExtensions.BroadcastScalarToVector128(heightCoefficients.P3);
            Debug.Assert((heightCoefficients.P4 == 0.5F) || (heightCoefficients.P4 == 1.0F)); // P4 = 0.5F; => B1 = sqrt(P4)
            Vector128<float> minusP5 = AvxExtensions.BroadcastScalarToVector128(-heightCoefficients.P5);
            Debug.Assert(heightCoefficients.P6 == 2.0F); // P6 = 0.5F; => FCR = -P5 * (1 - crownRatio)^P6 * P7^sqrt(CCF)
            Vector128<float> P7 = AvxExtensions.BroadcastScalarToVector128(heightCoefficients.P7);
            Vector128<float> P8 = AvxExtensions.BroadcastScalarToVector128(heightCoefficients.P8);
            Vector128<float> oldTreeAgeThreshold = AvxExtensions.BroadcastScalarToVector128(configuration.Variant.OldTreeAgeThreshold);
            Vector128<float> one = AvxExtensions.BroadcastScalarToVector128(1.0F);

            Vector128<int> oldTreeRecordCount = Vector128<int>.Zero;
            fixed (float* crownRatios = &trees.CrownRatio[0], expansionFactors = &trees.LiveExpansionFactor[0], heights = &trees.Height[0], heightGrowths = &trees.HeightGrowth[0])
            {
                for (int treeIndex = 0; treeIndex < trees.Count; treeIndex += Constant.Simd128x4.Width)
                {
                    // inline version of GetGrowthEffectiveAge()
                    Vector128<float> height = Avx.LoadVector128(heights + treeIndex);
                    Vector128<float> growthEffectiveAge;
                    Vector128<float> potentialHeightGrowth;
                    if (trees.Species == FiaCode.TsugaHeterophylla)
                    {
                        growthEffectiveAge = WesternHemlock.GetFlewellingGrowthEffectiveAge(tsheSite!, this.TimeStepInYears, height, out potentialHeightGrowth);
                    }
                    else
                    {
                        growthEffectiveAge = DouglasFir.GetPsmeAbgrGrowthEffectiveAge(psmeSite!, this.TimeStepInYears, height, out potentialHeightGrowth);
                    }
                    Vector128<float> crownCompetitionFactor = OrganonVariant.GetCrownCompetitionFactorByHeight(height, crownCompetitionByHeight);
                    Vector128<float> sqrtCrownCompetitionFactor = Avx.Sqrt(crownCompetitionFactor);
                    Vector128<float> crownCompetitionIncrementToP4;
                    if (heightCoefficients.P4 == 0.5F)
                    {
                        crownCompetitionIncrementToP4 = sqrtCrownCompetitionFactor;
                    }
                    else
                    {
                        crownCompetitionIncrementToP4 = crownCompetitionFactor;
                    }

                    Vector128<float> crownRatio = Avx.LoadVector128(crownRatios + treeIndex);
                    Vector128<float> proportionBelowCrown = Avx.Subtract(one, crownRatio);
                    Vector128<float> B0 = Avx.Multiply(P1, MathV.Exp(Avx.Multiply(P2, crownCompetitionFactor)));
                    Vector128<float> B1 = MathV.Exp(Avx.Multiply(P3, crownCompetitionIncrementToP4)); // exp(P3 * sqrt(CCI)) for PSME and THSE, exp(P3 * CCI) for ABGR
                    Vector128<float> FCR = Avx.Multiply(Avx.Multiply(minusP5, Avx.Multiply(proportionBelowCrown, proportionBelowCrown)), MathV.Exp(Avx.Multiply(P7, sqrtCrownCompetitionFactor))); // P7 is 0.0 for ABGR and TSHE -> exp() = 1.0
                    Vector128<float> modifier = Avx.Multiply(P8, Avx.Add(B0, Avx.Multiply(Avx.Subtract(B1, B0), MathV.Exp(FCR))));
                    Vector128<float> crownRatioAdjustment = OrganonVariant.GetCrownRatioAdjustment(crownRatio);
                    Vector128<float> heightGrowth = Avx.Multiply(potentialHeightGrowth, Avx.Multiply(modifier, crownRatioAdjustment));
                    Vector128<float> expansionFactor = Avx.LoadVector128(expansionFactors + treeIndex); // maybe worth continuing in loop if all expansion factors are zero?
                    Vector128<float> zero = Vector128<float>.Zero;
                    heightGrowth = Avx.BlendVariable(heightGrowth, zero, Avx.CompareLessThanOrEqual(expansionFactor, zero));
                    DebugV.Assert(Avx.CompareGreaterThanOrEqual(heightGrowth, zero));
                    DebugV.Assert(Avx.CompareLessThanOrEqual(heightGrowth, AvxExtensions.BroadcastScalarToVector128(Constant.Maximum.HeightIncrementInFeet)));
                    Avx.Store(heightGrowths + treeIndex, heightGrowth);

                    // if growth effective age > old tree age is true, then 0xffff ffff = -1 is returned from the comparison
                    // Reinterpreting as Vector128<int> and subtracting therefore adds one to the old tree record counts where old trees occur.
                    oldTreeRecordCount = Avx.Subtract(oldTreeRecordCount, Avx.CompareGreaterThan(growthEffectiveAge, oldTreeAgeThreshold).AsInt32());
                }
            }

            oldTreeRecordCount = Avx.HorizontalAdd(oldTreeRecordCount, oldTreeRecordCount);
            oldTreeRecordCount = Avx.HorizontalAdd(oldTreeRecordCount, oldTreeRecordCount);
            return oldTreeRecordCount.ToScalar();
        }

        private unsafe int GrowHeightBigSix256(OrganonConfiguration configuration, OrganonStand stand, Trees trees, float[] crownCompetitionByHeight)
        {
            OrganonHeightCoefficients heightCoefficients = this.GetOrCreateHeightCoefficients(trees.Species);

            // equivalent of this.GetGrowthEffectiveAge()
            DouglasFir.SiteConstants? psmeSite = null;
            WesternHemlock.SiteConstants? tsheSite = null;
            if (trees.Species == FiaCode.TsugaHeterophylla)
            {
                tsheSite = new WesternHemlock.SiteConstants(stand.HemlockSiteIndexInFeet);
            }
            else
            {
                psmeSite = new DouglasFir.SiteConstants(stand.SiteIndexInFeet); // also used for grand fir
            }

            Vector256<float> P1 = AvxExtensions.BroadcastScalarToVector256(heightCoefficients.P1);
            Vector256<float> P2 = AvxExtensions.BroadcastScalarToVector256(heightCoefficients.P2);
            Vector256<float> P3 = AvxExtensions.BroadcastScalarToVector256(heightCoefficients.P3);
            Debug.Assert((heightCoefficients.P4 == 0.5F) || (heightCoefficients.P4 == 1.0F)); // P4 = 0.5F; => B1 = sqrt(P4)
            Vector256<float> minusP5 = AvxExtensions.BroadcastScalarToVector256(-heightCoefficients.P5);
            Debug.Assert(heightCoefficients.P6 == 2.0F); // P6 = 0.5F; => FCR = -P5 * (1 - crownRatio)^P6 * P7^sqrt(CCF)
            Vector256<float> P7 = AvxExtensions.BroadcastScalarToVector256(heightCoefficients.P7);
            Vector256<float> P8 = AvxExtensions.BroadcastScalarToVector256(heightCoefficients.P8);
            Vector256<float> oldTreeAgeThreshold = AvxExtensions.BroadcastScalarToVector256(configuration.Variant.OldTreeAgeThreshold);
            Vector256<float> one = AvxExtensions.BroadcastScalarToVector256(1.0F);

            Vector256<int> oldTreeRecordCount = Vector256<int>.Zero;
            fixed (float* crownRatios = &trees.CrownRatio[0], expansionFactors = &trees.LiveExpansionFactor[0], heights = &trees.Height[0], heightGrowths = &trees.HeightGrowth[0])
            {
                for (int treeIndex = 0; treeIndex < trees.Count; treeIndex += Constant.Simd256x8.Width)
                {
                    // inline version of GetGrowthEffectiveAge()
                    Vector256<float> height = Avx.LoadVector256(heights + treeIndex);
                    Vector256<float> growthEffectiveAge;
                    Vector256<float> potentialHeightGrowth;
                    if (trees.Species == FiaCode.TsugaHeterophylla)
                    {
                        growthEffectiveAge = WesternHemlock.GetFlewellingGrowthEffectiveAge(tsheSite!, this.TimeStepInYears, height, out potentialHeightGrowth);
                    }
                    else
                    {
                        growthEffectiveAge = DouglasFir.GetPsmeAbgrGrowthEffectiveAge(psmeSite!, this.TimeStepInYears, height, out potentialHeightGrowth);
                    }
                    Vector256<float> crownCompetitionFactor = OrganonVariant.GetCrownCompetitionFactorByHeight(height, crownCompetitionByHeight);
                    Vector256<float> sqrtCrownCompetitionFactor = Avx.Sqrt(crownCompetitionFactor);
                    Vector256<float> crownCompetitionIncrementToP4;
                    if (heightCoefficients.P4 == 0.5F)
                    {
                        crownCompetitionIncrementToP4 = sqrtCrownCompetitionFactor;
                    }
                    else
                    {
                        crownCompetitionIncrementToP4 = crownCompetitionFactor;
                    }

                    Vector256<float> crownRatio = Avx.LoadVector256(crownRatios + treeIndex);
                    Vector256<float> proportionBelowCrown = Avx.Subtract(one, crownRatio);
                    Vector256<float> B0 = Avx.Multiply(P1, MathV.Exp(Avx.Multiply(P2, crownCompetitionFactor)));
                    Vector256<float> B1 = MathV.Exp(Avx.Multiply(P3, crownCompetitionIncrementToP4)); // exp(P3 * sqrt(CCI)) for PSME and THSE, exp(P3 * CCI) for ABGR
                    Vector256<float> FCR = Avx.Multiply(Avx.Multiply(minusP5, Avx.Multiply(proportionBelowCrown, proportionBelowCrown)), MathV.Exp(Avx.Multiply(P7, sqrtCrownCompetitionFactor))); // P7 is 0.0 for ABGR and TSHE -> exp() = 1.0
                    Vector256<float> modifier = Avx.Multiply(P8, Avx.Add(B0, Avx.Multiply(Avx.Subtract(B1, B0), MathV.Exp(FCR))));
                    Vector256<float> crownRatioAdjustment = OrganonVariant.GetCrownRatioAdjustment(crownRatio);
                    Vector256<float> heightGrowth = Avx.Multiply(potentialHeightGrowth, Avx.Multiply(modifier, crownRatioAdjustment));
                    Vector256<float> expansionFactor = Avx.LoadVector256(expansionFactors + treeIndex); // maybe worth continuing in loop if all expansion factors are zero?
                    Vector256<float> zero = Vector256<float>.Zero;
                    heightGrowth = Avx.BlendVariable(heightGrowth, zero, Avx.CompareLessThanOrEqual(expansionFactor, zero));
                    DebugV.Assert(Avx.CompareGreaterThanOrEqual(heightGrowth, zero));
                    DebugV.Assert(Avx.CompareLessThanOrEqual(heightGrowth, AvxExtensions.BroadcastScalarToVector256(Constant.Maximum.HeightIncrementInFeet)));
                    Avx.Store(heightGrowths + treeIndex, heightGrowth);

                    // if growth effective age > old tree age is true, then 0xffff ffff = -1 is returned from the comparison
                    // Reinterpreting as Vector256<int> and subtracting therefore adds one to the old tree record counts where old trees occur.
                    oldTreeRecordCount = Avx2.Subtract(oldTreeRecordCount, Avx.CompareGreaterThan(growthEffectiveAge, oldTreeAgeThreshold).AsInt32());
                }
            }

            oldTreeRecordCount = Avx2.HorizontalAdd(oldTreeRecordCount, oldTreeRecordCount);
            oldTreeRecordCount = Avx2.HorizontalAdd(oldTreeRecordCount, oldTreeRecordCount);
            return oldTreeRecordCount.ToScalar();
        }

        public bool IsBigSixSpecies(FiaCode species)
        {
            return this.TreeModel switch
            {
                TreeModel.OrganonNwo => (species == FiaCode.PseudotsugaMenziesii) || (species == FiaCode.AbiesGrandis) ||
                                        (species == FiaCode.TsugaHeterophylla),
                TreeModel.OrganonSmc => (species == FiaCode.PseudotsugaMenziesii) || (species == FiaCode.AbiesConcolor) ||
                                        (species == FiaCode.AbiesGrandis) || (species == FiaCode.TsugaHeterophylla),
                TreeModel.OrganonRap => (species == FiaCode.AlnusRubra) || (species == FiaCode.PseudotsugaMenziesii) ||
                                        (species == FiaCode.TsugaHeterophylla),
                TreeModel.OrganonSwo => (species == FiaCode.AbiesConcolor) || (species == FiaCode.AbiesGrandis) ||
                                        (species == FiaCode.CalocedrusDecurrens) || (species == FiaCode.PinusLambertiana) ||
                                        (species == FiaCode.PinusPonderosa) || (species == FiaCode.PseudotsugaMenziesii),
                _ => throw OrganonVariant.CreateUnhandledModelException(this.TreeModel),
            };
        }

        public bool IsSpeciesSupported(FiaCode species)
        {
            return this.TreeModel switch
            {
                TreeModel.OrganonNwo or 
                TreeModel.OrganonSmc => Constant.NwoSmcSpecies.Contains(species),
                TreeModel.OrganonRap => Constant.RapSpecies.Contains(species),
                TreeModel.OrganonSwo => Constant.SwoSpecies.Contains(species),
                _ => throw OrganonVariant.CreateUnhandledModelException(this.TreeModel),
            };
        }

        public abstract void ReduceExpansionFactors(OrganonStand stand, OrganonStandDensity densityBeforeGrowth, Trees trees, float fertilizationExponent);

        public abstract float ToHemlockSiteIndex(float siteIndex);
        public abstract float ToSiteIndex(float hemlockSiteIndex);
    }
}
