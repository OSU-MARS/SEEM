using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm
{
    public class StandVolume
    {
        /// <summary>
        /// Net value in US$/ha.
        /// </summary>
        public float[] NetPresentValue { get; private init; }
        /// <summary>
        /// Scribner log volume in MBF/ha.
        /// </summary>
        public float[] ScribnerTotal { get; private init; }

        public StandVolume(int planningPeriods)
        {
            this.NetPresentValue = new float[planningPeriods];
            this.ScribnerTotal = new float[planningPeriods];
        }

        public StandVolume(StandVolume other)
            : this(other.ScribnerTotal.Length)
        {
            this.CopyFrom(other);
        }

        public void CopyFrom(StandVolume other)
        {
            Array.Copy(other.NetPresentValue, 0, this.NetPresentValue, 0, other.NetPresentValue.Length);
            Array.Copy(other.ScribnerTotal, 0, this.ScribnerTotal, 0, other.ScribnerTotal.Length);
        }

        public void FromStand(Stand previousStand, SortedDictionary<FiaCode, int[]> individualTreeSelectionBySpecies, int periodIndex, TimberValue timberValue, int thinAge)
        {
            double harvested2SawBoardFeetPerAcre = 0.0F;
            double harvested3SawBoardFeetPerAcre = 0.0F;
            double harvested4SawBoardFeetPerAcre = 0.0F;
            foreach (Trees previousTreesOfSpecies in previousStand.TreesBySpecies.Values)
            {
                Debug.Assert(previousTreesOfSpecies.Units == Units.English, "TODO: per hectare.");

                int[] individualTreeSelection = individualTreeSelectionBySpecies[previousTreesOfSpecies.Species];
                timberValue.ScaledVolumeThinning.GetScribnerVolume(previousTreesOfSpecies, individualTreeSelection, periodIndex, out double scribner2saw, out double scribner3saw, out double scribner4saw);
                harvested2SawBoardFeetPerAcre += scribner2saw;
                harvested3SawBoardFeetPerAcre += scribner3saw;
                harvested4SawBoardFeetPerAcre += scribner4saw;
            }

            float net2SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)harvested2SawBoardFeetPerAcre;
            float net3SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)harvested3SawBoardFeetPerAcre;
            float net4SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)harvested4SawBoardFeetPerAcre;
            this.ScribnerTotal[periodIndex] = net2SawMbfPerHectare + net3SawMbfPerHectare + net4SawMbfPerHectare;

            if (this.ScribnerTotal[periodIndex] <= 0.001F)
            {
                Debug.Assert(harvested2SawBoardFeetPerAcre == 0.0F);
                Debug.Assert(harvested3SawBoardFeetPerAcre == 0.0F);
                Debug.Assert(harvested4SawBoardFeetPerAcre == 0.0F);
                this.NetPresentValue[periodIndex] = 0.0F;
            }
            else
            {
                this.NetPresentValue[periodIndex] = timberValue.GetNetPresentThinningValue(net2SawMbfPerHectare, net3SawMbfPerHectare, net4SawMbfPerHectare, thinAge);
                // float harvestNpvSimple = this.TimberValue.GetNetPresentThinningValue(this.ScribnerTotal[periodIndex], thinAge);
            }
        }

        public void FromStand(Stand stand, int periodIndex, TimberValue timberValue, int harvestAge)
        {
            double standing2SawBoardFeetPerAcre = 0.0;
            double standing3SawBoardFeetPerAcre = 0.0;
            double standing4SawBoardFeetPerAcre = 0.0;
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                Debug.Assert(treesOfSpecies.Units == Units.English, "TODO: per hectare.");

                timberValue.ScaledVolumeRegenerationHarvest.GetScribnerVolume(treesOfSpecies, out double scribner2saw, out double scribner3saw, out double scribner4saw);
                standing2SawBoardFeetPerAcre += scribner2saw;
                standing3SawBoardFeetPerAcre += scribner3saw;
                standing4SawBoardFeetPerAcre += scribner4saw;
            }

            float net2SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)standing2SawBoardFeetPerAcre;
            float net3SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)standing3SawBoardFeetPerAcre;
            float net4SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)standing4SawBoardFeetPerAcre;
            this.ScribnerTotal[periodIndex] = net2SawMbfPerHectare + net3SawMbfPerHectare + net4SawMbfPerHectare;
            this.NetPresentValue[periodIndex] = timberValue.GetNetPresentRegenerationHarvestValue(net2SawMbfPerHectare, net3SawMbfPerHectare, net4SawMbfPerHectare, harvestAge);
            // float npvSimple = this.TimberValue.GetNetPresentValueRegenerationHarvestScribner(this.ScribnerTotal[periodIndex], harvestAge);
        }
    }
}
