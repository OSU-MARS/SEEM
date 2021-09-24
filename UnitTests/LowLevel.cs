using Microsoft.VisualStudio.TestTools.UnitTesting;
using Osu.Cof.Ferm.Extensions;
using Osu.Cof.Ferm.Silviculture;
using Osu.Cof.Ferm.Tree;
using System;

namespace Osu.Cof.Ferm.Test
{
    [DeploymentItem("financial scenarios.xlsx")]
    [TestClass]
    public class LowLevel
    {
        [TestMethod]
        public void BreadthFirstEnumeration()
        {
            // 1D enumerations
            // basic 1-5 element arrays
            object object1 = new();
            BreadthFirstEnumerator<object> enumerator1D = new(new object[] { object1 }, 0);
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object1));
            Assert.IsTrue(enumerator1D.MoveNext() == false);

            object object2 = new();
            enumerator1D = new(new object[] { object1, object2 }, 0);
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object1));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object2));
            Assert.IsTrue(enumerator1D.MoveNext() == false);

            enumerator1D = new(new object[] { object1, object2 }, 1);
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object2));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object1));
            Assert.IsTrue(enumerator1D.MoveNext() == false);

            object object3 = new();
            enumerator1D = new(new object[] { object1, object2, object3 }, 1);
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object2));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object3));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object1));
            Assert.IsTrue(enumerator1D.MoveNext() == false);

            object object4 = new();
            enumerator1D = new(new object[] { object1, object2, object3, object4 }, 3);
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object4));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object3));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object2));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object1));
            Assert.IsTrue(enumerator1D.MoveNext() == false);

            object object5 = new();
            enumerator1D = new(new object[] { object1, object2, object3, object4, object5 }, 1);
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object2));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object3));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object1));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object4));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object5));
            Assert.IsTrue(enumerator1D.MoveNext() == false);

            // null skipping
            enumerator1D = new(new object?[] { null, null, null }, 0);
            Assert.IsTrue(enumerator1D.MoveNext() == false);

            enumerator1D = new(new object?[] { object1, null, null, object2, object3, null, object4, null, object5, null }, 2);
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object2));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object3));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object1));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object4));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object5));
            Assert.IsTrue(enumerator1D.MoveNext() == false);

            // 2D enumerations
            BreadthFirstEnumerator2D<object> enumerator2D = new(new object[][] { new object[] { object1 }  }, 0, 0);
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object1));
            Assert.IsTrue(enumerator2D.MoveNext() == false);

            enumerator2D = new(new object[][] { new object[] { object1, object2 },
                                                new object[] { object3, object4 } },
                               0, 0); // balanced traversal, top left
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object1));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object3));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object2));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object4));
            Assert.IsTrue(enumerator2D.MoveNext() == false);

            enumerator2D = new(new object[][] { new object[] { object1, object2 },
                                                new object[] { object3, object4 } },
                               1, 1); // balanced traversal, bottom right
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object4));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object2));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object3));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object1));
            Assert.IsTrue(enumerator2D.MoveNext() == false);

            enumerator2D = new(new object[][] { new object[] { object1, object2 },
                                                new object[] { object3, object4 } },
                               1, 0); // balanced traversal, bottom left
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object3));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object1));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object4));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object2));
            Assert.IsTrue(enumerator2D.MoveNext() == false);

            enumerator2D = new(new object[][] { new object[] { object1, object2 },
                                                new object[] { object3, object4 } },
                               0, 1); // balanced traversal, top right
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object2));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object4));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object1));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object3));
            Assert.IsTrue(enumerator2D.MoveNext() == false);

            object object6 = new();
            object object7 = new();
            object object8 = new();
            enumerator2D = new(new object?[][] { new object?[] { object1, object2, object3 },
                                                 new object?[] { object4, null, object5 },
                                                 new object?[] { object6, object7, object8 } },
                               1, 1);
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object7));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object2));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object5));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object4));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object8));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object1));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object6));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object3));
            Assert.IsTrue(enumerator2D.MoveNext() == false);

            enumerator2D = new(new object?[][] { new object?[] { object1, object2, object3 },
                                                 new object?[] { object4, object5, object6 },
                                                 new object?[] { null, object7, object8 } },
                               0, 1, BreadthFirstEnumerator2D.XFirst);
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object2));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object5));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object7));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object3));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object1));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object6));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object4));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object8));
            Assert.IsTrue(enumerator2D.MoveNext() == false);

            enumerator2D = new(new object?[][] { new object?[] { object1, object2, object3 },
                                                 new object?[] { object4, object5, object6 },
                                                 new object?[] { object7, null, object8 } },
                               2, 1, BreadthFirstEnumerator2D.YFirst);
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object8));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object7));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object5));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object6));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object4));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object2));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object3));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object1));
            Assert.IsTrue(enumerator2D.MoveNext() == false);

            // 3D enumerations
            BreadthFirstEnumerator3D<object> enumerator3D = new(new object[][][] { new object[][] { new object[] { object1 } } }, 0, 0, 0);
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object1));
            Assert.IsTrue(enumerator3D.MoveNext() == false);

            enumerator3D = new(new object[][][] { new object[][] { new object[] { object1, object2 },
                                                                   new object[] { object3, object4 } } },
                               0, 1, 0);
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object3));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object4));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object1));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object2));
            Assert.IsTrue(enumerator3D.MoveNext() == false);

            enumerator3D = new(new object[][][] { new object[][] { new object[] { object1, object2 },
                                                                   new object[] { object3, object4 } },
                                                  new object[][] { new object[] { object5, object6 },
                                                                   new object[] { object7, object8 } } },
                               1, 1, 0);
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object7));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object8));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object5));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object3));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object6));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object4));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object1));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object2));
            Assert.IsTrue(enumerator3D.MoveNext() == false);
        }

        [TestMethod]
        public void FinancialScenarios()
        {
            FinancialScenarios financialScenarios = new();
            financialScenarios.Read("financial scenarios.xlsx", "Sheet1");

            // properties of FinancialScenario
            Assert.IsTrue(financialScenarios.Count == 1);
            Assert.IsTrue((financialScenarios.DiscountRate.Count == 1) && 
                          (financialScenarios.DiscountRate[0] == Constant.Financial.DefaultAnnualDiscountRate));
            Assert.IsTrue((financialScenarios.DouglasFir2SawPondValuePerMbf.Count == 1) &&
                          (financialScenarios.DouglasFir2SawPondValuePerMbf[0] < 750.0F) &&
                          (financialScenarios.DouglasFir2SawPondValuePerMbf[0] > financialScenarios.DouglasFir3SawPondValuePerMbf[0]));
            Assert.IsTrue((financialScenarios.DouglasFir3SawPondValuePerMbf.Count == 1) &&
                          (financialScenarios.DouglasFir3SawPondValuePerMbf[0] <= financialScenarios.DouglasFir2SawPondValuePerMbf[0]) &&
                          (financialScenarios.DouglasFir3SawPondValuePerMbf[0] > financialScenarios.DouglasFir4SawPondValuePerMbf[0]));
            Assert.IsTrue((financialScenarios.DouglasFir4SawPondValuePerMbf.Count == 1) &&
                          (financialScenarios.DouglasFir4SawPondValuePerMbf[0] <= financialScenarios.DouglasFir3SawPondValuePerMbf[0]) &&
                          (financialScenarios.DouglasFir4SawPondValuePerMbf[0] > 500.0F));
            Assert.IsTrue(financialScenarios.HarvestSystems.Count == 1);
            Assert.IsTrue((financialScenarios.HarvestTaxPerMbf.Count == 1) && 
                          (financialScenarios.HarvestTaxPerMbf[0] == Constant.Financial.OregonForestProductsHarvestTax));
            Assert.IsTrue((financialScenarios.Name.Count == 1) && 
                          (String.IsNullOrWhiteSpace(financialScenarios.Name[0]) == false));
            Assert.IsTrue((financialScenarios.PropertyTaxAndManagementPerHectareYear.Count == 1) &&
                          (financialScenarios.PropertyTaxAndManagementPerHectareYear[0] < 75.0F) &&
                          (financialScenarios.PropertyTaxAndManagementPerHectareYear[0] > 30.0F));
            Assert.IsTrue((financialScenarios.RegenerationHarvestCostPerHectare.Count == 1) &&
                          (financialScenarios.RegenerationHarvestCostPerHectare[0] < 800.0F) &&
                          (financialScenarios.RegenerationHarvestCostPerHectare[0] > 400.0F));
            Assert.IsTrue((financialScenarios.RegenerationHaulCostPerCubicMeter.Count == 1) &&
                          (financialScenarios.RegenerationHaulCostPerCubicMeter[0] < 15.0F) &&
                          (financialScenarios.RegenerationHaulCostPerCubicMeter[0] > 5.0F));
            Assert.IsTrue((financialScenarios.ReleaseSprayCostPerHectare.Count == 1) &&
                          (financialScenarios.ReleaseSprayCostPerHectare[0] < 500.0F) &&
                          (financialScenarios.ReleaseSprayCostPerHectare[0] > 200.0F));
            Assert.IsTrue((financialScenarios.SeedlingCost.Count == 1) &&
                          (financialScenarios.SeedlingCost[0] < 2.00F) &&
                          (financialScenarios.SeedlingCost[0] > 0.25F));
            Assert.IsTrue((financialScenarios.SitePrepAndReplantingCostPerHectare.Count == 1) &&
                          (financialScenarios.SitePrepAndReplantingCostPerHectare[0] < 1000.0F) &&
                          (financialScenarios.SitePrepAndReplantingCostPerHectare[0] > 500.0F));
            Assert.IsTrue((financialScenarios.ThinningHarvestCostPerHectare.Count == 1) &&
                          (financialScenarios.ThinningHarvestCostPerHectare[0] < 500.0F) &&
                          (financialScenarios.ThinningHarvestCostPerHectare[0] > 250.0F));
            Assert.IsTrue((financialScenarios.ThinningHaulCostPerCubicMeter.Count == 1) &&
                          (financialScenarios.ThinningHaulCostPerCubicMeter[0] < 15.0F) &&
                          (financialScenarios.ThinningHaulCostPerCubicMeter[0] > 5.0F));
            Assert.IsTrue((financialScenarios.ThinningPondValueMultiplier.Count == 1) &&
                          (financialScenarios.ThinningPondValueMultiplier[0] <= 1.00F) &&
                          (financialScenarios.ThinningPondValueMultiplier[0] > 0.50F));
            Assert.IsTrue((financialScenarios.TimberAppreciationRate.Count == 1) &&
                          (financialScenarios.TimberAppreciationRate[0] <= 0.03F) &&
                          (financialScenarios.TimberAppreciationRate[0] >= 0.00F));
            Assert.IsTrue((financialScenarios.WesternRedcedarCamprunPondValuePerMbf.Count == 1) &&
                          (financialScenarios.WesternRedcedarCamprunPondValuePerMbf[0] < 2200.0F) &&
                          (financialScenarios.WesternRedcedarCamprunPondValuePerMbf[0] > 500.0F));
            Assert.IsTrue((financialScenarios.WhiteWood2SawPondValuePerMbf.Count == 1) &&
                          (financialScenarios.WhiteWood2SawPondValuePerMbf[0] < 600.0F) &&
                          (financialScenarios.WhiteWood2SawPondValuePerMbf[0] > financialScenarios.WhiteWood3SawPondValuePerMbf[0]));
            Assert.IsTrue((financialScenarios.WhiteWood3SawPondValuePerMbf.Count == 1) &&
                          (financialScenarios.WhiteWood3SawPondValuePerMbf[0] <= financialScenarios.WhiteWood2SawPondValuePerMbf[0]) &&
                          (financialScenarios.WhiteWood3SawPondValuePerMbf[0] > financialScenarios.WhiteWood4SawPondValuePerMbf[0]));
            Assert.IsTrue((financialScenarios.WhiteWood4SawPondValuePerMbf.Count == 1) &&
                          (financialScenarios.WhiteWood4SawPondValuePerMbf[0] <= financialScenarios.WhiteWood3SawPondValuePerMbf[0]) &&
                          (financialScenarios.WhiteWood4SawPondValuePerMbf[0] > 350.0F));

            // regeneration harvest system
            HarvestSystems harvestSystems = financialScenarios.HarvestSystems[0];
            Assert.IsTrue((harvestSystems.CorridorWidth > 4.0F) && // machine width + movement variability
                          (harvestSystems.CorridorWidth < 23.0F)); // machine reach
            Assert.IsTrue((harvestSystems.ChainsawPMh < 500.0F) &&
                          (harvestSystems.ChainsawPMh > 100.0F));
            Assert.IsTrue((harvestSystems.ChainsawProductivity < 50.0F) &&
                          (harvestSystems.ChainsawProductivity > 5.0F));

            Assert.IsTrue((harvestSystems.FellerBuncherFellingConstant < 100.0F) &&
                          (harvestSystems.FellerBuncherFellingConstant > 5.0F));
            Assert.IsTrue((harvestSystems.FellerBuncherFellingLinear < 25.0F) &&
                          (harvestSystems.FellerBuncherFellingLinear > 1.0F));
            Assert.IsTrue((harvestSystems.FellerBuncherPMh < 500.0F) &&
                          (harvestSystems.FellerBuncherPMh > 100.0F));
            Assert.IsTrue((harvestSystems.FellerBuncherSlopeThresholdInPercent < 65.0F) &&
                          (harvestSystems.FellerBuncherSlopeThresholdInPercent > 20.0F));

            Assert.IsTrue((harvestSystems.ForwarderPayloadInKg <= 20000.0F) &&
                          (harvestSystems.ForwarderPayloadInKg > 15000.0F));
            Assert.IsTrue((harvestSystems.ForwarderPMh < 500.0F) &&
                          (harvestSystems.ForwarderPMh > 100.0F));
            Assert.IsTrue((harvestSystems.ForwarderSpeedInStandLoadedTethered <= harvestSystems.ForwarderSpeedInStandLoadedUntethered) &&
                          (harvestSystems.ForwarderSpeedInStandLoadedTethered > 15.0F));
            Assert.IsTrue((harvestSystems.ForwarderSpeedInStandLoadedUntethered <= harvestSystems.ForwarderSpeedOnRoad) &&
                          (harvestSystems.ForwarderSpeedInStandLoadedUntethered > 20.0F));
            Assert.IsTrue((harvestSystems.ForwarderSpeedInStandUnloadedTethered <= harvestSystems.ForwarderSpeedInStandUnloadedUntethered) &&
                          (harvestSystems.ForwarderSpeedInStandUnloadedTethered > 20.0F));
            Assert.IsTrue((harvestSystems.ForwarderSpeedInStandUnloadedUntethered <= harvestSystems.ForwarderSpeedOnRoad) &&
                          (harvestSystems.ForwarderSpeedInStandUnloadedUntethered > 25.0F));
            Assert.IsTrue((harvestSystems.ForwarderSpeedOnRoad < 100.0F) &&
                          (harvestSystems.ForwarderSpeedOnRoad >= harvestSystems.ForwarderSpeedInStandUnloadedUntethered));

            Assert.IsTrue((harvestSystems.GrappleYardingConstant < 500.0F) &&
                          (harvestSystems.GrappleYardingConstant > 0.0F));
            Assert.IsTrue((harvestSystems.GrappleYardingLinear < 5.0F) &&
                          (harvestSystems.GrappleYardingLinear > 0.0F));
            Assert.IsTrue((harvestSystems.GrappleSwingYarderMaxPayload <= 8000.0F) &&
                          (harvestSystems.GrappleSwingYarderMaxPayload >= harvestSystems.GrappleSwingYarderMeanPayload));
            Assert.IsTrue((harvestSystems.GrappleSwingYarderMeanPayload <= harvestSystems.GrappleSwingYarderMaxPayload) &&
                          (harvestSystems.GrappleSwingYarderMeanPayload >= 1000.0F));
            Assert.IsTrue((harvestSystems.GrappleSwingYarderSMh < 500.0F) &&
                          (harvestSystems.GrappleSwingYarderSMh > 100.0F));
            Assert.IsTrue((harvestSystems.GrappleSwingYarderUtilization < 1.0F) &&
                          (harvestSystems.GrappleSwingYarderUtilization > 0.5F));
            Assert.IsTrue((harvestSystems.GrappleYoaderMaxPayload <= 4500.0F) &&
                          (harvestSystems.GrappleYoaderMaxPayload >= harvestSystems.GrappleYoaderMeanPayload));
            Assert.IsTrue((harvestSystems.GrappleYoaderMeanPayload <= harvestSystems.GrappleYoaderMaxPayload) &&
                          (harvestSystems.GrappleYoaderMeanPayload >= 1000.0F));
            Assert.IsTrue((harvestSystems.GrappleYoaderSMh < 500.0F) &&
                          (harvestSystems.GrappleYoaderSMh > 100.0F));
            Assert.IsTrue((harvestSystems.GrappleYoaderUtilization < 1.0F) &&
                          (harvestSystems.GrappleYoaderUtilization > 0.5F));

            Assert.IsTrue((harvestSystems.LoaderProductivity < 200.0F) &&
                          (harvestSystems.LoaderProductivity > 10.0F));
            Assert.IsTrue((harvestSystems.LoaderSMh < 500.0F) &&
                          (harvestSystems.LoaderSMh > 100.0F));
            Assert.IsTrue((harvestSystems.LoaderUtilization < 1.0F) &&
                          (harvestSystems.LoaderUtilization > 0.5F));

            Assert.IsTrue((harvestSystems.ProcessorConstant < 100.0F) &&
                          (harvestSystems.ProcessorConstant > 10.0F));
            Assert.IsTrue((harvestSystems.ProcessorLinear < 100.0F) &&
                          (harvestSystems.ProcessorLinear > 10.0F));
            Assert.IsTrue((harvestSystems.ProcessorQuadratic1 < 10.0F) &&
                          (harvestSystems.ProcessorQuadratic1 > 1.0F));
            Assert.IsTrue((harvestSystems.ProcessorQuadratic2 < 10.0F) &&
                          (harvestSystems.ProcessorQuadratic2 > 1.0F));
            Assert.IsTrue((harvestSystems.ProcessorQuadraticThreshold1 < 10.0F) &&
                          (harvestSystems.ProcessorQuadraticThreshold1 > 1.0F));
            Assert.IsTrue((harvestSystems.ProcessorQuadraticThreshold2 < 10.0F) &&
                          (harvestSystems.ProcessorQuadraticThreshold2 > 1.0F));
            Assert.IsTrue((harvestSystems.ProcessorSMh < 500.0F) &&
                          (harvestSystems.ProcessorSMh > 100.0F));
            Assert.IsTrue((harvestSystems.ProcessorUtilization < 1.0F) &&
                          (harvestSystems.ProcessorUtilization > 0.5F));

            Assert.IsTrue((harvestSystems.TrackedHarvesterFellingConstant < 100.0F) &&
                          (harvestSystems.TrackedHarvesterFellingConstant > 10.0F));
            Assert.IsTrue((harvestSystems.TrackedHarvesterDiameterLimit < 90.0F) &&
                          (harvestSystems.TrackedHarvesterDiameterLimit > 30.0F));
            Assert.IsTrue((harvestSystems.TrackedHarvesterFellingLinear < 100.0F) &&
                          (harvestSystems.TrackedHarvesterFellingLinear > 10.0F));
            Assert.IsTrue((harvestSystems.TrackedHarvesterFellingQuadratic1 < 10.0F) &&
                          (harvestSystems.TrackedHarvesterFellingQuadratic1 > 1.0F));
            Assert.IsTrue((harvestSystems.TrackedHarvesterFellingQuadratic2 < 10.0F) &&
                          (harvestSystems.TrackedHarvesterFellingQuadratic2 > 1.0F));
            Assert.IsTrue((harvestSystems.TrackedHarvesterPMh < 500.0F) &&
                          (harvestSystems.TrackedHarvesterPMh > 100.0F));
            Assert.IsTrue((harvestSystems.TrackedHarvesterQuadraticThreshold1 < 5.0F) &&
                          (harvestSystems.TrackedHarvesterQuadraticThreshold1 > 0.5F));
            Assert.IsTrue((harvestSystems.TrackedHarvesterQuadraticThreshold2 < 7.5F) &&
                          (harvestSystems.TrackedHarvesterQuadraticThreshold2 > 0.5F));
            Assert.IsTrue((harvestSystems.TrackedHarvesterSlopeThresholdInPercent < 65.0F) &&
                          (harvestSystems.TrackedHarvesterSlopeThresholdInPercent > 20.0F));

            Assert.IsTrue((harvestSystems.WheeledHarvesterFellingConstant < 100.0F) &&
                          (harvestSystems.WheeledHarvesterFellingConstant > 10.0F));
            Assert.IsTrue((harvestSystems.WheeledHarvesterDiameterLimit < 90.0F) &&
                          (harvestSystems.WheeledHarvesterDiameterLimit > 30.0F));
            Assert.IsTrue((harvestSystems.WheeledHarvesterFellingLinear < 100.0F) &&
                          (harvestSystems.WheeledHarvesterFellingLinear > 10.0F));
            Assert.IsTrue((harvestSystems.WheeledHarvesterFellingQuadratic < 10.0F) &&
                          (harvestSystems.WheeledHarvesterFellingQuadratic > 1.0F));
            Assert.IsTrue((harvestSystems.WheeledHarvesterPMh < 500.0F) &&
                          (harvestSystems.WheeledHarvesterPMh > 100.0F));
            Assert.IsTrue((harvestSystems.WheeledHarvesterQuadraticThreshold < 5.0F) &&
                          (harvestSystems.WheeledHarvesterQuadraticThreshold > 0.5F));
            Assert.IsTrue((harvestSystems.WheeledHarvesterSlopeThresholdInPercent < 65.0F) &&
                          (harvestSystems.WheeledHarvesterSlopeThresholdInPercent > 20.0F));
        }

        [TestMethod]
        public void RemoveZeroExpansionFactorTrees()
        {
            int treeCount = 147;
            Trees trees = new(FiaCode.PseudotsugaMenziesii, treeCount, Units.Metric);
            for (int treeIndex = 0; treeIndex < treeCount; ++treeIndex)
            {
                float treeIndexAsFloat = (float)treeIndex;
                float crownRatio = 0.5F;
                float expansionFactor = treeIndexAsFloat;
                if (treeIndex % 2 == 0)
                {
                    crownRatio = 0.01F;
                    expansionFactor = 0.0F;
                }
                trees.Add(1, treeIndex, treeIndexAsFloat, treeIndexAsFloat, crownRatio, expansionFactor);
                trees.DbhGrowth[treeIndex] = treeIndexAsFloat;
                trees.DeadExpansionFactor[treeIndex] = treeIndexAsFloat;
                trees.HeightGrowth[treeIndex] = treeIndexAsFloat;
            }
            Assert.IsTrue(trees.Count == treeCount);

            trees.RemoveZeroExpansionFactorTrees();
            Assert.IsTrue(trees.Count == treeCount / 2);
            for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
            {
                int tag = 2 * treeIndex + 1;
                float tagAsFloat = (float)tag;
                Assert.IsTrue(trees.CrownRatio[treeIndex] == 0.5F);
                Assert.IsTrue(trees.Dbh[treeIndex] == tagAsFloat);
                Assert.IsTrue(trees.DbhGrowth[treeIndex] == tagAsFloat);
                Assert.IsTrue(trees.DeadExpansionFactor[treeIndex] == tagAsFloat);
                Assert.IsTrue(trees.Height[treeIndex] == tagAsFloat);
                Assert.IsTrue(trees.HeightGrowth[treeIndex] == tagAsFloat);
                Assert.IsTrue(trees.LiveExpansionFactor[treeIndex] == tagAsFloat);
            }
            for (int treeIndex = trees.Count; treeIndex < trees.Capacity; ++treeIndex)
            {
                Assert.IsTrue(trees.LiveExpansionFactor[treeIndex] == 0.0F);
            }
        }
    }
}
