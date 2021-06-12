using System;

namespace Osu.Cof.Ferm
{
    internal static class ArrayExtensions
    {
        public static void CopyToExact<T>(this T[] source, T[] destination)
        {
            if (source.Length != destination.Length)
            {
                throw new ArgumentException("Source and destination lengths differ.");
            }
            Array.Copy(source, 0, destination, 0, source.Length);
        }

        public static unsafe void Fill(this float[,,] array, float value)
        {
            fixed (float* pinnedArray = array)
            {
                Span<float> span = new(pinnedArray, array.Length);
                span.Fill(value);
            }
        }

        public static unsafe void Fill(this int[,,] array, int value)
        {
            fixed (int* pinnedArray = array)
            {
                Span<int> span = new(pinnedArray, array.Length);
                span.Fill(value);
            }
        }

        public static T[] Extend<T>(this T[] array, int newLength)
        {
            T[] longerArray = new T[newLength];
            array.CopyTo(longerArray, 0);
            return longerArray;
        }
    }
}
