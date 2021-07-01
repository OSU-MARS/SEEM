using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm
{
    public class StandScribnerVolume
    {
        // net log volumes by grade after defect and breakage reduction in Scribner MBF/ha.
        public float[] Scribner2Saw { get; private init; }
        public float[] Scribner3Saw { get; private init; }
        public float[] Scribner4Saw { get; private init; }

        public StandScribnerVolume(int planningPeriods)
        {
            this.Scribner2Saw = new float[planningPeriods];
            this.Scribner3Saw = new float[planningPeriods];
            this.Scribner4Saw = new float[planningPeriods];
        }

        public StandScribnerVolume(StandScribnerVolume other)
            : this(other.Scribner2Saw.Length)
        {
            this.CopyFrom(other);
        }

        public void CopyFrom(StandScribnerVolume other)
        {
            int minPeriods = Math.Min(this.Scribner2Saw.Length, other.Scribner2Saw.Length);
            Array.Copy(other.Scribner2Saw, 0, this.Scribner2Saw, 0, minPeriods);
            Array.Copy(other.Scribner3Saw, 0, this.Scribner3Saw, 0, minPeriods);
            Array.Copy(other.Scribner4Saw, 0, this.Scribner4Saw, 0, minPeriods);
            if (this.Scribner2Saw.Length > minPeriods)
            {
                Array.Clear(this.Scribner2Saw, minPeriods, this.Scribner2Saw.Length - minPeriods);
                Array.Clear(this.Scribner3Saw, minPeriods, this.Scribner3Saw.Length - minPeriods);
                Array.Clear(this.Scribner4Saw, minPeriods, this.Scribner4Saw.Length - minPeriods);
            }
        }

        public float GetScribnerTotal(int periodIndex)
        {
            return this.Scribner2Saw[periodIndex] + this.Scribner3Saw[periodIndex] + this.Scribner4Saw[periodIndex];
        }

        public void SetScribnerHarvest(Stand previousStand, SortedDictionary<FiaCode, TreeSelection> individualTreeSelectionBySpecies, int periodIndex, TimberValue timberValue)
        {
            double harvested2SawBoardFeetPerAcre = 0.0F;
            double harvested3SawBoardFeetPerAcre = 0.0F;
            double harvested4SawBoardFeetPerAcre = 0.0F;
            foreach (Trees previousTreesOfSpecies in previousStand.TreesBySpecies.Values)
            {
                Debug.Assert(previousTreesOfSpecies.Units == Units.English, "TODO: per hectare.");

                TreeSelection individualTreeSelection = individualTreeSelectionBySpecies[previousTreesOfSpecies.Species];
                timberValue.ScaledVolumeThinning.GetHarvestedScribnerVolume(previousTreesOfSpecies, individualTreeSelection, periodIndex, out double scribner2saw, out double scribner3saw, out double scribner4saw);
                harvested2SawBoardFeetPerAcre += scribner2saw;
                harvested3SawBoardFeetPerAcre += scribner3saw;
                harvested4SawBoardFeetPerAcre += scribner4saw;
            }

            float net2SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)harvested2SawBoardFeetPerAcre;
            this.Scribner2Saw[periodIndex] = net2SawMbfPerHectare;
            float net3SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)harvested3SawBoardFeetPerAcre;
            this.Scribner3Saw[periodIndex] = net3SawMbfPerHectare;
            float net4SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)harvested4SawBoardFeetPerAcre;
            this.Scribner4Saw[periodIndex] = net4SawMbfPerHectare;
        }

        public void SetStandingScribnerVolume(Stand stand, int periodIndex, TimberValue timberValue)
        {
            double standing2SawBoardFeetPerAcre = 0.0;
            double standing3SawBoardFeetPerAcre = 0.0;
            double standing4SawBoardFeetPerAcre = 0.0;
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                Debug.Assert(treesOfSpecies.Units == Units.English, "TODO: per hectare.");

                timberValue.ScaledVolumeRegenerationHarvest.GetStandingScribnerVolume(treesOfSpecies, out double scribner2saw, out double scribner3saw, out double scribner4saw);
                standing2SawBoardFeetPerAcre += scribner2saw;
                standing3SawBoardFeetPerAcre += scribner3saw;
                standing4SawBoardFeetPerAcre += scribner4saw;
            }

            float net2SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)standing2SawBoardFeetPerAcre;
            this.Scribner2Saw[periodIndex] = net2SawMbfPerHectare;
            float net3SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)standing3SawBoardFeetPerAcre;
            this.Scribner3Saw[periodIndex] = net3SawMbfPerHectare;
            float net4SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * (float)standing4SawBoardFeetPerAcre;
            this.Scribner4Saw[periodIndex] = net4SawMbfPerHectare;
            Debug.Assert((net2SawMbfPerHectare >= 0.0F) && (net3SawMbfPerHectare >= 0.0F) && (net4SawMbfPerHectare >= 0.0F));
        }
    }
}
