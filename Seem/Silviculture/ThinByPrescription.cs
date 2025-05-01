using Mars.Seem.Extensions;
using Mars.Seem.Organon;
using Mars.Seem.Tree;
using System;
using System.Diagnostics;

namespace Mars.Seem.Silviculture
{
    public class ThinByPrescription : Harvest
    {
        private float fromAbovePercentage;
        private float fromBelowPercentage;
        private float proportionalPercentage;

        public ThinByPrescription(int harvestAtBeginningOfPeriod)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(harvestAtBeginningOfPeriod, 1);

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

        /// <summary>
        /// Mark trees for thinning from below, proportionally, and above based on specified thinning intensity percentages.
        /// </summary>
        /// <remarks>
        /// Percentages are applied to merchantable species on the basis of the stand's total basal area. Depending on intensity, stand
        /// composition, and number of reserve trees, percentages may not be achievable.
        /// </remarks>
        public override float EvaluateTreeSelection(OrganonStandTrajectory trajectory)
        {
            float totalPercentage = this.fromAbovePercentage + this.fromBelowPercentage + this.proportionalPercentage;
            if ((totalPercentage < 0.0F) || (totalPercentage > 100.0F))
            {
                throw new NotSupportedException("Sum of from above, from below, and proportional removal percentages is " + totalPercentage + ". This is beyond the valid range of 0-100%.");
            }

            OrganonStand? standAtEndOfPreviousPeriod = trajectory.StandByPeriod[this.Period - 1];
            OrganonStandDensity? densityAtEndOfPreviousPeriod = trajectory.DensityByPeriod[this.Period - 1];
            if ((standAtEndOfPreviousPeriod == null) || (densityAtEndOfPreviousPeriod == null))
            {
                throw new NotSupportedException("Stand information is not available for period " + (this.Period - 1) + ".");
            }

            Units standUnits = standAtEndOfPreviousPeriod.GetUnits();
            float diameterToCmMultiplier = standUnits.GetDbhConversionToMetric();
            float diameterToStandUnitsMultiplier = 1.0F / diameterToCmMultiplier;
            float basalAreaToStandUnitsMultiplier = 1.0F / standUnits.GetBasalAreaConversionToMetric();
            float totalBasalAreaAtEndOfPreviousPeriodInStandUnits = basalAreaToStandUnitsMultiplier * densityAtEndOfPreviousPeriod.BasalAreaPerHa;

            // flatten tree lists and sort by diameter
            // If needed, could special case to skip sorting if only proportional thinning's called for.
            int treeRecordsInStand = standAtEndOfPreviousPeriod.GetTreeRecordCount();
            int[] mergedIndicesCompacted = new int[treeRecordsInStand];
            FiaCode[] mergedSpecies = new FiaCode[treeRecordsInStand];
            float[] mergedDbh = new float[treeRecordsInStand];
            int flatDestinationIndex = 0;
            float totalThinnableBasalAreaInStandUnits = 0.0F;
            for (int speciesIndex = 0; speciesIndex < standAtEndOfPreviousPeriod.TreesBySpecies.Count; ++speciesIndex)
            {
                Trees treesOfSpecies = standAtEndOfPreviousPeriod.TreesBySpecies.Values[speciesIndex];
                FiaCode treeSpecies = treesOfSpecies.Species;
                if ((treesOfSpecies.Count == 0) || (trajectory.TreeScaling.TryGetForwarderVolumeTable(treeSpecies, out TreeSpeciesMerchantableVolumeTable? forwardedVolumeTable) == false))
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
                    // if needed, this could be thrown only when trees larger than a volume table limit are present
                    throw new NotSupportedException("Forwarded volume table's maximum DBH of " + forwardedVolumeTable.MaximumMerchantableDiameterInCentimeters + " cm differs from the long log volume table's " + longLogVolumeTable.MaximumMerchantableDiameterInCentimeters + " cm.  Since it is not known whether the thin will be performed as a long or short log harvest the largest harvest eligible tree size cannot be determined.");
                }

                float maximumFellableDbhInStandUnits = diameterToStandUnitsMultiplier * forwardedVolumeTable.MaximumMerchantableDiameterInCentimeters;
                Array.Copy(treesOfSpecies.Dbh, 0, mergedDbh, flatDestinationIndex, treesOfSpecies.Count);
                for (int compactedTreeIndex = 0; compactedTreeIndex < treesOfSpecies.Count; ++compactedTreeIndex)
                {
                    int uncompactedTreeIndex = treesOfSpecies.UncompactedIndex[compactedTreeIndex];
                    int currentHarvestPeriod = trajectory.TreeSelectionBySpecies[treesOfSpecies.Species][uncompactedTreeIndex];
                    if (currentHarvestPeriod == Constant.NoHarvestPeriod)
                    {
                        // trees marked not harvestable (reserve, nonmerchantable) can't be thinned so don't need to be flattened
                        // TODO: remove trees marked as cull?
                        continue;
                    }

                    float dbh = treesOfSpecies.Dbh[compactedTreeIndex];
                    if (dbh > maximumFellableDbhInStandUnits)
                    {
                        // for now, assume large tree retention
                        continue;
                    }

                    mergedDbh[flatDestinationIndex] = dbh;
                    mergedIndicesCompacted[flatDestinationIndex] = compactedTreeIndex;
                    mergedSpecies[flatDestinationIndex] = treeSpecies;
                    ++flatDestinationIndex;

                    totalThinnableBasalAreaInStandUnits += treesOfSpecies.GetBasalArea(compactedTreeIndex);
                }
            }

