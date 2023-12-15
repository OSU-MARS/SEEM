using Mars.Seem.Extensions;
using Mars.Seem.Optimization;
using Mars.Seem.Tree;
using System;
using System.Diagnostics;

namespace Mars.Seem.Silviculture
{
    public class CutToLengthHarvest : HarvestFinancialValue
    {
        public Forwarder Forwarder { get; private init; }
        public HarvesterSystem TrackedHarvester { get; private init; }
        public HarvesterSystem WheeledHarvester { get; private init; }

        public CutToLengthHarvest()
        {
            this.Forwarder = new();
            this.TrackedHarvester = new();
            this.WheeledHarvester = new();
        }

        /// <param name="stand">Stand at beginning of harvest period if thinning, stand at end of harvest period if regeneration harvest.</param>
        private void CalculatePMh(Stand stand, StandTrajectory trajectory, int harvestPeriod, bool isThin, HarvestSystems harvestSystems)
        {
            // BC Firmwood scaling considers trim to be merchantable cubic volume (Fonseca 2005 §2.2.2.2) so, for now, merchantableFractionOfLogLength = 1
            // float preferredLogLength = scaledVolume.PreferredLogLengthInMeters;
            // float preferredTrimLength = scaledVolume.GetPreferredTrim();
            // float merchantableFractionOfLogLength = preferredLogLength / (preferredLogLength + preferredTrimLength); // assumes cylindrical logs or cylindrical ones with equal amounts of trim at both ends
            bool previousOversizeTreeBehindHarvester = true;
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                if (trajectory.TreeScaling.TryGetForwarderVolumeTable(treesOfSpecies.Species, out TreeSpeciesMerchantableVolumeTable? forwardedVolumeTable) == false)
                {
                    // for now, assume all non-merchantable trees are left in place in stand
                    // TODO: include felling and handling costs for cut and leave
                    continue;
                }

                IndividualTreeSelection individualTreeSelection = trajectory.TreeSelectionBySpecies[treesOfSpecies.Species];
                (float diameterToCmMultiplier, float heightToMetersMultiplier, float expansionFactorMultiplier) = UnitsExtensions.GetConversionToMetric(treesOfSpecies.Units);

                TreeSpeciesProperties treeSpeciesProperties = TreeSpecies.Properties[treesOfSpecies.Species];
                for (int compactedTreeIndex = 0; compactedTreeIndex < treesOfSpecies.Count; ++compactedTreeIndex)
                {
                    int uncompactedTreeIndex = treesOfSpecies.UncompactedIndex[compactedTreeIndex];
                    if (individualTreeSelection[uncompactedTreeIndex] != harvestPeriod)
                    {
                        // tree is not included in this harvest
                        continue;
                    }

                    // Could factor constant and linear terms out of loop (true for all machines, both CTL and long log) but it doesn't
                    // appear important to do so.
                    float dbhInCm = diameterToCmMultiplier * treesOfSpecies.Dbh[compactedTreeIndex];
                    if (dbhInCm > forwardedVolumeTable.MaximumMerchantableDiameterInCentimeters)
                    {
                        HarvestFinancialValue.ThrowIfTreeNotAutomaticReserve(isThin, treesOfSpecies, compactedTreeIndex, forwardedVolumeTable);
                        continue;
                    }

                    float heightInM = heightToMetersMultiplier * treesOfSpecies.Height[compactedTreeIndex];
                    float expansionFactorPerHa = expansionFactorMultiplier * treesOfSpecies.LiveExpansionFactor[compactedTreeIndex];
                    float treeMerchantableVolumeInM3 = forwardedVolumeTable.GetCubicVolumeOfMerchantableWood(dbhInCm, heightInM, out float unscaledNeiloidVolumeInM3);
                    Debug.Assert(Single.IsNaN(treeMerchantableVolumeInM3) == false);
                    float treeMerchantableVolumePerHa = expansionFactorPerHa * treeMerchantableVolumeInM3;

                    // tracked harvester
                    (float treeHarvesterPMsWithTrackedHarvester, float treeChainsawPMsWithTrackedHarvester) = harvestSystems.GetTrackedHarvesterTime(dbhInCm, treeMerchantableVolumeInM3, previousOversizeTreeBehindHarvester);
                    float treeTrackedHarvesterPMsPerHa = expansionFactorPerHa * treeHarvesterPMsWithTrackedHarvester;
                    if (treeChainsawPMsWithTrackedHarvester == 0.0F)
                    {
                        // tree felled and bucked by harvester, no chainsaw use
                        this.TrackedHarvester.AddTree(treeTrackedHarvesterPMsPerHa);
                        // forwarded weight is added in wheeled harvester case
                        // Assumption is tracked and wheeled harvesters remove the same amount of bark during felling and bucking.
                    }
                    else
                    {
                        // a chainsaw is used on this tree, so it applies towards chainsaw utilization
                        // For now, assume all CTL harvests are thins and that the harvester operates in two scenarios occuring with equal
                        // probability.
                        //
                        //   1) A tree requiring chainsaw work is encountered behind the harvester's direction of progress across the stand. In
                        //      this case falling the tree away from the harvester does not impede the harvester in subsequent corridors and it's
                        //      unimportant whether the harvester operator or a separate chainsaw crew bucks the tree.
                        //   2) The tree is ahead of harvester, in which case falling it away from the harvester is unlikely to affect operations
                        //      in the current corridor but does block subsequent corridors. In this case either the operator has to buck the 
                        //      tree and move the logs out of the way with the harvester's processing head or leave the tree standing for falling
                        //      once the harvester has passed, either by the operator from the next corridor or independently by a falling crew.
                        //
                        // Since modelling is currently nonspatial, occurence of these two cases is approximated by a toggle forcing every other
                        // tree too large for the processing head to be manually felled. In a spatial model whether the tree needs to be manually
                        // felled would be determined by its position with respect to the harvester and whether the cutting pattern allows it to
                        // be felled with the processing head. In spatial modelling edge effects, such as needing to fell trees into a unit rather
                        // than across a property line or onto smaller trees in an adjacent stand, can also be considered.
                        float treeBasalAreaInM2PerHa = expansionFactorPerHa * MathF.PI * 0.0001F * dbhInCm * dbhInCm;
                        float treeChainsawPMsPerHa = expansionFactorPerHa * treeChainsawPMsWithTrackedHarvester;
                        this.TrackedHarvester.AddTree(treeTrackedHarvesterPMsPerHa, treeChainsawPMsPerHa, treeBasalAreaInM2PerHa, treeMerchantableVolumePerHa);

                        // forwarded weight is added in wheeled harvester case
                    }

                    // wheeled harvester + forwarder
                    (float treeHarvesterPMsWithWheeledHarvester, float treeChainsawPMsWithWheeledHarvester) = harvestSystems.GetWheeledHarvesterTime(dbhInCm, treeMerchantableVolumeInM3, previousOversizeTreeBehindHarvester);
                    float treeWheeledHarvesterPMsPerHa = expansionFactorPerHa * treeHarvesterPMsWithWheeledHarvester;
                    if (treeChainsawPMsWithWheeledHarvester == 0.0F)
                    {
                        // tree felled and bucked by harvester, no chainsaw use
                        this.WheeledHarvester.AddTree(treeWheeledHarvesterPMsPerHa);
                        
                        float woodAndRemainingBarkVolumePerStem = (treeMerchantableVolumeInM3 + unscaledNeiloidVolumeInM3) / (1.0F - treeSpeciesProperties.BarkFractionAfterHarvester);
                        float treeForwardedWeightInKgPerHa = expansionFactorPerHa * woodAndRemainingBarkVolumePerStem * treeSpeciesProperties.StemDensityAfterHarvester; // / merchantableFractionOfLogLength; // 1 in BC Firmwood
                        this.Forwarder.AddTree(treeForwardedWeightInKgPerHa);
                    }
                    else
                    {
                        float treeBasalAreaInM2PerHa = expansionFactorPerHa * MathF.PI * 0.0001F * dbhInCm * dbhInCm;
                        float treeChainsawPMsPerHa = expansionFactorPerHa * treeChainsawPMsWithWheeledHarvester;
                        this.WheeledHarvester.AddTree(treeWheeledHarvesterPMsPerHa, treeChainsawPMsPerHa, treeBasalAreaInM2PerHa, treeMerchantableVolumePerHa);

                        // for now, assume no bark loss from going through feed rollers
                        float treeForwardedWeightInKgPerHa = expansionFactorPerHa * treeSpeciesProperties.GetStemOrLogWeightWithAllBark(treeMerchantableVolumeInM3 + unscaledNeiloidVolumeInM3);
                        this.Forwarder.AddTree(treeForwardedWeightInKgPerHa);

                        previousOversizeTreeBehindHarvester = !previousOversizeTreeBehindHarvester;
                    }
                }
            }
        }

