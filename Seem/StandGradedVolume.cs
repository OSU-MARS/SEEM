using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm
{
    public class StandGradedVolume
    {
        public float[] Cubic2Saw { get; private set; }
        public float[] Cubic3Saw { get; private set; }
        public float[] Cubic4Saw { get; private set; }
        public float[] NetPresentValue2Saw { get; private set; }
        public float[] NetPresentValue3Saw { get; private set; }
        public float[] NetPresentValue4Saw { get; private set; }
        public float[] Scribner2Saw { get; private set; }
        public float[] Scribner3Saw { get; private set; }
        public float[] Scribner4Saw { get; private set; }

        public StandGradedVolume(int planningPeriods)
        {
            this.Cubic2Saw = new float[planningPeriods];
            this.Cubic3Saw = new float[planningPeriods];
            this.Cubic4Saw = new float[planningPeriods];
            this.NetPresentValue2Saw = new float[planningPeriods];
            this.NetPresentValue3Saw = new float[planningPeriods];
            this.NetPresentValue4Saw = new float[planningPeriods];
            this.Scribner2Saw = new float[planningPeriods];
            this.Scribner3Saw = new float[planningPeriods];
            this.Scribner4Saw = new float[planningPeriods];
        }

        public float GetCubicTotal(int periodIndex)
        {
            return this.Cubic2Saw[periodIndex] + this.Cubic3Saw[periodIndex] + this.Cubic4Saw[periodIndex];
        }

        public void FromStand(Stand previousStand, SortedDictionary<FiaCode, int[]> individualTreeSelectionBySpecies, int periodIndex, TimberValue timberValue, int thinAge)
        {
            double harvested2SawCubicMetersPerAcre = 0.0;
            double harvested3SawCubicMetersPerAcre = 0.0;
            double harvested4SawCubicMetersPerAcre = 0.0;
            double harvested2SawScribnerPerAcre = 0.0;
            double harvested3SawScribnerPerAcre = 0.0;
            double harvested4SawScribnerPerAcre = 0.0;
            foreach (Trees previousTreesOfSpecies in previousStand.TreesBySpecies.Values)
            {
                Debug.Assert(previousTreesOfSpecies.Units == Units.English, "TODO: per hectare.");

                int[] individualTreeSelection = individualTreeSelectionBySpecies[previousTreesOfSpecies.Species];
                timberValue.ScaledVolumeThinning.GetGradedVolume(previousTreesOfSpecies, individualTreeSelection, out double cubic2saw, out double cubic3saw, out double cubic4saw, out double scribner2saw, out double scribner3saw, out double scribner4saw);
                harvested2SawCubicMetersPerAcre += cubic2saw;
                harvested3SawCubicMetersPerAcre += cubic3saw;
                harvested4SawCubicMetersPerAcre += cubic4saw;
                harvested2SawScribnerPerAcre += scribner2saw;
                harvested3SawScribnerPerAcre += scribner3saw;
                harvested4SawScribnerPerAcre += scribner4saw;
            }

            this.Cubic2Saw[periodIndex] = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)harvested2SawCubicMetersPerAcre;
            this.Cubic3Saw[periodIndex] = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)harvested3SawCubicMetersPerAcre;
            this.Cubic4Saw[periodIndex] = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)harvested4SawCubicMetersPerAcre;
            this.Scribner2Saw[periodIndex] = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)harvested2SawScribnerPerAcre;
            this.Scribner3Saw[periodIndex] = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)harvested3SawScribnerPerAcre;
            this.Scribner4Saw[periodIndex] = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)harvested4SawScribnerPerAcre;

            float appreciationFactor = timberValue.GetTimberAppreciationFactor(thinAge);
            float discountFactor = timberValue.GetDiscountFactor(thinAge);
            this.NetPresentValue2Saw[periodIndex] = discountFactor * (appreciationFactor * timberValue.DouglasFir2SawPondValuePerMbf - timberValue.RegenerationHarvestCostPerMbf) * this.Scribner2Saw[periodIndex];
            this.NetPresentValue3Saw[periodIndex] = discountFactor * (appreciationFactor * timberValue.DouglasFir3SawPondValuePerMbf - timberValue.RegenerationHarvestCostPerMbf) * this.Scribner3Saw[periodIndex];
            this.NetPresentValue4Saw[periodIndex] = discountFactor * (appreciationFactor * timberValue.DouglasFir4SawPondValuePerMbf - timberValue.RegenerationHarvestCostPerMbf) * this.Scribner4Saw[periodIndex];
        }

        public void FromStand(Stand stand, int periodIndex, TimberValue timberValue, int harvestAge)
        {
            double standing2SawCubicMetersPerAcre = 0.0;
            double standing3SawCubicMetersPerAcre = 0.0;
            double standing4SawCubicMetersPerAcre = 0.0;
            double standing2SawBoardFeetPerAcre = 0.0;
            double standing3SawBoardFeetPerAcre = 0.0;
            double standing4SawBoardFeetPerAcre = 0.0;
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                Debug.Assert(treesOfSpecies.Units == Units.English, "TODO: per hectare.");

                timberValue.ScaledVolumeRegenerationHarvest.GetGradedVolume(treesOfSpecies, out double cubic2saw, out double cubic3saw, out double cubic4saw, out double scribner2saw, out double scribner3saw, out double scribner4saw);
                standing2SawCubicMetersPerAcre += cubic2saw;
                standing3SawCubicMetersPerAcre += cubic3saw;
                standing4SawCubicMetersPerAcre += cubic4saw;
                standing2SawBoardFeetPerAcre += scribner2saw;
                standing3SawBoardFeetPerAcre += scribner3saw;
                standing4SawBoardFeetPerAcre += scribner4saw;
            }

            this.Cubic2Saw[periodIndex] = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)standing2SawCubicMetersPerAcre;
            this.Cubic3Saw[periodIndex] = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)standing3SawCubicMetersPerAcre;
            this.Cubic4Saw[periodIndex] = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)standing4SawCubicMetersPerAcre;
            this.Scribner2Saw[periodIndex] = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)standing2SawBoardFeetPerAcre;
            this.Scribner3Saw[periodIndex] = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)standing3SawBoardFeetPerAcre;
            this.Scribner4Saw[periodIndex] = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)standing4SawBoardFeetPerAcre;

            float appreciationFactor = timberValue.GetTimberAppreciationFactor(harvestAge);
            float discountFactor = timberValue.GetDiscountFactor(harvestAge);
            this.NetPresentValue2Saw[periodIndex] = discountFactor * (appreciationFactor * timberValue.DouglasFir2SawPondValuePerMbf - timberValue.RegenerationHarvestCostPerMbf) * this.Scribner2Saw[periodIndex];
            this.NetPresentValue3Saw[periodIndex] = discountFactor * (appreciationFactor * timberValue.DouglasFir3SawPondValuePerMbf - timberValue.RegenerationHarvestCostPerMbf) * this.Scribner3Saw[periodIndex];
            this.NetPresentValue4Saw[periodIndex] = discountFactor * (appreciationFactor * timberValue.DouglasFir4SawPondValuePerMbf - timberValue.RegenerationHarvestCostPerMbf) * this.Scribner4Saw[periodIndex];
        }
    }
}
