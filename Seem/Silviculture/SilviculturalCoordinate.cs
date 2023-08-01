using System;

namespace Mars.Seem.Silviculture
{
    public class SilviculturalCoordinate : IEquatable<SilviculturalCoordinate>
    {
        public int FinancialIndex { get; set; }
        public int FirstThinPeriodIndex { get; init; }
        public int ParameterIndex { get; init; }
        public int RotationIndex { get; set; }
        public int SecondThinPeriodIndex { get; init; }
        public int ThirdThinPeriodIndex { get; init; }

        public SilviculturalCoordinate()
        {
            this.FinancialIndex = Constant.HeuristicDefault.CoordinateIndex;
            this.FirstThinPeriodIndex = Constant.HeuristicDefault.CoordinateIndex;
            this.SecondThinPeriodIndex = Constant.HeuristicDefault.CoordinateIndex;
            this.ThirdThinPeriodIndex = Constant.HeuristicDefault.CoordinateIndex;
            this.ParameterIndex = Constant.HeuristicDefault.CoordinateIndex;
            this.RotationIndex = Constant.HeuristicDefault.CoordinateIndex;
        }

        public SilviculturalCoordinate(SilviculturalCoordinate other)
        {
            this.FinancialIndex = other.FinancialIndex;
            this.FirstThinPeriodIndex = other.FirstThinPeriodIndex;
            this.SecondThinPeriodIndex = other.SecondThinPeriodIndex;
            this.ThirdThinPeriodIndex = other.ThirdThinPeriodIndex;
            this.ParameterIndex = other.ParameterIndex;
            this.RotationIndex = other.RotationIndex;
        }

        public static bool operator ==(SilviculturalCoordinate coordinate1, SilviculturalCoordinate coordinate2)
        {
            return coordinate1.Equals(coordinate2);
        }

        public static bool operator !=(SilviculturalCoordinate coordinate1, SilviculturalCoordinate coordinate2)
        {
            return coordinate1.Equals(coordinate2) == false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.FinancialIndex, this.RotationIndex, this.FirstThinPeriodIndex, this.SecondThinPeriodIndex, this.ThirdThinPeriodIndex, this.ParameterIndex);
        }

        public bool Equals(SilviculturalCoordinate? other)
        {
            if (other is null)
            {
                return false;
            }
            return (this.FinancialIndex == other.FinancialIndex) &&
                   (this.RotationIndex == other.RotationIndex) &&
                   (this.FirstThinPeriodIndex == other.FirstThinPeriodIndex) &&
                   (this.SecondThinPeriodIndex == other.SecondThinPeriodIndex) &&
                   (this.ThirdThinPeriodIndex == other.ThirdThinPeriodIndex) &&
                   (this.ParameterIndex == other.ParameterIndex);
        }

        public override bool Equals(object? obj)
        {
            if (obj is SilviculturalCoordinate other)
            {
                return this.Equals(other);
            }
            return false;
        }
    }
}
