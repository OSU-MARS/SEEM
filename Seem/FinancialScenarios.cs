using System;
using System.Collections.Generic;

namespace Osu.Cof.Ferm
{
    public class FinancialScenarios
    {
        private static readonly XlsxColumnIndices XlsxColumns;

        public static FinancialScenarios Default { get; private set; }

        public IList<float> DiscountRate { get; private init; }
        public IList<float> DouglasFir2SawPondValuePerMbf { get; private init; }
        public IList<float> DouglasFir3SawPondValuePerMbf { get; private init; }
        public IList<float> DouglasFir4SawPondValuePerMbf { get; private init; }
        public IList<float> FixedRegenerationHarvestCostPerHectare { get; private init; }
        public IList<float> FixedSitePrepAndReplantingCostPerHectare { get; private init; }
        public IList<float> FixedThinningCostPerHectare { get; private init; }
        public IList<string> Name { get; private init; }
        public IList<float> ReleaseSprayCostPerHectare { get; private init; }
        public IList<float> RegenerationHarvestCostPerMbf { get; private init; }
        public IList<float> SeedlingCost { get; private init; }
        public IList<float> TaxesAndManagementPerHectareYear { get; private init; }
        public IList<float> ThinningCostPerMbf { get; private init; }
        public IList<float> TimberAppreciationRate { get; private init; }
        public IList<float> WesternRedcedarCamprunPondValuePerMbf { get; private init; }
        public IList<float> WhiteWood2SawPondValuePerMbf { get; private init; }
        public IList<float> WhiteWood3SawPondValuePerMbf { get; private init; }
        public IList<float> WhiteWood4SawPondValuePerMbf { get; private init; }

        static FinancialScenarios()
        {
            FinancialScenarios.Default = new FinancialScenarios();
            FinancialScenarios.XlsxColumns = new XlsxColumnIndices();
        }

        public FinancialScenarios()
        {
            // all defaults in US$, nominal values from FOR 469 appraisal of land under Oregon's standard forestland program
            // Pond values from coast region median values in Washington Department of Natural Resources log price reports, October 2011-June 2021
            //   adjusted by US Bureau of Labor Statistics seasonally unadjusted monthly PPI.
            //
            // Harvesting cost follows from the harvest system's productivity in US$/merchantable m³.
            // Examples are
            //   1) Harvester + forwarder for thinning. ~US$ 21/m³ on tethered ground.
            //   2) Feller-buncher + yarder + loader for regeneration harvest. ~US $17/m³ on cable ground.
            //   3) Feller-buncher + skidder + processor + loader. ~US $10/m³ on low angle ground.
            //
            // Haul cost in US$/merchantable m³ is (truck capacity, kg) * (1 - bark fraction) / (log density, kg/m³) * (travel time, hours) *
            //   (hourly operating cost, US$).
            // Example truck configurations, haul is often 2-4 hours roundtrip but can be longer to more remote locations:
            //   1) 12.2 m logs on 5-6 axle truck: ~25,800 kg payload depending on regulations @ ~US$ 75-95/hour
            //   2) 7.3 m logs on 7 axle mule train: ~29,000 kg payload @ $125/hour
            // Douglas-fir example: ~17.6% bark, ~600 kg/m³ green density → ~35 merchantable m³/truck, ~39 m³/mule train → US$ 8.17/standard m³,
            //   10.10/m³ @ 3 hour roundtrip at 95% utilization of truck weight capacity
            //
            // Somewhat different tax calculations apply for lands enrolled in Oregon's small tract forestland program.
            //this.DouglasFirSpecialMillPondValuePerMbf = new List<float>() { 714.00 }; // US$/MBF special mill and better
            this.DiscountRate = new List<float>() { Constant.DefaultAnnualDiscountRate };
            this.DouglasFir2SawPondValuePerMbf = new List<float>() { 637.00F }; // US$/MBF 2S
            this.DouglasFir3SawPondValuePerMbf = new List<float>() { 615.00F }; // US$/MBF 3S
            this.DouglasFir4SawPondValuePerMbf = new List<float>() { 535.00F }; // US$/MBF 4S/CNS
            this.FixedRegenerationHarvestCostPerHectare = new List<float>() { Constant.AcresPerHectare * 100.0F }; // US$/ha
            this.FixedSitePrepAndReplantingCostPerHectare = new List<float>() { Constant.AcresPerHectare * (136.0F + 154.0F) }; // US$/ha: site prep + planting labor, cost of seedlings not included
            this.FixedThinningCostPerHectare = new List<float>() { Constant.AcresPerHectare * 60.0F }; // US$/ha
            this.Name = new List<string>() { "default" };
            this.RegenerationHarvestCostPerMbf = new List<float>() { 250.0F }; // US$/MBF, includes Oregon forest products havest tax
            this.ReleaseSprayCostPerHectare = new List<float>() { Constant.AcresPerHectare * 39.0F }; // US$/ha, one release spray
            this.SeedlingCost = new List<float>() { 0.50F }; // US$ per seedling
            this.TaxesAndManagementPerHectareYear = new List<float>() { Constant.AcresPerHectare * 7.5F }; // US$/ha-year, mean western Oregon forest land tax of $3.40/acre in 2006 plus nominal management expense
            this.ThinningCostPerMbf = new List<float>() { 275.0F }; // US$/MBF, includes Oregon forest products harvest tax
            this.TimberAppreciationRate = new List<float>() { 0.01F }; // per year
            this.WesternRedcedarCamprunPondValuePerMbf = new List<float>() { 1209.00F }; // US$/MBF
            this.WhiteWood2SawPondValuePerMbf = new List<float>() { 516.00F }; // US$/MBF 2S
            this.WhiteWood3SawPondValuePerMbf = new List<float>() { 511.00F }; // US$/MBF 3S
            this.WhiteWood4SawPondValuePerMbf = new List<float>() { 447.00F }; // US$/MBF 4S/CNS
        }

