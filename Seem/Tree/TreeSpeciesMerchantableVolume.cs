using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Tree
{
    public class TreeSpeciesMerchantableVolume
    {
        private readonly bool[] isCalculated;

        // net log volumes by grade after defect and breakage reduction in m³/ha.
        public float[] Cubic2Saw { get; private init; }
        public float[] Cubic3Saw { get; private init; }
        public float[] Cubic4Saw { get; private init; }

        // log density by grade after defect and breakage removal, logs/ha
        public float[] Logs2Saw { get; private init; }
        public float[] Logs3Saw { get; private init; }
        public float[] Logs4Saw { get; private init; }

        // net log volumes by grade after defect and breakage reduction in Scribner MBF/ha.
        public float[] Scribner2Saw { get; private init; }
        public float[] Scribner3Saw { get; private init; }
        public float[] Scribner4Saw { get; private init; }
        public FiaCode Species { get; private set; }

        public TreeSpeciesMerchantableVolume(FiaCode species, int planningPeriods)
        {
            this.isCalculated = new bool[planningPeriods];
            this.Cubic2Saw = new float[planningPeriods];
            this.Cubic3Saw = new float[planningPeriods];
            this.Cubic4Saw = new float[planningPeriods];
            this.Logs2Saw = new float[planningPeriods];
            this.Logs3Saw = new float[planningPeriods];
            this.Logs4Saw = new float[planningPeriods];
            this.Scribner2Saw = new float[planningPeriods];
            this.Scribner3Saw = new float[planningPeriods];
            this.Scribner4Saw = new float[planningPeriods];
            this.Species = species;

            Array.Fill(this.isCalculated, false, 0, planningPeriods);
        }

        public TreeSpeciesMerchantableVolume(TreeSpeciesMerchantableVolume other)
            : this(other.Species, other.Cubic2Saw.Length)
        {
            this.CopyFrom(other);
        }

        public void CalculateStandingVolume(Stand stand, int periodIndex, TreeVolume treeVolume)
        {
            Trees treesOfSpecies = stand.TreesBySpecies[this.Species];
            TreeSpeciesMerchantableVolumeForPeriod standingVolume = treeVolume.RegenerationHarvest.GetStandingVolume(treesOfSpecies);

            float volumeMultiplier = TreeSpeciesMerchantableVolume.GetVolumeMultiplier(treesOfSpecies.Units);
            standingVolume.Multiply(volumeMultiplier);
            this.SetVolume(standingVolume, periodIndex);

            Debug.Assert((standingVolume.Cubic2Saw >= 0.0F) && (standingVolume.Cubic3Saw >= 0.0F) && (standingVolume.Cubic4Saw >= 0.0F));
            Debug.Assert((standingVolume.Scribner2Saw >= 0.0F) && (standingVolume.Scribner3Saw >= 0.0F) && (standingVolume.Scribner4Saw >= 0.0F));
        }

        public void CalculateThinningVolume(Stand previousStand, SortedList<FiaCode, TreeSelection> individualTreeSelectionBySpecies, int periodIndex, TreeVolume treeVolume)
        {
            Trees previousTreesOfSpecies = previousStand.TreesBySpecies[this.Species];
            TreeSelection individualTreeSelection = individualTreeSelectionBySpecies[previousTreesOfSpecies.Species];
            TreeSpeciesMerchantableVolumeForPeriod thinningVolume = treeVolume.Thinning.GetHarvestedVolume(previousTreesOfSpecies, individualTreeSelection, periodIndex);

            float volumeMultiplier = TreeSpeciesMerchantableVolume.GetVolumeMultiplier(previousTreesOfSpecies.Units);
            thinningVolume.Multiply(volumeMultiplier);
            this.SetVolume(thinningVolume, periodIndex);

            Debug.Assert((thinningVolume.Cubic2Saw >= 0.0F) && (thinningVolume.Cubic3Saw >= 0.0F) && (thinningVolume.Cubic4Saw >= 0.0F));
            Debug.Assert((thinningVolume.Scribner2Saw >= 0.0F) && (thinningVolume.Scribner3Saw >= 0.0F) && (thinningVolume.Scribner4Saw >= 0.0F));
        }

        public void ClearVolume(int periodIndex)
        {
            this.Cubic2Saw[periodIndex] = 0.0F;
            this.Cubic3Saw[periodIndex] = 0.0F;
            this.Cubic4Saw[periodIndex] = 0.0F;
            this.Logs2Saw[periodIndex] = 0.0F;
            this.Logs3Saw[periodIndex] = 0.0F;
            this.Logs4Saw[periodIndex] = 0.0F;
            this.Scribner2Saw[periodIndex] = 0.0F;
            this.Scribner3Saw[periodIndex] = 0.0F;
            this.Scribner4Saw[periodIndex] = 0.0F;

            this.isCalculated[periodIndex] = true;
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
            Array.Copy(other.Logs2Saw, 0, this.Logs2Saw, 0, minPeriods);
            Array.Copy(other.Logs3Saw, 0, this.Logs3Saw, 0, minPeriods);
            Array.Copy(other.Logs4Saw, 0, this.Logs4Saw, 0, minPeriods);
            Array.Copy(other.Scribner2Saw, 0, this.Scribner2Saw, 0, minPeriods);
            Array.Copy(other.Scribner3Saw, 0, this.Scribner3Saw, 0, minPeriods);
            Array.Copy(other.Scribner4Saw, 0, this.Scribner4Saw, 0, minPeriods);
            if (this.Cubic2Saw.Length > minPeriods)
            {
                Array.Fill(this.Cubic2Saw, Single.NaN, minPeriods, this.Cubic2Saw.Length - minPeriods);
                Array.Fill(this.Cubic3Saw, Single.NaN, minPeriods, this.Cubic3Saw.Length - minPeriods);
                Array.Fill(this.Cubic4Saw, Single.NaN, minPeriods, this.Cubic4Saw.Length - minPeriods);
                Array.Fill(this.Logs2Saw, Single.NaN, minPeriods, this.Logs2Saw.Length - minPeriods);
                Array.Fill(this.Logs3Saw, Single.NaN, minPeriods, this.Logs3Saw.Length - minPeriods);
                Array.Fill(this.Logs4Saw, Single.NaN, minPeriods, this.Logs4Saw.Length - minPeriods);
                Array.Fill(this.Scribner2Saw, Single.NaN, minPeriods, this.Scribner2Saw.Length - minPeriods);
                Array.Fill(this.Scribner3Saw, Single.NaN, minPeriods, this.Scribner3Saw.Length - minPeriods);
                Array.Fill(this.Scribner4Saw, Single.NaN, minPeriods, this.Scribner4Saw.Length - minPeriods);
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

        private static float GetVolumeMultiplier(Units speciesUnits)
        {
            float volumeMultiplier = Constant.Bucking.DefectAndBreakageReduction;
            if (speciesUnits == Units.English)
            {
                volumeMultiplier *= Constant.AcresPerHectare;
            }
            return volumeMultiplier;
        }

        public bool IsCalculated(int periodIndex)
        {
            return this.isCalculated[periodIndex];
        }

        public void MarkUncalculated(int periodIndex)
        {
            this.isCalculated[periodIndex] = false;

            this.Cubic2Saw[periodIndex] = Single.NaN;
            this.Cubic3Saw[periodIndex] = Single.NaN;
            this.Cubic4Saw[periodIndex] = Single.NaN;

            this.Logs2Saw[periodIndex] = Single.NaN;
            this.Logs3Saw[periodIndex] = Single.NaN;
            this.Logs4Saw[periodIndex] = Single.NaN;
            
            this.Scribner2Saw[periodIndex] = Single.NaN;
            this.Scribner3Saw[periodIndex] = Single.NaN;
            this.Scribner4Saw[periodIndex] = Single.NaN;
        }

        private void SetVolume(TreeSpeciesMerchantableVolumeForPeriod volume, int periodIndex)
        {
            this.Cubic2Saw[periodIndex] = volume.Cubic2Saw;
            this.Cubic3Saw[periodIndex] = volume.Cubic3Saw;
            this.Cubic4Saw[periodIndex] = volume.Cubic4Saw;

            this.Logs2Saw[periodIndex] = volume.Logs2Saw;
            this.Logs3Saw[periodIndex] = volume.Logs3Saw;
            this.Logs4Saw[periodIndex] = volume.Logs4Saw;

            this.Scribner2Saw[periodIndex] = volume.Scribner2Saw;
            this.Scribner3Saw[periodIndex] = volume.Scribner3Saw;
            this.Scribner4Saw[periodIndex] = volume.Scribner4Saw;

            this.isCalculated[periodIndex] = true;
        }
    }
}
