using Mars.Seem.Optimization;
using Mars.Seem.Tree;

namespace Mars.Seem.Silviculture
{
    public class LongLogThin : LongLogHarvest
    {
        public override bool TryAddMerchantableVolume(StandTrajectory trajectory, int harvestPeriod, FinancialScenarios financialScenarios, int financialIndex, float appreciationFactor)
        {
            this.ClearNpvAndPond();

            bool merchantableVolumeAdded = false;
            for (int treeSpeciesIndex = 0; treeSpeciesIndex < trajectory.LongLogThinVolumeBySpecies.Count; ++treeSpeciesIndex)
            {
                TreeSpeciesMerchantableVolume harvestVolumeForSpecies = trajectory.LongLogThinVolumeBySpecies.Values[treeSpeciesIndex];
                merchantableVolumeAdded |= this.TryAddMerchantableVolume(harvestVolumeForSpecies, harvestPeriod, financialScenarios, financialIndex, appreciationFactor);
            }

            return merchantableVolumeAdded;
        }
    }
}
