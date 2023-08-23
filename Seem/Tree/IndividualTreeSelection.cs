using Mars.Seem.Extensions;
using System;
using System.Diagnostics;

namespace Mars.Seem.Tree
{
    public class IndividualTreeSelection
    {
        private int count;
        private readonly int[] treeSelection; // by compacted tree index

        public IndividualTreeSelection(int capacity)
        {
            this.count = 0;
            this.treeSelection = new int[capacity];
            Debug.Assert(Constant.RegenerationHarvestIfEligible == 0); // or Array.Fill(this.treeSelection, Constant.RegenerationHarvestPeriod);
        }

        public IndividualTreeSelection(IndividualTreeSelection other)
            : this(other.Capacity)
        {
            this.CopyFrom(other);
        }

        // test hook
        public IndividualTreeSelection(int[] treeSelection)
        {
            this.count = treeSelection.Length;
            this.treeSelection = treeSelection;
        }

        public IndividualTreeSelection(Trees treesOfSpecies, float reserveDbhInCm)
            : this(treesOfSpecies.Capacity)
        {
            // for now, if species isn't merchantable mark all trees as non harvestable
            if (Single.IsNaN(reserveDbhInCm))
            {
                Array.Fill(this.treeSelection, Constant.NoHarvestPeriod);
                return;
            }

            // automatically designate reserve trees by DBH
            // TODO: support designation of smaller reserve trees and nonmerchantable (cull or defect) trees
            (float diameterToCentimetersMultiplier, float _, float _) = UnitsExtensions.GetConversionToMetric(treesOfSpecies.Units);
            for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
            {
                float dbhInCm = diameterToCentimetersMultiplier * treesOfSpecies.Dbh[treeIndex];
                if (dbhInCm > reserveDbhInCm)
                {
                    this.treeSelection[treeIndex] = Constant.NoHarvestPeriod;
                }
            }
        }

        public int this[int index]
        {
            get { return this.treeSelection[index]; }
            set { this.treeSelection[index] = value; }
        }

        public int Capacity
        {
            get { return this.treeSelection.Length; }
        }

        public int Count
        {
            get 
            {
                return this.count; 
            }
            set
            {
                if ((value < 0) || (value > this.treeSelection.Length))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                this.count = value;
            }
        }

        /// <summary>
        /// Low level clone of tree selections. Use <see cref="StandTrajectory"/> methods to maintain cache coherency when modifying
        /// a <see cref="StandTrajectory"/>'s tree selections.
        /// </summary>
        public void CopyFrom(IndividualTreeSelection other)
        {
            other.CopyTo(this);
        }

        /// <summary>
        /// Low level clone of tree selections. Use <see cref="StandTrajectory"/> methods to maintain cache coherency when modifying
        /// a <see cref="StandTrajectory"/>'s tree selections.
        /// </summary>
        public void CopyTo(IndividualTreeSelection other)
        {
            if (other.treeSelection.Length != this.treeSelection.Length)
            {
                throw new NotSupportedException("Capacities of individual tree selections do not match.");
            }

            other.Count = this.Count;
            Array.Copy(this.treeSelection, 0, other.treeSelection, 0, other.treeSelection.Length);
        }
    }
}