        public FinancialScenarios(FinancialScenarios other)
        {
            this.DiscountRate = new List<float>(other.DiscountRate);
            this.DouglasFir2SawPondValuePerMbf = new List<float>(other.DouglasFir2SawPondValuePerMbf);
            this.DouglasFir3SawPondValuePerMbf = new List<float>(other.DouglasFir3SawPondValuePerMbf);
            this.DouglasFir4SawPondValuePerMbf = new List<float>(other.DouglasFir4SawPondValuePerMbf);
            this.FixedRegenerationHarvestCostPerHectare = new List<float>(other.FixedRegenerationHarvestCostPerHectare);
            this.FixedSitePrepAndReplantingCostPerHectare = new List<float>(other.FixedSitePrepAndReplantingCostPerHectare);
            this.FixedThinningCostPerHectare = new List<float>(other.FixedThinningCostPerHectare);
            this.Name = new List<string>(other.Name);
            this.RegenerationHarvestCostPerMbf = new List<float>(other.RegenerationHarvestCostPerMbf);
            this.ReleaseSprayCostPerHectare = new List<float>(other.ReleaseSprayCostPerHectare);
            this.SeedlingCost = new List<float>(other.SeedlingCost);
            this.TaxesAndManagementPerHectareYear = new List<float>(other.TaxesAndManagementPerHectareYear);
            this.ThinningCostPerMbf = new List<float>(other.ThinningCostPerMbf);
            this.TimberAppreciationRate = new List<float>(other.TimberAppreciationRate);
            this.WesternRedcedarCamprunPondValuePerMbf = new List<float>(other.WesternRedcedarCamprunPondValuePerMbf);
            this.WhiteWood2SawPondValuePerMbf = new List<float>(other.WhiteWood2SawPondValuePerMbf);
            this.WhiteWood3SawPondValuePerMbf = new List<float>(other.WhiteWood3SawPondValuePerMbf);
            this.WhiteWood4SawPondValuePerMbf = new List<float>(other.WhiteWood4SawPondValuePerMbf);
        }

