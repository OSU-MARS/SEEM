using System.Collections.Generic;

namespace Osu.Cof.Ferm.Extensions
{
    public static class ListExtensions
    {
        public static void CopyFrom<T>(this List<T> to, List<T> from)
        {
            int maxElementToElementCopyIndex = from.Count < to.Count ? from.Count : to.Count;
            for (int index = 0; index < maxElementToElementCopyIndex; ++index)
            {
                to[index] = from[index];
            }
            if (to.Count > from.Count)
            {
                to.RemoveRange(from.Count, to.Count - from.Count);
            }
            else if (from.Count > to.Count)
            {
                for (int index = to.Count; index < from.Count; ++index)
                {
                    to.Add(from[index]);
                }
            }
        }

        public static void Extend<T>(this List<T?> list, int lastIndexDesired)
        {
            for (int elementsToAdd = lastIndexDesired - list.Count; elementsToAdd >= 0; --elementsToAdd)
            {
                list.Add(default);
            }
        }
    }
}
