using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mars.Seem.Extensions;
using Mars.Seem.Optimization;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;

namespace Mars.Seem.Test
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
            financialScenarios.Read("financial scenarios.xlsx", "parameterization");
            int expectedCount = 3;

            // properties of FinancialScenario
            Assert.IsTrue(financialScenarios.Count == expectedCount);
            Assert.IsTrue(financialScenarios.DiscountRate.Count == expectedCount);
            Assert.IsTrue(financialScenarios.DouglasFir2SawPondValuePerMbf.Count == expectedCount);
            Assert.IsTrue(financialScenarios.DouglasFir3SawPondValuePerMbf.Count == expectedCount);
            Assert.IsTrue(financialScenarios.DouglasFir4SawPondValuePerMbf.Count == expectedCount);
            Assert.IsTrue(financialScenarios.HarvestSystems.Count == expectedCount);
            Assert.IsTrue(financialScenarios.HarvestTaxPerMbf.Count == expectedCount);
            Assert.IsTrue(financialScenarios.Name.Count == expectedCount);
            Assert.IsTrue(financialScenarios.PropertyTaxAndManagementPerHectareYear.Count == expectedCount);
            Assert.IsTrue(financialScenarios.RegenerationHarvestCostPerHectare.Count == expectedCount);
            Assert.IsTrue(financialScenarios.ReleaseSprayCostPerHectare.Count == expectedCount);
            Assert.IsTrue(financialScenarios.SeedlingCost.Count == expectedCount);
            Assert.IsTrue(financialScenarios.SitePrepAndReplantingCostPerHectare.Count == expectedCount);
            Assert.IsTrue(financialScenarios.ThinningHarvestCostPerHectare.Count == expectedCount);
            Assert.IsTrue(financialScenarios.ThinningPondValueMultiplier.Count == expectedCount);
            Assert.IsTrue(financialScenarios.TimberAppreciationRate.Count == expectedCount);
            Assert.IsTrue(financialScenarios.WesternRedcedarCamprunPondValuePerMbf.Count == expectedCount);
            Assert.IsTrue(financialScenarios.WhiteWood2SawPondValuePerMbf.Count == expectedCount);
            Assert.IsTrue(financialScenarios.WhiteWood3SawPondValuePerMbf.Count == expectedCount);
            Assert.IsTrue(financialScenarios.WhiteWood4SawPondValuePerMbf.Count == expectedCount);

            for (int financialIndex = 0; financialIndex < expectedCount; ++financialIndex)
            {
                Assert.IsTrue(financialScenarios.DiscountRate[financialIndex] == Constant.Financial.DefaultAnnualDiscountRate);
                Assert.IsTrue((financialScenarios.DouglasFir2SawPondValuePerMbf[financialIndex] < 750.0F) &&
                              (financialScenarios.DouglasFir2SawPondValuePerMbf[financialIndex] > financialScenarios.DouglasFir3SawPondValuePerMbf[financialIndex]));
                Assert.IsTrue((financialScenarios.DouglasFir3SawPondValuePerMbf[financialIndex] <= financialScenarios.DouglasFir2SawPondValuePerMbf[financialIndex]) &&
                              (financialScenarios.DouglasFir3SawPondValuePerMbf[financialIndex] > financialScenarios.DouglasFir4SawPondValuePerMbf[financialIndex]));
                Assert.IsTrue((financialScenarios.DouglasFir4SawPondValuePerMbf[financialIndex] <= financialScenarios.DouglasFir3SawPondValuePerMbf[financialIndex]) &&
                              (financialScenarios.DouglasFir4SawPondValuePerMbf[financialIndex] > 500.0F));
                Assert.IsTrue((financialScenarios.HarvestTaxPerMbf.Count == expectedCount) &&
                              (financialScenarios.HarvestTaxPerMbf[financialIndex] == Constant.Financial.OregonForestProductsHarvestTax));
                Assert.IsTrue((financialScenarios.Name.Count == expectedCount) &&
                              (String.IsNullOrWhiteSpace(financialScenarios.Name[financialIndex]) == false));
                Assert.IsTrue((financialScenarios.PropertyTaxAndManagementPerHectareYear[financialIndex] < 75.0F) &&
                              (financialScenarios.PropertyTaxAndManagementPerHectareYear[financialIndex] > 30.0F));
                Assert.IsTrue((financialScenarios.RegenerationHarvestCostPerHectare[financialIndex] < 800.0F) &&
                              (financialScenarios.RegenerationHarvestCostPerHectare[financialIndex] > 400.0F));
                Assert.IsTrue((financialScenarios.ReleaseSprayCostPerHectare[financialIndex] < 500.0F) &&
                              (financialScenarios.ReleaseSprayCostPerHectare[financialIndex] > 200.0F));
                Assert.IsTrue((financialScenarios.SeedlingCost[financialIndex] < 2.00F) &&
                              (financialScenarios.SeedlingCost[financialIndex] > 0.25F));
                Assert.IsTrue((financialScenarios.SitePrepAndReplantingCostPerHectare[financialIndex] < 1000.0F) &&
                              (financialScenarios.SitePrepAndReplantingCostPerHectare[financialIndex] > 500.0F));
                Assert.IsTrue((financialScenarios.ThinningHarvestCostPerHectare[financialIndex] < 500.0F) &&
                              (financialScenarios.ThinningHarvestCostPerHectare[financialIndex] > 250.0F));
                Assert.IsTrue((financialScenarios.ThinningPondValueMultiplier[financialIndex] <= 1.00F) &&
                              (financialScenarios.ThinningPondValueMultiplier[financialIndex] > 0.50F));
                Assert.IsTrue((financialScenarios.TimberAppreciationRate[financialIndex] <= 0.03F) &&
                              (financialScenarios.TimberAppreciationRate[financialIndex] >= 0.00F));
                Assert.IsTrue((financialScenarios.WesternRedcedarCamprunPondValuePerMbf[financialIndex] < 2200.0F) &&
                              (financialScenarios.WesternRedcedarCamprunPondValuePerMbf[financialIndex] > 500.0F));
                Assert.IsTrue((financialScenarios.WhiteWood2SawPondValuePerMbf[financialIndex] < 600.0F) &&
                              (financialScenarios.WhiteWood2SawPondValuePerMbf[financialIndex] > financialScenarios.WhiteWood3SawPondValuePerMbf[financialIndex]));
                Assert.IsTrue((financialScenarios.WhiteWood3SawPondValuePerMbf[financialIndex] <= financialScenarios.WhiteWood2SawPondValuePerMbf[financialIndex]) &&
                              (financialScenarios.WhiteWood3SawPondValuePerMbf[financialIndex] > financialScenarios.WhiteWood4SawPondValuePerMbf[financialIndex]));
                Assert.IsTrue((financialScenarios.WhiteWood4SawPondValuePerMbf[financialIndex] <= financialScenarios.WhiteWood3SawPondValuePerMbf[financialIndex]) &&
                              (financialScenarios.WhiteWood4SawPondValuePerMbf[financialIndex] > 350.0F));
            }

            // regeneration harvest system
            for (int financialIndex = 0; financialIndex < expectedCount; ++financialIndex)
            {
                HarvestSystems harvestSystems = financialScenarios.HarvestSystems[financialIndex];
                Assert.IsTrue((harvestSystems.AddOnWinchCableLengthInM < 351.0F) &&
                              (harvestSystems.AddOnWinchCableLengthInM > 349.0F));
                Assert.IsTrue((harvestSystems.AnchorCostPerSMh < 75.0F) &&
                              (harvestSystems.AnchorCostPerSMh > 70.0F));
                Assert.IsTrue((harvestSystems.ChainsawBuckConstant < 100.0F) &&
                              (harvestSystems.ChainsawBuckConstant > 20.0F));
                Assert.IsTrue((harvestSystems.ChainsawBuckCostPerSMh < 125.0F) &&
                              (harvestSystems.ChainsawBuckCostPerSMh > 60.0F));
                Assert.IsTrue((harvestSystems.ChainsawBuckLinear < 100.0F) &&
                              (harvestSystems.ChainsawBuckLinear > 10.0F));
                Assert.IsTrue((harvestSystems.ChainsawBuckUtilization <= 1.0F) &&
                              (harvestSystems.ChainsawBuckUtilization > 0.5F));
                Assert.IsTrue((harvestSystems.ChainsawBuckQuadratic < 50.0F) &&
                              (harvestSystems.ChainsawBuckQuadratic > 10.0F));
                Assert.IsTrue((harvestSystems.ChainsawBuckQuadraticThreshold < 2.0F) &&
                              (harvestSystems.ChainsawBuckQuadraticThreshold > 0.5F));
                Assert.IsTrue((harvestSystems.ChainsawFellAndBuckCostPerSMh < 200.0F) &&
                              (harvestSystems.ChainsawFellAndBuckCostPerSMh > 100.0F));
                Assert.IsTrue((harvestSystems.ChainsawFellAndBuckConstant < 150.0F) &&
                              (harvestSystems.ChainsawFellAndBuckConstant > 30.0F));
                Assert.IsTrue((harvestSystems.ChainsawFellAndBuckLinear < 120.0F) &&
                              (harvestSystems.ChainsawFellAndBuckLinear > 20.0F));
                Assert.IsTrue((harvestSystems.ChainsawFellAndBuckUtilization < 1.0F) &&
                              (harvestSystems.ChainsawFellAndBuckUtilization > 0.1F));
                Assert.IsTrue((harvestSystems.ChainsawSlopeLinear < 0.1F) &&
                              (harvestSystems.ChainsawSlopeLinear > 0.0F));
                Assert.IsTrue((harvestSystems.ChainsawSlopeThresholdInPercent < 70.0F) &&
                              (harvestSystems.ChainsawSlopeThresholdInPercent > 30.0F));

                Assert.IsTrue((harvestSystems.CorridorWidth > 4.0F) && // machine width + movement variability
                              (harvestSystems.CorridorWidth < 23.0F)); // machine reach

                Assert.IsTrue((harvestSystems.CutToLengthHaulPayloadInKg < 30000.0F) &&
                              (harvestSystems.CutToLengthHaulPayloadInKg > 28000.0F));
                Assert.IsTrue((harvestSystems.CutToLengthHaulPerSMh < 145.0F) &&
                              (harvestSystems.CutToLengthHaulPerSMh > 95.0F));
                Assert.IsTrue((harvestSystems.CutToLengthRoundtripHaulSMh < 4.5F) &&
                              (harvestSystems.CutToLengthRoundtripHaulSMh > 2.5F));

                Assert.IsTrue((harvestSystems.FellerBuncherFellingConstant < 100.0F) &&
                              (harvestSystems.FellerBuncherFellingConstant > 5.0F));
                Assert.IsTrue((harvestSystems.FellerBuncherFellingLinear < 25.0F) &&
                              (harvestSystems.FellerBuncherFellingLinear > 1.0F));
                Assert.IsTrue((harvestSystems.FellerBuncherCostPerSMh < 500.0F) &&
                              (harvestSystems.FellerBuncherCostPerSMh > 100.0F));
                Assert.IsTrue((harvestSystems.FellerBuncherSlopeLinear < 0.1F) &&
                              (harvestSystems.FellerBuncherSlopeLinear > 0.0F));
                Assert.IsTrue((harvestSystems.FellerBuncherSlopeThresholdInPercent < 65.0F) &&
                              (harvestSystems.FellerBuncherSlopeThresholdInPercent > 20.0F));

                Assert.IsTrue((harvestSystems.ForwarderCostPerSMh < 500.0F) &&
                              (harvestSystems.ForwarderCostPerSMh > 100.0F));
                Assert.IsTrue((harvestSystems.ForwarderDriveWhileLoadingLogs < 0.9F) &&
                              (harvestSystems.ForwarderDriveWhileLoadingLogs > 0.7F));
                Assert.IsTrue((harvestSystems.ForwarderEmptyWeight <= 30000.0F) &&
                              (harvestSystems.ForwarderEmptyWeight > 15000.0F));
                Assert.IsTrue((harvestSystems.ForwarderLoadMeanLogVolume < 0.7F) &&
                              (harvestSystems.ForwarderLoadMeanLogVolume > 0.5F));
                Assert.IsTrue((harvestSystems.ForwarderLoadPayload < 1.1F) &&
                              (harvestSystems.ForwarderLoadPayload > 0.9F));
                Assert.IsTrue((harvestSystems.ForwarderMaximumPayloadInKg <= 20000.0F) &&
                              (harvestSystems.ForwarderMaximumPayloadInKg > 15000.0F));
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
                Assert.IsTrue((harvestSystems.ForwarderTractiveForce < 250.0F) &&
                              (harvestSystems.ForwarderTractiveForce > 100.0F));
                Assert.IsTrue((harvestSystems.ForwarderUnloadLinearOneSort < harvestSystems.ForwarderUnloadLinearTwoSorts) &&
                              (harvestSystems.ForwarderUnloadLinearOneSort > 0.4F));
                Assert.IsTrue((harvestSystems.ForwarderUnloadLinearTwoSorts < harvestSystems.ForwarderUnloadLinearThreeSorts) &&
                              (harvestSystems.ForwarderUnloadLinearTwoSorts > harvestSystems.ForwarderUnloadLinearOneSort));
                Assert.IsTrue((harvestSystems.ForwarderUnloadLinearThreeSorts < 1.0F) &&
                              (harvestSystems.ForwarderUnloadLinearThreeSorts > harvestSystems.ForwarderUnloadLinearTwoSorts));
                Assert.IsTrue((harvestSystems.ForwarderUnloadMeanLogVolume < 0.6F) &&
                              (harvestSystems.ForwarderUnloadMeanLogVolume > 0.4F));
                Assert.IsTrue((harvestSystems.ForwarderUnloadPayload < 0.7F) &&
                              (harvestSystems.ForwarderUnloadPayload > 0.5F));
                Assert.IsTrue((harvestSystems.ForwarderUtilization < 1.0F) &&
                              (harvestSystems.ForwarderUtilization > 0.5F));

                Assert.IsTrue((harvestSystems.GrappleYardingConstant < 500.0F) &&
                              (harvestSystems.GrappleYardingConstant > 0.0F));
                Assert.IsTrue((harvestSystems.GrappleYardingLinear < 5.0F) &&
                              (harvestSystems.GrappleYardingLinear > 0.0F));
                Assert.IsTrue((harvestSystems.GrappleSwingYarderCostPerSMh < 500.0F) &&
                              (harvestSystems.GrappleSwingYarderCostPerSMh > 100.0F));
                Assert.IsTrue((harvestSystems.GrappleSwingYarderMaxPayload <= 8000.0F) &&
                              (harvestSystems.GrappleSwingYarderMaxPayload >= harvestSystems.GrappleSwingYarderMeanPayload));
                Assert.IsTrue((harvestSystems.GrappleSwingYarderMeanPayload <= harvestSystems.GrappleSwingYarderMaxPayload) &&
                              (harvestSystems.GrappleSwingYarderMeanPayload >= 1000.0F));
                Assert.IsTrue((harvestSystems.GrappleSwingYarderUtilization < 1.0F) &&
                              (harvestSystems.GrappleSwingYarderUtilization > 0.5F));
                Assert.IsTrue((harvestSystems.GrappleYoaderCostPerSMh < 500.0F) &&
                              (harvestSystems.GrappleYoaderCostPerSMh > 100.0F));
                Assert.IsTrue((harvestSystems.GrappleYoaderMaxPayload <= 4500.0F) &&
                              (harvestSystems.GrappleYoaderMaxPayload >= harvestSystems.GrappleYoaderMeanPayload));
                Assert.IsTrue((harvestSystems.GrappleYoaderMeanPayload <= harvestSystems.GrappleYoaderMaxPayload) &&
                              (harvestSystems.GrappleYoaderMeanPayload >= 1000.0F));
                Assert.IsTrue((harvestSystems.GrappleYoaderUtilization < 1.0F) &&
                              (harvestSystems.GrappleYoaderUtilization > 0.5F));

                Assert.IsTrue((harvestSystems.LoaderCostPerSMh < 500.0F) &&
                              (harvestSystems.LoaderCostPerSMh > 100.0F));
                Assert.IsTrue((harvestSystems.LoaderProductivity < 80000.0F) &&
                              (harvestSystems.LoaderProductivity > 20000.0F));
                Assert.IsTrue((harvestSystems.LoaderUtilization < 1.0F) &&
                              (harvestSystems.LoaderUtilization > 0.5F));

                Assert.IsTrue((harvestSystems.LongLogHaulPayloadInKg < 27000.0F) &&
                              (harvestSystems.LongLogHaulPayloadInKg > 25000.0F));
                Assert.IsTrue((harvestSystems.LongLogHaulPerSMh < 125.0F) &&
                              (harvestSystems.LongLogHaulPerSMh > 75.0F));
                Assert.IsTrue((harvestSystems.LongLogHaulRoundtripSMh < 4.5F) &&
                              (harvestSystems.LongLogHaulRoundtripSMh > 2.5F));

                Assert.IsTrue((harvestSystems.MachineMoveInOrOut < 600F) &&
                              (harvestSystems.MachineMoveInOrOut > 400F));

                Assert.IsTrue((harvestSystems.ProcessorBuckConstant < 100.0F) &&
                              (harvestSystems.ProcessorBuckConstant > 10.0F));
                Assert.IsTrue((harvestSystems.ProcessorBuckLinear < 100.0F) &&
                              (harvestSystems.ProcessorBuckLinear > 10.0F));
                Assert.IsTrue((harvestSystems.ProcessorBuckQuadratic1 < 10.0F) &&
                              (harvestSystems.ProcessorBuckQuadratic1 > 1.0F));
                Assert.IsTrue((harvestSystems.ProcessorBuckQuadratic2 < 10.0F) &&
                              (harvestSystems.ProcessorBuckQuadratic2 > 1.0F));
                Assert.IsTrue((harvestSystems.ProcessorBuckQuadraticThreshold1 < 10.0F) &&
                              (harvestSystems.ProcessorBuckQuadraticThreshold1 > 1.0F));
                Assert.IsTrue((harvestSystems.ProcessorBuckQuadraticThreshold2 < 10.0F) &&
                              (harvestSystems.ProcessorBuckQuadraticThreshold2 > 1.0F));
                Assert.IsTrue((harvestSystems.ProcessorCostPerSMh < 500.0F) &&
                              (harvestSystems.ProcessorCostPerSMh > 100.0F));
                Assert.IsTrue((harvestSystems.ProcessorUtilization < 1.0F) &&
                              (harvestSystems.ProcessorUtilization > 0.5F));

                Assert.IsTrue((harvestSystems.TrackedHarvesterCostPerSMh < 500.0F) &&
                              (harvestSystems.TrackedHarvesterCostPerSMh > 100.0F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterFellAndBuckConstant < 100.0F) &&
                              (harvestSystems.TrackedHarvesterFellAndBuckConstant > 10.0F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterFellAndBuckDiameterLimit < 90.0F) &&
                              (harvestSystems.TrackedHarvesterFellAndBuckDiameterLimit > 30.0F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterFellAndBuckLinear < 100.0F) &&
                              (harvestSystems.TrackedHarvesterFellAndBuckLinear > 10.0F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterFellAndBuckQuadratic1 < 10.0F) &&
                              (harvestSystems.TrackedHarvesterFellAndBuckQuadratic1 > 1.0F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterFellAndBuckQuadratic2 < 10.0F) &&
                              (harvestSystems.TrackedHarvesterFellAndBuckQuadratic2 > 1.0F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterQuadraticThreshold1 < 5.0F) &&
                              (harvestSystems.TrackedHarvesterQuadraticThreshold1 > 0.5F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterQuadraticThreshold2 < 7.5F) &&
                              (harvestSystems.TrackedHarvesterQuadraticThreshold2 > 0.5F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterSlopeLinear < 0.1F) &&
                              (harvestSystems.TrackedHarvesterSlopeLinear > 0.0F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterSlopeThresholdInPercent < 65.0F) &&
                              (harvestSystems.TrackedHarvesterSlopeThresholdInPercent > 20.0F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterUtilization <= 1.0F) &&
                              (harvestSystems.TrackedHarvesterUtilization > 0.0F));

                Assert.IsTrue((harvestSystems.WheeledHarvesterCostPerSMh < 500.0F) &&
                              (harvestSystems.WheeledHarvesterCostPerSMh > 100.0F));
                Assert.IsTrue((harvestSystems.WheeledHarvesterFellAndBuckConstant < 100.0F) &&
                              (harvestSystems.WheeledHarvesterFellAndBuckConstant > 10.0F));
                Assert.IsTrue((harvestSystems.WheeledHarvesterFellAndBuckDiameterLimit < 90.0F) &&
                              (harvestSystems.WheeledHarvesterFellAndBuckDiameterLimit > 30.0F));
                Assert.IsTrue((harvestSystems.WheeledHarvesterFellAndBuckLinear < 100.0F) &&
                              (harvestSystems.WheeledHarvesterFellAndBuckLinear > 10.0F));
                Assert.IsTrue((harvestSystems.WheeledHarvesterFellAndBuckQuadratic < 10.0F) &&
                              (harvestSystems.WheeledHarvesterFellAndBuckQuadratic > 1.0F));
                Assert.IsTrue((harvestSystems.WheeledHarvesterQuadraticThreshold < 5.0F) &&
                              (harvestSystems.WheeledHarvesterQuadraticThreshold > 0.5F));
                Assert.IsTrue((harvestSystems.WheeledHarvesterSlopeLinear < 0.1F) &&
                              (harvestSystems.WheeledHarvesterSlopeLinear > 0.0F));
                Assert.IsTrue((harvestSystems.WheeledHarvesterSlopeThresholdInPercent < 65.0F) &&
                              (harvestSystems.WheeledHarvesterSlopeThresholdInPercent > 20.0F));
            }
        }

        [TestMethod]
        public void Native()
        {
            ProcessorPowerInformation powerInfo = NativeMethods.CallNtPowerInformation();
            int highestMaxMHz = Int32.MinValue;
            for (int thread = 0; thread < powerInfo.Threads; ++thread)
            {
                int currentIdleState = powerInfo.CurrentIdleState[thread];
                int currentMHz = powerInfo.CurrentMHz[thread];
                int maxMHz = powerInfo.MaxMHz[thread];
                int mhzLimit = powerInfo.MHzLimit[thread];
                int maxIdleState = powerInfo.MaxIdleState[thread];

                Assert.IsTrue((currentIdleState >= 0) && (currentIdleState <= maxIdleState));
                Assert.IsTrue((currentMHz >= 0) && (currentMHz <= maxMHz));
                Assert.IsTrue((maxMHz >= 1.5 * 1000) && (maxMHz <= 7.5 * 1000)); // 1.5-7.5 GHz: admit 1.6 GHz laptops to 5+ GHz desktops
                Assert.IsTrue((mhzLimit >= currentMHz) && (mhzLimit <= maxMHz));
                Assert.IsTrue((maxIdleState >= 0) && (maxIdleState < 4));

                if (highestMaxMHz < maxMHz)
                {
                    highestMaxMHz = maxMHz;
                }
            }

            float referenceGHz = powerInfo.GetPerformanceFrequencyInGHz();
            Assert.IsTrue((referenceGHz > 0.0009991F * highestMaxMHz) && (referenceGHz < 0.001001F * highestMaxMHz));
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
