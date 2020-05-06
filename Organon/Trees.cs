using System;
using System.Diagnostics;

namespace Osu.Cof.Ferm
{
    public class Trees
    {
        /// <summary>
        /// Maximum number of trees which can be stored in this set.
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Number of trees currently in this set.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Crown ratio.
        /// </summary>
        public float[] CrownRatio { get; private set; }

        /// <summary>
        /// DBH in inches or meters.
        /// </summary>
        public float[] Dbh { get; private set; }

        /// <summary>
        /// DBH in inches or meters at the most recent simulation step. 
        /// </summary>
        public float[] DbhGrowth { get; private set; }

        /// <summary>
        /// Accumulated expansion factor of dead trees from mortality in either TPA (trees per acre) or TPH (trees per hectare).
        /// </summary>
        public float[] DeadExpansionFactor { get; private set; }

        /// <summary>
        /// Height in feet or meters.
        /// </summary>
        public float[] Height { get; private set; }

        /// <summary>
        /// Height growth in feet or meters at the most recent simulation step. 
        /// </summary>
        public float[] HeightGrowth { get; private set; }

        /// <summary>
        /// Expansion factor of live trees from mortality in either TPA (trees per acre) or TPH (trees per hectare).
        /// </summary>
        public float[] LiveExpansionFactor { get; private set; }

        /// <summary>
        /// Species of this set of trees.
        /// </summary>
        public FiaCode Species { get; private set; }

        /// <summary>
        /// Trees' tag numbers, if specified.
        /// </summary>
        public int[] Tag { get; private set; }

        /// <summary>
        /// Whether diameters, heights, and densities are in English or metric units.
        /// </summary>
        public Units Units { get; private set; }

        public Trees(FiaCode species, int minimumSize, Units units)
        {
            // ensure array lengths are an exact multiple of the SIMD width
            this.Capacity = Constant.Simd128x4.Width * (int)MathF.Ceiling((float)minimumSize / (float)Constant.Simd128x4.Width);
            this.Count = 0; // no trees assigned yet
            this.CrownRatio = new float[this.Capacity];
            this.Dbh = new float[this.Capacity];
            this.DbhGrowth = new float[this.Capacity];
            this.DeadExpansionFactor = new float[this.Capacity];
            this.LiveExpansionFactor = new float[this.Capacity];
            this.Height = new float[this.Capacity];
            this.HeightGrowth = new float[this.Capacity];
            this.Species = species;
            this.Tag = new int[this.Capacity];
            this.Units = units;
        }

        public Trees(Trees other)
            : this(other.Species, other.Capacity, other.Units)
        {
            this.Count = other.Count;

            other.CrownRatio.CopyTo(this.CrownRatio, 0);
            other.Dbh.CopyTo(this.Dbh, 0);
            other.DbhGrowth.CopyTo(this.DbhGrowth, 0);
            other.DeadExpansionFactor.CopyTo(this.DeadExpansionFactor, 0);
            other.LiveExpansionFactor.CopyTo(this.LiveExpansionFactor, 0);
            other.Height.CopyTo(this.Height, 0);
            other.HeightGrowth.CopyTo(this.HeightGrowth, 0);
            other.Tag.CopyTo(this.Tag, 0);
        }

        public static NotSupportedException CreateUnhandledSpeciesException(FiaCode species)
        {
            return new NotSupportedException(String.Format("Unhandled species {0}.", species));
        }

