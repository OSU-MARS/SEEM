using System;
using System.Diagnostics;

namespace Osu.Cof.Ferm
{
    public class Trees
    {
        /// <summary>
        /// Maximum number of trees which can be stored in this set.
        /// </summary>
        public int Capacity { get; private set; }

        /// <summary>
        /// Number of trees currently in this set.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Crown ratio.
        /// </summary>
        public float[] CrownRatio { get; private set; }

        /// <summary>
        /// DBH in inches or centimeters.
        /// </summary>
        public float[] Dbh { get; private set; }

        /// <summary>
        /// DBH in inches or centimeters at the most recent simulation step. 
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
        public FiaCode Species { get; private init; }

        /// <summary>
        /// Trees' tag numbers, if specified.
        /// </summary>
        public int[] Tag { get; private set; }

        /// <summary>
        /// Original index of tree prior to any harvests. Used for locating retained trees in tree selection vectors.
        /// </summary>
        public int[] UncompactedIndex { get; private set; }

        /// <summary>
        /// Whether diameters, heights, and densities are in English or metric units.
        /// </summary>
        public Units Units { get; private init; }

        public Trees(FiaCode species, int minimumSize, Units units)
        {
            // ensure array lengths are an exact multiple of the SIMD width
            this.Capacity = Trees.GetCapacity(minimumSize);
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
            this.UncompactedIndex = new int[this.Capacity];
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
            other.UncompactedIndex.CopyTo(this.UncompactedIndex, 0);
        }

        public void CopyFrom(Trees other)
        {
            if ((other.Count > this.Capacity) || (other.Species != this.Species))
            {
                throw new ArgumentOutOfRangeException(nameof(other));
            }

            // copy tree data
            this.Count = other.Count;
            Array.Copy(other.CrownRatio, 0, this.CrownRatio, 0, other.Count);
            Array.Copy(other.Dbh, 0, this.Dbh, 0, other.Count);
            Array.Copy(other.DbhGrowth, 0, this.DbhGrowth, 0, other.Count);
            Array.Copy(other.DeadExpansionFactor, 0, this.DeadExpansionFactor, 0, other.Count);
            Array.Copy(other.Height, 0, this.Height, 0, other.Count);
            Array.Copy(other.HeightGrowth, 0, this.HeightGrowth, 0, other.Count);
            Array.Copy(other.LiveExpansionFactor, 0, this.LiveExpansionFactor, 0, other.Count);
            Array.Copy(other.Tag, 0, this.Tag, 0, other.Count);
            Array.Copy(other.UncompactedIndex, 0, this.UncompactedIndex, 0, other.Count);

            // ensure expansion factors are zeroed in any unused capacity
            for (int treeIndex = this.Count; treeIndex < this.Capacity; ++treeIndex)
            {
                this.LiveExpansionFactor[treeIndex] = 0.0F;
            }
        }

        public static NotSupportedException CreateUnhandledSpeciesException(FiaCode species)
        {
            return new NotSupportedException(String.Format("Unhandled species {0}.", species));
        }

        public void Add(int tag, float dbh, float height, float crownRatio, float liveExpansionFactor)
        {
            Debug.Assert(Single.IsNaN(dbh) || ((dbh >= 0.0F) && (dbh < 500.0F)));
            Debug.Assert(Single.IsNaN(height) || ((height >= 0.0F) && (height < 380.0F)));
            Debug.Assert(crownRatio >= 0.0F);
            Debug.Assert(crownRatio <= 1.0F);
            Debug.Assert(liveExpansionFactor >= 0.0F);
            Debug.Assert(liveExpansionFactor <= Constant.Maximum.ExpansionFactor);

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
                this.UncompactedIndex = this.UncompactedIndex.Extend(this.Capacity);
            }

            if (this.UncompactedIndex[this.Count] != 0)
            {
                throw new NotSupportedException("Uncompacted index is nonzero. Is this an attempt to add a tree after trees have been removed?");
            }

            this.Tag[this.Count] = tag;
            this.Dbh[this.Count] = dbh;
            this.Height[this.Count] = height;
            this.CrownRatio[this.Count] = crownRatio;
            this.LiveExpansionFactor[this.Count] = liveExpansionFactor;
            this.UncompactedIndex[this.Count] = this.Count; // if needed, support multiple species by including offset due to trees of other species

            ++this.Count;
        }

        public float GetBasalArea(int compactedTreeIndex)
        {
            // TODO: support metric
            Debug.Assert(this.Units == Units.English);

            float dbhInInches = this.Dbh[compactedTreeIndex];
            float liveExpansionFactor = this.LiveExpansionFactor[compactedTreeIndex];
            return Constant.ForestersEnglish * dbhInInches * dbhInInches * liveExpansionFactor;
        }

