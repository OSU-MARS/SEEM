using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Organon
{
    public class ThinByPrescription : IHarvest
    {
        private readonly float fromBelowProportion;
        private readonly float proportionalProportion;

        public int Period { get; private set; }

        public ThinByPrescription(int harvestPeriod, float fromBelowPercentage, float proportionalPercentage)
        {
            if (harvestPeriod < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(harvestPeriod));
            }
            if ((fromBelowPercentage <= 0.0F) || (fromBelowPercentage > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(fromBelowPercentage));
            }
            if ((proportionalPercentage <= 0.0F) || (fromBelowPercentage + proportionalPercentage > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(proportionalPercentage));
            }
            if (fromBelowPercentage + proportionalPercentage <= 0.0F)
            {
                throw new ArgumentOutOfRangeException();
            }

            this.fromBelowProportion = 0.01F * fromBelowPercentage;
            this.proportionalProportion = 0.01F * proportionalPercentage;
            this.Period = harvestPeriod;
        }

        public float EvaluateTreeSelection(OrganonStandTrajectory trajectory)
        {
            OrganonStand standAtEndOfPreviousPeriod = trajectory.StandByPeriod[this.Period - 1];

            // thin from below
            SortedDictionary<FiaCode, int[]> dbhSortOrderBySpecies = new SortedDictionary<FiaCode, int[]>();
            SortedDictionary<FiaCode, int> thinIndexBySpecies = new SortedDictionary<FiaCode, int>();
            float minimumDiameter = Single.MaxValue;
            FiaCode minimumSpecies = default;
            foreach (Trees treesOfSpecies in standAtEndOfPreviousPeriod.TreesBySpecies.Values)
            {
                int[] dbhSortOrder = treesOfSpecies.GetDbhSortOrder();
                dbhSortOrderBySpecies.Add(treesOfSpecies.Species, dbhSortOrder);
                thinIndexBySpecies.Add(treesOfSpecies.Species, 0);

                float smallestDbh = treesOfSpecies.Dbh[dbhSortOrder[0]];
                if (smallestDbh < minimumDiameter)
                {
                    minimumDiameter = smallestDbh;
                    minimumSpecies = treesOfSpecies.Species;
                }
            }

            OrganonStandDensity densityAtEndOfPreviousPeriod = trajectory.DensityByPeriod[this.Period - 1];
            float targetBasalArea = this.fromBelowProportion * densityAtEndOfPreviousPeriod.BasalAreaPerAcre;
            float basalAreaRemoved = 0.0F;
            while (basalAreaRemoved < targetBasalArea)
            {
                Trees treesWithSmallest = standAtEndOfPreviousPeriod.TreesBySpecies[minimumSpecies];
                int thinIndex = thinIndexBySpecies[minimumSpecies];
                int treeIndex = dbhSortOrderBySpecies[minimumSpecies][thinIndex];
                float basalAreaOfTree = treesWithSmallest.GetBasalArea(treeIndex);

                trajectory.SetTreeSelection(minimumSpecies, treeIndex, this.Period);

                basalAreaRemoved += basalAreaOfTree; // for now, use complete removal of tree's expansion factor
                thinIndexBySpecies[minimumSpecies] = thinIndex + 1;

                minimumDiameter = Single.MaxValue;
                foreach (Trees treesOfSpecies in standAtEndOfPreviousPeriod.TreesBySpecies.Values)
                {
                    int[] dbhSortOrder = dbhSortOrderBySpecies[treesOfSpecies.Species];
                    thinIndex = thinIndexBySpecies[treesOfSpecies.Species];

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
            foreach (KeyValuePair<FiaCode, int[]> speciesDbhSortOrder in dbhSortOrderBySpecies)
            {
                int[] dbhSortOrder = speciesDbhSortOrder.Value;
                Trees treesOfSpecies = standAtEndOfPreviousPeriod.TreesBySpecies[speciesDbhSortOrder.Key];

                for (int thinIndex = thinIndexBySpecies[speciesDbhSortOrder.Key]; thinIndex < dbhSortOrder.Length; ++thinIndex)
                {
                    proportionalThinAccumulator += this.proportionalProportion;
                    if (proportionalThinAccumulator >= 1.0F)
                    {
                        int treeIndex = dbhSortOrder[thinIndex];

                        float basalAreaOfTree = treesOfSpecies.GetBasalArea(treeIndex);
                        trajectory.SetTreeSelection(speciesDbhSortOrder.Key, treeIndex, this.Period);
                        basalAreaRemoved += basalAreaOfTree;
                        
                        proportionalThinAccumulator -= 1.0F;
                    }
                }
            }

            Debug.Assert(basalAreaRemoved > 0.0F);
            return basalAreaRemoved;
        }
    }
}
