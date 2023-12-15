using Mars.Seem.Extensions;
using Mars.Seem.Tree;
using System;
using System.Diagnostics;

namespace Mars.Seem.Silviculture
{
    public abstract class LongLogHarvest : HarvestFinancialValue
    {
        public Fallers Fallers { get; private init; }
        public FellerBuncherSystems FellerBuncher { get; private init; }
        
        public float LoaderSMhPerHectare { get; private set; } // SMh/ha

        public HarvesterSystem TrackedHarvester { get; private init; }
        public HarvesterSystem WheeledHarvester { get; private init; }
        public YardingSystem Yarder { get; private init; }
        public YardingSystem Yoader { get; private init; }

        public LongLogHarvest()
        {
            this.Fallers = new();
            this.FellerBuncher = new();
            this.TrackedHarvester = new()
            {
                IsTracked = true
            };
            this.WheeledHarvester = new();
            this.Yarder = new();
            this.Yoader = new()
            {
                IsYoader = true
            };

            this.LoaderSMhPerHectare = Single.NaN;
        }

        /// <param name="harvestPeriod">Index of thinning period or <see cref="Constant.RegenerationHarvestIfEligible"/> for regeneration harvest trees.</param>
        private void CalculatePMh(Stand stand, StandTrajectory trajectory, int harvestPeriod, bool isThin, HarvestSystems harvestSystems)
        {
            // float preferredLogLengthWithTrimInM = scaledVolume.PreferredLogLengthInMeters + scaledVolume.GetPreferredTrim(); // for checking first log weight
            // float merchantableFractionOfLogLength = scaledVolume.PreferredLogLengthInMeters / preferredLogLengthWithTrimInM; // 1 in BC Firmwood
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                if (trajectory.TreeScaling.TryGetLongLogVolumeTable(treesOfSpecies.Species, out TreeSpeciesMerchantableVolumeTable? longLogVolumeTable) == false)
                {
                    // for now, assume all non-merchantable trees are left in place in stand
                    // TODO: include felling and handling costs for cut and leave
                    continue;
                }

                IndividualTreeSelection individualTreeSelection = trajectory.TreeSelectionBySpecies[treesOfSpecies.Species];
                (float diameterToCmMultiplier, float heightToMetersMultiplier, float hectareExpansionFactorMultiplier) = UnitsExtensions.GetConversionToMetric(treesOfSpecies.Units);

                TreeSpeciesProperties treeSpeciesProperties = TreeSpecies.Properties[treesOfSpecies.Species];
                for (int compactedTreeIndex = 0; compactedTreeIndex < treesOfSpecies.Count; ++compactedTreeIndex)
                {
                    int uncompactedTreeIndex = treesOfSpecies.UncompactedIndex[compactedTreeIndex];
                    if (individualTreeSelection[uncompactedTreeIndex] != harvestPeriod)
                    {
                        // tree is not included in this harvest
                        continue;
                    }

                    float dbhInCm = diameterToCmMultiplier * treesOfSpecies.Dbh[compactedTreeIndex];
                    if (dbhInCm > longLogVolumeTable.MaximumMerchantableDiameterInCentimeters)
                    {
                        HarvestFinancialValue.ThrowIfTreeNotAutomaticReserve(isThin, treesOfSpecies, compactedTreeIndex, longLogVolumeTable);
                        continue;
                    }

                    float heightInM = heightToMetersMultiplier * treesOfSpecies.Height[compactedTreeIndex];
                    float expansionFactorPerHa = hectareExpansionFactorMultiplier * treesOfSpecies.LiveExpansionFactor[compactedTreeIndex];
                    float treeBasalAreaInM2PerHa = expansionFactorPerHa * MathF.PI * 0.0001F * dbhInCm * dbhInCm;
                    float treeMerchantableVolumeInM3 = longLogVolumeTable.GetCubicVolumeOfMerchantableWood(dbhInCm, heightInM, out float unscaledNeiloidVolume);
                    Debug.Assert(Single.IsNaN(treeMerchantableVolumeInM3) == false);
                    float treeMerchantableVolumeInM3PerHa = expansionFactorPerHa * treeMerchantableVolumeInM3;

                    // yarded weight
                    // Since no yarder productivity study appears to consider bark loss it's generally unclear where in the turn reported
                    // yarding weights occur. In lieu of more specific data, yarding weight limits are checked when half the yarding bark
                    // loss has occurred and this same weight is used in finding the total yarded weight for calculating the total number
                    // of yarding turns.
                    // TODO: include weight of remaining crown after branch breaking during felling and swinging
                    float woodAndRemainingBarkVolumePerStemAfterFelling = treeMerchantableVolumeInM3 / (1.0F - treeSpeciesProperties.BarkFractionAtMidspanWithoutHarvester);
                    float treeYardedWeightWithoutHarvester = woodAndRemainingBarkVolumePerStemAfterFelling * treeSpeciesProperties.StemDensityAtMidspanWithoutHarvester;
                    float treeYardedWeightInKgPerHaWithoutHarvester = expansionFactorPerHa * treeYardedWeightWithoutHarvester;
                    float woodAndRemainingBarkVolumePerStemAfterYardingAndProcessing = (treeMerchantableVolumeInM3 + unscaledNeiloidVolume) / (1.0F - treeSpeciesProperties.BarkFractionAfterYardingAndProcessing); // / merchantableFractionOfLogLength; // 1 in BC Firmwood
                    float treeLoadedWeightInKgPerHaWithoutHarvester = expansionFactorPerHa * woodAndRemainingBarkVolumePerStemAfterYardingAndProcessing * treeSpeciesProperties.StemDensityAfterYardingAndProcessing;

                    // chainsaw felling of all trees
                    float treeChainsawFellAndBuckPMs = harvestSystems.GetChainsawFellAndBuckTime(treeMerchantableVolumeInM3);
                    float chainsawFellAndBuckPMsPerHa = expansionFactorPerHa * treeChainsawFellAndBuckPMs;
                    this.Fallers.AddTree(chainsawFellAndBuckPMsPerHa, treeBasalAreaInM2PerHa, treeMerchantableVolumeInM3PerHa, treeYardedWeightInKgPerHaWithoutHarvester, treeLoadedWeightInKgPerHaWithoutHarvester);

                    // feller-buncher with hot saw or directional felling head
                    // For now, assume directional felling head where needed and no diameter limit to directional felling.
                    (float treeFellerBuncherPMs, float treeChainsawBuckPMsWithFellerBuncherAndYarder, float treeChainsawBuckPMsWithFellerBuncherAndYoader) = harvestSystems.GetFellerBuncherTime(treeMerchantableVolumeInM3, treeYardedWeightWithoutHarvester);
                    float treeFellerBuncherPMsPerHa = expansionFactorPerHa * treeFellerBuncherPMs;
                    if ((treeChainsawBuckPMsWithFellerBuncherAndYarder == 0.0F) && (treeChainsawBuckPMsWithFellerBuncherAndYoader == 0.0F))
                    {
                        this.FellerBuncher.AddTree(treeFellerBuncherPMsPerHa, treeYardedWeightInKgPerHaWithoutHarvester, treeLoadedWeightInKgPerHaWithoutHarvester);
                    }
                    else
                    {
                        // chainsaw bucking of trees felled by faller crew or a feller-buncher but too heavy to be yarded as whole stems
                        // For now,
                        //   1) Assume the feller-buncher has a directional felling head if trees are too large for a hot saw and that
                        //      no trees smaller than FinancialSceanrios.MaximumMerchantableDiameterInCentimeters are too large for a
                        //      directional felling head.
                        //   2) Neglect effects of short log production when trees are large enough full size logs still exceed the
                        //      yoader weight limit.
                        //   3) The tree's first log is calculated from a Smalian estimate of diameter inside bark. A more complete correct
                        //      would look up the tree's first log's merchantable volume from the volume table and adjust for the bark
                        //      fraction.
                        //   4) Assume all unscaled Neiloid volume (BC Firmwood) is in the first log. This is very likely true for long
                        //      logs but may not hold for short logs.
                        float treeChainsawBuckPMsPerHaWithYarder = expansionFactorPerHa * treeChainsawBuckPMsWithFellerBuncherAndYarder;
                        float treeChainsawBuckPMsPerHaWithYoader = expansionFactorPerHa * treeChainsawBuckPMsWithFellerBuncherAndYoader;
                        float firstLogVolumeInM3 = longLogVolumeTable.GetCubicVolumeOfMerchantableWoodInFirstLog(dbhInCm, heightInM) + unscaledNeiloidVolume;
                        this.FellerBuncher.AddTree(treeFellerBuncherPMsPerHa, treeChainsawBuckPMsPerHaWithYarder, treeChainsawBuckPMsPerHaWithYoader, treeBasalAreaInM2PerHa, treeMerchantableVolumeInM3PerHa, treeYardedWeightInKgPerHaWithoutHarvester, treeLoadedWeightInKgPerHaWithoutHarvester);

                        float firstLogWeightWithFellerBuncherAndMidCorridorBarkLoss = firstLogVolumeInM3 * treeSpeciesProperties.StemDensityAtMidspanWithoutHarvester; // / merchantableFractionOfLogLength; // 1 in BC Firmwood
                        if (firstLogWeightWithFellerBuncherAndMidCorridorBarkLoss > harvestSystems.GrappleSwingYarderMaxPayload)
                        {
                            // for now, logs produced by feller-buncher plus chainsaw aren't distinguished from their somewhat lighter
                            // equivalent with less bark after being bucked by a harvester
                            this.Yarder.AddOverweightFirstLog(expansionFactorPerHa);
                        }
                        if (firstLogWeightWithFellerBuncherAndMidCorridorBarkLoss > harvestSystems.GrappleYoaderMaxPayload)
                        {
                            // for now, logs produced by feller-buncher plus chainsaw aren't distinguished from their somewhat lighter
                            // equivalent with less bark after being bucked by a harvester
                            this.Yoader.AddOverweightFirstLog(expansionFactorPerHa);
                        }
                    }

                    // tracked harvester
                    (float treeTrackedHarvesterPMs, float treeChainsawPMsWithTrackedHarvester) = harvestSystems.GetTrackedHarvesterTime(dbhInCm, treeMerchantableVolumeInM3, previousOversizeTreeBehindHarvester: false);
                    float treeTrackedHarvesterPMsPerHa = expansionFactorPerHa * treeTrackedHarvesterPMs;
                    float treeWoodAndRemainingBarkVolumePerStemWithHarvester = treeMerchantableVolumeInM3 / (1.0F - treeSpeciesProperties.BarkFractionAtMidspanAfterHarvester);
                    float treeYardedWeightInKgPerHaWithHarvester = expansionFactorPerHa * treeWoodAndRemainingBarkVolumePerStemWithHarvester * treeSpeciesProperties.StemDensityAtMidspanAfterHarvester;
                    if (treeChainsawPMsWithTrackedHarvester == 0.0F)
                    {
                        // tree felled and bucked by harvester, no chainsaw use
                        this.TrackedHarvester.AddTree(treeTrackedHarvesterPMsPerHa, treeYardedWeightInKgPerHaWithHarvester);
                    }
                    else
                    {
                        // a chainsaw is used on harvestSystems tree, so it applies towards chainsaw utilization
                        // For now, since long log harvests are assumed to be regeneration harvests, assume the harvester is free to maneuver
                        // across corridors to fall all trees behind its progression across the unit. This implies the harvester is free to assist
                        // chainsaw work by felling as many trees as it can.
                        float treeChainsawPMsPerHa = expansionFactorPerHa * treeChainsawPMsWithTrackedHarvester;
                        float treeYardedWeightInKgPerHa = expansionFactorPerHa * woodAndRemainingBarkVolumePerStemAfterFelling * treeSpeciesProperties.StemDensityAtMidspanWithoutHarvester;
                        this.TrackedHarvester.AddTree(treeTrackedHarvesterPMsPerHa, treeChainsawPMsPerHa, treeBasalAreaInM2PerHa, treeMerchantableVolumeInM3PerHa, treeYardedWeightInKgPerHa);
                    }

                    // wheeled harvester
                    (float treeWheeledHarvesterPMs, float treeChainsawPMsWithWheeledHarvester) = harvestSystems.GetWheeledHarvesterTime(dbhInCm, treeMerchantableVolumeInM3, false);
                    float treeWheeledHarvesterPMsPerHa = expansionFactorPerHa * treeWheeledHarvesterPMs;
                    if (treeChainsawPMsWithWheeledHarvester == 0.0F)
                    {
                        // tree felled and bucked by harvester, no chainsaw use
                        this.WheeledHarvester.AddTree(treeWheeledHarvesterPMsPerHa, treeYardedWeightInKgPerHaWithHarvester);
                    }
                    else
                    {
                        // a chainsaw is used on this tree, so it applies towards chainsaw utilization
                        float treeChainsawPMsPerHa = expansionFactorPerHa * treeChainsawPMsWithWheeledHarvester;
                        float treeYardedWeightInKgPerHa = expansionFactorPerHa * woodAndRemainingBarkVolumePerStemAfterFelling * treeSpeciesProperties.StemDensityAtMidspanWithoutHarvester;
                        this.WheeledHarvester.AddTree(treeWheeledHarvesterPMsPerHa, treeChainsawPMsPerHa, treeBasalAreaInM2PerHa, treeMerchantableVolumeInM3PerHa, treeYardedWeightInKgPerHa);
                    }

                    // processor
                    // Include constant and linear time components regardless of whether processor needs to buck the tree as, if the tree
                    // was bucked by chainsaw before yarding, the processor still needs to move the logs from the yarder to the loader.
                    float treeProcessingPMsWithYarder = harvestSystems.GetProcessorTime(treeMerchantableVolumeInM3, includeQuadratic: treeChainsawBuckPMsWithFellerBuncherAndYarder == 0.0F);
                    float treeProcessingPMsPerHaWithYarder = expansionFactorPerHa * treeProcessingPMsWithYarder;
                    this.Yarder.AddTreeProcessing(treeProcessingPMsPerHaWithYarder);

                    float treeProcessingPMsWithYoader = harvestSystems.GetProcessorTime(treeMerchantableVolumeInM3, includeQuadratic: treeChainsawBuckPMsWithFellerBuncherAndYoader == 0.0F);
                    float treeProcessingPMsPerHaWithYoader = expansionFactorPerHa * treeProcessingPMsWithYoader;
                    this.Yoader.AddTreeProcessing(treeProcessingPMsPerHaWithYoader);

                    // loader productivity is specified so no loader calculations
                }
            }
        }

