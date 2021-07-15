using Microsoft.VisualStudio.TestTools.UnitTesting;
using Osu.Cof.Ferm.Extensions;
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

            Assert.IsTrue(financialScenarios.Count == 1);
            Assert.IsTrue((financialScenarios.DiscountRate.Count == 1) && (financialScenarios.DiscountRate[0] > 0.0F));
            Assert.IsTrue((financialScenarios.DouglasFir2SawPondValuePerMbf.Count == 1) && (financialScenarios.DouglasFir2SawPondValuePerMbf[0] > 0.0F));
            Assert.IsTrue((financialScenarios.DouglasFir3SawPondValuePerMbf.Count == 1) && (financialScenarios.DouglasFir3SawPondValuePerMbf[0] > 0.0F));
            Assert.IsTrue((financialScenarios.DouglasFir4SawPondValuePerMbf.Count == 1) && (financialScenarios.DouglasFir4SawPondValuePerMbf[0] > 0.0F));
            Assert.IsTrue((financialScenarios.HarvestTaxPerMbf.Count == 1) && (financialScenarios.HarvestTaxPerMbf[0] > 0.0F));
            Assert.IsTrue((financialScenarios.Name.Count == 1) && (String.IsNullOrWhiteSpace(financialScenarios.Name[0]) == false));
            Assert.IsTrue((financialScenarios.PropertyTaxAndManagementPerHectareYear.Count == 1) && (financialScenarios.PropertyTaxAndManagementPerHectareYear[0] > 0.0F));
            Assert.IsTrue((financialScenarios.RegenerationHarvestCostPerCubicMeter.Count == 1) && (financialScenarios.RegenerationHarvestCostPerCubicMeter[0] > 0.0F));
            Assert.IsTrue((financialScenarios.RegenerationHarvestCostPerHectare.Count == 1) && (financialScenarios.RegenerationHarvestCostPerHectare[0] > 0.0F));
            Assert.IsTrue((financialScenarios.RegenerationHaulCostPerCubicMeter.Count == 1) && (financialScenarios.RegenerationHaulCostPerCubicMeter[0] > 0.0F));
            Assert.IsTrue((financialScenarios.ReleaseSprayCostPerHectare.Count == 1) && (financialScenarios.ReleaseSprayCostPerHectare[0] > 0.0F));
            Assert.IsTrue((financialScenarios.SeedlingCost.Count == 1) && (financialScenarios.SeedlingCost[0] > 0.0F));
            Assert.IsTrue((financialScenarios.SitePrepAndReplantingCostPerHectare.Count == 1) && (financialScenarios.SitePrepAndReplantingCostPerHectare[0] > 0.0F));
            Assert.IsTrue((financialScenarios.ThinningHarvestCostPerCubicMeter.Count == 1) && (financialScenarios.ThinningHarvestCostPerCubicMeter[0] > 0.0F));
            Assert.IsTrue((financialScenarios.ThinningHarvestCostPerHectare.Count == 1) && (financialScenarios.ThinningHarvestCostPerHectare[0] > 0.0F));
            Assert.IsTrue((financialScenarios.ThinningHaulCostPerCubicMeter.Count == 1) && (financialScenarios.ThinningHaulCostPerCubicMeter[0] > 0.0F));
            Assert.IsTrue((financialScenarios.ThinningPondValueMultiplier.Count == 1) && (financialScenarios.ThinningPondValueMultiplier[0] > 0.0F));
            Assert.IsTrue((financialScenarios.TimberAppreciationRate.Count == 1) && (financialScenarios.TimberAppreciationRate[0] >= 0.0F));
            Assert.IsTrue((financialScenarios.WesternRedcedarCamprunPondValuePerMbf.Count == 1) && (financialScenarios.WesternRedcedarCamprunPondValuePerMbf[0] > 0.0F));
            Assert.IsTrue((financialScenarios.WhiteWood2SawPondValuePerMbf.Count == 1) && (financialScenarios.WhiteWood2SawPondValuePerMbf[0] > 0.0F));
            Assert.IsTrue((financialScenarios.WhiteWood3SawPondValuePerMbf.Count == 1) && (financialScenarios.WhiteWood3SawPondValuePerMbf[0] > 0.0F));
            Assert.IsTrue((financialScenarios.WhiteWood4SawPondValuePerMbf.Count == 1) && (financialScenarios.WhiteWood4SawPondValuePerMbf[0] > 0.0F));
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
