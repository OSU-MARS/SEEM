using Osu.Cof.Ferm.Species;
using System;
using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Osu.Cof.Ferm.Organon
{
    public class OrganonVariantNwo : OrganonVariant
    {
        public OrganonVariantNwo()
            : this(TreeModel.OrganonNwo, 120.0F)
        {
        }

        protected OrganonVariantNwo(TreeModel treeModel, float oldTreeAgeThreshold)
            : base(treeModel, oldTreeAgeThreshold)
        {
        }

        // reference scalar implementation
        //public override void AddCrownCompetitionByHeight(Trees trees, float[] crownCompetitionByHeight)
        //{
        //    // coefficients for maximum crown width
        //    FiaCode species = trees.Species;
        //    float mcwB0;
        //    float mcwB1;
        //    float mcwB2;
        //    float dbhLimitForMaxCrownWidth;
        //    switch (species)
        //    {
        //        // Paine and Hann(1982) FRL Research Paper 46
        //        case FiaCode.PseudotsugaMenziesii:
        //            mcwB0 = 4.6198F;
        //            mcwB1 = 1.8426F;
        //            mcwB2 = -0.011311F;
        //            dbhLimitForMaxCrownWidth = 81.45F;
        //            break;
        //        // GF Coefficients from Paine and Hann(1982) FRL Research Paper 46
        //        case FiaCode.AbiesGrandis:
        //            mcwB0 = 6.1880F;
        //            mcwB1 = 1.0069F;
        //            mcwB2 = 0.0F;
        //            dbhLimitForMaxCrownWidth = 999.99F;
        //            break;
        //        // Johnson(2002) Willamette Industries Report
        //        case FiaCode.TsugaHeterophylla:
        //            mcwB0 = 4.3586F;
        //            mcwB1 = 1.57458F;
        //            mcwB2 = 0.0F;
        //            dbhLimitForMaxCrownWidth = 76.70F;
        //            break;
        //        // Smith(1966) Proc. 6th World Forestry Conference
        //        case FiaCode.ThujaPlicata:
        //            mcwB0 = 4.0F;
        //            mcwB1 = 1.65F;
        //            mcwB2 = 0.0F;
        //            dbhLimitForMaxCrownWidth = 999.99F;
        //            break;
        //        // WH of Paine and Hann(1982) FRL Research Paper 46
        //        case FiaCode.TaxusBrevifolia:
        //            mcwB0 = 4.5652F;
        //            mcwB1 = 1.4147F;
        //            mcwB2 = 0.0F;
        //            dbhLimitForMaxCrownWidth = 999.99F;
        //            break;
        //        // Paine and Hann(1982) FRL Research Paper 46
        //        case FiaCode.ArbutusMenziesii:
        //            mcwB0 = 3.4298629F;
        //            mcwB1 = 1.3532302F;
        //            mcwB2 = 0.0F;
        //            dbhLimitForMaxCrownWidth = 999.99F;
        //            break;
        //        // Ek(1974) School of Natural Res., U.Wisc., Forestry Res. Notes.
        //        case FiaCode.AcerMacrophyllum:
        //            mcwB0 = 4.0953F;
        //            mcwB1 = 2.3849F;
        //            mcwB2 = -0.0102651F;
        //            dbhLimitForMaxCrownWidth = 102.53F;
        //            break;
        //        // WO Coefficients from Paine and Hann (1982) FRL Research Paper 46
        //        case FiaCode.QuercusGarryana:
        //            mcwB0 = 3.0785639F;
        //            mcwB1 = 1.9242211F;
        //            mcwB2 = 0.0F;
        //            dbhLimitForMaxCrownWidth = 999.99F;
        //            break;
        //        // Smith(1966) Proc. 6th World Forestry Conference
        //        case FiaCode.AlnusRubra:
        //            mcwB0 = 8.0F;
        //            mcwB1 = 1.53F;
        //            mcwB2 = 0.0F;
        //            dbhLimitForMaxCrownWidth = 999.99F;
        //            break;
        //        // GC of Paine and Hann(1982) FRL Research Paper 46
        //        case FiaCode.CornusNuttallii:
        //        case FiaCode.Salix:
        //            mcwB0 = 2.9793895F;
        //            mcwB1 = 1.5512443F;
        //            mcwB2 = -0.01416129F;
        //            dbhLimitForMaxCrownWidth = 54.77F;
        //            break;
        //        default:
        //            throw Trees.CreateUnhandledSpeciesException(species);
        //    }

        //    // coefficients for height to largest crown width
        //    float hlcwB1;
        //    switch (species)
        //    {
        //        // Hann(1999) FS 45: 217-225
        //        case FiaCode.PseudotsugaMenziesii:
        //            hlcwB1 = 0.062000F;
        //            break;
        //        // Hann and Hanus(2001) FRL Research Contribution 34
        //        case FiaCode.AbiesGrandis:
        //            hlcwB1 = 0.028454F;
        //            break;
        //        // Marshall, Johnson, and Hann(2003) CJFR 33: 2059-2066
        //        case FiaCode.TsugaHeterophylla:
        //            hlcwB1 = 0.355270F;
        //            break;
        //        // WH of Hann and Hanus(2001) FRL Research Contribution 34
        //        case FiaCode.ThujaPlicata:
        //        case FiaCode.TaxusBrevifolia:
        //            hlcwB1 = 0.209806F;
        //            break;
        //        // Hann and Hanus(2001) FRL Research Contribution 34
        //        case FiaCode.ArbutusMenziesii:
        //        case FiaCode.AcerMacrophyllum:
        //        case FiaCode.QuercusGarryana:
        //        case FiaCode.AlnusRubra:
        //        case FiaCode.CornusNuttallii:
        //        case FiaCode.Salix:
        //            hlcwB1 = 0.0F;
        //            break;
        //        default:
        //            throw Trees.CreateUnhandledSpeciesException(species);
        //    }

        //    float lcwB1;
        //    float lcwB2;
        //    float lcwB3;
        //    switch (species)
        //    {
        //        // Hann(1997) FRL Research Contribution 17
        //        case FiaCode.PseudotsugaMenziesii:
        //            lcwB1 = 0.0F;
        //            lcwB2 = 0.00436324F;
        //            lcwB3 = 0.6020020F;
        //            break;
        //        // Hann(1997) FRL Research Contribution 17
        //        case FiaCode.AbiesGrandis:
        //            lcwB1 = 0.0F;
        //            lcwB2 = 0.00308402F;
        //            lcwB3 = 0.0F;
        //            break;
        //        // Johnson(2002) Willamette Industries Report
        //        case FiaCode.TsugaHeterophylla:
        //            lcwB1 = 0.105590F;
        //            lcwB2 = 0.0035662F;
        //            lcwB3 = 0.0F;
        //            break;
        //        // IC of Hann(1997) FRL Research Contribution 17
        //        case FiaCode.ThujaPlicata:
        //            lcwB1 = -0.2513890F;
        //            lcwB2 = 0.006925120F;
        //            lcwB3 = 0.985922F;
        //            break;
        //        // WH of Hann(1997) FRL Research Contribution 17
        //        case FiaCode.TaxusBrevifolia:
        //            lcwB1 = 0.0F;
        //            lcwB2 = 0.0F;
        //            lcwB3 = 0.0F;
        //            break;
        //        // Hann(1997) FRL Research Contribution 17
        //        case FiaCode.ArbutusMenziesii:
        //            lcwB1 = 0.118621F;
        //            lcwB2 = 0.00384872F;
        //            lcwB3 = 0.0F;
        //            break;
        //        // Hann(1997) FRL Research Contribution 17
        //        case FiaCode.AcerMacrophyllum:
        //            lcwB1 = 0.0F;
        //            lcwB2 = 0.0F;
        //            lcwB3 = 1.470180F;
        //            break;
        //        // Hann(1997) FRL Research Contribution 17
        //        case FiaCode.QuercusGarryana:
        //            lcwB1 = 0.3648110F;
        //            lcwB2 = 0.0F;
        //            lcwB3 = 0.0F;
        //            break;
        //        // Hann(1997) FRL Research Contribution 17
        //        case FiaCode.AlnusRubra:
        //            lcwB1 = 0.3227140F;
        //            lcwB2 = 0.0F;
        //            lcwB3 = 0.0F;
        //            break;
        //        // GC of Hann(1997) FRL Research Contribution 17
        //        case FiaCode.CornusNuttallii:
        //        case FiaCode.Salix:
        //            lcwB1 = 0.0F;
        //            lcwB2 = 0.0F;
        //            lcwB3 = 1.61440F;
        //            break;
        //        default:
        //            throw Trees.CreateUnhandledSpeciesException(species);
        //    }

        //    // coefficients for crown width
        //    float cwB1;
        //    float cwB2;
        //    float cwB3;
        //    float cwMaxHeightDiameterRatio = Single.MaxValue;
        //    switch (species)
        //    {
        //        // DF Coefficients from Hann(1999) FS 45: 217-225
        //        case FiaCode.PseudotsugaMenziesii:
        //            cwB1 = 0.929973F;
        //            cwB2 = -0.135212F;
        //            cwB3 = -0.0157579F;
        //            cwMaxHeightDiameterRatio = 50.0F;
        //            break;
        //        // GF Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
        //        case FiaCode.AbiesGrandis:
        //            cwB1 = 0.999291F;
        //            cwB2 = 0.0F;
        //            cwB3 = -0.0314603F;
        //            cwMaxHeightDiameterRatio = 31.0F;
        //            break;
        //        // Marshall, Johnson, and Hann(2003) CJFR 33: 2059-2066
        //        case FiaCode.TsugaHeterophylla:
        //            cwB1 = 0.461782F;
        //            cwB2 = 0.552011F;
        //            cwB3 = 0.0F;
        //            break;
        //        // Hann and Hanus(2001) FRL Research Contribution 34
        //        case FiaCode.ThujaPlicata:
        //        case FiaCode.TaxusBrevifolia:
        //            cwB1 = 0.629785F;
        //            cwB2 = 0.0F;
        //            cwB3 = 0.0F;
        //            break;
        //        // Hann and Hanus(2001) FRL Research Contribution 34
        //        case FiaCode.ArbutusMenziesii:
        //        case FiaCode.AcerMacrophyllum:
        //        case FiaCode.QuercusGarryana:
        //        case FiaCode.AlnusRubra:
        //        case FiaCode.CornusNuttallii:
        //        case FiaCode.Salix:
        //            cwB1 = 0.5F;
        //            cwB2 = 0.0F;
        //            cwB3 = 0.0F;
        //            break;
        //        default:
        //            throw Trees.CreateUnhandledSpeciesException(species);
        //    }

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

        // VEX 128 with quads of strata: 3.0x speedup from scalar
        public unsafe override void AddCrownCompetitionByHeight(Trees trees, float[] crownCompetitionByHeight)
        {
            // coefficients for maximum crown width
            FiaCode species = trees.Species;
            float mcwB0;
            float mcwB1;
            float mcwB2;
            float dbhLimitForMaxCrownWidth;
            switch (species)
            {
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.PseudotsugaMenziesii:
                    mcwB0 = 4.6198F;
                    mcwB1 = 1.8426F;
                    mcwB2 = -0.011311F;
                    dbhLimitForMaxCrownWidth = 81.45F;
                    break;
                // GF Coefficients from Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.AbiesGrandis:
                    mcwB0 = 6.1880F;
                    mcwB1 = 1.0069F;
                    mcwB2 = 0.0F;
                    dbhLimitForMaxCrownWidth = 999.99F;
                    break;
                // Johnson(2002) Willamette Industries Report
                case FiaCode.TsugaHeterophylla:
                    mcwB0 = 4.3586F;
                    mcwB1 = 1.57458F;
                    mcwB2 = 0.0F;
                    dbhLimitForMaxCrownWidth = 76.70F;
                    break;
                // Smith(1966) Proc. 6th World Forestry Conference
                case FiaCode.ThujaPlicata:
                    mcwB0 = 4.0F;
                    mcwB1 = 1.65F;
                    mcwB2 = 0.0F;
                    dbhLimitForMaxCrownWidth = 999.99F;
                    break;
                // WH of Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.TaxusBrevifolia:
                    mcwB0 = 4.5652F;
                    mcwB1 = 1.4147F;
                    mcwB2 = 0.0F;
                    dbhLimitForMaxCrownWidth = 999.99F;
                    break;
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.ArbutusMenziesii:
                    mcwB0 = 3.4298629F;
                    mcwB1 = 1.3532302F;
                    mcwB2 = 0.0F;
                    dbhLimitForMaxCrownWidth = 999.99F;
                    break;
                // Ek(1974) School of Natural Res., U.Wisc., Forestry Res. Notes.
                case FiaCode.AcerMacrophyllum:
                    mcwB0 = 4.0953F;
                    mcwB1 = 2.3849F;
                    mcwB2 = -0.0102651F;
                    dbhLimitForMaxCrownWidth = 102.53F;
                    break;
                // WO Coefficients from Paine and Hann (1982) FRL Research Paper 46
                case FiaCode.QuercusGarryana:
                    mcwB0 = 3.0785639F;
                    mcwB1 = 1.9242211F;
                    mcwB2 = 0.0F;
                    dbhLimitForMaxCrownWidth = 999.99F;
                    break;
                // Smith(1966) Proc. 6th World Forestry Conference
                case FiaCode.AlnusRubra:
                    mcwB0 = 8.0F;
                    mcwB1 = 1.53F;
                    mcwB2 = 0.0F;
                    dbhLimitForMaxCrownWidth = 999.99F;
                    break;
                // GC of Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    mcwB0 = 2.9793895F;
                    mcwB1 = 1.5512443F;
                    mcwB2 = -0.01416129F;
                    dbhLimitForMaxCrownWidth = 54.77F;
                    break;
                default:
                    throw Trees.CreateUnhandledSpeciesException(species);
            }

            // coefficients for height to largest crown width
            float hlcwB1;
            switch (species)
            {
                // Hann(1999) FS 45: 217-225
                case FiaCode.PseudotsugaMenziesii:
                    hlcwB1 = 0.062000F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.AbiesGrandis:
                    hlcwB1 = 0.028454F;
                    break;
                // Marshall, Johnson, and Hann(2003) CJFR 33: 2059-2066
                case FiaCode.TsugaHeterophylla:
                    hlcwB1 = 0.355270F;
                    break;
                // WH of Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.ThujaPlicata:
                case FiaCode.TaxusBrevifolia:
                    hlcwB1 = 0.209806F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.ArbutusMenziesii:
                case FiaCode.AcerMacrophyllum:
                case FiaCode.QuercusGarryana:
                case FiaCode.AlnusRubra:
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    hlcwB1 = 0.0F;
                    break;
                default:
                    throw Trees.CreateUnhandledSpeciesException(species);
            }

            float lcwB1;
            float lcwB2;
            float lcwB3;
            switch (species)
            {
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.PseudotsugaMenziesii:
                    lcwB1 = 0.0F;
                    lcwB2 = 0.00436324F;
                    lcwB3 = 0.6020020F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.AbiesGrandis:
                    lcwB1 = 0.0F;
                    lcwB2 = 0.00308402F;
                    lcwB3 = 0.0F;
                    break;
                // Johnson(2002) Willamette Industries Report
                case FiaCode.TsugaHeterophylla:
                    lcwB1 = 0.105590F;
                    lcwB2 = 0.0035662F;
                    lcwB3 = 0.0F;
                    break;
                // IC of Hann(1997) FRL Research Contribution 17
                case FiaCode.ThujaPlicata:
                    lcwB1 = -0.2513890F;
                    lcwB2 = 0.006925120F;
                    lcwB3 = 0.985922F;
                    break;
                // WH of Hann(1997) FRL Research Contribution 17
                case FiaCode.TaxusBrevifolia:
                    lcwB1 = 0.0F;
                    lcwB2 = 0.0F;
                    lcwB3 = 0.0F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.ArbutusMenziesii:
                    lcwB1 = 0.118621F;
                    lcwB2 = 0.00384872F;
                    lcwB3 = 0.0F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.AcerMacrophyllum:
                    lcwB1 = 0.0F;
                    lcwB2 = 0.0F;
                    lcwB3 = 1.470180F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.QuercusGarryana:
                    lcwB1 = 0.3648110F;
                    lcwB2 = 0.0F;
                    lcwB3 = 0.0F;
                    break;
                // Hann(1997) FRL Research Contribution 17
                case FiaCode.AlnusRubra:
                    lcwB1 = 0.3227140F;
                    lcwB2 = 0.0F;
                    lcwB3 = 0.0F;
                    break;
                // GC of Hann(1997) FRL Research Contribution 17
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    lcwB1 = 0.0F;
                    lcwB2 = 0.0F;
                    lcwB3 = 1.61440F;
                    break;
                default:
                    throw Trees.CreateUnhandledSpeciesException(species);
            }

            // coefficients for crown width
            float cwB1;
            float cwB2;
            float cwB3;
            float cwMaxHeightDiameterRatio = Single.MaxValue;
            switch (species)
            {
                // DF Coefficients from Hann(1999) FS 45: 217-225
                case FiaCode.PseudotsugaMenziesii:
                    cwB1 = 0.929973F;
                    cwB2 = -0.135212F;
                    cwB3 = -0.0157579F;
                    cwMaxHeightDiameterRatio = 50.0F;
                    break;
                // GF Coefficients from Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.AbiesGrandis:
                    cwB1 = 0.999291F;
                    cwB2 = 0.0F;
                    cwB3 = -0.0314603F;
                    cwMaxHeightDiameterRatio = 31.0F;
                    break;
                // Marshall, Johnson, and Hann(2003) CJFR 33: 2059-2066
                case FiaCode.TsugaHeterophylla:
                    cwB1 = 0.461782F;
                    cwB2 = 0.552011F;
                    cwB3 = 0.0F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.ThujaPlicata:
                case FiaCode.TaxusBrevifolia:
                    cwB1 = 0.629785F;
                    cwB2 = 0.0F;
                    cwB3 = 0.0F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.ArbutusMenziesii:
                case FiaCode.AcerMacrophyllum:
                case FiaCode.QuercusGarryana:
                case FiaCode.AlnusRubra:
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    cwB1 = 0.5F;
                    cwB2 = 0.0F;
                    cwB3 = 0.0F;
                    break;
                default:
                    throw Trees.CreateUnhandledSpeciesException(species);
            }

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
                    Debug.Assert(heightInFeet >= 4.5F);
                    float dbhForMaxCrownWidth = MathF.Min(dbhInInches, dbhLimitForMaxCrownWidth);
                    float maxCrownWidth = mcwB0 + mcwB1 * dbhForMaxCrownWidth + mcwB2 * dbhForMaxCrownWidth * dbhForMaxCrownWidth;

                    // height to crown base and largest crown width
                    float largestCrownWidth = maxCrownWidth * MathV.Pow(crownRatio, lcwB1 + lcwB2 * crownLengthInFeet + lcwB3 * dbhInInches / heightInFeet);
                    float heightToLargestCrownWidth = heightInFeet - (1.0F - hlcwB1) * crownLengthInFeet;
                    float heightToCrownBaseInFeet = heightInFeet - crownLengthInFeet;
                    float cwB3heightDiameterRatio = cwB3 * MathF.Min(heightInFeet / dbhInInches, cwMaxHeightDiameterRatio);
                    if (heightToCrownBaseInFeet > heightToLargestCrownWidth)
                    {
                        float relativePosition = (heightInFeet - heightToCrownBaseInFeet) / (heightInFeet - heightToLargestCrownWidth);
                        largestCrownWidth *= MathV.Pow(relativePosition, cwB1 + cwB2 * MathF.Sqrt(relativePosition) + cwB3heightDiameterRatio);
                        heightToLargestCrownWidth = heightToCrownBaseInFeet;
                    }

                    // crown competition factor by strata
                    float ccfExpansionFactor = 0.001803F * expansionFactor;
                    Vector128<float> ccfExpansionFactor128 = AvxExtensions.BroadcastScalarToVector128(ccfExpansionFactor);
                    Vector128<float> crownCompetitionFactor = AvxExtensions.BroadcastScalarToVector128(ccfExpansionFactor * largestCrownWidth * largestCrownWidth);
                    Vector128<float> cwB1_128 = AvxExtensions.BroadcastScalarToVector128(cwB1);
                    Vector128<float> cwB2_128 = AvxExtensions.BroadcastScalarToVector128(cwB2);
                    Vector128<float> cwB3heightDiameterRatio128 = AvxExtensions.BroadcastScalarToVector128(cwB3heightDiameterRatio);
                    Vector128<float> heightInFeet128 = AvxExtensions.BroadcastScalarToVector128(heightInFeet);
                    Vector128<float> heightToLargestCrownWidth128 = AvxExtensions.BroadcastScalarToVector128(heightToLargestCrownWidth);
                    Vector128<float> largestCrownWidth128 = AvxExtensions.BroadcastScalarToVector128(largestCrownWidth);
                    Vector128<float> strataHeightIncrement = AvxExtensions.BroadcastScalarToVector128(4.0F * crownCompetitionByHeight[^1] / Constant.HeightStrata);
                    Vector128<float> strataHeight = Avx.Multiply(Vector128.Create(0.125F, 0.375F, 0.625F, 0.875F), strataHeightIncrement); // find CCF at middle of strata
                    Vector128<float> zero = Vector128<float>.Zero;
                    for (int strataIndex = 0; strataIndex < crownCompetitionByHeight.Length - 1; strataIndex += 4)
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
                        crownCompetitionFactor = Avx.Blend(zero, crownCompetitionFactor, (byte)strataBelowTreeHeightMask);

                        // accumulate CCF
                        Vector128<float> crownCompetitionByHeight128 = Avx.LoadVector128(pinnedCrownCompetitionByHeight + strataIndex);
                        crownCompetitionByHeight128 = Avx.Add(crownCompetitionByHeight128, crownCompetitionFactor);
                        Avx.Store(pinnedCrownCompetitionByHeight + strataIndex, crownCompetitionByHeight128);

                        // move upwards to next quad of strata
                        strataHeight = Avx.Add(strataHeight, strataHeightIncrement);
                    }
                }
            }
        }

        // VEX 128 with quads of trees: 2.1x speedup from scalar
        //public override unsafe void AddCrownCompetitionByHeight(Trees trees, float[] crownCompetitionByHeight)
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
        //        for (int treeIndex = 0; treeIndex < trees.Count; treeIndex += 4)
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

        protected override float GetCrownWidth(FiaCode species, float HLCW, float LCW, float HT, float DBH, float XL)
        {
            throw new NotImplementedException("Inlined into AddCrownCompetitionByHeight() for efficiency.");
        }

        public override float GetGrowthEffectiveAge(OrganonConfiguration configuration, OrganonStand stand, Trees trees, int treeIndex, out float potentialHeightGrowth)
        {
            float growthEffectiveAge;
            if (trees.Species == FiaCode.TsugaHeterophylla)
            {
                // GROWTH EFFECTIVE AGE FROM FLEWELLING'S WESTERN HEMLOCK DOMINANT HEIGHT GROWTH EQUATION
                WesternHemlock.SiteConstants siteConstants = new WesternHemlock.SiteConstants(stand.HemlockSiteIndex);
                growthEffectiveAge = WesternHemlock.GetFlewellingGrowthEffectiveAge(siteConstants, this.TimeStepInYears, trees.Height[treeIndex], out potentialHeightGrowth);
            }
            else
            {
                // GROWTH EFFECTIVE AGE FROM BRUCE'S (1981) DOMINANT HEIGHT GROWTH EQUATION FOR DOUGLAS-FIR AND GRAND FIR
                DouglasFir.SiteConstants siteConstants = new DouglasFir.SiteConstants(stand.SiteIndex); 
                growthEffectiveAge = DouglasFir.GetBrucePsmeAbgrGrowthEffectiveAge(siteConstants, this.TimeStepInYears, trees.Height[treeIndex], out potentialHeightGrowth);
            }
            return growthEffectiveAge;
        }

        public override void GetHeightPredictionCoefficients(FiaCode species, out float B0, out float B1, out float B2)
        {
            switch (species)
            {
                // Wang and Hann(1988) FRL Research Paper 51
                case FiaCode.PseudotsugaMenziesii:
                    B0 = 7.04524F;
                    B1 = -5.16836F;
                    B2 = -0.253869F;
                    break;
                // Wang and Hann(1988) FRL Research Paper 51
                case FiaCode.AbiesGrandis:
                    B0 = 7.42808F;
                    B1 = -5.80832F;
                    B2 = -0.240317F;
                    break;
                // Johnson(2000) Willamette Industries Report
                case FiaCode.TsugaHeterophylla:
                    B0 = 5.93792F;
                    B1 = -4.43822F;
                    B2 = -0.411373F;
                    break;
                // Hann and Hanus(2002) OSU Department of Forest Management Internal Report #2
                case FiaCode.ThujaPlicata:
                    B0 = 6.14817441F;
                    B1 = -5.40092761F;
                    B2 = -0.38922036F;
                    break;
                // Wang and Hann(1988) FRL Research Paper 51
                case FiaCode.TaxusBrevifolia:
                    B0 = 9.30172F;
                    B1 = -7.50951F;
                    B2 = -0.100000F;
                    break;
                // Wang and Hann(1988) FRL Research Paper 51
                case FiaCode.ArbutusMenziesii:
                    B0 = 5.84487F;
                    B1 = -3.84795F;
                    B2 = -0.289213F;
                    break;
                // Wang and Hann(1988) FRL Research Paper 51
                case FiaCode.AcerMacrophyllum:
                    B0 = 5.21462F;
                    B1 = -2.70252F;
                    B2 = -0.354756F;
                    break;
                // Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23: 26-33
                case FiaCode.QuercusGarryana:
                    B0 = 4.69753118F;
                    B1 = -3.51586969F;
                    B2 = -0.57665068F;
                    break;
                // Hann and Hanus(2002) OSU Department of Forest Management Internal Report #1
                case FiaCode.AlnusRubra:
                    B0 = 5.59759126F;
                    B1 = -3.19942952F;
                    B2 = -0.38783403F;
                    break;
                // Wang and Hann(1988) FRL Research Paper 51
                case FiaCode.CornusNuttallii:
                    B0 = 4.49727F;
                    B1 = -2.07667F;
                    B2 = -0.388650F;
                    break;
                // Wang and Hann(1988) FRL Research Paper 51
                case FiaCode.Salix:
                    B0 = 4.88361F;
                    B1 = -2.47605F;
                    B2 = -0.309050F;
                    break;
                default:
                    throw Trees.CreateUnhandledSpeciesException(species);
            }
        }

        public override float GetHeightToCrownBase(FiaCode species, float HT, float DBH, float CCFL, float BA, float SI_1, float SI_2, float OG)
        {
            float hcbB0;
            float hcbB1;
            float hcbB2;
            float hcbB3;
            float hcbB4;
            float hcbB5;
            float hcbB6;
            switch (species)
            {
                // DF Coefficients from Zumrawi and Hann (1989) FRL Research Paper 52
                case FiaCode.PseudotsugaMenziesii:
                    hcbB0 = 1.94093F;
                    hcbB1 = -0.0065029F;
                    hcbB2 = -0.0048737F;
                    hcbB3 = -0.261573F;
                    hcbB4 = 1.08785F;
                    hcbB5 = 0.0F;
                    hcbB6 = 0.0F;
                    break;
                // Zumrawi and Hann (1989) FRL Research Paper 52
                case FiaCode.AbiesGrandis:
                    hcbB0 = 1.04746F;
                    hcbB1 = -0.0066643F;
                    hcbB2 = -0.0067129F;
                    hcbB3 = 0.0F;
                    hcbB4 = 0.0F;
                    hcbB5 = 0.0F;
                    hcbB6 = 0.0F;
                    break;
                // Johnson (2002) Willamette Industries Report
                case FiaCode.TsugaHeterophylla:
                    hcbB0 = 1.92682F;
                    hcbB1 = -0.00280478F;
                    hcbB2 = -0.0011939F;
                    hcbB3 = -0.513134F;
                    hcbB4 = 3.68901F;
                    hcbB5 = 0.00742219F;
                    hcbB6 = 0.0F;
                    break;
                // Hann and Hanus (2002) OSU Department of Forest Management Internal Report #2
                case FiaCode.ThujaPlicata:
                    hcbB0 = 4.49102006F;
                    hcbB1 = 0.0F;
                    hcbB2 = -0.00132412F;
                    hcbB3 = -1.01460531F;
                    hcbB4 = 0.0F;
                    hcbB5 = 0.01340624F;
                    hcbB6 = 0.0F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.TaxusBrevifolia:
                    hcbB0 = 0.0F;
                    hcbB1 = 0.0F;
                    hcbB2 = 0.0F;
                    hcbB3 = 0.0F;
                    hcbB4 = 2.030940382F;
                    hcbB5 = 0.0F;
                    hcbB6 = 0.0F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.ArbutusMenziesii:
                    hcbB0 = 2.955339267F;
                    hcbB1 = 0.0F;
                    hcbB2 = 0.0F;
                    hcbB3 = -0.798610738F;
                    hcbB4 = 3.095269471F;
                    hcbB5 = 0.0F;
                    hcbB6 = 0.700465646F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.AcerMacrophyllum:
                    hcbB0 = 0.9411395642F;
                    hcbB1 = -0.00768402F;
                    hcbB2 = -0.005476131F;
                    hcbB3 = 0.0F;
                    hcbB4 = 0.0F;
                    hcbB5 = 0.0F;
                    hcbB6 = 0.0F;
                    break;
                // Gould, Marshall, and Harrington (2008) West.J.Appl.For. 23: 26-33
                case FiaCode.QuercusGarryana:
                    hcbB0 = 1.05786632F;
                    hcbB1 = 0.0F;
                    hcbB2 = -0.00183283F;
                    hcbB3 = -0.28644547F;
                    hcbB4 = 0.0F;
                    hcbB5 = 0.0F;
                    hcbB6 = 0.0F;
                    break;
                // Hann and Hanus (2002) OSU Department of Forest Management Internal Report #1
                case FiaCode.AlnusRubra:
                    hcbB0 = 0.56713781F;
                    hcbB1 = -0.010377976F;
                    hcbB2 = -0.002066036F;
                    hcbB3 = 0.0F;
                    hcbB4 = 1.39796223F;
                    hcbB5 = 0.0F;
                    hcbB6 = 0.0F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    hcbB0 = 0.0F;
                    hcbB1 = 0.0F;
                    hcbB2 = -0.005666559F;
                    hcbB3 = -0.745540494F;
                    hcbB4 = 0.0F;
                    hcbB5 = 0.038476613F;
                    hcbB6 = 0.0F;
                    break;
                default:
                    throw Trees.CreateUnhandledSpeciesException(species);
            }

            float siteIndexFromDbh = SI_1;
            if (species == FiaCode.TsugaHeterophylla)
            {
                siteIndexFromDbh = SI_2;
            }

            float HCB = HT / (1.0F + MathV.Exp(hcbB0 + hcbB1 * HT + hcbB2 * CCFL + hcbB3 * MathV.Ln(BA) + hcbB4 * (DBH / HT) + hcbB5 * siteIndexFromDbh + hcbB6 * OG * OG));
            Debug.Assert(HCB >= 0.0F);
            Debug.Assert(HCB <= HT);
            return HCB;
        }

        protected override float GetHeightToLargestCrownWidth(FiaCode species, float HT, float CR)
        {
            throw new NotImplementedException("Inlined into AddCrownCompetitionByHeight() for efficiency.");
        }

        protected override float GetLargestCrownWidth(FiaCode species, float MCW, float CR, float DBH, float HT)
        {
            throw new NotImplementedException("Inlined into AddCrownCompetitionByHeight() for efficiency.");
        }

        public override float GetMaximumCrownWidth(FiaCode species, float D, float H)
        {
            float B0;
            float B1;
            float B2;
            float PKDBH;
            switch (species)
            {
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.PseudotsugaMenziesii:
                    B0 = 4.6198F;
                    B1 = 1.8426F;
                    B2 = -0.011311F;
                    PKDBH = 81.45F;
                    break;
                // GF Coefficients from Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.AbiesGrandis:
                    B0 = 6.1880F;
                    B1 = 1.0069F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Johnson(2002) Willamette Industries Report
                case FiaCode.TsugaHeterophylla:
                    B0 = 4.3586F;
                    B1 = 1.57458F;
                    B2 = 0.0F;
                    PKDBH = 76.70F;
                    break;
                // Smith(1966) Proc. 6th World Forestry Conference
                case FiaCode.ThujaPlicata:
                    B0 = 4.0F;
                    B1 = 1.65F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // WH of Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.TaxusBrevifolia:
                    B0 = 4.5652F;
                    B1 = 1.4147F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.ArbutusMenziesii:
                    B0 = 3.4298629F;
                    B1 = 1.3532302F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Ek(1974) School of Natural Res., U.Wisc., Forestry Res. Notes.
                case FiaCode.AcerMacrophyllum:
                    B0 = 4.0953F;
                    B1 = 2.3849F;
                    B2 = -0.0102651F;
                    PKDBH = 102.53F;
                    break;
                // WO Coefficients from Paine and Hann (1982) FRL Research Paper 46
                case FiaCode.QuercusGarryana:
                    B0 = 3.0785639F;
                    B1 = 1.9242211F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // Smith(1966) Proc. 6th World Forestry Conference
                case FiaCode.AlnusRubra:
                    B0 = 8.0F;
                    B1 = 1.53F;
                    B2 = 0.0F;
                    PKDBH = 999.99F;
                    break;
                // GC of Paine and Hann(1982) FRL Research Paper 46
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B0 = 2.9793895F;
                    B1 = 1.5512443F;
                    B2 = -0.01416129F;
                    PKDBH = 54.77F;
                    break;
                default:
                    throw Trees.CreateUnhandledSpeciesException(species);
            }

            float DBH = D;
            if (DBH > PKDBH)
            {
                DBH = PKDBH;
            }
            float HT = H;
            float MCW;
            if (HT < 4.501F)
            {
                MCW = HT / 4.5F * B0;
            }
            else
            {
                MCW = B0 + B1 * DBH + B2 * DBH * DBH;
            }
            return MCW;
        }

        protected override float GetMaximumHeightToCrownBase(FiaCode species, float HT, float CCFL)
        {
            throw new NotImplementedException("Inlined into GrowCrown() for efficiency.");
        }

        public override void GrowCrown(OrganonStand stand, Trees trees, OrganonStandDensity densityAfterGrowth, float oldGrowthIndicator, float nwoCrownRatioMultiplier)
        {
            // coefficients for maximum height to crown base
            float mhcbB0;
            float mhcbB1;
            float mhcbB2;
            float mhcbB3 = 1.0F;
            float heightToCrownBaseRatioLimit;
            switch (trees.Species)
            {
                case FiaCode.PseudotsugaMenziesii:
                    mhcbB0 = 0.96F;
                    mhcbB1 = 0.26F;
                    mhcbB2 = -0.900721383F;
                    heightToCrownBaseRatioLimit = 0.95F;
                    break;
                case FiaCode.AbiesGrandis:
                    mhcbB0 = 0.96F;
                    mhcbB1 = 0.31F;
                    mhcbB2 = -2.450718394F;
                    heightToCrownBaseRatioLimit = 0.95F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    mhcbB0 = 1.01F;
                    mhcbB1 = 0.36F;
                    mhcbB2 = -0.944528054F;
                    heightToCrownBaseRatioLimit = 0.96F;
                    break;
                case FiaCode.ThujaPlicata:
                    mhcbB0 = 0.96F;
                    mhcbB1 = 0.31F;
                    mhcbB2 = -1.059636222F;
                    heightToCrownBaseRatioLimit = 0.95F;
                    break;
                case FiaCode.TaxusBrevifolia:
                    mhcbB0 = 0.85F;
                    mhcbB1 = 0.35F;
                    mhcbB2 = -0.922868139F;
                    mhcbB3 = 0.8F;
                    heightToCrownBaseRatioLimit = 0.80F;
                    break;
                case FiaCode.ArbutusMenziesii:
                    mhcbB0 = 0.981F;
                    mhcbB1 = 0.161F;
                    mhcbB2 = -1.73666044F;
                    heightToCrownBaseRatioLimit = 0.98F;
                    break;
                case FiaCode.AcerMacrophyllum:
                    mhcbB0 = 1.0F;
                    mhcbB1 = 0.45F;
                    mhcbB2 = -1.020016685F;
                    heightToCrownBaseRatioLimit = 0.95F;
                    break;
                case FiaCode.QuercusGarryana:
                    mhcbB0 = 1.0F;
                    mhcbB1 = 0.3F;
                    mhcbB2 = -0.95634399F;
                    mhcbB3 = 1.1F;
                    heightToCrownBaseRatioLimit = 0.98F;
                    break;
                case FiaCode.AlnusRubra:
                    mhcbB0 = 0.93F;
                    mhcbB1 = 0.18F;
                    mhcbB2 = -0.928243505F;
                    heightToCrownBaseRatioLimit = 0.92F;
                    break;
                case FiaCode.CornusNuttallii:
                    mhcbB0 = 1.0F;
                    mhcbB1 = 0.45F;
                    mhcbB2 = -1.020016685F;
                    heightToCrownBaseRatioLimit = 0.95F;
                    break;
                case FiaCode.Salix:
                    mhcbB0 = 0.985F;
                    mhcbB1 = 0.285F;
                    mhcbB2 = -0.969750805F;
                    mhcbB3 = 0.9F;
                    heightToCrownBaseRatioLimit = 0.98F;
                    break;
                default:
                    throw Trees.CreateUnhandledSpeciesException(trees.Species);
            }

            // coefficients for height to crown base
            float hcbB0;
            float hcbB1;
            float hcbB2;
            float hcbB3;
            float hcbB4;
            float hcbB5;
            float hcbB6;
            switch (trees.Species)
            {
                // DF Coefficients from Zumrawi and Hann (1989) FRL Research Paper 52
                case FiaCode.PseudotsugaMenziesii:
                    hcbB0 = 1.94093F;
                    hcbB1 = -0.0065029F;
                    hcbB2 = -0.0048737F;
                    hcbB3 = -0.261573F;
                    hcbB4 = 1.08785F;
                    hcbB5 = 0.0F;
                    hcbB6 = 0.0F;
                    break;
                // Zumrawi and Hann (1989) FRL Research Paper 52
                case FiaCode.AbiesGrandis:
                    hcbB0 = 1.04746F;
                    hcbB1 = -0.0066643F;
                    hcbB2 = -0.0067129F;
                    hcbB3 = 0.0F;
                    hcbB4 = 0.0F;
                    hcbB5 = 0.0F;
                    hcbB6 = 0.0F;
                    break;
                // Johnson (2002) Willamette Industries Report
                case FiaCode.TsugaHeterophylla:
                    hcbB0 = 1.92682F;
                    hcbB1 = -0.00280478F;
                    hcbB2 = -0.0011939F;
                    hcbB3 = -0.513134F;
                    hcbB4 = 3.68901F;
                    hcbB5 = 0.00742219F;
                    hcbB6 = 0.0F;
                    break;
                // Hann and Hanus (2002) OSU Department of Forest Management Internal Report #2
                case FiaCode.ThujaPlicata:
                    hcbB0 = 4.49102006F;
                    hcbB1 = 0.0F;
                    hcbB2 = -0.00132412F;
                    hcbB3 = -1.01460531F;
                    hcbB4 = 0.0F;
                    hcbB5 = 0.01340624F;
                    hcbB6 = 0.0F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.TaxusBrevifolia:
                    hcbB0 = 0.0F;
                    hcbB1 = 0.0F;
                    hcbB2 = 0.0F;
                    hcbB3 = 0.0F;
                    hcbB4 = 2.030940382F;
                    hcbB5 = 0.0F;
                    hcbB6 = 0.0F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.ArbutusMenziesii:
                    hcbB0 = 2.955339267F;
                    hcbB1 = 0.0F;
                    hcbB2 = 0.0F;
                    hcbB3 = -0.798610738F;
                    hcbB4 = 3.095269471F;
                    hcbB5 = 0.0F;
                    hcbB6 = 0.700465646F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.AcerMacrophyllum:
                    hcbB0 = 0.9411395642F;
                    hcbB1 = -0.00768402F;
                    hcbB2 = -0.005476131F;
                    hcbB3 = 0.0F;
                    hcbB4 = 0.0F;
                    hcbB5 = 0.0F;
                    hcbB6 = 0.0F;
                    break;
                // Gould, Marshall, and Harrington (2008) West.J.Appl.For. 23: 26-33
                case FiaCode.QuercusGarryana:
                    hcbB0 = 1.05786632F;
                    hcbB1 = 0.0F;
                    hcbB2 = -0.00183283F;
                    hcbB3 = -0.28644547F;
                    hcbB4 = 0.0F;
                    hcbB5 = 0.0F;
                    hcbB6 = 0.0F;
                    break;
                // Hann and Hanus (2002) OSU Department of Forest Management Internal Report #1
                case FiaCode.AlnusRubra:
                    hcbB0 = 0.56713781F;
                    hcbB1 = -0.010377976F;
                    hcbB2 = -0.002066036F;
                    hcbB3 = 0.0F;
                    hcbB4 = 1.39796223F;
                    hcbB5 = 0.0F;
                    hcbB6 = 0.0F;
                    break;
                // Hanus, Hann, and Marshall (2000) FRL Research Contribution 29
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    hcbB0 = 0.0F;
                    hcbB1 = 0.0F;
                    hcbB2 = -0.005666559F;
                    hcbB3 = -0.745540494F;
                    hcbB4 = 0.0F;
                    hcbB5 = 0.038476613F;
                    hcbB6 = 0.0F;
                    break;
                default:
                    throw Trees.CreateUnhandledSpeciesException(trees.Species);
            }

            // grow trees' crowns
            float siteIndexFromDbh = stand.SiteIndex;
            if (trees.Species == FiaCode.TsugaHeterophylla)
            {
                siteIndexFromDbh = stand.HemlockSiteIndex - 4.5F;
            }
            for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
            {
                float endDbhInInches = trees.Dbh[treeIndex];
                float endHeightInFeet = trees.Height[treeIndex];

                // get height to crown base at start of period
                float startDbh = endDbhInInches - trees.DbhGrowth[treeIndex]; // diameter at end of step
                float startHeight = endHeightInFeet - trees.HeightGrowth[treeIndex]; // height at beginning of step
                Debug.Assert(startDbh >= 0.0F);
                Debug.Assert(startHeight >= 0.0F);
                float startCrownRatio = trees.CrownRatio[treeIndex];
                float startHeightToCrownBase = (1.0F - startCrownRatio) * startHeight;

                // get height to crown base at end of period
                float endCcfl = densityAfterGrowth.GetCrownCompetitionFactorLarger(endDbhInInches);
                float endHeightToCrownBase = endHeightInFeet / (1.0F + MathV.Exp(hcbB0 + hcbB1 * endHeightInFeet + hcbB2 * endCcfl + hcbB3 * MathV.Ln(densityAfterGrowth.BasalAreaPerAcre) + hcbB4 * (endDbhInInches / endHeightInFeet) + hcbB5 * siteIndexFromDbh + hcbB6 * oldGrowthIndicator * oldGrowthIndicator));

                float crownCompetitionFraction = endCcfl / 100.0F;
                if (mhcbB3 != 1.0F)
                {
                    crownCompetitionFraction = MathV.Pow(crownCompetitionFraction, mhcbB3);
                }
                float heightToCrownBaseRatio = mhcbB0 - mhcbB1 * MathV.Exp(mhcbB2 * crownCompetitionFraction);
                if (heightToCrownBaseRatio > heightToCrownBaseRatioLimit)
                {
                    heightToCrownBaseRatio = heightToCrownBaseRatioLimit;
                }
                float endMaxHeightToCrownBase = heightToCrownBaseRatio * endHeightInFeet;
                Debug.Assert(endMaxHeightToCrownBase >= 0.0F);
                Debug.Assert(endMaxHeightToCrownBase <= endHeightInFeet);

                float endCrownRatio = nwoCrownRatioMultiplier * (1.0F - endHeightToCrownBase / endHeightInFeet);
                endHeightToCrownBase = (1.0F - endCrownRatio) * endHeightInFeet;

                // crown recession = change in height of crown base
                float crownRecession = endHeightToCrownBase - startHeightToCrownBase;
                if (crownRecession < 0.0F)
                {
                    crownRecession = 0.0F;
                }
                Debug.Assert(crownRecession >= 0.0F); // catch NaNs

                // update tree's crown ratio
                float alternateHeightToCrownBase1 = (1.0F - trees.CrownRatio[treeIndex]) * startHeight;
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

        public override void GrowDiameter(Trees trees, float growthMultiplier, float siteIndexFromDbh, OrganonStandDensity densityBeforeGrowth)
        {
            float B0;
            float B1;
            float B2;
            float B3;
            float B4;
            float B5;
            float B6;
            float K1 = 1.0F;
            int K2 = 2;
            int K3 = 2;
            float K4 = 5.0F;
            float speciesMultiplier = 0.8F; // source of these adjustment factors unknown
            switch (trees.Species)
            {
                // Zumrawi and Hann(1993) FRL Research Contribution 4
                case FiaCode.PseudotsugaMenziesii:
                    B0 = -4.69624F;
                    B1 = 0.339513F;
                    B2 = -0.000428261F;
                    B3 = 1.19952F;
                    B4 = 1.15612F;
                    B5 = -0.0000446327F;
                    B6 = -0.0237003F;
                    speciesMultiplier = 0.7011014F;
                    break;
                // Zumrawi and Hann(1993) FRL Research Contribution 4
                case FiaCode.AbiesGrandis:
                    B0 = -2.34619F;
                    B1 = 0.594640F;
                    B2 = -0.000976092F;
                    B3 = 1.12712F;
                    B4 = 0.555333F;
                    B5 = -0.0000290672F;
                    B6 = -0.0470848F;
                    speciesMultiplier = 0.8722F;
                    break;
                // Johnson(2002) Willamette Industries Report
                case FiaCode.TsugaHeterophylla:
                    B0 = -4.49867F;
                    B1 = 0.362369F;
                    B2 = -0.00153907F;
                    B3 = 1.1557F;
                    B4 = 1.12154F;
                    B5 = -0.0000201041F;
                    B6 = -0.0417388F;
                    speciesMultiplier = 0.7163F;
                    break;
                // Hann and Hanus(2002) OSU Department of Forest Management Internal Report #2
                case FiaCode.ThujaPlicata:
                    B0 = -11.45456097F;
                    B1 = 0.784133664F;
                    B2 = -0.0261377888F;
                    B3 = 0.70174783F;
                    B4 = 2.057236260F;
                    B5 = -0.00415440257F;
                    B6 = 0.0F;
                    K1 = 5.0F;
                    K2 = 1;
                    K3 = 1;
                    K4 = 2.7F;
                    break;
                // Hann and Hanus(2002) FRL Research Contribution 39
                case FiaCode.TaxusBrevifolia:
                    B0 = -9.15835863F;
                    B1 = 1.0F;
                    B2 = -0.00000035F;
                    B3 = 1.16688474F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = -0.02F;
                    K1 = 4000.0F;
                    K2 = 4;
                    K3 = 1;
                    K4 = 2.7F;
                    break;
                // Hann and Hanus(2002) FRL Research Contribution 39
                case FiaCode.ArbutusMenziesii:
                    B0 = -8.84531757F;
                    B1 = 1.5F;
                    B2 = -0.0006F;
                    B3 = 0.51225596F;
                    B4 = 0.418129153F;
                    B5 = -0.00355254593F;
                    B6 = -0.0321315389F;
                    K1 = 110.0F;
                    K2 = 2;
                    K3 = 1;
                    K4 = 2.7F;
                    speciesMultiplier = 0.7928F;
                    break;
                // Hann and Hanus(2002) FRL Research Contribution 39
                case FiaCode.AcerMacrophyllum:
                    B0 = -3.41449922F;
                    B1 = 1.0F;
                    B2 = -0.05F;
                    B3 = 0.0F;
                    B4 = 0.324349277F;
                    B5 = 0.0F;
                    B6 = -0.0989519477F;
                    K1 = 10.0F;
                    K2 = 1;
                    K3 = 1;
                    K4 = 2.7F;
                    break;
                // Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23:26-33
                case FiaCode.QuercusGarryana:
                    B0 = -7.81267986F;
                    B1 = 1.405616529F;
                    B2 = -0.0603105850F;
                    B3 = 0.64286007F;
                    B4 = 1.037687142F;
                    B5 = 0.0F;
                    B6 = -0.0787012218F;
                    K1 = 5.0F;
                    K2 = 1;
                    K3 = 1;
                    K4 = 2.7F;
                    break;
                // Hann and Hanus(2002) OSU Department of Forest Management Internal Report #1
                case FiaCode.AlnusRubra:
                    B0 = -4.39082007F;
                    B1 = 1.0F;
                    B2 = -0.0945057147F;
                    B3 = 1.06867026F;
                    B4 = 0.685908029F;
                    B5 = -0.00586331028F;
                    B6 = 0.0F;
                    K1 = 5.0F;
                    K2 = 1;
                    K3 = 1;
                    K4 = 2.7F;
                    speciesMultiplier = 1.0F;
                    break;
                // Hann and Hanus(2002) FRL Research Contribution 39
                case FiaCode.CornusNuttallii:
                case FiaCode.Salix:
                    B0 = -8.08352683F;
                    B1 = 1.0F;
                    B2 = -0.00000035F;
                    B3 = 0.31176647F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    B6 = -0.0730788052F;
                    K1 = 4000.0F;
                    K2 = 4;
                    K3 = 1;
                    K4 = 2.7F;
                    break;
                default:
                    throw Trees.CreateUnhandledSpeciesException(trees.Species);
            }
            Debug.Assert((K2 >= 1) && (K2 <= 4));
            Debug.Assert((K3 >= 1) && (K3 <= 4));

            for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
            {
                if (trees.LiveExpansionFactor[treeIndex] <= 0.0F)
                {
                    trees.DbhGrowth[treeIndex] = 0.0F;
                    continue;
                }

                float dbhInInches = trees.Dbh[treeIndex];
                float dbhK2 = dbhInInches;
                if (K2 > 1)
                {
                    dbhK2 *= dbhInInches; // square
                    if (K2 > 2)
                    {
                        dbhK2 *= dbhInInches; // cube
                        if (K2 > 3)
                        {
                            dbhK2 *= dbhInInches; // fourth power
                        }
                    }
                }

                float basalAreaLarger = densityBeforeGrowth.GetBasalAreaLarger(dbhInInches);
                float basalAreaLargerK3 = basalAreaLarger;
                if (K3 == 2)
                {
                    basalAreaLargerK3 *= basalAreaLarger;
                }

                float crownRatio = trees.CrownRatio[treeIndex];
                float LNDG = B0 + B1 * MathV.Ln(dbhInInches + K1) + B2 * dbhK2 + B3 * MathV.Ln((crownRatio + 0.2F) / 1.2F) + B4 * MathV.Ln(siteIndexFromDbh) + B5 * (basalAreaLargerK3 / MathV.Ln(dbhInInches + K4)) + B6 * MathF.Sqrt(basalAreaLarger);
                float crownRatioAdjustment = OrganonGrowth.GetCrownRatioAdjustment(crownRatio);
                trees.DbhGrowth[treeIndex] = speciesMultiplier * MathV.Exp(LNDG) * crownRatioAdjustment;
                Debug.Assert(trees.DbhGrowth[treeIndex] > 0.0F);
                Debug.Assert(trees.DbhGrowth[treeIndex] < Constant.Maximum.DiameterIncrementInInches);
            }
        }

        // scalar reference source
        //public override int GrowHeightBigSix(OrganonConfiguration configuration, OrganonStand stand, Trees trees, float[] crownCompetitionByHeight)
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

        public unsafe override int GrowHeightBigSix(OrganonConfiguration configuration, OrganonStand stand, Trees trees, float[] crownCompetitionByHeight)
        {
            Vector128<float> P1 = AvxExtensions.BroadcastScalarToVector128(1.0F);
            Vector128<float> P2;
            Vector128<float> P3;
            // float P4 = 0.5F; // sqrt()
            Vector128<float> minusP5;
            Vector128<float> P7 = Vector128<float>.Zero;
            Vector128<float> P8;
            switch (trees.Species)
            {
                // Hann, Marshall, and Hanus(2006) FRL Research Contribution ??
                case FiaCode.PseudotsugaMenziesii:
                    P1 = AvxExtensions.BroadcastScalarToVector128(0.655258886F);
                    P2 = AvxExtensions.BroadcastScalarToVector128(-0.006322913F);
                    P3 = AvxExtensions.BroadcastScalarToVector128(-0.039409636F);
                    minusP5 = AvxExtensions.BroadcastScalarToVector128(-0.597617316F);
                    P7 = AvxExtensions.BroadcastScalarToVector128(0.631643636F);
                    P8 = AvxExtensions.BroadcastScalarToVector128(1.010018427F);
                    break;
                // Ritchie and Hann(1990) FRL Research Paper 54
                case FiaCode.AbiesGrandis:
                    P2 = AvxExtensions.BroadcastScalarToVector128(-0.0328142F);
                    P3 = AvxExtensions.BroadcastScalarToVector128(-0.0127851F);
                    // P4 = 1.0F;
                    minusP5 = AvxExtensions.BroadcastScalarToVector128(-6.19784F);
                    P8 = AvxExtensions.BroadcastScalarToVector128(1.01F);
                    break;
                // Johnson(2002) Willamette Industries Report
                case FiaCode.TsugaHeterophylla:
                    P2 = AvxExtensions.BroadcastScalarToVector128(-0.0384415F);
                    P3 = AvxExtensions.BroadcastScalarToVector128(-0.0144139F);
                    minusP5 = AvxExtensions.BroadcastScalarToVector128(-1.04409F);
                    P8 = AvxExtensions.BroadcastScalarToVector128(1.03F);
                    break;
                default:
                    throw Trees.CreateUnhandledSpeciesException(trees.Species);
            }

            Vector128<int> oldTreeRecordCount = Vector128<int>.Zero;
            fixed (float* crownRatios = &trees.CrownRatio[0], expansionFactors = &trees.LiveExpansionFactor[0], heights = &trees.Height[0], heightGrowths = &trees.HeightGrowth[0])
            {
                Vector128<float> one = AvxExtensions.BroadcastScalarToVector128(1.0F);
                DouglasFir.SiteConstants psmeSite = trees.Species == FiaCode.TsugaHeterophylla ? null : new DouglasFir.SiteConstants(stand.SiteIndex); // also used for grand fir
                WesternHemlock.SiteConstants tsheSite = trees.Species == FiaCode.TsugaHeterophylla ? new WesternHemlock.SiteConstants(stand.HemlockSiteIndex) : null;
                Vector128<float> zero = Vector128<float>.Zero;

                for (int treeIndex = 0; treeIndex < trees.Count; treeIndex += 4)
                {
                    // inline version of GetGrowthEffectiveAge()
                    Vector128<float> height = Avx.LoadVector128(heights + treeIndex);
                    Vector128<float> growthEffectiveAge;
                    Vector128<float> potentialHeightGrowth;
                    if (trees.Species == FiaCode.TsugaHeterophylla)
                    {
                        growthEffectiveAge = WesternHemlock.GetFlewellingGrowthEffectiveAge(tsheSite, this.TimeStepInYears, height, out potentialHeightGrowth);
                    }
                    else
                    {
                        growthEffectiveAge = DouglasFir.GetBrucePsmeAbgrGrowthEffectiveAge(psmeSite, this.TimeStepInYears, height, out potentialHeightGrowth);
                    }
                    Vector128<float> crownCompetitionFactor = this.GetCrownCompetitionFactorByHeight(height, crownCompetitionByHeight);
                    Vector128<float> sqrtCrownCompetitionFactor = Avx.Sqrt(crownCompetitionFactor);
                    Vector128<float> crownCompetitionIncrementToP4 = sqrtCrownCompetitionFactor;
                    if (trees.Species == FiaCode.AbiesGrandis)
                    {
                        crownCompetitionIncrementToP4 = crownCompetitionFactor;
                    }

                    Vector128<float> crownRatio = Avx.LoadVector128(crownRatios + treeIndex);
                    Vector128<float> proportionBelowCrown = Avx.Subtract(one, crownRatio);
                    Vector128<float> B0 = Avx.Multiply(P1, MathV.Exp(Avx.Multiply(P2, crownCompetitionFactor)));
                    Vector128<float> B1 = MathV.Exp(Avx.Multiply(P3, crownCompetitionIncrementToP4)); // exp(P3 * sqrt(CCI)) for PSME and THSE, exp(P3 * CCI) for ABGR
                    Vector128<float> FCR = Avx.Multiply(Avx.Multiply(minusP5, Avx.Multiply(proportionBelowCrown, proportionBelowCrown)), MathV.Exp(Avx.Multiply(P7, sqrtCrownCompetitionFactor))); // P7 is 0.0 for ABGR and TSHE -> exp() = 1.0
                    Vector128<float> modifier = Avx.Multiply(P8, Avx.Add(B0, Avx.Multiply(Avx.Subtract(B1, B0), MathV.MaskExp(FCR, 0))));
                    Vector128<float> crownRatioAdjustment = OrganonGrowth.GetCrownRatioAdjustment(crownRatio);
                    Vector128<float> heightGrowth = Avx.Multiply(potentialHeightGrowth, Avx.Multiply(modifier, crownRatioAdjustment));

                    Vector128<float> expansionFactor = Avx.LoadVector128(expansionFactors + treeIndex); // maybe worth continuing in loop if all expansion factors are zero?
                    heightGrowth = Avx.BlendVariable(heightGrowth, zero, Avx.CompareLessThanOrEqual(expansionFactor, zero));
                    DebugV.Assert(Avx.CompareGreaterThanOrEqual(heightGrowth, zero));
                    Avx.Store(heightGrowths + treeIndex, heightGrowth);

                    // if growth effective age > old tree age is true, then 0xffff ffff = -1 is returned from the comparison
                    // Reinterpreting as Vector128<int> and subtracting therefore adds one to the old tree record counts where old trees occur.
                    oldTreeRecordCount = Avx.Subtract(oldTreeRecordCount, Avx.CompareGreaterThan(growthEffectiveAge, AvxExtensions.BroadcastScalarToVector128(configuration.Variant.OldTreeAgeThreshold)).AsInt32());
                }
            }

            oldTreeRecordCount = Avx.HorizontalAdd(oldTreeRecordCount, oldTreeRecordCount);
            oldTreeRecordCount = Avx.HorizontalAdd(oldTreeRecordCount, oldTreeRecordCount);
            return oldTreeRecordCount.ToScalar();
        }

        public override void ReduceExpansionFactors(OrganonStand stand, OrganonStandDensity densityBeforeGrowth, Trees trees, float fertilizationExponent)
        {
            float B0;
            float B1;
            float B2 = 0.0F;
            float B3;
            float B4;
            float B5;
            float siteIndex = stand.SiteIndex;
            switch (trees.Species)
            {
                // DF Coefficients from Unpublished Equation on File at OSU Dept.Forest Resources
                case FiaCode.PseudotsugaMenziesii:
                    B0 = -4.13142F;
                    B1 = -1.13736F;
                    B3 = -0.823305F;
                    B4 = 0.0307749F;
                    B5 = 0.00991005F;
                    break;
                // Unpublished Equation on File at OSU Dept. Forest Resources
                case FiaCode.AbiesGrandis:
                    B0 = -7.60159F;
                    B1 = -0.200523F;
                    B3 = 0.0F;
                    B4 = 0.0441333F;
                    B5 = 0.00063849F;
                    break;
                // Hann, Marshall, Hanus (2003) FRL Research Contribution 40
                case FiaCode.TsugaHeterophylla:
                case FiaCode.ThujaPlicata:
                    B0 = -0.761609F;
                    B1 = -0.529366F;
                    B3 = -4.74019F;
                    B4 = 0.0119587F;
                    B5 = 0.00756365F;
                    siteIndex = stand.HemlockSiteIndex;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.TaxusBrevifolia:
                    B0 = -4.072781265F;
                    B1 = -0.176433475F;
                    B3 = -1.729453975F;
                    B4 = 0.0F;
                    B5 = 0.012525642F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.ArbutusMenziesii:
                    B0 = -6.089598985F;
                    B1 = -0.245615070F;
                    B3 = -3.208265570F;
                    B4 = 0.033348079F;
                    B5 = 0.013571319F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.AcerMacrophyllum:
                    B0 = -2.976822456F;
                    B1 = 0.0F;
                    B3 = -6.223250962F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    break;
                // Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23: 26-33
                case FiaCode.QuercusGarryana:
                    B0 = -6.00031085F;
                    B1 = -0.10490823F;
                    B3 = -0.99541909F;
                    B4 = 0.00912739F;
                    B5 = 0.87115652F;
                    break;
                // Best Guess
                case FiaCode.AlnusRubra:
                    B0 = -2.0F;
                    B1 = -0.5F;
                    B2 = 0.015F;
                    B3 = -3.0F;
                    B4 = 0.015F;
                    B5 = 0.01F;
                    break;
                // Hann and Hanus(2001) FRL Research Contribution 34
                case FiaCode.CornusNuttallii:
                    B0 = -3.020345211F;
                    B1 = 0.0F;
                    B3 = -8.467882343F;
                    B4 = 0.013966388F;
                    B5 = 0.009461545F;
                    break;
                // Best Guess
                case FiaCode.Salix:
                    B0 = -1.386294361F;
                    B1 = 0.0F;
                    B3 = 0.0F;
                    B4 = 0.0F;
                    B5 = 0.0F;
                    break;
                default:
                    throw Trees.CreateUnhandledSpeciesException(trees.Species);
            }
            
            float[] mortalityKforRedAlder = null;
            if (trees.Species == FiaCode.AlnusRubra)
            {
                mortalityKforRedAlder = new float[trees.Capacity];
            }

            for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
            {
                if (trees.LiveExpansionFactor[treeIndex] <= 0.0F)
                {
                    continue;
                }

                float dbhInInches = trees.Dbh[treeIndex];
                if (trees.Species == FiaCode.PseudotsugaMenziesii)
                {
                    dbhInInches = MathF.Sqrt(dbhInInches);
                }
                float basalAreaLarger = densityBeforeGrowth.GetBasalAreaLarger(dbhInInches);
                if (trees.Species == FiaCode.AbiesGrandis)
                {
                    basalAreaLarger /= dbhInInches;
                }
                else if (trees.Species == FiaCode.QuercusGarryana)
                {
                    basalAreaLarger = MathV.Ln(basalAreaLarger + 5.0F);
                }
                float crownRatio = trees.CrownRatio[treeIndex];
                if (trees.Species == FiaCode.PseudotsugaMenziesii)
                {
                    // double square root is considerably faster than MathV.Pow(crownRatio, 0.25F)
                    // Overall stand simulation speedup of +4% at time of testing on i7-3770.
                    crownRatio = MathF.Sqrt(MathF.Sqrt(crownRatio));
                }
                float PMK = B0 + B1 * dbhInInches + B2 * dbhInInches * dbhInInches + B3 * crownRatio + B4 * siteIndex + B5 * basalAreaLarger + fertilizationExponent;
                if (trees.Species == FiaCode.AlnusRubra)
                {
                    mortalityKforRedAlder[treeIndex] = PMK;
                }

                float survivalProbability = 1.0F - 1.0F / (1.0F + MathV.Exp(-PMK));
                survivalProbability *= OrganonGrowth.GetCrownRatioAdjustment(crownRatio);
                Debug.Assert(survivalProbability >= 0.0F);
                Debug.Assert(survivalProbability <= 1.0F);

                float newLiveExpansionFactor = survivalProbability * trees.LiveExpansionFactor[treeIndex];
                if (newLiveExpansionFactor < 0.00001F)
                {
                    newLiveExpansionFactor = 0.0F;
                }
                float mortalityExpansionFactor = trees.LiveExpansionFactor[treeIndex] - newLiveExpansionFactor;

                trees.DeadExpansionFactor[treeIndex] = mortalityExpansionFactor;
                trees.LiveExpansionFactor[treeIndex] = newLiveExpansionFactor;
            }

            if ((trees.Species == FiaCode.AlnusRubra) && (this.TreeModel != TreeModel.OrganonRap))
            {
                if (stand.RedAlderGrowthEffectiveAge >= Constant.RedAlderAdditionalMortalityGrowthEffectiveAgeInYears)
                {
                    Debug.Assert(trees.Units == Units.English);
                    float alnusRubraTreesPerAcre = 0.0F;
                    for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
                    {
                        alnusRubraTreesPerAcre += trees.LiveExpansionFactor[treeIndex];
                    }
                    if (alnusRubraTreesPerAcre > 0.0001F)
                    {
                        RedAlder.ReduceExpansionFactor(trees, stand.RedAlderGrowthEffectiveAge, alnusRubraTreesPerAcre, mortalityKforRedAlder);
                    }
                }
            }
        }
    }
}
