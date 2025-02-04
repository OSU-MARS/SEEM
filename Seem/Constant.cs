﻿using Mars.Seem.Heuristics;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mars.Seem
{
    internal static class Constant
    {
        public const float AcresPerHectare = 2.47105F;
        public const int AllFinancialScenariosPosition = -2;
        public const int AllRotationPosition = -2;
        public const float CentimetersPerInch = 2.54000F;
        // 100 * pi / (4 * 43560), from definition of crown competition factor
        public const float CrownCompetionConstantEnglish = 0.001803026F;
        public const float CubicFeetPerCubicMeter = 35.3147F;
        public const float CubicMetersPerCubicFoot = 0.0283168F;
        public const float DbhHeightInM = 1.37F; // cm
        public const float ExpToZeroPower = -16.2F; // truncate thinning effects of less than 1E-7 to zero
        public const float FeetPerMeter = 3.28084F;
        public const float ForestersEnglish = 0.005454154F; // pi/4 * 1/12²: DBH in inches to ft² basal area
        public const float ForestersMetric = 0.00007853981F; // pi/4 * 1/100²: DBH in cm to m² basal area
        public const float HectaresPerAcre = 0.404685F;
        public const float InchesPerCentimeter = 0.393701F;
        public const int MaximizeForAllPlanningPeriods = -2;
        public const float MetersPerFoot = 0.3048F;
        public const float MinutesPerHour = 60.0F;
        public const float NaturalLogOf10 = 2.3025850930F;
        public const Int16 NoDataInt16 = Int16.MinValue;
        public const int NoHarvestPeriod = -1;
        // number of height strata must currently be an exact multiple of SIMD width: multiples of 4 for 128 bit, 8 for 256 bit, 16 for 512 bit
        public const int OrganonHeightStrata = 40; // supports 128 and 256 bit, use 48 for 512 bit SIMD
        public const float PolymorphicLocusThreshold = 0.95F;
        public const float RedAlderAdditionalMortalityGrowthEffectiveAgeInYears = 55.0F;
        public const float ReinekeExponent = 1.605F;
        public const int RegenerationHarvestIfEligible = 0;
        // 0.00003 and smaller result in expected ArgumentOutOfRangeExceptions due to single precision
        // However, 0.0001 still results in rare exceptions. The underlying cause is unclear.
        public const float RoundTowardsZeroTolerance = 0.001F;
        public const float SecondsPerHour = 3600.0F;
        public const float SquareMetersPerSquareFoot = 0.09290304F;
        public const float SquareFeetPerSquareMeter = 10.7639104167097F;
        public const float SquareMetersPerHectare = 10000.0F;

        public static readonly ReadOnlyCollection<FiaCode> NwoSmcSpecies = new([
            FiaCode.PseudotsugaMenziesii,
            FiaCode.AbiesGrandis,
            FiaCode.TsugaHeterophylla,
            FiaCode.ThujaPlicata,
            FiaCode.TaxusBrevifolia,
            FiaCode.ArbutusMenziesii,
            FiaCode.AcerMacrophyllum,
            FiaCode.QuercusGarryana,
            FiaCode.AlnusRubra,
            FiaCode.CornusNuttallii,
            FiaCode.Salix
        ]);
        public static readonly ReadOnlyCollection<FiaCode> RapSpecies = new([
            FiaCode.AlnusRubra,
            FiaCode.PseudotsugaMenziesii,
            FiaCode.TsugaHeterophylla,
            FiaCode.ThujaPlicata,
            FiaCode.AcerMacrophyllum,
            FiaCode.CornusNuttallii,
            FiaCode.Salix
        ]);
        public static readonly ReadOnlyCollection<FiaCode> SwoSpecies = new([
            FiaCode.PseudotsugaMenziesii,
            FiaCode.AbiesConcolor,
            FiaCode.AbiesGrandis,
            FiaCode.PinusPonderosa,
            FiaCode.PinusLambertiana,
            FiaCode.CalocedrusDecurrens,
            FiaCode.TsugaHeterophylla,
            FiaCode.ThujaPlicata,
            FiaCode.TaxusBrevifolia,
            FiaCode.ArbutusMenziesii,
            FiaCode.ChrysolepisChrysophyllaVarChrysophylla,
            FiaCode.NotholithocarpusDensiflorus,
            FiaCode.QuercusChrysolepis,
            FiaCode.AcerMacrophyllum,
            FiaCode.QuercusGarryana,
            FiaCode.QuercusKelloggii,
            FiaCode.AlnusRubra,
            FiaCode.CornusNuttallii,
            FiaCode.Salix
        ]);

        public static class Avx512
        {
            public const int RoundToZero = 3; // _MM_FROUND_TO_ZERO
        }

        public static class Bucking
        {
            public const float BarSawKerf = 0.007F; // m
            public const float BCFirmwoodLogTaperSegmentLengthInM = Constant.MetersPerFoot * 8.0F; // m
            public const float DefaultForwarderLogLengthInM = Constant.MetersPerFoot * 24.0F; // m
            public const float DefaultLongLogLengthInM = Constant.MetersPerFoot * 40.0F; // m
            public const float DefaultStumpHeightInM = 0.15F; // m
            public const float DefectAndBreakageReduction = 0.955F; // 100 - 4.5%
            public const float EvaluationHeightStepInM = 0.5F; // m
            public const float ProcessingHeadFeedRollerHeightInM = 0.70F; // m
            public const float ScribnerShortLogLengthInM = Constant.MetersPerFoot * 20.0F; // m
            public const float ScribnerTrimLongLogInM = Constant.MetersPerFoot * 1.0F - 0.0001F; // m with 100 μm margin for numerical precision
            public const float ScribnerTrimShortLogInM = Constant.MetersPerFoot * 0.5F - 0.0001F; // m with 100 μm margin for numerical precision
            public const float VolumeTableDiameterClassSizeInCentimeters = 1.0F;
            public const float VolumeTableHeightClassSizeInMeters = 1.0F; // m
            public const float VolumeTableMaximumDiameterToLogInCentimeters = 250.0F;
        }

        public static class Default
        {
            public const string CubicVolumeFormat = "0.0#";
            public const string CrownRatioFormat = "0.0##";
            public const string DiameterInCmFormat = "0.0#";
            public const float DouglasFirSiteIndexInM = 39.6F; // from Malcolm Knapp Research Forest spacing trials, Douglas-fir site class 1 (~130 feet)
            public const string ExpansionFactorFormat = "0.0##";
            public const string FileSizeLimitFormat = "0.00";
            public const int FileWriteBufferSizeInBytes = 128 * 1024; // .NET FileWriter() defaults to 4 kB
            public const string HeightInMFormat = "0.0#";
            public const string LogVolumeFormat = "0.0##";
            public const string PercentageFormat = "0.0#";
            public const int PlotID = 0;
            public const string ProbabilityFormat = "0.00##";
            public const int RotationLengths = 9;
            public const float SlopeForTetheringInPercent = 45.0F;
            public const int SolutionPoolSize = 4;
            public const int ThinningPeriod = 3;
            public const float ThinningPondValueMultiplier = 0.90F;
            public const float WesternHemlockSiteIndexInM = 29.1F; // from Malcolm Knapp Research Forest spacing trial plot 21
        }

        public static class FileExtension
        {
            public const string Csv = ".csv";
            public const string Feather = ".feather";
        }

        public static class Financial
        {
            public const float DefaultAnnualDiscountRate = 0.04F;
        }

        public static class GeneticDefault
        {
            public const float CrossoverProbabilityEnd = 0.5F;
            public const float ExchangeProbabilityEnd = 0.1F;
            public const float ExchangeProbabilityStart = 0.0F;
            public const float ExponentK = -8.0F;
            public const float FlipProbabilityEnd = 0.9F; // ~0.85 best for constant probability
            public const float FlipProbabilityStart = 0.0F;
            public const float GenerationMultiplier = 5.5F;
            public const float GenerationPower = 0.6F;
            public const int InitializationClasses = 1;
            public const PopulationInitializationMethod InitializationMethod = PopulationInitializationMethod.DiameterClass;
            public const float MinimumCoefficientOfVariation = 0.000001F;
            public const int PopulationSize = 30;
            public const PopulationReplacementStrategy ReplacementStrategy = PopulationReplacementStrategy.ContributionOfDiversityReplaceWorst;
            public const float ReservedPopulationProportion = 1.0F;
        }

        public static class Grasp
        {
            public const float DefaultMinimumConstructionGreedinessForMaximization = 0.9F;
            public const float FullyGreedyConstructionForMaximization = 1.0F;
            public const float FullyRandomConstructionForMaximization = 0.0F;
            public const int MinimumTreesRandomized = 5; // see Heuristic.ConstructTreeSelection()
            public const float NoConstruction = -1.0F;
        }

        public static class HarvestCost
        {
            // catch all for staffing, road maintenance, and other costs
            // For fire protection, the Oregon Department of Forestry's Forest Patrol Assessment varies by county. These vary by
            // biennium and are typically reported by a county's assessor or fire protection association. Charges include acreage
            // and structure protection.
            public const float AdmininistrationCost = 14.82F; // US$/ha-year
            // US$/ha-year, default is average of northwestern Oregon counties adjusted up to site class 1
            // Oregon Department of Revenue sets SAV (specially assessed value) and maximum SAV (MSAV) for counties under ORS 321.216.
            //  https://www.oregon.gov/dor/programs/property/Documents/specially-assessed-forestland-values.pdf
            //  https://www.oregon.gov/dor/programs/property/Documents/2022-23%20Cert%20OR%20Forestland%20Values-EastWest%20Counties.pdf
            public const float AssessedValue = 1.26F * 1128.57F;
            public const float BrushControl = 45.0F; // US$/ha
            public const float ChainsawBasalAreaPerHaForFullUtilization = 30.0F; // m²/ha
            public const float DefaultAccessDistanceInM = 0.0F; // m, assume stand is adjacent to or encompasses at least one road
            public const float DefaultAccessSlopeInPercent = 0.0F;
            public const float DefaultForwardingDistanceInStandTethered = 310.0F; // m
            public const float DefaultForwardingDistanceInStandUntethered = 10.0F; // m
            public const float DefaultForwardingDistanceOnRoad = 30.0F; // m
            public const float DefaultHarvestUnitSizeInHa = 15.0F;
            public const float DefaultSlopeInPercent = 65.0F;
            // fraction of assessed value = 0.01 * percent of assessed value, default is average of northwestern Oregon counties
            //  https://www.oregon.gov/dor/programs/gov-research/Documents/publication-or-pts_303-405_2019-20.pdf, Table 1.8
            public const float ForestlandPropertyTaxRate = 0.01F * 1.61F;
            public const float ForwardingDistanceOnRoadPerSortInM = 10.0F; // m
            public const float MeanYardingDistanceFactor = 0.5F; // fraction of corridor length, 0.5 = parallel yearding
            public const float OregonForestProductsHarvestTax = 4.1322F; // US$/MBF in 2020 and 2021, https://www.oregon.gov/dor/programs/property/Pages/timber-forest-harvest.aspx
            public const float PlantingLabor = 383.0F; // US$/ha
            public const float ReleaseSpray = 100.0F + 175.0F; // US$/ha, labor + herbicide cost
            public const float RoadMaintenance = 0.10F * 15.0F; // US$/merchantable m³-km * 15 km of access road
            public const float RoadReopening = 25.0F; // US$/ha
            public const float SitePrep = 145.0F + 200.0F; // US$/ha, labor + herbicide cost
            public const float SlashDisposal = 0.35F; // US$/merchantable m³
            public const float TimberCruisePerHectare = 65.0F; // US$/ha
            public const float TimberSaleAdministrationPerHectare = 32.0F; // US$/ha
            public const float YarderLandingSlashDisposal = 0.12F; // US$/merchantable m³
        }

        public static class HeuristicDefault
        {
            public const int CoordinateIndex = 0;
            public const int FirstCircularIterationMultiplier = 20;
            public const int HeroMaximumIterations = 20;
            public const float InitialThinningProbability = 0.5F;
            public const bool LogOnlyImprovingMoves = false;
            public const int MoveCapacity = 1024 * 1024;
        }

        public static class MalcolmKnapp
        {
            public static class TreeCondition
            {
                public const int Harvested = 1;
                public const int Dead = 2;
            }
        }

        public static class Math
        {
            public const float Exp10ToExp2 = 3.321928094887362F; // log2(10)
            public const float ExpToExp2 = 1.442695040888963F; // log2(e) for converting base 2 IEEE 754 exponent manipulation to base e

            public const float ExpC1 = 0.007972914726F;
            public const float ExpC2 = 0.1385283768F;
            public const float ExpC3 = 2.885390043F;

            public const float FloatExp2MaximumPower = 127.0F; // range limiting decompositions using 8 bit signed exponent
            public const int FloatExponentMask = 0x7F800000;
            public const int FloatMantissaBits = 23;
            public const int FloatMantissaZero = 127;
            public const int FloatMantissaMask = 0x007FFFFF;

            public const float Log2Beta1 = 1.441814292091611F;
            public const float Log2Beta2 = -0.708440969761796F;
            public const float Log2Beta3 = 0.414281442395441F;
            public const float Log2Beta4 = -0.192544768195605F;
            public const float Log2Beta5 = 0.044890234549254F;
            public const float Log2ToNaturalLog = 0.693147180559945F;
            public const float Log2ToLog10 = 0.301029995663981F;

            public const float One = 1.0F;
            public const int OneAsInt = 0x3f800000; // 0x3f800000 = 1.0F
        }

        public static class Maximum
        {
            public const float AgeInYears = 1000.0F;
            public const float DiameterIncrementInInches = 4.5F;
            public const float ExpansionFactorPerAcre = 1000.0F; // for 1/1000 ac seedling plots
            public const float ExpansionFactorPerHa = 2500.0F;
            public const float HeightIncrementInFeet = 21.0F;
            public const float PlantingDensityInTreesPerHectare = 40000.0F; // sanity upper bound, 0.5 m spacing
            //public const float SdiPerAcre = 1000.0F; // Reineke SDI in English units
            public const float SiteIndexInFeet = 200.0F; // sanity upper bound
            public const float SiteIndexInM = 61.0F; // sanity upper bound
            public const float TetheredCorridorLengthInM = 590.0F; // Summit Attachments & Machinery winch assist, 25.4 mm cable
        }

        public static class Minimum
        {
            public const float SiteIndexInFeet = 4.5F;
        }

        public static class MonteCarloDefault
        {
            public const float AnnealingAlpha = 0.7F;
            public const int AnnealingAveragingWindowLength = 10;
            public const int AnnealingIterationsPerTemperature = 10;
            public const float AnnealingReheadBy = 0.33F;
            public const float DelugeFinalMultiplier = 1.75F;
            public const float DelugeInitialMultiplier = 1.25F;
            public const float DelugeLowerWaterBy = 0.0033F;
            public const int IterationMultiplier = 19;
            public const float RecordTravelAlpha = 0.75F;
            public const float RecordTravelRelativeIncrease = 0.0075F;
            public const float ReheatAfter = 1.6F;
            public const int StopAfter = 19;
        }

        public static class PrescriptionSearchDefault
        {
            public const float DefaultIntensityStepSize = 8.0F; // loose optimum for coordinate ascent solution quality given minimum step of 1%
            public const int LogLastNImprovingMoves = 25;
            public const float InitialThinningProbability = 0.0F;
            public const float MethodPercentageUpperLimit = 100.0F;
            public const float MaximumIntensity = 80.0F;
            public const float MaximumIntensityStepSize = 100.0F; // default to no limiting by percentage
            public const float MinimumIntensity = 20.0F;
            public const float MinimumIntensityStepSize = 1.0F;
            public const float StepSizeMultiplier = 0.5F;
            public const PrescriptionUnits Units = PrescriptionUnits.StemPercentageRemoved;
        }

        public static class OpenXml
        {
            public const string Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

            public static class Attribute
            {
                public const string CellReference = "r";
                public const string CellType = "t";
                public const string Reference = "ref";
            }

            public static class CellType
            {
                public const string InlineString = "inlineStr";
                public const string SharedString = "s";
            }

            public static class Element
            {
                public const string Cell = "c";
                public const string CellValue = "v";
                public const string Dimension = "dimension";
                public const string InlineString = "is";
                public const string Row = "row";
                public const string SharedString = "si";
                public const string SheetData = "sheetData";
                public const string String = "t";
            }
        }

        public static class Psp
        {
            public const int DefaultNumberOfStandMeasurements = 8;

            public static class ColumnIndex
            {
                public const int Dbh = 11;
                public const int Plot = 5;
                public const int Species = 7;
                public const int Status = 10;
                public const int Tag = 8;
                public const int Year = 9;
            }

            public static class TreeStatus
            {
                public const int Dead = 6;
                public const int Fused = 3;
                public const int Ingrowth = 2;
                public const int Live = 1;
                public const int NotFound = 9;
            }
        }

        public static class Simd128x4
        {
            public const int MaskAllFalse = 0x0;
            public const int MaskAllTrue = 0xf;
            public const byte Broadcast0toAll = 0; // 0 << 6  | 0 << 4 | 0 << 2 | 0
            public const int ShuffleRotateLower1 = 0x39; // 0 << 6 | 3 << 4 | 2 << 2 | 1
            public const int ShuffleRotateLower2 = 0x4e; // 1 << 6 | 0 << 4 | 3 << 2 | 2
            public const int ShuffleRotateLower3 = 0x93; // 2 << 6 | 1 << 4 | 0 << 2 | 3
            public const int Width = 4;
        }

        public static class Simd256x8
        {
            public const byte InsertUpper128 = 1;
            public const byte ExtractLower128 = 0;
            public const byte ExtractUpper128 = 1;
            public const byte Mantissa12Sign = 0x00; // _MM_MANT_NORM_1_2 << ? | _MM_MANT_SIGN_src << ?, https://github.com/dotnet/dotnet-api-docs/issues/9520
            public const int MaskAllFalse = 0x00;
            public const int MaskAllTrue = 0xff;
            public const int Width = 8;
        }

        public static class Simd512x16
        {
            public const byte ExtractLower256 = 0;
            public const byte ExtractUpper256 = 1;
            public const int MaskAllFalse = 0x0000;
            public const int MaskAllTrue = 0xffff;
            public const int Width = 16;
        }

        public static class TabuDefault
        {
            public const float EscapeAfter = 1000.0F * 1000.0F; // off by default, nominal on value: 0.06F
            public const float EscapeBy = 0.04F;
            public const float IterationMultiplier = 4.25F;
            public const float MaximumTenureRatio = 0.1F;
            public const TabuTenure Tenure = TabuTenure.Stochastic;
        }
    }
}
