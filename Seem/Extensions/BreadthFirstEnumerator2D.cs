using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mars.Seem.Extensions
{
    internal class BreadthFirstEnumerator2D
    {
        public static readonly SearchPattern2D BalancedXY;
        public static readonly SearchPattern2D XFirst;
        public static readonly SearchPattern2D YFirst;

        static BreadthFirstEnumerator2D()
        {
            // balanced search pattern: equal priority to x and y favoring traversal in x and then traversal in y
            // Range is the Euclidean distance in the grid formed by the X and Y arrays, rounded to nearest integer.
            // Generating R code:
            // y = replicate(13, c(6, 5, 4, 3, 2, 1, 0, -1, -2, -3, -4, -5, -6))
            // x = -t(y)
            // range = sqrt(x^2 + y^2)
            // round(ifelse(range > 4, range, NA), 1) # for some set of thresholds excluding ranges not of interest
            //  y
            //  5        5 5 5 5 5
            //  4      5 4 4 4 4 4 5
            //  3    5 4 4 3 3 3 4 4 5
            //  2  5 4 4 3 2 2 2 3 4 4 5
            //  1  5 4 3 2 1 1 1 2 3 4 5
            //  0  5 4 3 2 1 0 1 2 3 4 5
            // -1  5 4 3 2 1 1 1 2 3 4 5
            // -2  5 4 4 3 2 2 2 3 4 4 5
            // -3    5 4 4 3 3 3 4 4 5
            // -4      5 4 4 4 4 4 5
            // -5        5 5 5 5 5
            //  x   -4  -2   0 1 2 3 4 5
            BreadthFirstEnumerator2D.BalancedXY = new()
            {
                MaxRange = 4,
                Offsets = new (int, int)[][]
                { 
                    // 1@0 + 8@1 + 12@2 + 16@3 + 24@4 + 32@5 = enumeration over 93 nearest positions, extend to longer ranges if needed
                    new (int, int)[] { new(0, 0) },
                    new (int, int)[] { new(1, 0), new(-1, 0), new(0, 1), new(0, -1),     // cardinals @ Hamming distance = Euclidean distance = 1
                                       new(1, 1), new(-1, -1), new(1, -1), new(-1, 1) }, // corners @ Hamming = 2, Euclidean = 1.4
                    new (int, int)[] { new(2, 0), new(-2, 0), new(0, 2), new(0, -2),     // cardinals @ Hamming = Euclidean = 2
                                       new(2, 1), new(-2, -1), new(2, -1), new(-2, 1),   // x favoring @ Hamming = 3, Euclidean = 2.2
                                       new(1, 2), new(-1, -2), new(1, -2), new(-1, 2) }, // y favoring @ Hamming = 3, Euclidean = 2.2
                    new (int, int)[] { new(2, 2), new(-2, -2), new(2, -2), new(-2, 2),   // corners @ Hamming = 4, Euclidean = 2.8
                                       new(3, 0), new(-3, 0), new(0, 3), new(0, -3),     // cardinals @ Hamming = 3, Euclidean = 3
                                       new(3, 1), new(-3, -1), new(3, -1), new(-3, 1),   // x favoring @ Hamming = 4, Euclidean = 3.2
                                       new(1, 3), new(-1, -3), new(1, -3), new(-1, 3) }, // y favoring @ Hamming = 4, Euclidean = 3.2
                    // 24@4
                    new (int, int)[] { new(3, 2), new(-3, -2), new(3, -2), new(-3, 2),   // x favoring @ Hamming = 5, Euclidean = 3.6
                                       new(2, 3), new(-2, -3), new(2, -3), new(-2, 3),   // y favoring @ Hamming = 5, Euclidean = 3.6
                                       new(4, 0), new(-4, 0), new(0, 4), new(0, -4),     // cardinals @ Hamming = 4, Euclidean = 4
                                       new(4, 1), new(-4, -1), new(4, -1), new(-4, 1),   // x favoring @ Hamming = 5, Euclidean = 4.1
                                       new(1, 4), new(-1, -4), new(1, -4), new(-1, 4),   // y favoring @ Hamming = 5, Euclidean = 4.1
                                       new(3, 3), new(-3, -3), new(3, -3), new(-3, 3) }, // diagonals @ Hamming = 6, Euclidean = 4.2
                    // 32@5
                    new (int, int)[] { new(4, 2), new(-4, -2), new(4, -2), new(-4, 2),   // x favoring @ Hamming = 6, Euclidean = 4.5
                                       new(2, 4), new(-2, -4), new(2, -4), new(-2, 4),   // y favoring @ Hamming = 6, Euclidean = 4.5
                                       new(4, 3), new(-4, -3), new(4, -3), new(-4, 3),   // near diagonals @ Hamming = 7, Euclidean = 5
                                       new(3, 4), new(-3, -4), new(3, -4), new(-3, 4),   // near diagonals @ Hamming = 7, Euclidean = 5
                                       new(5, 0), new(-5, 0), new(0, 5), new(0, -5),     // cardinals @ Hamming = 5, Euclidean = 5
                                       new(5, 1), new(-5, -1), new(5, -1), new(-5, 1),   // x favoring @ Hamming = 6, Euclidean = 5.1
                                       new(1, 5), new(-1, -5), new(1, -5), new(-1, 5),   // y favoring @ Hamming = 6, Euclidean = 5.1
                                       new(4, 2), new(-5, -2), new(5, -2), new(-5, 2),   // x favoring @ Hamming = 7, Euclidean = 5.4
                                       new(2, 5), new(-2, -5), new(2, -5), new(-2, 5) }, // y favoring @ Hamming = 7, Euclidean = 5.4
                }
            };

            // search in x first, then y
            // Range is the y distance.
            //  y
            //  5  5 5 5 5 5 5 5 5 5 5 5
            //  4  4 4 4 4 4 4 4 4 4 4 4 
            //  3  3 3 3 3 3 3 3 3 3 3 3
            //  2  2 2 2 2 2 2 2 2 2 2 2
            //  1  1 1 1 1 1 1 1 1 1 1 1
            //  0  0 0 0 0 0 0 0 0 0 0 0
            // -1  1 1 1 1 1 1 1 1 1 1 1
            // -2  2 2 2 2 2 2 2 2 2 2 2
            // -3  3 3 3 3 3 3 3 3 3 3 3
            // -4  4 4 4 4 4 4 4 4 4 4 4 
            // -5  5 5 5 5 5 5 5 5 5 5 5
            //  x   -4  -2   0 1 2 3 4 5
            BreadthFirstEnumerator2D.XFirst = new()
            {
                MaxRange = 5,
                Offsets = new (int, int)[][]
                {
                    new(int, int)[] { new(0, 0), new(1, 0), new(-1, 0), new(2, 0), new(-2, 0), new(3, 0), new(-3, 0), new(4, 0), new(-4, 0), 
                                      new(5, 0), new(-5, 0) },
                    new(int, int)[] { new(0, 1), new(0, -1), new(1, 1), new(-1, 1), new(1, -1), new(-1, -1), new(2, 1), new(-2, 1), new(2, -1), new(-2, -1), 
                                      new(3, 1), new(-3, 1), new(3, -1), new(-3, -1), new(4, -1), new(-4, -1), new(4, 1), new(-4, 1), 
                                      new(5, 1), new(-5, 1), new(5, -1), new(-5, -1) },
                    new(int, int)[] { new(0, 2), new(0, -2), new(1, 2), new(-1, 2), new(1, -2), new(-1, -2), new(2, 2), new(-2, 2), new(2, -2), new(-2, -2),
                                      new(3, 2), new(-3, 2), new(3, -2), new(-3, -2), new(4, 2), new(-4, 2), new(4, -2), new(-4, -2),
                                      new(5, 2), new(-5, 2), new(5, -2), new(-5, -2) },
                    new(int, int)[] { new(0, 3), new(0, -3), new(1, 3), new(-1, 3), new(1, -3), new(-1, -3), new(2, 3), new(-2, 3), new(2, -3), new(-2, -3),
                                      new(3, 3), new(-3, 3), new(3, -3), new(-3, -3), new(4, 3), new(-4, 3), new(4, -3), new(-4, -3),
                                      new(5, 3), new(-5, 3), new(5, -3), new(-5, -3) },
                    new(int, int)[] { new(0, 4), new(0, -4), new(1, 4), new(-1, 4), new(1, -4), new(-1, -4), new(2, 4), new(-2, 4), new(2, -4), new(-2, -4),
                                      new(3, 4), new(-3, 4), new(3, -4), new(-3, -4), new(4, 4), new(-4, 4), new(4, -4), new(-4, -4),
                                      new(5, 4), new(-5, 4), new(5, -4), new(-5, -4) },
                    new(int, int)[] { new(0, 5), new(0, -5), new(1, 5), new(-1, 5), new(1, -5), new(-1, -5), new(2, 5), new(-2, 5), new(2, -5), new(-2, -5),
                                      new(3, 5), new(-3, 5), new(3, -5), new(-3, -5), new(4, 5), new(-4, 5), new(4, -5), new(-4, -5),
                                      new(5, 5), new(-5, 5), new(5, -5), new(-5, -5) },
                }
            };

            // search in y first, then x
            // Range is the x distance.
            //  y
            //  5  5 4 3 2 1 0 1 2 3 4 5
            //  4  5 4 3 2 1 0 1 2 3 4 5 
            //  3  5 4 3 2 1 0 1 2 3 4 5
            //  2  5 4 3 2 1 0 1 2 3 4 5
            //  1  5 4 3 2 1 0 1 2 3 4 5
            //  0  5 4 3 2 1 0 1 2 3 4 5
            // -1  5 4 3 2 1 0 1 2 3 4 5
            // -2  5 4 3 2 1 0 1 2 3 4 5
            // -3  5 4 3 2 1 0 1 2 3 4 5
            // -4  5 4 3 2 1 0 1 2 3 4 5 
            // -5  5 4 3 2 1 0 1 2 3 4 5
            //  x   -4  -2   0 1 2 3 4 5
            BreadthFirstEnumerator2D.YFirst = new()
            {
                MaxRange = 5,
                Offsets = new (int, int)[][]
                {
                    new(int, int)[] { new(0, 0), new(0, 1), new(0, -1), new(0, 2), new(0, -2), new(0, 3), new(0, -3), new(0, 4), new(0, -4), new(0, 5), new(0, -5) },
                    new(int, int)[] { new(1, 0), new(-1, 0), new(1, 1), new(1, -1), new(-1, 1), new(-1, -1), new(1, 2), new(1, -2), new(-1, 2), new(-1, -2),
                                      new(1, 3), new(1, -3), new(-1, 3), new(-1, -3), new(1, 4), new(1, -4), new(-1, 4), new(-1, -4),
                                      new(1, 5), new(1, -5), new(-1, 5), new(-1, -5) },
                    new(int, int)[] { new(2, 0), new(-2, 0), new(2, 1), new(2, -1), new(-2, 1), new(-2, -1), new(2, 2), new(2, -2), new(-2, 2), new(-2, -2),
                                      new(2, 3), new(2, -3), new(-2, 3), new(-2, -3), new(2, 4), new(2, -4), new(-2, 4), new(-2, -4),
                                      new(2, 5), new(2, -5), new(-2, 5), new(-2, -5) },
                    new(int, int)[] { new(3, 0), new(-3, 0), new(3, 1), new(3, -1), new(-3, 1), new(-3, -1), new(3, 2), new(3, -2), new(-3, 2), new(-3, -2),
                                      new(3, 3), new(3, -3), new(-3, 3), new(-3, -3), new(3, 4), new(3, -4), new(-3, 4), new(-3, -4),
                                      new(3, 5), new(3, -5), new(-3, 5), new(-3, -5) },
                    new(int, int)[] { new(4, 0), new(-4, 0), new(4, 1), new(4, -1), new(-4, 1), new(-4, -1), new(4, 2), new(4, -2), new(-4, 2), new(-4, -2),
                                      new(4, 3), new(4, -3), new(-4, 3), new(-4, -3), new(4, 4), new(4, -4), new(-4, 4), new(-4, -4),
                                      new(4, 5), new(4, -5), new(-4, 5), new(-4, -5) },
                    new(int, int)[] { new(5, 0), new(-5, 0), new(5, 1), new(5, -1), new(-5, 1), new(-5, -1), new(5, 2), new(5, -2), new(-5, 2), new(-5, -2),
                                      new(5, 3), new(5, -3), new(-5, 3), new(-5, -3), new(5, 4), new(5, -4), new(-5, 4), new(-5, -4),
                                      new(5, 5), new(5, -5), new(-5, 5), new(-5, -5) }
                }
            };
        }

        // if needed, MaxRange can be removed and Offsets flattened to a one dimensional array
        // For now, Offsets is kept two dimensional for readability and debuggability.
        public class SearchPattern2D
        {
            public int MaxRange { get; init; }
            public (int OffsetX, int OffsetY)[][] Offsets { get; init; }

            public SearchPattern2D()
            {
                this.MaxRange = -1;
                this.Offsets = Array.Empty<(int, int)[]>(); // nullablity hack
            }
        }
    }

    internal class BreadthFirstEnumerator2D<T> : BreadthFirstEnumerator2D, IEnumerator<T> where T : class
    {
        private readonly T?[][] arrayX;
        private readonly int originX;
        private readonly int originY;
        private bool isDisposed;
        private readonly int maxRange;
        private readonly SearchPattern2D searchPattern;
        private int range;
        private int rangeIndex;

        public int IndexX { get; private set; }
        public int IndexY { get; private set; }

        public BreadthFirstEnumerator2D(T?[][] array, int originX, int originY)
            : this(array, originX, originY, BreadthFirstEnumerator2D.BalancedXY)
        {
        }

        public BreadthFirstEnumerator2D(T?[][] array, int originX, int originY, SearchPattern2D searchPattern)
        {
            if ((originX < 0) || (originX >= array.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(originX)); // also throws on a zero length array
            }

            int maxOffsetX = Math.Max(originX, array.Length - originX - 1);
            int maxOffsetY = 0;
            for (int indexX = 0; indexX < array.Length; ++indexX)
            {
                T?[]? arrayY = array[indexX];
                if (arrayY != null)
                {
                    int maxOffsetYatX = Math.Max(originY, arrayY.Length - originY - 1);
                    if (maxOffsetYatX > maxOffsetY)
                    {
                        maxOffsetY = maxOffsetYatX;
                    }
                }
            }

            this.arrayX = array;
            this.originX = originX;
            this.originY = originY;
            this.isDisposed = false;
            this.range = 0;
            this.rangeIndex = -1;
            this.searchPattern = searchPattern;

            this.maxRange = Math.Min(this.searchPattern.MaxRange, (int)MathF.Ceiling(MathF.Sqrt(maxOffsetX * maxOffsetX + maxOffsetY * maxOffsetY)));

            this.IndexX = -1;
            this.IndexY = -1;
        }

        public T Current
        {
            get 
            {
                T? element = this.arrayX[this.IndexX][this.IndexY];
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
            (int, int)[] offsetsAtRange = this.searchPattern.Offsets[this.range];
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
            (int offsetX, int offsetY) = offsetsAtRange[this.rangeIndex];
            this.IndexX = this.originX + offsetX;
            if ((this.IndexX >= this.arrayX.Length) || (this.IndexX < 0))
            {
                // if index is beyond the end of the array, check if a negative offset puts the index on the front of the array
                return this.MoveNext();
            }

            // skip null and empty elements in X
            T?[] arrayY = this.arrayX[this.IndexX];
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

            // skip null elements
            T? element = arrayY[this.IndexY];
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