        public static int GetCapacity(int treeCount)
        {
            return Constant.Simd128x4.Width * (int)MathF.Ceiling((float)treeCount / (float)Constant.Simd128x4.Width);
        }

        public int[] GetDbhSortOrder()
        {
            int[] dbhSortIndices = new int[this.Count];
            for (int treeIndex = 0; treeIndex < this.Count; ++treeIndex)
            {
                dbhSortIndices[treeIndex] = treeIndex;
            }
            float[] dbhCloneWhichBecomesSorted = new float[this.Count];
            Array.Copy(this.Dbh, 0, dbhCloneWhichBecomesSorted, 0, this.Count);
            Array.Sort(dbhCloneWhichBecomesSorted, dbhSortIndices);
            return dbhSortIndices;
        }

        public int[] GetHeightSortOrder()
        {
            int[] heightSortIndices = new int[this.Count];
            for (int treeIndex = 0; treeIndex < this.Count; ++treeIndex)
            {
                heightSortIndices[treeIndex] = treeIndex;
            }
            float[] heightCloneWhichBecomesSorted = new float[this.Count];
            Array.Copy(this.Height, 0, heightCloneWhichBecomesSorted, 0, this.Count);
            Array.Sort(heightCloneWhichBecomesSorted, heightSortIndices);
            return heightSortIndices;
        }

        public void RemoveZeroExpansionFactorTrees()
        {
            // for now, assume snags are indicated by dead expansion factor can be dropped
            int moreCompactedTreeIndex = 0;
            for (int lessCompactedTreeIndex = 0; lessCompactedTreeIndex < this.Count; ++lessCompactedTreeIndex)
            {
                float sourceExpansionFactor = this.LiveExpansionFactor[lessCompactedTreeIndex];
                if (sourceExpansionFactor > 0.0F)
                {
                    // if the source and destinations aren't matched, compact this tree in the array
                    if (moreCompactedTreeIndex != lessCompactedTreeIndex)
                    {
                        Debug.Assert(moreCompactedTreeIndex < lessCompactedTreeIndex);
                        this.CrownRatio[moreCompactedTreeIndex] = this.CrownRatio[lessCompactedTreeIndex];
                        this.Dbh[moreCompactedTreeIndex] = this.Dbh[lessCompactedTreeIndex];
                        this.DbhGrowth[moreCompactedTreeIndex] = this.DbhGrowth[lessCompactedTreeIndex];
                        this.DeadExpansionFactor[moreCompactedTreeIndex] = this.DeadExpansionFactor[lessCompactedTreeIndex];
                        this.Height[moreCompactedTreeIndex] = this.Height[lessCompactedTreeIndex];
                        this.HeightGrowth[moreCompactedTreeIndex] = this.HeightGrowth[lessCompactedTreeIndex];
                        this.LiveExpansionFactor[moreCompactedTreeIndex] = this.LiveExpansionFactor[lessCompactedTreeIndex];
                        this.Tag[moreCompactedTreeIndex] = this.Tag[lessCompactedTreeIndex];
                        this.UncompactedIndex[moreCompactedTreeIndex] = this.UncompactedIndex[lessCompactedTreeIndex];
                    }

                    // if the source tree remains present in the stand, advance the destination index
                    // If the source tree is removed, the destination isn't incremented and the source-destination index mismatch will trigger array
                    // compaction for all remaining loop iterations.
                    Debug.Assert(sourceExpansionFactor >= 0.0F);
                    ++moreCompactedTreeIndex;
                }
            }

            // update count for any removed trees
            this.Count = moreCompactedTreeIndex;
            // zero tail of now unused tree records due to compaction
            // This
            // - Clears expansion factors in the last SIMD block processed, resulting in no longer valid records being masked from computations.
            // - May need to include other properties depending on sorting implementations or other tree processing requirements.
            for (int treeIndex = this.Count; treeIndex < this.Capacity; ++treeIndex)
            {
                // this.CrownRatio[treeIndex] = 0.0F;
                // this.Dbh[treeIndex] = 0.0F;
                // this.DbhGrowth[treeIndex] = 0.0F;
                // this.DeadExpansionFactor[treeIndex] = 0.0F;
                // this.Height[treeIndex] = 0.0F;
                // this.HeightGrowth[treeIndex] = 0.0F;
                this.LiveExpansionFactor[treeIndex] = 0.0F;
                // this.Tag[treeIndex] = 0; // potential desirable not to zero as a aid to debugging
            }
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
