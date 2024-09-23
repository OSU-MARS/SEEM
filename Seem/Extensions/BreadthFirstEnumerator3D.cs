using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mars.Seem.Extensions
{
    internal class BreadthFirstEnumerator3D
    {
        public static readonly SearchPattern3D BalancedZyx;

        static BreadthFirstEnumerator3D()
        {
            // balanced search pattern: equal priority to x, y, and z favoring z first traversal, then y, then x
            // Three dimensional version of BreadthFirstEnumerator2D.Balanced. See description there.
            // Generating R code is a small extension of the two dimensional case to look at slices in z:
            // y = replicate(13, c(6, 5, 4, 3, 2, 1, 0, -1, -2, -3, -4, -5, -6))
            // x = -t(y)
            // z = 0, 1, 2, ... # distances are symmetric about z = 0
            // range = sqrt(x^2 + y^2 + z^2)
            // round(ifelse(range > 2, range, NA), 1)
            BreadthFirstEnumerator3D.BalancedZyx = new()
            {
                MaxRange = 3,
                Offsets = [ 
                              // 1@0 + 18@1 + 62@2 + 82@3 = checking of 163 nearest neighbors
                              [ new(0, 0, 0) ],
                              [ new(0, 0, 1), new(0, 0, -1), new(0, 1, 0), new(0, -1, 0), new(1, 0, 0), new(-1, 0, 0), // cardinals @ Hamming distance = Euclidean distance = 1
                                new(0, 1, 1), new(0, 1, -1), new(0, -1, 1), new(0, -1, -1),                            // ordinals x, y, z @ Hamming = 2, Euclidean = 1.4
                                new(1, 0, 1), new(1, 0, -1), new(-1, 0, 1), new(-1, 0, -1),                            // ordinals x, y, z @ Hamming = 2, Euclidean = 1.4
                                new(1, 1, 0), new(-1, -1, 0), new(1, -1, 0), new(-1, 1, 0) ],                          // corners in x, y, z = 0 plane @ Hamming = 2, Euclidean = 1.4
                              [ new(1, 1, 1), new(-1, -1, 1), new(1, -1, 1), new(-1, 1, 1),                            // corners in x, y, z = 1 plane @ Hamming = 3, Euclidean = 1.7
                                new(1, 1, -1), new(-1, -1, -1), new(1, -1, -1), new(-1, 1, -1),                        // corners in x, y, z = -1 plane @ Hamming = 3, Euclidean = 1.7
                                new(0, 0, 2), new(0, 0, -2), new(0, 2, 0), new(0, -2, 0), new(2, 0, 0), new(-2, 0, 0), // cardinals @ Hamming distance = Euclidean distance = 2
                                new(0, 2, 1), new(0, -2, -1), new(0, -2, 1), new(0, 2, -1),                            // ordinals in x, y, z plane @ Hamming = 3, Euclidean = 2.2
                                new(0, 2, 1), new(0, -2, -1), new(0, -2, 1), new(0, 2, -1),                            // ordinals in x, y, z plane @ Hamming = 3, Euclidean = 2.2
                                new(0, 1, 2), new(0, 1, -2), new(0, -1, 2), new(0, -1, -2),                            // ordinals in x, y, z plane @ Hamming = 3, Euclidean = 2.2
                                new(1, 0, 2), new(1, 0, -2), new(-1, 0, 2), new(-1, 0, -2),                            // ordinals in x, y, z plane @ Hamming = 3, Euclidean = 2.2
                                new(1, 2, 0), new(-1, -2, 0), new(1, -2, 0), new(-1, 2, 0),                            // y favoring in x, y, z = 0 plane @ Hamming = 3, Euclidean = 2.2
                                new(2, 1, 0), new(-2, -1, 0), new(2, -1, 0), new(-2, 1, 0),                            // x favoring in x, y, z = 0 plane @ Hamming = 3, Euclidean = 2.2
                                new(1, 2, 1), new(-1, -2, 1), new(1, -2, 1), new(-1, 2, 1),                            // y favoring in x, y, z = 1 plane @ Hamming = 4, Euclidean = 2.4
                                new(1, 1, 2), new(1, 1, -2), new(1, -1, 2), new(1, -1, -2),                            // xy corners in x, y, z plane @ Hamming = 4, Euclidean = 2.4
                                new(-1, 1, 2), new(-1, 1, -2), new(-1, -1, 2), new(-1, -1, -2),                        // xy corners in x, y, z plane @ Hamming = 4, Euclidean = 2.4
                                new(1, 2, -1), new(-1, -2, -1), new(1, -2, 1), new(-1, 2, -1),                         // y favoring in x, y, z = -1 plane @ Hamming = 4, Euclidean = 2.4
                                new(2, 1, 1), new(-2, -1, 1), new(2, -1, 1), new(-2, 1, 1),                            // x favoring in x, y, z = 1 plane @ Hamming = 4, Euclidean = 2.4
                                new(2, 1, -1), new(-2, -1, -1), new(2, -1, -1), new(-2, 1, -1) ],                      // x favoring in x, y, z = -1 plane @ Hamming = 4, Euclidean = 2.4
                              [ new(0, 2, 2), new(0, 2, -2), new(0, -2, 2), new(0, -2, -2),                            // diagonals @ Hamming = 4, Euclidean = 2.8
                                new(2, 0, 2), new(2, 0, -2), new(-2, 0, 2), new(-2, 0, -2),                            // diagonals @ Hamming = 4, Euclidean = 2.8
                                new(2, 2, 0), new(-2, -2, 0), new(2, -2, 0), new(-2, 2, 0),                            // diagonals @ Hamming = 4, Euclidean = 2.8
                                new(1, 2, 2), new(1, 2, -2), new(1, -2, 2), new(1, 2, -2),                             // y favoring in x, y, z plane @ Hamming = 5, Euclidean = 3
                                new(-1, 2, 2), new(-1, 2, -2), new(-1, -2, 2), new(-1, 2, -2),                         // y favoring in x, y, z plane @ Hamming = 5, Euclidean = 3
                                new(2, 1, 2), new(2, 1, -2), new(2, -1, 2), new(2, 1, -2),                             // x favoring in x, y, z plane @ Hamming = 5, Euclidean = 3
                                new(-2, 1, 2), new(-2, 1, -2), new(-2, -1, 2), new(-2, 1, -2),                         // x favoring in x, y, z plane @ Hamming = 5, Euclidean = 3
                                new(2, 2, 1), new(2, 2, -1), new(2, -2, 1), new(2, 2, -1),                             // xy corners in x, y, z plane @ Hamming = 5, Euclidean = 3
                                new(-2, 2, 1), new(-2, 2, -1), new(-2, -2, 1), new(-2, 2, -1),                         // xy corners in x, y, z plane @ Hamming = 5, Euclidean = 3
                                new(0, 0, 3), new(0, 0, -3), new(0, 3, 0), new(0, -3, 0), new(3, 0, 0), new(-3, 0, 0), // cardinals @ Hamming distance = Euclidean distance = 3
                                new(0, 1, 3), new(0, 1, -3), new(0, -1, 3), new(0, -1, -3),                            // y favoring in x, y, z plane @ Hamming = 4, Euclidean = 3.2
                                new(1, 0, 3), new(1, 0, -3), new(-1, 0, 3), new(-1, 0, -3),                            // x favoring in x, y, z plane @ Hamming = 4, Euclidean = 3.2
                                new(0, 3, 1), new(0, -3, -1), new(0, -3, 1), new(0, 3, -1),                            // ordinals in x, y, z plane @ Hamming = 4, Euclidean = 3.2
                                new(3, 0, 1), new(-3, 0, -1), new(3, 0, -1), new(3, 0, -1),                            // ordinals in x, y, z plane @ Hamming = 4, Euclidean = 3.2
                                new(1, 3, 0), new(-1, -3, 0), new(1, -3, 0), new(-1, 3, 0),                            // y favoring in x, y, z = 0 plane @ Hamming = 4, Euclidean = 3.2
                                new(3, 1, 0), new(-3, -1, 0), new(3, -1, 0), new(-3, 1, 0),                            // x favoring in x, y, z = 0 plane @ Hamming = 4, Euclidean = 3.2
                                new(1, 3, 1), new(-1, -3, 1), new(1, -3, 1), new(-1, 3, 1),                            // y favoring in x, y, z = 0 plane @ Hamming = 5, Euclidean = 3.3
                                new(1, 3, -1), new(-1, -3, -1), new(1, -3, -1), new(-1, 3, -1),                        // y favoring in x, y, z = 0 plane @ Hamming = 5, Euclidean = 3.3
                                new(3, 1, 1), new(-3, -1, 1), new(3, -1, 1), new(-3, 1, 1),                            // x favoring in x, y, z = 0 plane @ Hamming = 5, Euclidean = 3.3
                                new(3, 1, -1), new(-3, -1, -1), new(3, -1, -1), new(-3, 1, -1) ],                      // x favoring in x, y, z = 0 plane @ Hamming = 5, Euclidean = 3.3
                          ]
            };
        }

        // if needed, MaxRange can be removed and Offsets flattened to a one dimensional array
        // For now, Offsets is kept three dimensional for readability and debuggability.
        public class SearchPattern3D
        {
            public int MaxRange { get; init; }
            public (int OffsetX, int OffsetY, int OffsetZ)[][] Offsets { get; init; }

            public SearchPattern3D()
            {
                this.MaxRange = -1;
                this.Offsets = [];
            }
        }
    }

    internal class BreadthFirstEnumerator3D<T> : BreadthFirstEnumerator3D, IEnumerator<T> where T : class
    {
        private readonly T?[][][] arrayX;
        private readonly int originX;
        private readonly int originY;
        private readonly int originZ;
        private bool isDisposed;
        private readonly int maxRange;
        private readonly SearchPattern3D searchPattern;
        private int range;
        private int rangeIndex;

        public int IndexX { get; private set; }
        public int IndexY { get; private set; }
        public int IndexZ { get; private set; }

        public BreadthFirstEnumerator3D(T?[][][] array, int originX, int originY, int originZ)
            : this(array, originX, originY, originZ, BreadthFirstEnumerator3D.BalancedZyx)
        {
        }

        public BreadthFirstEnumerator3D(T?[][][] array, int originX, int originY, int originZ, SearchPattern3D searchPattern)
        {
            if ((originX < 0) || (originX >= array.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(originX)); // also throws on a zero length array
            }

            int maxOffsetX = Math.Max(originX, array.Length - originX - 1);
            int maxOffsetY = 0;
            int maxOffsetZ = 0;
            for (int indexX = 0; indexX < array.Length; ++indexX)
            {
                T?[][]? arrayY = array[indexX];
                if (arrayY != null)
                {
                    int maxOffsetYatX = Math.Max(originY, arrayY.Length - originY - 1);
                    if (maxOffsetYatX > maxOffsetY)
                    {
                        maxOffsetY = maxOffsetYatX;
                    }

                    for (int indexY = 0; indexY < arrayY.Length; ++indexY)
                    {
                        T?[]? arrayZ = arrayY[indexY];
                        if (arrayZ != null)
                        {
                            int maxOffsetZatY = Math.Max(originZ, arrayZ.Length - originZ - 1);
                            if (maxOffsetZatY > maxOffsetZ)
                            {
                                maxOffsetZ = maxOffsetZatY;
                            }
                        }
                    }
                }
            }

            this.arrayX = array;
            this.originX = originX;
            this.originY = originY;
            this.originZ = originZ;
            this.isDisposed = false;
            this.range = 0;
            this.rangeIndex = -1;
            this.searchPattern = searchPattern;

            this.maxRange = Math.Min(this.searchPattern.MaxRange, (int)MathF.Ceiling(MathF.Sqrt(maxOffsetX * maxOffsetX + maxOffsetY * maxOffsetY + maxOffsetZ * maxOffsetZ)));

            this.IndexX = -1;
            this.IndexY = -1;
            this.IndexZ = -1;
        }

        public T Current
        {
            get 
            {
                T? element = this.arrayX[this.IndexX][this.IndexY][this.IndexZ];
                Debug.Assert(element != null);
                return element!;
            }
        }

        object IEnumerator.Current
        {
            get { return this.Current; }
        }

        void IDisposable.Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                // IEnumerable<T> requires Dispose() but, for now, nothing to do
                // if (disposing)
                // {
                // }
                this.isDisposed = true;
            }
        }

        public bool MoveNext()
        {
            if (this.range > this.maxRange)
            {
                // beyond maximum iteration
                return false;
            }

            // startup logic
            (int, int, int)[] offsetsAtRange = this.searchPattern.Offsets[this.range];
            if (++this.rangeIndex >= offsetsAtRange.Length)
            {
                ++this.range;
                if (this.range > this.maxRange)
                {
                    return false;
                }

                offsetsAtRange = this.searchPattern.Offsets[this.range];
                this.rangeIndex = 0;
            }

            // set indexX and check bounds in X
            (int offsetX, int offsetY, int offsetZ) = offsetsAtRange[this.rangeIndex];
            this.IndexX = this.originX + offsetX;
            if ((this.IndexX >= this.arrayX.Length) || (this.IndexX < 0))
            {
                // if index is beyond the end of the array, check if a negative offset puts the index on the front of the array
                return this.MoveNext();
            }

            // skip null and empty elements in X
            T?[][] arrayY = this.arrayX[this.IndexX];
            if ((arrayY == null) || (arrayY.Length < 1))
            {
                return this.MoveNext();
            }

            // set indexY and check bounds in Y
            this.IndexY = this.originY + offsetY;
            if ((this.IndexY >= arrayY.Length) || (this.IndexY < 0))
            {
                return this.MoveNext();
            }

            // skip null and empty elements in Y
            T?[] arrayZ = arrayY[this.IndexY];
            if ((arrayZ == null) || (arrayZ.Length < 1))
            {
                return this.MoveNext();
            }

            // set indexZ and check bounds in Z
            this.IndexZ = this.originZ + offsetZ;
            if ((this.IndexZ >= arrayZ.Length) || (this.IndexZ < 0))
            {
                return this.MoveNext();
            }

            // skip null elements in Z
            T? element = arrayZ[this.IndexZ];
            if (element == null)
            {
                return this.MoveNext();
            }

            return true;
        }

        void IEnumerator.Reset()
        {
            this.range = 0;
        }
    }
}
