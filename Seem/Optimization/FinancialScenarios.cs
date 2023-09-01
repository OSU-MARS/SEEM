using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Mars.Seem.Optimization
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
        public IList<float> RedAlder2SawPondValuePerMbf { get; private init; }
        public IList<float> RedAlder3SawPondValuePerMbf { get; private init; }
        public IList<float> RedAlder4SawPondValuePerMbf { get; private init; }
        public IList<float> RegenerationHarvestCostPerHectare { get; private init; }
        public IList<float> RegenerationRoadCostPerCubicMeter { get; private init; }
        public IList<float> RegenerationSlashCostPerCubicMeter { get; private init; }
        public IList<float> ReleaseSprayCostPerHectare { get; private init; }
        public IList<float> SeedlingCost { get; private init; }
        public IList<float> ShortLogPondValueMultiplier { get; private init; }
        public IList<float> SitePrepAndReplantingCostPerHectare { get; private init; }
        public IList<float> ThinningHarvestCostPerHectare { get; private init; }
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
            //   adjusted by US Bureau of Labor Statistics seasonally unadjusted monthly PPI to 2020 dollars.
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
            this.HarvestTaxPerMbf = new List<float>() { Constant.HarvestCost.OregonForestProductsHarvestTax };
            this.Name = new List<string>() { "default" };
            this.PropertyTaxAndManagementPerHectareYear = new List<float>() { Constant.HarvestCost.ForestlandPropertyTaxRate * Constant.HarvestCost.AssessedValue + Constant.HarvestCost.AdmininistrationCost }; // US$/ha-year
            this.RedAlder2SawPondValuePerMbf = new List<float>() { 721 }; // US$/MBF 2S
            this.RedAlder3SawPondValuePerMbf = new List<float>() { 680 }; // US$/MBF 3S
            this.RedAlder4SawPondValuePerMbf = new List<float>() { 420 }; // US$/MBF 4S
            this.RegenerationHarvestCostPerHectare = new List<float>() { Constant.HarvestCost.TimberCruisePerHectare + Constant.HarvestCost.TimberSaleAdministrationPerHectare + Constant.HarvestCost.RoadReopening + Constant.HarvestCost.BrushControl }; // US$/ha
            this.RegenerationRoadCostPerCubicMeter = new List<float>() { Constant.HarvestCost.RoadMaintenance }; // US$/m³
            this.RegenerationSlashCostPerCubicMeter = new List<float>() { Constant.HarvestCost.SlashDisposal + Constant.HarvestCost.YarderLandingSlashDisposal }; // US$/m³
            this.ReleaseSprayCostPerHectare = new List<float>() { Constant.HarvestCost.ReleaseSpray }; // US$/ha, one release spray
            this.SeedlingCost = new List<float>() { 0.50F }; // US$ per seedling, make species specific when needed
            this.ShortLogPondValueMultiplier = new List<float>() { Constant.Default.ThinningPondValueMultiplier }; // short log price penalty
            this.SitePrepAndReplantingCostPerHectare = new List<float>() { Constant.HarvestCost.SitePrep + Constant.HarvestCost.PlantingLabor }; // US$/ha: site prep + planting labor, cost of seedlings not included
            this.ThinningHarvestCostPerHectare = new List<float>() { Constant.HarvestCost.TimberCruisePerHectare + Constant.HarvestCost.TimberSaleAdministrationPerHectare + Constant.HarvestCost.RoadReopening + Constant.HarvestCost.BrushControl }; // US$/ha
            this.ThinningRoadCostPerCubicMeter = new List<float>() { Constant.HarvestCost.RoadMaintenance }; // US$/m³
            this.ThinningSlashCostPerCubicMeter = new List<float>() { Constant.HarvestCost.SlashDisposal }; // US$/m³
            this.TimberAppreciationRate = new List<float>() { 0.0F }; // per year
            this.WesternRedcedarCamprunPondValuePerMbf = new List<float>() { 1238.00F }; // US$/MBF
            this.WhiteWood2SawPondValuePerMbf = new List<float>() { 531.00F }; // US$/MBF 2S
            this.WhiteWood3SawPondValuePerMbf = new List<float>() { 525.00F }; // US$/MBF 3S
            this.WhiteWood4SawPondValuePerMbf = new List<float>() { 454.00F }; // US$/MBF 4S/CNS
        }

        public int Count
        {
            get { return this.DiscountRate.Count; }
        }

        public FinancialScenarios Filter(string name)
        {
            for (int financialIndex = 0; financialIndex < this.Count; ++financialIndex)
            {
                if (String.Equals(this.Name[financialIndex], name, StringComparison.Ordinal))
                {
                    FinancialScenarios match = new();
                    match.DiscountRate[0] = this.DiscountRate[financialIndex];
                    match.DouglasFir2SawPondValuePerMbf[0] = this.DouglasFir2SawPondValuePerMbf[financialIndex];
                    match.DouglasFir3SawPondValuePerMbf[0] = this.DouglasFir3SawPondValuePerMbf[financialIndex];
                    match.DouglasFir4SawPondValuePerMbf[0] = this.DouglasFir4SawPondValuePerMbf[financialIndex];
                    match.HarvestSystems[0] = this.HarvestSystems[financialIndex];
                    match.HarvestTaxPerMbf[0] = this.HarvestTaxPerMbf[financialIndex];
                    match.Name[0] = this.Name[financialIndex];
                    match.PropertyTaxAndManagementPerHectareYear[0] = this.PropertyTaxAndManagementPerHectareYear[financialIndex];
                    match.RedAlder2SawPondValuePerMbf[0] = this.RedAlder2SawPondValuePerMbf[financialIndex];
                    match.RedAlder3SawPondValuePerMbf[0] = this.RedAlder3SawPondValuePerMbf[financialIndex];
                    match.RedAlder4SawPondValuePerMbf[0] = this.RedAlder4SawPondValuePerMbf[financialIndex];
                    match.RegenerationHarvestCostPerHectare[0] = this.RegenerationHarvestCostPerHectare[financialIndex];
                    match.RegenerationRoadCostPerCubicMeter[0] = this.RegenerationRoadCostPerCubicMeter[financialIndex];
                    match.RegenerationSlashCostPerCubicMeter[0] = this.RegenerationSlashCostPerCubicMeter[financialIndex];
                    match.ReleaseSprayCostPerHectare[0] = this.ReleaseSprayCostPerHectare[financialIndex];
                    match.SeedlingCost[0] = this.SeedlingCost[financialIndex];
                    match.ShortLogPondValueMultiplier[0] = this.ShortLogPondValueMultiplier[financialIndex];
                    match.SitePrepAndReplantingCostPerHectare[0] = this.SitePrepAndReplantingCostPerHectare[financialIndex];
                    match.ThinningHarvestCostPerHectare[0] = this.ThinningHarvestCostPerHectare[financialIndex];
                    match.ThinningRoadCostPerCubicMeter[0] = this.ThinningRoadCostPerCubicMeter[financialIndex];
                    match.ThinningSlashCostPerCubicMeter[0] = this.ThinningSlashCostPerCubicMeter[financialIndex];
                    match.TimberAppreciationRate[0] = this.TimberAppreciationRate[financialIndex];
                    match.WesternRedcedarCamprunPondValuePerMbf[0] = this.WesternRedcedarCamprunPondValuePerMbf[financialIndex];
                    match.WhiteWood2SawPondValuePerMbf[0] = this.WhiteWood2SawPondValuePerMbf[financialIndex];
                    match.WhiteWood3SawPondValuePerMbf[0] = this.WhiteWood3SawPondValuePerMbf[financialIndex];
                    match.WhiteWood4SawPondValuePerMbf[0] = this.WhiteWood4SawPondValuePerMbf[financialIndex];
                    return match;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(name));
        }

        public float GetAppreciationFactor(int financialIndex, int years)
        {
            float discountRate = this.DiscountRate[financialIndex];
            return MathF.Pow(1.0F + discountRate, years);
        }

        /// <summary>
        /// Get net present value of large landowner amoritized reforestation costs at stand age zero.
        /// </summary>
        /// <returns>Net present value of amoritized reforestation costs at stand age zero.</returns>
        /// <remarks>
        /// Stand age zero is defined as immediately following regeneration harvest and post-harvest slash management (and road maintenance)
        /// but before site preparation, replanting, and release sprays.
        /// </remarks>
        private float GetBareEarthReforestationValue(int financialIndex, float plantingDensityInTreesPerHectare)
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
            float releaseSprayCost = this.ReleaseSprayCostPerHectare[financialIndex] * annualDiscountFactor * annualDiscountFactor; // release spray at two years, so discount factor squared
            // releaseSprayCost = this.ReleaseSprayCostPerHectare[financialIndex]; // disable discounting
            reforestationNpv -= releaseSprayCost;

            Debug.Assert((reforestationNpv > -1000.0F) && (reforestationNpv < 0.0F));
            return reforestationNpv;
        }

        private float GetDiscountFactor(int financialIndex, int years)
        {
            float discountRate = this.DiscountRate[financialIndex];
            return 1.0F / MathF.Pow(1.0F + discountRate, years);
        }

        public float GetLandExpectationValue(StandTrajectory trajectory, int financialIndex, int endOfRotationPeriod)
        {
            float npvOfAllThins = this.GetNetPresentValueOfAllThins(trajectory, financialIndex);
            HarvestFinancialValue regenFinancialValue = this.GetNetPresentRegenerationHarvestValue(trajectory, financialIndex, endOfRotationPeriod);

            return this.GetLandExpectationValue(trajectory, financialIndex, endOfRotationPeriod, npvOfAllThins, regenFinancialValue);
        }

        public float GetLandExpectationValue(StandTrajectory trajectory, int financialIndex, int endOfRotationPeriod, float npvOfAllThins, HarvestFinancialValue regenHarvestFinancialValue)
        {
            // NPV has reforestation as a regen harvest expense, LEV has reforestation as an initial expense
            float netPresentHarvestValueFromPeriodZeroAge = npvOfAllThins + regenHarvestFinancialValue.NetPresentValuePerHa - regenHarvestFinancialValue.ReforestationNpv;

            int rotationLengthInYears = trajectory.GetEndOfPeriodAge(endOfRotationPeriod);
            int regenerationHarvestYearsFromNow = rotationLengthInYears - trajectory.PeriodZeroAgeInYears;

            float discountFactorFromBareEarth = this.GetDiscountFactor(financialIndex, trajectory.PeriodZeroAgeInYears);
            float discountFactorToEndOfRotation = this.GetDiscountFactor(financialIndex, regenerationHarvestYearsFromNow);

            float netPresentValueFromBareEarth = discountFactorFromBareEarth * netPresentHarvestValueFromPeriodZeroAge +
                regenHarvestFinancialValue.ReforestationNpv / discountFactorToEndOfRotation +
                this.GetNetPresentPropertyTaxAndManagement(financialIndex, rotationLengthInYears);

            float presentToFutureConversionFactor = this.GetAppreciationFactor(financialIndex, rotationLengthInYears);
            float landExpectationValue = presentToFutureConversionFactor * netPresentValueFromBareEarth / (presentToFutureConversionFactor - 1.0F);
            Debug.Assert(Single.IsNaN(landExpectationValue) == false);

            return landExpectationValue;
        }

        private float GetNetPresentPropertyTaxAndManagement(int financialIndex, int regenerationHarvestYearsFromNow)
        {
            float discountRate = this.DiscountRate[financialIndex];
            float propertyTaxesAndManagement = -this.PropertyTaxAndManagementPerHectareYear[financialIndex];
            if (discountRate > 0.0F)
            {
                float discountFactor = this.GetDiscountFactor(financialIndex, regenerationHarvestYearsFromNow);
                propertyTaxesAndManagement *= (1.0F - discountFactor) / discountRate;
            }
            else
            {
                propertyTaxesAndManagement *= regenerationHarvestYearsFromNow;
            }

            return propertyTaxesAndManagement;
        }

        public LongLogRegenerationHarvest GetNetPresentRegenerationHarvestValue(StandTrajectory trajectory, int financialIndex, int endOfRotationPeriod)
        {
            Stand? endOfRotationStand = trajectory.StandByPeriod[endOfRotationPeriod];
            if (endOfRotationStand == null)
            {
                throw new InvalidOperationException("Stand information is missing for period " + endOfRotationPeriod + ". Has the stand trajectory been fully simulated?");
            }

            // get harvest revenue
            trajectory.RecalculateRegenerationHarvestMerchantableVolumeIfNeeded(endOfRotationPeriod);

            LongLogRegenerationHarvest longLogHarvest = new();
            int rotationLengthInYears = trajectory.GetEndOfPeriodAge(endOfRotationPeriod);
            int regenerationHarvestYearsFromNow = rotationLengthInYears - trajectory.PeriodZeroAgeInYears;
            float appreciationFactor = this.GetTimberAppreciationFactor(financialIndex, regenerationHarvestYearsFromNow);
            longLogHarvest.TryAddMerchantableVolume(trajectory, endOfRotationPeriod, this, financialIndex, appreciationFactor);

            // get harvest cost
            // TODO: unify standing volume calculation?
            HarvestSystems harvestSystems = this.HarvestSystems[financialIndex];
            float regenHarvestTaskCostPerCubicMeter = this.RegenerationSlashCostPerCubicMeter[financialIndex] + this.RegenerationRoadCostPerCubicMeter[financialIndex];
            longLogHarvest.CalculateProductivityAndCost(trajectory, endOfRotationPeriod, isThin: false, harvestSystems, this.RegenerationHarvestCostPerHectare[financialIndex], regenHarvestTaskCostPerCubicMeter);

            float discountFactor = this.GetDiscountFactor(financialIndex, regenerationHarvestYearsFromNow);
            float reforestationNpv = discountFactor * this.GetBareEarthReforestationValue(financialIndex, trajectory.PlantingDensityInTreesPerHectare);
            longLogHarvest.SetNetPresentValue(discountFactor, reforestationNpv);

            Debug.Assert((longLogHarvest.NetPresentValuePerHa > -100.0F * 1000.0F) && (longLogHarvest.NetPresentValuePerHa < 1000.0F * 1000.0F));
            return longLogHarvest;
        }

        public float GetNetPresentValue(StandTrajectory trajectory, int financialIndex, int endOfRotationPeriod)
        {
            float npvOfAllThins = this.GetNetPresentValueOfAllThins(trajectory, financialIndex);
            HarvestFinancialValue regenHarvestFinancialValue = this.GetNetPresentRegenerationHarvestValue(trajectory, financialIndex, endOfRotationPeriod);
            return this.GetNetPresentValue(trajectory, financialIndex, endOfRotationPeriod, npvOfAllThins, regenHarvestFinancialValue);
        }

        public float GetNetPresentValue(StandTrajectory trajectory, int financialIndex, int endOfRotationPeriod, float npvOfAllThins, HarvestFinancialValue regenHarvestFinancialValue)
        {
            float npv = npvOfAllThins + regenHarvestFinancialValue.NetPresentValuePerHa; // includes reforestation after regeneration harvest

            // standard annuity formula for present value of finite series of annual payments a: NPV = a (1 - (1 + r)^-n) / r, r ≠ 0
            // for r = 0, NPV = na (https://math.stackexchange.com/questions/3159219/limit-of-geometric-series-sum-when-r-1)
            int rotationLengthInYears = trajectory.GetEndOfPeriodAge(endOfRotationPeriod);
            int regenerationHarvestYearsFromNow = rotationLengthInYears - trajectory.PeriodZeroAgeInYears;
            npv += this.GetNetPresentPropertyTaxAndManagement(financialIndex, regenerationHarvestYearsFromNow);

            Debug.Assert((npv > -11.0F * 1000.0F) && (npv < 1000.0F * 1000.0F));
            return npv;
        }

        private float GetNetPresentValueOfAllThins(StandTrajectory trajectory, int financialIndex)
        {
            float netPresentValue = 0.0F;
            foreach (Harvest harvest in trajectory.Treatments.Harvests)
            {
                if (this.TryGetNetPresentThinValue(trajectory, financialIndex, harvest.Period, out HarvestFinancialValue? thinFinancialValue))
                {
                    netPresentValue += thinFinancialValue.NetPresentValuePerHa;
                }
            }

            return netPresentValue;
        }

        public (float pondValue2Saw, float pondValue3Saw, float pondValue4Saw) GetPondValueAfterTax(FiaCode treeSpecies, int financialIndex)
        {
            float pondValue2Saw;
            float pondValue3Saw;
            float pondValue4Saw;
            switch (treeSpecies)
            {
                case FiaCode.AlnusRubra:
                    pondValue2Saw = this.RedAlder2SawPondValuePerMbf[financialIndex];
                    pondValue3Saw = this.RedAlder3SawPondValuePerMbf[financialIndex];
                    pondValue4Saw = this.RedAlder4SawPondValuePerMbf[financialIndex];
                    break;
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
                // white whood
                case FiaCode.TsugaHeterophylla:
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                case FiaCode.PiceaSitchensis:
                    pondValue2Saw = this.WhiteWood2SawPondValuePerMbf[financialIndex];
                    pondValue3Saw = this.WhiteWood3SawPondValuePerMbf[financialIndex];
                    pondValue4Saw = this.WhiteWood4SawPondValuePerMbf[financialIndex];
                    break;
                // merchantable species not supported by Organon
                // case FiaCode.AbiesAmabalis:
                // case FiaCode.AbiesProcera:
                // case FiaCode.ChamaecyparisLawsoniana:
                // case FiaCode.PiceaEnglemannii:
                // case FiaCode.PiceaSitchensis:
                // case FiaCode.PopulusTrichocarpa:
                // case FiaCode.UmbellulariaCalifornica:
                // merchantable species not supported by volume scaling
                // case FiaCode.AbiesConcolor:
                // case FiaCode.AcerMacrophyllum:
                // case FiaCode.CalocedrusDecurrens:
                // case FiaCode.PinusLambertiana:
                // case FiaCode.PinusPonderosa:
                // nonmerchantable species currently treated as nonmerchantable
                // case FiaCode.ArbutusMenziesii:
                // case FiaCode.ChrysolepisChrysophyllaVarChrysophylla:
                // case FiaCode.CornusNuttallii:
                // case FiaCode.NotholithocarpusDensiflorus:
                // case FiaCode.QuercusChrysolepis:
                // case FiaCode.QuercusGarryana:
                // case FiaCode.QuercusKelloggii:
                // case FiaCode.Salix:
                // case FiaCode.TaxusBrevifolia:
                //     pondValue2Saw = 0.0F;
                //     pondValue3Saw = 0.0F;
                //     pondValue4Saw = 0.0F;
                //     break;
                default:
                    throw new NotSupportedException("Unhandled species " + treeSpecies + ".");
            }

            float harvestTaxPerMbf = this.HarvestTaxPerMbf[financialIndex];
            float pondValue2SawAfterTax = pondValue2Saw - harvestTaxPerMbf;
            float pondValue3SawAfterTax = pondValue3Saw - harvestTaxPerMbf;
            float pondValue4SawAfterTax = pondValue4Saw - harvestTaxPerMbf;
            return (pondValue2SawAfterTax, pondValue3SawAfterTax, pondValue4SawAfterTax);
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
                    FinancialScenarios.XlsxColumns.SetColumnIndex(rowAsStrings[columnIndex], columnIndex);
                }
                FinancialScenarios.XlsxColumns.VerifyAllColumnsFound();

                this.DiscountRate.Clear();
                this.DouglasFir2SawPondValuePerMbf.Clear();
                this.DouglasFir3SawPondValuePerMbf.Clear();
                this.DouglasFir4SawPondValuePerMbf.Clear();
                this.HarvestSystems.Clear();
                this.HarvestTaxPerMbf.Clear();
                this.Name.Clear();
                this.PropertyTaxAndManagementPerHectareYear.Clear();
                this.RedAlder2SawPondValuePerMbf.Clear();
                this.RedAlder3SawPondValuePerMbf.Clear();
                this.RedAlder4SawPondValuePerMbf.Clear();
                this.RegenerationHarvestCostPerHectare.Clear();
                this.ReleaseSprayCostPerHectare.Clear();
                this.SeedlingCost.Clear();
                this.ShortLogPondValueMultiplier.Clear();
                this.SitePrepAndReplantingCostPerHectare.Clear();
                this.ThinningHarvestCostPerHectare.Clear();
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
            float discountRate = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.DiscountRate]);
            if ((discountRate < 0.0F) || (discountRate > 0.1F))
            {
                throw new NotSupportedException("Annual disount rate is not in the range [0.0, 0.1].");
            }
            this.DiscountRate.Add(discountRate);

            harvestSystems.AddOnWinchCableLengthInM = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.AddOnWinchCableLength]);
            harvestSystems.AnchorCostPerSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.AnchorCostPerSMh]);
            harvestSystems.ChainsawBuckConstant = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ChainsawBuckConstant]);
            harvestSystems.ChainsawBuckCostPerSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ChainsawBuckCostPerSMh]);
            harvestSystems.ChainsawBuckLinear = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ChainsawBuckLinear]);
            harvestSystems.ChainsawBuckUtilization = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ChainsawBuckUtilization]);
            harvestSystems.ChainsawBuckQuadratic = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ChainsawBuckQuadratic]);
            harvestSystems.ChainsawBuckQuadraticThreshold = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ChainsawBuckQuadraticThreshold]);
            harvestSystems.ChainsawByOperatorCostPerSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ChainsawByOperatorCostPerSMh]);
            harvestSystems.ChainsawByOperatorUtilization = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ChainsawByOperatorUtilization]);
            harvestSystems.ChainsawFellAndBuckCostPerSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ChainsawFellAndBuckCostPerSMh]);
            harvestSystems.ChainsawFellAndBuckConstant = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ChainsawFellAndBuckConstant]);
            harvestSystems.ChainsawFellAndBuckLinear = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ChainsawFellAndBuckLinear]);
            harvestSystems.ChainsawFellAndBuckUtilization = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ChainsawFellAndBuckUtilization]);
            harvestSystems.ChainsawSlopeLinear = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ChainsawSlopeLinear]);
            harvestSystems.ChainsawSlopeThresholdInPercent = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ChainsawSlopeThreshold]);

            harvestSystems.CorridorWidth = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.CorridorWidth]);
            harvestSystems.CutToLengthHaulPayloadInKg = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.CutToLengthHaulPayload]);
            harvestSystems.CutToLengthHaulPerSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.CutToLengthHaulPerSMh]);
            harvestSystems.CutToLengthRoundtripHaulSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.CutToLengthHaulHours]);

            harvestSystems.FellerBuncherCostPerSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.FellerBuncherCostPerSMh]);
            harvestSystems.FellerBuncherFellingConstant = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.FellerBuncherConstant]);
            harvestSystems.FellerBuncherFellingLinear = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.FellerBuncherLinear]);
            harvestSystems.FellerBuncherSlopeLinear = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.FellerBuncherSlopeLinear]);
            harvestSystems.FellerBuncherSlopeThresholdInPercent = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.FellerBuncherSlopeThreshold]);
            harvestSystems.FellerBuncherUtilization = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.FellerBuncherUtilization]);

            harvestSystems.ForwarderCostPerSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderCostPerSMh]);
            harvestSystems.ForwarderDriveWhileLoadingLogs = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderDriveWhileLoadingLogs]);
            harvestSystems.ForwarderEmptyWeight = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderWeight]);
            harvestSystems.ForwarderLoadMeanLogVolume = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderLoadMeanLogVolume]);
            harvestSystems.ForwarderLoadPayload = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderLoadPayload]);
            harvestSystems.ForwarderMaximumPayloadInKg = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderPayload]);
            harvestSystems.ForwarderSpeedInStandLoadedTethered = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderLoadedTethered]);
            harvestSystems.ForwarderSpeedInStandLoadedUntethered = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderLoadedUntethered]);
            harvestSystems.ForwarderSpeedInStandUnloadedTethered = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderUnloadedTethered]);
            harvestSystems.ForwarderSpeedInStandUnloadedUntethered = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderUnloadedUntethered]);
            harvestSystems.ForwarderSpeedOnRoad = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderOnRoad]);
            harvestSystems.ForwarderTractiveForce = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderTractiveForce]);
            harvestSystems.ForwarderUnloadLinearOneSort = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderUnloadLinearOneSort]);
            harvestSystems.ForwarderUnloadLinearTwoSorts = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderUnloadLinearTwoSorts]);
            harvestSystems.ForwarderUnloadLinearThreeSorts = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderUnloadLinearThreeSorts]);
            harvestSystems.ForwarderUnloadMeanLogVolume = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderUnloadMeanLogVolume]);
            harvestSystems.ForwarderUnloadPayload = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderUnloadPayload]);
            harvestSystems.ForwarderUtilization = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ForwarderUtilization]);

            harvestSystems.GrappleYardingConstantRegen = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleYardingConstantRegen]);
            harvestSystems.GrappleYardingConstantThin = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleYardingConstantThin]);
            harvestSystems.GrappleYardingLinearRegen = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleYardingLinearRegen]);
            harvestSystems.GrappleYardingLinearThin = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleYardingLinearThin]);
            harvestSystems.GrappleSwingYarderCostPerSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleSwingYarderCostPerSMh]);
            harvestSystems.GrappleSwingYarderMaxPayload = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleSwingYarderMaxPayload]);
            harvestSystems.GrappleSwingYarderMeanPayload = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleSwingYarderMeanPayload]);
            harvestSystems.GrappleSwingYarderUtilization = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleSwingYarderUtilization]);
            harvestSystems.GrappleYoaderCostPerSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleYoaderCostPerSMh]);
            harvestSystems.GrappleYoaderMaxPayload = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleYoaderMaxPayload]);
            harvestSystems.GrappleYoaderMeanPayload = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleYoaderMeanPayload]);
            harvestSystems.GrappleYoaderUtilization = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.GrappleYoaderUtilization]);

            harvestSystems.LoaderCostPerSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.LoaderCostPerSMh]);
            harvestSystems.LoaderProductivity = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.LoaderProductivity]);
            harvestSystems.LoaderUtilization = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.LoaderUtilization]);

            harvestSystems.LongLogHaulPayloadInKg = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.LongLogHaulPayload]);
            harvestSystems.LongLogHaulPerSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.LongLogHaulPerSMh]);
            harvestSystems.LongLogHaulRoundtripSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.LongLogHaulHours]);

            harvestSystems.MachineMoveInOrOut = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.MachineInOut]);

            harvestSystems.ProcessorBuckConstant = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ProcessorConstant]);
            harvestSystems.ProcessorBuckLinear = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ProcessorLinear]);
            harvestSystems.ProcessorBuckQuadratic1 = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ProcessorQuadratic1]);
            harvestSystems.ProcessorBuckQuadratic2 = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ProcessorQuadratic2]);
            harvestSystems.ProcessorBuckQuadraticThreshold1 = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ProcessorQuadraticThreshold1]);
            harvestSystems.ProcessorBuckQuadraticThreshold2 = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ProcessorQuadraticThreshold2]);
            harvestSystems.ProcessorCostPerSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ProcessorCostPerSMh]);
            harvestSystems.ProcessorUtilization = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ProcessorUtilization]);

            harvestSystems.TrackedHarvesterCostPerSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterCostPerSMh]);
            harvestSystems.TrackedHarvesterFellAndBuckConstant = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterConstant]);
            harvestSystems.TrackedHarvesterFellAndBuckDiameterLimit = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterFellAndBuckDiameterLimit]);
            harvestSystems.TrackedHarvesterFellAndBuckLinear = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterLinear]);
            harvestSystems.TrackedHarvesterFellAndBuckQuadratic1 = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterQuadratic1]);
            harvestSystems.TrackedHarvesterFellAndBuckQuadratic2 = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterQuadratic2]);
            harvestSystems.TrackedHarvesterFellingDiameterLimit = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterFellingDiameterLimit]);
            harvestSystems.TrackedHarvesterQuadraticThreshold1 = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterQuadraticThreshold1]);
            harvestSystems.TrackedHarvesterQuadraticThreshold2 = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterQuadraticThreshold2]);
            harvestSystems.TrackedHarvesterSlopeLinear = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterSlopeLinear]);
            harvestSystems.TrackedHarvesterSlopeThresholdInPercent = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterSlopeThreshold]);
            harvestSystems.TrackedHarvesterUtilization = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.TrackedHarvesterUtilization]);

            harvestSystems.WheeledHarvesterCostPerSMh = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.WheeledHarvesterCostPerSMh]);
            harvestSystems.WheeledHarvesterFellAndBuckConstant = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.WheeledHarvesterConstant]);
            harvestSystems.WheeledHarvesterFellAndBuckDiameterLimit = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.WheeledHarvesterFellAndBuckDiameterLimit]);
            harvestSystems.WheeledHarvesterFellAndBuckLinear = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.WheeledHarvesterLinear]);
            harvestSystems.WheeledHarvesterFellAndBuckQuadratic = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.WheeledHarvesterQuadratic]);
            harvestSystems.WheeledHarvesterFellingDiameterLimit = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.WheeledHarvesterFellingDiameterLimit]);
            harvestSystems.WheeledHarvesterQuadraticThreshold = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.WheeledHarvesterQuadraticThreshold]);
            harvestSystems.WheeledHarvesterSlopeLinear = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.WheeledHarvesterSlopeLinear]);
            harvestSystems.WheeledHarvesterSlopeThresholdInPercent = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.WheeledHarvesterSlopeThreshold]);
            harvestSystems.WheeledHarvesterUtilization = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.WheeledHarvesterUtilization]);

            harvestSystems.VerifyPropertyValues();

            float harvestTaxMbf = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.HarvestTaxPerMbf]);
            if ((harvestTaxMbf < 0.0F) || (harvestTaxMbf > 100.0F))
            {
                throw new NotSupportedException("Per MBF harvest tax is not in the range US$ [0.0, 100.0]/MBF.");
            }
            this.HarvestTaxPerMbf.Add(harvestTaxMbf);

            string name = rowAsStrings[FinancialScenarios.XlsxColumns.Name];
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new NotSupportedException("Financial scenario's name is missing or contains only whitespace.");
            }
            this.Name.Add(name);

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

            float alru2Spond = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.Alru2SPond]);
            if ((alru2Spond < 100.0F) || (alru2Spond > 1000.0F))
            {
                throw new NotSupportedException("Red alder 2S pond value is not in the range US$ [100.0, 1000.0]/MBF.");
            }
            this.RedAlder2SawPondValuePerMbf.Add(alru2Spond);

            float alru3Spond = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.Alru3SPond]);
            if ((alru3Spond < 100.0F) || (alru3Spond > 1000.0F))
            {
                throw new NotSupportedException("Red alder 3S pond value is not in the range US$ [100.0, 1000.0]/MBF.");
            }
            this.RedAlder3SawPondValuePerMbf.Add(alru3Spond);

            float alru4Spond = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.Alru4SPond]);
            if ((alru4Spond < 100.0F) || (alru4Spond > 1000.0F))
            {
                throw new NotSupportedException("Red alder 4S pond value is not in the range US$ [100.0, 1000.0]/MBF.");
            }
            this.RedAlder4SawPondValuePerMbf.Add(alru4Spond);

            float regenHarvestPerHectare = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.RegenPerHa]);
            if ((regenHarvestPerHectare <= 0.0F) || (regenHarvestPerHectare > 1000.0F))
            {
                throw new NotSupportedException("Regeneration harvest cost is not in the range US$ (0.0, 1000.0]/ha.");
            }
            this.RegenerationHarvestCostPerHectare.Add(regenHarvestPerHectare);

            float regenRoadMaintenancePerCubicMeter = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.RegenRoads]);
            if ((regenRoadMaintenancePerCubicMeter <= 0.0F) || (regenRoadMaintenancePerCubicMeter > 10.0F))
            {
                throw new NotSupportedException("Regeneration harvest road maintenance cost per cubic meter is not in the range US$ (0.0, 10.0]/ha.");
            }
            this.RegenerationRoadCostPerCubicMeter.Add(regenRoadMaintenancePerCubicMeter);

            float regenSlashDisposalPerCubicMeter = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.RegenSlash]);
            if ((regenSlashDisposalPerCubicMeter <= 0.0F) || (regenSlashDisposalPerCubicMeter > 10.0F))
            {
                throw new NotSupportedException("Regeneration harvest slash disposal cost per cubic meter is not in the range US$ (0.0, 10.0]/ha.");
            }
            this.RegenerationSlashCostPerCubicMeter.Add(regenRoadMaintenancePerCubicMeter);

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

            float shortLogPondValueMultiplier = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ShortLogPondValueMultiplier]);
            if ((shortLogPondValueMultiplier <= 0.0F) || (shortLogPondValueMultiplier > 2.0F))
            {
                throw new NotSupportedException("Pond value multiplier for short logs is not in the range (0.0, 2.0].");
            }
            this.ShortLogPondValueMultiplier.Add(shortLogPondValueMultiplier);

            float sitePrepAndReplanting = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.SitePrepAndPlantPerHa]);
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

            float thinRoadMaintenancePerCubicMeter = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ThinRoads]);
            if ((thinRoadMaintenancePerCubicMeter <= 0.0F) || (thinRoadMaintenancePerCubicMeter > 10.0F))
            {
                throw new NotSupportedException("Thinning road maintenance cost per cubic meter is not in the range US$ (0.0, 10.0]/ha.");
            }
            this.ThinningRoadCostPerCubicMeter.Add(thinRoadMaintenancePerCubicMeter);

            float thinSlashDisposalPerCubicMeter = Single.Parse(rowAsStrings[FinancialScenarios.XlsxColumns.ThinSlash]);
            if ((thinSlashDisposalPerCubicMeter <= 0.0F) || (thinSlashDisposalPerCubicMeter > 10.0F))
            {
                throw new NotSupportedException("Thinning slash disposal cost per cubic meter is not in the range US$ (0.0, 10.0]/ha.");
            }
            this.ThinningSlashCostPerCubicMeter.Add(thinSlashDisposalPerCubicMeter);

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
            Debug.Assert(this.Count == this.HarvestSystems.Count);
            Debug.Assert(this.Count == this.HarvestTaxPerMbf.Count);
            Debug.Assert(this.Count == this.Name.Count);
            Debug.Assert(this.Count == this.ReleaseSprayCostPerHectare.Count);
            Debug.Assert(this.Count == this.RegenerationHarvestCostPerHectare.Count);
            Debug.Assert(this.Count == this.SeedlingCost.Count);
            Debug.Assert(this.Count == this.SitePrepAndReplantingCostPerHectare.Count);
            Debug.Assert(this.Count == this.PropertyTaxAndManagementPerHectareYear.Count);
            Debug.Assert(this.Count == this.ThinningHarvestCostPerHectare.Count);
            Debug.Assert(this.Count == this.ShortLogPondValueMultiplier.Count);
            Debug.Assert(this.Count == this.TimberAppreciationRate.Count);
            Debug.Assert(this.Count == this.WesternRedcedarCamprunPondValuePerMbf.Count);
            Debug.Assert(this.Count == this.WhiteWood2SawPondValuePerMbf.Count);
            Debug.Assert(this.Count == this.WhiteWood3SawPondValuePerMbf.Count);
            Debug.Assert(this.Count == this.WhiteWood4SawPondValuePerMbf.Count);
        }

        public bool TryGetNetPresentThinValue(StandTrajectory trajectory, int financialIndex, int thinningPeriod, [NotNullWhen(true)] out HarvestFinancialValue? thinningRevenue)
        {
            if (trajectory.HasThinInPeriod(thinningPeriod) == false)
            {
                thinningRevenue = null;
                return false;
            }

            Stand? previousStand = trajectory.StandByPeriod[thinningPeriod - 1];
            if (previousStand == null)
            {
                throw new InvalidOperationException("Stand information is missing for period " + (thinningPeriod - 1) + ". Has the stand trajectory been simulated?");
            }

            // TODO: unify merchantable and operable volume calculations?
            trajectory.RecalculateThinningMerchantableVolumeIfNeeded(thinningPeriod);

            // get harvest revenue
            HarvestSystems harvestSystems = this.HarvestSystems[financialIndex];
            int thinningAgeInYears = trajectory.GetEndOfPeriodAge(thinningPeriod);
            int harvestYearsFromNow = thinningAgeInYears - trajectory.PeriodZeroAgeInYears;
            float longLogPondValueMultiplier = this.GetTimberAppreciationFactor(financialIndex, harvestYearsFromNow);

            CutToLengthHarvest cutToLengthThin = new();
            float shortLogPondValueMultiplier = this.ShortLogPondValueMultiplier[financialIndex] * longLogPondValueMultiplier;
            cutToLengthThin.TryAddMerchantableVolume(trajectory, thinningPeriod, this, financialIndex, shortLogPondValueMultiplier);

            LongLogThin longLogThin = new();
            longLogThin.TryAddMerchantableVolume(trajectory, thinningPeriod, this, financialIndex, longLogPondValueMultiplier);

            // get harvest cost
            float thinningHarvestTaskCostPerCubicMeter = this.ThinningRoadCostPerCubicMeter[financialIndex] + this.ThinningSlashCostPerCubicMeter[financialIndex];
            cutToLengthThin.CalculateProductivityAndCost(trajectory, thinningPeriod, isThin: true, harvestSystems, this.ThinningHarvestCostPerHectare[financialIndex], thinningHarvestTaskCostPerCubicMeter);
            longLogThin.CalculateProductivityAndCost(trajectory, thinningPeriod, isThin: true, harvestSystems, this.ThinningHarvestCostPerHectare[financialIndex], thinningHarvestTaskCostPerCubicMeter);

            float discountFactor = this.GetDiscountFactor(financialIndex, harvestYearsFromNow);
            cutToLengthThin.SetNetPresentValue(discountFactor, reforestationNpv: Single.NaN); // no site prep and planting after thinning
            longLogThin.SetNetPresentValue(discountFactor, reforestationNpv: Single.NaN); // no site prep and planting after thinning

            Debug.Assert((cutToLengthThin.NetPresentValuePerHa > -100.0F * 1000.0F) && (cutToLengthThin.NetPresentValuePerHa < 1000.0F * 1000.0F) &&
                         (longLogThin.NetPresentValuePerHa > -100.0F * 1000.0F) && (longLogThin.NetPresentValuePerHa < 1000.0F * 1000.0F));

            thinningRevenue = cutToLengthThin.NetPresentValuePerHa >= longLogThin.NetPresentValuePerHa ? cutToLengthThin : longLogThin;
            return true;
        }

        private class XlsxColumnIndices
        {
            public int AddOnWinchCableLength { get; set; }
            public int AnchorCostPerSMh { get; set; }

            public int Alru2SPond { get; set; }
            public int Alru3SPond { get; set; }
            public int Alru4SPond { get; set; }

            public int ChainsawBuckConstant { get; set; }
            public int ChainsawBuckCostPerSMh { get; set; }
            public int ChainsawBuckLinear { get; set; }
            public int ChainsawBuckUtilization { get; set; }
            public int ChainsawBuckQuadratic { get; set; }
            public int ChainsawBuckQuadraticThreshold { get; set; }
            public int ChainsawByOperatorCostPerSMh { get; set; }
            public int ChainsawByOperatorUtilization { get; set; }
            public int ChainsawFellAndBuckConstant { get; set; }
            public int ChainsawFellAndBuckLinear { get; set; }
            public int ChainsawFellAndBuckCostPerSMh { get; set; }
            public int ChainsawFellAndBuckUtilization { get; set; }
            public int ChainsawSlopeLinear { get; set; }
            public int ChainsawSlopeThreshold { get; set; }

            public int CorridorWidth { get; set; }

            public int CutToLengthHaulHours { get; set; }
            public int CutToLengthHaulPayload { get; set; }
            public int CutToLengthHaulPerSMh { get; set; }

            public int DiscountRate { get; set; }

            public int FellerBuncherConstant { get; set; }
            public int FellerBuncherCostPerSMh { get; set; }
            public int FellerBuncherLinear { get; set; }
            public int FellerBuncherSlopeLinear { get; set; }
            public int FellerBuncherSlopeThreshold { get; set; }
            public int FellerBuncherUtilization { get; set; }

            public int ForwarderCostPerSMh { get; set; }
            public int ForwarderDriveWhileLoadingLogs { get; set; }
            public int ForwarderLoadedTethered { get; set; }
            public int ForwarderLoadedUntethered { get; set; }
            public int ForwarderLoadMeanLogVolume { get; set; }
            public int ForwarderLoadPayload { get; set; }
            public int ForwarderOnRoad { get; set; }
            public int ForwarderPayload { get; set; }
            public int ForwarderTractiveForce { get; set; }
            public int ForwarderUnloadedTethered { get; set; }
            public int ForwarderUnloadedUntethered { get; set; }
            public int ForwarderUnloadLinearOneSort { get; set; }
            public int ForwarderUnloadLinearTwoSorts { get; set; }
            public int ForwarderUnloadLinearThreeSorts { get; set; }
            public int ForwarderUnloadMeanLogVolume { get; set; }
            public int ForwarderUnloadPayload { get; set; }
            public int ForwarderUtilization { get; set; }
            public int ForwarderWeight { get; set; }

            public int GrappleYardingConstantRegen { get; set; }
            public int GrappleYardingConstantThin { get; set; }
            public int GrappleYardingLinearRegen { get; set; }
            public int GrappleYardingLinearThin { get; set; }
            public int GrappleSwingYarderCostPerSMh { get; set; }
            public int GrappleSwingYarderMaxPayload { get; set; }
            public int GrappleSwingYarderMeanPayload { get; set; }
            public int GrappleSwingYarderUtilization { get; set; }
            public int GrappleYoaderCostPerSMh { get; set; }
            public int GrappleYoaderMaxPayload { get; set; }
            public int GrappleYoaderMeanPayload { get; set; }
            public int GrappleYoaderUtilization { get; set; }

            public int HarvestTaxPerMbf { get; set; }

            public int LoaderProductivity { get; set; }
            public int LoaderCostPerSMh { get; set; }
            public int LoaderUtilization { get; set; }
            public int LongLogHaulHours { get; set; }
            public int LongLogHaulPayload { get; set; }
            public int LongLogHaulPerSMh { get; set; }

            public int MachineInOut { get; set; }
            public int Name { get; set; }

            public int ProcessorConstant { get; set; }
            public int ProcessorLinear { get; set; }
            public int ProcessorQuadratic1 { get; set; }
            public int ProcessorQuadratic2 { get; set; }
            public int ProcessorQuadraticThreshold1 { get; set; }
            public int ProcessorQuadraticThreshold2 { get; set; }
            public int ProcessorCostPerSMh { get; set; }
            public int ProcessorUtilization { get; set; }

            public int PropertyTaxAndManagement { get; set; }

            public int Psme2SPond { get; set; }
            public int Psme3SPond { get; set; }
            public int Psme4SPond { get; set; }

            public int RegenPerHa { get; set; }
            public int RegenRoads { get; set; }
            public int RegenSlash { get; set; }
            public int ReleaseSpray { get; set; }
            public int Seedling { get; set; }
            public int ShortLogPondValueMultiplier { get; set; }
            public int SitePrepAndPlantPerHa { get; set; }

            public int ThplCamprun { get; set; }

            public int ThinPerHa { get; set; }
            public int ThinRoads { get; set; }
            public int ThinSlash { get; set; }
            public int TimberAppreciation { get; set; }

            public int TrackedHarvesterConstant { get; set; }
            public int TrackedHarvesterCostPerSMh { get; set; }
            public int TrackedHarvesterFellAndBuckDiameterLimit { get; set; }
            public int TrackedHarvesterFellingDiameterLimit { get; set; }
            public int TrackedHarvesterLinear { get; set; }
            public int TrackedHarvesterQuadratic1 { get; set; }
            public int TrackedHarvesterQuadratic2 { get; set; }
            public int TrackedHarvesterQuadraticThreshold1 { get; set; }
            public int TrackedHarvesterQuadraticThreshold2 { get; set; }
            public int TrackedHarvesterSlopeLinear { get; set; }
            public int TrackedHarvesterSlopeThreshold { get; set; }
            public int TrackedHarvesterUtilization { get; set; }

            public int WheeledHarvesterConstant { get; set; }
            public int WheeledHarvesterCostPerSMh { get; set; }
            public int WheeledHarvesterFellAndBuckDiameterLimit { get; set; }
            public int WheeledHarvesterFellingDiameterLimit { get; set; }
            public int WheeledHarvesterLinear { get; set; }
            public int WheeledHarvesterQuadratic { get; set; }
            public int WheeledHarvesterQuadraticThreshold { get; set; }
            public int WheeledHarvesterSlopeLinear { get; set; }
            public int WheeledHarvesterSlopeThreshold { get; set; }
            public int WheeledHarvesterUtilization { get; set; }

            public int White2SPond { get; set; }
            public int White3SPond { get; set; }
            public int White4SPond { get; set; }

            public XlsxColumnIndices()
            {
                this.Reset();
            }

            public void SetColumnIndex(string columnHeader, int columnIndex)
            {
                switch (columnHeader)
                {
                    case "addOnWinchCableLength":
                        this.AddOnWinchCableLength = columnIndex;
                        break;
                    case "anchorSMh":
                        this.AnchorCostPerSMh = columnIndex;
                        break;
                    case "alru2Spond":
                        this.Alru2SPond = columnIndex;
                        break;
                    case "alru3Spond":
                        this.Alru3SPond = columnIndex;
                        break;
                    case "alru4Spond":
                        this.Alru4SPond = columnIndex;
                        break;
                    case "chainsawBuckConstant":
                        this.ChainsawBuckConstant = columnIndex;
                        break;
                    case "chainsawBuckLinear":
                        this.ChainsawBuckLinear = columnIndex;
                        break;
                    case "chainsawBuckSMh":
                        this.ChainsawBuckCostPerSMh = columnIndex;
                        break;
                    case "chainsawBuckQuadratic":
                        this.ChainsawBuckQuadratic = columnIndex;
                        break;
                    case "chainsawBuckQuadraticThreshold":
                        this.ChainsawBuckQuadraticThreshold = columnIndex;
                        break;
                    case "chainsawBuckUtilization":
                        this.ChainsawBuckUtilization = columnIndex;
                        break;
                    case "chainsawFellAndBuckConstant":
                        this.ChainsawFellAndBuckConstant = columnIndex;
                        break;
                    case "chainsawFellAndBuckLinear":
                        this.ChainsawFellAndBuckLinear = columnIndex;
                        break;
                    case "chainsawFellAndBuckSMh":
                        this.ChainsawFellAndBuckCostPerSMh = columnIndex;
                        break;
                    case "chainsawFellAndBuckUtilization":
                        this.ChainsawFellAndBuckUtilization = columnIndex;
                        break;
                    case "chainsawByOperatorSMh":
                        this.ChainsawByOperatorCostPerSMh = columnIndex;
                        break;
                    case "chainsawByOperatorUtilization":
                        this.ChainsawByOperatorUtilization = columnIndex;
                        break;
                    case "chainsawSlopeLinear":
                        this.ChainsawSlopeLinear = columnIndex;
                        break;
                    case "chainsawSlopeThreshold":
                        this.ChainsawSlopeThreshold = columnIndex;
                        break;
                    case "corridorWidth":
                        this.CorridorWidth = columnIndex;
                        break;
                    case "ctlHaulHours":
                        this.CutToLengthHaulHours = columnIndex;
                        break;
                    case "ctlHaulPayload":
                        this.CutToLengthHaulPayload = columnIndex;
                        break;
                    case "ctlHaulSMh":
                        this.CutToLengthHaulPerSMh = columnIndex;
                        break;
                    case "discountRate":
                        this.DiscountRate = columnIndex;
                        break;
                    case "fellerBuncherConstant":
                        this.FellerBuncherConstant = columnIndex;
                        break;
                    case "fellerBuncherLinear":
                        this.FellerBuncherLinear = columnIndex;
                        break;
                    case "fellerBuncherSlopeLinear":
                        this.FellerBuncherSlopeLinear = columnIndex;
                        break;
                    case "fellerBuncherSlopeThreshold":
                        this.FellerBuncherSlopeThreshold = columnIndex;
                        break;
                    case "fellerBuncherSMh":
                        this.FellerBuncherCostPerSMh = columnIndex;
                        break;
                    case "fellerBuncherUtilization":
                        this.FellerBuncherUtilization = columnIndex;
                        break;
                    case "forwarderDriveWhileLoadingLogs":
                        this.ForwarderDriveWhileLoadingLogs = columnIndex;
                        break;
                    case "forwarderPayload":
                        this.ForwarderPayload = columnIndex;
                        break;
                    case "forwarderLoadedTethered":
                        this.ForwarderLoadedTethered = columnIndex;
                        break;
                    case "forwarderLoadedUntethered":
                        this.ForwarderLoadedUntethered = columnIndex;
                        break;
                    case "forwarderLoadMeanLogVolume":
                        this.ForwarderLoadMeanLogVolume = columnIndex;
                        break;
                    case "forwarderLoadPayload":
                        this.ForwarderLoadPayload = columnIndex;
                        break;
                    case "forwarderOnRoad":
                        this.ForwarderOnRoad = columnIndex;
                        break;
                    case "forwarderSMh":
                        this.ForwarderCostPerSMh = columnIndex;
                        break;
                    case "forwarderTractiveForce":
                        this.ForwarderTractiveForce = columnIndex;
                        break;
                    case "forwarderUnloadedTethered":
                        this.ForwarderUnloadedTethered = columnIndex;
                        break;
                    case "forwarderUnloadedUntethered":
                        this.ForwarderUnloadedUntethered = columnIndex;
                        break;
                    case "forwarderUnloadLinearOneSort":
                        this.ForwarderUnloadLinearOneSort = columnIndex;
                        break;
                    case "forwarderUnloadLinearTwoSorts":
                        this.ForwarderUnloadLinearTwoSorts = columnIndex;
                        break;
                    case "forwarderUnloadLinearThreeSorts":
                        this.ForwarderUnloadLinearThreeSorts = columnIndex;
                        break;
                    case "forwarderUnloadMeanLogVolume":
                        this.ForwarderUnloadMeanLogVolume = columnIndex;
                        break;
                    case "forwarderUnloadPayload":
                        this.ForwarderUnloadPayload = columnIndex;
                        break;
                    case "forwarderUtilization":
                        this.ForwarderUtilization = columnIndex;
                        break;
                    case "forwarderWeight":
                        this.ForwarderWeight = columnIndex;
                        break;
                    case "grappleYardingConstantRegen":
                        this.GrappleYardingConstantRegen = columnIndex;
                        break;
                    case "grappleYardingConstantThin":
                        this.GrappleYardingConstantThin = columnIndex;
                        break;
                    case "grappleYardingLinearRegen":
                        this.GrappleYardingLinearRegen = columnIndex;
                        break;
                    case "grappleYardingLinearThin":
                        this.GrappleYardingLinearThin = columnIndex;
                        break;
                    case "grappleSwingYarderMaxPayload":
                        this.GrappleSwingYarderMaxPayload = columnIndex;
                        break;
                    case "grappleSwingYarderMeanPayload":
                        this.GrappleSwingYarderMeanPayload = columnIndex;
                        break;
                    case "grappleSwingYarderSMh":
                        this.GrappleSwingYarderCostPerSMh = columnIndex;
                        break;
                    case "grappleSwingYarderUtilization":
                        this.GrappleSwingYarderUtilization = columnIndex;
                        break;
                    case "grappleYoaderMaxPayload":
                        this.GrappleYoaderMaxPayload = columnIndex;
                        break;
                    case "grappleYoaderMeanPayload":
                        this.GrappleYoaderMeanPayload = columnIndex;
                        break;
                    case "grappleYoaderSMh":
                        this.GrappleYoaderCostPerSMh = columnIndex;
                        break;
                    case "grappleYoaderUtilization":
                        this.GrappleYoaderUtilization = columnIndex;
                        break;
                    case "harvestTaxPerMbf":
                        this.HarvestTaxPerMbf = columnIndex;
                        break;
                    case "loaderProductivity":
                        this.LoaderProductivity = columnIndex;
                        break;
                    case "loaderSMh":
                        this.LoaderCostPerSMh = columnIndex;
                        break;
                    case "loaderUtilization":
                        this.LoaderUtilization = columnIndex;
                        break;
                    case "longLogHaulHours":
                        this.LongLogHaulHours = columnIndex;
                        break;
                    case "longLogHaulPayload":
                        this.LongLogHaulPayload = columnIndex;
                        break;
                    case "longLogHaulSMh":
                        this.LongLogHaulPerSMh = columnIndex;
                        break;
                    case "machineInOut":
                        this.MachineInOut = columnIndex;
                        break;
                    case "name":
                        this.Name = columnIndex;
                        break;
                    case "psme2Spond":
                        this.Psme2SPond = columnIndex;
                        break;
                    case "psme3Spond":
                        this.Psme3SPond = columnIndex;
                        break;
                    case "psme4Spond":
                        this.Psme4SPond = columnIndex;
                        break;
                    case "processorConstant":
                        this.ProcessorConstant = columnIndex;
                        break;
                    case "processorLinear":
                        this.ProcessorLinear = columnIndex;
                        break;
                    case "processorQuadratic1":
                        this.ProcessorQuadratic1 = columnIndex;
                        break;
                    case "processorQuadratic2":
                        this.ProcessorQuadratic2 = columnIndex;
                        break;
                    case "processorQuadraticThreshold1":
                        this.ProcessorQuadraticThreshold1 = columnIndex;
                        break;
                    case "processorQuadraticThreshold2":
                        this.ProcessorQuadraticThreshold2 = columnIndex;
                        break;
                    case "processorSMh":
                        this.ProcessorCostPerSMh = columnIndex;
                        break;
                    case "processorUtilization":
                        this.ProcessorUtilization = columnIndex;
                        break;
                    case "propertyTaxAndManagementPerHa":
                        this.PropertyTaxAndManagement = columnIndex;
                        break;
                    case "regenPerHa":
                        this.RegenPerHa = columnIndex;
                        break;
                    case "regenRoads":
                        this.RegenRoads = columnIndex;
                        break;
                    case "regenSlash":
                        this.RegenSlash = columnIndex;
                        break;
                    case "releaseSpray":
                        this.ReleaseSpray = columnIndex;
                        break;
                    case "seedling":
                        this.Seedling = columnIndex;
                        break;
                    case "shortLogPondMultiplier":
                        this.ShortLogPondValueMultiplier = columnIndex;
                        break;
                    case "sitePrepPlant":
                        this.SitePrepAndPlantPerHa = columnIndex;
                        break;
                    case "thinPerHa":
                        this.ThinPerHa = columnIndex;
                        break;
                    case "thinRoads":
                        this.ThinRoads = columnIndex;
                        break;
                    case "thinSlash":
                        this.ThinSlash = columnIndex;
                        break;
                    case "thplCamprun":
                        this.ThplCamprun = columnIndex;
                        break;
                    case "timberAppreciation":
                        this.TimberAppreciation = columnIndex;
                        break;
                    case "trackedHarvesterConstant":
                        this.TrackedHarvesterConstant = columnIndex;
                        break;
                    case "trackedHarvesterFellingDiameterLimit":
                        this.TrackedHarvesterFellingDiameterLimit = columnIndex;
                        break;
                    case "trackedHarvesterFellAndBuckDiameterLimit":
                        this.TrackedHarvesterFellAndBuckDiameterLimit = columnIndex;
                        break;
                    case "trackedHarvesterLinear":
                        this.TrackedHarvesterLinear = columnIndex;
                        break;
                    case "trackedHarvesterQuadratic1":
                        this.TrackedHarvesterQuadratic1 = columnIndex;
                        break;
                    case "trackedHarvesterQuadratic2":
                        this.TrackedHarvesterQuadratic2 = columnIndex;
                        break;
                    case "trackedHarvesterQuadraticThreshold1":
                        this.TrackedHarvesterQuadraticThreshold1 = columnIndex;
                        break;
                    case "trackedHarvesterQuadraticThreshold2":
                        this.TrackedHarvesterQuadraticThreshold2 = columnIndex;
                        break;
                    case "trackedHarvesterSlopeLinear":
                        this.TrackedHarvesterSlopeLinear = columnIndex;
                        break;
                    case "trackedHarvesterSlopeThreshold":
                        this.TrackedHarvesterSlopeThreshold = columnIndex;
                        break;
                    case "trackedHarvesterSMh":
                        this.TrackedHarvesterCostPerSMh = columnIndex;
                        break;
                    case "trackedHarvesterUtilization":
                        this.TrackedHarvesterUtilization = columnIndex;
                        break;
                    case "wheeledHarvesterConstant":
                        this.WheeledHarvesterConstant = columnIndex;
                        break;
                    case "wheeledHarvesterFellAndBuckDiameterLimit":
                        this.WheeledHarvesterFellAndBuckDiameterLimit = columnIndex;
                        break;
                    case "wheeledHarvesterFellingDiameterLimit":
                        this.WheeledHarvesterFellingDiameterLimit = columnIndex;
                        break;
                    case "wheeledHarvesterLinear":
                        this.WheeledHarvesterLinear = columnIndex;
                        break;
                    case "wheeledHarvesterQuadratic":
                        this.WheeledHarvesterQuadratic = columnIndex;
                        break;
                    case "wheeledHarvesterQuadraticThreshold":
                        this.WheeledHarvesterQuadraticThreshold = columnIndex;
                        break;
                    case "wheeledHarvesterSlopeLinear":
                        this.WheeledHarvesterSlopeLinear = columnIndex;
                        break;
                    case "wheeledHarvesterSlopeThreshold":
                        this.WheeledHarvesterSlopeThreshold = columnIndex;
                        break;
                    case "wheeledHarvesterSMh":
                        this.WheeledHarvesterCostPerSMh = columnIndex;
                        break;
                    case "wheeledHarvesterUtilization":
                        this.WheeledHarvesterUtilization = columnIndex;
                        break;
                    case "white2Spond":
                        this.White2SPond = columnIndex;
                        break;
                    case "white3Spond":
                        this.White3SPond = columnIndex;
                        break;
                    case "white4Spond":
                        this.White4SPond = columnIndex;
                        break;
                    default:
                        throw new NotSupportedException("Unknown column '" + columnHeader + "'.");
                }
            }

            public void Reset()
            {
                this.AddOnWinchCableLength = -1;
                this.AnchorCostPerSMh = -1;

                this.Alru2SPond = -1;
                this.Alru3SPond = -1;
                this.Alru4SPond = -1;

                this.ChainsawBuckConstant = -1;
                this.ChainsawBuckLinear = -1;
                this.ChainsawBuckCostPerSMh = -1;
                this.ChainsawBuckQuadratic = -1;
                this.ChainsawBuckQuadraticThreshold = -1;
                this.ChainsawBuckUtilization = -1;
                this.ChainsawByOperatorCostPerSMh = -1;
                this.ChainsawByOperatorUtilization = -1;
                this.ChainsawFellAndBuckConstant = -1;
                this.ChainsawFellAndBuckCostPerSMh = -1;
                this.ChainsawFellAndBuckLinear = -1;
                this.ChainsawFellAndBuckUtilization = -1;
                this.ChainsawSlopeLinear = -1;
                this.ChainsawSlopeThreshold = -1;

                this.CorridorWidth = -1;
                this.CutToLengthHaulHours = -1;
                this.CutToLengthHaulPayload = -1;
                this.CutToLengthHaulPerSMh = -1;
                this.DiscountRate = -1;

                this.FellerBuncherConstant = -1;
                this.FellerBuncherLinear = -1;
                this.FellerBuncherCostPerSMh = -1;
                this.FellerBuncherSlopeLinear = -1;
                this.FellerBuncherSlopeThreshold = -1;
                this.FellerBuncherUtilization = -1;

                this.ForwarderCostPerSMh = -1;
                this.ForwarderDriveWhileLoadingLogs = -1;
                this.ForwarderLoadedTethered = -1;
                this.ForwarderLoadedUntethered = -1;
                this.ForwarderLoadMeanLogVolume = -1;
                this.ForwarderLoadPayload = -1;
                this.ForwarderOnRoad = -1;
                this.ForwarderPayload = -1;
                this.ForwarderTractiveForce = -1;
                this.ForwarderUnloadedTethered = -1;
                this.ForwarderUnloadedUntethered = -1;
                this.ForwarderUnloadLinearOneSort = -1;
                this.ForwarderUnloadLinearTwoSorts = -1;
                this.ForwarderUnloadLinearThreeSorts = -1;
                this.ForwarderUnloadMeanLogVolume = -1;
                this.ForwarderUnloadPayload = -1;
                this.ForwarderUtilization = -1;
                this.ForwarderWeight = -1;

                this.GrappleYardingConstantRegen = -1;
                this.GrappleYardingConstantThin = -1;
                this.GrappleYardingLinearRegen = -1;
                this.GrappleYardingLinearThin = -1;
                this.GrappleSwingYarderCostPerSMh = -1;
                this.GrappleSwingYarderMaxPayload = -1;
                this.GrappleSwingYarderMeanPayload = -1;
                this.GrappleSwingYarderUtilization = -1;
                this.GrappleYoaderCostPerSMh = -1;
                this.GrappleYoaderMaxPayload = -1;
                this.GrappleYoaderMeanPayload = -1;
                this.GrappleYoaderUtilization = -1;

                this.HarvestTaxPerMbf = -1;
                this.LoaderProductivity = -1;
                this.LoaderCostPerSMh = -1;
                this.LoaderUtilization = -1;
                this.LongLogHaulHours = -1;
                this.LongLogHaulPayload = -1;
                this.LongLogHaulPerSMh = -1;

                this.MachineInOut = -1;
                this.Name = -1;

                this.ProcessorCostPerSMh = -1;
                this.ProcessorLinear = -1;
                this.ProcessorQuadratic1 = -1;
                this.ProcessorQuadratic2 = -1;
                this.ProcessorQuadraticThreshold1 = -1;
                this.ProcessorQuadraticThreshold2 = -1;
                this.ProcessorUtilization = -1;

                this.PropertyTaxAndManagement = -1;

                this.Psme2SPond = -1;
                this.Psme3SPond = -1;
                this.Psme4SPond = -1;

                this.RegenPerHa = -1;
                this.RegenRoads = -1;
                this.RegenSlash = -1;
                this.ReleaseSpray = -1;
                this.Seedling = -1;
                this.SitePrepAndPlantPerHa = -1;

                this.ThinPerHa = -1;
                this.ShortLogPondValueMultiplier = -1;
                this.ThinRoads = -1;
                this.ThinSlash = -1;

                this.ThplCamprun = -1;
                this.TimberAppreciation = -1;

                this.TrackedHarvesterConstant = -1;
                this.TrackedHarvesterFellAndBuckDiameterLimit = -1;
                this.TrackedHarvesterFellingDiameterLimit = -1;
                this.TrackedHarvesterLinear = -1;
                this.TrackedHarvesterCostPerSMh = -1;
                this.TrackedHarvesterQuadratic1 = -1;
                this.TrackedHarvesterQuadratic2 = -1;
                this.TrackedHarvesterQuadraticThreshold1 = -1;
                this.TrackedHarvesterQuadraticThreshold2 = -1;
                this.TrackedHarvesterSlopeLinear = -1;
                this.TrackedHarvesterSlopeThreshold = -1;
                this.TrackedHarvesterUtilization = -1;

                this.WheeledHarvesterConstant = -1;
                this.WheeledHarvesterFellAndBuckDiameterLimit = -1;
                this.WheeledHarvesterFellingDiameterLimit = -1;
                this.WheeledHarvesterLinear = -1;
                this.WheeledHarvesterCostPerSMh = -1;
                this.WheeledHarvesterQuadratic = -1;
                this.WheeledHarvesterQuadraticThreshold = -1;
                this.WheeledHarvesterSlopeLinear = -1;
                this.WheeledHarvesterSlopeThreshold = -1;
                this.WheeledHarvesterUtilization = -1;

                this.White2SPond = -1;
                this.White3SPond = -1;
                this.White4SPond = -1;
            }

            public void VerifyAllColumnsFound()
            {
                if (this.AddOnWinchCableLength < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.AddOnWinchCableLength), "Column for wheeled harvesters' and forwarders' add on winches' cable length not found.");
                }
                if (this.AnchorCostPerSMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.AnchorCostPerSMh), "Anchor machine cost column not found.");
                }

                if (this.Alru2SPond < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.Alru2SPond), "Red alder 2S column not found.");
                }
                if (this.Alru3SPond < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.Alru3SPond), "Red alder 3S column not found.");
                }
                if (this.Alru4SPond < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.Alru4SPond), "Red alder 4S column not found.");
                }

                if (this.ChainsawBuckConstant < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ChainsawBuckConstant), "Column for chainsaw bucking time intercept not found.");
                }
                if (this.ChainsawBuckCostPerSMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ChainsawBuckCostPerSMh), "Chainsaw bucking cost column not found.");
                }
                if (this.ChainsawBuckLinear < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ChainsawBuckLinear), "Column for chainsaw bucking time linear coefficent not found.");
                }
                if (this.ChainsawBuckUtilization < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ChainsawBuckUtilization), "Chainsaw bucker utilization column not found.");
                }
                if (this.ChainsawBuckQuadratic < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ChainsawBuckQuadratic), "Column for chainsaw bucking time quadratic coefficent not found.");
                }
                if (this.ChainsawBuckQuadraticThreshold < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ChainsawBuckQuadraticThreshold), "Column for onset of chainsaw quadratic bucking time not found.");
                }
                if (this.ChainsawByOperatorCostPerSMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ChainsawByOperatorCostPerSMh), "Column for cost of chainsaw use by heavy equipment operator not found.");
                }
                if (this.ChainsawByOperatorUtilization < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ChainsawByOperatorCostPerSMh), "Column for chainsaw utilization by heavy equipment operator not found.");
                }
                if (this.ChainsawFellAndBuckConstant < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ChainsawFellAndBuckConstant), "Column for chainsaw felling and bucking time intercept not found.");
                }
                if (this.ChainsawFellAndBuckCostPerSMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ChainsawFellAndBuckCostPerSMh), "Chainsaw felling and bucking cost column not found.");
                }
                if (this.ChainsawFellAndBuckLinear < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ChainsawFellAndBuckLinear), "Column for chainsaw felling and bucking time linear coefficent not found.");
                }
                if (this.ChainsawFellAndBuckUtilization < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ChainsawFellAndBuckUtilization), "Chainsaw felling and bucking utilization column not found.");
                }
                if (this.ChainsawSlopeLinear < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ChainsawSlopeLinear), "Column for linear coefficient of slope increases in chainsaw operation time column not found.");
                }
                if (this.ChainsawSlopeThreshold < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ChainsawSlopeThreshold), "Column for onset of slope increases in chainsaw operation time column not found.");
                }

                if (this.CorridorWidth < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.CorridorWidth), "Corridor width column not found.");
                }
                if (this.CutToLengthHaulHours < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.CutToLengthHaulHours), "Thinning haul roundtrip hours column not found.");
                }
                if (this.CutToLengthHaulPayload < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.CutToLengthHaulPayload), "Thinning haul payload column not found.");
                }
                if (this.CutToLengthHaulPerSMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.CutToLengthHaulPerSMh), "Thinning haul cost per m³ column not found.");
                }
                if (this.DiscountRate < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.DiscountRate), "Discount rate column not found.");
                }

                if (this.FellerBuncherConstant < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.FellerBuncherConstant), "Column for feller-buncher felling time intercept not found.");
                }
                if (this.FellerBuncherCostPerSMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.FellerBuncherCostPerSMh), "Feller-buncher cost column not found.");
                }
                if (this.FellerBuncherLinear < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.FellerBuncherLinear), "Column for feller-buncher felling time linear coefficent not found.");
                }
                if (this.FellerBuncherSlopeLinear < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.FellerBuncherSlopeLinear), "Column for linear coefficient of slope increases in feller-buncher felling time not found.");
                }
                if (this.FellerBuncherSlopeThreshold < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.FellerBuncherSlopeThreshold), "Column for onset of slope increases in feller-buncher felling time not found.");
                }
                if (this.FellerBuncherUtilization < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.FellerBuncherUtilization), "Feller-buncher utilization column not found.");
                }

                if (this.ForwarderCostPerSMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ForwarderCostPerSMh), "Forwarder cost column not found.");
                }
                if (this.ForwarderDriveWhileLoadingLogs < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ForwarderDriveWhileLoadingLogs), "Column for forwarder coefficient of number of logs in payload for driving while loading not found.");
                }
                if (this.ForwarderLoadedTethered < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ForwarderLoadedTethered), "Column for forwarder movement speed when loaded and tethered not found.");
                }
                if (this.ForwarderLoadedUntethered < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ForwarderLoadedUntethered), "Column for forwarder movement speed when loaded and not tethered cost column not found.");
                }
                if (this.ForwarderLoadMeanLogVolume < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ForwarderLoadMeanLogVolume), "Column for forwarder coefficient of mean log volume while loading not found.");
                }
                if (this.ForwarderLoadPayload < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ForwarderLoadPayload), "Column for forwarder coefficient of payload size while loading not found.");
                }
                if (this.ForwarderOnRoad < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ForwarderOnRoad), "Column for forwarder movement on road speed not found.");
                }
                if (this.ForwarderPayload < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ForwarderPayload), "Forwarder payload column not found.");
                }
                if (this.ForwarderUnloadedTethered < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ForwarderUnloadedTethered), "Column for forwarder movement speed when unloaded and tethered not found.");
                }
                if (this.ForwarderUnloadLinearOneSort < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ForwarderUnloadLinearOneSort), "Column for linear coefficient of forwarder unload time with one sort not found.");
                }
                if (this.ForwarderUnloadLinearTwoSorts < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ForwarderUnloadLinearTwoSorts), "Column for linear coefficient of forwarder unload time with two sorts not found.");
                }
                if (this.ForwarderUnloadLinearThreeSorts < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ForwarderUnloadLinearThreeSorts), "Column for linear coefficient of forwarder unload time with three sorts not found.");
                }
                if (this.ForwarderUnloadMeanLogVolume < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ForwarderLoadMeanLogVolume), "Column for forwarder coefficient of mean log volume while unloading not found.");
                }
                if (this.ForwarderUnloadPayload < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ForwarderLoadPayload), "Column for forwarder coefficient of payload size while unloading not found.");
                }
                if (this.ForwarderUnloadedUntethered < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ForwarderUnloadedUntethered), "Column for forwarder movement speed when unloaded and not tethered column not found.");
                }
                if (this.ForwarderTractiveForce < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ForwarderTractiveForce), "Forwarder tractive force column not found.");
                }
                if (this.ForwarderUtilization < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ForwarderUtilization), "Forwarder utilization column not found.");
                }
                if (this.ForwarderWeight < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ForwarderWeight), "Forwarder weight column not found.");
                }

                if (this.GrappleYardingConstantRegen < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.GrappleYardingConstantRegen), "Column for grapple yarding turn time intercept in regeneration harvests not found.");
                }
                if (this.GrappleYardingConstantThin < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.GrappleYardingConstantThin), "Column for grapple yarding turn time intercept during thinning not found.");
                }
                if (this.GrappleYardingLinearRegen < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.GrappleYardingLinearRegen), "Column for linear skyline length term in grapple yarding turn time in regeneration harvests not found.");
                }
                if (this.GrappleYardingLinearThin < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.GrappleYardingLinearThin), "Column for linear skyline length term in grapple yarding turn time during thinning not found.");
                }
                if (this.GrappleSwingYarderCostPerSMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.GrappleSwingYarderCostPerSMh), "Column for grapple swing yarder operating cost not found.");
                }
                if (this.GrappleSwingYarderMaxPayload < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.GrappleSwingYarderMaxPayload), "Column for grapple yoader maximum payload not found.");
                }
                if (this.GrappleSwingYarderMeanPayload < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.GrappleSwingYarderMeanPayload), "Column for grapple swing yarder mean payload not found.");
                }
                if (this.GrappleSwingYarderUtilization < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.GrappleSwingYarderUtilization), "Column for grapple swing yarder utilization not found.");
                }
                if (this.GrappleYoaderCostPerSMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.GrappleYoaderCostPerSMh), "Column for grapple yoader operating cost not found.");
                }
                if (this.GrappleYoaderMaxPayload < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.GrappleYoaderMaxPayload), "Column for grapple yoader maximum payload not found.");
                }
                if (this.GrappleYoaderMeanPayload < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.GrappleYoaderMeanPayload), "Column for grapple yoader mean payload not found.");
                }
                if (this.GrappleYoaderUtilization < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.GrappleYoaderUtilization), "Column for grapple yoader utilization not found.");
                }

                if (this.HarvestTaxPerMbf < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.HarvestTaxPerMbf), "Per MBF harvest tax column not found.");
                }
                if (this.LoaderProductivity < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.LoaderProductivity), "Loader productivity column not found.");
                }
                if (this.LoaderCostPerSMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.LoaderCostPerSMh), "Loader cost column not found.");
                }
                if (this.LoaderUtilization < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.LoaderUtilization), "Column for loader utilization not found.");
                }

                if (this.LongLogHaulHours < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.LongLogHaulPerSMh), "Regeneration haul roundtrip hours column not found.");
                }
                if (this.LongLogHaulPayload < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.LongLogHaulPayload), "Regeneration haul payload column not found.");
                }
                if (this.LongLogHaulPerSMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.LongLogHaulPerSMh), "Regeneration haul cost per SMh column not found.");
                }

                if (this.MachineInOut < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.Name), "Column for cost of moving heavy equipment in and out not found.");
                }
                if (this.Name < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.Name), "Scenario name column not found.");
                }

                if (this.ProcessorConstant < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ProcessorConstant), "Column for processor intercept not found.");
                }
                if (this.ProcessorCostPerSMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ProcessorCostPerSMh), "Processor cost column not found.");
                }
                if (this.ProcessorLinear < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ProcessorLinear), "Column for processor linear coefficent not found.");
                }
                if (this.ProcessorQuadratic1 < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ProcessorQuadratic1), "Column for processor first quadratic coefficient not found.");
                }
                if (this.ProcessorQuadratic2 < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ProcessorQuadratic2), "Column for processor second quadratic coefficient not found.");
                }
                if (this.ProcessorQuadraticThreshold1 < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ProcessorQuadraticThreshold1), "Column for first onset of quadratic processor felling time not found.");
                }
                if (this.ProcessorQuadraticThreshold2 < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ProcessorQuadraticThreshold2), "Column for second onset of quadratic processor felling time not found.");
                }
                if (this.ProcessorUtilization < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ProcessorUtilization), "Column for processor utilization not found.");
                }

                if (this.PropertyTaxAndManagement < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.PropertyTaxAndManagement), "Annual taxes and management column not found.");
                }
                if (this.Psme2SPond < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.Psme2SPond), "Douglas-fir 2S column not found.");
                }
                if (this.Psme3SPond < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.Psme3SPond), "Douglas-fir 3S column not found.");
                }
                if (this.Psme4SPond < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.Psme4SPond), "Douglas-fir 4S/CNS column not found.");
                }

                if (this.RegenPerHa < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.RegenPerHa), "Per hectare regeneration harvest cost column not found.");
                }
                if (this.RegenRoads < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.RegenPerHa), "Regeneration harvest road maintenance cost column not found.");
                }
                if (this.RegenSlash < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.RegenPerHa), "Regeneration harvest slash disposal cost column not found.");
                }
                if (this.ReleaseSpray < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ReleaseSpray), "Release spray column not found.");
                }
                if (this.Seedling < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.Seedling), "Cost per seedling column not found.");
                }
                if (this.SitePrepAndPlantPerHa < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.SitePrepAndPlantPerHa), "Fixed site prep cost column not found.");
                }
                if (this.ThplCamprun < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ThplCamprun), "Western redcedar camprun column not found.");
                }

                if (this.ThinPerHa < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ThinPerHa), "Per hectare thinning cost column not found.");
                }
                if (this.ThinRoads < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.RegenPerHa), "Thinning road maintenance cost column not found.");
                }
                if (this.ThinSlash < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.RegenPerHa), "Thinning slash disposal cost column not found.");
                }
                if (this.ShortLogPondValueMultiplier < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ShortLogPondValueMultiplier), "Short log pond value multiplier column not found.");
                }
                if (this.TimberAppreciation < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.TimberAppreciation), "Annual timber appreciation rate column not found.");
                }

                if (this.TrackedHarvesterConstant < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.TrackedHarvesterConstant), "Column for tracked harvester felling time intercept not found.");
                }
                if (this.TrackedHarvesterCostPerSMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.TrackedHarvesterCostPerSMh), "Tracked harvester cost column not found.");
                }
                if (this.TrackedHarvesterFellAndBuckDiameterLimit < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.TrackedHarvesterFellAndBuckDiameterLimit), "Column for tracked harvester fell and buck diameter limit not found.");
                }
                if (this.TrackedHarvesterFellingDiameterLimit < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.TrackedHarvesterFellingDiameterLimit), "Column for tracked harvester felling diameter limit not found.");
                }
                if (this.TrackedHarvesterLinear < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.TrackedHarvesterLinear), "Column for tracked harvester felling time linear coefficent not found.");
                }
                if (this.TrackedHarvesterQuadratic1 < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.TrackedHarvesterQuadratic1), "Column for tracked harvester felling time first quadratic coefficient not found.");
                }
                if (this.TrackedHarvesterQuadratic2 < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.TrackedHarvesterQuadratic2), "Column for tracked harvester felling time second quadratic coefficient not found.");
                }
                if (this.TrackedHarvesterQuadraticThreshold1 < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.TrackedHarvesterQuadraticThreshold1), "Column for first onset of quadratic tracked harvester felling time not found.");
                }
                if (this.TrackedHarvesterQuadraticThreshold2 < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.TrackedHarvesterQuadraticThreshold2), "Column for second onset of quadratic tracked harvester felling time not found.");
                }
                if (this.TrackedHarvesterSlopeLinear < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.TrackedHarvesterSlopeLinear), "Column for linear coefficient of slope increases in tracked harvester felling time not found.");
                }
                if (this.TrackedHarvesterSlopeThreshold < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.TrackedHarvesterSlopeThreshold), "Column for onset of slope increases in tracked harvester felling time not found.");
                }
                if (this.TrackedHarvesterUtilization < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.TrackedHarvesterUtilization), "Tracked harvester utilization column not found.");
                }

                if (this.WheeledHarvesterConstant < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.WheeledHarvesterConstant), "Column for wheeled harvester felling time intercept not found.");
                }
                if (this.WheeledHarvesterCostPerSMh < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.WheeledHarvesterCostPerSMh), "Wheeled harvester cost column not found.");
                }
                if (this.WheeledHarvesterFellAndBuckDiameterLimit < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.WheeledHarvesterFellAndBuckDiameterLimit), "Column for wheeled harvester felling diameter limit not found.");
                }
                if (this.WheeledHarvesterLinear < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.WheeledHarvesterLinear), "Column for wheeled harvester felling time linear coefficent not found.");
                }
                if (this.WheeledHarvesterQuadratic < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.WheeledHarvesterQuadratic), "Column for wheeled harvester felling time quadratic coefficient not found.");
                }
                if (this.WheeledHarvesterQuadraticThreshold < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.WheeledHarvesterQuadraticThreshold), "Column for onset of quadratic wheeled harvester felling time not found.");
                }
                if (this.WheeledHarvesterSlopeLinear < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.WheeledHarvesterSlopeLinear), "Column for linear coeffience of slope increases in wheeled harvester felling time not found.");
                }
                if (this.WheeledHarvesterSlopeThreshold < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.WheeledHarvesterSlopeThreshold), "Column for onset of slope increases in wheeled harvester felling time not found.");
                }
                if (this.WheeledHarvesterUtilization < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.WheeledHarvesterUtilization), "Wheeled harvester utilization column not found.");
                }

                if (this.White2SPond < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.White2SPond), "White wood 2S column not found.");
                }
                if (this.White3SPond < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.White3SPond), "White wood 3S column not found.");
                }
                if (this.White4SPond < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.White4SPond), "White wood 4S/CNS column not found.");
                }
            }
        }
    }
}