            int maxFlatDestinationIndexExclusive = flatDestinationIndex;
            if (maxFlatDestinationIndexExclusive < 1)
            {
                // no trees eligible for thinning
                // This case can be reached several different ways but is most likely to occur when a previous thin cuts all of the trees
                // eligible for removal or all eligible trees die out of the stand before this thinning period.
                Debug.Assert(totalThinnableBasalAreaInStandUnits == 0.0F);
                return 0.0F; // no basal area can be removed
            }

            Array.Fill(mergedDbh, Single.PositiveInfinity, flatDestinationIndex, mergedDbh.Length - flatDestinationIndex); // set any unused DBHes to infinity so they sort at end
            Debug.Assert(totalThinnableBasalAreaInStandUnits <= totalBasalAreaAtEndOfPreviousPeriodInStandUnits + Constant.Math.SinglePrecisionSumTolerance); // identical within numerical accuracy if stand is 100% merch species without any reserves

            int[] dbhSortIndices = ArrayExtensions.CreateSequentialIndices(treeRecordsInStand);
            Array.Sort(mergedDbh, dbhSortIndices);

            // thin from below
            float basalAreaToRemoveFromBelow = 0.01F * this.fromBelowPercentage * totalBasalAreaAtEndOfPreviousPeriodInStandUnits;
            float basalAreaRemovedFromBelow = 0.0F;
            int thinFromBelowIndex = 0;
            while (basalAreaRemovedFromBelow < basalAreaToRemoveFromBelow)
            {
                int flatTreeIndex = dbhSortIndices[thinFromBelowIndex];
                FiaCode treeSpecies = mergedSpecies[flatTreeIndex];
                int compactedTreeIndex = mergedIndicesCompacted[flatTreeIndex];
                
                // for now, thin individual tree records
                // Basal area target is thus very likely to be exceeded by a fraction of a tree record. If needed, exact removal can be
                // implemented by fractionally reducing the record's expansion factor instead of setting it to zero.
                Trees treesOfSpecies = standAtEndOfPreviousPeriod.TreesBySpecies[treeSpecies];
                int uncompactedTreeIndex = treesOfSpecies.UncompactedIndex[compactedTreeIndex];
                trajectory.SetTreeSelection(treeSpecies, uncompactedTreeIndex, this.Period);
                basalAreaRemovedFromBelow += treesOfSpecies.GetBasalArea(compactedTreeIndex);

                ++thinFromBelowIndex;
                if (thinFromBelowIndex >= maxFlatDestinationIndexExclusive)
                {
                    break;
                }
            }

