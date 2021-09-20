using System;
using System.Collections.Generic;

namespace Osu.Cof.Ferm.Heuristics
{
    public class FinancialValueTrajectory
    {
        private readonly List<float>[,] acceptedValueByRotationAndScenario;
        private readonly List<float>[,] candidateValueByRotationAndScenario;
        private readonly float[,] highestFinancialValueByRotationAndScenario;

        public int MoveCapacity { get; private init; }

        public FinancialValueTrajectory(int rotationCapacity, int discountRateCapacity, int moveCapacity)
        {
            this.acceptedValueByRotationAndScenario = new List<float>[rotationCapacity, discountRateCapacity];
            this.candidateValueByRotationAndScenario = new List<float>[rotationCapacity, discountRateCapacity];
            this.highestFinancialValueByRotationAndScenario = new float[rotationCapacity, discountRateCapacity];

            for (int rotationIndex = 0; rotationIndex < rotationCapacity; ++rotationIndex)
            {
                for (int financialIndex = 0; financialIndex < discountRateCapacity; ++financialIndex)
                {
                    this.acceptedValueByRotationAndScenario[rotationIndex, financialIndex] = new();
                    this.candidateValueByRotationAndScenario[rotationIndex, financialIndex] = new();
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
            if (this.acceptedValueByRotationAndScenario.Length == 1)
            {
                return this.acceptedValueByRotationAndScenario[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.FinancialIndex];
            }
            return this.acceptedValueByRotationAndScenario[rotationIndex, financialIndex];
        }

        public IList<float> GetCandidateValuesWithDefaulting(HeuristicResultPosition position)
        {
            return this.GetCandidateValuesWithDefaulting(position.RotationIndex, position.FinancialIndex);
        }

        public IList<float> GetCandidateValuesWithDefaulting(int rotationIndex, int discountRateIndex)
        {
            if (this.candidateValueByRotationAndScenario.Length == 1)
            {
                return this.candidateValueByRotationAndScenario[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.FinancialIndex];
            }
            return this.candidateValueByRotationAndScenario[rotationIndex, discountRateIndex];
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
            this.acceptedValueByRotationAndScenario[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.FinancialIndex].Capacity = capacity;
            this.candidateValueByRotationAndScenario[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.FinancialIndex].Capacity = capacity;
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

            List<float> acceptedValues = this.acceptedValueByRotationAndScenario[rotationIndex, financialIndex];
            List<float> candidateValues = this.candidateValueByRotationAndScenario[rotationIndex, financialIndex];
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
