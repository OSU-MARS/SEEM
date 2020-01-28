using System;

namespace Osu.Cof.Organon
{
    public abstract class OrganonVariant
    {
        public int TimeStepInYears { get; private set; }
        public TreeModel TreeModel { get; private set; }

        protected OrganonVariant(TreeModel treeModel)
        {
            this.TimeStepInYears = treeModel == TreeModel.OrganonRap ? 1 : 5;
            this.TreeModel = treeModel;
        }

        public static NotSupportedException CreateUnhandledModelException(TreeModel treeModel)
        {
            return new NotSupportedException(String.Format("Unhandled model {0}.", treeModel));
        }

        public static NotSupportedException CreateUnhandledSpeciesException(FiaCode species)
        {
            return new NotSupportedException(String.Format("Unhandled species {0}.", species));
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
        public abstract float GetCrownWidth(FiaCode species, float HLCW, float LCW, float HT, float DBH, float XL);

        public int GetEndYear(int simulationStep)
        {
            return this.TimeStepInYears * (simulationStep + 1);
        }

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
        /// <param name="OG"></param>
        /// <returns>Height to crown base (feet).</returns>
        public abstract float GetHeightToCrownBase(FiaCode species, float HT, float DBH, float CCFL, float BA, float SI_1, float SI_2, float OG);

        /// <summary>
        /// Estimate height to largest crown width.
        /// </summary>
        /// <param name="species">Tree's species.</param>
        /// <param name="HT">Tree's height (feet).</param>
        /// <param name="CR">Tree's crown ratio.</param>
        /// <param name="SCR"></param>
        /// <returns>Height to largest crown width (feet)</returns>
        public abstract float GetHeightToLargestCrownWidth(FiaCode species, float HT, float CR);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="species">Tree's species.</param>
        /// <param name="MCW">Tree's maximum crown width (feet).</param>
        /// <param name="CR">Tree's crown ratio.</param>
        /// <param name="DBH">Tree's diameter at breast height (inches).</param>
        /// <param name="HT">Tree's height (feet).</param>
        /// <returns>Tree's largest crown width (feet).</returns>
        public abstract float GetLargestCrownWidth(FiaCode species, float MCW, float CR, float DBH, float HT);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="species">Tree's species.</param>
        /// <param name="D">Tree's diameter at breast height (inches).</param>
        /// <param name="H">Tree's height (feet).</param>
        /// <returns>Estimated maximum crown width.</returns>
        public abstract float GetMaximumCrownWidth(FiaCode species, float D, float H);

        public abstract float GetMaximumHeightToCrownBase(FiaCode species, float HT, float CCFL);

        public abstract void GetMortalityCoefficients(FiaCode species, float DBH, float CR, float SI_1, float SI_2, float BAL, float OG, out float POW, out float PM);

        /// <summary>
        /// Predict height from DBH for minor (non-big six) species.
        /// </summary>
        /// <param name="species">Tree's species.</param>
        /// <param name="dbhInInches">Diameter at breast height (inches).</param>
        /// <returns>Predicted height in feet.</param>
        public abstract float GetPredictedHeight(FiaCode species, float dbhInInches);

        public abstract float GrowDiameter(FiaCode species, float dbhInInches, float crownRatio, float SITE, float SBAL1, StandDensity densityBeforeGrowth);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="species"></param>
        /// <param name="potentialHeightGrowth"></param>
        /// <param name="CR">Crown ratio.</param>
        /// <param name="SCCH">Percent stand level crown closure at height.</param>
        /// <returns>Height growth in feet.</param>
        public abstract float GrowHeightBigSix(FiaCode species, float potentialHeightGrowth, float CR, float TCCH);

        public bool IsBigSixSpecies(FiaCode species)
        {
            switch (this.TreeModel)
            {
                case TreeModel.OrganonNwo:
                    return (species == FiaCode.PseudotsugaMenziesii) || (species == FiaCode.AbiesGrandis) ||
                           (species == FiaCode.TsugaHeterophylla);
                case TreeModel.OrganonSmc:
                    return (species == FiaCode.PseudotsugaMenziesii) || (species == FiaCode.AbiesConcolor) ||
                           (species == FiaCode.AbiesGrandis) || (species == FiaCode.TsugaHeterophylla);
                case TreeModel.OrganonRap:
                    return (species == FiaCode.AlnusRubra) || (species == FiaCode.PseudotsugaMenziesii) ||
                           (species == FiaCode.TsugaHeterophylla);
                case TreeModel.OrganonSwo:
                    return (species == FiaCode.AbiesConcolor) || (species == FiaCode.AbiesGrandis) ||
                           (species == FiaCode.CalocedrusDecurrens) || (species == FiaCode.PinusLambertiana) ||
                           (species == FiaCode.PinusPonderosa) || (species == FiaCode.PseudotsugaMenziesii);
                default:
                    throw Organon.OrganonVariant.CreateUnhandledModelException(this.TreeModel);
            }
        }

        public bool IsSpeciesSupported(FiaCode species)
        {
            switch (this.TreeModel)
            {
                case TreeModel.OrganonNwo:
                case TreeModel.OrganonSmc:
                    return Constant.NwoSmcSpecies.Contains(species);
                case TreeModel.OrganonRap:
                    return Constant.RapSpecies.Contains(species);
                case TreeModel.OrganonSwo:
                    return Constant.SwoSpecies.Contains(species);
                default:
                    throw Organon.OrganonVariant.CreateUnhandledModelException(this.TreeModel);
            }
        }
    }
}
