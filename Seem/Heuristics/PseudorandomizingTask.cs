using System;

namespace Osu.Cof.Ferm.Heuristics
{
    public class PseudorandomizingTask
    {
        private readonly byte[] pseudorandomBytes;
        private int pseudorandomByteIndex;

        protected Pseudorandom Pseudorandom { get; set; }

        protected PseudorandomizingTask()
        {
            this.Pseudorandom = new Pseudorandom();
            this.pseudorandomBytes = new byte[4096];
            this.Pseudorandom.NextBytes(this.pseudorandomBytes);
            this.pseudorandomByteIndex = 0;
        }

        protected float GetPseudorandomByteAsFloat()
        {
            float byteAsFloat = this.pseudorandomBytes[this.pseudorandomByteIndex];
            ++this.pseudorandomByteIndex;

            // if the last available byte was used, ensure more bytes are available
            if (this.pseudorandomByteIndex >= this.pseudorandomBytes.Length)
            {
                this.Pseudorandom.NextBytes(this.pseudorandomBytes);
                this.pseudorandomByteIndex = 0;
            }

            return byteAsFloat;
        }

        protected float GetPseudorandomByteAsProbability()
        {
            return this.GetPseudorandomByteAsFloat() / byte.MaxValue;
        }

        // TODO: audit callers for sufficient bit depth as a function of the number of trees being optimized
        protected float GetTwoPseudorandomBytesAsFloat()
        {
            // ensure two bytes are available
            if (this.pseudorandomByteIndex > this.pseudorandomBytes.Length - 2)
            {
                this.Pseudorandom.NextBytes(this.pseudorandomBytes);
                this.pseudorandomByteIndex = 0;
            }

            // get bytes
            float bytesAsFloat = (float)BitConverter.ToUInt16(this.pseudorandomBytes, this.pseudorandomByteIndex);
            this.pseudorandomByteIndex += 2;

            // if the last available byte was used, ensure more bytes are available
            if (this.pseudorandomByteIndex > this.pseudorandomBytes.Length - 1)
            {
                this.Pseudorandom.NextBytes(this.pseudorandomBytes);
                this.pseudorandomByteIndex = 0;
            }

            return bytesAsFloat;
        }
    }
}
