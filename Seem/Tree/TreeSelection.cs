using System;

namespace Osu.Cof.Ferm.Tree
{
    public class TreeSelection
    {
        private int count;
        private readonly int[] treeSelection;

        public TreeSelection(int capacity)
        {
            this.count = 0;
            this.treeSelection = new int[capacity];
        }

        public TreeSelection(TreeSelection other)
            : this(other.Capacity)
        {
            this.CopyFrom(other);
        }

        // test hook
        public TreeSelection(int[] treeSelection)
        {
            this.count = treeSelection.Length;
            this.treeSelection = treeSelection;
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
        public void CopyFrom(TreeSelection other)
        {
            other.CopyTo(this);
        }

        /// <summary>
        /// Low level clone of tree selections. Use <see cref="StandTrajectory"/> methods to maintain cache coherency when modifying
        /// a <see cref="StandTrajectory"/>'s tree selections.
        /// </summary>
        public void CopyTo(TreeSelection other)
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
