using Mars.Seem.Extensions;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mars.Seem.Silviculture
{
    public class SnagDownLogTable : LogTable
    {
        public float[] SnagQmdInCentimetersByPeriod { get; private init; } // average across all species
        public float[] SnagsPerHectareByPeriod { get; private init; } // total across all species
        public SortedList<FiaCode, float[,]> SnagsPerHectareBySpeciesAndDiameterClass { get; private init; } // diameter class in cm

        public SnagDownLogTable(StandTrajectory standTrajectory, float maximumDiameterInCm, float diameterClassSizeInCm)
            : base(standTrajectory.PlanningPeriods, maximumDiameterInCm, diameterClassSizeInCm)
        {
            Debug.Assert(standTrajectory.PeriodLengthInYears > 0);

            this.SnagsPerHectareBySpeciesAndDiameterClass = [];
            this.SnagQmdInCentimetersByPeriod = new float[this.Periods];
            this.SnagsPerHectareByPeriod = new float[this.Periods];

            // accumulate snags newly created in each period
            // TODO: initialize snag and log pools with trees which died prior to simulation
            for (int period = 0; period < standTrajectory.StandByPeriod.Length; ++period)
            {
                Stand standForPeriod = standTrajectory.StandByPeriod[period] ?? throw new ArgumentOutOfRangeException(nameof(standTrajectory));
                float decayTimeInYears = 0.5F * standTrajectory.PeriodLengthInYears;
                foreach (Trees treesOfSpecies in standForPeriod.TreesBySpecies.Values)
                {
                    float[,] logsPerHectare;
                    if (this.SnagsPerHectareBySpeciesAndDiameterClass.TryGetValue(treesOfSpecies.Species, out float[,]? snagsPerHectare) == false)
                    {
                        logsPerHectare = new float[standTrajectory.PlanningPeriods, this.DiameterClasses];
                        this.LogsPerHectareBySpeciesAndDiameterClass.Add(treesOfSpecies.Species, logsPerHectare);

                        snagsPerHectare = new float[standTrajectory.PlanningPeriods, this.DiameterClasses];
                        this.SnagsPerHectareBySpeciesAndDiameterClass.Add(treesOfSpecies.Species, snagsPerHectare);
                    }
                    else
                    {
                        logsPerHectare = this.LogsPerHectareBySpeciesAndDiameterClass[treesOfSpecies.Species];
                    }

                    SnagDownLogTable.GetSnagfallCofficients(treesOfSpecies.Species, out float decayDivider0, out float decayDivider1, out float decayPower);
                    for (int compactedIndex = 0; compactedIndex < treesOfSpecies.Count; ++compactedIndex)
                    {
                        float dbhAtEndOfPeriodInCm = treesOfSpecies.Dbh[compactedIndex];
                        float dbhGrowthWithinPeriodInCm = treesOfSpecies.DbhGrowth[compactedIndex];
                        float sourceExpansionFactorPerHa = treesOfSpecies.DeadExpansionFactor[compactedIndex];
                        if (treesOfSpecies.Units == Units.English)
                        {
                            dbhAtEndOfPeriodInCm *= Constant.CentimetersPerInch;
                            dbhGrowthWithinPeriodInCm *= Constant.CentimetersPerInch;
                            sourceExpansionFactorPerHa *= Constant.AcresPerHectare;
                        }
                        Debug.Assert(sourceExpansionFactorPerHa >= 0.0F);

                        // assume expansion factor which died within this period didn't grow in diameter before mortality
                        // These assumptions are reasonable for suppression mortality in managed stands but are incorrect for broken tops and
                        // windthrow.
                        float diameterAtMortality = dbhAtEndOfPeriodInCm - dbhGrowthWithinPeriodInCm;
                        int diameterClassIndex = (int)((diameterAtMortality + 0.5 * diameterClassSizeInCm) / diameterClassSizeInCm);

                        float stillStandingMultiplier = MathV.Exp(-MathV.Pow(decayTimeInYears / (decayDivider0 + decayDivider1 * diameterAtMortality), decayPower));
                        Debug.Assert(stillStandingMultiplier >= 0.0F && stillStandingMultiplier <= 1.0F);

                        float unfallenExpansionFactor = stillStandingMultiplier * sourceExpansionFactorPerHa;
                        float fallenExpansionFactor = sourceExpansionFactorPerHa - unfallenExpansionFactor;

                        logsPerHectare[period, diameterClassIndex] += fallenExpansionFactor;
                        this.LogQmdInCentimetersByPeriod[period] += fallenExpansionFactor * diameterAtMortality * diameterAtMortality;
                        this.LogsPerHectareByPeriod[period] += fallenExpansionFactor;

                        snagsPerHectare[period, diameterClassIndex] += unfallenExpansionFactor;
                        this.SnagQmdInCentimetersByPeriod[period] += unfallenExpansionFactor * dbhAtEndOfPeriodInCm * dbhAtEndOfPeriodInCm;
                        this.SnagsPerHectareByPeriod[period] += unfallenExpansionFactor;
                    }
                }
            }

            // fall snags from previous periods
            // Parish R, Antos JA, Ot PK, Di Lucca CM. 2010. Snag longevity of Douglas-fir, western hemlock, and western redcedar from
            //   permanent sample plots in coastal British Columbia. Forest Ecology and Management 259:633–640.
            //   https://doi.org/10.1016/j.foreco.2009.11.022
            // Coefficients revised from Table 1 since 1) correct values aren't divided by the power and 2) Parish et al. expressed diameter in mm
            // rather than cm.
            foreach (KeyValuePair<FiaCode, float[,]> speciesAndSnags in this.SnagsPerHectareBySpeciesAndDiameterClass)
            {
                FiaCode species = speciesAndSnags.Key;
                SnagDownLogTable.GetSnagfallCofficients(species, out float decayDivider0, out float decayDivider1, out float decayPower);
                float[,] logsPerHectare = this.LogsPerHectareBySpeciesAndDiameterClass[species];
                float[,] snagsPerHectare = speciesAndSnags.Value;
                for (int period = standTrajectory.PlanningPeriods - 1; period >= 0; --period)
                {
                    // sum unfallen expansion factors from all previous periods to obtain total snags
                    // sum fallen expansion factors to obtain total log recruitment
                    for (int previousPeriod = 0; previousPeriod < period; ++previousPeriod)
                    {
                        // assume trees die in the middle of a simulation period
                        // Therefore, decay time is the difference between the current and previous period plus half a period.
                        float decayTimeInYears = standTrajectory.PeriodLengthInYears * (period - previousPeriod + 0.5F);
                        for (int diameterClassIndex = 0; diameterClassIndex < this.DiameterClasses; ++diameterClassIndex)
                        {
                            float diameterInCm = this.GetDiameter(diameterClassIndex);
                            float stillStandingMultiplier = MathV.Exp(-MathV.Pow(decayTimeInYears / (decayDivider0 + decayDivider1 * diameterInCm), decayPower));
                            Debug.Assert(stillStandingMultiplier >= 0.0F && stillStandingMultiplier <= 1.0F);

                            float sourceExpansionFactor = snagsPerHectare[previousPeriod, diameterClassIndex];
                            float unfallenExpansionFactor = stillStandingMultiplier * sourceExpansionFactor;
                            float fallenExpansionFactor = sourceExpansionFactor - unfallenExpansionFactor;

                            logsPerHectare[period, diameterClassIndex] += fallenExpansionFactor;
                            snagsPerHectare[period, diameterClassIndex] += unfallenExpansionFactor;

                            // TODO: log decay
                            this.LogQmdInCentimetersByPeriod[period] += fallenExpansionFactor * diameterInCm * diameterInCm;
                            this.LogsPerHectareByPeriod[period] += fallenExpansionFactor;

                            this.SnagQmdInCentimetersByPeriod[period] += unfallenExpansionFactor * diameterInCm * diameterInCm;
                            this.SnagsPerHectareByPeriod[period] += unfallenExpansionFactor;
                        }
                    }

                    // find QMD now that all accumulation has completed for this period
                    Debug.Assert(this.LogsPerHectareByPeriod[period] >= 0.0F);
                    if (this.LogsPerHectareByPeriod[period] > 0.0F)
                    {
                        this.LogQmdInCentimetersByPeriod[period] = MathF.Sqrt(this.LogQmdInCentimetersByPeriod[period] / this.LogsPerHectareByPeriod[period]);
                    }
                    else
                    {
                        Debug.Assert(this.LogQmdInCentimetersByPeriod[period] == 0.0F);
                    }

                    Debug.Assert(this.SnagsPerHectareByPeriod[period] >= 0.0F);
                    if (this.SnagsPerHectareByPeriod[period] > 0.0F)
                    {
                        this.SnagQmdInCentimetersByPeriod[period] = MathF.Sqrt(this.SnagQmdInCentimetersByPeriod[period] / this.SnagsPerHectareByPeriod[period]);
                    }
                    else
                    {
                        Debug.Assert(this.SnagQmdInCentimetersByPeriod[period] == 0.0F);
                    }
                }
            }
        }

        private static void GetSnagfallCofficients(FiaCode species, out float decayDivider0, out float decayDivider1, out float decayPower)
        {
            switch (species)
            {
                case FiaCode.PseudotsugaMenziesii:
                    decayDivider0 = 1.7256F * 4.336F;
                    decayDivider1 = 1.7256F * 0.068F * 10.0F;
                    decayPower = 1.7256F;
                    break;
                case FiaCode.ThujaPlicata:
                    decayDivider0 = 1.777F * 4.485F;
                    decayDivider1 = 1.777F * 0.023F * 10.0F;
                    decayPower = 1.777F;
                    break;
                case FiaCode.TsugaHeterophylla:
                    decayDivider0 = 1.152F * 1.112F;
                    decayDivider1 = 1.152F * 0.066F * 10.0F;
                    decayPower = 1.152F;
                    break;
                default:
                    throw new NotSupportedException("Unhandled tree species " + species + ".");
            }
        }
    }
}