        public override void CalculateProductivityAndCost(StandTrajectory trajectory, int harvestPeriod, bool isThin, HarvestSystems harvestSystems, float harvestCostPerHectare, float harvestTaskCostPerCubicMeter)
        {
            Stand stand = HarvestFinancialValue.GetAndValidateStand(trajectory, harvestPeriod, isThin);

            // clear PMh, SMh, productivities, and harvest system selection, reset chainsaw volume and basal area accumulators
            // Merchantable harvest volume and pond values are accumulated separately before this function is called, so don't clear those.
            this.Forwarder.Clear();
            this.WheeledHarvester.Clear();

            if (this.MerchantableCubicVolumePerHa == 0.0F)
            {
                this.HarvestRelatedTaskCostPerHa = 0.0F;
                this.NetPresentValuePerHa = 0.0F;
                this.SetMinimumCostSystem();
                return;
            }

            // calculate work hours needed to perform
            int thinOrRegenerationHarvestPeriod = isThin ? harvestPeriod : Constant.RegenerationHarvestIfEligible;
            this.CalculatePMh(stand, trajectory, thinOrRegenerationHarvestPeriod, isThin, harvestSystems);

            // modify PMh for slope, convert to hours, set falling machine and chainsaw crew productivity
            // For now, assume uniform slope across stand.
            this.TrackedHarvester.CalculatePMhAndProductivity(stand, harvestSystems, this.MerchantableCubicVolumePerHa);
            this.WheeledHarvester.CalculatePMhAndProductivity(stand, harvestSystems, this.MerchantableCubicVolumePerHa);
            this.Forwarder.CalculatePMhAndProductivity(stand, trajectory, harvestPeriod, harvestSystems);

            this.TrackedHarvester.CalculateSystemCost(stand, harvestSystems, this.Forwarder);
            this.WheeledHarvester.CalculateSystemCost(stand, harvestSystems, this.Forwarder);

            this.HarvestRelatedTaskCostPerHa = harvestCostPerHectare + harvestTaskCostPerCubicMeter * this.MerchantableCubicVolumePerHa;
            this.SetMinimumCostSystem();
        }

