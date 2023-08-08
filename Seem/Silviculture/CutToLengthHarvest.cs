using Mars.Seem.Tree;
using System;
using System.Diagnostics;

namespace Mars.Seem.Silviculture
{
    public class CutToLengthHarvest : HarvestFinancialValue
    {
        public Forwarder Forwarder { get; private init; }

        public float MinimumSystemCostPerHa { get; set; } // US$/ha

        public HarvesterSystem WheeledHarvester { get; private init; }

        public CutToLengthHarvest()
        {
            this.Forwarder = new();
            this.WheeledHarvester = new();

            this.Clear();
        }

        /// <param name="stand">Stand at beginning of harvest period if thinning, stand at end of harvest period if regeneration harvest.</param>
        private void CalculateVolumeAndPMh(Stand stand, StandTrajectory trajectory, int harvestPeriod, HarvestSystems harvestSystems)
        {
            // BC Firmwood scaling considers trim to be merchantable cubic volume (Fonseca 2005 §2.2.2.2) so, for now, merchantableFractionOfLogLength = 1
            // float preferredLogLength = scaledVolume.PreferredLogLengthInMeters;
            // float preferredTrimLength = scaledVolume.GetPreferredTrim();
            // float merchantableFractionOfLogLength = preferredLogLength / (preferredLogLength + preferredTrimLength); // assumes cylindrical logs or cylindrical ones with equal amounts of trim at both ends
            bool previousOversizeTreeBehindHarvester = true;
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                float diameterToCentimetersMultiplier = 1.0F;
                float heightToMetersMultiplier = 1.0F;
                float hectareExpansionFactorMultiplier = 1.0F;
                if (treesOfSpecies.Units == Units.English)
                {
                    diameterToCentimetersMultiplier = Constant.CentimetersPerInch;
                    heightToMetersMultiplier = Constant.MetersPerFoot;
                    hectareExpansionFactorMultiplier = Constant.AcresPerHectare;
                }
                IndividualTreeSelection individualTreeSelection = trajectory.TreeSelectionBySpecies[treesOfSpecies.Species];
                TreeSpeciesProperties treeSpeciesProperties = TreeSpecies.Properties[treesOfSpecies.Species];
                if (trajectory.TreeVolume.TryGetForwarderVolumeTable(treesOfSpecies.Species, out TreeSpeciesMerchantableVolumeTable? forwardedVolumeTable) == false)
                {
                    // for now, assume all non-merchantable trees are left in place in stand
                    // TODO: include felling and handling costs for cut and leave
                    continue;
                }

                for (int compactedTreeIndex = 0; compactedTreeIndex < treesOfSpecies.Count; ++compactedTreeIndex)
                {
                    int uncompactedTreeIndex = treesOfSpecies.UncompactedIndex[compactedTreeIndex];
                    if (individualTreeSelection[uncompactedTreeIndex] != harvestPeriod)
                    {
                        // tree was either removed previously or was retained rather than thinned
                        continue;
                    }

                    // Could factor constant and linear terms out of loop (true for all machines, both CTL and long log) but it doesn't
                    // appear important to do so.
                    float dbhInCm = diameterToCentimetersMultiplier * treesOfSpecies.Dbh[compactedTreeIndex];
                    if (dbhInCm > forwardedVolumeTable.MaximumMerchantableDiameterInCentimeters)
                    {
                        throw new NotSupportedException("Large reserve " + treesOfSpecies.Species + " " + treesOfSpecies.Tag[compactedTreeIndex] + " is selected for harvest.");
                    }

                    float heightInM = heightToMetersMultiplier * treesOfSpecies.Height[compactedTreeIndex];
                    float expansionFactorPerHa = hectareExpansionFactorMultiplier * treesOfSpecies.LiveExpansionFactor[compactedTreeIndex];
                    float treeMerchantableVolumeInM3 = forwardedVolumeTable.GetCubicVolumeOfMerchantableWood(dbhInCm, heightInM, out float unscaledNeiloidVolumeInM3);
                    Debug.Assert(Single.IsNaN(treeMerchantableVolumeInM3) == false);
                    float treeMerchantableVolumePerHa = expansionFactorPerHa * treeMerchantableVolumeInM3;
                    this.MerchantableCubicVolumePerHa += treeMerchantableVolumePerHa;

                    // TODO: tracked harvester

                    // wheeled harvster
                    (float treeHarvesterPMs, float treeChainsawPMs) = harvestSystems.GetWheeledHarvesterTime(dbhInCm, treeMerchantableVolumeInM3, previousOversizeTreeBehindHarvester);
                    float treeHarvesterPMsPerHa = expansionFactorPerHa * treeHarvesterPMs;
                    if (treeChainsawPMs == 0.0F)
                    {
                        // tree felled and bucked by harvester, no chainsaw use
                        this.WheeledHarvester.AddTree(treeHarvesterPMsPerHa);
                        
                        float woodAndRemainingBarkVolumePerStem = (treeMerchantableVolumeInM3 + unscaledNeiloidVolumeInM3) / (1.0F - treeSpeciesProperties.BarkFractionAfterHarvester);
                        float treeForwardedWeightInKgPerHa = expansionFactorPerHa * woodAndRemainingBarkVolumePerStem * treeSpeciesProperties.StemDensityAfterHarvester; // / merchantableFractionOfLogLength; // 1 in BC Firmwood
                        this.Forwarder.AddTree(treeForwardedWeightInKgPerHa);
                    }
                    else
                    {
                        // a chainsaw is used on harvestSystems tree, so it applies towards chainsaw utilization
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
                        float treeChainsawPMsPerHa = expansionFactorPerHa * treeChainsawPMs;
                        this.WheeledHarvester.AddTree(treeHarvesterPMsPerHa, treeChainsawPMsPerHa, treeBasalAreaInM2PerHa, treeMerchantableVolumePerHa);

                        // no bark loss from going through feed rollers
                        float woodAndBarkVolumePerStem = (treeMerchantableVolumeInM3 + unscaledNeiloidVolumeInM3) / (1.0F - treeSpeciesProperties.BarkFraction);
                        float treeForwardedWeightInKgPerHa = expansionFactorPerHa * woodAndBarkVolumePerStem * treeSpeciesProperties.StemDensity; // / merchantableFractionOfLogLength; // 1 in BC Firmwood
                        this.Forwarder.AddTree(treeForwardedWeightInKgPerHa); // / merchantableFractionOfLogLength; // 1 in BC Firmwood 

                        previousOversizeTreeBehindHarvester = !previousOversizeTreeBehindHarvester;
                    }
                }
            }
        }

