using Mars.Seem.Extensions;
using Mars.Seem.Organon;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mars.Seem.Silviculture
{
    public class ThinByPrescription : Harvest
    {
        private float fromAbovePercentage;
        private float fromBelowPercentage;
        private float proportionalPercentage;

        public ThinByPrescription(int harvestAtBeginningOfPeriod)
        {
            if (harvestAtBeginningOfPeriod < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(harvestAtBeginningOfPeriod));
            }

            this.fromAbovePercentage = 0.0F;
            this.fromBelowPercentage = 0.0F;
            this.proportionalPercentage = 0.0F;
            this.Period = harvestAtBeginningOfPeriod;
        }

        public float FromAbovePercentage
        {
            get 
            { 
                return this.fromAbovePercentage; 
            }
            set 
            {
                if ((value < 0.0F) || (value > 100.0F))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                Debug.Assert(Single.IsNaN(value) == false);
                this.fromAbovePercentage = value; 
            }
        }

        public float FromBelowPercentage
        {
            get
            {
                return this.fromBelowPercentage;
            }
            set
            {
                if ((value < 0.0F) || (value > 100.0F))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                Debug.Assert(Single.IsNaN(value) == false);
                this.fromBelowPercentage = value;
            }
        }

        public float ProportionalPercentage
        {
            get
            {
                return this.proportionalPercentage;
            }
            set
            {
                if ((value < 0.0F) || (value > 100.0F))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                Debug.Assert(Single.IsNaN(value) == false);
                this.proportionalPercentage = value;
            }
        }

        public override Harvest Clone()
        {
            return new ThinByPrescription(this.Period)
            {
                FromAbovePercentage = this.FromAbovePercentage, 
                ProportionalPercentage = this.ProportionalPercentage, 
                fromBelowPercentage = this.FromBelowPercentage
            };
        }

        public override float EvaluateTreeSelection(OrganonStandTrajectory trajectory)
        {
            float totalPercentage = this.fromAbovePercentage + this.fromBelowPercentage + this.proportionalPercentage;
            if ((totalPercentage < 0.0F) || (totalPercentage > 100.0F))
            {
                throw new NotSupportedException("Sum of from above, from below, and proportional removal percentages is " + totalPercentage + ". This is beyond the valid range of 0-100%.");
            }

            OrganonStand standAtEndOfPreviousPeriod = trajectory.StandByPeriod[this.Period - 1] ?? throw new NotSupportedException("Stand information is not available for period " + (this.Period - 1) + ".");
            Units standUnits = standAtEndOfPreviousPeriod.GetUnits();
            (float diameterToCmMultiplier, float _, float _) = UnitsExtensions.GetConversionToMetric(standUnits);
            float diameterToStandUnitsMultiplier = 1.0F / diameterToCmMultiplier;

            // sort trees by diameter
            SortedList<FiaCode, int[]> dbhSortOrderBySpecies = new();
            SortedList<FiaCode, int> thinFromAboveIndexBySpecies = new();
            SortedList<FiaCode, int> thinFromBelowIndexBySpecies = new();
            float maximumDbh = Single.MinValue;
            float minimumDbh = Single.MaxValue;
            FiaCode currentMaximumDbhSpeciesSelection = default;
            FiaCode currentMinimumDbhSpeciesSelection = default;
            foreach (Trees treesOfSpecies in standAtEndOfPreviousPeriod.TreesBySpecies.Values)
            {
                if ((treesOfSpecies.Count == 0) || 
                    (trajectory.TreeScaling.TryGetForwarderVolumeTable(treesOfSpecies.Species, out TreeSpeciesMerchantableVolumeTable? forwardedVolumeTable) == false))
                {
                    // no trees to thin
                    // TODO: support thinning of nonmerchantable species
                    continue;
                }

                if (trajectory.TreeScaling.TryGetLongLogVolumeTable(treesOfSpecies.Species, out TreeSpeciesMerchantableVolumeTable? longLogVolumeTable) == false)
                {
                    throw new NotSupportedException(treesOfSpecies.Species + " has a forwarded volume table but not a long log volume table.");
                }
                if (forwardedVolumeTable.MaximumMerchantableDiameterInCentimeters != longLogVolumeTable.MaximumMerchantableDiameterInCentimeters)
                {
                    // if needed, this can be moved after the max DBH is obtained and thrown only when trees larger than a volume table limit are present
                    throw new NotSupportedException("Forwarded volume table's maximum DBH of " + forwardedVolumeTable.MaximumMerchantableDiameterInCentimeters + " cm differs from the long log volume table's " + longLogVolumeTable.MaximumMerchantableDiameterInCentimeters + " cm.  Since it is not known whether the thin will be performed as a long or short log harvest the largest harvest eligible tree size cannot be determined.");
                }

                // for now, assume large tree retention
                float maximumFellableDbh = diameterToStandUnitsMultiplier * forwardedVolumeTable.MaximumMerchantableDiameterInCentimeters;

                int[] dbhSortOrder = treesOfSpecies.GetDbhSortOrder();
                float maximumDbhInSpecies = Single.NaN;
                int thinFromAboveIndexOfLargestFellableTreeInSpecies = dbhSortOrder.Length - 1;
                for (int thinFromAboveIndex = thinFromAboveIndexOfLargestFellableTreeInSpecies; thinFromAboveIndex > 0; --thinFromAboveIndex)
                {
                    int compactedTreeIndex = dbhSortOrder[thinFromAboveIndex];
                    float dbh = treesOfSpecies.Dbh[compactedTreeIndex];
                    if (dbh <= maximumFellableDbh)
                    {
                        maximumDbhInSpecies = dbh;
                        thinFromAboveIndexOfLargestFellableTreeInSpecies = thinFromAboveIndex;
                        break;
                    }
                }
                if (Single.IsNaN(maximumDbhInSpecies))
                {
                    continue; // all trees in species are larger than the maximum fellable DBH and thus, for now, assumed to be retained
                }

                dbhSortOrderBySpecies.Add(treesOfSpecies.Species, dbhSortOrder);
                thinFromAboveIndexBySpecies.Add(treesOfSpecies.Species, thinFromAboveIndexOfLargestFellableTreeInSpecies + 1); // + 1 because thin from above loop needs to - 1
                thinFromBelowIndexBySpecies.Add(treesOfSpecies.Species, 0); // TODO: support retention of advance regeneration

                if (maximumDbhInSpecies > maximumDbh)
                {
                    maximumDbh = maximumDbhInSpecies;
                    currentMaximumDbhSpeciesSelection = treesOfSpecies.Species;
                }

                float minimumDbhInSpecies = treesOfSpecies.Dbh[dbhSortOrder[0]];
                if (minimumDbhInSpecies < minimumDbh)
                {
                    minimumDbh = minimumDbhInSpecies;
                    currentMinimumDbhSpeciesSelection = treesOfSpecies.Species;
                }
            }

            // thin from above
            OrganonStandDensity? densityAtEndOfPreviousPeriod = trajectory.DensityByPeriod[this.Period - 1];
            Debug.Assert((densityAtEndOfPreviousPeriod != null) && (standUnits == Units.English));
            float targetBasalAreaEnglish = 0.01F * this.FromAbovePercentage * Constant.HectaresPerAcre * Constant.SquareFeetPerSquareMeter * densityAtEndOfPreviousPeriod.BasalAreaPerHa;
            float basalAreaRemovedFromAbove = 0.0F;
            while (basalAreaRemovedFromAbove < targetBasalAreaEnglish)
            {
                Trees treesWithLargestDbh = standAtEndOfPreviousPeriod.TreesBySpecies[currentMaximumDbhSpeciesSelection];
                int thinFromAboveIndex = thinFromAboveIndexBySpecies[currentMaximumDbhSpeciesSelection] - 1;
                if (thinFromAboveIndex < 0)
                {
                    continue; // no more fellable trees in this species
                }

                int compactedTreeIndex = dbhSortOrderBySpecies[currentMaximumDbhSpeciesSelection][thinFromAboveIndex];
                int uncompactedTreeIndex = treesWithLargestDbh.UncompactedIndex[compactedTreeIndex];

                int currentHarvestPeriod = trajectory.TreeSelectionBySpecies[treesWithLargestDbh.Species][uncompactedTreeIndex];
                if (currentHarvestPeriod == Constant.NoHarvestPeriod)
                {
                    // skip trees which have been marked as not harvestable (reserves, cull, nonmerchantable)
                    continue;
                }

                // selection of previously harvested trees is a defect but trees 1) not selected for thinning, selected for thinning in
                // 2) this period or 3) later periods are eligible for removal in this period
                if ((currentHarvestPeriod != Constant.NoHarvestPeriod) && (currentHarvestPeriod != Constant.RegenerationHarvestIfEligible) && (currentHarvestPeriod < this.Period))
                {
                    throw new NotSupportedException("Could not select tree " + treesWithLargestDbh.Tag[compactedTreeIndex] + " for proportional thinning in period " + this.Period + " because it is assigned to period " + currentHarvestPeriod + ".");
                }
                if (treesWithLargestDbh.LiveExpansionFactor[compactedTreeIndex] <= 0.0F)
                {
                    throw new NotSupportedException("Could not select tree " + treesWithLargestDbh.Tag[compactedTreeIndex] + " for proportional thinning in period " + this.Period + " because its expansion factor is " + treesWithLargestDbh.LiveExpansionFactor[compactedTreeIndex].ToString(Constant.Default.ExpansionFactorFormat) + ".");
                }
                Debug.Assert(diameterToCmMultiplier * treesWithLargestDbh.Dbh[compactedTreeIndex] <= 100.1F); // for now, quick implementation, TODO: follow volume table maximum DBH by species
                trajectory.SetTreeSelection(currentMaximumDbhSpeciesSelection, uncompactedTreeIndex, this.Period);

                float basalAreaOfTree = treesWithLargestDbh.GetBasalArea(compactedTreeIndex);
                basalAreaRemovedFromAbove += basalAreaOfTree; // for now, use complete removal of tree's expansion factor
                thinFromAboveIndexBySpecies[currentMaximumDbhSpeciesSelection] = thinFromAboveIndex;

                // find next largest tree (by diameter) which hasn't been thinned
                maximumDbh = Single.MinValue;
                bool foundNextTree = false;
                foreach (Trees treesOfSpecies in standAtEndOfPreviousPeriod.TreesBySpecies.Values)
                {
                    int[] dbhSortOrder = dbhSortOrderBySpecies[treesOfSpecies.Species];
                    thinFromAboveIndex = thinFromAboveIndexBySpecies[treesOfSpecies.Species];
                    Debug.Assert((thinFromAboveIndex >= 0) && (thinFromAboveIndex < dbhSortOrder.Length));

                    float largestDbh = treesOfSpecies.Dbh[dbhSortOrder[thinFromAboveIndex]];
                    if (largestDbh > maximumDbh)
                    {
                        maximumDbh = largestDbh;
                        currentMaximumDbhSpeciesSelection = treesOfSpecies.Species;
                        foundNextTree = true;
                    }
                }
                if (foundNextTree == false)
                {
                    // avoid looping forever if, for some reason, basal area target cannot be reached
                    break;
                }
            }

            // thin from below
            Debug.Assert(standUnits == Units.English);
            targetBasalAreaEnglish = 0.01F * this.FromBelowPercentage * Constant.HectaresPerAcre * Constant.SquareFeetPerSquareMeter * densityAtEndOfPreviousPeriod.BasalAreaPerHa;
            float basalAreaRemovedFromBelow = 0.0F;
            while (basalAreaRemovedFromBelow < targetBasalAreaEnglish)
            {
                Trees treesWithSmallestDbh = standAtEndOfPreviousPeriod.TreesBySpecies[currentMinimumDbhSpeciesSelection];
                int thinFromBelowIndex = thinFromBelowIndexBySpecies[currentMinimumDbhSpeciesSelection];
                int compactedTreeIndex = dbhSortOrderBySpecies[currentMinimumDbhSpeciesSelection][thinFromBelowIndex];
                int uncompactedTreeIndex = treesWithSmallestDbh.UncompactedIndex[compactedTreeIndex];
                float basalAreaOfTree = treesWithSmallestDbh.GetBasalArea(compactedTreeIndex);

                int currentHarvestPeriod = trajectory.TreeSelectionBySpecies[treesWithSmallestDbh.Species][uncompactedTreeIndex];
                if ((currentHarvestPeriod != Constant.NoHarvestPeriod) && (currentHarvestPeriod != Constant.RegenerationHarvestIfEligible) && (currentHarvestPeriod < this.Period))
                {
                    throw new NotSupportedException("Could not select tree " + treesWithSmallestDbh.Tag[compactedTreeIndex] + " for proportional thinning in period " + this.Period + " because it is assigned to period " + currentHarvestPeriod + ".");
                }
                if (treesWithSmallestDbh.LiveExpansionFactor[compactedTreeIndex] <= 0.0F)
                {
                    throw new NotSupportedException("Could not select tree " + treesWithSmallestDbh.Tag[compactedTreeIndex] + " for proportional thinning in period " + this.Period + " because its expansion factor is " + treesWithSmallestDbh.LiveExpansionFactor[compactedTreeIndex].ToString(Constant.Default.ExpansionFactorFormat) + ".");
                }
                Debug.Assert(diameterToCmMultiplier * treesWithSmallestDbh.Dbh[compactedTreeIndex] <= 100.1F); // for now, quick implementation, TODO: follow volume table maximum DBH by species
                trajectory.SetTreeSelection(currentMinimumDbhSpeciesSelection, uncompactedTreeIndex, this.Period);

                basalAreaRemovedFromBelow += basalAreaOfTree;
                thinFromBelowIndexBySpecies[currentMinimumDbhSpeciesSelection] = thinFromBelowIndex + 1;

                // find next smallest tree (by diameter) which hasn't been thinned
                minimumDbh = Single.MaxValue;
                bool foundNextTree = false;
                foreach (Trees treesOfSpecies in standAtEndOfPreviousPeriod.TreesBySpecies.Values)
                {
                    int thinFromAboveIndex = thinFromAboveIndexBySpecies[treesOfSpecies.Species];
                    thinFromBelowIndex = thinFromBelowIndexBySpecies[treesOfSpecies.Species];
                    if (thinFromBelowIndex >= thinFromAboveIndex)
                    {
                        // no more trees to remove in this species
                        continue;
                    }

                    int[] dbhSortOrder = dbhSortOrderBySpecies[treesOfSpecies.Species];
                    float smallestDbh = treesOfSpecies.Dbh[dbhSortOrder[thinFromBelowIndex]];
                    if (smallestDbh < minimumDbh)
                    {
                        minimumDbh = smallestDbh;
                        currentMinimumDbhSpeciesSelection = treesOfSpecies.Species;
                        foundNextTree = true;
                    }
                }
                if (foundNextTree == false)
                {
                    break;
                }
            }

            // thin remaining trees proportionally
            float proportionalThinAccumulator = 0.0F;
            float proportionalIncrement = 0.01F * this.ProportionalPercentage * 100.0F / (100.0F - this.FromAbovePercentage - this.FromBelowPercentage);
            float basalAreaRemovedProportionally = 0.0F;
            for (int speciesIndex = 0; speciesIndex < dbhSortOrderBySpecies.Count; ++speciesIndex)
            {
                FiaCode treeSpecies = dbhSortOrderBySpecies.Keys[speciesIndex];
                Trees treesOfSpecies = standAtEndOfPreviousPeriod.TreesBySpecies[treeSpecies];
                if (treesOfSpecies.Count == 0)
                {
                    continue;
                }

                int[] dbhSortOrder = dbhSortOrderBySpecies.Values[speciesIndex];
                for (int proportionalThinIndex = thinFromBelowIndexBySpecies[treeSpecies]; proportionalThinIndex < thinFromAboveIndexBySpecies[treeSpecies]; ++proportionalThinIndex)
                {
                    int compactedTreeIndex = dbhSortOrder[proportionalThinIndex];
                    int uncompactedTreeIndex = treesOfSpecies.UncompactedIndex[compactedTreeIndex];
                    int currentHarvestPeriod = trajectory.TreeSelectionBySpecies[treesOfSpecies.Species][uncompactedTreeIndex];
                    if (currentHarvestPeriod == Constant.NoHarvestPeriod)
                    {
                        // skip trees which have been marked as not harvestable (reserves, cull, nonmerchantable)
                        continue;
                    }

                    proportionalThinAccumulator += proportionalIncrement;
                    if (proportionalThinAccumulator >= 1.0F)
                    {
                        float basalAreaOfTree = treesOfSpecies.GetBasalArea(compactedTreeIndex);
                        if ((currentHarvestPeriod != Constant.NoHarvestPeriod) && (currentHarvestPeriod != Constant.RegenerationHarvestIfEligible) && (currentHarvestPeriod < this.Period))
                        {
                            throw new NotSupportedException("Could not select tree " + treesOfSpecies.Tag[compactedTreeIndex] + " for proportional thinning in period " + this.Period + " because it is assigned to period " + currentHarvestPeriod + ".");
                        }
                        if (treesOfSpecies.LiveExpansionFactor[compactedTreeIndex] <= 0.0F)
                        {
                            throw new NotSupportedException("Could not select tree " + treesOfSpecies.Tag[compactedTreeIndex] + " for proportional thinning in period " + this.Period + " because its expansion factor is " + treesOfSpecies.LiveExpansionFactor[compactedTreeIndex].ToString(Constant.Default.ExpansionFactorFormat) + ".");
                        }
                        Debug.Assert(diameterToCmMultiplier * treesOfSpecies.Dbh[compactedTreeIndex] <= 100.1F); // for now, quick implementation, TODO: follow volume table maximum DBH by species
                        trajectory.SetTreeSelection(treeSpecies, uncompactedTreeIndex, this.Period);

                        basalAreaRemovedProportionally += basalAreaOfTree;                        
                        proportionalThinAccumulator -= 1.0F;
                    }
                    else if (currentHarvestPeriod == this.Period)
                    {
                        // for now, assume this is the only harvest prescription active for this period
                        // This makes the prescription authorative for tree assignments in the period and, therefore, able to release trees from
                        // harvest.
                        trajectory.SetTreeSelection(treeSpecies, uncompactedTreeIndex, Constant.RegenerationHarvestIfEligible);
                    }
                    else if ((currentHarvestPeriod != Constant.NoHarvestPeriod) && (currentHarvestPeriod != Constant.RegenerationHarvestIfEligible) && (currentHarvestPeriod < this.Period))
                    {
                        // tree is expected to be retained through this period (but possibly harvested later), so prior removal is an error
                        throw new NotSupportedException("Tree " + treesOfSpecies.Tag[compactedTreeIndex] + " is thinned in period " + currentHarvestPeriod + " but is expected to be retained through period " + this.Period + ".");
                    }
                }
            }

            float basalAreaRemoved = basalAreaRemovedFromAbove + basalAreaRemovedProportionally + basalAreaRemovedFromBelow;
            Debug.Assert((totalPercentage >= 0.0F && basalAreaRemoved > 0.0F) || (((int)(0.01F * totalPercentage * thinFromAboveIndexBySpecies.Values.Sum() - Constant.RoundTowardsZeroTolerance) <= 1) && (basalAreaRemoved == 0.0F)));
            return basalAreaRemoved;
        }

        public override bool TryCopyFrom(Harvest other)
        {
            if (other is ThinByIndividualTreeSelection thinByIndividualTreeSelection)
            {
                // this is, for now, just a stub to enable test cases so only flow period
                // If needed, APIs can be created to calculate approximate above, proportional, and below percentages from the tree selection.
                this.Period = thinByIndividualTreeSelection.Period;
                return true;
            }

            if (other is ThinByPrescription thinByPrescription)
            {
                this.FromAbovePercentage = thinByPrescription.FromAbovePercentage;
                this.FromBelowPercentage = thinByPrescription.FromBelowPercentage;
                this.Period = thinByPrescription.Period;
                this.ProportionalPercentage = thinByPrescription.ProportionalPercentage;
                return true;
            }

            return false;
        }
    }
}
