using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm
{
    public class SpeciesScribnerVolume
    {
        // net log volumes by grade after defect and breakage reduction in Scribner MBF/ha.
        public float[] Scribner2Saw { get; private init; }
        public float[] Scribner3Saw { get; private init; }
        public float[] Scribner4Saw { get; private init; }
        public FiaCode Species { get; private set; }

        public SpeciesScribnerVolume(FiaCode species, int planningPeriods)
        {
            this.Scribner2Saw = new float[planningPeriods];
            this.Scribner3Saw = new float[planningPeriods];
            this.Scribner4Saw = new float[planningPeriods];
            this.Species = species;
        }

        public SpeciesScribnerVolume(SpeciesScribnerVolume other)
            : this(other.Species, other.Scribner2Saw.Length)
        {
            this.CopyFrom(other);
        }

        public void CopyFrom(SpeciesScribnerVolume other)
        {
            if (this.Species != other.Species)
            {
                throw new ArgumentOutOfRangeException(nameof(other), "Attempt to copy volumes of " + other.Species + " to " + this.Species + ".");
            }

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

        public void SetScribnerHarvest(Stand previousStand, SortedList<FiaCode, TreeSelection> individualTreeSelectionBySpecies, int periodIndex, TreeVolume treeVolume)
        {
            double harvested2SawBoardFeetPerAcre = 0.0F;
            double harvested3SawBoardFeetPerAcre = 0.0F;
            double harvested4SawBoardFeetPerAcre = 0.0F;
            foreach (Trees previousTreesOfSpecies in previousStand.TreesBySpecies.Values)
            {
                Debug.Assert(previousTreesOfSpecies.Units == Units.English, "TODO: per hectare.");

                TreeSelection individualTreeSelection = individualTreeSelectionBySpecies[previousTreesOfSpecies.Species];
                treeVolume.Thinning.GetHarvestedScribnerVolume(previousTreesOfSpecies, individualTreeSelection, periodIndex, out double scribner2saw, out double scribner3saw, out double scribner4saw);
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

        public void SetStandingScribnerVolume(Stand stand, int periodIndex, TreeVolume treeVolume)
        {
            Trees treesOfSpecies = stand.TreesBySpecies[this.Species];
            Debug.Assert(treesOfSpecies.Units == Units.English, "TODO: per hectare.");

            treeVolume.RegenerationHarvest.GetStandingScribnerVolume(treesOfSpecies, out float standing2SawBoardFeetPerAcre, out float standing3SawBoardFeetPerAcre, out float standing4SawBoardFeetPerAcre);

            float net2SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * standing2SawBoardFeetPerAcre;
            this.Scribner2Saw[periodIndex] = net2SawMbfPerHectare;
            float net3SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * standing3SawBoardFeetPerAcre;
            this.Scribner3Saw[periodIndex] = net3SawMbfPerHectare;
            float net4SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * standing4SawBoardFeetPerAcre;
            this.Scribner4Saw[periodIndex] = net4SawMbfPerHectare;
            Debug.Assert((net2SawMbfPerHectare >= 0.0F) && (net3SawMbfPerHectare >= 0.0F) && (net4SawMbfPerHectare >= 0.0F));
        }
    }
}
