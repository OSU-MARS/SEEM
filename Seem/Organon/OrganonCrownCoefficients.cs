using System;

namespace Mars.Seem.Organon
{
    public class OrganonCrownCoefficients
    {
        // coefficients for crown width
        public float CWb1 { get; init; }
        public float CWb2 { get; init; }
        public float CWb3 { get; init; }
        public float CWMaxHeightDiameterRatio { get; init; }

        // coefficients for height to crown base
        public float HcbB0 { get; init; }
        public float HcbB1 { get; init; }
        public float HcbB2 { get; init; }
        public float HcbB3 { get; init; }
        public float HcbB4 { get; init; }
        public float HcbB5 { get; init; }
        public float HcbB6 { get; init; }
        public float HcbK { get; init; }

        // coefficients for largest crown width
        public float HlcwB1 { get; init; }
        public float HlcwB2 { get; init; }
        public float LcwB0 { get; init; }
        public float LcwB1 { get; init; }
        public float LcwB2 { get; init; }
        public float LcwB3 { get; init; }

        // coefficients for maximum crown width
        public float DbhLimitForMaxCrownWidth { get; init; }
        public float McwB0 { get; init; }
        public float McwB1 { get; init; }
        public float McwB2 { get; init; }
        public float McwK { get; init; }

        // coefficients for maximum height to crown base
        public float MhcbB0 { get; init; }
        public float MhcbB1 { get; init; }
        public float MhcbB2 { get; init; }
        public float MhcbB3 { get; init; }
        public float HeightToCrownBaseRatioLimit { get; init; } // imposes minimum crown length

        public OrganonCrownCoefficients()
        {
            this.CWb1 = 0.0F;
            this.CWb2 = 0.0F;
            this.CWb3 = 0.0F;
            this.CWMaxHeightDiameterRatio = Single.MaxValue;

            this.HcbB0 = 0.0F;
            this.HcbB1 = 0.0F;
            this.HcbB2 = 0.0F;
            this.HcbB3 = 0.0F;
            this.HcbB4 = 0.0F;
            this.HcbB5 = 0.0F;
            this.HcbB6 = 0.0F;
            this.HcbK = 0.0F; // only changed from zero for red alder in RAP variant

            this.HlcwB1 = 0.0F;
            this.HlcwB2 = 0.0F; // only changed from zero for red alder in RAP variant
            this.LcwB0 = 1.0F; // only changed from unity for red alder in RAP variant
            this.LcwB1 = 0.0F;
            this.LcwB2 = 0.0F;
            this.LcwB3 = 0.0F;

            this.DbhLimitForMaxCrownWidth = 999.99F;
            this.McwB0 = 0.0F;
            this.McwB1 = 0.0F;
            this.McwB2 = 0.0F;
            this.McwK = 1.0F; // only changed from unity for red alder in RAP variant

            this.MhcbB0 = 0.0F;
            this.MhcbB1 = 0.0F;
            this.MhcbB2 = 0.0F;
            this.MhcbB3 = 1.0F;
            this.HeightToCrownBaseRatioLimit = 1.0F;
        }
    }
}