        public int Count
        {
            get { return this.DiscountRate.Count; }
        }

        public float GetAppreciationFactor(int financialIndex, int years)
        {
            float discountRate = this.DiscountRate[financialIndex];
            return MathF.Pow(1.0F + discountRate, years);
        }

        private float GetDiscountFactor(int financialIndex, int years)
        {
            float discountRate = this.DiscountRate[financialIndex];
            return 1.0F / MathF.Pow(1.0F + discountRate, years);
        }

        public float GetNetPresentReforestationValue(int financialIndex, float plantingDensityInTreesPerHectare)
        {
            // amoritzed reforestation expenses under 26 USC § 194 https://www.law.cornell.edu/uscode/text/26/194
            // NPV = C0 * (-1 + 1/7 * (0.5 / (1 + r) + 1 / (1 + r)^2 + ... + 1 / (1 + r)^7 + 0.5 / (1 + r)^8))
            //     = C0 * (-1 + 1/7 * (1 / (1 + r) * (0.5 + 1 / (1 + r)^1 + ... + 1 / (1 + r)^6 + 0.5 / (1 + r)^8)))
            //     = C0 * (-1 + 1/7 * (1 / (1 + r) * (0.5 + 1 / (1 + r) * (1 + ... + 1 / (1 + r)^6 + 0.5 / (1 + r)^8)))
            //     = C0 * (-1 + 1/7 * (1 / (1 + r) * (0.5 + 1 / (1 + r) * (1 + 1 / (1 + r) * (1 + ... ))))
            float discountRate = this.DiscountRate[financialIndex];
            float annualDiscountFactor = 1.0F / (1.0F + discountRate);
            float amortizationFactor = -1.0F + 1.0F / 7.0F * (annualDiscountFactor * (0.5F + // amoritzation year 1
                                                              annualDiscountFactor * (1.0F + // year 2
                                                              annualDiscountFactor * (1.0F + // year 3
                                                              annualDiscountFactor * (1.0F + // year 4
                                                              annualDiscountFactor * (1.0F + // year 5
                                                              annualDiscountFactor * (1.0F + // year 6
                                                              annualDiscountFactor * (1.0F + // year 7
                                                              annualDiscountFactor * 0.5F)))))))); // year 8
            // float amortizationFactor = -1.0F; // disable amoritzation
            float replantingCost = this.FixedSitePrepAndReplantingCostPerHectare[financialIndex] + this.SeedlingCost[financialIndex] * plantingDensityInTreesPerHectare;
            float reforestationNpv = replantingCost * amortizationFactor;

            // 26 USC § 194(c)(3) defines reforestation expenses as site prep and planting, release sprays are therefore excluded
            float releaseSprayCost = this.ReleaseSprayCostPerHectare[financialIndex] * annualDiscountFactor * annualDiscountFactor;
            // releaseSprayCost = this.ReleaseSprayCostPerHectare; // disable discounting
            reforestationNpv -= releaseSprayCost;

            return reforestationNpv;
        }

