using Osu.Cof.Ferm.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        public IList<float> HarvestTaxPerMbf { get; private init; }
        public IList<string> Name { get; private init; }
        public IList<float> PropertyTaxAndManagementPerHectareYear { get; private init; }
        public IList<float> RegenerationHarvestCostPerCubicMeter { get; private init; }
        public IList<float> RegenerationHarvestCostPerHectare { get; private init; }
        public IList<float> RegenerationHaulCostPerCubicMeter { get; private init; }
        public IList<float> ReleaseSprayCostPerHectare { get; private init; }
        public IList<float> SeedlingCost { get; private init; }
        public IList<float> SitePrepAndReplantingCostPerHectare { get; private init; }
        public IList<float> ThinningHarvestCostPerHectare { get; private init; }
        public IList<float> ThinningHarvestCostPerCubicMeter { get; private init; }
        public IList<float> ThinningHaulCostPerCubicMeter { get; private init; }
        public IList<float> ThinningPondValueMultiplier { get; private init; }
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
            // Pond values from coast region mean values in Washington Department of Natural Resources log price reports, October 2011-June 2021
            //   adjusted by US Bureau of Labor Statistics seasonally unadjusted monthly PPI.
            //
            // Harvesting cost follows from the harvest system's productivity in US$/merchantable m³.
            // Examples are
            //   1) Harvester + forwarder for thinning. ~US$ 18/m³ on tethered ground, ~US$ 17/m³ untethered.
            //   2) Feller-buncher + yarder + loader for regeneration harvest. ~US $17/m³ on cable ground from ODF Cold Boulder sale appraisal.
            //   3) Feller-buncher + skidder + processor + loader. ~US $10/m³ on low angle ground from ODF Cold Boulder sale appraisal.
            // See, for example,
            // Eriksson M, Lindroos O. 2014. Productivity of harvesters and forwarders in CTL operations in northern Sweden based on large
            //   follow-up datasets. International Journal of Forest Engineering 25(3):179-200. https://doi.org/10.1080/14942119.2014.974309
            // Green PQ, Chung W, Leshinsky B, et al. 2020. Insight into the Productivity, Cost and Soil Impacts of Cable-assisted
            //   Harvester-forwarder Thinning in Western Oregon. Forest Science 66(1):82–96. https://doi.org/10.1093/forsci/fxz049, Table 6
            //
            // Haul cost in US$/merchantable m³ is (truck capacity, kg) * (1 - bark fraction) / (log density, kg/m³) * (travel time, hours) *
            //   (hourly operating cost, US$).
            // Example truck configurations with Douglas-fir @ ~17.6% bark, ~600 kg/m³ green density
            //                      merchantable m³  truck cost, 2020 US$/hour  haul cost, US$/m³ @ 3 hour roundtrip with 97.5% utilization of weight capacity
            //   5 axle long log    ~32               ~95                       ~8.83
            //   6 axle long log    ~35              ~100                       ~8.51
            //   7 axle mule train  ~38              ~125                       ~9.80
            // Haul is often 2-4 hours roundtrip but can be longer to more remote locations.
            // See also Mason CL, Casavant KL, Libble BR, et al. 2008. The Washington Log Trucking Industry: Costs and Safety Analysis.
            //   Rural Technology Initiative and Transportation Research Group, University of  Washington.
            //   http://www.ruraltech.org/pubs/reports/2008/log_trucks/index.asp, Tables 2.31-35 and 4.6
            //
            // Oregon forest products harvest tax (https://www.oregon.gov/dor/programs/property/Pages/timber-forest-harvest.aspx) is assumed as
            // a default. Different tax calculations apply in other jusristictions and somewhat different tax calculations apply for lands
            // enrolled in Oregon's small tract forestland program.

            this.DiscountRate = new List<float>() { Constant.Financial.DefaultAnnualDiscountRate };
            //this.DouglasFirSpecialMillPondValuePerMbf = new List<float>() { 719.00 }; // US$/MBF special mill and better
            this.DouglasFir2SawPondValuePerMbf = new List<float>() { 648.00F }; // US$/MBF 2S
            this.DouglasFir3SawPondValuePerMbf = new List<float>() { 634.00F }; // US$/MBF 3S
            this.DouglasFir4SawPondValuePerMbf = new List<float>() { 551.00F }; // US$/MBF 4S/CNS
            this.HarvestTaxPerMbf = new List<float>() { Constant.Financial.OregonForestProductsHarvestTax };
            this.Name = new List<string>() { "default" };
            this.PropertyTaxAndManagementPerHectareYear = new List<float>() { Constant.AcresPerHectare * (3.40F + 4.10F) }; // US$/ha-year, mean western Oregon forest land tax of $3.40/acre in 2006 plus nominal management expense
            this.RegenerationHarvestCostPerCubicMeter = new List<float>() { 17.0F }; // US$/m³, tethered feller-buncher + yarder + processor + loader assumed
            this.RegenerationHarvestCostPerHectare = new List<float>() { Constant.AcresPerHectare * 100.0F }; // US$/ha
            this.RegenerationHaulCostPerCubicMeter = new List<float>() { 8.51F }; // US$/m³, 6 axle long log truck assumed
            this.ReleaseSprayCostPerHectare = new List<float>() { Constant.AcresPerHectare * 39.0F }; // US$/ha, one release spray
            this.SeedlingCost = new List<float>() { 0.50F }; // US$ per seedling
            this.SitePrepAndReplantingCostPerHectare = new List<float>() { Constant.AcresPerHectare * (140.0F + 155.0F) }; // US$/ha: site prep + planting labor, cost of seedlings not included
            this.ThinningHarvestCostPerCubicMeter = new List<float>() { 19.50F }; // US$/m³, harvester-forwarder on tethered ground steeper than Green et. al 2020 assumed
            this.ThinningHarvestCostPerHectare = new List<float>() { Constant.AcresPerHectare * 60.0F }; // US$/ha
            this.ThinningHaulCostPerCubicMeter = new List<float>() { 9.80F }; // US$/m³, 7 axle mule train assumed
            this.ThinningPondValueMultiplier = new List<float>() { 0.90F }; // short log price penalty
            this.TimberAppreciationRate = new List<float>() { 0.01F }; // per year
            this.WesternRedcedarCamprunPondValuePerMbf = new List<float>() { 1234.00F }; // US$/MBF
            this.WhiteWood2SawPondValuePerMbf = new List<float>() { 531.00F }; // US$/MBF 2S
            this.WhiteWood3SawPondValuePerMbf = new List<float>() { 525.00F }; // US$/MBF 3S
            this.WhiteWood4SawPondValuePerMbf = new List<float>() { 453.00F }; // US$/MBF 4S/CNS
        }

        public FinancialScenarios(FinancialScenarios other)
        {
            this.DiscountRate = new List<float>(other.DiscountRate);
            this.DouglasFir2SawPondValuePerMbf = new List<float>(other.DouglasFir2SawPondValuePerMbf);
            this.DouglasFir3SawPondValuePerMbf = new List<float>(other.DouglasFir3SawPondValuePerMbf);
            this.DouglasFir4SawPondValuePerMbf = new List<float>(other.DouglasFir4SawPondValuePerMbf);
            this.HarvestTaxPerMbf = new List<float>(other.HarvestTaxPerMbf);
            this.Name = new List<string>(other.Name);
            this.PropertyTaxAndManagementPerHectareYear = new List<float>(other.PropertyTaxAndManagementPerHectareYear);
            this.RegenerationHarvestCostPerCubicMeter = new List<float>(other.RegenerationHarvestCostPerCubicMeter);
            this.RegenerationHarvestCostPerHectare = new List<float>(other.RegenerationHarvestCostPerHectare);
            this.RegenerationHaulCostPerCubicMeter = new List<float>(other.RegenerationHaulCostPerCubicMeter);
            this.ReleaseSprayCostPerHectare = new List<float>(other.ReleaseSprayCostPerHectare);
            this.SeedlingCost = new List<float>(other.SeedlingCost);
            this.SitePrepAndReplantingCostPerHectare = new List<float>(other.SitePrepAndReplantingCostPerHectare);
            this.ThinningHarvestCostPerCubicMeter = new List<float>(other.ThinningHarvestCostPerCubicMeter);
            this.ThinningHarvestCostPerHectare = new List<float>(other.ThinningHarvestCostPerHectare);
            this.ThinningHaulCostPerCubicMeter = new List<float>(other.ThinningHaulCostPerCubicMeter);
            this.ThinningPondValueMultiplier = new List<float>(other.ThinningPondValueMultiplier);
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
            float replantingCost = this.SitePrepAndReplantingCostPerHectare[financialIndex] + this.SeedlingCost[financialIndex] * plantingDensityInTreesPerHectare;
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
            float harvestTaxPerMbf = this.HarvestTaxPerMbf[financialIndex];
            float regenerationHarvestCostPerCubicMeter = this.RegenerationHarvestCostPerCubicMeter[financialIndex] + this.RegenerationHaulCostPerCubicMeter[financialIndex];
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

                pondValue2saw -= harvestTaxPerMbf;
                pondValue3saw -= harvestTaxPerMbf;
                pondValue4saw -= harvestTaxPerMbf;

                npv2Saw += appreciationFactor * pondValue2saw * standingVolumeForSpecies.Scribner2Saw[endOfRotationPeriod] - regenerationHarvestCostPerCubicMeter * standingVolumeForSpecies.Cubic2Saw[endOfRotationPeriod];
                npv3Saw += appreciationFactor * pondValue3saw * standingVolumeForSpecies.Scribner3Saw[endOfRotationPeriod] - regenerationHarvestCostPerCubicMeter * standingVolumeForSpecies.Cubic3Saw[endOfRotationPeriod];
                npv4Saw += appreciationFactor * pondValue4saw * standingVolumeForSpecies.Scribner4Saw[endOfRotationPeriod] - regenerationHarvestCostPerCubicMeter * standingVolumeForSpecies.Cubic4Saw[endOfRotationPeriod];
            }

            float discountRate = this.DiscountRate[financialIndex];
            float netValuePerHectareAtHarvest = npv2Saw + npv3Saw + npv4Saw - this.RegenerationHarvestCostPerHectare[financialIndex];
            float discountFactor = this.GetDiscountFactor(financialIndex, harvestAgeInYears);
            return discountFactor * netValuePerHectareAtHarvest - this.PropertyTaxAndManagementPerHectareYear[financialIndex] * (1.0F - discountFactor) / discountRate;
        }

        public float GetNetPresentThinningValue(StandTrajectory trajectory, int financialIndex, int thinningPeriod, out float npv2Saw, out float npv3Saw, out float npv4Saw)
        {
            float harvestTaxPerMbf = this.HarvestTaxPerMbf[financialIndex];
            int thinningAgeInYears = trajectory.GetStartOfPeriodAge(thinningPeriod);
            float thinningCostPerCubicMeter = this.ThinningHarvestCostPerCubicMeter[financialIndex] + this.ThinningHaulCostPerCubicMeter[financialIndex];
            float thinningPondValueMultiplier = this.ThinningPondValueMultiplier[financialIndex] * this.GetTimberAppreciationFactor(financialIndex, thinningAgeInYears);
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

                pondValue2saw -= harvestTaxPerMbf;
                pondValue3saw -= harvestTaxPerMbf;
                pondValue4saw -= harvestTaxPerMbf;

                npv2Saw += thinningPondValueMultiplier * pondValue2saw * harvestVolumeForSpecies.Scribner2Saw[thinningPeriod] - thinningCostPerCubicMeter * harvestVolumeForSpecies.Cubic2Saw[thinningPeriod];
                npv3Saw += thinningPondValueMultiplier * pondValue3saw * harvestVolumeForSpecies.Scribner3Saw[thinningPeriod] - thinningCostPerCubicMeter * harvestVolumeForSpecies.Cubic3Saw[thinningPeriod];
                npv4Saw += thinningPondValueMultiplier * pondValue4saw * harvestVolumeForSpecies.Scribner4Saw[thinningPeriod] - thinningCostPerCubicMeter * harvestVolumeForSpecies.Cubic4Saw[thinningPeriod];
            }

            float netValuePerHectareAtHarvest = npv2Saw + npv3Saw + npv4Saw;
            if (netValuePerHectareAtHarvest == 0.0F)
            {
                // no volume was harvested so no fixed thinning cost is incurred
                return 0.0F;
            }
            netValuePerHectareAtHarvest -= this.ThinningHarvestCostPerHectare[financialIndex];

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
                FinancialScenarios.XlsxColumns.Reset();
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
                    else if (columnHeader.Equals("propertyTaxAndManagementPerHa", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.PropertyTaxAndManagement = columnIndex;
                    }
                    else if (columnHeader.Equals("harvestTaxPerMbf", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.HarvestTaxPerMbf = columnIndex;
                    }
                    else if (columnHeader.Equals("regenPerHa", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.RegenPerHa = columnIndex;
                    }
                    else if (columnHeader.Equals("regenHarvestPerM3", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.RegenHarvestPerCubicMeter = columnIndex;
                    }
                    else if (columnHeader.Equals("regenHaulPerM3", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.RegenHaulPerCubicMeter = columnIndex;
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
                    else if (columnHeader.Equals("thinPerHa", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ThinPerHa = columnIndex;
                    }
                    else if (columnHeader.Equals("thinHarvestPerM3", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ThinHarvestCostPerCubicMeter = columnIndex;
                    }
                    else if (columnHeader.Equals("thinHaulPerM3", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ThinHaulCostPerCubicMeter = columnIndex;
                    }
                    else if (columnHeader.Equals("thinPondMultiplier", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ThinPondValueMultiplier = columnIndex;
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
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.Name), "Scenario name column not found.");
                }
                if (FinancialScenarios.XlsxColumns.DiscountRate < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.DiscountRate), "Discount rate column not found.");
                }
                if (FinancialScenarios.XlsxColumns.HarvestTaxPerMbf < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.HarvestTaxPerMbf), "Per MBF harvest tax column not found.");
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
                if (FinancialScenarios.XlsxColumns.PropertyTaxAndManagement < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.PropertyTaxAndManagement), "Annual taxes and management column not found.");
                }
                if (FinancialScenarios.XlsxColumns.RegenPerHa < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.RegenPerHa), "Per hectare regeneration harvest cost column not found.");
                }
                if (FinancialScenarios.XlsxColumns.RegenHarvestPerCubicMeter < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.RegenHarvestPerCubicMeter), "Regeneration harvest cost per m³ column not found.");
                }
                if (FinancialScenarios.XlsxColumns.RegenHaulPerCubicMeter < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.RegenHaulPerCubicMeter), "Regeneration haul cost per m³ column not found.");
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
                if (FinancialScenarios.XlsxColumns.ThinPerHa < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ThinPerHa), "Per hectare thinning cost column not found.");
                }
                if (FinancialScenarios.XlsxColumns.ThinHarvestCostPerCubicMeter < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ThinHarvestCostPerCubicMeter), "Thinning harvest cost per m³ column not found.");
                }
                if (FinancialScenarios.XlsxColumns.ThinHaulCostPerCubicMeter < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ThinHaulCostPerCubicMeter), "Thinning haul cost per m³ column not found.");
                }
                if (FinancialScenarios.XlsxColumns.ThinPondValueMultiplier < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ThinHarvestCostPerCubicMeter), "Thinning pond value multiplier column not found.");
                }
                if (FinancialScenarios.XlsxColumns.TimberAppreciation < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.TimberAppreciation), "Annual timber appreciation rate column not found.");
                }

                this.DiscountRate.Clear();
                this.DouglasFir2SawPondValuePerMbf.Clear();
                this.DouglasFir3SawPondValuePerMbf.Clear();
                this.DouglasFir4SawPondValuePerMbf.Clear();
                this.RegenerationHarvestCostPerHectare.Clear();
                this.SitePrepAndReplantingCostPerHectare.Clear();
                this.ThinningHarvestCostPerHectare.Clear();
                this.HarvestTaxPerMbf.Clear();
                this.Name.Clear();
                this.RegenerationHarvestCostPerCubicMeter.Clear();
                this.RegenerationHaulCostPerCubicMeter.Clear();
                this.ReleaseSprayCostPerHectare.Clear();
                this.SeedlingCost.Clear();
                this.PropertyTaxAndManagementPerHectareYear.Clear();
                this.ThinningHarvestCostPerCubicMeter.Clear();
                this.ThinningHaulCostPerCubicMeter.Clear();
                this.ThinningPondValueMultiplier.Clear();
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

            // parse and check scenario's values
            float discountRate = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.DiscountRate]);
            if ((discountRate <= 0.0F) || (discountRate > 0.2F))
            {
                throw new NotSupportedException("Annual disount rate is not in the range (0.0, 0.2].");
            }
            this.DiscountRate.Add(discountRate);

            float psme2Spond = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.Psme2SPond]);
            if ((psme2Spond < 100.0F) || (psme2Spond > 3000.0F))
            {
                throw new NotSupportedException("Douglas-fir 2S pond value is not in the range US$ [100.0, 3000.0]/MBF.");
            }
            this.DouglasFir2SawPondValuePerMbf.Add(psme2Spond);

            float psme3Spond = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.Psme3SPond]);
            if ((psme3Spond < 100.0F) || (psme3Spond > 3000.0F))
            {
                throw new NotSupportedException("Douglas-fir 3S pond value is not in the range US$ [100.0, 3000.0]/MBF.");
            }
            this.DouglasFir3SawPondValuePerMbf.Add(psme3Spond);

            float psme4Spond = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.Psme4SPond]);
            if ((psme4Spond < 100.0F) || (psme4Spond > 3000.0F))
            {
                throw new NotSupportedException("Douglas-fir 4S pond value is not in the range US$ [100.0, 3000.0]/MBF.");
            }
            this.DouglasFir4SawPondValuePerMbf.Add(psme4Spond);

            float sitePrepAndReplanting = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.SitePrepFixed]);
            if ((sitePrepAndReplanting <= 0.0F) || (sitePrepAndReplanting > 1000.0F))
            {
                throw new NotSupportedException("Site preparation and replanting cost is not in the range US$ (0.0, 1000.0]/ha.");
            }
            this.SitePrepAndReplantingCostPerHectare.Add(sitePrepAndReplanting);

            float harvestTaxMbf = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.HarvestTaxPerMbf]);
            if ((harvestTaxMbf < 0.0F) || (harvestTaxMbf > 100.0F))
            {
                throw new NotSupportedException("Per MBF harvest tax is not in the range US$ [0.0, 100.0]/MBF.");
            }
            this.HarvestTaxPerMbf.Add(harvestTaxMbf);
            
            string name = rowAsStrings[FinancialScenarios.XlsxColumns.Name];
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new NotSupportedException("Financial scenario name is null or whitespace.");
            }
            this.Name.Add(name);

            float propertyTaxAndManagement = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.PropertyTaxAndManagement]);
            if ((propertyTaxAndManagement <= 0.0F) || (propertyTaxAndManagement > 100.0F))
            {
                throw new NotSupportedException("Annual property tax and management cost is not in the range US$ (0.0, 100.0]/ha.");
            }
            this.PropertyTaxAndManagementPerHectareYear.Add(propertyTaxAndManagement);

            float regenHarvestCubic = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.RegenHarvestPerCubicMeter]);
            if ((regenHarvestCubic <= 0.0F) || (regenHarvestCubic > 100.0F))
            {
                throw new NotSupportedException("Regeneration harvest cost is not in the range US$ (0.0, 100.0]/m³.");
            }
            this.RegenerationHarvestCostPerCubicMeter.Add(regenHarvestCubic);

            float regenHarvestHectare = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.RegenPerHa]);
            if ((regenHarvestHectare <= 0.0F) || (regenHarvestHectare > 1000.0F))
            {
                throw new NotSupportedException("Regeneration harvest cost is not in the range US$ (0.0, 1000.0]/ha.");
            }
            this.RegenerationHarvestCostPerHectare.Add(regenHarvestHectare);

            float regenHarvestHaul = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.RegenHarvestPerCubicMeter]);
            if ((regenHarvestHaul <= 0.0F) || (regenHarvestHaul > 100.0F))
            {
                throw new NotSupportedException("Regeneration harvest haul cost is not in the range US$ (0.0, 100.0]/m³.");
            }
            this.RegenerationHaulCostPerCubicMeter.Add(regenHarvestHaul);

            float releaseSpray = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ReleaseSpray]);
            if ((releaseSpray <= 0.0F) || (releaseSpray > 1000.0F))
            {
                throw new NotSupportedException("Release spray cost is not in the range US$ (0.0, 1000.0]/ha.");
            }
            this.ReleaseSprayCostPerHectare.Add(releaseSpray);

            float seedling = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.Seedling]); // TODO: per species pricing
            if ((seedling <= 0.0F) || (seedling > 10.0F))
            {
                throw new NotSupportedException("Seedling cost is not in the range US$ (0.0, 10.0]/seedling.");
            }
            this.SeedlingCost.Add(seedling);

            float thinCubic = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ThinHarvestCostPerCubicMeter]);
            if ((thinCubic <= 0.0F) || (thinCubic > 100.0F))
            {
                throw new NotSupportedException("Thinning harvest cost is not in the range US$ (0.0, 100.0]/ha.");
            }
            this.ThinningHarvestCostPerCubicMeter.Add(thinCubic);

            float thinHectare = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ThinPerHa]);
            if ((thinHectare <= 0.0F) || (thinHectare > 1000.0F))
            {
                throw new NotSupportedException("Thinning harvest cost is not in the range US$ (0.0, 1000.0]/ha.");
            }
            this.ThinningHarvestCostPerHectare.Add(thinHectare);

            float thinHaul = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ThinHarvestCostPerCubicMeter]);
            if ((thinHaul <= 0.0F) || (thinHaul > 100.0F))
            {
                throw new NotSupportedException("Thinning haul cost is not in the range US$ (0.0, 100.0]/m³.");
            }
            this.ThinningHaulCostPerCubicMeter.Add(thinHaul);

            float thinPondMultiplier = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ThinPondValueMultiplier]);
            if ((thinPondMultiplier <= 0.0F) || (thinPondMultiplier > 2.0F))
            {
                throw new NotSupportedException("Pond value multiplier for thinned logs is not in the range (0.0, 2.0].");
            }
            this.ThinningPondValueMultiplier.Add(thinPondMultiplier);

            float timberAppreciation = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TimberAppreciation]);
            if ((discountRate < -0.2F) || (discountRate > 0.2F))
            {
                throw new NotSupportedException("Annual timber appreciation rate is not in the range [-0.2, 0.2].");
            }
            this.TimberAppreciationRate.Add(timberAppreciation);

            float thplCamprun = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ThplCamprun]);
            if ((thplCamprun < 100.0F) || (thplCamprun > 3000.0F))
            {
                throw new NotSupportedException("Western redcedar camprun pond value is not in the range US$ [100.0, 3000.0]/MBF.");
            }
            this.WesternRedcedarCamprunPondValuePerMbf.Add(thplCamprun);

            float white2Spond = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.White2SPond]);
            if ((white2Spond < 100.0F) || (white2Spond > 3000.0F))
            {
                throw new NotSupportedException("White wood 2S pond value is not in the range US$ [100.0, 3000.0]/MBF.");
            }
            this.WhiteWood2SawPondValuePerMbf.Add(white2Spond);

            float white3Spond = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.White3SPond]);
            if ((white3Spond < 100.0F) || (white3Spond > 3000.0F))
            {
                throw new NotSupportedException("White wood 3S pond value is not in the range US$ [100.0, 3000.0]/MBF.");
            }
            this.WhiteWood3SawPondValuePerMbf.Add(white3Spond);

            float white4Spond = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.White4SPond]);
            if ((white4Spond < 100.0F) || (white4Spond >= 3000.0F))
            {
                throw new NotSupportedException("White wood 4S pond value is not in the range US$ [100.0, 3000.0]/MBF.");
            }
            this.WhiteWood4SawPondValuePerMbf.Add(white4Spond);
        }

        public void Read(string xlsxFilePath, string worksheetName)
        {
            XlsxReader reader = new();
            XlsxReader.ReadWorksheet(xlsxFilePath, worksheetName, this.ParseRow);

            Debug.Assert(this.Count == this.DiscountRate.Count);
            Debug.Assert(this.Count == this.DouglasFir2SawPondValuePerMbf.Count);
            Debug.Assert(this.Count == this.DouglasFir3SawPondValuePerMbf.Count);
            Debug.Assert(this.Count == this.DouglasFir4SawPondValuePerMbf.Count);
            Debug.Assert(this.Count == this.RegenerationHarvestCostPerHectare.Count);
            Debug.Assert(this.Count == this.SitePrepAndReplantingCostPerHectare.Count);
            Debug.Assert(this.Count == this.ThinningHarvestCostPerHectare.Count);
            Debug.Assert(this.Count == this.HarvestTaxPerMbf.Count);
            Debug.Assert(this.Count == this.Name.Count);
            Debug.Assert(this.Count == this.ReleaseSprayCostPerHectare.Count);
            Debug.Assert(this.Count == this.RegenerationHarvestCostPerCubicMeter.Count);
            Debug.Assert(this.Count == this.RegenerationHaulCostPerCubicMeter.Count);
            Debug.Assert(this.Count == this.SeedlingCost.Count);
            Debug.Assert(this.Count == this.PropertyTaxAndManagementPerHectareYear.Count);
            Debug.Assert(this.Count == this.ThinningHarvestCostPerCubicMeter.Count);
            Debug.Assert(this.Count == this.ThinningHaulCostPerCubicMeter.Count);
            Debug.Assert(this.Count == this.ThinningPondValueMultiplier.Count);
            Debug.Assert(this.Count == this.TimberAppreciationRate.Count);
            Debug.Assert(this.Count == this.WesternRedcedarCamprunPondValuePerMbf.Count);
            Debug.Assert(this.Count == this.WhiteWood2SawPondValuePerMbf.Count);
            Debug.Assert(this.Count == this.WhiteWood3SawPondValuePerMbf.Count);
            Debug.Assert(this.Count == this.WhiteWood4SawPondValuePerMbf.Count);
        }

        private class XlsxColumnIndices
        {
            public int DiscountRate { get; set; }
            public int HarvestTaxPerMbf { get; set; }
            public int Name { get; set; }
            public int PropertyTaxAndManagement { get; set; }
            public int Psme2SPond { get; set; }
            public int Psme3SPond { get; set; }
            public int Psme4SPond { get; set; }
            public int RegenPerHa { get; set; }
            public int RegenHarvestPerCubicMeter { get; set; }
            public int RegenHaulPerCubicMeter { get; set; }
            public int ReleaseSpray { get; set; }
            public int Seedling { get; set; }
            public int SitePrepFixed { get; set; }
            public int ThplCamprun { get; set; }
            public int ThinPerHa { get; set; }
            public int ThinHarvestCostPerCubicMeter { get; set; }
            public int ThinHaulCostPerCubicMeter { get; set; }
            public int ThinPondValueMultiplier { get; set; }
            public int TimberAppreciation { get; set; }
            public int White2SPond { get; set; }
            public int White3SPond { get; set; }
            public int White4SPond { get; set; }

            public XlsxColumnIndices()
            {
                this.Reset();
            }

            public void Reset()
            {
                this.DiscountRate = -1;
                this.HarvestTaxPerMbf = -1;
                this.Name = -1;
                this.PropertyTaxAndManagement = -1;
                this.Psme2SPond = -1;
                this.Psme3SPond = -1;
                this.Psme4SPond = -1;
                this.RegenPerHa = -1;
                this.RegenHarvestPerCubicMeter = -1;
                this.RegenHaulPerCubicMeter = -1;
                this.ReleaseSpray = -1;
                this.Seedling = -1;
                this.SitePrepFixed = -1;
                this.ThinPerHa = -1;
                this.ThinHarvestCostPerCubicMeter = -1;
                this.ThinHaulCostPerCubicMeter = -1;
                this.ThplCamprun = -1;
                this.TimberAppreciation = -1;
                this.ThinPondValueMultiplier = -1;
                this.White2SPond = -1;
                this.White3SPond = -1;
                this.White4SPond = -1;
            }
        }
    }
}
