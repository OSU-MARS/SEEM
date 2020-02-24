using System;

namespace Osu.Cof.Organon.Heuristics
{
    public class RandomNumberConsumer
    {
        private readonly Random pseudorandom;
        private readonly byte[] pseudorandomBytes;
        private int pseudorandomByteIndex;

        protected RandomNumberConsumer()
        {
            this.pseudorandom = new Random();
            this.pseudorandomBytes = new byte[1024];
            pseudorandom.NextBytes(pseudorandomBytes);
            this.pseudorandomByteIndex = 0;
        }

        protected float GetPseudorandomByteAsFloat()
        {
            float byteAsFloat = this.pseudorandomBytes[this.pseudorandomByteIndex];
            ++this.pseudorandomByteIndex;

            // if the last available byte was used, ensure more bytes are available
            if (this.pseudorandomByteIndex >= this.pseudorandomBytes.Length)
            {
                this.pseudorandom.NextBytes(this.pseudorandomBytes);
                this.pseudorandomByteIndex = 0;
            }

            return byteAsFloat;
        }

        protected float GetTwoPseudorandomBytesAsFloat()
        {
            // ensure two bytes are available
            if (this.pseudorandomByteIndex > this.pseudorandomBytes.Length - 2)
            {
                this.pseudorandom.NextBytes(this.pseudorandomBytes);
                this.pseudorandomByteIndex = 0;
            }

            // get bytes
            float bytesAsFloat = (float)BitConverter.ToUInt16(this.pseudorandomBytes, this.pseudorandomByteIndex);
            this.pseudorandomByteIndex += 2;

            // if the last available byte was used, ensure more bytes are available
            if (this.pseudorandomByteIndex > this.pseudorandomBytes.Length - 1)
            {
                this.pseudorandom.NextBytes(this.pseudorandomBytes);
                this.pseudorandomByteIndex = 0;
            }

            return bytesAsFloat;
        }
    }
}