        public float GetNetPresentRegenerationHarvestValue(StandTrajectory trajectory, int financialIndex, int endOfRotationPeriod, out float npv2Saw, out float npv3Saw, out float npv4Saw)
        {
            int harvestAgeInYears = trajectory.GetEndOfPeriodAge(endOfRotationPeriod);
            float appreciationFactor = this.GetTimberAppreciationFactor(financialIndex, harvestAgeInYears);
            float regenerationHarvestCostPerMbf = this.RegenerationHarvestCostPerMbf[financialIndex];
            npv2Saw = 0.0F;
            npv3Saw = 0.0F;
            npv4Saw = 0.0F;
            foreach (TreeSpeciesMerchantableVolume standingVolumeForSpecies in trajectory.StandingVolumeBySpecies.Values)
            {
                float pondValue2saw;
                float pondValue3saw;
                float pondValue4saw;
                switch (standingVolumeForSpecies.Species)
                {
                    case FiaCode.PseudotsugaMenziesii:
                        pondValue2saw = this.DouglasFir2SawPondValuePerMbf[financialIndex];
                        pondValue3saw = this.DouglasFir3SawPondValuePerMbf[financialIndex];
                        pondValue4saw = this.DouglasFir4SawPondValuePerMbf[financialIndex];
                        break;
                    case FiaCode.ThujaPlicata:
                        pondValue2saw = this.WesternRedcedarCamprunPondValuePerMbf[financialIndex];
                        pondValue3saw = pondValue2saw;
                        pondValue4saw = pondValue2saw;
                        break;
                    case FiaCode.TsugaHeterophylla:
                        pondValue2saw = this.WhiteWood2SawPondValuePerMbf[financialIndex];
                        pondValue3saw = this.WhiteWood3SawPondValuePerMbf[financialIndex];
                        pondValue4saw = this.WhiteWood4SawPondValuePerMbf[financialIndex];
                        break;
                    default:
                        throw new NotSupportedException("Unhandled species " + standingVolumeForSpecies.Species + ".");
                }

                npv2Saw += (appreciationFactor * pondValue2saw - regenerationHarvestCostPerMbf) * standingVolumeForSpecies.Scribner2Saw[endOfRotationPeriod];
                npv3Saw += (appreciationFactor * pondValue3saw - regenerationHarvestCostPerMbf) * standingVolumeForSpecies.Scribner3Saw[endOfRotationPeriod];
                npv4Saw += (appreciationFactor * pondValue4saw - regenerationHarvestCostPerMbf) * standingVolumeForSpecies.Scribner4Saw[endOfRotationPeriod];
            }

            float discountRate = this.DiscountRate[financialIndex];
            float netValuePerHectareAtHarvest = npv2Saw + npv3Saw + npv4Saw - this.FixedRegenerationHarvestCostPerHectare[financialIndex];
            float discountFactor = this.GetDiscountFactor(financialIndex, harvestAgeInYears);
            return discountFactor * netValuePerHectareAtHarvest - this.TaxesAndManagementPerHectareYear[financialIndex] * (1.0F - discountFactor) / discountRate;
        }

        public float GetNetPresentThinningValue(StandTrajectory trajectory, int financialIndex, int thinningPeriod, out float npv2Saw, out float npv3Saw, out float npv4Saw)
        {
            int thinningAgeInYears = trajectory.GetStartOfPeriodAge(thinningPeriod);
            float appreciationFactor = this.GetTimberAppreciationFactor(financialIndex, thinningAgeInYears);
            float thinningCostPerMbf = this.ThinningCostPerMbf[financialIndex];
            npv2Saw = 0.0F;
            npv3Saw = 0.0F;
            npv4Saw = 0.0F;
            foreach (TreeSpeciesMerchantableVolume harvestVolumeForSpecies in trajectory.ThinningVolumeBySpecies.Values)
            {
                float pondValue2saw;
                float pondValue3saw;
                float pondValue4saw;
                switch (harvestVolumeForSpecies.Species)
                {
                    case FiaCode.PseudotsugaMenziesii:
                        pondValue2saw = this.DouglasFir2SawPondValuePerMbf[financialIndex];
                        pondValue3saw = this.DouglasFir3SawPondValuePerMbf[financialIndex];
                        pondValue4saw = this.DouglasFir4SawPondValuePerMbf[financialIndex];
                        break;
                    case FiaCode.ThujaPlicata:
                        pondValue2saw = this.WesternRedcedarCamprunPondValuePerMbf[financialIndex];
                        pondValue3saw = pondValue2saw;
                        pondValue4saw = pondValue2saw;
                        break;
                    case FiaCode.TsugaHeterophylla:
                        pondValue2saw = this.WhiteWood2SawPondValuePerMbf[financialIndex];
                        pondValue3saw = this.WhiteWood3SawPondValuePerMbf[financialIndex];
                        pondValue4saw = this.WhiteWood4SawPondValuePerMbf[financialIndex];
                        break;
                    default:
                       throw new NotSupportedException("Unhandled species " + harvestVolumeForSpecies.Species + ".");
                }

                npv2Saw += (appreciationFactor * pondValue2saw - thinningCostPerMbf) * harvestVolumeForSpecies.Scribner2Saw[thinningPeriod];
                npv3Saw += (appreciationFactor * pondValue3saw - thinningCostPerMbf) * harvestVolumeForSpecies.Scribner3Saw[thinningPeriod];
                npv4Saw += (appreciationFactor * pondValue4saw - thinningCostPerMbf) * harvestVolumeForSpecies.Scribner4Saw[thinningPeriod];
            }

            float netValuePerHectareAtHarvest = npv2Saw + npv3Saw + npv4Saw;
            if (netValuePerHectareAtHarvest == 0.0F)
            {
                // no volume was harvested so no fixed thinning cost is incurred
                return 0.0F;
            }
            netValuePerHectareAtHarvest -= this.FixedThinningCostPerHectare[financialIndex];

            float discountFactor = this.GetDiscountFactor(financialIndex, thinningAgeInYears);
            return discountFactor * netValuePerHectareAtHarvest;
        }

