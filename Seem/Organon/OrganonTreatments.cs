using Osu.Cof.Ferm.Extensions;
using Osu.Cof.Ferm.Silviculture;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Organon
{
    public class OrganonTreatments : Treatments
    {
        // TODO: avoid shadowing OrganonStandDensity.BasalArea with basalAreaByPeriod, requires replumbing Organon APIs with OrganonStandTrajectory
        private readonly List<float> basalAreaByPeriod;
        private int currentSimulationPeriod;

        // Basal area removed by thinning in ft²/acre in the years specified
        public List<float> BasalAreaThinnedByPeriod { get; private init; }

        // Nitrogen fertilization applied in lb/ac (Hann RC 40)
        public List<float> PoundsOfNitrogenPerAcreByPeriod { get; private init; }

        public OrganonTreatments()
        {
            this.basalAreaByPeriod = new();
            this.currentSimulationPeriod = 0;

            this.BasalAreaThinnedByPeriod = new() { 0.0F }; // no removal in pre-simulation period
            this.PoundsOfNitrogenPerAcreByPeriod = new();
        }

        private OrganonTreatments(OrganonTreatments other)
            : base(other)
        {
            this.basalAreaByPeriod = new(other.basalAreaByPeriod);
            this.currentSimulationPeriod = other.currentSimulationPeriod;

            this.BasalAreaThinnedByPeriod = new(other.BasalAreaThinnedByPeriod);
            this.PoundsOfNitrogenPerAcreByPeriod = new(other.PoundsOfNitrogenPerAcreByPeriod);
        }

        public bool ApplyToPeriod(int periodJustBeginning, OrganonStandTrajectory trajectory)
        {
            if (periodJustBeginning < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(periodJustBeginning));
            }

            if (this.basalAreaByPeriod.Count < periodJustBeginning)
            {
                this.basalAreaByPeriod.Add(0.0F);
            }
            if (this.BasalAreaThinnedByPeriod.Count <= periodJustBeginning)
            {
                this.BasalAreaThinnedByPeriod.Add(0.0F);
            }
            OrganonStandDensity? standDensity = trajectory.DensityByPeriod[periodJustBeginning - 1];
            Debug.Assert(standDensity != null);
            this.basalAreaByPeriod[periodJustBeginning - 1] = standDensity.BasalAreaPerAcre;
            this.BasalAreaThinnedByPeriod[periodJustBeginning] = 0.0F;

            this.currentSimulationPeriod = periodJustBeginning;

            int harvestsEvaluated = 0;
            foreach (Harvest harvest in this.Harvests)
            {
                if (harvest.Period == periodJustBeginning)
                {
                    if (harvestsEvaluated != 0)
                    {
                        // for now, assume only one harvest occurs in any one period
                        // If multiple harvests occur then the logic below .
                        throw new NotSupportedException("Multiple harvests found for period " + harvest.Period + ".");
                    }

                    float basalAreaRemovedByHarvest = harvest.EvaluateTreeSelection(trajectory);
                    Debug.Assert(basalAreaRemovedByHarvest >= 0.0F);

                    this.BasalAreaThinnedByPeriod[periodJustBeginning] = basalAreaRemovedByHarvest;
                    ++harvestsEvaluated;
                }
            }
            return harvestsEvaluated > 0;
        }

        public override Treatments Clone()
        {
            return new OrganonTreatments(this);
        }

        public void CopyFrom(OrganonTreatments other)
        {
            Debug.Assert(Object.ReferenceEquals(this, other) == false);
            base.CopyFrom(other);
        }

        public void CopyTreeGrowthFrom(OrganonTreatments other)
        {
            this.basalAreaByPeriod.CopyFrom(other.basalAreaByPeriod);
            this.BasalAreaThinnedByPeriod.CopyFrom(other.BasalAreaThinnedByPeriod);
            this.PoundsOfNitrogenPerAcreByPeriod.CopyFrom(other.PoundsOfNitrogenPerAcreByPeriod);
        }

        public float GetFertX1(OrganonVariant variant, float k, out float mostRecentFertilization, out int yearsSinceMostRecentFertilization)
        {
            if (this.PoundsOfNitrogenPerAcreByPeriod.Count < 1)
            {
                // no fertilization treatments
                mostRecentFertilization = 0.0F;
                yearsSinceMostRecentFertilization = -1;
                return 0.0F;
            }

            // Hann DW, Marshall DD, Hanus ML. 2003. Equations for predicting height-to-crown-base, 5-year diameter-growth rate, 5 year height
            //   growth rate, 5-year mortality rate, and maximum size-density trajectory for Douglas-fir and western hemlock in the coastal region
            //   of the Pacific Northwest. Research Contribution 40, Forest Research Laboratory, College of Forestry, Oregon State University.
            //   https://ir.library.oregonstate.edu/concern/technical_reports/jd472x893
            // diameter equation 11 (p35), height equation 15 (p48)
            //   eq 11: MFR_ΔD = a10 (pn1 / 800 + sum((pni / 800 exp(a12 (YFi - YF1)))^a11 exp(a13 YF1 + b2 ((SI - 4.5) / 100)^(2))
            //   eq 15: FR_Δh40 = 1.0 + pn1 / 800 + sum((pni / 800 exp(b1 (YFi - YF1)))^(1/3) exp(b1 YF1 + b2 ((SI - 4.5) / 100)^(3/2))
            // Here just fertX1 = sum((pni / 800 exp(k (YFi - YF1)) is calculated and YF1 returned.
            mostRecentFertilization = this.PoundsOfNitrogenPerAcreByPeriod[this.currentSimulationPeriod];
            int mostRecentFertilizationPeriod = mostRecentFertilization > 0.0F ? this.currentSimulationPeriod : Constant.NoThinPeriod;
            int mostRecentFertilizationYear = variant.TimeStepInYears * mostRecentFertilizationPeriod;

            float fertX1 = 0.0F;
            for (int previousPeriodIndex = this.currentSimulationPeriod - 1; previousPeriodIndex > 0; --previousPeriodIndex)
            {
                float fertilization = this.PoundsOfNitrogenPerAcreByPeriod[previousPeriodIndex];
                if (fertilization > 0.0F)
                {
                    if (mostRecentFertilizationPeriod == Constant.NoThinPeriod)
                    {
                        // most recent thinning: exp(k * (mostRecentThinYear - thinYear)) = exp(0)
                        Debug.Assert(fertX1 == 0.0F);
                        fertX1 = fertilization;
                        mostRecentFertilizationPeriod = previousPeriodIndex;
                        mostRecentFertilizationYear = variant.TimeStepInYears * mostRecentFertilizationPeriod;
                    }
                    else
                    {
                        // at least through Organon 2.2.4, a code typo results in the power being calculated as (thinYear - mostRecentThinYear)
                        // This creates a positive exponent, PREM values approaching 1, and therefore overestimation of thinning responses once
                        // a second thin is performed.
                        int fertilizationYear = variant.TimeStepInYears * previousPeriodIndex;
                        Debug.Assert(fertilizationYear < mostRecentFertilizationYear); // check for negative power to exponent
                        Debug.Assert(k > 0.0F);
                        fertX1 += fertilization / 800.0F * MathV.Exp(k * (fertilizationYear - mostRecentFertilizationYear));
                    }
                }
            }

            yearsSinceMostRecentFertilization = variant.TimeStepInYears * this.currentSimulationPeriod - mostRecentFertilizationYear;
            return fertX1;
        }

        public float GetPrem(OrganonVariant variant, float k, out int yearsSinceMostRecentThin)
        {
            // Hann DW, Marshall DD, Hanus ML. 2003. Equations for predicting height-to-crown-base, 5-year diameter-growth rate, 5 year height
            //   growth rate, 5-year mortality rate, and maximum size-density trajectory for Douglas-fir and western hemlock in the coastal region
            //   of the Pacific Northwest. Research Contribution 40, Forest Research Laboratory, College of Forestry, Oregon State University.
            //   https://ir.library.oregonstate.edu/concern/technical_reports/jd472x893
            // Diameter equation 10 (p34) and height equation 18 (p47). The two definitions of PREM have the same form with k = a9 = b11/b10.
            float basalAreaRemovedInMostRecentThin = this.BasalAreaThinnedByPeriod[this.currentSimulationPeriod];
            int mostRecentThinPeriod = basalAreaRemovedInMostRecentThin > 0.0F ? this.currentSimulationPeriod : Constant.NoThinPeriod;
            int mostRecentThinYear = variant.TimeStepInYears * mostRecentThinPeriod;

            float partiallyDiscountedBasalAreaSum = 0.0F;
            for (int previousPeriodIndex = this.currentSimulationPeriod - 1; previousPeriodIndex > 0; --previousPeriodIndex)
            {
                float basalAreaPreviouslyRemoved = this.BasalAreaThinnedByPeriod[previousPeriodIndex];
                if (basalAreaPreviouslyRemoved > 0.0F)
                {
                    if (mostRecentThinPeriod == Constant.NoThinPeriod)
                    {
                        // most recent thinning: exp(k * (mostRecentThinYear - thinYear)) = exp(0)
                        Debug.Assert(basalAreaRemovedInMostRecentThin == 0.0F);
                        basalAreaRemovedInMostRecentThin = basalAreaPreviouslyRemoved;
                        mostRecentThinPeriod = previousPeriodIndex;
                        mostRecentThinYear = variant.TimeStepInYears * mostRecentThinPeriod;
                    }
                    else
                    {
                        // at least through Organon 2.2.4, a code typo results in the power being calculated as (thinYear - mostRecentThinYear)
                        // This creates a positive exponent, PREM values approaching 1, and therefore overestimation of thinning responses once
                        // a second thin is performed.
                        int thinYear = variant.TimeStepInYears * previousPeriodIndex;
                        partiallyDiscountedBasalAreaSum += basalAreaPreviouslyRemoved * MathV.Exp(k * (mostRecentThinYear - thinYear));
                    }
                }
            }

            if (mostRecentThinPeriod == Constant.NoThinPeriod)
            {
                // no thin has been performed so thinning multipliers do not apply
                Debug.Assert(partiallyDiscountedBasalAreaSum == 0.0F);
                yearsSinceMostRecentThin = -1;
                return 0.0F;
            }

            yearsSinceMostRecentThin = variant.TimeStepInYears * this.currentSimulationPeriod - mostRecentThinYear;
            float basalAreaBeforeMostRecentThin = this.basalAreaByPeriod[mostRecentThinPeriod - 1];
            float prem = (basalAreaRemovedInMostRecentThin + partiallyDiscountedBasalAreaSum) / (basalAreaBeforeMostRecentThin + partiallyDiscountedBasalAreaSum);
            Debug.Assert(prem > 0.0F);
            if (prem > 0.75F)
            {
                prem = 0.75F; // max without clamp is 1.0F
            }
            return prem;
        }
    }
}