        public override void CalculateProductivityAndCost(StandTrajectory trajectory, int harvestPeriod, bool isThin, HarvestSystems harvestSystems, float harvestCostPerHectare, float harvestTaskCostPerCubicMeter)
        {
            Stand stand = HarvestFinancialValue.GetAndValidateStand(trajectory, harvestPeriod, isThin);

            // clear PMh, SMh, productivities, and harvest system selection, reset chainsaw volume and basal area accumulators
            // Merchantable harvest volume and pond values are accumulated separately before this function is called, so don't clear those.
            this.Fallers.Clear();
            this.FellerBuncher.Clear();
            this.TrackedHarvester.Clear();
            this.WheeledHarvester.Clear();
            this.Yarder.Clear();
            this.Yoader.Clear();

            if (this.MerchantableCubicVolumePerHa == 0.0F)
            {
                this.HarvestRelatedTaskCostPerHa = 0.0F;
                this.LoaderSMhPerHectare = Single.NaN;
                this.NetPresentValuePerHa = 0.0F;
                this.SetMinimumCostSystem();
                return;
            }

            // calculate work hours needed to perform harvest
            int thinOrRegenerationHarvestPeriod = isThin ? harvestPeriod : Constant.RegenerationHarvestIfEligible;
            this.CalculatePMh(stand, trajectory, thinOrRegenerationHarvestPeriod, isThin, harvestSystems);

            // modify PMh for slope, convert to hours, set falling machine and chainsaw crew productivity
            // For now, assume uniform slope across stand.
            this.Fallers.CalculatePMhAndProductivity(stand, harvestSystems, this.MerchantableCubicVolumePerHa);
            this.FellerBuncher.CalculatePMhAndProductivity(stand, harvestSystems, this.MerchantableCubicVolumePerHa);
            this.TrackedHarvester.CalculatePMhAndProductivity(stand, harvestSystems, this.MerchantableCubicVolumePerHa);
            this.WheeledHarvester.CalculatePMhAndProductivity(stand, harvestSystems, this.MerchantableCubicVolumePerHa);

            // yarding productivity: not affected by slope since operation from road or low angle roadside is assumed
            // Model design question: does processing head bark removal result in more yarded weight per turn than whole tree yarding?
            // For now, assume loss of efficiency in grappling bunched logs rather than bunched trees cancels weight advantage
            // of using a harvester and calculate yarding as invariant of the felling system. It's also unclear if yarding trees
            // versus logs might result in different hang up rates.
            //Debug.Assert((this.FellerBuncher.YardedWeightPerHa >= this.TrackedHarvester.YardedWeightPerHa) && (this.FellerBuncher.YardedWeightPerHa >= 0.999 * this.WheeledHarvester.YardedWeightPerHa));
            this.Yarder.CalculatePMhAndProductivity(stand, isThin, this.MerchantableCubicVolumePerHa, harvestSystems, this.FellerBuncher.YardedWeightPerHa);
            this.Yoader.CalculatePMhAndProductivity(stand, isThin, this.MerchantableCubicVolumePerHa, harvestSystems, this.FellerBuncher.YardedWeightPerHa);

            // loader productivity is specified: doesn't need to be calculated or output to file
            this.LoaderSMhPerHectare = this.FellerBuncher.LoadedWeightPerHa / (harvestSystems.LoaderUtilization * harvestSystems.LoaderProductivity);

            // component costs are known: calculate system costs
            this.Fallers.CalculateSystemCost(stand, harvestSystems, this.Yarder, this.Yoader, this.LoaderSMhPerHectare);
            this.FellerBuncher.CalculateSystemCost(stand, harvestSystems, this.Yarder, this.Yoader, this.LoaderSMhPerHectare);
            this.TrackedHarvester.CalculateSystemCost(stand, harvestSystems, this.Yarder, this.Yoader, this.LoaderSMhPerHectare);
            this.WheeledHarvester.CalculateSystemCost(stand, harvestSystems, this.Yarder, this.Yoader, this.LoaderSMhPerHectare);

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

            this.MinimumCostHarvestSystem = HarvestSystemEquipment.FallersGrappleSwingYarderProcessorLoader;
            this.MinimumSystemCostPerHa = this.Fallers.SystemCostPerHaWithYarder;

            if (this.Fallers.SystemCostPerHaWithYoader < this.MinimumSystemCostPerHa)
            {
                this.MinimumCostHarvestSystem = HarvestSystemEquipment.FallersGrappleYoaderProcessorLoader;
                this.MinimumSystemCostPerHa = this.Fallers.SystemCostPerHaWithYoader;
            }
            if (this.FellerBuncher.Yarder.SystemCostPerHa < this.MinimumSystemCostPerHa)
            {
                this.MinimumCostHarvestSystem = HarvestSystemEquipment.FellerBuncherGrappleSwingYarderProcessorLoader;
                this.MinimumSystemCostPerHa = this.FellerBuncher.Yarder.SystemCostPerHa;
            }
            if (this.FellerBuncher.Yoader.SystemCostPerHa < this.MinimumSystemCostPerHa)
            {
                this.MinimumCostHarvestSystem = HarvestSystemEquipment.FellerBuncherGrappleYoaderProcessorLoader;
                this.MinimumSystemCostPerHa = this.FellerBuncher.Yoader.SystemCostPerHa;
            }
            if (this.TrackedHarvester.SystemCostPerHaWithYarder < this.MinimumSystemCostPerHa)
            {
                this.MinimumCostHarvestSystem = HarvestSystemEquipment.TrackedHarvesterGrappleSwingYarderLoader;
                this.MinimumSystemCostPerHa = this.TrackedHarvester.SystemCostPerHaWithYarder;
            }
            if (this.TrackedHarvester.SystemCostPerHaWithYoader < this.MinimumSystemCostPerHa)
            {
                this.MinimumCostHarvestSystem = HarvestSystemEquipment.TrackedHarvesterGrappleYoaderLoader;
                this.MinimumSystemCostPerHa = this.TrackedHarvester.SystemCostPerHaWithYoader;
            }
            if (this.WheeledHarvester.SystemCostPerHaWithYarder < this.MinimumSystemCostPerHa)
            {
                this.MinimumCostHarvestSystem = HarvestSystemEquipment.WheeledHarvesterGrappleSwingYarderLoader;
                this.MinimumSystemCostPerHa = this.WheeledHarvester.SystemCostPerHaWithYarder;
            }
            if (this.WheeledHarvester.SystemCostPerHaWithYoader < this.MinimumSystemCostPerHa)
            {
                this.MinimumCostHarvestSystem = HarvestSystemEquipment.WheeledHarvesterGrappleYoaderLoader;
                this.MinimumSystemCostPerHa = this.WheeledHarvester.SystemCostPerHaWithYoader;
            }
        }
    }
}
