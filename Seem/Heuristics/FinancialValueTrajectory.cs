using System;
using System.Collections.Generic;

namespace Osu.Cof.Ferm.Heuristics
{
    public class FinancialValueTrajectory
    {
        private readonly List<float>[,] acceptedValueByRotationAndDiscount;
        private readonly List<float>[,] candidateValueByRotationAndDiscount;
        private readonly float[,] highestFinancialValueByRotationAndDiscount;

        public FinancialValueTrajectory(int rotationCapacity, int discountRateCapacity)
        {
            this.acceptedValueByRotationAndDiscount = new List<float>[rotationCapacity, discountRateCapacity];
            this.candidateValueByRotationAndDiscount = new List<float>[rotationCapacity, discountRateCapacity];
            this.highestFinancialValueByRotationAndDiscount = new float[rotationCapacity, discountRateCapacity];

            for (int rotationIndex = 0; rotationIndex < rotationCapacity; ++rotationIndex)
            {
                for (int discountRateIndex = 0; discountRateIndex < discountRateCapacity; ++discountRateIndex)
                {
                    this.acceptedValueByRotationAndDiscount[rotationIndex, discountRateIndex] = new();
                    this.candidateValueByRotationAndDiscount[rotationIndex, discountRateIndex] = new();
                    this.highestFinancialValueByRotationAndDiscount[rotationIndex, discountRateIndex] = Single.MinValue;
                }
            }
        }

        public void AddMove(float acceptedValue, float candidateValue)
        {
            this.AddMove(Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.DiscountRateIndex, acceptedValue, candidateValue);
        }

        public void AddMove(int rotationIndex, int discountRateIndex, float acceptedValue, float candidateValue)
        {
            // typically, acceptedValue >= candidateValue but this does not hold for tabu search
            this.acceptedValueByRotationAndDiscount[rotationIndex, discountRateIndex].Add(acceptedValue);
            this.candidateValueByRotationAndDiscount[rotationIndex, discountRateIndex].Add(candidateValue);

            if (acceptedValue > this.highestFinancialValueByRotationAndDiscount[rotationIndex, discountRateIndex])
            {
                this.highestFinancialValueByRotationAndDiscount[rotationIndex, discountRateIndex] = acceptedValue;
            }
        }

        public IList<float> GetAcceptedValuesWithDefaulting(HeuristicResultPosition position)
        {
            return this.GetAcceptedValuesWithDefaulting(position.RotationIndex, position.DiscountRateIndex);
        }

        public IList<float> GetAcceptedValuesWithDefaulting(int rotationIndex, int discountRateIndex)
        {
            if (this.acceptedValueByRotationAndDiscount.Length == 1)
            {
                return this.acceptedValueByRotationAndDiscount[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.DiscountRateIndex];
            }
            return this.acceptedValueByRotationAndDiscount[rotationIndex, discountRateIndex];
        }

        public IList<float> GetCandidateValuesWithDefaulting(HeuristicResultPosition position)
        {
            return this.GetCandidateValuesWithDefaulting(position.RotationIndex, position.DiscountRateIndex);
        }

        public IList<float> GetCandidateValuesWithDefaulting(int rotationIndex, int discountRateIndex)
        {
            if (this.candidateValueByRotationAndDiscount.Length == 1)
            {
                return this.candidateValueByRotationAndDiscount[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.DiscountRateIndex];
            }
            return this.candidateValueByRotationAndDiscount[rotationIndex, discountRateIndex];
        }

        public float GetHighestValue()
        {
            return this.highestFinancialValueByRotationAndDiscount[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.DiscountRateIndex];
        }

        public float GetHighestValue(int rotationIndex, int discountRateIndex)
        {
            return this.highestFinancialValueByRotationAndDiscount[rotationIndex, discountRateIndex];
        }

        public float GetHighestValueWithDefaulting(HeuristicResultPosition position)
        {
            return this.GetHighestValueWithDefaulting(position.RotationIndex, position.DiscountRateIndex);
        }

        public float GetHighestValueWithDefaulting(int rotationIndex, int discountRateIndex)
        {
            if (this.highestFinancialValueByRotationAndDiscount.Length == 1)
            {
                return this.highestFinancialValueByRotationAndDiscount[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.DiscountRateIndex];
            }
            return this.highestFinancialValueByRotationAndDiscount[rotationIndex, discountRateIndex];
        }

        public void SetMoveCapacity(int capacity)
        {
            this.acceptedValueByRotationAndDiscount[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.DiscountRateIndex].Capacity = capacity;
            this.candidateValueByRotationAndDiscount[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.DiscountRateIndex].Capacity = capacity;
        }
    }
}