            // thin from above
            float basalAreaToRemoveFromAbove = 0.01F * this.fromAbovePercentage * totalBasalAreaAtEndOfPreviousPeriodInStandUnits;
            float basalAreaRemovedFromAbove = 0.0F;
            int thinFromAboveIndex = maxFlatDestinationIndexExclusive - 1;
            while (basalAreaRemovedFromAbove < basalAreaToRemoveFromAbove)
            {
                int flatTreeIndex = dbhSortIndices[thinFromAboveIndex];
                FiaCode treeSpecies = mergedSpecies[flatTreeIndex];
                int compactedTreeIndex = mergedIndicesCompacted[flatTreeIndex];

                Trees treesOfSpecies = standAtEndOfPreviousPeriod.TreesBySpecies[treeSpecies];
                int uncompactedTreeIndex = treesOfSpecies.UncompactedIndex[compactedTreeIndex];
                trajectory.SetTreeSelection(treeSpecies, uncompactedTreeIndex, this.Period);
                basalAreaRemovedFromAbove += treesOfSpecies.GetBasalArea(compactedTreeIndex);
                
                --thinFromAboveIndex;
                if (thinFromAboveIndex <= thinFromBelowIndex)
                {
                    break;
                }
            }

            // thin remaining trees proportionally or unmark trees from thinning if no longer selected at the current intensities
            // For now, assumes sufficient tree records and even enough expansion factors for the error of using constant increment
            // accumulation to average out. Variable rate accumulation linked to trees' basal area and expansion factors would be more
            // robust and thus likely preferable.
            // float basalAreaToRemoveProportionally = 0.01F * this.fromAbovePercentage * totalBasalAreaAtEndOfPreviousPeriodInStandUnits;
            float basalAreaRemovedProportionally = 0.0F;
            float proportionalThinIncrementAccumulator = 0.0F;
            float proportionalThinIncrementInMeanTreeRecords = 0.01F * this.ProportionalPercentage * 100.0F / (100.0F - this.FromAbovePercentage - this.FromBelowPercentage);
            for (int proportionalThinIndex = thinFromBelowIndex; proportionalThinIndex <= thinFromAboveIndex; ++proportionalThinIndex)
            {
                int flatTreeIndex = dbhSortIndices[proportionalThinIndex];
                FiaCode treeSpecies = mergedSpecies[flatTreeIndex];
                int compactedTreeIndex = mergedIndicesCompacted[flatTreeIndex];

                Trees treesOfSpecies = standAtEndOfPreviousPeriod.TreesBySpecies[treeSpecies];
                int uncompactedTreeIndex = treesOfSpecies.UncompactedIndex[compactedTreeIndex];                

                proportionalThinIncrementAccumulator += proportionalThinIncrementInMeanTreeRecords;
                if (proportionalThinIncrementAccumulator >= 1.0F)
                {                    
                    trajectory.SetTreeSelection(treeSpecies, uncompactedTreeIndex, this.Period);

                    float basalAreaOfTree = treesOfSpecies.GetBasalArea(compactedTreeIndex);
                    basalAreaRemovedProportionally += basalAreaOfTree;
                    proportionalThinIncrementAccumulator -= 1.0F;
                }
                else
                {
                    int currentHarvestPeriod = trajectory.TreeSelectionBySpecies[treesOfSpecies.Species][uncompactedTreeIndex];
                    if (currentHarvestPeriod == this.Period)
                    {
                        // for now, assume this is the only harvest prescription active for this period
                        // This makes the prescription authorative for tree assignments in the period and, therefore, able to release trees from
                        // harvest. Separate processing is not needed for thins from below or above as this loop always runs and will thus mark
                        // (above case) or unmark (this case) every harvest eligible tree not marked for thinning from above or below. (This will
                        // break if the definition of harvest eligibility changes at runtime, but that's not currently supported.)
                        trajectory.SetTreeSelection(treeSpecies, uncompactedTreeIndex, Constant.RegenerationHarvestIfEligible);
                    }
                }
            }

            float basalAreaRemovedInStandUnits = basalAreaRemovedFromAbove + basalAreaRemovedProportionally + basalAreaRemovedFromBelow;
            Debug.Assert((totalPercentage >= 0.0F && basalAreaRemovedInStandUnits > 0.0F) || (((int)(0.01F * totalPercentage * maxFlatDestinationIndexExclusive - Constant.Math.RoundTowardsZeroTolerance) <= 1.0F) && (basalAreaRemovedInStandUnits == 0.0F)));
            return basalAreaRemovedInStandUnits;
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
