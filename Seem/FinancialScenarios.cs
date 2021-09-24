using Osu.Cof.Ferm.Silviculture;
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
        public IList<HarvestSystems> HarvestSystems { get; private init; }
        public IList<float> HarvestTaxPerMbf { get; private init; }
        public IList<string> Name { get; private init; }
        public IList<float> PropertyTaxAndManagementPerHectareYear { get; private init; }
        public IList<float> RegenerationHarvestCostPerHectare { get; private init; }
        public IList<float> RegenerationHaulCostPerCubicMeter { get; private init; }
        public IList<float> RegenerationRoadCostPerCubicMeter { get; private init; }
        public IList<float> RegenerationSlashCostPerCubicMeter { get; private init; }
        public IList<float> ReleaseSprayCostPerHectare { get; private init; }
        public IList<float> SeedlingCost { get; private init; }
        public IList<float> SitePrepAndReplantingCostPerHectare { get; private init; }
        public IList<float> ThinningHarvestCostPerHectare { get; private init; }
        public IList<float> ThinningHaulCostPerCubicMeter { get; private init; }
        public IList<float> ThinningPondValueMultiplier { get; private init; }
        public IList<float> ThinningRoadCostPerCubicMeter { get; private init; }
        public IList<float> ThinningSlashCostPerCubicMeter { get; private init; }
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
            //this.DouglasFirSpecialMillPondValuePerMbf = new List<float>() { 720.00 }; // US$/MBF special mill and better
            this.DouglasFir2SawPondValuePerMbf = new List<float>() { 649.00F }; // US$/MBF 2S
            this.DouglasFir3SawPondValuePerMbf = new List<float>() { 635.00F }; // US$/MBF 3S
            this.DouglasFir4SawPondValuePerMbf = new List<float>() { 552.00F }; // US$/MBF 4S/CNS
            this.HarvestSystems = new List<HarvestSystems>() { Silviculture.HarvestSystems.Default };
            this.HarvestTaxPerMbf = new List<float>() { Constant.Financial.OregonForestProductsHarvestTax };
            this.Name = new List<string>() { "default" };
            this.PropertyTaxAndManagementPerHectareYear = new List<float>() { Constant.HarvestCost.PropertyTaxRate * Constant.HarvestCost.AssessedValue + Constant.HarvestCost.AdmininistrationCost }; // US$/ha-year
            this.RegenerationHarvestCostPerHectare = new List<float>() { Constant.HarvestCost.TimberCruisePerHectare + Constant.HarvestCost.TimberSaleAdministrationPerHectare + 5.0F * Constant.HarvestCost.LowboyInAndOut / Constant.HarvestCost.UnitSize + Constant.HarvestCost.RoadReopening + Constant.HarvestCost.BrushControl }; // US$/ha, machines are dozer + feller-buncher + yarder + processor + loader
            this.RegenerationHaulCostPerCubicMeter = new List<float>() { 8.79F }; // US$/m³, 6 axle long log truck assumed
            this.RegenerationRoadCostPerCubicMeter = new List<float>() { Constant.HarvestCost.RoadMaintenance }; // US$/m³
            this.RegenerationSlashCostPerCubicMeter = new List<float>() { Constant.HarvestCost.SlashDisposal + Constant.HarvestCost.YarderLandingSlashDisposal }; // US$/m³
            this.ReleaseSprayCostPerHectare = new List<float>() { Constant.HarvestCost.ReleaseSpray }; // US$/ha, one release spray
            this.SeedlingCost = new List<float>() { 0.50F }; // US$ per seedling, make species specific when needed
            this.SitePrepAndReplantingCostPerHectare = new List<float>() { Constant.HarvestCost.SitePrep + Constant.HarvestCost.PlantingLabor }; // US$/ha: site prep + planting labor, cost of seedlings not included
            this.ThinningHarvestCostPerHectare = new List<float>() { Constant.HarvestCost.TimberCruisePerHectare + Constant.HarvestCost.TimberSaleAdministrationPerHectare + 3.0F * Constant.HarvestCost.LowboyInAndOut / Constant.HarvestCost.UnitSize + Constant.HarvestCost.RoadReopening + Constant.HarvestCost.BrushControl }; // US$/ha, machines are dozer + harvester + forwarder
            this.ThinningHaulCostPerCubicMeter = new List<float>() { 10.17F }; // US$/m³, 7 axle mule train assumed
            this.ThinningPondValueMultiplier = new List<float>() { 0.90F }; // short log price penalty
            this.ThinningRoadCostPerCubicMeter = new List<float>() { Constant.HarvestCost.RoadMaintenance }; // US$/m³
            this.ThinningSlashCostPerCubicMeter = new List<float>() { Constant.HarvestCost.SlashDisposal }; // US$/m³
            this.TimberAppreciationRate = new List<float>() { 0.0F }; // per year
            this.WesternRedcedarCamprunPondValuePerMbf = new List<float>() { 1238.00F }; // US$/MBF
            this.WhiteWood2SawPondValuePerMbf = new List<float>() { 531.00F }; // US$/MBF 2S
            this.WhiteWood3SawPondValuePerMbf = new List<float>() { 525.00F }; // US$/MBF 3S
            this.WhiteWood4SawPondValuePerMbf = new List<float>() { 454.00F }; // US$/MBF 4S/CNS
        }

        public FinancialScenarios(FinancialScenarios other)
        {
            this.DiscountRate = new List<float>(other.DiscountRate);
            this.DouglasFir2SawPondValuePerMbf = new List<float>(other.DouglasFir2SawPondValuePerMbf);
            this.DouglasFir3SawPondValuePerMbf = new List<float>(other.DouglasFir3SawPondValuePerMbf);
            this.DouglasFir4SawPondValuePerMbf = new List<float>(other.DouglasFir4SawPondValuePerMbf);
            this.HarvestSystems = new List<HarvestSystems>(other.HarvestSystems);
            this.HarvestTaxPerMbf = new List<float>(other.HarvestTaxPerMbf);
            this.Name = new List<string>(other.Name);
            this.PropertyTaxAndManagementPerHectareYear = new List<float>(other.PropertyTaxAndManagementPerHectareYear);
            this.RegenerationHarvestCostPerHectare = new List<float>(other.RegenerationHarvestCostPerHectare);
            this.RegenerationHaulCostPerCubicMeter = new List<float>(other.RegenerationHaulCostPerCubicMeter);
            this.RegenerationRoadCostPerCubicMeter = new List<float>(other.RegenerationRoadCostPerCubicMeter);
            this.RegenerationSlashCostPerCubicMeter = new List<float>(other.RegenerationSlashCostPerCubicMeter);
            this.ReleaseSprayCostPerHectare = new List<float>(other.ReleaseSprayCostPerHectare);
            this.SeedlingCost = new List<float>(other.SeedlingCost);
            this.SitePrepAndReplantingCostPerHectare = new List<float>(other.SitePrepAndReplantingCostPerHectare);
            this.ThinningHarvestCostPerHectare = new List<float>(other.ThinningHarvestCostPerHectare);
            this.ThinningHaulCostPerCubicMeter = new List<float>(other.ThinningHaulCostPerCubicMeter);
            this.ThinningPondValueMultiplier = new List<float>(other.ThinningPondValueMultiplier);
            this.ThinningRoadCostPerCubicMeter = new List<float>(other.ThinningRoadCostPerCubicMeter);
            this.ThinningSlashCostPerCubicMeter = new List<float>(other.ThinningSlashCostPerCubicMeter);
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

        public HarvestFinancialValue GetNetPresentRegenerationHarvestValue(StandTrajectory trajectory, int financialIndex, int endOfRotationPeriod)
        {
            Stand? stand = trajectory.StandByPeriod[endOfRotationPeriod];
            if (stand == null)
            {
                throw new InvalidOperationException("Stand information is missing for period " + endOfRotationPeriod + ". Has the stand trajectory been fully simulated?");
            }
            trajectory.RecalculateStandingVolumeIfNeeded(endOfRotationPeriod);

            int harvestAgeInYears = trajectory.GetEndOfPeriodAge(endOfRotationPeriod);
            float appreciationFactor = this.GetTimberAppreciationFactor(financialIndex, harvestAgeInYears);
            HarvestSystems harvestSystems = this.HarvestSystems[financialIndex];
            float harvestTaxPerMbf = this.HarvestTaxPerMbf[financialIndex];
            float regenerationHarvestCostPerCubicMeter = this.RegenerationHaulCostPerCubicMeter[financialIndex] + this.RegenerationSlashCostPerCubicMeter[financialIndex] + this.RegenerationRoadCostPerCubicMeter[financialIndex];
            HarvestFinancialValue financialValue = new();
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
                    case FiaCode.AbiesGrandis: // also Abies amabalis and A. procera, westside
                    case FiaCode.PiceaSitchensis: // also Picea engelmanii
                        pondValue2saw = this.WhiteWood2SawPondValuePerMbf[financialIndex];
                        pondValue3saw = this.WhiteWood3SawPondValuePerMbf[financialIndex];
                        pondValue4saw = this.WhiteWood4SawPondValuePerMbf[financialIndex];
                        break;
                    default:
                        throw new NotSupportedException("Unhandled species " + standingVolumeForSpecies.Species + ".");
                }

                float cubic2saw = standingVolumeForSpecies.Cubic2Saw[endOfRotationPeriod];
                float scribner2saw = standingVolumeForSpecies.Scribner2Saw[endOfRotationPeriod];
                financialValue.NetPresentValue2Saw += appreciationFactor * (pondValue2saw - harvestTaxPerMbf) * scribner2saw - regenerationHarvestCostPerCubicMeter * cubic2saw;

                float cubic3saw = standingVolumeForSpecies.Cubic3Saw[endOfRotationPeriod];
                float scribner3saw = standingVolumeForSpecies.Scribner3Saw[endOfRotationPeriod];
                financialValue.NetPresentValue3Saw += appreciationFactor * (pondValue3saw - harvestTaxPerMbf) * scribner3saw - regenerationHarvestCostPerCubicMeter * cubic3saw;

                float cubic4saw = standingVolumeForSpecies.Cubic4Saw[endOfRotationPeriod];
                float scribner4saw = standingVolumeForSpecies.Scribner4Saw[endOfRotationPeriod];
                financialValue.NetPresentValue4Saw += appreciationFactor * (pondValue4saw - harvestTaxPerMbf) * scribner4saw - regenerationHarvestCostPerCubicMeter * cubic4saw;
            }

            // assmume some volume was harvested so costs are incurred
            LongLogHarvest logLogHarvestCost = harvestSystems.GetLongLogHarvestCosts(stand);
            float netValuePerHectareAtHarvest = financialValue.NetPresentValue2Saw + financialValue.NetPresentValue3Saw + financialValue.NetPresentValue4Saw - logLogHarvestCost.MinimumCost - this.RegenerationHarvestCostPerHectare[financialIndex];

            float discountRate = this.DiscountRate[financialIndex];
            float discountFactor = this.GetDiscountFactor(financialIndex, harvestAgeInYears);
            financialValue.NetPresentValue = discountFactor * netValuePerHectareAtHarvest - this.PropertyTaxAndManagementPerHectareYear[financialIndex] * (1.0F - discountFactor) / discountRate; // present value of finite series of annual payments = (1 - (1 + r)^-n) / r
            return financialValue;
        }

        public HarvestFinancialValue GetNetPresentThinningValue(StandTrajectory trajectory, int financialIndex, int thinningPeriod)
        {
            Stand? previousStand = trajectory.StandByPeriod[thinningPeriod - 1];
            if (previousStand == null)
            {
                throw new InvalidOperationException("Stand information is missing for period " + (thinningPeriod - 1) + ". Has the stand trajectory been simulated?");
            }
            trajectory.RecalculateThinningVolumeIfNeeded(thinningPeriod);

            HarvestSystems harvestSystems = this.HarvestSystems[financialIndex];
            float harvestTaxPerMbf = this.HarvestTaxPerMbf[financialIndex];
            int thinningAgeInYears = trajectory.GetStartOfPeriodAge(thinningPeriod);
            float thinningHaulAndSlashCostPerCubicMeter = this.ThinningHaulCostPerCubicMeter[financialIndex] + this.ThinningRoadCostPerCubicMeter[financialIndex] + this.ThinningSlashCostPerCubicMeter[financialIndex];
            float thinningPondValueMultiplier = this.ThinningPondValueMultiplier[financialIndex] * this.GetTimberAppreciationFactor(financialIndex, thinningAgeInYears);

            // pond value net of log movement costs
            HarvestFinancialValue financialValue = new();
            foreach (TreeSpeciesMerchantableVolume harvestVolumeForSpecies in trajectory.ThinningVolumeBySpecies.Values)
            {
                // check for nonzero cubic volume removal
                float cubic2Saw = harvestVolumeForSpecies.Cubic2Saw[thinningPeriod];
                float cubic3Saw = harvestVolumeForSpecies.Cubic3Saw[thinningPeriod];
                float cubic4Saw = harvestVolumeForSpecies.Cubic4Saw[thinningPeriod];
                float cubicVolumeFromSpecies = cubic2Saw + cubic3Saw + cubic4Saw;
                if (cubicVolumeFromSpecies == 0.0F)
                {
                    continue;
                }

                // account for volume harvested
                financialValue.CubicVolume += cubicVolumeFromSpecies;

                float logs2saw = harvestVolumeForSpecies.Logs2Saw[thinningPeriod];
                float forwardingCost2Saw = harvestSystems.GetForwardingCostForSort(previousStand, harvestVolumeForSpecies.Species, cubic2Saw, logs2saw);
                float logs3saw = harvestVolumeForSpecies.Logs3Saw[thinningPeriod];
                float forwardingCost3Saw = harvestSystems.GetForwardingCostForSort(previousStand, harvestVolumeForSpecies.Species, cubic3Saw, logs3saw);
                float logs4saw = harvestVolumeForSpecies.Logs4Saw[thinningPeriod];
                float forwardingCost4Saw = harvestSystems.GetForwardingCostForSort(previousStand, harvestVolumeForSpecies.Species, cubic4Saw, logs4saw);

                // Scribner volume
                float pondValue2Saw;
                float pondValue3Saw;
                float pondValue4Saw;
                switch (harvestVolumeForSpecies.Species)
                {
                    case FiaCode.PseudotsugaMenziesii:
                        pondValue2Saw = this.DouglasFir2SawPondValuePerMbf[financialIndex];
                        pondValue3Saw = this.DouglasFir3SawPondValuePerMbf[financialIndex];
                        pondValue4Saw = this.DouglasFir4SawPondValuePerMbf[financialIndex];
                        break;
                    case FiaCode.ThujaPlicata:
                        pondValue2Saw = this.WesternRedcedarCamprunPondValuePerMbf[financialIndex];
                        pondValue3Saw = pondValue2Saw;
                        pondValue4Saw = pondValue2Saw;
                        break;
                    case FiaCode.TsugaHeterophylla:
                    case FiaCode.AbiesGrandis:
                    case FiaCode.PiceaSitchensis:
                        pondValue2Saw = this.WhiteWood2SawPondValuePerMbf[financialIndex];
                        pondValue3Saw = this.WhiteWood3SawPondValuePerMbf[financialIndex];
                        pondValue4Saw = this.WhiteWood4SawPondValuePerMbf[financialIndex];
                        break;
                    default:
                       throw new NotSupportedException("Unhandled species " + harvestVolumeForSpecies.Species + ".");
                }

                // NPV = scale adjustment * appreciation * $/MBF * MBF/ha - $/m³ * m³/ha = $/ha
                float scribner2saw = harvestVolumeForSpecies.Scribner2Saw[thinningPeriod]; // MBF/ha
                financialValue.NetPresentValue2Saw += thinningPondValueMultiplier * (pondValue2Saw - harvestTaxPerMbf) * scribner2saw - forwardingCost2Saw - thinningHaulAndSlashCostPerCubicMeter * cubic2Saw;

                float scribner3saw = harvestVolumeForSpecies.Scribner3Saw[thinningPeriod];
                financialValue.NetPresentValue3Saw += thinningPondValueMultiplier * (pondValue3Saw - harvestTaxPerMbf) * scribner3saw - forwardingCost3Saw - thinningHaulAndSlashCostPerCubicMeter * cubic3Saw;

                float scribner4saw = harvestVolumeForSpecies.Scribner4Saw[thinningPeriod];
                financialValue.NetPresentValue4Saw += thinningPondValueMultiplier * (pondValue4Saw - harvestTaxPerMbf) * scribner4saw - forwardingCost4Saw - thinningHaulAndSlashCostPerCubicMeter * cubic4Saw;
            }

            if (financialValue.CubicVolume > 0.0F)
            {
                // volume was harvested so thinning costs are incurred
                float harvesterFellingCost = harvestSystems.GetHarvesterFellingAndProcessingCost(previousStand, trajectory.IndividualTreeSelectionBySpecies, thinningPeriod);
                float netValuePerHectareAtHarvest = financialValue.NetPresentValue2Saw + financialValue.NetPresentValue3Saw + financialValue.NetPresentValue4Saw - harvesterFellingCost - this.ThinningHarvestCostPerHectare[financialIndex];
                float discountFactor = this.GetDiscountFactor(financialIndex, thinningAgeInYears);
                financialValue.NetPresentValue = discountFactor * netValuePerHectareAtHarvest;
            }

            return financialValue;
        }

        public float GetNetPresentValue(StandTrajectory trajectory, int financialIndex, int endOfRotationPeriod)
        {
            float netPresentValue = this.GetNetPresentReforestationValue(financialIndex, trajectory.PlantingDensityInTreesPerHectare);
            foreach (Harvest harvest in trajectory.Treatments.Harvests)
            {
                // for now, assume only one harvest per period
                HarvestFinancialValue thinFinancialValue = this.GetNetPresentThinningValue(trajectory, financialIndex, harvest.Period);
                netPresentValue += thinFinancialValue.NetPresentValue;
            }
            HarvestFinancialValue regenFinancialValue = this.GetNetPresentRegenerationHarvestValue(trajectory, financialIndex, endOfRotationPeriod);
            netPresentValue += regenFinancialValue.NetPresentValue;
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
                    if (columnHeader.Equals("chainsawPMh", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ChainsawPMh = columnIndex;
                    }
                    else if (columnHeader.Equals("chainsawProductivity", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ChainsawProductivity = columnIndex;
                    }
                    else if (columnHeader.Equals("corridorWidth", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.CorridorWidth = columnIndex;
                    }
                    else if (columnHeader.Equals("discountRate", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.DiscountRate = columnIndex;
                    }
                    else if (columnHeader.Equals("fellerBuncherConstant", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.FellerBuncherConstant = columnIndex;
                    }
                    else if (columnHeader.Equals("fellerBuncherLinear", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.FellerBuncherLinear = columnIndex;
                    }
                    else if (columnHeader.Equals("fellerBuncherPMh", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.FellerBuncherPMh = columnIndex;
                    }
                    else if (columnHeader.Equals("fellerBuncherSlopeThreshold", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.FellerBuncherSlopeThreshold = columnIndex;
                    }
                    else if (columnHeader.Equals("forwarderPayload", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ForwarderPayload = columnIndex;
                    }
                    else if (columnHeader.Equals("forwarderLoadedTethered", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ForwarderLoadedTethered = columnIndex;
                    }
                    else if (columnHeader.Equals("forwarderLoadedUntethered", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ForwarderLoadedUntethered = columnIndex;
                    }
                    else if (columnHeader.Equals("forwarderOnRoad", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ForwarderOnRoad = columnIndex;
                    }
                    else if (columnHeader.Equals("forwarderPMh", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ForwarderPMh = columnIndex;
                    }
                    else if (columnHeader.Equals("forwarderUnloadedTethered", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ForwarderUnloadedTethered = columnIndex;
                    }
                    else if (columnHeader.Equals("forwarderUnloadedUntethered", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ForwarderUnloadedUntethered = columnIndex;
                    }
                    else if (columnHeader.Equals("grappleYardingConstant", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.GrappleYardingConstant = columnIndex;
                    }
                    else if (columnHeader.Equals("grappleYardingLinear", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.GrappleYardingLinear = columnIndex;
                    }
                    else if (columnHeader.Equals("grappleSwingYarderMaxPayload", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.GrappleSwingYarderMaxPayload = columnIndex;
                    }
                    else if (columnHeader.Equals("grappleSwingYarderMeanPayload", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.GrappleSwingYarderMeanPayload = columnIndex;
                    }
                    else if (columnHeader.Equals("grappleSwingYarderSMh", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.GrappleSwingYarderSMh = columnIndex;
                    }
                    else if (columnHeader.Equals("grappleSwingYarderUtilization", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.GrappleSwingYarderUtilization = columnIndex;
                    }
                    else if (columnHeader.Equals("grappleYoaderMaxPayload", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.GrappleYoaderMaxPayload = columnIndex;
                    }
                    else if (columnHeader.Equals("grappleYoaderMeanPayload", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.GrappleYoaderMeanPayload = columnIndex;
                    }
                    else if (columnHeader.Equals("grappleYoaderSMh", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.GrappleYoaderSMh = columnIndex;
                    }
                    else if (columnHeader.Equals("grappleYoaderUtilization", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.GrappleYoaderUtilization = columnIndex;
                    }
                    else if (columnHeader.Equals("harvestTaxPerMbf", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.HarvestTaxPerMbf = columnIndex;
                    }
                    else if (columnHeader.Equals("loaderProductivity", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.LoaderProductivity = columnIndex;
                    }
                    else if (columnHeader.Equals("loaderSMh", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.LoaderSMh = columnIndex;
                    }
                    else if (columnHeader.Equals("loaderUtilization", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.LoaderUtilization = columnIndex;
                    }
                    else if (columnHeader.Equals("name", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.Name = columnIndex;
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
                    else if (columnHeader.Equals("processorConstant", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ProcessorConstant = columnIndex;
                    }
                    else if (columnHeader.Equals("processorLinear", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ProcessorLinear = columnIndex;
                    }
                    else if (columnHeader.Equals("processorQuadratic1", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ProcessorQuadratic1 = columnIndex;
                    }
                    else if (columnHeader.Equals("processorQuadratic2", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ProcessorQuadratic2 = columnIndex;
                    }
                    else if (columnHeader.Equals("processorQuadraticThreshold1", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ProcessorQuadraticThreshold1 = columnIndex;
                    }
                    else if (columnHeader.Equals("processorQuadraticThreshold2", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ProcessorQuadraticThreshold2 = columnIndex;
                    }
                    else if (columnHeader.Equals("processorSMh", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ProcessorSMh = columnIndex;
                    }
                    else if (columnHeader.Equals("processorUtilization", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ProcessorUtilization = columnIndex;
                    }
                    else if (columnHeader.Equals("propertyTaxAndManagementPerHa", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.PropertyTaxAndManagement = columnIndex;
                    }
                    else if (columnHeader.Equals("regenPerHa", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.RegenPerHa = columnIndex;
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
                    else if (columnHeader.Equals("thinHaulPerM3", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ThinHaulCostPerCubicMeter = columnIndex;
                    }
                    else if (columnHeader.Equals("thinPondMultiplier", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ThinPondValueMultiplier = columnIndex;
                    }
                    else if (columnHeader.Equals("thplCamprun", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.ThplCamprun = columnIndex;
                    }
                    else if (columnHeader.Equals("timberAppreciation", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.TimberAppreciation = columnIndex;
                    }
                    else if (columnHeader.Equals("trackedHarvesterConstant", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.TrackedHarvesterConstant = columnIndex;
                    }
                    else if (columnHeader.Equals("trackedHarvesterDiameterLimit", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.TrackedHarvesterDiameterLimit = columnIndex;
                    }
                    else if (columnHeader.Equals("trackedHarvesterLinear", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.TrackedHarvesterLinear = columnIndex;
                    }
                    else if (columnHeader.Equals("trackedHarvesterPMh", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.TrackedHarvesterPMh = columnIndex;
                    }
                    else if (columnHeader.Equals("trackedHarvesterQuadratic1", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.TrackedHarvesterQuadratic1 = columnIndex;
                    }
                    else if (columnHeader.Equals("trackedHarvesterQuadratic2", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.TrackedHarvesterQuadratic2 = columnIndex;
                    }
                    else if (columnHeader.Equals("trackedHarvesterQuadraticThreshold1", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.TrackedHarvesterQuadraticThreshold1 = columnIndex;
                    }
                    else if (columnHeader.Equals("trackedHarvesterQuadraticThreshold2", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.TrackedHarvesterQuadraticThreshold2 = columnIndex;
                    }
                    else if (columnHeader.Equals("trackedHarvesterSlopeThreshold", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.TrackedHarvesterSlopeThreshold = columnIndex;
                    }
                    else if (columnHeader.Equals("wheeledHarvesterConstant", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.WheeledHarvesterConstant = columnIndex;
                    }
                    else if (columnHeader.Equals("wheeledHarvesterDiameterLimit", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.WheeledHarvesterDiameterLimit = columnIndex;
                    }
                    else if (columnHeader.Equals("wheeledHarvesterLinear", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.WheeledHarvesterLinear = columnIndex;
                    }
                    else if (columnHeader.Equals("wheeledHarvesterPMh", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.WheeledHarvesterPMh = columnIndex;
                    }
                    else if (columnHeader.Equals("wheeledHarvesterQuadratic", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.WheeledHarvesterQuadratic = columnIndex;
                    }
                    else if (columnHeader.Equals("wheeledHarvesterQuadraticThreshold", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.WheeledHarvesterQuadraticThreshold = columnIndex;
                    }
                    else if (columnHeader.Equals("wheeledHarvesterSlopeThreshold", StringComparison.OrdinalIgnoreCase))
                    {
                        FinancialScenarios.XlsxColumns.WheeledHarvesterSlopeThreshold = columnIndex;
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
                    else
                    {
                        throw new NotSupportedException("Unknown column header " + columnHeader + ".");
                    }
                }

                // check header
                if (FinancialScenarios.XlsxColumns.ChainsawPMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ChainsawPMh), "Chainsaw crew cost column not found.");
                }
                if (FinancialScenarios.XlsxColumns.ChainsawProductivity < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ChainsawProductivity), "Chainsaw productivity column not found.");
                }
                if (FinancialScenarios.XlsxColumns.CorridorWidth < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.CorridorWidth), "Corridor width column not found.");
                }
                if (FinancialScenarios.XlsxColumns.DiscountRate < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.DiscountRate), "Discount rate column not found.");
                }

                if (FinancialScenarios.XlsxColumns.FellerBuncherConstant < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.FellerBuncherConstant), "Column for feller-buncher felling time intercept not found.");
                }
                if (FinancialScenarios.XlsxColumns.FellerBuncherLinear < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.FellerBuncherLinear), "Column for feller-buncher felling time linear coefficent not found.");
                }
                if (FinancialScenarios.XlsxColumns.FellerBuncherPMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.FellerBuncherPMh), "Feller-buncher cost column not found.");
                }
                if (FinancialScenarios.XlsxColumns.FellerBuncherSlopeThreshold < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.FellerBuncherSlopeThreshold), "Column for onset of slope increases in feller-buncher felling time not found.");
                }

                if (FinancialScenarios.XlsxColumns.ForwarderLoadedTethered < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ForwarderLoadedTethered), "Column for forwarder movement speed when loaded and tethered not found.");
                }
                if (FinancialScenarios.XlsxColumns.ForwarderLoadedUntethered < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ForwarderLoadedUntethered), "Column for forwarder movement speed when loaded and not tethered cost column not found.");
                }
                if (FinancialScenarios.XlsxColumns.ForwarderOnRoad < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ForwarderOnRoad), "Column for forwarder movement on road speed not found.");
                }
                if (FinancialScenarios.XlsxColumns.ForwarderPayload < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ForwarderPayload), "Forwarder payload column not found.");
                }
                if (FinancialScenarios.XlsxColumns.ForwarderPMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ForwarderPMh), "Forwarder cost column not found.");
                }
                if (FinancialScenarios.XlsxColumns.ForwarderUnloadedTethered < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ForwarderUnloadedTethered), "Column for forwarder movement speed when unloaded and tethered not found.");
                }
                if (FinancialScenarios.XlsxColumns.ForwarderUnloadedUntethered < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ForwarderUnloadedUntethered), "Column for forwarder movement speed when unloaded and not tethered column not found.");
                }

                if (FinancialScenarios.XlsxColumns.GrappleYardingConstant < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.GrappleYardingConstant), "Column for grapple yarding turn time intercept not found.");
                }
                if (FinancialScenarios.XlsxColumns.GrappleYardingLinear < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.GrappleYardingLinear), "Column for linear skyline length term in grapple yarding turn time not found.");
                }
                if (FinancialScenarios.XlsxColumns.GrappleSwingYarderMaxPayload < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.GrappleSwingYarderMaxPayload), "Column for grapple yoader maximum payload not found.");
                }
                if (FinancialScenarios.XlsxColumns.GrappleSwingYarderMeanPayload < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.GrappleSwingYarderMeanPayload), "Column for grapple swing yarder mean payload not found.");
                }
                if (FinancialScenarios.XlsxColumns.GrappleSwingYarderSMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.GrappleSwingYarderSMh), "Column for grapple swing yarder operating cost not found.");
                }
                if (FinancialScenarios.XlsxColumns.GrappleSwingYarderUtilization < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.GrappleSwingYarderUtilization), "Column for grapple swing yarder utilization not found.");
                }
                if (FinancialScenarios.XlsxColumns.GrappleYoaderMaxPayload < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.GrappleYoaderMaxPayload), "Column for grapple yoader maximum payload not found.");
                }
                if (FinancialScenarios.XlsxColumns.GrappleYoaderMeanPayload < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.GrappleYoaderMeanPayload), "Column for grapple yoader mean payload not found.");
                }
                if (FinancialScenarios.XlsxColumns.GrappleYoaderSMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.GrappleYoaderSMh), "Column for grapple yoader operating cost not found.");
                }
                if (FinancialScenarios.XlsxColumns.GrappleYoaderUtilization < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.GrappleYoaderUtilization), "Column for grapple yoader utilization not found.");
                }

                if (FinancialScenarios.XlsxColumns.HarvestTaxPerMbf < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.HarvestTaxPerMbf), "Per MBF harvest tax column not found.");
                }
                if (FinancialScenarios.XlsxColumns.LoaderProductivity < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.LoaderProductivity), "Loader productivity column not found.");
                }
                if (FinancialScenarios.XlsxColumns.LoaderSMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.LoaderSMh), "Loader cost column not found.");
                }
                if (FinancialScenarios.XlsxColumns.LoaderUtilization < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.LoaderUtilization), "Column for loader utilization not found.");
                }
                if (FinancialScenarios.XlsxColumns.Name < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.Name), "Scenario name column not found.");
                }

                if (FinancialScenarios.XlsxColumns.ProcessorConstant < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ProcessorConstant), "Column for processor intercept not found.");
                }
                if (FinancialScenarios.XlsxColumns.ProcessorLinear < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ProcessorLinear), "Column for processor linear coefficent not found.");
                }
                if (FinancialScenarios.XlsxColumns.ProcessorQuadratic1 < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ProcessorQuadratic1), "Column for processor first quadratic coefficient not found.");
                }
                if (FinancialScenarios.XlsxColumns.ProcessorQuadratic2 < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ProcessorQuadratic2), "Column for processor second quadratic coefficient not found.");
                }
                if (FinancialScenarios.XlsxColumns.ProcessorQuadraticThreshold1 < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ProcessorQuadraticThreshold1), "Column for first onset of quadratic processor felling time not found.");
                }
                if (FinancialScenarios.XlsxColumns.ProcessorQuadraticThreshold2 < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ProcessorQuadraticThreshold2), "Column for second onset of quadratic processor felling time not found.");
                }
                if (FinancialScenarios.XlsxColumns.ProcessorSMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ProcessorSMh), "Processor cost column not found.");
                }
                if (FinancialScenarios.XlsxColumns.ProcessorUtilization < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ProcessorUtilization), "Column for processor utilization not found.");
                }

                if (FinancialScenarios.XlsxColumns.PropertyTaxAndManagement < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.PropertyTaxAndManagement), "Annual taxes and management column not found.");
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

                if (FinancialScenarios.XlsxColumns.RegenPerHa < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.RegenPerHa), "Per hectare regeneration harvest cost column not found.");
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
                if (FinancialScenarios.XlsxColumns.ThplCamprun < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ThplCamprun), "Western redcedar camprun column not found.");
                }

                if (FinancialScenarios.XlsxColumns.ThinPerHa < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ThinPerHa), "Per hectare thinning cost column not found.");
                }
                if (FinancialScenarios.XlsxColumns.ThinHaulCostPerCubicMeter < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ThinHaulCostPerCubicMeter), "Thinning haul cost per m³ column not found.");
                }
                if (FinancialScenarios.XlsxColumns.ThinPondValueMultiplier < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.ThinPondValueMultiplier), "Thinning pond value multiplier column not found.");
                }
                if (FinancialScenarios.XlsxColumns.TimberAppreciation < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.TimberAppreciation), "Annual timber appreciation rate column not found.");
                }

                if (FinancialScenarios.XlsxColumns.TrackedHarvesterConstant < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.TrackedHarvesterConstant), "Column for tracked harvester felling time intercept not found.");
                }
                if (FinancialScenarios.XlsxColumns.TrackedHarvesterDiameterLimit < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.TrackedHarvesterDiameterLimit), "Column for tracked harvester felling diameter limit not found.");
                }
                if (FinancialScenarios.XlsxColumns.TrackedHarvesterPMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.TrackedHarvesterPMh), "Tracked harvester cost column not found.");
                }
                if (FinancialScenarios.XlsxColumns.TrackedHarvesterLinear < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.TrackedHarvesterLinear), "Column for tracked harvester felling time linear coefficent not found.");
                }
                if (FinancialScenarios.XlsxColumns.TrackedHarvesterQuadratic1 < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.TrackedHarvesterQuadratic1), "Column for tracked harvester felling time first quadratic coefficient not found.");
                }
                if (FinancialScenarios.XlsxColumns.TrackedHarvesterQuadratic2 < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.TrackedHarvesterQuadratic2), "Column for tracked harvester felling time second quadratic coefficient not found.");
                }
                if (FinancialScenarios.XlsxColumns.TrackedHarvesterQuadraticThreshold1 < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.TrackedHarvesterQuadraticThreshold1), "Column for first onset of quadratic tracked harvester felling time not found.");
                }
                if (FinancialScenarios.XlsxColumns.TrackedHarvesterQuadraticThreshold2 < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.TrackedHarvesterQuadraticThreshold2), "Column for second onset of quadratic tracked harvester felling time not found.");
                }
                if (FinancialScenarios.XlsxColumns.TrackedHarvesterSlopeThreshold < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.TrackedHarvesterSlopeThreshold), "Column for onset of slope increases in tracked harvester felling time not found.");
                }

                if (FinancialScenarios.XlsxColumns.WheeledHarvesterConstant < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.WheeledHarvesterConstant), "Column for wheeled harvester felling time intercept not found.");
                }
                if (FinancialScenarios.XlsxColumns.WheeledHarvesterDiameterLimit < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.WheeledHarvesterDiameterLimit), "Column for wheeled harvester felling diameter limit not found.");
                }
                if (FinancialScenarios.XlsxColumns.WheeledHarvesterPMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.WheeledHarvesterPMh), "Wheeled harvester cost column not found.");
                }
                if (FinancialScenarios.XlsxColumns.WheeledHarvesterLinear < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.WheeledHarvesterLinear), "Column for wheeled harvester felling time linear coefficent not found.");
                }
                if (FinancialScenarios.XlsxColumns.WheeledHarvesterQuadratic < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.WheeledHarvesterQuadratic), "Column for wheeled harvester felling time quadratic coefficient not found.");
                }
                if (FinancialScenarios.XlsxColumns.WheeledHarvesterQuadraticThreshold < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.WheeledHarvesterQuadraticThreshold), "Column for onset of quadratic wheeled harvester felling time not found.");
                }
                if (FinancialScenarios.XlsxColumns.WheeledHarvesterSlopeThreshold < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(FinancialScenarios.XlsxColumns.WheeledHarvesterSlopeThreshold), "Column for onset of slope increases in wheeled harvester felling time not found.");
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

                this.DiscountRate.Clear();
                this.DouglasFir2SawPondValuePerMbf.Clear();
                this.DouglasFir3SawPondValuePerMbf.Clear();
                this.DouglasFir4SawPondValuePerMbf.Clear();
                this.HarvestSystems.Clear();
                this.HarvestTaxPerMbf.Clear();
                this.Name.Clear();
                this.PropertyTaxAndManagementPerHectareYear.Clear();
                this.RegenerationHarvestCostPerHectare.Clear();
                this.RegenerationHaulCostPerCubicMeter.Clear();
                this.ReleaseSprayCostPerHectare.Clear();
                this.SeedlingCost.Clear();
                this.SitePrepAndReplantingCostPerHectare.Clear();
                this.ThinningHarvestCostPerHectare.Clear();
                this.ThinningHaulCostPerCubicMeter.Clear();
                this.ThinningPondValueMultiplier.Clear();
                this.TimberAppreciationRate.Clear();
                this.WesternRedcedarCamprunPondValuePerMbf.Clear();
                this.WhiteWood2SawPondValuePerMbf.Clear();
                this.WhiteWood3SawPondValuePerMbf.Clear();
                this.WhiteWood4SawPondValuePerMbf.Clear();

                return;
            }

            // instantiate harvest systems for this scenario
            HarvestSystems harvestSystems = new();
            this.HarvestSystems.Add(harvestSystems);

            // parse and check scenario's values
            harvestSystems.ChainsawPMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ChainsawPMh]);
            if ((harvestSystems.ChainsawPMh < 100.0F) || (harvestSystems.ChainsawPMh > 500.0F))
            {
                throw new NotSupportedException("Chainsaw crew cost is not in the range US$ [100.0, 500.0]/PMh.");
            }
            harvestSystems.ChainsawProductivity = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ChainsawProductivity]);
            if ((harvestSystems.ChainsawProductivity < 0.0F) || (harvestSystems.ChainsawProductivity > 25.0F))
            {
                throw new NotSupportedException("Chainsaw crew productivity is not in the range (0.0, 25.0] m³/PMh.");
            }
            harvestSystems.CorridorWidth = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.CorridorWidth]);
            if ((harvestSystems.CorridorWidth < 1.0F) || (harvestSystems.CorridorWidth > 25.0F))
            {
                throw new NotSupportedException("Equipment corridor width is not in the range [1.0, 25.0].");
            }
            float discountRate = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.DiscountRate]);
            if ((discountRate < 0.0F) || (discountRate > 0.1F))
            {
                throw new NotSupportedException("Annual disount rate is not in the range [0.0, 0.1].");
            }
            this.DiscountRate.Add(discountRate);

            harvestSystems.FellerBuncherFellingConstant = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.FellerBuncherConstant]);
            if ((harvestSystems.FellerBuncherFellingConstant < 0.0F) || (harvestSystems.FellerBuncherFellingConstant > 500.0F))
            {
                throw new NotSupportedException("Intercept of feller-buncher per tree felling time is not in the range [0.0, 500.0] seconds.");
            }
            harvestSystems.FellerBuncherFellingLinear = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.FellerBuncherLinear]);
            if ((harvestSystems.FellerBuncherFellingLinear < 0.0F) || (harvestSystems.FellerBuncherFellingLinear > 250.0F))
            {
                throw new NotSupportedException("Linear coefficient of feller-buncher per tree felling time is not in the range [0.0, 250.0] seconds/m³.");
            }
            harvestSystems.FellerBuncherPMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.FellerBuncherPMh]);
            if ((harvestSystems.FellerBuncherPMh < 0.0F) || (harvestSystems.FellerBuncherPMh > 1000.0F))
            {
                throw new NotSupportedException("Feller-buncher operating cost is not in the range US$ [0.0, 1000.0]/PMh.");
            }
            harvestSystems.FellerBuncherSlopeThresholdInPercent = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.FellerBuncherSlopeThreshold]);
            if ((harvestSystems.FellerBuncherSlopeThresholdInPercent < 0.0F) || (harvestSystems.FellerBuncherSlopeThresholdInPercent > 200.0F))
            {
                throw new NotSupportedException("Onset of slope effects on feller-buncher felling time is not in the range [0.0, 200.0] %.");
            }

            harvestSystems.ForwarderPayloadInKg = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderPayload]);
            if ((harvestSystems.ForwarderPayloadInKg < 1000.0F) || (harvestSystems.ForwarderPayloadInKg > 30000.0F))
            {
                throw new NotSupportedException("Forwarder payload is not in the range [1000.0, 30000.0] kg.");
            }
            harvestSystems.ForwarderPMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderPMh]);
            if ((harvestSystems.ForwarderPMh < 0.0F) || (harvestSystems.ForwarderPMh > 1000.0F))
            {
                throw new NotSupportedException("Forwarder operating cost is not in the range US$ [0.0, 1000.0]/PMh.");
            }
            harvestSystems.ForwarderSpeedInStandLoadedTethered = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderLoadedTethered]);
            if ((harvestSystems.ForwarderSpeedInStandLoadedTethered <= 0.0F) || (harvestSystems.ForwarderSpeedInStandLoadedTethered > 100.0F))
            {
                throw new NotSupportedException("Forwarder loaded travel speed while tethered is not in the range (0.0, 100.0] m/min.");
            }
            harvestSystems.ForwarderSpeedInStandLoadedUntethered = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderLoadedUntethered]);
            if ((harvestSystems.ForwarderSpeedInStandLoadedUntethered <= 0.0F) || (harvestSystems.ForwarderSpeedInStandLoadedUntethered > 100.0F))
            {
                throw new NotSupportedException("Forwarder loaded travel speed without a tether is not in the range (0.0, 100.0] m/min.");
            }
            harvestSystems.ForwarderSpeedInStandUnloadedTethered = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderUnloadedTethered]);
            if ((harvestSystems.ForwarderSpeedInStandUnloadedTethered <= 0.0F) || (harvestSystems.ForwarderSpeedInStandUnloadedTethered > 100.0F))
            {
                throw new NotSupportedException("Forwarder unloaded travel speed while tethered is not in the range (0.0, 100.0] m/min.");
            }
            harvestSystems.ForwarderSpeedInStandUnloadedUntethered = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderUnloadedUntethered]);
            if ((harvestSystems.ForwarderSpeedInStandUnloadedUntethered <= 0.0F) || (harvestSystems.ForwarderSpeedInStandUnloadedUntethered > 100.0F))
            {
                throw new NotSupportedException("Forwarder unloaded travel speed without a tether is not in the range (0.0, 100.0] m/min.");
            }
            harvestSystems.ForwarderSpeedOnRoad = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderOnRoad]);
            if ((harvestSystems.ForwarderSpeedOnRoad <= 0.0F) || (harvestSystems.ForwarderSpeedOnRoad > 100.0F))
            {
                throw new NotSupportedException("Forwarder travel speed on roads is not in the range (0.0, 100.0] m/min.");
            }

            harvestSystems.GrappleYardingConstant = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleYardingConstant]);
            if ((harvestSystems.GrappleYardingConstant <= 0.0F) || (harvestSystems.GrappleYardingConstant > 500.0F))
            {
                throw new NotSupportedException("Grapple yarding turn time constant is not in the range (0.0, 500.0] seconds.");
            }
            harvestSystems.GrappleYardingLinear = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleYardingLinear]);
            if ((harvestSystems.GrappleYardingLinear <= 0.0F) || (harvestSystems.GrappleYardingLinear > 5.0F))
            {
                throw new NotSupportedException("Grapple yarding turn time per meter of skyline length is not in the range (0.0, 5.0] seconds.");
            }
            harvestSystems.GrappleSwingYarderMaxPayload = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleSwingYarderMaxPayload]);
            if ((harvestSystems.GrappleSwingYarderMaxPayload < 500.0F) || (harvestSystems.GrappleSwingYarderMaxPayload > 8000.0F))
            {
                throw new NotSupportedException("Grapple swing yarder maximum payload is not in the range of [500.0, 8000.0] kg.");
            }
            harvestSystems.GrappleSwingYarderMeanPayload = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleSwingYarderMeanPayload]);
            if ((harvestSystems.GrappleSwingYarderMeanPayload < 100.0F) || (harvestSystems.GrappleSwingYarderMeanPayload > 4500.0F))
            {
                throw new NotSupportedException("Grapple swing yarder mean payload is not in the range of [100.0, 4500.0] kg.");
            }
            harvestSystems.GrappleSwingYarderSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleSwingYarderSMh]);
            if ((harvestSystems.GrappleSwingYarderSMh < 100.0F) || (harvestSystems.GrappleSwingYarderSMh > 500.0F))
            {
                throw new NotSupportedException("Grapple swing yarder operating cost is not in the range of US$ [100.0, 500.0]/PMh.");
            }
            harvestSystems.GrappleSwingYarderUtilization = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleSwingYarderUtilization]);
            if ((harvestSystems.GrappleSwingYarderUtilization <= 0.0F) || (harvestSystems.GrappleSwingYarderUtilization > 1.0F))
            {
                throw new NotSupportedException("Grapple swing yarder utilization is not in the range of (0.0, 1.0].");
            }
            harvestSystems.GrappleYoaderMaxPayload = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleYoaderMaxPayload]);
            if ((harvestSystems.GrappleYoaderMaxPayload < 500.0F) || (harvestSystems.GrappleYoaderMaxPayload > 8000.0F))
            {
                throw new NotSupportedException("Grapple yoader maximum payload is not in the range of [500.0, 8000.0] kg.");
            }
            harvestSystems.GrappleYoaderMeanPayload = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleYoaderMeanPayload]);
            if ((harvestSystems.GrappleYoaderMeanPayload < 100.0F) || (harvestSystems.GrappleYoaderMeanPayload > 4500.0F))
            {
                throw new NotSupportedException("Grapple yoader mean payload is not in the range of [100.0, 4500.0] kg.");
            }
            harvestSystems.GrappleYoaderSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleYoaderSMh]);
            if ((harvestSystems.GrappleYoaderSMh < 100.0F) || (harvestSystems.GrappleYoaderSMh > 500.0F))
            {
                throw new NotSupportedException("Grapple yoader operating cost is not in the range of US$ [100.0, 500.0]/PMh.");
            }
            harvestSystems.GrappleYoaderUtilization = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleYoaderUtilization]);
            if ((harvestSystems.GrappleYoaderUtilization <= 0.0F) || (harvestSystems.GrappleYoaderUtilization > 1.0F))
            {
                throw new NotSupportedException("Grapple yoader utilization is not in the range of (0.0, 1.0].");
            }

            float harvestTaxMbf = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.HarvestTaxPerMbf]);
            if ((harvestTaxMbf < 0.0F) || (harvestTaxMbf > 100.0F))
            {
                throw new NotSupportedException("Per MBF harvest tax is not in the range US$ [0.0, 100.0]/MBF.");
            }
            this.HarvestTaxPerMbf.Add(harvestTaxMbf);

            harvestSystems.LoaderProductivity = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.LoaderProductivity]);
            if ((harvestSystems.LoaderProductivity < 0.0F) || (harvestSystems.LoaderProductivity > 200.0F))
            {
                throw new NotSupportedException("Loader productivity is not in the range [0.0, 200.0] m³/PMh.");
            }
            harvestSystems.LoaderSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.LoaderSMh]);
            if ((harvestSystems.LoaderSMh < 0.0F) || (harvestSystems.LoaderSMh > 1000.0F))
            {
                throw new NotSupportedException("Loader operating cost is not in the range US$ [0.0, 1000.0]/PMh.");
            }
            harvestSystems.LoaderUtilization = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.LoaderUtilization]);
            if ((harvestSystems.LoaderUtilization <= 0.0F) || (harvestSystems.LoaderUtilization > 1.0F))
            {
                throw new NotSupportedException("Loader utilization is not in the range of (0.0, 1.0].");
            }

            string name = rowAsStrings[FinancialScenarios.XlsxColumns.Name];
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new NotSupportedException("Financial scenario's name is missing or contains only whitespace.");
            }
            this.Name.Add(name);

            harvestSystems.ProcessorConstant = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ProcessorConstant]);
            if ((harvestSystems.ProcessorConstant < 0.0F) || (harvestSystems.ProcessorConstant > 500.0F))
            {
                throw new NotSupportedException("Intercept of processing time is not in the range [0.0, 500.0] seconds.");
            }
            harvestSystems.ProcessorLinear = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ProcessorLinear]);
            if ((harvestSystems.ProcessorLinear < 0.0F) || (harvestSystems.ProcessorLinear > 250.0F))
            {
                throw new NotSupportedException("Linear coefficient of processing time is not in the range [0.0, 250.0] seconds/m³.");
            }
            harvestSystems.ProcessorQuadratic1 = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ProcessorQuadratic1]);
            if ((harvestSystems.ProcessorQuadratic1 < 0.0F) || (harvestSystems.ProcessorQuadratic1 > 30.0F))
            {
                throw new NotSupportedException("First quadratic coefficient of processing time is not in the range [0.0, 30.0] seconds/m⁶.");
            }
            harvestSystems.ProcessorQuadratic2 = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ProcessorQuadratic2]);
            if ((harvestSystems.ProcessorQuadratic2 < 0.0F) || (harvestSystems.ProcessorQuadratic2 > 30.0F))
            {
                throw new NotSupportedException("Second quadratic coefficient of processing is not in the range [0.0, 30.0] seconds/m⁶.");
            }
            harvestSystems.ProcessorQuadraticThreshold1 = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ProcessorQuadraticThreshold1]);
            if ((harvestSystems.ProcessorQuadraticThreshold1 <= 0.0F) || (harvestSystems.ProcessorQuadraticThreshold1 > 20.0F))
            {
                throw new NotSupportedException("Onset of first quadratic increase in processing time is not in the range (0.0, 20.0] m³.");
            }
            harvestSystems.ProcessorQuadraticThreshold2 = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ProcessorQuadraticThreshold2]);
            if ((harvestSystems.ProcessorQuadraticThreshold2 <= 0.0F) || (harvestSystems.ProcessorQuadraticThreshold2 > 20.0F))
            {
                throw new NotSupportedException("Onset of second quadratic increase in processing time is not in the range (0.0, 20.0] m³.");
            }
            harvestSystems.ProcessorSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ProcessorSMh]);
            if ((harvestSystems.ProcessorSMh < 0.0F) || (harvestSystems.ProcessorSMh > 1000.0F))
            {
                throw new NotSupportedException("Processor operating cost is not in the range US$ [0.0, 1000.0]/PMh.");
            }
            harvestSystems.ProcessorUtilization = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ProcessorUtilization]);
            if ((harvestSystems.ProcessorUtilization <= 0.0F) || (harvestSystems.LoaderUtilization > 1.0F))
            {
                throw new NotSupportedException("Processor utilization is not in the range of (0.0, 1.0].");
            }

            float propertyTaxAndManagement = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.PropertyTaxAndManagement]);
            if ((propertyTaxAndManagement <= 0.0F) || (propertyTaxAndManagement > 100.0F))
            {
                throw new NotSupportedException("Annual property tax and management cost is not in the range US$ (0.0, 100.0]/ha.");
            }
            this.PropertyTaxAndManagementPerHectareYear.Add(propertyTaxAndManagement);

            float psme2Spond = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.Psme2SPond]);
            if ((psme2Spond < 100.0F) || (psme2Spond > 1000.0F))
            {
                throw new NotSupportedException("Douglas-fir 2S pond value is not in the range US$ [100.0, 1000.0]/MBF.");
            }
            this.DouglasFir2SawPondValuePerMbf.Add(psme2Spond);

            float psme3Spond = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.Psme3SPond]);
            if ((psme3Spond < 100.0F) || (psme3Spond > 1000.0F))
            {
                throw new NotSupportedException("Douglas-fir 3S pond value is not in the range US$ [100.0, 1000.0]/MBF.");
            }
            this.DouglasFir3SawPondValuePerMbf.Add(psme3Spond);

            float psme4Spond = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.Psme4SPond]);
            if ((psme4Spond < 100.0F) || (psme4Spond > 1000.0F))
            {
                throw new NotSupportedException("Douglas-fir 4S pond value is not in the range US$ [100.0, 1000.0]/MBF.");
            }
            this.DouglasFir4SawPondValuePerMbf.Add(psme4Spond);

            float regenHarvestPerHectare = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.RegenPerHa]);
            if ((regenHarvestPerHectare <= 0.0F) || (regenHarvestPerHectare > 1000.0F))
            {
                throw new NotSupportedException("Regeneration harvest cost is not in the range US$ (0.0, 1000.0]/ha.");
            }
            this.RegenerationHarvestCostPerHectare.Add(regenHarvestPerHectare);

            float regenHarvestHaulCost = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.RegenHaulPerCubicMeter]);
            if ((regenHarvestHaulCost <= 0.0F) || (regenHarvestHaulCost > 100.0F))
            {
                throw new NotSupportedException("Regeneration harvest haul cost is not in the range US$ (0.0, 100.0]/m³.");
            }
            this.RegenerationHaulCostPerCubicMeter.Add(regenHarvestHaulCost);

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

            float sitePrepAndReplanting = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.SitePrepFixed]);
            if ((sitePrepAndReplanting <= 0.0F) || (sitePrepAndReplanting > 1000.0F))
            {
                throw new NotSupportedException("Site preparation and replanting cost is not in the range US$ (0.0, 1000.0]/ha.");
            }
            this.SitePrepAndReplantingCostPerHectare.Add(sitePrepAndReplanting);

            float thinHectare = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ThinPerHa]);
            if ((thinHectare <= 0.0F) || (thinHectare > 1000.0F))
            {
                throw new NotSupportedException("Thinning harvest cost is not in the range US$ (0.0, 1000.0]/ha.");
            }
            this.ThinningHarvestCostPerHectare.Add(thinHectare);

            float thinHaulCost = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ThinHaulCostPerCubicMeter]);
            if ((thinHaulCost <= 0.0F) || (thinHaulCost > 100.0F))
            {
                throw new NotSupportedException("Thinning haul cost is not in the range US$ (0.0, 100.0]/m³.");
            }
            this.ThinningHaulCostPerCubicMeter.Add(thinHaulCost);

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

            harvestSystems.TrackedHarvesterDiameterLimit = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterDiameterLimit]);
            if ((harvestSystems.TrackedHarvesterDiameterLimit < 30.0F) || (harvestSystems.TrackedHarvesterDiameterLimit > 100.0F))
            {
                throw new NotSupportedException("Intercept of tracked harvester's diameter limit is not in the range [30.0, 100.0] cm.");
            }
            harvestSystems.TrackedHarvesterFellingConstant = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterConstant]);
            if ((harvestSystems.TrackedHarvesterFellingConstant < 0.0F) || (harvestSystems.TrackedHarvesterFellingConstant > 500.0F))
            {
                throw new NotSupportedException("Intercept of tracked harvester felling and processing time is not in the range [0.0, 500.0] seconds.");
            }
            harvestSystems.TrackedHarvesterFellingLinear = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterLinear]);
            if ((harvestSystems.TrackedHarvesterFellingLinear < 0.0F) || (harvestSystems.TrackedHarvesterFellingLinear > 250.0F))
            {
                throw new NotSupportedException("Linear coefficient of tracked harvester felling and processing time is not in the range [0.0, 250.0] seconds/m³.");
            }
            harvestSystems.TrackedHarvesterFellingQuadratic1 = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterQuadratic1]);
            if ((harvestSystems.TrackedHarvesterFellingQuadratic1 < 0.0F) || (harvestSystems.TrackedHarvesterFellingQuadratic1 > 30.0F))
            {
                throw new NotSupportedException("First quadratic coefficient of tracked harvester felling and processing time is not in the range [0.0, 30.0] seconds/m⁶.");
            }
            harvestSystems.TrackedHarvesterFellingQuadratic2 = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterQuadratic2]);
            if ((harvestSystems.TrackedHarvesterFellingQuadratic2 < 0.0F) || (harvestSystems.TrackedHarvesterFellingQuadratic2 > 30.0F))
            {
                throw new NotSupportedException("Second quadratic coefficient of tracked harvester felling and processing time is not in the range [0.0, 30.0] seconds/m⁶.");
            }
            harvestSystems.TrackedHarvesterPMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterPMh]);
            if ((harvestSystems.TrackedHarvesterPMh < 0.0F) || (harvestSystems.TrackedHarvesterPMh > 1000.0F))
            {
                throw new NotSupportedException("Tracked harvester operating cost is not in the range US$ [0.0, 1000.0]/PMh.");
            }
            harvestSystems.TrackedHarvesterQuadraticThreshold1 = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterQuadraticThreshold1]);
            if ((harvestSystems.TrackedHarvesterQuadraticThreshold1 <= 0.0F) || (harvestSystems.TrackedHarvesterQuadraticThreshold1 > 20.0F))
            {
                throw new NotSupportedException("Onset of first quadratic increase in tracked harvester felling and processing time is not in the range (0.0, 20.0] m³.");
            }
            harvestSystems.TrackedHarvesterQuadraticThreshold2 = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterQuadraticThreshold2]);
            if ((harvestSystems.TrackedHarvesterQuadraticThreshold2 <= 0.0F) || (harvestSystems.TrackedHarvesterQuadraticThreshold2 > 20.0F))
            {
                throw new NotSupportedException("Onset of second quadratic increase in tracked harvester felling and processing time is not in the range (0.0, 20.0] m³.");
            }
            harvestSystems.TrackedHarvesterSlopeThresholdInPercent = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterSlopeThreshold]);
            if ((harvestSystems.TrackedHarvesterSlopeThresholdInPercent < 0.0F) || (harvestSystems.TrackedHarvesterSlopeThresholdInPercent > 200.0F))
            {
                throw new NotSupportedException("Onset of slope effects on tracked harvester felling and processing time is not in the range [0.0, 200.0] %.");
            }

            harvestSystems.WheeledHarvesterDiameterLimit = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.WheeledHarvesterDiameterLimit]);
            if ((harvestSystems.WheeledHarvesterDiameterLimit < 30.0F) || (harvestSystems.WheeledHarvesterDiameterLimit > 100.0F))
            {
                throw new NotSupportedException("Intercept of wheeled harvester's diameter limit is not in the range [30.0, 100.0] cm.");
            }
            harvestSystems.WheeledHarvesterFellingConstant = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.WheeledHarvesterConstant]);
            if ((harvestSystems.WheeledHarvesterFellingConstant < 0.0F) || (harvestSystems.WheeledHarvesterFellingConstant > 500.0F))
            {
                throw new NotSupportedException("Intercept of wheeled harvester felling and processing time is not in the range [0.0, 500.0] seconds.");
            }
            harvestSystems.WheeledHarvesterFellingLinear = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.WheeledHarvesterLinear]);
            if ((harvestSystems.WheeledHarvesterFellingLinear < 0.0F) || (harvestSystems.WheeledHarvesterFellingLinear > 250.0F))
            {
                throw new NotSupportedException("Linear coefficient of wheeled harvester felling and processing time is not in the range [0.0, 250.0] seconds/m³.");
            }
            harvestSystems.WheeledHarvesterFellingQuadratic = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.WheeledHarvesterQuadratic]);
            if ((harvestSystems.WheeledHarvesterFellingQuadratic < 0.0F) || (harvestSystems.WheeledHarvesterFellingQuadratic > 30.0F))
            {
                throw new NotSupportedException("Quadratic coefficient of wheeled harvester felling and processing time is not in the range [0.0, 30.0] seconds/m⁶.");
            }
            harvestSystems.WheeledHarvesterPMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.WheeledHarvesterPMh]);
            if ((harvestSystems.WheeledHarvesterPMh < 0.0F) || (harvestSystems.WheeledHarvesterPMh > 1000.0F))
            {
                throw new NotSupportedException("Wheeled harvester operating cost is not in the range US$ [0.0, 1000.0]/PMh.");
            }
            harvestSystems.WheeledHarvesterQuadraticThreshold = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.WheeledHarvesterQuadraticThreshold]);
            if ((harvestSystems.WheeledHarvesterQuadraticThreshold <= 0.0F) || (harvestSystems.WheeledHarvesterQuadraticThreshold > 20.0F))
            {
                throw new NotSupportedException("Onset of quadratic increase in wheeled harvester felling and processing time is not in the range (0.0, 20.0] m³.");
            }
            harvestSystems.WheeledHarvesterSlopeThresholdInPercent = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.WheeledHarvesterSlopeThreshold]);
            if ((harvestSystems.WheeledHarvesterSlopeThresholdInPercent < 0.0F) || (harvestSystems.WheeledHarvesterSlopeThresholdInPercent > 200.0F))
            {
                throw new NotSupportedException("Onset of slope effects on wheeled harvester felling and processing time is not in the range [0.0, 200.0] %.");
            }

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
            Debug.Assert(this.Count == this.HarvestSystems.Count);
            Debug.Assert(this.Count == this.HarvestTaxPerMbf.Count);
            Debug.Assert(this.Count == this.Name.Count);
            Debug.Assert(this.Count == this.ReleaseSprayCostPerHectare.Count);
            Debug.Assert(this.Count == this.RegenerationHaulCostPerCubicMeter.Count);
            Debug.Assert(this.Count == this.RegenerationHarvestCostPerHectare.Count);
            Debug.Assert(this.Count == this.SeedlingCost.Count);
            Debug.Assert(this.Count == this.SitePrepAndReplantingCostPerHectare.Count);
            Debug.Assert(this.Count == this.PropertyTaxAndManagementPerHectareYear.Count);
            Debug.Assert(this.Count == this.ThinningHarvestCostPerHectare.Count);
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
            public int ChainsawPMh { get; set; }
            public int ChainsawProductivity { get; set; }
            public int CorridorWidth { get; set; }
            public int DiscountRate { get; set; }

            public int FellerBuncherConstant { get; set; }
            public int FellerBuncherPMh { get; set; }
            public int FellerBuncherLinear { get; set; }
            public int FellerBuncherSlopeThreshold { get; set; }

            public int ForwarderLoadedTethered { get; set; }
            public int ForwarderLoadedUntethered { get; set; }
            public int ForwarderOnRoad { get; set; }
            public int ForwarderPayload { get; set; }
            public int ForwarderPMh { get; set; }
            public int ForwarderUnloadedTethered { get; set; }
            public int ForwarderUnloadedUntethered { get; set; }

            public int GrappleYardingConstant { get; set; }
            public int GrappleYardingLinear { get; set; }
            public int GrappleSwingYarderMaxPayload { get; set; }
            public int GrappleSwingYarderMeanPayload { get; set; }
            public int GrappleSwingYarderSMh { get; set; }
            public int GrappleSwingYarderUtilization { get; set; }
            public int GrappleYoaderMaxPayload { get; set; }
            public int GrappleYoaderMeanPayload { get; set; }
            public int GrappleYoaderSMh { get; set; }
            public int GrappleYoaderUtilization { get; set; }

            public int HarvestTaxPerMbf { get; set; }
            public int LoaderProductivity { get; set; }
            public int LoaderSMh { get; set; }
            public int LoaderUtilization { get; set; }
            public int Name { get; set; }
            
            public int ProcessorConstant { get; set; }
            public int ProcessorLinear { get; set; }
            public int ProcessorQuadratic1 { get; set; }
            public int ProcessorQuadratic2 { get; set; }
            public int ProcessorQuadraticThreshold1 { get; set; }
            public int ProcessorQuadraticThreshold2 { get; set; }
            public int ProcessorSMh { get; set; }
            public int ProcessorUtilization { get; set; }

            public int PropertyTaxAndManagement { get; set; }
            
            public int Psme2SPond { get; set; }
            public int Psme3SPond { get; set; }
            public int Psme4SPond { get; set; }
            
            public int RegenPerHa { get; set; }
            public int RegenHaulPerCubicMeter { get; set; }
            public int ReleaseSpray { get; set; }
            public int Seedling { get; set; }
            public int SitePrepFixed { get; set; }
            
            public int ThplCamprun { get; set; }
            
            public int ThinPerHa { get; set; }
            public int ThinHaulCostPerCubicMeter { get; set; }
            public int ThinPondValueMultiplier { get; set; }
            public int TimberAppreciation { get; set; }

            public int TrackedHarvesterConstant { get; set; }
            public int TrackedHarvesterDiameterLimit { get; set; }
            public int TrackedHarvesterLinear { get; set; }
            public int TrackedHarvesterPMh { get; set; }
            public int TrackedHarvesterQuadratic1 { get; set; }
            public int TrackedHarvesterQuadratic2 { get; set; }
            public int TrackedHarvesterQuadraticThreshold1 { get; set; }
            public int TrackedHarvesterQuadraticThreshold2 { get; set; }
            public int TrackedHarvesterSlopeThreshold { get; set; }

            public int WheeledHarvesterConstant { get; set; }
            public int WheeledHarvesterDiameterLimit { get; set; }
            public int WheeledHarvesterLinear { get; set; }
            public int WheeledHarvesterPMh { get; set; }
            public int WheeledHarvesterQuadratic { get; set; }
            public int WheeledHarvesterQuadraticThreshold { get; set; }
            public int WheeledHarvesterSlopeThreshold { get; set; }

            public int White2SPond { get; set; }
            public int White3SPond { get; set; }
            public int White4SPond { get; set; }

            public XlsxColumnIndices()
            {
                this.Reset();
            }

            public void Reset()
            {
                this.ChainsawPMh = -1;
                this.ChainsawProductivity = -1;
                this.CorridorWidth = -1;
                this.DiscountRate = -1;

                this.FellerBuncherConstant = -1;
                this.FellerBuncherLinear = -1;
                this.FellerBuncherPMh = -1;
                this.FellerBuncherSlopeThreshold = -1;
                
                this.ForwarderLoadedTethered = -1;
                this.ForwarderLoadedUntethered = -1;
                this.ForwarderOnRoad = -1;
                this.ForwarderPayload = -1;
                this.ForwarderPMh = -1;
                this.ForwarderUnloadedTethered = -1;
                this.ForwarderUnloadedUntethered = -1;
                
                this.GrappleYardingConstant = -1;
                this.GrappleYardingLinear = -1;
                this.GrappleSwingYarderMaxPayload = -1;
                this.GrappleSwingYarderMeanPayload = -1;
                this.GrappleSwingYarderSMh = -1;
                this.GrappleSwingYarderUtilization = -1;
                this.GrappleYoaderMaxPayload = -1;
                this.GrappleYoaderMeanPayload = -1;
                this.GrappleYoaderSMh = -1;
                this.GrappleYoaderUtilization = -1;

                this.HarvestTaxPerMbf = -1;
                this.LoaderProductivity = -1;
                this.LoaderSMh = -1;
                this.LoaderUtilization = -1;
                this.Name = -1;
                
                this.ProcessorLinear = -1;
                this.ProcessorQuadratic1 = -1;
                this.ProcessorQuadratic2 = -1;
                this.ProcessorQuadraticThreshold1 = -1;
                this.ProcessorQuadraticThreshold2 = -1;
                this.ProcessorSMh = -1;
                this.ProcessorUtilization = -1;

                this.PropertyTaxAndManagement = -1;
                
                this.Psme2SPond = -1;
                this.Psme3SPond = -1;
                this.Psme4SPond = -1;
                
                this.RegenPerHa = -1;
                this.RegenHaulPerCubicMeter = -1;
                this.ReleaseSpray = -1;
                this.Seedling = -1;
                this.SitePrepFixed = -1;
                
                this.ThinPerHa = -1;
                this.ThinHaulCostPerCubicMeter = -1;
                this.ThplCamprun = -1;
                this.TimberAppreciation = -1;
                this.ThinPondValueMultiplier = -1;

                this.TrackedHarvesterConstant = -1;
                this.TrackedHarvesterDiameterLimit = -1;
                this.TrackedHarvesterLinear = -1;
                this.TrackedHarvesterPMh = -1;
                this.TrackedHarvesterQuadratic1 = -1;
                this.TrackedHarvesterQuadratic2 = -1;
                this.TrackedHarvesterQuadraticThreshold1 = -1;
                this.TrackedHarvesterQuadraticThreshold2 = -1;
                this.TrackedHarvesterSlopeThreshold = -1;

                this.WheeledHarvesterConstant = -1;
                this.WheeledHarvesterDiameterLimit = -1;
                this.WheeledHarvesterLinear = -1;
                this.WheeledHarvesterPMh = -1;
                this.WheeledHarvesterQuadratic = -1;
                this.WheeledHarvesterQuadraticThreshold = -1;
                this.WheeledHarvesterSlopeThreshold = -1;

                this.White2SPond = -1;
                this.White3SPond = -1;
                this.White4SPond = -1;
            }
        }
    }
}
