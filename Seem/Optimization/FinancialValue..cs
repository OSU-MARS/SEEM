using Mars.Seem.Silviculture;
using System;

namespace Mars.Seem.Optimization
{
    public class FinancialValue
    {
        protected float[,] HighestFinancialValueByRotationAndScenario { get; private init; }

        public FinancialValue(int rotationCapacity, int financialCapacity)
        {
            this.HighestFinancialValueByRotationAndScenario = new float[rotationCapacity, financialCapacity];

            for (int rotationIndex = 0; rotationIndex < rotationCapacity; ++rotationIndex)
            {
                for (int financialIndex = 0; financialIndex < financialCapacity; ++financialIndex)
                {
                    this.HighestFinancialValueByRotationAndScenario[rotationIndex, financialIndex] = Single.MinValue;
                }
            }
        }

        public float GetHighestValue()
        {
            return this.HighestFinancialValueByRotationAndScenario[Constant.HeuristicDefault.CoordinateIndex, Constant.HeuristicDefault.CoordinateIndex];
        }

        public float GetHighestValue(int rotationIndex, int financialIndex)
        {
            return this.HighestFinancialValueByRotationAndScenario[rotationIndex, financialIndex];
        }

        public float GetHighestValueWithDefaulting(SilviculturalCoordinate coordinate)
        {
            return this.GetHighestValueWithDefaulting(coordinate.RotationIndex, coordinate.FinancialIndex);
        }

        public float GetHighestValueWithDefaulting(int rotationIndex, int financialIndex)
        {
            if (this.HighestFinancialValueByRotationAndScenario.Length == 1)
            {
                return this.HighestFinancialValueByRotationAndScenario[Constant.HeuristicDefault.CoordinateIndex, Constant.HeuristicDefault.CoordinateIndex];
            }
            return this.HighestFinancialValueByRotationAndScenario[rotationIndex, financialIndex];
        }

        public void SetValue(int rotationIndex, int financialIndex, float financialValue)
        {
            this.HighestFinancialValueByRotationAndScenario[rotationIndex, financialIndex] = financialValue;
        }
    }
}
