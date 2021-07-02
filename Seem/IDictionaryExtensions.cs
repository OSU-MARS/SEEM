using System;
using System.Collections.Generic;
using System.Linq;

namespace Osu.Cof.Ferm
{
    internal static class IDictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull where TValue : new()
        {
            if (dictionary.TryGetValue(key, out TValue? value) == false)
            {
                value = new TValue();
                dictionary.Add(key, value);
            }
            return value;
        }

        public static TArray[] GetOrAdd<TKey, TArray>(this IDictionary<TKey, TArray[]> dictionary, TKey key, int capacity) where TKey : notnull
        {
            if (dictionary.TryGetValue(key, out TArray[]? array) == false)
            {
                array = new TArray[capacity];
                dictionary.Add(key, array);
            }
            return array;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> createValue) where TKey : notnull
        {
            if (dictionary.TryGetValue(key, out TValue? value) == false)
            {
                value = createValue.Invoke();
                dictionary.Add(key, value);
            }
            return value;
        }

        public static bool KeysIdentical<TKey, TValue1, TValue2>(IDictionary<TKey, TValue1> dictionary1, IDictionary<TKey, TValue2> dictionary2) where TKey : notnull
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

        public static bool ValueLengthsIdentical<TKey, TArray1, TArray2>(IDictionary<TKey, TArray1[]> dictionary1, IDictionary<TKey, TArray2[]> dictionary2) where TKey : notnull
        {
            if (Object.ReferenceEquals(dictionary1, dictionary2))
            {
                return true;
            }
            if (IDictionaryExtensions.KeysIdentical(dictionary1, dictionary2) == false)
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
