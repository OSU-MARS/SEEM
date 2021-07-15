using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Tree
{
    public class TreeSpeciesMerchantableVolume
    {
        public float[] Cubic2Saw { get; private init; }
        public float[] Cubic3Saw { get; private init; }
        public float[] Cubic4Saw { get; private init; }

        // net log volumes by grade after defect and breakage reduction in Scribner MBF/ha.
        public float[] Scribner2Saw { get; private init; }
        public float[] Scribner3Saw { get; private init; }
        public float[] Scribner4Saw { get; private init; }
        public FiaCode Species { get; private set; }

        public TreeSpeciesMerchantableVolume(FiaCode species, int planningPeriods)
        {
            this.Cubic2Saw = new float[planningPeriods];
            this.Cubic3Saw = new float[planningPeriods];
            this.Cubic4Saw = new float[planningPeriods];
            this.Scribner2Saw = new float[planningPeriods];
            this.Scribner3Saw = new float[planningPeriods];
            this.Scribner4Saw = new float[planningPeriods];
            this.Species = species;
        }

        public TreeSpeciesMerchantableVolume(TreeSpeciesMerchantableVolume other)
            : this(other.Species, other.Cubic2Saw.Length)
        {
            this.CopyFrom(other);
        }

        public void CopyFrom(TreeSpeciesMerchantableVolume other)
        {
            if (this.Species != other.Species)
            {
                throw new ArgumentOutOfRangeException(nameof(other), "Attempt to copy volumes of " + other.Species + " to " + this.Species + ".");
            }

            int minPeriods = Math.Min(this.Cubic2Saw.Length, other.Cubic2Saw.Length);
            Array.Copy(other.Cubic2Saw, 0, this.Cubic2Saw, 0, minPeriods);
            Array.Copy(other.Cubic3Saw, 0, this.Cubic3Saw, 0, minPeriods);
            Array.Copy(other.Cubic4Saw, 0, this.Cubic4Saw, 0, minPeriods);
            Array.Copy(other.Scribner2Saw, 0, this.Scribner2Saw, 0, minPeriods);
            Array.Copy(other.Scribner3Saw, 0, this.Scribner3Saw, 0, minPeriods);
            Array.Copy(other.Scribner4Saw, 0, this.Scribner4Saw, 0, minPeriods);
            if (this.Cubic2Saw.Length > minPeriods)
            {
                Array.Clear(this.Cubic2Saw, minPeriods, this.Cubic2Saw.Length - minPeriods);
                Array.Clear(this.Cubic3Saw, minPeriods, this.Cubic3Saw.Length - minPeriods);
                Array.Clear(this.Cubic4Saw, minPeriods, this.Cubic4Saw.Length - minPeriods);
                Array.Clear(this.Scribner2Saw, minPeriods, this.Scribner2Saw.Length - minPeriods);
                Array.Clear(this.Scribner3Saw, minPeriods, this.Scribner3Saw.Length - minPeriods);
                Array.Clear(this.Scribner4Saw, minPeriods, this.Scribner4Saw.Length - minPeriods);
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

        public void SetHarvestVolume(Stand previousStand, SortedList<FiaCode, TreeSelection> individualTreeSelectionBySpecies, int periodIndex, TreeVolume treeVolume)
        {
            Trees previousTreesOfSpecies = previousStand.TreesBySpecies[this.Species];
            Debug.Assert(previousTreesOfSpecies.Units == Units.English, "TODO: per hectare.");

            TreeSelection individualTreeSelection = individualTreeSelectionBySpecies[previousTreesOfSpecies.Species];
            treeVolume.Thinning.GetHarvestedVolume(previousTreesOfSpecies, individualTreeSelection, periodIndex, out float harvested2SawCubicMetersPerAcre, out float harvested3SawCubicMetersPerAcre, out float harvested4SawCubicMetersPerAcre, out float harvested2SawBoardFeetPerAcre, out float harvested3SawBoardFeetPerAcre, out float harvested4SawBoardFeetPerAcre);

            float net2SawCubicPerHectare = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * harvested2SawCubicMetersPerAcre;
            this.Cubic2Saw[periodIndex] = net2SawCubicPerHectare;
            float net3SawCubicPerHectare = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * harvested3SawCubicMetersPerAcre;
            this.Cubic3Saw[periodIndex] = net3SawCubicPerHectare;
            float net4SawCubicPerHectare = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * harvested4SawCubicMetersPerAcre;
            this.Cubic4Saw[periodIndex] = net4SawCubicPerHectare;

            float net2SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * harvested2SawBoardFeetPerAcre;
            this.Scribner2Saw[periodIndex] = net2SawMbfPerHectare;
            float net3SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * harvested3SawBoardFeetPerAcre;
            this.Scribner3Saw[periodIndex] = net3SawMbfPerHectare;
            float net4SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * harvested4SawBoardFeetPerAcre;
            this.Scribner4Saw[periodIndex] = net4SawMbfPerHectare;

            Debug.Assert((net2SawCubicPerHectare >= 0.0F) && (net3SawCubicPerHectare >= 0.0F) && (net4SawCubicPerHectare >= 0.0F));
            Debug.Assert((net2SawMbfPerHectare >= 0.0F) && (net3SawMbfPerHectare >= 0.0F) && (net4SawMbfPerHectare >= 0.0F));
        }

        public void SetStandingVolume(Stand stand, int periodIndex, TreeVolume treeVolume)
        {
            Trees treesOfSpecies = stand.TreesBySpecies[this.Species];
            Debug.Assert(treesOfSpecies.Units == Units.English, "TODO: per hectare.");

            treeVolume.RegenerationHarvest.GetStandingVolume(treesOfSpecies, out float standing2SawCubicMetersPerAcre, out float standing3SawCubicMetersPerAcre, out float standing4SawCubicMetersPerAcre, out float standing2SawBoardFeetPerAcre, out float standing3SawBoardFeetPerAcre, out float standing4SawBoardFeetPerAcre);
            float net2SawCubicPerHectare = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * standing2SawCubicMetersPerAcre;
            this.Cubic2Saw[periodIndex] = net2SawCubicPerHectare;
            float net3SawCubicPerHectare = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * standing3SawCubicMetersPerAcre;
            this.Cubic3Saw[periodIndex] = net3SawCubicPerHectare;
            float net4SawCubicPerHectare = Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * standing4SawCubicMetersPerAcre;
            this.Cubic4Saw[periodIndex] = net4SawCubicPerHectare;

            float net2SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * standing2SawBoardFeetPerAcre;
            this.Scribner2Saw[periodIndex] = net2SawMbfPerHectare;
            float net3SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * standing3SawBoardFeetPerAcre;
            this.Scribner3Saw[periodIndex] = net3SawMbfPerHectare;
            float net4SawMbfPerHectare = 0.001F * Constant.AcresPerHectare * Constant.Bucking.DefectAndBreakageReduction * standing4SawBoardFeetPerAcre;
            this.Scribner4Saw[periodIndex] = net4SawMbfPerHectare;

            Debug.Assert((net2SawCubicPerHectare >= 0.0F) && (net3SawCubicPerHectare >= 0.0F) && (net4SawCubicPerHectare >= 0.0F));
            Debug.Assert((net2SawMbfPerHectare >= 0.0F) && (net3SawMbfPerHectare >= 0.0F) && (net4SawMbfPerHectare >= 0.0F));
        }
    }
}
