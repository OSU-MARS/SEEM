using Osu.Cof.Ferm.Silviculture;
using System.Collections.Generic;

namespace Osu.Cof.Ferm.Optimization
{
    public class FinancialOptimizationTrajectory : FinancialValue
    {
        private readonly List<float>[,] acceptedValueByRotationAndScenario;
        private readonly List<float>[,] candidateValueByRotationAndScenario;

        public int MoveCapacity { get; private init; }

        public FinancialOptimizationTrajectory(int rotationCapacity, int financialCapacity, int moveCapacity)
            : base(rotationCapacity, financialCapacity)
        {
            this.acceptedValueByRotationAndScenario = new List<float>[rotationCapacity, financialCapacity];
            this.candidateValueByRotationAndScenario = new List<float>[rotationCapacity, financialCapacity];

            for (int rotationIndex = 0; rotationIndex < rotationCapacity; ++rotationIndex)
            {
                for (int financialIndex = 0; financialIndex < financialCapacity; ++financialIndex)
                {
                    this.acceptedValueByRotationAndScenario[rotationIndex, financialIndex] = new();
                    this.candidateValueByRotationAndScenario[rotationIndex, financialIndex] = new();
                }
            }

            this.MoveCapacity = moveCapacity;
        }

        public IList<float> GetAcceptedValuesWithDefaulting(StandTrajectoryCoordinate coordinate)
        {
            return this.GetAcceptedValuesWithDefaulting(coordinate.RotationIndex, coordinate.FinancialIndex);
        }

        public IList<float> GetAcceptedValuesWithDefaulting(int rotationIndex, int financialIndex)
        {
            if (this.acceptedValueByRotationAndScenario.Length == 1)
            {
                return this.acceptedValueByRotationAndScenario[Constant.HeuristicDefault.CoordinateIndex, Constant.HeuristicDefault.CoordinateIndex];
            }
            return this.acceptedValueByRotationAndScenario[rotationIndex, financialIndex];
        }

        public IList<float> GetCandidateValuesWithDefaulting(StandTrajectoryCoordinate coordinate)
        {
            return this.GetCandidateValuesWithDefaulting(coordinate.RotationIndex, coordinate.FinancialIndex);
        }

        public IList<float> GetCandidateValuesWithDefaulting(int rotationIndex, int discountRateIndex)
        {
            if (this.candidateValueByRotationAndScenario.Length == 1)
            {
                return this.candidateValueByRotationAndScenario[Constant.HeuristicDefault.CoordinateIndex, Constant.HeuristicDefault.CoordinateIndex];
            }
            return this.candidateValueByRotationAndScenario[rotationIndex, discountRateIndex];
        }

        public void SetMoveCapacity(int capacity)
        {
            this.acceptedValueByRotationAndScenario[Constant.HeuristicDefault.CoordinateIndex, Constant.HeuristicDefault.CoordinateIndex].Capacity = capacity;
            this.candidateValueByRotationAndScenario[Constant.HeuristicDefault.CoordinateIndex, Constant.HeuristicDefault.CoordinateIndex].Capacity = capacity;
        }

        public bool TryAddMove(StandTrajectoryCoordinate coordinate, float acceptedValue, float candidateValue)
        {
            int rotationIndex = coordinate.RotationIndex;
            int financialIndex = coordinate.FinancialIndex;
            if (acceptedValue > this.HighestFinancialValueByRotationAndScenario[rotationIndex, financialIndex])
            {
                this.HighestFinancialValueByRotationAndScenario[rotationIndex, financialIndex] = acceptedValue;
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
