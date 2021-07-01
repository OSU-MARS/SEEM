using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm
{
    public class StandCubicAndScribnerVolume : StandScribnerVolume
    {
        public float[] Cubic2Saw { get; private init; }
        public float[] Cubic3Saw { get; private init; }
        public float[] Cubic4Saw { get; private init; }

        public StandCubicAndScribnerVolume(StandScribnerVolume scribnerVolume)
            : base(scribnerVolume)
        {
            this.Cubic2Saw = new float[this.Scribner2Saw.Length];
            this.Cubic3Saw = new float[this.Scribner3Saw.Length];
            this.Cubic4Saw = new float[this.Scribner4Saw.Length];
        }

        public float GetCubicTotal(int periodIndex)
        {
            return this.Cubic2Saw[periodIndex] + this.Cubic3Saw[periodIndex] + this.Cubic4Saw[periodIndex];
        }

        public void SetCubicVolumeHarvested(Stand previousStand, SortedDictionary<FiaCode, TreeSelection> individualTreeSelectionBySpecies, int periodIndex, TimberValue timberValue)
        {
            double harvested2SawCubicMetersPerAcre = 0.0;
            double harvested3SawCubicMetersPerAcre = 0.0;
            double harvested4SawCubicMetersPerAcre = 0.0;
            foreach (Trees previousTreesOfSpecies in previousStand.TreesBySpecies.Values)
            {
                Debug.Assert(previousTreesOfSpecies.Units == Units.English, "TODO: per hectare.");

                TreeSelection individualTreeSelection = individualTreeSelectionBySpecies[previousTreesOfSpecies.Species];
                timberValue.ScaledVolumeThinning.GetHarvestedCubicVolume(previousTreesOfSpecies, individualTreeSelection, periodIndex, out double cubic2saw, out double cubic3saw, out double cubic4saw);
                harvested2SawCubicMetersPerAcre += cubic2saw;
                harvested3SawCubicMetersPerAcre += cubic3saw;
                harvested4SawCubicMetersPerAcre += cubic4saw;
            }

            this.Cubic2Saw[periodIndex] = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)harvested2SawCubicMetersPerAcre;
            this.Cubic3Saw[periodIndex] = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)harvested3SawCubicMetersPerAcre;
            this.Cubic4Saw[periodIndex] = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)harvested4SawCubicMetersPerAcre;
        }

        public void SetStandingCubicVolume(Stand stand, int periodIndex, TimberValue timberValue)
        {
            double standing2SawCubicMetersPerAcre = 0.0;
            double standing3SawCubicMetersPerAcre = 0.0;
            double standing4SawCubicMetersPerAcre = 0.0;
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                Debug.Assert(treesOfSpecies.Units == Units.English, "TODO: per hectare.");

                timberValue.ScaledVolumeRegenerationHarvest.GetStandingCubicVolume(treesOfSpecies, out double cubic2saw, out double cubic3saw, out double cubic4saw);
                standing2SawCubicMetersPerAcre += cubic2saw;
                standing3SawCubicMetersPerAcre += cubic3saw;
                standing4SawCubicMetersPerAcre += cubic4saw;
            }

            this.Cubic2Saw[periodIndex] = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)standing2SawCubicMetersPerAcre;
            this.Cubic3Saw[periodIndex] = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)standing3SawCubicMetersPerAcre;
            this.Cubic4Saw[periodIndex] = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)standing4SawCubicMetersPerAcre;
        }
    }
}