        private void SetMinimumCostSystem()
        {
            if (this.MerchantableCubicVolumePerHa == 0.0F)
            {
                this.MinimumCostHarvestSystem = HarvestSystemEquipment.None;
                this.MinimumSystemCostPerHa = 0.0F;
                return;
            }

            this.MinimumCostHarvestSystem = HarvestSystemEquipment.TrackedHarvesterForwarder;
            this.MinimumSystemCostPerHa = this.TrackedHarvester.SystemCostPerHaWithForwarder;

            if (this.WheeledHarvester.SystemCostPerHaWithForwarder < this.MinimumSystemCostPerHa)
            {
                this.MinimumCostHarvestSystem = HarvestSystemEquipment.WheeledHarvesterForwarder;
                this.MinimumSystemCostPerHa = this.WheeledHarvester.SystemCostPerHaWithForwarder;
            }
        }

        public override bool TryAddMerchantableVolume(StandTrajectory trajectory, int harvestPeriod, FinancialScenarios financialScenarios, int financialIndex, float shortLogPondMultiplier)
        {
            this.ClearNpvAndPond();

            bool merchantableVolumeAdded = false;
            for (int treeSpeciesIndex = 0; treeSpeciesIndex < trajectory.ForwardedThinVolumeBySpecies.Count; ++treeSpeciesIndex)
            {
                TreeSpeciesMerchantableVolume harvestVolumeForSpecies = trajectory.ForwardedThinVolumeBySpecies.Values[treeSpeciesIndex];
                merchantableVolumeAdded |= this.TryAddMerchantableVolume(harvestVolumeForSpecies, harvestPeriod, financialScenarios, financialIndex, shortLogPondMultiplier);
            }

            return merchantableVolumeAdded;
        }
    }
}
