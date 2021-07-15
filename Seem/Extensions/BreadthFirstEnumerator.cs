using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Extensions
{
    // doesn't use offset maps like the higher dimensional BreadthFirstEnumerators, so not range limited
    internal class BreadthFirstEnumerator<T> : IEnumerator<T> where T : class
    {
        private readonly T?[] array;
        private bool isDisposed;
        private readonly int maxOffset;
        private int offset;
        private readonly int origin;
        private bool positiveOffset;

        public int Index { get; private set; }

        public BreadthFirstEnumerator(T?[] array, int origin)
        {
            if ((origin < 0) || (origin >= array.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(origin)); // also throws on a zero length array
            }

            this.array = array;
            this.isDisposed = false;
            this.maxOffset = Math.Max(origin, array.Length - origin - 1); ;
            this.offset = -1;
            this.origin = origin;
            this.positiveOffset = false;

            this.Index = -1;
        }

        public T Current
        {
            get 
            {
                T? element = this.array[this.Index];
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
            // beyond maximum iteration
            if (this.offset > this.maxOffset)
            {
                return false;
            }

            // startup logic
            // call    offset  positiveOffset  index
            // init    -1      false           -1
            // 1        0      true             centerIndex
            // 2        1      true             centerIndex + 1       special case: skip over negative zero
            // 3        1      false            centerIndex - 1
            // 4        2      true             centerIndex + 2
            // ...
            if (this.offset == 0)
            {
                this.offset = 1;
                this.positiveOffset = false;
            }
            else if (this.positiveOffset == false)
            {
                ++this.offset;
            }
            this.positiveOffset = !this.positiveOffset;

            // set index and check bounds
            if (this.positiveOffset)
            {
                this.Index = this.origin + this.offset;
                if (this.Index >= this.array.Length)
                {
                    // if index is beyond the end of the array, check if a negative offset puts the index on the front of the array
                    return this.MoveNext();
                }
            }
            else
            {
                this.Index = this.origin - this.offset;
                if (this.Index < 0)
                {
                    // if index is before the start of array, check if a positive offset puts the index on the front of the array
                    return this.MoveNext();
                }
            }

            // skip null elements
            T? element = this.array[this.Index];
            if (element == null)
            {
                return this.MoveNext();
            }

            return true;
        }

        void IEnumerator.Reset()
        {
            this.offset = 0;
        }
    }
}