        public float GetNetPresentValue(StandTrajectory trajectory, int financialIndex, int endOfRotationPeriod)
        {
            float netPresentValue = this.GetNetPresentReforestationValue(financialIndex, trajectory.PlantingDensityInTreesPerHectare);
            for (int periodIndex = 1; periodIndex < endOfRotationPeriod; ++periodIndex) // no thinning in period 0
            {
                netPresentValue += this.GetNetPresentThinningValue(trajectory, financialIndex, periodIndex, out float _, out float _, out float _);
            }
            netPresentValue += this.GetNetPresentRegenerationHarvestValue(trajectory, financialIndex, endOfRotationPeriod, out float _, out float _, out float _);
            return netPresentValue;
        }

        private float GetTimberAppreciationFactor(int financialIndex, int years)
        {
            float timberAppreciationRate = this.TimberAppreciationRate[financialIndex];
            if (timberAppreciationRate == 0.0F)
            {
                return 1.0F;
            }
            return MathF.Pow(1.0F + this.TimberAppreciationRate[financialIndex], years);
        }

        private void ParseRow(int rowIndex, string[] rowAsStrings)
        {
            if (rowIndex == 0)
            {
                // parse header
                for (int columnIndex = 0; columnIndex < rowAsStrings.Length; ++columnIndex)
                {
                    string columnHeader = rowAsStrings[columnIndex];
                    if (String.IsNullOrWhiteSpace(columnHeader))
                    {
                        break;
                    }

                    if (columnHeader.Equals("name", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.Name = columnIndex;
                    }
                    else if (columnHeader.Equals("discountRate", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.DiscountRate = columnIndex;
                    }
                    else if (columnHeader.Equals("psme2Spond", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.Psme2SPond = columnIndex;
                    }
                    else if (columnHeader.Equals("psme3Spond", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.Psme3SPond = columnIndex;
                    }
                    else if (columnHeader.StartsWith("psme4Spond", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.Psme4SPond = columnIndex;
                    }
                    else if (columnHeader.Equals("thplCamprun", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ThplCamprun = columnIndex;
                    }
                    else if (columnHeader.StartsWith("white2Spond", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.White2SPond = columnIndex;
                    }
                    else if (columnHeader.StartsWith("white3Spond", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.White3SPond = columnIndex;
                    }
                    else if (columnHeader.StartsWith("white4Spond", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.White4SPond = columnIndex;
                    }
                    else if (columnHeader.Equals("annualTaxAndManagement", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.AnnualTaxAndManagement = columnIndex;
                    }
                    else if (columnHeader.Equals("regenFixed", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.RegenFixed = columnIndex;
                    }
                    else if (columnHeader.Equals("regenPerMbf", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.RegenPerMbf = columnIndex;
                    }
                    else if (columnHeader.Equals("releaseSpray", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ReleaseSpray = columnIndex;
                    }
                    else if (columnHeader.Equals("seedling", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.Seedling = columnIndex;
                    }
                    else if (columnHeader.Equals("sitePrepFixed", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.SitePrepFixed = columnIndex;
                    }
                    else if (columnHeader.Equals("thinFixed", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ThinFixed = columnIndex;
                    }
                    else if (columnHeader.Equals("thinPerMbf", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ThinPerMbf = columnIndex;
                    }
                    else if (columnHeader.Equals("timberAppreciation", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.TimberAppreciation = columnIndex;
                    }
                    else
                    {
                        // ignore column for now
                    }
                }

                // check header
                if (FinancialScenarios.XlsxColumns.Name < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.Name), "Name column not found.");
                }
                if (FinancialScenarios.XlsxColumns.DiscountRate < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.DiscountRate), "Discount rate column not found.");
                }
                if (FinancialScenarios.XlsxColumns.Psme2SPond < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.Psme2SPond), "Douglas-fir 2S column not found.");
                }
                if (FinancialScenarios.XlsxColumns.Psme3SPond < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.Psme3SPond), "Douglas-fir 3S column not found.");
                }
                if (FinancialScenarios.XlsxColumns.Psme4SPond < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.Psme4SPond), "Douglas-fir 4S/CNS column not found.");
                }
                if (FinancialScenarios.XlsxColumns.ThplCamprun < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ThplCamprun), "Western redcedar camprun column not found.");
                }
                if (FinancialScenarios.XlsxColumns.White2SPond < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.White2SPond), "White wood 2S column not found.");
                }
                if (FinancialScenarios.XlsxColumns.White3SPond < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.White3SPond), "White wood 3S column not found.");
                }
                if (FinancialScenarios.XlsxColumns.White4SPond < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.White4SPond), "White wood 4S/CNS column not found.");
                }
                if (FinancialScenarios.XlsxColumns.AnnualTaxAndManagement < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.AnnualTaxAndManagement), "Annual taxes and management column not found.");
                }
                if (FinancialScenarios.XlsxColumns.RegenFixed < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.RegenFixed), "Fixed regeneration harvest cost column not found.");
                }
                if (FinancialScenarios.XlsxColumns.RegenPerMbf < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.RegenPerMbf), "Variable regeneration harvest cost column not found.");
                }
                if (FinancialScenarios.XlsxColumns.ReleaseSpray < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ReleaseSpray), "Release spray column not found.");
                }
                if (FinancialScenarios.XlsxColumns.Seedling < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.Seedling), "Cost per seedling column not found.");
                }
                if (FinancialScenarios.XlsxColumns.SitePrepFixed < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.SitePrepFixed), "Fixed site prep cost column not found.");
                }
                if (FinancialScenarios.XlsxColumns.ThinFixed < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ThinFixed), "Fixed thinning cost column not found.");
                }
                if (FinancialScenarios.XlsxColumns.ThinPerMbf < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ThinPerMbf), "Per MBF thinning cost column not found.");
                }
                if (FinancialScenarios.XlsxColumns.TimberAppreciation < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.TimberAppreciation), "Annual timber appreciation rate column not found.");
                }

                this.DiscountRate.Clear();
                this.DouglasFir2SawPondValuePerMbf.Clear();
                this.DouglasFir3SawPondValuePerMbf.Clear();
                this.DouglasFir4SawPondValuePerMbf.Clear();
                this.FixedRegenerationHarvestCostPerHectare.Clear();
                this.FixedSitePrepAndReplantingCostPerHectare.Clear();
                this.FixedThinningCostPerHectare.Clear();
                this.Name.Clear();
                this.RegenerationHarvestCostPerMbf.Clear();
                this.ReleaseSprayCostPerHectare.Clear();
                this.SeedlingCost.Clear();
                this.TaxesAndManagementPerHectareYear.Clear();
                this.ThinningCostPerMbf.Clear();
                this.TimberAppreciationRate.Clear();
                this.WesternRedcedarCamprunPondValuePerMbf.Clear();
                this.WhiteWood2SawPondValuePerMbf.Clear();
                this.WhiteWood3SawPondValuePerMbf.Clear();
                this.WhiteWood4SawPondValuePerMbf.Clear();

                return;
            }

            if (String.IsNullOrEmpty(rowAsStrings[FinancialScenarios.XlsxColumns.Name]))
            {
                return; // assume end of data in file
            }

            // parse data
            this.DiscountRate.Add(Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.DiscountRate]));
            this.DouglasFir2SawPondValuePerMbf.Add(Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.Psme2SPond]));
            this.DouglasFir3SawPondValuePerMbf.Add(Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.Psme3SPond]));
            this.DouglasFir4SawPondValuePerMbf.Add(Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.Psme4SPond]));
            this.FixedRegenerationHarvestCostPerHectare.Add(Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.RegenFixed]));
            this.FixedSitePrepAndReplantingCostPerHectare.Add(Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.SitePrepFixed]));
            this.FixedThinningCostPerHectare.Add(Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ThinFixed]));
            this.Name.Add(rowAsStrings[FinancialScenarios.XlsxColumns.Name]);
            this.RegenerationHarvestCostPerMbf.Add(Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.RegenPerMbf]));
            this.ReleaseSprayCostPerHectare.Add(Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ReleaseSpray]));
            this.SeedlingCost.Add(Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.Seedling]));
            this.TaxesAndManagementPerHectareYear.Add(Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.AnnualTaxAndManagement]));
            this.ThinningCostPerMbf.Add(Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ThinPerMbf]));
            this.TimberAppreciationRate.Add(Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TimberAppreciation]));
            this.WesternRedcedarCamprunPondValuePerMbf.Add(Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ThplCamprun]));
            this.WhiteWood2SawPondValuePerMbf.Add(Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.White2SPond]));
            this.WhiteWood3SawPondValuePerMbf.Add(Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.White3SPond]));
            this.WhiteWood4SawPondValuePerMbf.Add(Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.White4SPond]));
        }

        public void Read(string xlsxFilePath, string worksheetName)
        {
            XlsxReader reader = new();
            XlsxReader.ReadWorksheet(xlsxFilePath, worksheetName, this.ParseRow);
        }

        private class XlsxColumnIndices
        {
            public int AnnualTaxAndManagement { get; set; }
            public int DiscountRate { get; set; }
            public int Name { get; set; }
            public int Psme2SPond { get; set; }
            public int Psme3SPond { get; set; }
            public int Psme4SPond { get; set; }
            public int RegenFixed { get; set; }
            public int RegenPerMbf { get; set; }
            public int ReleaseSpray { get; set; }
            public int Seedling { get; set; }
            public int SitePrepFixed { get; set; }
            public int ThplCamprun { get; set; }
            public int ThinFixed { get; set; }
            public int ThinPerMbf { get; set; }
            public int TimberAppreciation { get; set; }
            public int White2SPond { get; set; }
            public int White3SPond { get; set; }
            public int White4SPond { get; set; }

            public XlsxColumnIndices()
            {
                this.AnnualTaxAndManagement = -1;
                this.DiscountRate = -1;
                this.Name = -1;
                this.Psme2SPond = -1;
                this.Psme3SPond = -1;
                this.Psme4SPond = -1;
                this.RegenFixed = -1;
                this.RegenPerMbf = -1;
                this.ReleaseSpray = -1;
                this.Seedling = -1;
                this.SitePrepFixed = -1;
                this.ThinFixed = -1;
                this.ThinPerMbf = -1;
                this.ThplCamprun = -1;
                this.TimberAppreciation = -1;
                this.White2SPond = -1;
                this.White3SPond = -1;
                this.White4SPond = -1;
            }
        }
    }
}
