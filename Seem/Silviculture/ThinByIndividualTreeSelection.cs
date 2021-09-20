using Osu.Cof.Ferm.Organon;
using Osu.Cof.Ferm.Tree;
using System;
using System.Collections.Generic;

namespace Osu.Cof.Ferm.Silviculture
{
    public class ThinByIndividualTreeSelection : Harvest
    {
        public ThinByIndividualTreeSelection(int harvestPeriod)
        {
            if (harvestPeriod < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(harvestPeriod));
            }

            this.Period = harvestPeriod;
        }

        public override Harvest Clone()
        {
            return new ThinByIndividualTreeSelection(this.Period);
        }

        public override float EvaluateTreeSelection(OrganonStandTrajectory trajectory)
        {
            float basalAreaRemoved = 0.0F;
            OrganonStand standAtEndOfPreviousPeriod = trajectory.StandByPeriod[this.Period - 1] ?? throw new NotSupportedException("No stand information prior to thinning.");
            foreach (KeyValuePair<FiaCode, TreeSelection> treeSelectionForSpecies in trajectory.IndividualTreeSelectionBySpecies)
            {
                Trees treesOfSpecies = standAtEndOfPreviousPeriod.TreesBySpecies[treeSelectionForSpecies.Key];
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    int harvestPeriod = treeSelectionForSpecies.Value[treeIndex];
                    if (harvestPeriod == this.Period)
                    {
                        float basalArea = treesOfSpecies.GetBasalArea(treeIndex);
                        basalAreaRemoved += basalArea;
                    }
                }
            }

            return basalAreaRemoved;
        }

        public override bool TryCopyFrom(Harvest other)
        {
            if (other is ThinByIndividualTreeSelection)
            {
                this.Period = other.Period;
                return true;
            }

            if (other is ThinByPrescription)
            {
                // needed for individual tree selection to pick up from prescription enumeration
                throw new NotImplementedException();
            }

            return false;
        }
    }
}