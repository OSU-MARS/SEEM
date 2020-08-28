using System;

namespace Osu.Cof.Ferm
{
    public class StandVolume
    {
        /// <summary>
        /// Merchantable cubic volume in m³/ha.
        /// </summary>
        public float[] Cubic { get; private set; }
        /// <summary>
        /// Net value in US$/ha. Not discounted.
        /// </summary>
        public float[] NetPresentValue { get; private set; }
        /// <summary>
        /// Scribner volume in MBF/ha.
        /// </summary>
        public float[] Scribner { get; private set; }

        public StandVolume(int planningPeriods)
        {
            this.Cubic = new float[planningPeriods];
            this.NetPresentValue = new float[planningPeriods];
            this.Scribner = new float[planningPeriods];
        }

        public StandVolume(StandVolume other)
            : this(other.Cubic.Length)
        {
            this.CopyFrom(other);
        }

        public void CopyFrom(StandVolume other)
        {
            Array.Copy(other.Cubic, 0, this.Cubic, 0, other.Cubic.Length);
            Array.Copy(other.NetPresentValue, 0, this.NetPresentValue, 0, other.NetPresentValue.Length);
            Array.Copy(other.Scribner, 0, this.Scribner, 0, other.Scribner.Length);
        }
    }
}
