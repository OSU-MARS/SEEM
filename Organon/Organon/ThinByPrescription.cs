using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Organon
{
    public class ThinByPrescription : IHarvest
    {
        private readonly float fromAboveProportion;
        private readonly float fromBelowProportion;
        private readonly float proportionalProportion;

        public int Period { get; private set; }

        public ThinByPrescription(int harvestAtBeginningOfPeriod, float fromAbovePercentage, float proportionalPercentage, float fromBelowPercentage)
        {
            if (harvestAtBeginningOfPeriod < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(harvestAtBeginningOfPeriod));
            }
            if ((fromAbovePercentage < 0.0F) || (fromAbovePercentage >= 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(fromAbovePercentage));
            }
            if ((proportionalPercentage < 0.0F) || (proportionalPercentage >= 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(proportionalPercentage));
            }
            if ((fromBelowPercentage < 0.0F) || (fromBelowPercentage >= 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(fromBelowPercentage));
            }
            float totalPercentage = fromAbovePercentage + fromBelowPercentage + proportionalPercentage;
            if ((totalPercentage <= 0.0F) || (totalPercentage >= 100.0F))
            {
                throw new ArgumentOutOfRangeException();
            }

            this.fromAboveProportion = 0.01F * fromAbovePercentage;
            this.fromBelowProportion = 0.01F * fromBelowPercentage;
            this.proportionalProportion = 0.01F * proportionalPercentage;
            this.Period = harvestAtBeginningOfPeriod;
        }

        public float EvaluateTreeSelection(OrganonStandTrajectory trajectory)
        {
            OrganonStand standAtEndOfPreviousPeriod = trajectory.StandByPeriod[this.Period - 1];

            // sort trees by diameter
            SortedDictionary<FiaCode, int[]> dbhSortOrderBySpecies = new SortedDictionary<FiaCode, int[]>();
            SortedDictionary<FiaCode, int> thinFromAboveIndexBySpecies = new SortedDictionary<FiaCode, int>();
            SortedDictionary<FiaCode, int> thinFromBelowIndexBySpecies = new SortedDictionary<FiaCode, int>();
            float maximumDiameter = Single.MinValue;
            float minimumDiameter = Single.MaxValue;
            FiaCode maximumSpecies = default;
            FiaCode minimumSpecies = default;
            foreach (Trees treesOfSpecies in standAtEndOfPreviousPeriod.TreesBySpecies.Values)
            {
                int[] dbhSortOrder = treesOfSpecies.GetDbhSortOrder();
                dbhSortOrderBySpecies.Add(treesOfSpecies.Species, dbhSortOrder);
                thinFromAboveIndexBySpecies.Add(treesOfSpecies.Species, treesOfSpecies.Count);
                thinFromBelowIndexBySpecies.Add(treesOfSpecies.Species, 0);

                float largestDbh = treesOfSpecies.Dbh[dbhSortOrder[^1]];
                if (largestDbh > maximumDiameter)
                {
                    maximumDiameter = largestDbh;
                    maximumSpecies = treesOfSpecies.Species;
                }

                float smallestDbh = treesOfSpecies.Dbh[dbhSortOrder[0]];
                if (smallestDbh < minimumDiameter)
                {
                    minimumDiameter = smallestDbh;
                    minimumSpecies = treesOfSpecies.Species;
                }
            }

            // thin from above
            OrganonStandDensity densityAtEndOfPreviousPeriod = trajectory.DensityByPeriod[this.Period - 1];
            float targetBasalArea = this.fromAboveProportion * densityAtEndOfPreviousPeriod.BasalAreaPerAcre;
            float basalAreaRemovedFromAbove = 0.0F;
            while (basalAreaRemovedFromAbove < targetBasalArea)
            {
                Trees treesWithLargest = standAtEndOfPreviousPeriod.TreesBySpecies[maximumSpecies];
                int thinIndex = thinFromAboveIndexBySpecies[maximumSpecies] - 1;
                int treeIndex = dbhSortOrderBySpecies[maximumSpecies][thinIndex];
                float basalAreaOfTree = treesWithLargest.GetBasalArea(treeIndex);

                trajectory.SetTreeSelection(maximumSpecies, treeIndex, this.Period);

                basalAreaRemovedFromAbove += basalAreaOfTree; // for now, use complete removal of tree's expansion factor
                thinFromAboveIndexBySpecies[maximumSpecies] = thinIndex;

                maximumDiameter = Single.MinValue;
                foreach (Trees treesOfSpecies in standAtEndOfPreviousPeriod.TreesBySpecies.Values)
                {
                    int[] dbhSortOrder = dbhSortOrderBySpecies[treesOfSpecies.Species];
                    thinIndex = thinFromAboveIndexBySpecies[treesOfSpecies.Species];

                    float largestDbh = treesOfSpecies.Dbh[dbhSortOrder[thinIndex]];
                    if (largestDbh > maximumDiameter)
                    {
                        maximumDiameter = largestDbh;
                        maximumSpecies = treesOfSpecies.Species;
                    }
                }
            }

            // thin from below
            targetBasalArea = this.fromBelowProportion * densityAtEndOfPreviousPeriod.BasalAreaPerAcre;
            float basalAreaRemovedFromBelow = 0.0F;
            while (basalAreaRemovedFromBelow < targetBasalArea)
            {
                Trees treesWithSmallest = standAtEndOfPreviousPeriod.TreesBySpecies[minimumSpecies];
                int thinIndex = thinFromBelowIndexBySpecies[minimumSpecies];
                int treeIndex = dbhSortOrderBySpecies[minimumSpecies][thinIndex];
                float basalAreaOfTree = treesWithSmallest.GetBasalArea(treeIndex);

                trajectory.SetTreeSelection(minimumSpecies, treeIndex, this.Period);

                basalAreaRemovedFromBelow += basalAreaOfTree;
                thinFromBelowIndexBySpecies[minimumSpecies] = thinIndex + 1;

                minimumDiameter = Single.MaxValue;
                foreach (Trees treesOfSpecies in standAtEndOfPreviousPeriod.TreesBySpecies.Values)
                {
                    int[] dbhSortOrder = dbhSortOrderBySpecies[treesOfSpecies.Species];
                    thinIndex = thinFromBelowIndexBySpecies[treesOfSpecies.Species];

                    float smallestDbh = treesOfSpecies.Dbh[dbhSortOrder[thinIndex]];
                    if (smallestDbh < minimumDiameter)
                    {
                        minimumDiameter = smallestDbh;
                        minimumSpecies = treesOfSpecies.Species;
                    }
                }
            }

            // thin remaining trees proportionally
            float proportionalThinAccumulator = 0.0F;
            float proportionalIncrement = 1.0F / (1.0F - this.fromAboveProportion - this.fromBelowProportion) * this.proportionalProportion;
            float basalAreaRemovedProportionally = 0.0F;
            foreach (KeyValuePair<FiaCode, int[]> speciesDbhSortOrder in dbhSortOrderBySpecies)
            {
                int[] dbhSortOrder = speciesDbhSortOrder.Value;
                Trees treesOfSpecies = standAtEndOfPreviousPeriod.TreesBySpecies[speciesDbhSortOrder.Key];

                for (int thinIndex = thinFromBelowIndexBySpecies[speciesDbhSortOrder.Key]; thinIndex < thinFromAboveIndexBySpecies[speciesDbhSortOrder.Key]; ++thinIndex)
                {
                    proportionalThinAccumulator += proportionalIncrement;
                    if (proportionalThinAccumulator >= 1.0F)
                    {
                        int treeIndex = dbhSortOrder[thinIndex];

                        float basalAreaOfTree = treesOfSpecies.GetBasalArea(treeIndex);
                        trajectory.SetTreeSelection(speciesDbhSortOrder.Key, treeIndex, this.Period);
                        basalAreaRemovedProportionally += basalAreaOfTree;
                        
                        proportionalThinAccumulator -= 1.0F;
                    }
                }
            }

            float basalAreaRemoved = basalAreaRemovedFromAbove + basalAreaRemovedProportionally + basalAreaRemovedFromBelow;
            Debug.Assert(basalAreaRemoved > 0.0F);
            return basalAreaRemoved;
        }
    }
}