        public void Add(int tag, float dbh, float height, float crownRatio, float liveExpansionFactor)
        {
            Debug.Assert(dbh >= 0.0F);
            Debug.Assert(dbh < 500.0F);
            Debug.Assert(height >= 0.0F);
            Debug.Assert(height < 380.0F);
            Debug.Assert(crownRatio >= 0.0F);
            Debug.Assert(crownRatio <= 1.0F);
            Debug.Assert(liveExpansionFactor >= 0.0F);
            Debug.Assert(liveExpansionFactor < 1000.0F);

            if (this.Capacity == this.Count)
            {
                // for now, double in size like List<T>
                this.Capacity *= 2;
                this.CrownRatio = this.CrownRatio.Extend(this.Capacity);
                this.Dbh = this.Dbh.Extend(this.Capacity);
                this.DbhGrowth = this.DbhGrowth.Extend(this.Capacity);
                this.DeadExpansionFactor = this.DeadExpansionFactor.Extend(this.Capacity);
                this.LiveExpansionFactor = this.LiveExpansionFactor.Extend(this.Capacity);
                this.Height = this.Height.Extend(this.Capacity);
                this.HeightGrowth = this.HeightGrowth.Extend(this.Capacity);
                this.Tag = this.Tag.Extend(this.Capacity);
            }

            this.Tag[this.Count] = tag;
            this.Dbh[this.Count] = dbh;
            this.Height[this.Count] = height;
            this.CrownRatio[this.Count] = crownRatio;
            this.LiveExpansionFactor[this.Count] = liveExpansionFactor;

            ++this.Count;
        }

        public float GetBasalArea(int treeIndex)
        {
            // TODO: support metric
            Debug.Assert(this.Units == Units.English);

            float dbhInInches = this.Dbh[treeIndex];
            float liveExpansionFactor = this.LiveExpansionFactor[treeIndex];
            return Constant.ForestersEnglish * dbhInInches * dbhInInches * liveExpansionFactor;
        }

        public int[] GetDbhSortOrder()
        {
            int[] dbhSortIndices = new int[this.Capacity];
            for (int treeIndex = 0; treeIndex < this.Capacity; ++treeIndex)
            {
                dbhSortIndices[treeIndex] = treeIndex;
            }
            float[] dbhCloneWhichBecomesSorted = new float[this.Capacity];
            this.Dbh.CopyTo(dbhCloneWhichBecomesSorted, 0);
            Array.Sort(dbhCloneWhichBecomesSorted, dbhSortIndices);

            int unusedCapacity = this.Capacity - this.Count;
            if (unusedCapacity == 0)
            {
                return dbhSortIndices;
            }

            int[] trimmedSortIndices = new int[this.Count];
            Array.Copy(dbhSortIndices, unusedCapacity, trimmedSortIndices, 0, this.Count - unusedCapacity);
            return trimmedSortIndices;
        }

        public void SortByDbh()
        {
            int[] dbhSortIndices = new int[this.Capacity];
            for (int treeIndex = 0; treeIndex < this.Capacity; ++treeIndex)
            {
                dbhSortIndices[treeIndex] = treeIndex;
            }
            Array.Sort(this.Dbh, dbhSortIndices);

            float[] sortedCrownRatio = new float[this.Capacity];
            float[] sortedDeadExpansionFactor = new float[this.Capacity];
            float[] sortedDbhGrowth = new float[this.Capacity];
            float[] sortedHeight = new float[this.Capacity];
            float[] sortedHeightGrowth = new float[this.Capacity];
            float[] sortedLiveExpansionFactor = new float[this.Capacity];
            int unusedCapacity = this.Capacity - this.Count;
            for (int destinationIndex = 0; destinationIndex < this.Count; ++destinationIndex)
            {
                // any unused trees have diameters of zero and therefore occur first in the sort array; skip over these
                // This will behave improperly if tree records exist on the plot with zero diameters. If necessary, expansion factor can also
                // be checked for zero.
                int sourceIndex = dbhSortIndices[destinationIndex + unusedCapacity];

                sortedCrownRatio[destinationIndex] = this.CrownRatio[sourceIndex];
                sortedDeadExpansionFactor[destinationIndex] = this.DeadExpansionFactor[sourceIndex];
                sortedDbhGrowth[destinationIndex] = this.DbhGrowth[sourceIndex];
                sortedHeight[destinationIndex] = this.Height[sourceIndex];
                sortedHeightGrowth[destinationIndex] = this.HeightGrowth[sourceIndex];
                sortedLiveExpansionFactor[destinationIndex] = this.LiveExpansionFactor[sourceIndex];
            }
        }
    }
}
