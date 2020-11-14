using System;
using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Osu.Cof.Ferm.Organon
{
    public abstract class OrganonVariant
    {
        public float OldTreeAgeThreshold { get; private init; }
        public int TimeStepInYears { get; private init; }
        public TreeModel TreeModel { get; private init; }

        protected OrganonVariant(TreeModel treeModel, float oldTreeAgeThreshold)
        {
            this.OldTreeAgeThreshold = oldTreeAgeThreshold;
            this.TimeStepInYears = treeModel == TreeModel.OrganonRap ? 1 : 5;
            this.TreeModel = treeModel;
        }

        public virtual void AddCrownCompetitionByHeight(Trees trees, float[] crownCompetitionByHeight)
        {
            FiaCode species = trees.Species;
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
                float heightToCrownBaseInFeet = heightInFeet - crownLengthInFeet;
                float maxCrownWidth = this.GetMaximumCrownWidth(species, dbhInInches, heightInFeet);
                float largestCrownWidth = this.GetLargestCrownWidth(species, maxCrownWidth, crownRatio, dbhInInches, heightInFeet);
                float heightToLargestCrownWidth = this.GetHeightToLargestCrownWidth(species, heightInFeet, crownRatio);

                float XHLCW = heightToLargestCrownWidth;
                float XLCW = largestCrownWidth;
                if (heightToCrownBaseInFeet > heightToLargestCrownWidth)
                {
                    XHLCW = heightToCrownBaseInFeet;
                    XLCW = this.GetCrownWidth(species, heightToLargestCrownWidth, largestCrownWidth, heightInFeet, dbhInInches, XHLCW);
                }

                float strataThickness = crownCompetitionByHeight[^1] / Constant.HeightStrata;
                for (int strataIndex = crownCompetitionByHeight.Length - 2; strataIndex >= 0; --strataIndex)
                {
                    float strataHeight = strataThickness * (strataIndex + 1);
                    float crownWidth = 0.0F;
                    if (strataHeight <= XHLCW)
                    {
                        crownWidth = XLCW;
                    }
                    else if ((strataHeight > XHLCW) && (strataHeight < heightInFeet))
                    {
                        crownWidth = this.GetCrownWidth(species, heightToLargestCrownWidth, largestCrownWidth, heightInFeet, dbhInInches, strataHeight);
                    }
                    float crownCompetitionFactor = Constant.CrownCompetionConstantEnglish * expansionFactor * crownWidth * crownWidth;
                    crownCompetitionByHeight[strataIndex] += crownCompetitionFactor;
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

            Debug.Assert(crownCompetitionByHeight.Length == Constant.HeightStrata + 1);
            int strataIndex = (int)(Constant.HeightStrata * height / crownCompetitionByHeight[^1]);
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
            Vector128<float> strataIndexAsFloat = Avx.Multiply(AvxExtensions.BroadcastScalarToVector128((float)Constant.HeightStrata / crownCompetitionByHeight[^1]), height);
            Vector128<int> strataIndex = Avx.ConvertToVector128Int32WithTruncation(strataIndexAsFloat);
            DebugV.Assert(Avx.CompareGreaterThan(strataIndex, AvxExtensions.BroadcastScalarToVector128(-1))); // no integer >=
            DebugV.Assert(Avx.CompareLessThan(strataIndex, AvxExtensions.BroadcastScalarToVector128(2 * Constant.HeightStrata))); // factor of 2 empirically fitted to tests, likely fragile

            // AVX implementation of Avx2.GatherVector128(&crownCompetitionByHeight[0], strataIndex, 1)
            float crownCompetitionFactor0 = 0.0F;
            int crownCompetitionIndex0 = strataIndex.ToScalar();
            if (crownCompetitionIndex0 < Constant.HeightStrata)
            {
                crownCompetitionFactor0 = crownCompetitionByHeight[crownCompetitionIndex0];
            }
            float crownCompetitionFactor1 = 0.0F;
            int crownCompetitionIndex1 = Avx.Shuffle(strataIndex, Constant.Simd128x4.ShuffleRotateLower1).ToScalar();
            if (crownCompetitionIndex1 < Constant.HeightStrata)
            {
                crownCompetitionFactor1 = crownCompetitionByHeight[crownCompetitionIndex1];
            }
            float crownCompetitionFactor2 = 0.0F;
            int crownCompetitionIndex2 = Avx.Shuffle(strataIndex, Constant.Simd128x4.ShuffleRotateLower2).ToScalar();
            if (crownCompetitionIndex2 < Constant.HeightStrata)
            {
                crownCompetitionFactor2 = crownCompetitionByHeight[crownCompetitionIndex2];
            }
            float crownCompetitionFactor3 = 0.0F;
            int crownCompetitionIndex3 = Avx.Shuffle(strataIndex, Constant.Simd128x4.ShuffleRotateLower3).ToScalar();
            if (crownCompetitionIndex3 < Constant.HeightStrata)
            {
                crownCompetitionFactor3 = crownCompetitionByHeight[crownCompetitionIndex3];
            }

            return Vector128.Create(crownCompetitionFactor0, crownCompetitionFactor1, crownCompetitionFactor2, crownCompetitionFactor3);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="species"></param>
        /// <param name="HLCW"></param>
        /// <param name="LCW"></param>
        /// <param name="HT"></param>
        /// <param name="DBH"></param>
        /// <param name="XL"></param>
        /// <returns>Crown width above maximum crown width (feet).</returns>
        protected abstract float GetCrownWidth(FiaCode species, float HLCW, float LCW, float HT, float DBH, float XL);

        public int GetEndYear(int simulationStep)
        {
            return this.TimeStepInYears * (simulationStep + 1);
        }

        public abstract float GetGrowthEffectiveAge(OrganonConfiguration configuration, OrganonStand stand, Trees trees, int treeIndex, out float potentialHeightGrowth);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="species">Tree's species.</param>
        /// <param name="HT">Tree height (feet).</param>
        /// <param name="DBH">Tree's diameter at breast height (inches)</param>
        /// <param name="CCFL"></param>
        /// <param name="BA">Stand basal area.</param>
        /// <param name="SI_1">Stand site index.</param>
        /// <param name="SI_2">Stand site index.</param>
        /// <param name="oldGrowthIndex"></param>
        /// <returns>Height to crown base (feet).</returns>
        public abstract float GetHeightToCrownBase(FiaCode species, float HT, float DBH, float CCFL, float BA, float SI_1, float SI_2, float oldGrowthIndex);

        /// <summary>
        /// Estimate height to largest crown width.
        /// </summary>
        /// <param name="species">Tree's species.</param>
        /// <param name="HT">Tree's height (feet).</param>
        /// <param name="CR">Tree's crown ratio.</param>
        /// <param name="SCR"></param>
        /// <returns>Height to largest crown width (feet)</returns>
        protected abstract float GetHeightToLargestCrownWidth(FiaCode species, float HT, float CR);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="species">Tree's species.</param>
        /// <param name="MCW">Tree's maximum crown width (feet).</param>
        /// <param name="CR">Tree's crown ratio.</param>
        /// <param name="DBH">Tree's diameter at breast height (inches).</param>
        /// <param name="HT">Tree's height (feet).</param>
        /// <returns>Tree's largest crown width (feet).</returns>
        protected abstract float GetLargestCrownWidth(FiaCode species, float MCW, float CR, float DBH, float HT);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="species">Tree's species.</param>
        /// <param name="D">Tree's diameter at breast height (inches).</param>
        /// <param name="H">Tree's height (feet).</param>
        /// <returns>Estimated maximum crown width.</returns>
        public abstract float GetMaximumCrownWidth(FiaCode species, float D, float H);

        protected abstract float GetMaximumHeightToCrownBase(FiaCode species, float HT, float CCFL);

        /// <summary>
        /// Predict height from DBH for minor (non-big six) species.
        /// </summary>
        /// <param name="species">Tree's species.</param>
        /// <param name="B0"></param>
        /// <param name="B1"></param>
        /// <param name="B2"></param>
        public abstract void GetHeightPredictionCoefficients(FiaCode species, out float B0, out float B1, out float B2);

        public virtual void GrowCrown(OrganonStand stand, Trees trees, OrganonStandDensity densityAfterGrowth, float oldGrowthIndicator, float nwoCrownRatioMultiplier)
        {
            float siteIndexFromDbh = stand.SiteIndex - 4.5F;
            float hemlockIndexFromDbh = stand.HemlockSiteIndex - 4.5F;
            FiaCode species = trees.Species;
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
                //     startCrownRatio = CALIB[species][1] * (1.0F - startHeightToCrownBase / startHeight);
                // }
                //startHeightToCrownBase = (1.0F - startCrownRatio) * startHeight;
                float startCrownRatio = trees.CrownRatio[treeIndex];
                float startHeightToCrownBase = (1.0F - startCrownRatio) * startHeightInFeet;

                // get height to crown base at end of period
                float endCcfl = densityAfterGrowth.GetCrownCompetitionFactorLarger(endDbhInInches);
                float endHeightToCrownBase = this.GetHeightToCrownBase(species, endHeightInFeet, endDbhInInches, endCcfl, densityAfterGrowth.BasalAreaPerAcre, siteIndexFromDbh, hemlockIndexFromDbh, oldGrowthIndicator);
                float endMaxHeightToCrownBase = this.GetMaximumHeightToCrownBase(species, endHeightInFeet, endCcfl);
                float endCrownRatio = 1.0F - endHeightToCrownBase / endHeightInFeet; // NWO overrides so NWO multiplier isn't needed here
                endHeightToCrownBase = (1.0F - endCrownRatio) * endHeightInFeet;

                // crown recession = change in height of crown base
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="stand"></param>
        /// <param name="trees"></param>
        /// <param name="crownCompetitionByHeight">Percent stand level crown closure by height.</param>
        /// <returns>Height growth in feet.</param>
        public abstract int GrowHeightBigSix(OrganonConfiguration configuration, OrganonStand stand, Trees trees, float[] crownCompetitionByHeight);

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
    }
}
