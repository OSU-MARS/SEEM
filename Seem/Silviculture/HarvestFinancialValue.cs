using Mars.Seem.Extensions;
using Mars.Seem.Optimization;
using Mars.Seem.Tree;
using System;
using System.Diagnostics;

namespace Mars.Seem.Silviculture
{
    public abstract class HarvestFinancialValue
    {
        public float HarvestRelatedTaskCostPerHa { get; protected set; } // US$/ha
        public float MerchantableCubicVolumePerHa { get; protected set; } // m³/ha
        public HarvestSystemEquipment MinimumCostHarvestSystem { get; protected set; }
        public float MinimumSystemCostPerHa { get; protected set; } // US$/ha
        public float NetPresentValuePerHa { get; protected set; } // US$/ha
        public float ReforestationNpv { get; private set; } // US$/ha
        public float PondValue2SawPerHa { get; private set; } // US$/ha
        public float PondValue3SawPerHa { get; private set; } // US$/ha
        public float PondValue4SawPerHa { get; private set; } // US$/ha

        protected HarvestFinancialValue()
        {
            this.HarvestRelatedTaskCostPerHa = Single.NaN;
            this.MerchantableCubicVolumePerHa = 0.0F;
            this.MinimumCostHarvestSystem = HarvestSystemEquipment.None;
            this.MinimumSystemCostPerHa = Single.NaN;
            this.NetPresentValuePerHa = Single.NaN;
            this.ReforestationNpv = Single.NaN;

            this.ClearNpvAndPond();
        }

        public abstract void CalculateProductivityAndCost(StandTrajectory trajectory, int harvestPeriod, bool isThin, HarvestSystems harvestSystems, float harvestCostPerHectare, float harvestTaskCostPerCubicMeter);

        protected void ClearNpvAndPond()
        {
            this.NetPresentValuePerHa = Single.NaN;
            this.PondValue2SawPerHa = 0.0F;
            this.PondValue3SawPerHa = 0.0F;
            this.PondValue4SawPerHa = 0.0F;
            this.ReforestationNpv = Single.NaN;
        }

        protected static Stand GetAndValidateStand(StandTrajectory trajectory, int harvestPeriod, bool isThin)
        {
            Stand? stand = trajectory.StandByPeriod[harvestPeriod - (isThin ? 1 : 0)] ?? throw new ArgumentOutOfRangeException(nameof(trajectory), "Stand at period " + harvestPeriod + " is null.");

            Debug.Assert((stand.AccessDistanceInM >= 0.0F) && (stand.AccessDistanceInM <= 5000.0F) &&
                         (stand.AccessSlopeInPercent >= 0.0F) && (stand.AccessSlopeInPercent <= 100.0F) &&
                         (stand.AreaInHa > 0.0F) && (stand.AreaInHa <= 1000.0F) &&
                         (stand.CorridorLengthInM > 0.0F) && (stand.CorridorLengthInM <= 2000.0F) &&
                         (stand.CorridorLengthInMTethered >= 0.0F) && (stand.CorridorLengthInMTethered <= 2000.0F) &&
                         (stand.CorridorLengthInMUntethered >= 0.0F) && (stand.CorridorLengthInMUntethered <= 2000.0F) &&
                         (stand.ForwardingDistanceOnRoad >= 0.0F) && (stand.ForwardingDistanceOnRoad <= 5000.0F) &&
                         (stand.MeanYardingDistanceFactor > 0.0F) && (stand.MeanYardingDistanceFactor <= 4.0F) &&
                         (stand.SlopeInPercent >= 0.0F) && (stand.SlopeInPercent <= 200.0F),
                         "Stand '" + stand.Name + "' has a nonphysical distance, area, or length.");
            
            // TODO: check if stand or access is steep enough to require tethering.
            //if (stand.CorridorLengthInMTethered > Constant.Maximum.TetheredCorridorLengthInM)
            //{
            //    throw new ArgumentOutOfRangeException(nameof(trajectory), "Stand at period " + harvestPeriod + " has a corridor length of " + stand.CorridorLengthInMTethered + " m, which exceeds the maximum tethered corridor length of " + Constant.Maximum.TetheredCorridorLengthInM + " m.");
            //}
            return stand;
        }

        public void SetNetPresentValue(float discountFactor, float reforestationNpv)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(reforestationNpv, 0.0F);

