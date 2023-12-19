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
        public SimdInstructions Simd { get; init; }
        public int TimeStepInYears { get; private init; }
        public TreeModel TreeModel { get; private init; }

        protected OrganonVariant(TreeModel treeModel, float oldTreeAgeThreshold)
        {
            this.crownCoefficients = [];
            this.heightCoefficients = [];
            this.OldTreeAgeThreshold = oldTreeAgeThreshold;
            this.Simd = Constant.Default.Simd;
            this.TimeStepInYears = treeModel == TreeModel.OrganonRap ? 1 : 5;
            this.TreeModel = treeModel;
        }

        // VEX 128 with quads of strata: 3.0x speedup from scalar
        public void AddCrownCompetitionByHeight(Trees trees, float[] crownCompetitionByHeight)
        {
            switch (this.Simd)
            {
                case SimdInstructions.Avx:
                    this.AddCrownCompetitionByHeightAvx(trees, crownCompetitionByHeight);
                    break;
                case SimdInstructions.Avx10:
                    this.AddCrownCompetitionByHeightAvx10(trees, crownCompetitionByHeight);
                    break;
                case SimdInstructions.Avx512:
                    this.AddCrownCompetitionByHeightAvx512(trees, crownCompetitionByHeight);
                    break;
                case SimdInstructions.Vex128:
                    this.AddCrownCompetitionByHeightVex128(trees, crownCompetitionByHeight);
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
        //            mcwB0 = Vector128.Create(4.6198F);
        //            mcwB1 = Vector128.Create(1.8426F);
        //            mcwB2 = Vector128.Create(-0.011311F);
        //            dbhLimitForMaxCrownWidth = Vector128.Create(81.45F);
        //            break;
        //        // GF Coefficients from Paine and Hann(1982) FRL Research Paper 46
        //        case FiaCode.AbiesGrandis:
        //            mcwB0 = Vector128.Create(6.1880F);
        //            mcwB1 = Vector128.Create(1.0069F);
        //            mcwB2 = Vector128.Create(0.0F);
        //            dbhLimitForMaxCrownWidth = Vector128.Create(999.99F);
        //            break;
        //        // Johnson(2002) Willamette Industries Report
        //        case FiaCode.TsugaHeterophylla:
        //            mcwB0 = Vector128.Create(4.3586F);
        //            mcwB1 = Vector128.Create(1.57458F);
        //            mcwB2 = Vector128.Create(0.0F);
        //            dbhLimitForMaxCrownWidth = Vector128.Create(76.70F);
        //            break;
        //        // Smith(1966) Proc. 6th World Forestry Conference
        //        case FiaCode.ThujaPlicata:
        //            mcwB0 = Vector128.Create(4.0F);
        //            mcwB1 = Vector128.Create(1.65F);
        //            mcwB2 = Vector128.Create(0.0F);
        //            dbhLimitForMaxCrownWidth = Vector128.Create(999.99F);
        //            break;
        //        // WH of Paine and Hann(1982) FRL Research Paper 46
        //        case FiaCode.TaxusBrevifolia:
        //            mcwB0 = Vector128.Create(4.5652F);
        //            mcwB1 = Vector128.Create(1.4147F);
        //            mcwB2 = Vector128.Create(0.0F);
        //            dbhLimitForMaxCrownWidth = Vector128.Create(999.99F);
        //            break;
        //        // Paine and Hann(1982) FRL Research Paper 46
        //        case FiaCode.ArbutusMenziesii:
        //            mcwB0 = Vector128.Create(3.4298629F);
        //            mcwB1 = Vector128.Create(1.3532302F);
        //            mcwB2 = Vector128.Create(0.0F);
        //            dbhLimitForMaxCrownWidth = Vector128.Create(999.99F);
        //            break;
        //        // Ek(1974) School of Natural Res., U.Wisc., Forestry Res. Notes.
        //        case FiaCode.AcerMacrophyllum:
        //            mcwB0 = Vector128.Create(4.0953F);
        //            mcwB1 = Vector128.Create(2.3849F);
        //            mcwB2 = Vector128.Create(-0.0102651F);
        //            dbhLimitForMaxCrownWidth = Vector128.Create(102.53F);
        //            break;
        //        // WO Coefficients from Paine and Hann (1982) FRL Research Paper 46
        //        case FiaCode.QuercusGarryana:
        //            mcwB0 = Vector128.Create(3.0785639F);
        //            mcwB1 = Vector128.Create(1.9242211F);
        //            mcwB2 = Vector128.Create(0.0F);
        //            dbhLimitForMaxCrownWidth = Vector128.Create(999.99F);
        //            break;
        //        // Smith(1966) Proc. 6th World Forestry Conference
        //        case FiaCode.AlnusRubra:
        //            mcwB0 = Vector128.Create(8.0F);
        //            mcwB1 = Vector128.Create(1.53F);
        //            mcwB2 = Vector128.Create(0.0F);
        //            dbhLimitForMaxCrownWidth = Vector128.Create(999.99F);
        //            break;
        //        // GC of Paine and Hann(1982) FRL Research Paper 46
        //        case FiaCode.CornusNuttallii:
        //        case FiaCode.Salix:
        //            mcwB0 = Vector128.Create(2.9793895F);
        //            mcwB1 = Vector128.Create(1.5512443F);
        //            mcwB2 = Vector128.Create(-0.01416129F);
        //            dbhLimitForMaxCrownWidth = Vector128.Create(54.77F);
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
        //            hlcwB1 = Vector128.Create(0.062000F);
        //            break;
        //        // Hann and Hanus(2001) FRL Research Contribution 34
        //        case FiaCode.AbiesGrandis:
        //            hlcwB1 = Vector128.Create(0.028454F);
        //            break;
        //        // Marshall, Johnson, and Hann(2003) CJFR 33: 2059-2066
        //        case FiaCode.TsugaHeterophylla:
        //            hlcwB1 = Vector128.Create(0.355270F);
        //            break;
        //        // WH of Hann and Hanus(2001) FRL Research Contribution 34
        //        case FiaCode.ThujaPlicata:
        //        case FiaCode.TaxusBrevifolia:
        //            hlcwB1 = Vector128.Create(0.209806F);
        //            break;
        //        // Hann and Hanus(2001) FRL Research Contribution 34
        //        case FiaCode.ArbutusMenziesii:
        //        case FiaCode.AcerMacrophyllum:
        //        case FiaCode.QuercusGarryana:
        //        case FiaCode.AlnusRubra:
        //        case FiaCode.CornusNuttallii:
        //        case FiaCode.Salix:
        //            hlcwB1 = Vector128.Create(0.0F);
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
        //            lcwB1 = Vector128.Create(0.0F);
        //            lcwB2 = Vector128.Create(0.00436324F);
        //            lcwB3 = Vector128.Create(0.6020020F);
        //            break;
        //        // Hann(1997) FRL Research Contribution 17
        //        case FiaCode.AbiesGrandis:
        //            lcwB1 = Vector128.Create(0.0F);
        //            lcwB2 = Vector128.Create(0.00308402F);
        //            lcwB3 = Vector128.Create(0.0F);
        //            break;
        //        // Johnson(2002) Willamette Industries Report
        //        case FiaCode.TsugaHeterophylla:
        //            lcwB1 = Vector128.Create(0.105590F);
        //            lcwB2 = Vector128.Create(0.0035662F);
        //            lcwB3 = Vector128.Create(0.0F);
        //            break;
        //        // IC of Hann(1997) FRL Research Contribution 17
        //        case FiaCode.ThujaPlicata:
        //            lcwB1 = Vector128.Create(-0.2513890F);
        //            lcwB2 = Vector128.Create(0.006925120F);
        //            lcwB3 = Vector128.Create(0.985922F);
        //            break;
        //        // WH of Hann(1997) FRL Research Contribution 17
        //        case FiaCode.TaxusBrevifolia:
        //            lcwB1 = Vector128.Create(0.0F);
        //            lcwB2 = Vector128.Create(0.0F);
        //            lcwB3 = Vector128.Create(0.0F);
        //            break;
        //        // Hann(1997) FRL Research Contribution 17
        //        case FiaCode.ArbutusMenziesii:
        //            lcwB1 = Vector128.Create(0.118621F);
        //            lcwB2 = Vector128.Create(0.00384872F);
        //            lcwB3 = Vector128.Create(0.0F);
        //            break;
        //        // Hann(1997) FRL Research Contribution 17
        //        case FiaCode.AcerMacrophyllum:
        //            lcwB1 = Vector128.Create(0.0F);
        //            lcwB2 = Vector128.Create(0.0F);
        //            lcwB3 = Vector128.Create(1.470180F);
        //            break;
        //        // Hann(1997) FRL Research Contribution 17
        //        case FiaCode.QuercusGarryana:
        //            lcwB1 = Vector128.Create(0.3648110F);
        //            lcwB2 = Vector128.Create(0.0F);
        //            lcwB3 = Vector128.Create(0.0F);
        //            break;
        //        // Hann(1997) FRL Research Contribution 17
        //        case FiaCode.AlnusRubra:
        //            lcwB1 = Vector128.Create(0.3227140F);
        //            lcwB2 = Vector128.Create(0.0F);
        //            lcwB3 = Vector128.Create(0.0F);
        //            break;
        //        // GC of Hann(1997) FRL Research Contribution 17
        //        case FiaCode.CornusNuttallii:
        //        case FiaCode.Salix:
        //            lcwB1 = Vector128.Create(0.0F);
        //            lcwB2 = Vector128.Create(0.0F);
        //            lcwB3 = Vector128.Create(1.61440F);
        //            break;
        //        default:
        //            throw Trees.CreateUnhandledSpeciesException(species);
        //    }

        //    // coefficients for crown width
        //    Vector128<float> cwB1;
        //    Vector128<float> cwB2;
        //    Vector128<float> cwB3;
        //    Vector128<float> cwMaxHeightDiameterRatio = Vector128.Create(Single.MaxValue);
        //    switch (species)
        //    {
        //        // DF Coefficients from Hann(1999) FS 45: 217-225
        //        case FiaCode.PseudotsugaMenziesii:
        //            cwB1 = Vector128.Create(0.929973F);
        //            cwB2 = Vector128.Create(-0.135212F);
        //            cwB3 = Vector128.Create(-0.0157579F);
        //            cwMaxHeightDiameterRatio = Vector128.Create(50.0F); // BUGBUG: Fortran code divides feet by inches?
        //            break;
        //        // GF Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
        //        case FiaCode.AbiesGrandis:
        //            cwB1 = Vector128.Create(0.999291F);
        //            cwB2 = Vector128.Create(0.0F);
        //            cwB3 = Vector128.Create(-0.0314603F);
        //            cwMaxHeightDiameterRatio = Vector128.Create(31.0F); // BUGBUG: Fortran code divides feet by inches?
        //            break;
        //        // Marshall, Johnson, and Hann(2003) CJFR 33: 2059-2066
        //        case FiaCode.TsugaHeterophylla:
        //            cwB1 = Vector128.Create(0.461782F);
        //            cwB2 = Vector128.Create(0.552011F);
        //            cwB3 = Vector128.Create(0.0F);
        //            break;
        //        // Hann and Hanus(2001) FRL Research Contribution 34
        //        case FiaCode.ThujaPlicata:
        //        case FiaCode.TaxusBrevifolia:
        //            cwB1 = Vector128.Create(0.629785F);
        //            cwB2 = Vector128.Create(0.0F);
        //            cwB3 = Vector128.Create(0.0F);
        //            break;
        //        // Hann and Hanus(2001) FRL Research Contribution 34
        //        case FiaCode.ArbutusMenziesii:
        //        case FiaCode.AcerMacrophyllum:
        //        case FiaCode.QuercusGarryana:
        //        case FiaCode.AlnusRubra:
        //        case FiaCode.CornusNuttallii:
        //        case FiaCode.Salix:
        //            cwB1 = Vector128.Create(0.5F);
        //            cwB2 = Vector128.Create(0.0F);
        //            cwB3 = Vector128.Create(0.0F);
        //            break;
        //        default:
        //            throw Trees.CreateUnhandledSpeciesException(species);
        //    }

        //    Vector128<float> zero = Vector128.Create(0.0F);
        //    Vector128<float> one = Vector128.Create(1.0F);
        //    Vector128<float> v4p5 = Vector128.Create(4.5F);
        //    Vector128<float> vCrownCompetitionConstantEnglish = Vector128.Create(Constant.CrownCompetionConstantEnglish);
        //    Vector128<float> strataThickness = Vector128.Create(crownCompetitionByHeight[^1] / Constant.HeightStrataAsFloat);
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
        //            Vector128<float> crownWidthEvaluationHeight = Avx.Multiply(Vector128.Create(0.5F), strataThickness);
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

        private unsafe void AddCrownCompetitionByHeightAvx(Trees trees, float[] crownCompetitionByHeight)
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
                    Vector256<float> ccfExpansionFactor256 = Vector256.Create(ccfExpansionFactor);
                    Vector256<float> crownCompetitionFactor = Vector256.Create(ccfExpansionFactor * largestCrownWidth * largestCrownWidth);
                    Vector256<float> cwB1_256 = Vector256.Create(crown.CWb1);
                    Vector256<float> cwB2_256 = Vector256.Create(crown.CWb2);
                    Vector256<float> cwB3heightDiameterRatio256 = Vector256.Create(cwB3heightDiameterRatio);
                    Vector256<float> heightInFeet256 = Vector256.Create(heightInFeet);
                    Vector256<float> heightToLargestCrownWidth256 = Vector256.Create(heightToLargestCrownWidth);
                    Vector256<float> largestCrownWidth256 = Vector256.Create(largestCrownWidth);
                    Vector256<float> strataHeightIncrement = Vector256.Create(8.0F * crownCompetitionByHeight[^1] / Constant.OrganonHeightStrata);
                    Vector256<float> strataHeight = Avx.Multiply(Vector256.Create(0.125F, 0.250F, 0.375F, 0.500F, 0.625F, 0.750F, 0.875F, 1.00F), strataHeightIncrement); // find CCF at top of strata as in Fortran
                    for (int strataIndex = 0; strataIndex < crownCompetitionByHeight.Length - 2; strataIndex += Constant.Simd256x8.Width)
                    {
                        Vector256<float> strataBelowTreeHeightMask = Avx.CompareLessThan(strataHeight, heightInFeet256);
                        if (Avx.MoveMask(strataBelowTreeHeightMask) == Constant.Simd256x8.MaskAllFalse)
                        {
                            // tree contributes no crown competition factor above its height
                            break;
                        }

                        // find crown width and lowered CCFs for any strata above height of largest crown width
                        Vector256<float> strataAboveLargestCrownMask = Avx.CompareGreaterThan(strataHeight, heightToLargestCrownWidth256);
                        if (Avx.MoveMask(strataAboveLargestCrownMask) != Constant.Simd256x8.MaskAllFalse)
                        {
                            // very slightly faster to divide than to precompute denominator reciprocal
                            Vector256<float> relativePosition = Avx.Divide(Avx.Subtract(heightInFeet256, strataHeight), Avx.Subtract(heightInFeet256, heightToLargestCrownWidth256));
                            Vector256<float> largestWidthMultiplier = MathAvx.Pow(relativePosition, Avx.Add(cwB1_256, Avx.Add(Avx.Multiply(cwB2_256, Avx.Sqrt(relativePosition)), cwB3heightDiameterRatio256)));
                            Vector256<float> crownWidthInStrata = Avx.Multiply(largestCrownWidth256, largestWidthMultiplier);
                            Vector256<float> crownCompetitionFactorInStrata = Avx.Multiply(ccfExpansionFactor256, Avx.Multiply(crownWidthInStrata, crownWidthInStrata));
                            crownCompetitionFactor = Avx.BlendVariable(crownCompetitionFactor, crownCompetitionFactorInStrata, strataAboveLargestCrownMask);
                        }

                        // zero any elements above tree height
                        crownCompetitionFactor = Avx.BlendVariable(Vector256<float>.Zero, crownCompetitionFactor, strataBelowTreeHeightMask);

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

        private unsafe void AddCrownCompetitionByHeightAvx10(Trees trees, float[] crownCompetitionByHeight)
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
                    Vector256<float> ccfExpansionFactor256 = Vector256.Create(ccfExpansionFactor);
                    Vector256<float> crownCompetitionFactor = Vector256.Create(ccfExpansionFactor * largestCrownWidth * largestCrownWidth);
                    Vector256<float> cwB1_256 = Vector256.Create(crown.CWb1);
                    Vector256<float> cwB2_256 = Vector256.Create(crown.CWb2);
                    Vector256<float> cwB3heightDiameterRatio256 = Vector256.Create(cwB3heightDiameterRatio);
                    Vector256<float> heightInFeet256 = Vector256.Create(heightInFeet);
                    Vector256<float> heightToLargestCrownWidth256 = Vector256.Create(heightToLargestCrownWidth);
                    Vector256<float> largestCrownWidth256 = Vector256.Create(largestCrownWidth);
                    Vector256<float> strataHeightIncrement = Vector256.Create(8.0F * crownCompetitionByHeight[^1] / Constant.OrganonHeightStrata);
                    Vector256<float> strataHeight = Avx512F.Multiply(Vector256.Create(0.125F, 0.250F, 0.375F, 0.500F, 0.625F, 0.750F, 0.875F, 1.00F), strataHeightIncrement); // find CCF at top of strata as in Fortran
                    for (int strataIndex = 0; strataIndex < crownCompetitionByHeight.Length - 2; strataIndex += Constant.Simd256x8.Width)
                    {
                        Vector256<float> strataBelowTreeHeightMask = Avx512F.CompareLessThan(strataHeight, heightInFeet256);
                        if (Avx512F.MoveMask(strataBelowTreeHeightMask) == Constant.Simd256x8.MaskAllFalse)
                        {
                            // tree contributes no crown competition factor above its height
                            break;
                        }

                        // find crown width and lowered CCFs for any strata above height of largest crown width
                        Vector256<float> strataAboveLargestCrownMask = Avx512F.CompareGreaterThan(strataHeight, heightToLargestCrownWidth256);
                        if (Avx512F.MoveMask(strataAboveLargestCrownMask) != Constant.Simd256x8.MaskAllFalse)
                        {
                            // very slightly faster to divide than to precompute denominator reciprocal
                            Vector256<float> relativePosition = Avx512F.Divide(Avx512F.Subtract(heightInFeet256, strataHeight), Avx512F.Subtract(heightInFeet256, heightToLargestCrownWidth256));
                            Vector256<float> largestWidthMultiplier = MathAvx10.Pow(relativePosition, Avx512F.Add(cwB1_256, Avx512F.Add(Avx512F.Multiply(cwB2_256, Avx512F.Sqrt(relativePosition)), cwB3heightDiameterRatio256)));
                            Vector256<float> crownWidthInStrata = Avx512F.Multiply(largestCrownWidth256, largestWidthMultiplier);
                            Vector256<float> crownCompetitionFactorInStrata = Avx512F.Multiply(ccfExpansionFactor256, Avx512F.Multiply(crownWidthInStrata, crownWidthInStrata));
                            crownCompetitionFactor = Avx512F.BlendVariable(crownCompetitionFactor, crownCompetitionFactorInStrata, strataAboveLargestCrownMask);
                        }

                        // zero any elements above tree height
                        crownCompetitionFactor = Avx512F.BlendVariable(Vector256<float>.Zero, crownCompetitionFactor, strataBelowTreeHeightMask);

                        // accumulate CCF
                        Vector256<float> crownCompetitionByHeight256 = Avx512F.LoadVector256(pinnedCrownCompetitionByHeight + strataIndex);
                        crownCompetitionByHeight256 = Avx512F.Add(crownCompetitionByHeight256, crownCompetitionFactor);
                        Avx512F.Store(pinnedCrownCompetitionByHeight + strataIndex, crownCompetitionByHeight256);

                        // move upwards to next quad of strata
                        strataHeight = Avx512F.Add(strataHeight, strataHeightIncrement);
                    }
                }
            }
        }

        private unsafe void AddCrownCompetitionByHeightAvx512(Trees trees, float[] crownCompetitionByHeight)
        {
            Debug.Assert(((crownCompetitionByHeight.Length - 1) % Constant.Simd512x16.Width == 0) && (trees.Capacity % Constant.Simd512x16.Width == 0));

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
                    Vector512<float> ccfExpansionFactor512 = Vector512.Create(ccfExpansionFactor);
                    Vector512<float> crownCompetitionFactor = Vector512.Create(ccfExpansionFactor * largestCrownWidth * largestCrownWidth);
                    Vector512<float> cwB1_512 = Vector512.Create(crown.CWb1);
                    Vector512<float> cwB2_512 = Vector512.Create(crown.CWb2);
                    Vector512<float> cwB3heightDiameterRatio512 = Vector512.Create(cwB3heightDiameterRatio);
                    Vector512<float> heightInFeet512 = Vector512.Create(heightInFeet);
                    Vector512<float> heightToLargestCrownWidth512 = Vector512.Create(heightToLargestCrownWidth);
                    Vector512<float> largestCrownWidth512 = Vector512.Create(largestCrownWidth);
                    Vector512<float> strataHeightIncrement = Vector512.Create(8.0F * crownCompetitionByHeight[^1] / Constant.OrganonHeightStrata);
                    Vector512<float> strataHeight = Avx512F.Multiply(Vector512.Create(0.0625F, 0.1250F, 0.1875F, 0.2500F, 0.3125F, 0.375F, 0.4375F, 0.500F, 5625F, 0.625F, 0.6875F, 0.750F, 0.8125F, 0.875F, 0.9375F, 1.00F), strataHeightIncrement);
                    for (int strataIndex = 0; strataIndex < crownCompetitionByHeight.Length - 2; strataIndex += Constant.Simd512x16.Width)
                    {
                        Vector512<float> strataBelowTreeHeightMask = Avx512F.CompareLessThan(strataHeight, heightInFeet512);
                        if (strataBelowTreeHeightMask.ExtractMostSignificantBits() == Constant.Simd512x16.MaskAllFalse)
                        {
                            // tree contributes no crown competition factor above its height
                            break;
                        }

                        // find crown width and lowered CCFs for any strata above height of largest crown width
                        Vector512<float> strataAboveLargestCrownMask = Avx512F.CompareGreaterThan(strataHeight, heightToLargestCrownWidth512);
                        if (strataAboveLargestCrownMask.ExtractMostSignificantBits() != Constant.Simd512x16.MaskAllFalse)
                        {
                            // very slightly faster to divide than to precompute denominator reciprocal
                            Vector512<float> relativePosition = Avx512F.Divide(Avx512F.Subtract(heightInFeet512, strataHeight), Avx512F.Subtract(heightInFeet512, heightToLargestCrownWidth512));
                            Vector512<float> largestWidthMultiplier = MathAvx10.Pow(relativePosition, Avx512F.Add(cwB1_512, Avx512F.Add(Avx512F.Multiply(cwB2_512, Avx512F.Sqrt(relativePosition)), cwB3heightDiameterRatio512)));
                            Vector512<float> crownWidthInStrata = Avx512F.Multiply(largestCrownWidth512, largestWidthMultiplier);
                            Vector512<float> crownCompetitionFactorInStrata = Avx512F.Multiply(ccfExpansionFactor512, Avx512F.Multiply(crownWidthInStrata, crownWidthInStrata));
                            crownCompetitionFactor = Avx512F.BlendVariable(crownCompetitionFactor, crownCompetitionFactorInStrata, strataAboveLargestCrownMask);
                        }

                        // zero any elements above tree height
                        crownCompetitionFactor = Avx512F.BlendVariable(Vector512<float>.Zero, crownCompetitionFactor, strataBelowTreeHeightMask);

                        // accumulate CCF
                        Vector512<float> crownCompetitionByHeight256 = Avx512F.LoadVector512(pinnedCrownCompetitionByHeight + strataIndex);
                        crownCompetitionByHeight256 = Avx512F.Add(crownCompetitionByHeight256, crownCompetitionFactor);
                        Avx512F.Store(pinnedCrownCompetitionByHeight + strataIndex, crownCompetitionByHeight256);

                        // move upwards to next quad of strata
                        strataHeight = Avx512F.Add(strataHeight, strataHeightIncrement);
                    }
                }
            }
        }

        private unsafe void AddCrownCompetitionByHeightVex128(Trees trees, float[] crownCompetitionByHeight)
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
                    Vector128<float> ccfExpansionFactor128 = Vector128.Create(ccfExpansionFactor);
                    Vector128<float> crownCompetitionFactor = Vector128.Create(ccfExpansionFactor * largestCrownWidth * largestCrownWidth);
                    Vector128<float> cwB1_128 = Vector128.Create(crown.CWb1);
                    Vector128<float> cwB2_128 = Vector128.Create(crown.CWb2);
                    Vector128<float> cwB3heightDiameterRatio128 = Vector128.Create(cwB3heightDiameterRatio);
                    Vector128<float> heightInFeet128 = Vector128.Create(heightInFeet);
                    Vector128<float> heightToLargestCrownWidth128 = Vector128.Create(heightToLargestCrownWidth);
                    Vector128<float> largestCrownWidth128 = Vector128.Create(largestCrownWidth);
                    Vector128<float> strataHeightIncrement = Vector128.Create(4.0F * crownCompetitionByHeight[^1] / Constant.OrganonHeightStrata);
                    Vector128<float> strataHeight = Avx.Multiply(Vector128.Create(0.25F, 0.50F, 0.75F, 1.0F), strataHeightIncrement); // find CCF at top of strata as in Fortran
                    for (int strataIndex = 0; strataIndex < crownCompetitionByHeight.Length - 2; strataIndex += Constant.Simd128x4.Width)
                    {
                        Vector128<float> strataBelowTreeHeightMask = Avx.CompareLessThan(strataHeight, heightInFeet128);
                        if (Avx.MoveMask(strataBelowTreeHeightMask) == Constant.Simd128x4.MaskAllFalse)
                        {
                            // tree contributes no crown competition factor above its height
                            break;
                        }

                        // find crown width and lowered CCFs for any strata above height of largest crown width
                        Vector128<float> strataAboveLargestCrownMask = Avx.CompareGreaterThan(strataHeight, heightToLargestCrownWidth128);
                        if (Avx.MoveMask(strataAboveLargestCrownMask) != Constant.Simd128x4.MaskAllFalse)
                        {
                            // very slightly faster to divide than to precompute denominator reciprocal
                            Vector128<float> relativePosition = Avx.Divide(Avx.Subtract(heightInFeet128, strataHeight), Avx.Subtract(heightInFeet128, heightToLargestCrownWidth128));
                            Vector128<float> largestWidthMultiplier = MathAvx.Pow(relativePosition, Avx.Add(cwB1_128, Avx.Add(Avx.Multiply(cwB2_128, Avx.Sqrt(relativePosition)), cwB3heightDiameterRatio128)));
                            Vector128<float> crownWidthInStrata = Avx.Multiply(largestCrownWidth128, largestWidthMultiplier);
                            Vector128<float> crownCompetitionFactorInStrata = Avx.Multiply(ccfExpansionFactor128, Avx.Multiply(crownWidthInStrata, crownWidthInStrata));
                            crownCompetitionFactor = Avx.BlendVariable(crownCompetitionFactor, crownCompetitionFactorInStrata, strataAboveLargestCrownMask);
                        }

                        // zero any elements above tree height
                        crownCompetitionFactor = Avx.BlendVariable(Vector128<float>.Zero, crownCompetitionFactor, strataBelowTreeHeightMask);

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

        protected static unsafe Vector256<float> GetCrownCompetitionFactorByHeightAvx(Vector256<float> height, float[] crownCompetitionByHeight)
        {
            // this is called during GrowHeight() with grown height but before crown competition has been recomputed for new heights
            // As a result, indices well beyond the end of the crown competition array can be generated and must be clamped. If needed, the code 
            // here can be made slightly more efficient by adding a guard strata whose competition factor is always zero and vectorizing the
            // compare and clamp.
            Debug.Assert(crownCompetitionByHeight[^1] > 4.5F);
            Vector256<float> strataIndexAsFloat = Avx.Multiply(Vector256.Create((float)Constant.OrganonHeightStrata / crownCompetitionByHeight[^1]), height);
            Vector256<int> strataIndex = Avx.ConvertToVector256Int32WithTruncation(strataIndexAsFloat);
            DebugV.Assert(Avx2.CompareGreaterThan(strataIndex, Vector256.Create(-1))); // no integer >=

            Vector256<int> maxStrataIndex = Vector256.Create(crownCompetitionByHeight.Length - 2); // CCF is zero in uppermost strata
            Vector256<int> treeTallerThanMaxStrataHeight = Avx2.CompareGreaterThan(strataIndex, maxStrataIndex);
            strataIndex = Avx2.BlendVariable(strataIndex, maxStrataIndex, treeTallerThanMaxStrataHeight);

            fixed (float* crownCompetition = crownCompetitionByHeight)
            {
                Vector256<float> crownCompetitionFactor = Avx2.GatherVector256(crownCompetition, strataIndex, sizeof(float));
                return crownCompetitionFactor;
            }
        }

        protected static unsafe Vector256<float> GetCrownCompetitionFactorByHeightAvx10(Vector256<float> height, float[] crownCompetitionByHeight)
        {
            // this is called during GrowHeight() with grown height but before crown competition has been recomputed for new heights
            // As a result, indices well beyond the end of the crown competition array can be generated and must be clamped. If needed, the code 
            // here can be made slightly more efficient by adding a guard strata whose competition factor is always zero and vectorizing the
            // compare and clamp.
            Debug.Assert(crownCompetitionByHeight[^1] > 4.5F);
            Vector256<float> strataIndexAsFloat = Avx512F.Multiply(Vector256.Create((float)Constant.OrganonHeightStrata / crownCompetitionByHeight[^1]), height);
            Vector256<int> strataIndex = Avx512F.ConvertToVector256Int32WithTruncation(strataIndexAsFloat);
            DebugV.Assert(Avx512F.CompareGreaterThan(strataIndex, Vector256.Create(-1))); // no integer >=

            Vector256<int> maxStrataIndex = Vector256.Create(crownCompetitionByHeight.Length - 2); // CCF is zero in uppermost strata
            Vector256<int> treeTallerThanMaxStrataHeight = Avx512F.CompareGreaterThan(strataIndex, maxStrataIndex);
            strataIndex = Avx512F.BlendVariable(strataIndex, maxStrataIndex, treeTallerThanMaxStrataHeight);

            fixed (float* crownCompetition = crownCompetitionByHeight)
            {
                Vector256<float> crownCompetitionFactor = Avx512F.GatherVector256(crownCompetition, strataIndex, sizeof(float));
                return crownCompetitionFactor;
            }
        }

        protected static unsafe Vector512<float> GetCrownCompetitionFactorByHeightAvx512(Vector512<float> height, float[] crownCompetitionByHeight)
        {
            // this is called during GrowHeight() with grown height but before crown competition has been recomputed for new heights
            // As a result, indices well beyond the end of the crown competition array can be generated and must be clamped. If needed, the code 
            // here can be made slightly more efficient by adding a guard strata whose competition factor is always zero and vectorizing the
            // compare and clamp.
            Debug.Assert(crownCompetitionByHeight[^1] > 4.5F);
            Vector512<float> strataIndexAsFloat = Avx512F.Multiply(Vector512.Create((float)Constant.OrganonHeightStrata / crownCompetitionByHeight[^1]), height);
            Vector512<int> strataIndex = Avx512F.ConvertToVector512Int32WithTruncation(strataIndexAsFloat);
            DebugV.Assert(Avx512F.CompareGreaterThan(strataIndex, Vector512.Create(-1))); // no integer >=

            Vector512<int> maxStrataIndex = Vector512.Create(crownCompetitionByHeight.Length - 2); // CCF is zero in uppermost strata
            Vector512<int> treeTallerThanMaxStrataHeight = Avx512F.CompareGreaterThan(strataIndex, maxStrataIndex);
            strataIndex = Avx512F.BlendVariable(strataIndex, maxStrataIndex, treeTallerThanMaxStrataHeight);

            fixed (float* crownCompetition = crownCompetitionByHeight)
            {
                // no 512 bit gather in .NET 8; https://github.com/dotnet/runtime/issues/87097
                Vector512<float> crownCompetitionFactor = Vector512.Create(Avx512F.GatherVector256(crownCompetition, strataIndex.GetLower(), sizeof(float)),
                                                                           Avx512F.GatherVector256(crownCompetition, strataIndex.GetUpper(), sizeof(float)));
                return crownCompetitionFactor;
            }
        }

        protected unsafe static Vector128<float> GetCrownCompetitionFactorByHeightVex128(Vector128<float> height, float[] crownCompetitionByHeight)
        {
            // this is called during GrowHeight() with grown height but before crown competition has been recomputed for new heights
            // As a result, indices well beyond the end of the crown competition array can be generated and must be clamped. If needed, the code 
            // here can be made slightly more efficient by adding a guard strata whose competition factor is always zero and vectorizing the
            // compare and clamp.
            Debug.Assert(crownCompetitionByHeight[^1] > 4.5F);
            Vector128<float> strataIndexAsFloat = Avx.Multiply(Vector128.Create((float)Constant.OrganonHeightStrata / crownCompetitionByHeight[^1]), height);
            Vector128<int> strataIndex = Avx.ConvertToVector128Int32WithTruncation(strataIndexAsFloat);
            DebugV.Assert(Avx.CompareGreaterThan(strataIndex, Vector128.Create(-1))); // no integer >=

            Vector128<int> maxStrataIndex = Vector128.Create(crownCompetitionByHeight.Length - 2); // CCF is zero in uppermost strata
            Vector128<int> treeTallerThanMaxStrataHeight = Avx.CompareGreaterThan(strataIndex, maxStrataIndex);
            strataIndex = Avx.BlendVariable(strataIndex, maxStrataIndex, treeTallerThanMaxStrataHeight);

            fixed (float* crownCompetition = crownCompetitionByHeight)
            {
                Vector128<float> crownCompetitionFactor = Avx2.GatherVector128(crownCompetition, strataIndex, sizeof(float));
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

        protected static Vector256<float> GetCrownRatioAdjustmentAvx(Vector256<float> crownRatio)
        {
            Vector256<float> crownRatioAdjustment = Vector256.Create(1.0F);
            Vector256<float> exponentMask = Avx.CompareLessThan(crownRatio, Vector256.Create(0.11F));
            if (Avx.MoveMask(exponentMask) != Constant.Simd256x8.MaskAllFalse)
            {
                Vector256<float> power = Avx.Multiply(Vector256.Create(-25.0F * 25.0F), Avx.Multiply(crownRatio, crownRatio));
                Vector256<float> exponent = MathAvx.MaskExp(power, exponentMask);
                crownRatioAdjustment = Avx.Subtract(crownRatioAdjustment, exponent);
            }
            return crownRatioAdjustment;
        }

        protected static Vector256<float> GetCrownRatioAdjustmentAvx10(Vector256<float> crownRatio)
        {
            Vector256<float> crownRatioAdjustment = Vector256.Create(1.0F);
            Vector256<float> exponentMask = Avx512F.CompareLessThan(crownRatio, Vector256.Create(0.11F));
            if (Avx512F.MoveMask(exponentMask) != Constant.Simd256x8.MaskAllFalse)
            {
                Vector256<float> power = Avx512F.Multiply(Vector256.Create(-25.0F * 25.0F), Avx512F.Multiply(crownRatio, crownRatio));
                Vector256<float> exponent = MathAvx10.MaskExp(power, exponentMask);
                crownRatioAdjustment = Avx512F.Subtract(crownRatioAdjustment, exponent);
            }
            return crownRatioAdjustment;
        }

        protected static Vector512<float> GetCrownRatioAdjustmentAvx512(Vector512<float> crownRatio)
        {
            Vector512<float> crownRatioAdjustment = Vector512.Create(1.0F);
            Vector512<float> exponentMask = Avx512F.CompareLessThan(crownRatio, Vector512.Create(0.11F));
            if (exponentMask.ExtractMostSignificantBits() != Constant.Simd256x8.MaskAllFalse)
            {
                Vector512<float> power = Avx512F.Multiply(Vector512.Create(-25.0F * 25.0F), Avx512F.Multiply(crownRatio, crownRatio));
                Vector512<float> exponent = MathAvx10.MaskExp(power, exponentMask);
                crownRatioAdjustment = Avx512F.Subtract(crownRatioAdjustment, exponent);
            }
            return crownRatioAdjustment;
        }

        protected static Vector128<float> GetCrownRatioAdjustmentVex128(Vector128<float> crownRatio)
        {
            Vector128<float> crownRatioAdjustment = Vector128.Create(1.0F);
            Vector128<float> exponentMask = Avx.CompareLessThan(crownRatio, Vector128.Create(0.11F));
            if (Avx.MoveMask(exponentMask) != Constant.Simd128x4.MaskAllFalse)
            {
                Vector128<float> power = Avx.Multiply(Vector128.Create(-25.0F * 25.0F), Avx.Multiply(crownRatio, crownRatio));
                Vector128<float> exponent = MathAvx.MaskExp(power, exponentMask);
                crownRatioAdjustment = Avx.Subtract(crownRatioAdjustment, exponent);
                DebugV.Assert(Avx.CompareGreaterThanOrEqual(crownRatioAdjustment, Vector128<float>.Zero));
                DebugV.Assert(Avx.CompareLessThanOrEqual(crownRatioAdjustment, Vector128.Create(1.0F)));
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
                SimdInstructions.Avx => this.GrowHeightBigSixAvx(configuration, stand, trees, crownCompetitionByHeight),
                SimdInstructions.Avx10 => this.GrowHeightBigSixAvx10(configuration, stand, trees, crownCompetitionByHeight),
                SimdInstructions.Avx512 => this.GrowHeightBigSixAvx512(configuration, stand, trees, crownCompetitionByHeight),
                SimdInstructions.Vex128 => this.GrowHeightBigSixVex128(configuration, stand, trees, crownCompetitionByHeight),
                _ => throw new NotSupportedException("Unhandled SIMD instruction set " + this.Simd + ".")
            };
        }

        private unsafe int GrowHeightBigSixAvx(OrganonConfiguration configuration, OrganonStand stand, Trees trees, float[] crownCompetitionByHeight)
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

            Vector256<float> P1 = Vector256.Create(heightCoefficients.P1);
            Vector256<float> P2 = Vector256.Create(heightCoefficients.P2);
            Vector256<float> P3 = Vector256.Create(heightCoefficients.P3);
            Debug.Assert((heightCoefficients.P4 == 0.5F) || (heightCoefficients.P4 == 1.0F)); // P4 = 0.5F; => B1 = sqrt(P4)
            Vector256<float> minusP5 = Vector256.Create(-heightCoefficients.P5);
            Debug.Assert(heightCoefficients.P6 == 2.0F); // P6 = 0.5F; => FCR = -P5 * (1 - crownRatio)^P6 * P7^sqrt(CCF)
            Vector256<float> P7 = Vector256.Create(heightCoefficients.P7);
            Vector256<float> P8 = Vector256.Create(heightCoefficients.P8);
            Vector256<float> oldTreeAgeThreshold = Vector256.Create(configuration.Variant.OldTreeAgeThreshold);
            Vector256<float> one = Vector256.Create(1.0F);

            Vector256<int> oldTreeRecordCount256 = Vector256<int>.Zero;
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
                        growthEffectiveAge = WesternHemlock.GetFlewellingGrowthEffectiveAgeAvx(tsheSite!, this.TimeStepInYears, height, out potentialHeightGrowth);
                    }
                    else
                    {
                        growthEffectiveAge = DouglasFir.GetPsmeAbgrGrowthEffectiveAgeAvx(psmeSite!, this.TimeStepInYears, height, out potentialHeightGrowth);
                    }
                    Vector256<float> crownCompetitionFactor = OrganonVariant.GetCrownCompetitionFactorByHeightAvx(height, crownCompetitionByHeight);
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
                    Vector256<float> B0 = Avx.Multiply(P1, MathAvx.Exp(Avx.Multiply(P2, crownCompetitionFactor)));
                    Vector256<float> B1 = MathAvx.Exp(Avx.Multiply(P3, crownCompetitionIncrementToP4)); // exp(P3 * sqrt(CCI)) for PSME and THSE, exp(P3 * CCI) for ABGR
                    Vector256<float> FCR = Avx.Multiply(Avx.Multiply(minusP5, Avx.Multiply(proportionBelowCrown, proportionBelowCrown)), MathAvx.Exp(Avx.Multiply(P7, sqrtCrownCompetitionFactor))); // P7 is 0.0 for ABGR and TSHE -> exp() = 1.0
                    Vector256<float> modifier = Avx.Multiply(P8, Avx.Add(B0, Avx.Multiply(Avx.Subtract(B1, B0), MathAvx.Exp(FCR))));
                    Vector256<float> crownRatioAdjustment = OrganonVariant.GetCrownRatioAdjustmentAvx(crownRatio);
                    Vector256<float> heightGrowth = Avx.Multiply(potentialHeightGrowth, Avx.Multiply(modifier, crownRatioAdjustment));
                    Vector256<float> expansionFactor = Avx.LoadVector256(expansionFactors + treeIndex); // maybe worth continuing in loop if all expansion factors are zero?
                    Vector256<float> zero = Vector256<float>.Zero;
                    heightGrowth = Avx.BlendVariable(heightGrowth, zero, Avx.CompareLessThanOrEqual(expansionFactor, zero));
                    DebugV.Assert(Avx.CompareGreaterThanOrEqual(heightGrowth, zero));
                    DebugV.Assert(Avx.CompareLessThanOrEqual(heightGrowth, Vector256.Create(Constant.Maximum.HeightIncrementInFeet)));
                    Avx.Store(heightGrowths + treeIndex, heightGrowth);

                    // if growth effective age > old tree age is true, then 0xffff ffff = -1 is returned from the comparison
                    // Reinterpreting as Vector256<int> and subtracting therefore adds one to the old tree record counts where old trees occur.
                    oldTreeRecordCount256 = Avx2.Subtract(oldTreeRecordCount256, Avx.CompareGreaterThan(growthEffectiveAge, oldTreeAgeThreshold).AsInt32());
                }
            }

            // oldTreeRecordCount128[0] = oldTreeRecordCount256[5] + oldTreeRecordCount256[0]
            // oldTreeRecordCount128[1] = oldTreeRecordCount256[6] + oldTreeRecordCount256[1]
            // oldTreeRecordCount128[2] = oldTreeRecordCount256[7] + oldTreeRecordCount256[2]
            // oldTreeRecordCount128[3] = oldTreeRecordCount256[8] + oldTreeRecordCount256[3]
            Vector128<int> oldTreeRecordCount128 = Avx.Add(Avx.ExtractVector128(oldTreeRecordCount256, Constant.Simd256x8.ExtractLower128),
                                                           Avx.ExtractVector128(oldTreeRecordCount256, Constant.Simd256x8.ExtractUpper128));
            // oldTreeRecordCount128[0] = a[1] + a[0] = oldTreeRecordCount256[6] + oldTreeRecordCount256[5] + oldTreeRecordCount256[1] + oldTreeRecordCount256[0]
            // oldTreeRecordCount128[1] = a[3] + a[2] = oldTreeRecordCount256[8] + oldTreeRecordCount256[7] + oldTreeRecordCount256[3] + oldTreeRecordCount256[2]
            // oldTreeRecordCount128[upper] = same as lower
            oldTreeRecordCount128 = Avx.HorizontalAdd(oldTreeRecordCount128, oldTreeRecordCount128);
            // oldTreeRecordCount128[0] = a[1] + a[0] = oldTreeRecordCount256[7] + [6] + [5] + [4] + [3] + [2] + [1] + [0]
            oldTreeRecordCount128 = Avx.HorizontalAdd(oldTreeRecordCount128, oldTreeRecordCount128);
            return oldTreeRecordCount256.ToScalar();
        }

        private unsafe int GrowHeightBigSixAvx10(OrganonConfiguration configuration, OrganonStand stand, Trees trees, float[] crownCompetitionByHeight)
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

            Vector256<float> P1 = Vector256.Create(heightCoefficients.P1);
            Vector256<float> P2 = Vector256.Create(heightCoefficients.P2);
            Vector256<float> P3 = Vector256.Create(heightCoefficients.P3);
            Debug.Assert((heightCoefficients.P4 == 0.5F) || (heightCoefficients.P4 == 1.0F)); // P4 = 0.5F; => B1 = sqrt(P4)
            Vector256<float> minusP5 = Vector256.Create(-heightCoefficients.P5);
            Debug.Assert(heightCoefficients.P6 == 2.0F); // P6 = 0.5F; => FCR = -P5 * (1 - crownRatio)^P6 * P7^sqrt(CCF)
            Vector256<float> P7 = Vector256.Create(heightCoefficients.P7);
            Vector256<float> P8 = Vector256.Create(heightCoefficients.P8);
            Vector256<float> oldTreeAgeThreshold = Vector256.Create(configuration.Variant.OldTreeAgeThreshold);
            Vector256<float> one = Vector256.Create(1.0F);

            Vector256<int> oldTreeRecordCount256 = Vector256<int>.Zero;
            fixed (float* crownRatios = &trees.CrownRatio[0], expansionFactors = &trees.LiveExpansionFactor[0], heights = &trees.Height[0], heightGrowths = &trees.HeightGrowth[0])
            {
                for (int treeIndex = 0; treeIndex < trees.Count; treeIndex += Constant.Simd256x8.Width)
                {
                    // inline version of GetGrowthEffectiveAge()
                    Vector256<float> height = Avx512F.LoadVector256(heights + treeIndex);
                    Vector256<float> growthEffectiveAge;
                    Vector256<float> potentialHeightGrowth;
                    if (trees.Species == FiaCode.TsugaHeterophylla)
                    {
                        growthEffectiveAge = WesternHemlock.GetFlewellingGrowthEffectiveAgeAvx(tsheSite!, this.TimeStepInYears, height, out potentialHeightGrowth);
                    }
                    else
                    {
                        growthEffectiveAge = DouglasFir.GetPsmeAbgrGrowthEffectiveAgeAvx10(psmeSite!, this.TimeStepInYears, height, out potentialHeightGrowth);
                    }
                    Vector256<float> crownCompetitionFactor = OrganonVariant.GetCrownCompetitionFactorByHeightAvx10(height, crownCompetitionByHeight);
                    Vector256<float> sqrtCrownCompetitionFactor = Avx512F.Sqrt(crownCompetitionFactor);
                    Vector256<float> crownCompetitionIncrementToP4;
                    if (heightCoefficients.P4 == 0.5F)
                    {
                        crownCompetitionIncrementToP4 = sqrtCrownCompetitionFactor;
                    }
                    else
                    {
                        crownCompetitionIncrementToP4 = crownCompetitionFactor;
                    }

                    Vector256<float> crownRatio = Avx512F.LoadVector256(crownRatios + treeIndex);
                    Vector256<float> proportionBelowCrown = Avx512F.Subtract(one, crownRatio);
                    Vector256<float> B0 = Avx512F.Multiply(P1, MathAvx10.Exp(Avx512F.Multiply(P2, crownCompetitionFactor)));
                    Vector256<float> B1 = MathAvx10.Exp(Avx512F.Multiply(P3, crownCompetitionIncrementToP4)); // exp(P3 * sqrt(CCI)) for PSME and THSE, exp(P3 * CCI) for ABGR
                    Vector256<float> FCR = Avx512F.Multiply(Avx512F.Multiply(minusP5, Avx512F.Multiply(proportionBelowCrown, proportionBelowCrown)), MathAvx10.Exp(Avx512F.Multiply(P7, sqrtCrownCompetitionFactor))); // P7 is 0.0 for ABGR and TSHE -> exp() = 1.0
                    Vector256<float> modifier = Avx512F.Multiply(P8, Avx512F.Add(B0, Avx512F.Multiply(Avx512F.Subtract(B1, B0), MathAvx10.Exp(FCR))));
                    Vector256<float> crownRatioAdjustment = OrganonVariant.GetCrownRatioAdjustmentAvx10(crownRatio);
                    Vector256<float> heightGrowth = Avx512F.Multiply(potentialHeightGrowth, Avx512F.Multiply(modifier, crownRatioAdjustment));
                    Vector256<float> expansionFactor = Avx512F.LoadVector256(expansionFactors + treeIndex); // maybe worth continuing in loop if all expansion factors are zero?
                    Vector256<float> zero = Vector256<float>.Zero;
                    heightGrowth = Avx512F.BlendVariable(heightGrowth, zero, Avx512F.CompareLessThanOrEqual(expansionFactor, zero));
                    DebugV.Assert(Avx512F.CompareGreaterThanOrEqual(heightGrowth, zero));
                    DebugV.Assert(Avx512F.CompareLessThanOrEqual(heightGrowth, Vector256.Create(Constant.Maximum.HeightIncrementInFeet)));
                    Avx512F.Store(heightGrowths + treeIndex, heightGrowth);

                    // if growth effective age > old tree age is true, then 0xffff ffff = -1 is returned from the comparison
                    // Reinterpreting as Vector256<int> and subtracting therefore adds one to the old tree record counts where old trees occur.
                    oldTreeRecordCount256 = Avx512F.Subtract(oldTreeRecordCount256, Avx512F.CompareGreaterThan(growthEffectiveAge, oldTreeAgeThreshold).AsInt32());
                }
            }

            Vector128<int> oldTreeRecordCount128 = Avx512F.Add(Avx512F.ExtractVector128(oldTreeRecordCount256, Constant.Simd256x8.ExtractLower128),
                                                               Avx512F.ExtractVector128(oldTreeRecordCount256, Constant.Simd256x8.ExtractUpper128));
            oldTreeRecordCount128 = Avx512F.HorizontalAdd(oldTreeRecordCount128, oldTreeRecordCount128);
            oldTreeRecordCount128 = Avx512F.HorizontalAdd(oldTreeRecordCount128, oldTreeRecordCount128);
            return oldTreeRecordCount256.ToScalar();
        }

        private unsafe int GrowHeightBigSixAvx512(OrganonConfiguration configuration, OrganonStand stand, Trees trees, float[] crownCompetitionByHeight)
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

            Vector512<float> P1 = Vector512.Create(heightCoefficients.P1);
            Vector512<float> P2 = Vector512.Create(heightCoefficients.P2);
            Vector512<float> P3 = Vector512.Create(heightCoefficients.P3);
            Debug.Assert((heightCoefficients.P4 == 0.5F) || (heightCoefficients.P4 == 1.0F)); // P4 = 0.5F; => B1 = sqrt(P4)
            Vector512<float> minusP5 = Vector512.Create(-heightCoefficients.P5);
            Debug.Assert(heightCoefficients.P6 == 2.0F); // P6 = 0.5F; => FCR = -P5 * (1 - crownRatio)^P6 * P7^sqrt(CCF)
            Vector512<float> P7 = Vector512.Create(heightCoefficients.P7);
            Vector512<float> P8 = Vector512.Create(heightCoefficients.P8);
            Vector512<float> oldTreeAgeThreshold = Vector512.Create(configuration.Variant.OldTreeAgeThreshold);
            Vector512<float> one = Vector512.Create(1.0F);

            Vector512<int> oldTreeRecordCount512 = Vector512<int>.Zero;
            fixed (float* crownRatios = &trees.CrownRatio[0], expansionFactors = &trees.LiveExpansionFactor[0], heights = &trees.Height[0], heightGrowths = &trees.HeightGrowth[0])
            {
                for (int treeIndex = 0; treeIndex < trees.Count; treeIndex += Constant.Simd512x16.Width)
                {
                    // inline version of GetGrowthEffectiveAge()
                    Vector512<float> height = Avx512F.LoadVector512(heights + treeIndex);
                    Vector512<float> growthEffectiveAge;
                    Vector512<float> potentialHeightGrowth;
                    if (trees.Species == FiaCode.TsugaHeterophylla)
                    {
                        growthEffectiveAge = WesternHemlock.GetFlewellingGrowthEffectiveAgeAvx512(tsheSite!, this.TimeStepInYears, height, out potentialHeightGrowth);
                    }
                    else
                    {
                        growthEffectiveAge = DouglasFir.GetPsmeAbgrGrowthEffectiveAgeAvx512(psmeSite!, this.TimeStepInYears, height, out potentialHeightGrowth);
                    }
                    Vector512<float> crownCompetitionFactor = OrganonVariant.GetCrownCompetitionFactorByHeightAvx512(height, crownCompetitionByHeight);
                    Vector512<float> sqrtCrownCompetitionFactor = Avx512F.Sqrt(crownCompetitionFactor);
                    Vector512<float> crownCompetitionIncrementToP4;
                    if (heightCoefficients.P4 == 0.5F)
                    {
                        crownCompetitionIncrementToP4 = sqrtCrownCompetitionFactor;
                    }
                    else
                    {
                        crownCompetitionIncrementToP4 = crownCompetitionFactor;
                    }

                    Vector512<float> crownRatio = Avx512F.LoadVector512(crownRatios + treeIndex);
                    Vector512<float> proportionBelowCrown = Avx512F.Subtract(one, crownRatio);
                    Vector512<float> B0 = Avx512F.Multiply(P1, MathAvx10.Exp(Avx512F.Multiply(P2, crownCompetitionFactor)));
                    Vector512<float> B1 = MathAvx10.Exp(Avx512F.Multiply(P3, crownCompetitionIncrementToP4)); // exp(P3 * sqrt(CCI)) for PSME and THSE, exp(P3 * CCI) for ABGR
                    Vector512<float> FCR = Avx512F.Multiply(Avx512F.Multiply(minusP5, Avx512F.Multiply(proportionBelowCrown, proportionBelowCrown)), MathAvx10.Exp(Avx512F.Multiply(P7, sqrtCrownCompetitionFactor))); // P7 is 0.0 for ABGR and TSHE -> exp() = 1.0
                    Vector512<float> modifier = Avx512F.Multiply(P8, Avx512F.Add(B0, Avx512F.Multiply(Avx512F.Subtract(B1, B0), MathAvx10.Exp(FCR))));
                    Vector512<float> crownRatioAdjustment = OrganonVariant.GetCrownRatioAdjustmentAvx512(crownRatio);
                    Vector512<float> heightGrowth = Avx512F.Multiply(potentialHeightGrowth, Avx512F.Multiply(modifier, crownRatioAdjustment));
                    Vector512<float> expansionFactor = Avx512F.LoadVector512(expansionFactors + treeIndex); // maybe worth continuing in loop if all expansion factors are zero?
                    Vector512<float> zero = Vector512<float>.Zero;
                    heightGrowth = Avx512F.BlendVariable(heightGrowth, zero, Avx512F.CompareLessThanOrEqual(expansionFactor, zero));
                    DebugV.Assert(Avx512F.CompareGreaterThanOrEqual(heightGrowth, zero));
                    DebugV.Assert(Avx512F.CompareLessThanOrEqual(heightGrowth, Vector512.Create(Constant.Maximum.HeightIncrementInFeet)));
                    Avx512F.Store(heightGrowths + treeIndex, heightGrowth);
                    
                    // if growth effective age > old tree age is true, then 0xffff ffff = -1 is returned from the comparison
                    // Reinterpreting as Vector512<int> and subtracting therefore adds one to the old tree record counts where old trees occur.
                    oldTreeRecordCount512 = Avx512F.Subtract(oldTreeRecordCount512, Avx512F.CompareGreaterThan(growthEffectiveAge, oldTreeAgeThreshold).AsInt32());
                }
            }

            // oldTreeRecordCount256[0] = oldTreeRecordCount512[8] + oldTreeRecordCount512[0]
            // oldTreeRecordCount256[1] = oldTreeRecordCount512[9] + oldTreeRecordCount512[1]
            // oldTreeRecordCount256[2] = oldTreeRecordCount512[10] + oldTreeRecordCount512[2]
            // oldTreeRecordCount256[3] = oldTreeRecordCount512[11] + oldTreeRecordCount512[3]
            // oldTreeRecordCount256[4] = oldTreeRecordCount512[12] + oldTreeRecordCount512[4]
            // oldTreeRecordCount256[5] = oldTreeRecordCount512[13] + oldTreeRecordCount512[5]
            // oldTreeRecordCount256[6] = oldTreeRecordCount512[14] + oldTreeRecordCount512[6]
            // oldTreeRecordCount256[7] = oldTreeRecordCount512[15] + oldTreeRecordCount512[7]
            Vector256<int> oldTreeRecordCount256 = Avx512F.Add(Avx512F.ExtractVector256(oldTreeRecordCount512, Constant.Simd512x16.ExtractLower256),
                                                               Avx512F.ExtractVector256(oldTreeRecordCount512, Constant.Simd512x16.ExtractUpper256));
            Vector128<int> oldTreeRecordCount128 = Avx512F.Add(Avx512F.ExtractVector128(oldTreeRecordCount256, Constant.Simd256x8.ExtractLower128),
                                                               Avx512F.ExtractVector128(oldTreeRecordCount256, Constant.Simd256x8.ExtractUpper128));
            oldTreeRecordCount128 = Avx512F.HorizontalAdd(oldTreeRecordCount128, oldTreeRecordCount128);
            oldTreeRecordCount128 = Avx512F.HorizontalAdd(oldTreeRecordCount128, oldTreeRecordCount128);
            return oldTreeRecordCount128.ToScalar();
        }

        private unsafe int GrowHeightBigSixVex128(OrganonConfiguration configuration, OrganonStand stand, Trees trees, float[] crownCompetitionByHeight)
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

            Vector128<float> P1 = Vector128.Create(heightCoefficients.P1);
            Vector128<float> P2 = Vector128.Create(heightCoefficients.P2);
            Vector128<float> P3 = Vector128.Create(heightCoefficients.P3);
            Debug.Assert((heightCoefficients.P4 == 0.5F) || (heightCoefficients.P4 == 1.0F)); // P4 = 0.5F; => B1 = sqrt(P4)
            Vector128<float> minusP5 = Vector128.Create(-heightCoefficients.P5);
            Debug.Assert(heightCoefficients.P6 == 2.0F); // P6 = 0.5F; => FCR = -P5 * (1 - crownRatio)^P6 * P7^sqrt(CCF)
            Vector128<float> P7 = Vector128.Create(heightCoefficients.P7);
            Vector128<float> P8 = Vector128.Create(heightCoefficients.P8);
            Vector128<float> oldTreeAgeThreshold = Vector128.Create(configuration.Variant.OldTreeAgeThreshold);
            Vector128<float> one = Vector128.Create(1.0F);

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
                        growthEffectiveAge = WesternHemlock.GetFlewellingGrowthEffectiveAgeVex128(tsheSite!, this.TimeStepInYears, height, out potentialHeightGrowth);
                    }
                    else
                    {
                        growthEffectiveAge = DouglasFir.GetPsmeAbgrGrowthEffectiveAgeVex128(psmeSite!, this.TimeStepInYears, height, out potentialHeightGrowth);
                    }
                    Vector128<float> crownCompetitionFactor = OrganonVariant.GetCrownCompetitionFactorByHeightVex128(height, crownCompetitionByHeight);
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
                    Vector128<float> B0 = Avx.Multiply(P1, MathAvx.Exp(Avx.Multiply(P2, crownCompetitionFactor)));
                    Vector128<float> B1 = MathAvx.Exp(Avx.Multiply(P3, crownCompetitionIncrementToP4)); // exp(P3 * sqrt(CCI)) for PSME and THSE, exp(P3 * CCI) for ABGR
                    Vector128<float> FCR = Avx.Multiply(Avx.Multiply(minusP5, Avx.Multiply(proportionBelowCrown, proportionBelowCrown)), MathAvx.Exp(Avx.Multiply(P7, sqrtCrownCompetitionFactor))); // P7 is 0.0 for ABGR and TSHE -> exp() = 1.0
                    Vector128<float> modifier = Avx.Multiply(P8, Avx.Add(B0, Avx.Multiply(Avx.Subtract(B1, B0), MathAvx.Exp(FCR))));
                    Vector128<float> crownRatioAdjustment = OrganonVariant.GetCrownRatioAdjustmentVex128(crownRatio);
                    Vector128<float> heightGrowth = Avx.Multiply(potentialHeightGrowth, Avx.Multiply(modifier, crownRatioAdjustment));
                    Vector128<float> expansionFactor = Avx.LoadVector128(expansionFactors + treeIndex); // maybe worth continuing in loop if all expansion factors are zero?
                    Vector128<float> zero = Vector128<float>.Zero;
                    heightGrowth = Avx.BlendVariable(heightGrowth, zero, Avx.CompareLessThanOrEqual(expansionFactor, zero));
                    DebugV.Assert(Avx.CompareGreaterThanOrEqual(heightGrowth, zero));
                    DebugV.Assert(Avx.CompareLessThanOrEqual(heightGrowth, Vector128.Create(Constant.Maximum.HeightIncrementInFeet)));
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
