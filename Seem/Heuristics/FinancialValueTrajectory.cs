using System;
using System.Collections.Generic;

namespace Osu.Cof.Ferm.Heuristics
{
    public class FinancialValueTrajectory
    {
        private readonly List<float>[,] acceptedValueByRotationAndDiscount;
        private readonly List<float>[,] candidateValueByRotationAndDiscount;
        private readonly float[,] highestFinancialValueByRotationAndScenario;

        public int MoveCapacity { get; private init; }

        public FinancialValueTrajectory(int rotationCapacity, int discountRateCapacity, int moveCapacity)
        {
            this.acceptedValueByRotationAndDiscount = new List<float>[rotationCapacity, discountRateCapacity];
            this.candidateValueByRotationAndDiscount = new List<float>[rotationCapacity, discountRateCapacity];
            this.highestFinancialValueByRotationAndScenario = new float[rotationCapacity, discountRateCapacity];

            for (int rotationIndex = 0; rotationIndex < rotationCapacity; ++rotationIndex)
            {
                for (int financialIndex = 0; financialIndex < discountRateCapacity; ++financialIndex)
                {
                    this.acceptedValueByRotationAndDiscount[rotationIndex, financialIndex] = new();
                    this.candidateValueByRotationAndDiscount[rotationIndex, financialIndex] = new();
                    this.highestFinancialValueByRotationAndScenario[rotationIndex, financialIndex] = Single.MinValue;
                }
            }

            this.MoveCapacity = moveCapacity;
        }

        public IList<float> GetAcceptedValuesWithDefaulting(HeuristicResultPosition position)
        {
            return this.GetAcceptedValuesWithDefaulting(position.RotationIndex, position.FinancialIndex);
        }

        public IList<float> GetAcceptedValuesWithDefaulting(int rotationIndex, int financialIndex)
        {
            if (this.acceptedValueByRotationAndDiscount.Length == 1)
            {
                return this.acceptedValueByRotationAndDiscount[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.FinancialIndex];
            }
            return this.acceptedValueByRotationAndDiscount[rotationIndex, financialIndex];
        }

        public IList<float> GetCandidateValuesWithDefaulting(HeuristicResultPosition position)
        {
            return this.GetCandidateValuesWithDefaulting(position.RotationIndex, position.FinancialIndex);
        }

        public IList<float> GetCandidateValuesWithDefaulting(int rotationIndex, int discountRateIndex)
        {
            if (this.candidateValueByRotationAndDiscount.Length == 1)
            {
                return this.candidateValueByRotationAndDiscount[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.FinancialIndex];
            }
            return this.candidateValueByRotationAndDiscount[rotationIndex, discountRateIndex];
        }

        public float GetHighestValue()
        {
            return this.highestFinancialValueByRotationAndScenario[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.FinancialIndex];
        }

        public float GetHighestValue(int rotationIndex, int financialIndex)
        {
            return this.highestFinancialValueByRotationAndScenario[rotationIndex, financialIndex];
        }

        public float GetHighestValueWithDefaulting(HeuristicResultPosition position)
        {
            return this.GetHighestValueWithDefaulting(position.RotationIndex, position.FinancialIndex);
        }

        public float GetHighestValueWithDefaulting(int rotationIndex, int financialIndex)
        {
            if (this.highestFinancialValueByRotationAndScenario.Length == 1)
            {
                return this.highestFinancialValueByRotationAndScenario[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.FinancialIndex];
            }
            return this.highestFinancialValueByRotationAndScenario[rotationIndex, financialIndex];
        }

        public void SetMoveCapacity(int capacity)
        {
            this.acceptedValueByRotationAndDiscount[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.FinancialIndex].Capacity = capacity;
            this.candidateValueByRotationAndDiscount[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.FinancialIndex].Capacity = capacity;
        }

        public bool TryAddMove(float acceptedValue, float candidateValue)
        {
            return this.TryAddMove(Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.FinancialIndex, acceptedValue, candidateValue);
        }

        public bool TryAddMove(int rotationIndex, int financialIndex, float acceptedValue, float candidateValue)
        {
            if (acceptedValue > this.highestFinancialValueByRotationAndScenario[rotationIndex, financialIndex])
            {
                this.highestFinancialValueByRotationAndScenario[rotationIndex, financialIndex] = acceptedValue;
            }
            // typically, acceptedValue >= candidateValue but this does not hold during Monte Carlo reheating or after tabu search's
            // initial ascent phase

            List<float> acceptedValues = this.acceptedValueByRotationAndDiscount[rotationIndex, financialIndex];
            List<float> candidateValues = this.candidateValueByRotationAndDiscount[rotationIndex, financialIndex];
            if (acceptedValues.Count < this.MoveCapacity)
            {
                // append new values up to maximum capacity
                acceptedValues.Add(acceptedValue);
                candidateValues.Add(candidateValue);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