            float netValuePerHaAtHarvest = 0.0F;
            if (this.MerchantableCubicVolumePerHa > 0.0F)
            {
                netValuePerHaAtHarvest = this.PondValue2SawPerHa + this.PondValue3SawPerHa + this.PondValue4SawPerHa - this.MinimumSystemCostPerHa - this.HarvestRelatedTaskCostPerHa;
            }

            this.NetPresentValuePerHa = discountFactor * netValuePerHaAtHarvest;
            if (Single.IsNaN(reforestationNpv) == false)
            {
                // reforestation still required in edge case where all merchantable trees were thinned
                this.NetPresentValuePerHa += reforestationNpv;
            }
            this.ReforestationNpv = reforestationNpv;

            Debug.Assert((this.NetPresentValuePerHa > -100.0F * 1000.0F) && (this.NetPresentValuePerHa < 1000.0F * 1000.0F));
        }

        protected static void ThrowIfTreeNotAutomaticReserve(bool isThin, Trees treesOfSpecies, int compactedTreeIndex, TreeSpeciesMerchantableVolumeTable volumeTable)
        {
            if (isThin)
            {
                // automatic reserve trees should never be marked for thinning: this indicates a tree selection bug in a heuristic or
                // silvicultural prescription
                (float diameterToCentimetersMultiplier, float _, float _) = UnitsExtensions.GetConversionToMetric(treesOfSpecies.Units);
                throw new NotSupportedException(treesOfSpecies.Species + " " + treesOfSpecies.Tag[compactedTreeIndex] + " with DBH of " + diameterToCentimetersMultiplier * treesOfSpecies.Dbh[compactedTreeIndex] + " cm is selected for thinning. This tree exceeds the long log volume table's DBH limit of " + volumeTable.MaximumMerchantableDiameterInCentimeters + " cm.");
            }

            // otherwise, for now, assume tree was initially small enough not to be automatic reserve but has grown across the
            // automatic reserve threshold
        }

        public abstract bool TryAddMerchantableVolume(StandTrajectory trajectory, int harvestPeriod, FinancialScenarios financialScenarios, int financialIndex, float pondValueMultiplier);

        protected bool TryAddMerchantableVolume(TreeSpeciesMerchantableVolume harvestVolumeForSpecies, int harvestPeriod, FinancialScenarios financialScenarios, int financialIndex, float pondValueMultiplier)
        {
            // check for nonzero cubic volume removal
            Debug.Assert(harvestVolumeForSpecies.IsCalculated(harvestPeriod));
            float cubic2Saw = harvestVolumeForSpecies.Cubic2Saw[harvestPeriod];
            float cubic3Saw = harvestVolumeForSpecies.Cubic3Saw[harvestPeriod];
            float cubic4Saw = harvestVolumeForSpecies.Cubic4Saw[harvestPeriod];
            float cubicVolumeFromSpecies = cubic2Saw + cubic3Saw + cubic4Saw;
            if (cubicVolumeFromSpecies == 0.0F)
            {
                return false; // nothing to do
            }

            // account for volume harvested
            this.MerchantableCubicVolumePerHa += cubicVolumeFromSpecies;

            // net pond value: US$/Scribner.C MBF
            (float pondValue2SawAfterTax, float pondValue3SawAfterTax, float pondValue4SawAfterTax) = financialScenarios.GetPondValueAfterTax(harvestVolumeForSpecies.Species, financialIndex);

            // NPV = scale adjustment * appreciation * $/MBF * MBF/ha - $/m³ * m³/ha = $/ha
            float scribner2saw = harvestVolumeForSpecies.Scribner2Saw[harvestPeriod]; // MBF/ha
            this.PondValue2SawPerHa += pondValueMultiplier * pondValue2SawAfterTax * scribner2saw;

            float scribner3saw = harvestVolumeForSpecies.Scribner3Saw[harvestPeriod];
            this.PondValue3SawPerHa += pondValueMultiplier * pondValue3SawAfterTax * scribner3saw;

            float scribner4saw = harvestVolumeForSpecies.Scribner4Saw[harvestPeriod];
            this.PondValue4SawPerHa += pondValueMultiplier * pondValue4SawAfterTax * scribner4saw;

            return true;
        }
    }
}
