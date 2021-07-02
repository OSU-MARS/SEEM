using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm
{
    public class StandCubicAndScribnerVolume
    {
        public float[] Cubic2Saw { get; private init; }
        public float[] Cubic3Saw { get; private init; }
        public float[] Cubic4Saw { get; private init; }
        public float[] Scribner2Saw { get; private init; }
        public float[] Scribner3Saw { get; private init; }
        public float[] Scribner4Saw { get; private init; }

        public StandCubicAndScribnerVolume(SortedList<FiaCode, SpeciesScribnerVolume> scribnerVolumeBySpecies)
        {
            if (scribnerVolumeBySpecies.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(scribnerVolumeBySpecies));
            }
            
            int planningPeriods = scribnerVolumeBySpecies.Values[0].Scribner2Saw.Length;
            this.Cubic2Saw = new float[planningPeriods];
            this.Cubic3Saw = new float[planningPeriods];
            this.Cubic4Saw = new float[planningPeriods];
            this.Scribner2Saw = new float[planningPeriods];
            this.Scribner3Saw = new float[planningPeriods];
            this.Scribner4Saw = new float[planningPeriods];

            foreach (SpeciesScribnerVolume scribnerForSpecies in scribnerVolumeBySpecies.Values)
            {
                for (int periodIndex = 0; periodIndex < planningPeriods; ++periodIndex)
                {
                    this.Scribner2Saw[periodIndex] += scribnerForSpecies.Scribner2Saw[periodIndex];
                    this.Scribner3Saw[periodIndex] += scribnerForSpecies.Scribner3Saw[periodIndex];
                    this.Scribner4Saw[periodIndex] += scribnerForSpecies.Scribner4Saw[periodIndex];
                }
            }
        }

        public float GetCubicTotal(int periodIndex)
        {
            return this.Cubic2Saw[periodIndex] + this.Cubic3Saw[periodIndex] + this.Cubic4Saw[periodIndex];
        }

        public float GetScribnerTotal(int periodIndex)
        {
            return this.Scribner2Saw[periodIndex] + this.Scribner3Saw[periodIndex] + this.Scribner4Saw[periodIndex];
        }

        public void SetCubicVolumeHarvested(Stand previousStand, SortedList<FiaCode, TreeSelection> individualTreeSelectionBySpecies, int periodIndex, TreeVolume treeVolume)
        {
            float harvested2SawCubicMetersPerAcre = 0.0F;
            float harvested3SawCubicMetersPerAcre = 0.0F;
            float harvested4SawCubicMetersPerAcre = 0.0F;
            foreach (Trees previousTreesOfSpecies in previousStand.TreesBySpecies.Values)
            {
                Debug.Assert(previousTreesOfSpecies.Units == Units.English, "TODO: per hectare.");

                TreeSelection individualTreeSelection = individualTreeSelectionBySpecies[previousTreesOfSpecies.Species];
                treeVolume.Thinning.GetHarvestedCubicVolume(previousTreesOfSpecies, individualTreeSelection, periodIndex, out float cubic2saw, out float cubic3saw, out float cubic4saw);
                harvested2SawCubicMetersPerAcre += cubic2saw;
                harvested3SawCubicMetersPerAcre += cubic3saw;
                harvested4SawCubicMetersPerAcre += cubic4saw;
            }

            this.Cubic2Saw[periodIndex] = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * harvested2SawCubicMetersPerAcre;
            this.Cubic3Saw[periodIndex] = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * harvested3SawCubicMetersPerAcre;
            this.Cubic4Saw[periodIndex] = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * harvested4SawCubicMetersPerAcre;
        }

        public void SetStandingCubicVolume(Stand stand, int periodIndex, TreeVolume treeVolume)
        {
            float standing2SawCubicMetersPerAcre = 0.0F;
            float standing3SawCubicMetersPerAcre = 0.0F;
            float standing4SawCubicMetersPerAcre = 0.0F;
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                Debug.Assert(treesOfSpecies.Units == Units.English, "TODO: per hectare.");

                treeVolume.RegenerationHarvest.GetStandingCubicVolume(treesOfSpecies, out float cubic2saw, out float cubic3saw, out float cubic4saw);
                standing2SawCubicMetersPerAcre += cubic2saw;
                standing3SawCubicMetersPerAcre += cubic3saw;
                standing4SawCubicMetersPerAcre += cubic4saw;
            }

            this.Cubic2Saw[periodIndex] = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * standing2SawCubicMetersPerAcre;
            this.Cubic3Saw[periodIndex] = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * standing3SawCubicMetersPerAcre;
            this.Cubic4Saw[periodIndex] = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * standing4SawCubicMetersPerAcre;
        }
    }
}
