using System;
using System.Security.Cryptography;

namespace Osu.Cof.Ferm.Heuristics
{
    public class Pseudorandom
    {
        private static readonly RandomNumberGenerator CryptographicRandom;

        private readonly Random pseudorandom;

        static Pseudorandom()
        {
            Pseudorandom.CryptographicRandom = RandomNumberGenerator.Create();
        }

        public Pseudorandom()
        {
            // special seeding of Random is not required but still desirable to avoid correlation between heuristics running on parallel threads
            // .NET Core implements Knuth subtractive PRNG, seeding user created randoms from thread randoms seeded from a global random. However, the
            // global random obtains its seed from a call to Interop.GetRandomBytes() and this API does not appear to be documented.
            // https://github.com/dotnet/runtime/blob/master/src/libraries/System.Private.CoreLib/src/System/Random.cs
            // https://github.com/dotnet/coreclr/pull/2192
            byte[] seed = new byte[4];
            lock (Pseudorandom.CryptographicRandom)
            {
                Pseudorandom.CryptographicRandom.GetBytes(seed);
            }
            this.pseudorandom = new Random(BitConverter.ToInt32(seed, 0));
        }

        public int Next(int exclusiveUpperBound)
        {
            return this.pseudorandom.Next(exclusiveUpperBound);
        }

        public void NextBytes(byte[] bytes)
        {
            this.pseudorandom.NextBytes(bytes);
        }

        public void Shuffle(int[] array)
        {
            // Fisher-Yates shuffle to randomize order of vector in place
            for (int destinationIndex = array.Length; destinationIndex > 1; /* decrement in body */)
            {
                int sourceIndex = this.pseudorandom.Next(destinationIndex--);
                int buffer = array[destinationIndex];
                array[destinationIndex] = array[sourceIndex];
                array[sourceIndex] = buffer;
            }
        }
    }
}
