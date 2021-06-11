using System;
using System.Collections.Generic;

namespace Osu.Cof.Ferm.Heuristics
{
    public class FinancialValueTrajectory
    {
        private readonly List<float>[] acceptedValueByDiscountRate;
        private readonly List<float>[] candidateValueByDiscountRate;
        private readonly float[] highestFinancialValueByDiscountRate;

        public FinancialValueTrajectory(int capacity)
        {
            this.acceptedValueByDiscountRate = new List<float>[capacity];
            this.candidateValueByDiscountRate = new List<float>[capacity];
            this.highestFinancialValueByDiscountRate = new float[capacity];

            for (int discountRateIndex = 0; discountRateIndex < capacity; ++discountRateIndex)
            {
                this.acceptedValueByDiscountRate[discountRateIndex] = new();
                this.candidateValueByDiscountRate[discountRateIndex] = new();
                this.highestFinancialValueByDiscountRate[discountRateIndex] = Single.MinValue;
            }
        }

        public void AddMoveToDefaultDiscountRate(float acceptedValue, float candidateValue)
        {
            this.AddMoveToDiscountRate(Constant.HeuristicDefault.DiscountRateIndex, acceptedValue, candidateValue);
        }

        public void AddMoveToDiscountRate(int discountRateIndex, float acceptedValue, float candidateValue)
        {
            // typically, acceptedValue >= candidateValue but this does not hold for tabu search
            this.acceptedValueByDiscountRate[discountRateIndex].Add(acceptedValue);
            this.candidateValueByDiscountRate[discountRateIndex].Add(candidateValue);

            if (acceptedValue > this.highestFinancialValueByDiscountRate[discountRateIndex])
            {
                this.highestFinancialValueByDiscountRate[discountRateIndex] = acceptedValue;
            }
        }

        public IList<float> GetAcceptedValueForDiscountRateOrDefault(HeuristicResultPosition position)
        {
            return this.GetAcceptedValueForDiscountRateOrDefault(position.DiscountRateIndex);
        }

        public IList<float> GetAcceptedValueForDiscountRateOrDefault(int discountRateIndex)
        {
            if (this.acceptedValueByDiscountRate.Length == 1)
            {
                return this.acceptedValueByDiscountRate[Constant.HeuristicDefault.DiscountRateIndex];
            }
            return this.acceptedValueByDiscountRate[discountRateIndex];
        }

        public IList<float> GetCandidateValueForDiscountRateOrDefault(HeuristicResultPosition position)
        {
            return this.GetCandidateValueForDiscountRateOrDefault(position.DiscountRateIndex);
        }

        public IList<float> GetCandidateValueForDiscountRateOrDefault(int discountRateIndex)
        {
            if (this.candidateValueByDiscountRate.Length == 1)
            {
                return this.candidateValueByDiscountRate[Constant.HeuristicDefault.DiscountRateIndex];
            }
            return this.candidateValueByDiscountRate[discountRateIndex];
        }

        public float GetHighestValueForDefaultDiscountRate()
        {
            return this.highestFinancialValueByDiscountRate[Constant.HeuristicDefault.DiscountRateIndex];
        }

        public float GetHighestValueForDiscountRate(int discountRateIndex)
        {
            return this.highestFinancialValueByDiscountRate[discountRateIndex];
        }

        public float GetHighestValueForDiscountRateOrDefault(int discountRateIndex)
        {
            if (this.highestFinancialValueByDiscountRate.Length == 1)
            {
                return this.highestFinancialValueByDiscountRate[Constant.HeuristicDefault.DiscountRateIndex];
            }
            return this.highestFinancialValueByDiscountRate[discountRateIndex];
        }

        public void SetMoveCapacityForDefaultDiscountRate(int capacity)
        {
            this.SetMoveCapacityForDiscountRate(Constant.HeuristicDefault.DiscountRateIndex, capacity);
        }

        public void SetMoveCapacityForDiscountRate(int discountRateIndex, int capacity)
        {
            this.acceptedValueByDiscountRate[discountRateIndex].Capacity = capacity;
            this.candidateValueByDiscountRate[discountRateIndex].Capacity = capacity;
        }
    }
}
