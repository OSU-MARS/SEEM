using System;
using System.Collections.Generic;
using System.Linq;

namespace Osu.Cof.Ferm
{
    internal static class SortedDictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this SortedDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            if (dictionary.TryGetValue(key, out TValue value) == false)
            {
                value = new TValue();
                dictionary.Add(key, value);
            }
            return value;
        }

        public static TArray[] GetOrAdd<TKey, TArray>(this SortedDictionary<TKey, TArray[]> dictionary, TKey key, int capacity)
        {
            if (dictionary.TryGetValue(key, out TArray[] array) == false)
            {
                array = new TArray[capacity];
                dictionary.Add(key, array);
            }
            return array;
        }

        public static bool KeysIdentical<TKey, TValue1, TValue2>(SortedDictionary<TKey, TValue1> dictionary1, SortedDictionary<TKey, TValue2> dictionary2)
        {
            if (Object.ReferenceEquals(dictionary1, dictionary2))
            {
                return true;
            }
            if ((dictionary1 == null) || (dictionary2 == null) || (dictionary1.Count != dictionary2.Count))
            {
                return false;
            }
            return !dictionary1.Keys.Except(dictionary2.Keys).Any();
        }

        public static bool ValueLengthsIdentical<TKey, TArray1, TArray2>(SortedDictionary<TKey, TArray1[]> dictionary1, SortedDictionary<TKey, TArray2[]> dictionary2)
        {
            if (Object.ReferenceEquals(dictionary1, dictionary2))
            {
                return true;
            }
            if (SortedDictionaryExtensions.KeysIdentical(dictionary1, dictionary2) == false)
            {
                return false;
            }

            foreach (KeyValuePair<TKey, TArray1[]> array1 in dictionary1)
            {
                TArray1[] value1 = array1.Value;
                TArray2[] value2 = dictionary2[array1.Key];
                if (Object.ReferenceEquals(value1, value2))
                {
                    continue;
                }
                if ((value1 == null) || (value2 == null) || (value1.Length != value2.Length))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
