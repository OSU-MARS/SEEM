﻿using Mars.Seem.Organon;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;

namespace Mars.Seem.Silviculture
{
    public class ThinByIndividualTreeSelection : Harvest
    {
        public ThinByIndividualTreeSelection(int harvestPeriod)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(harvestPeriod, 1);

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
            foreach (KeyValuePair<FiaCode, IndividualTreeSelection> treeSelectionForSpecies in trajectory.TreeSelectionBySpecies)
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