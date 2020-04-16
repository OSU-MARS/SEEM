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

        public static T[] Extend<T>(this T[] array, int newLength)
        {
            T[] longerArray = new T[newLength];
            array.CopyTo(longerArray, 0);
            return longerArray;
        }
    }
}
