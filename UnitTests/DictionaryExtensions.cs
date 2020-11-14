using System.Collections.Generic;

namespace Osu.Cof.Ferm.Test
{
    internal static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull where TValue : new()
        {
            if (dictionary.TryGetValue(key, out TValue value) == false)
            {
                value = new TValue();
                dictionary.Add(key, value);
            }
            return value;
        }

        public static TArray[] GetOrAdd<TKey, TArray>(this Dictionary<TKey, TArray[]> dictionary, TKey key, int capacity) where TKey : notnull
        {
            if (dictionary.TryGetValue(key, out TArray[]? array) == false)
            {
                array = new TArray[capacity];
                dictionary.Add(key, array);
            }
            return array;
        }
    }
}