        public void CalculateVolumeProductivityAndCost(StandTrajectory trajectory, int harvestPeriod, bool isThin, HarvestSystems harvestSystems)
        {
            Stand? stand = trajectory.StandByPeriod[harvestPeriod - (isThin ? 1 : 0)];
            if ((stand == null) ||
                (stand.AccessDistanceInM < 0.0F) ||
                (stand.AccessSlopeInPercent < 0.0F) ||
                (stand.AreaInHa <= 0.0F) ||
                (stand.CorridorLengthInM <= 0.0F) ||
                (stand.CorridorLengthInMTethered < 0.0F) ||
                (stand.CorridorLengthInMTethered > Constant.Maximum.TetheredCorridorLengthInM) ||
                (stand.CorridorLengthInMUntethered < 0.0F) ||
                (stand.ForwardingDistanceOnRoad < 0.0F) ||
                (stand.MeanYardingDistanceFactor <= 0.0F) ||
                (stand.SlopeInPercent < 0.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(trajectory), "Stand at period " + harvestPeriod + " is null or has negative distance, slope, or area.");
            }

            // clear PMh, SMh, productivities, and harvest system selection, reset volume and basal accumulators
            this.Clear();

            // calculate harvest volume in thin
            this.CalculateVolumeAndPMh(stand, trajectory, harvestPeriod, harvestSystems);

            // modify PMh for slope, convert to hours, set falling machine and chainsaw crew productivity
            // For now, assume uniform slope across stand.
            this.WheeledHarvester.CalculatePMhAndProductivity(stand, harvestSystems, this.MerchantableCubicVolumePerHa);
            this.Forwarder.CalculatePMhAndProductivity(stand, trajectory, harvestPeriod, harvestSystems);

            if (this.CubicVolumePerHa > 0.0F)
            {
                int machinesToMoveInAndOut = 3 + (this.WheeledHarvester.AnchorMachine ? 1 : 0) + (this.Forwarder.AnchorMachine ? 1 : 0); // bulldozer + harvester + forwarder + anchors
                float machineMoveInAndOutPerHa = 2.0F * machinesToMoveInAndOut * harvestSystems.MachineMoveInOrOut / stand.AreaInHa; // 2 = move in + move out
                float haulRoundtripsPerHectare = this.Forwarder.ForwardedWeightPerHa / harvestSystems.CutToLengthHaulPayloadInKg;
                float haulCostPerHectare = harvestSystems.CutToLengthRoundtripHaulSMh * haulRoundtripsPerHectare * harvestSystems.CutToLengthHaulPerSMh;
                this.MinimumSystemCostPerHa = this.WheeledHarvester.HarvesterCostPerHa + this.WheeledHarvester.ChainsawMinimumCost + this.Forwarder.ForwarderCostPerHa + haulCostPerHectare + machineMoveInAndOutPerHa;
                Debug.Assert((haulCostPerHectare > 0.0F) && (machineMoveInAndOutPerHa > 0.0F) &&
                             (Single.IsNaN(this.MinimumSystemCostPerHa) == false) && (this.MinimumSystemCostPerHa > 0.0F));
            }
        }

        private new void Clear()
        {
            this.Forwarder.Clear();

            this.MerchantableCubicVolumePerHa = 0.0F;
            this.MinimumSystemCostPerHa = Single.NaN;

            this.WheeledHarvester.Clear();
        }
    }
}
