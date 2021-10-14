using System;

namespace Osu.Cof.Ferm.Heuristics
{
    public class HeuristicResultPosition : IEquatable<HeuristicResultPosition>
    {
        public int FinancialIndex { get; set; }
        public int FirstThinPeriodIndex { get; init; }
        public int ParameterIndex { get; init; }
        public int RotationIndex { get; set; }
        public int SecondThinPeriodIndex { get; init; }
        public int ThirdThinPeriodIndex { get; init; }

        public HeuristicResultPosition()
        {
            this.FinancialIndex = -1;
            this.FirstThinPeriodIndex = Constant.NoThinPeriod;
            this.SecondThinPeriodIndex = Constant.NoThinPeriod;
            this.ThirdThinPeriodIndex = Constant.NoThinPeriod;
            this.ParameterIndex = -1;
            this.RotationIndex = -1;
        }

        public HeuristicResultPosition(HeuristicResultPosition other)
        {
            this.FinancialIndex = other.FinancialIndex;
            this.FirstThinPeriodIndex = other.FirstThinPeriodIndex;
            this.SecondThinPeriodIndex = other.SecondThinPeriodIndex;
            this.ThirdThinPeriodIndex = other.ThirdThinPeriodIndex;
            this.ParameterIndex = other.ParameterIndex;
            this.RotationIndex = other.RotationIndex;
        }

        public static bool operator ==(HeuristicResultPosition position1, HeuristicResultPosition position2)
        {
            return position1.Equals(position2);
        }

        public static bool operator !=(HeuristicResultPosition position1, HeuristicResultPosition position2)
        {
            return position1.Equals(position2) == false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.FinancialIndex, this.RotationIndex, this.FirstThinPeriodIndex, this.SecondThinPeriodIndex, this.ThirdThinPeriodIndex, this.ParameterIndex);
        }

        public bool Equals(HeuristicResultPosition? other)
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
            if (obj is HeuristicResultPosition other)
            {
                return this.Equals(other);
            }
            return false;
        }
    }
}
