using Mars.Seem.Extensions;
using Mars.Seem.Organon;
using Mars.Seem.Silviculture;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mars.Seem.Heuristics
{
    // arrays are allocated for all possible combinations of rotations and discount rates
    // This is likely to be an overallocation as thinnings will probably preclude some rotations, leading to unused elements within the arrays. For now,
    // it's assumed that the FIFO (first in, first out) length is short enough the memory required isn't a concern.
    public class PrescriptionLastNMoveLog : HeuristicMoveLog
    {
        // first thin
        private readonly float[,,] fromAbovePercentageByRotationAndRate1;
        private readonly float[,,] fromBelowPercentageByRotationAndRate1;
        private readonly float[,,] proportionalPercentageByRotationAndRate1;

        // second thin
        private readonly float[,,] fromAbovePercentageByRotationAndRate2;
        private readonly float[,,] fromBelowPercentageByRotationAndRate2;
        private readonly float[,,] proportionalPercentageByRotationAndRate2;

        // third thin
        private readonly float[,,] fromAbovePercentageByRotationAndRate3;
        private readonly float[,,] fromBelowPercentageByRotationAndRate3;
        private readonly float[,,] proportionalPercentageByRotationAndRate3;

        private readonly int fifoLength;
        private readonly List<int>[,] moveNumberByRotationAndRate;
        private readonly int[,,] retainedMoveNumberByRotationAndRate;
        private readonly int[,] setIndexByRotationAndRate;

        public PrescriptionLastNMoveLog(int rotationLengths, int financialScenarios, int moveCapacity, int nMoves)
            : base(moveCapacity)
        {
            if (rotationLengths < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(rotationLengths));
            }
            if (financialScenarios < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(financialScenarios));
            }
            if (nMoves < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(nMoves));
            }

            this.fromAbovePercentageByRotationAndRate1 = new float[rotationLengths, financialScenarios, nMoves];
            this.fromBelowPercentageByRotationAndRate1 = new float[rotationLengths, financialScenarios, nMoves];
            this.proportionalPercentageByRotationAndRate1 = new float[rotationLengths, financialScenarios, nMoves];

            this.fromAbovePercentageByRotationAndRate2 = new float[rotationLengths, financialScenarios, nMoves];
            this.fromBelowPercentageByRotationAndRate2 = new float[rotationLengths, financialScenarios, nMoves];
            this.proportionalPercentageByRotationAndRate2 = new float[rotationLengths, financialScenarios, nMoves];

            this.fromAbovePercentageByRotationAndRate3 = new float[rotationLengths, financialScenarios, nMoves];
            this.fromBelowPercentageByRotationAndRate3 = new float[rotationLengths, financialScenarios, nMoves];
            this.proportionalPercentageByRotationAndRate3 = new float[rotationLengths, financialScenarios, nMoves];

            this.fifoLength = nMoves;
            this.moveNumberByRotationAndRate = new List<int>[rotationLengths, financialScenarios];
            this.retainedMoveNumberByRotationAndRate = new int[rotationLengths, financialScenarios, nMoves];
            this.setIndexByRotationAndRate = new int[rotationLengths, financialScenarios];

            ArrayExtensions.Fill(this.fromAbovePercentageByRotationAndRate1, -1.0F);
            ArrayExtensions.Fill(this.fromBelowPercentageByRotationAndRate1, -1.0F);
            ArrayExtensions.Fill(this.proportionalPercentageByRotationAndRate1, -1.0F);

            ArrayExtensions.Fill(this.fromAbovePercentageByRotationAndRate2, -1.0F);
            ArrayExtensions.Fill(this.fromBelowPercentageByRotationAndRate2, -1.0F);
            ArrayExtensions.Fill(this.proportionalPercentageByRotationAndRate2, -1.0F);

            ArrayExtensions.Fill(this.fromAbovePercentageByRotationAndRate3, -1.0F);
            ArrayExtensions.Fill(this.fromBelowPercentageByRotationAndRate3, -1.0F);
            ArrayExtensions.Fill(this.proportionalPercentageByRotationAndRate3, -1.0F);

            for (int rotationIndex = 0; rotationIndex < rotationLengths; ++rotationIndex)
            {
                for (int financialIndex = 0; financialIndex < financialScenarios; ++financialIndex)
                {
                    this.moveNumberByRotationAndRate[rotationIndex, financialIndex] = new();
                }
            }

            ArrayExtensions.Fill(this.retainedMoveNumberByRotationAndRate, -1);
            // this.setIndexByRotationAndRate can remain zeros
        }

        public override string GetCsvHeader(string prefix)
        {
            return prefix + "Thin1above," + prefix + "Thin1proportional," + prefix + "Thin1below," +
                   prefix + "Thin2above," + prefix + "Thin2proportional," + prefix + "Thin2below," +
                   prefix + "Thin3above," + prefix + "Thin3proportional," + prefix + "Thin3below";
        }

        public override string GetCsvValues(StandTrajectoryCoordinate coordinate, int moveNumber)
        {
            // apply defaulting
            // If this move log instance is specific to a single rotation and financial scenario then the provided position is ignored.
            int rotationIndex = 0;
            int financialIndex = 0;
            if (this.setIndexByRotationAndRate.Length > 1)
            {
                rotationIndex = coordinate.RotationIndex;
                financialIndex = coordinate.FinancialIndex;
            }

            for (int fifoIndex = 0; fifoIndex < this.fifoLength; ++fifoIndex)
            {
                if (moveNumber == this.retainedMoveNumberByRotationAndRate[rotationIndex, financialIndex, fifoIndex])
                {
                    float fromAbovePercentage1 = this.fromAbovePercentageByRotationAndRate1[rotationIndex, financialIndex, fifoIndex];
                    float proportionalPercentage1 = this.proportionalPercentageByRotationAndRate1[rotationIndex, financialIndex, fifoIndex];
                    float fromBelowPercentage1 = this.fromBelowPercentageByRotationAndRate1[rotationIndex, financialIndex, fifoIndex];
                    float fromAbovePercentage2 = this.fromAbovePercentageByRotationAndRate2[rotationIndex, financialIndex, fifoIndex];
                    float proportionalPercentage2 = this.proportionalPercentageByRotationAndRate2[rotationIndex, financialIndex, fifoIndex];
                    float fromBelowPercentage2 = this.fromBelowPercentageByRotationAndRate2[rotationIndex, financialIndex, fifoIndex];
                    float fromAbovePercentage3 = this.fromAbovePercentageByRotationAndRate3[rotationIndex, financialIndex, fifoIndex];
                    float proportionalPercentage3 = this.proportionalPercentageByRotationAndRate3[rotationIndex, financialIndex, fifoIndex];
                    float fromBelowPercentage3 = this.fromBelowPercentageByRotationAndRate3[rotationIndex, financialIndex, fifoIndex];

                    return fromAbovePercentage1.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                           proportionalPercentage1.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                           fromBelowPercentage1.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                           fromAbovePercentage2.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                           proportionalPercentage2.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                           fromBelowPercentage2.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                           fromAbovePercentage3.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                           proportionalPercentage3.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture) + "," +
                           fromBelowPercentage3.ToString(Constant.DefaultPercentageFormat, CultureInfo.InvariantCulture);
                }
            }

            return ",,,,,,,,";
        }

        public override int GetMoveNumberWithDefaulting(StandTrajectoryCoordinate coordinate, int moveIndex)
        {
            if (this.moveNumberByRotationAndRate.Length == 1)
            {
                return this.moveNumberByRotationAndRate[Constant.HeuristicDefault.CoordinateIndex, Constant.HeuristicDefault.CoordinateIndex][moveIndex];
            }
            return this.moveNumberByRotationAndRate[coordinate.RotationIndex, coordinate.FinancialIndex][moveIndex];
        }

        public bool TryAddMove(StandTrajectoryCoordinate coordinate, int moveNumber, ThinByPrescription? firstThinPrescription, ThinByPrescription? secondThinPrescription, ThinByPrescription? thirdThinPrescription)
        {
            int rotationIndex = coordinate.RotationIndex;
            int financialIndex = coordinate.FinancialIndex;
            List<int> moveNumbers = this.moveNumberByRotationAndRate[rotationIndex, financialIndex];
            if (moveNumbers.Count > this.MoveCapacity)
            {
                return false;
            }
            moveNumbers.Add(moveNumber);

            // capture thinning prescriptions (for now, treat no prescription as interchangeable with zero thinning intensity)
            int setIndex = this.setIndexByRotationAndRate[rotationIndex, financialIndex];
            this.retainedMoveNumberByRotationAndRate[rotationIndex, financialIndex, setIndex] = moveNumber;

            if (firstThinPrescription != null)
            {
                this.fromAbovePercentageByRotationAndRate1[rotationIndex, financialIndex, setIndex] = firstThinPrescription.FromAbovePercentage;
                this.proportionalPercentageByRotationAndRate1[rotationIndex, financialIndex, setIndex] = firstThinPrescription.ProportionalPercentage;
                this.fromBelowPercentageByRotationAndRate1[rotationIndex, financialIndex, setIndex] = firstThinPrescription.FromBelowPercentage;
            }
            else
            {
                this.fromAbovePercentageByRotationAndRate1[rotationIndex, financialIndex, setIndex] = 0.0F;
                this.proportionalPercentageByRotationAndRate1[rotationIndex, financialIndex, setIndex] = 0.0F;
                this.fromBelowPercentageByRotationAndRate1[rotationIndex, financialIndex, setIndex] = 0.0F;
            }

            if (secondThinPrescription != null)
            {
                this.fromAbovePercentageByRotationAndRate2[rotationIndex, financialIndex, setIndex] = secondThinPrescription.FromAbovePercentage;
                this.proportionalPercentageByRotationAndRate2[rotationIndex, financialIndex, setIndex] = secondThinPrescription.ProportionalPercentage;
                this.fromBelowPercentageByRotationAndRate2[rotationIndex, financialIndex, setIndex] = secondThinPrescription.FromBelowPercentage;
            }
            else
            {
                this.fromAbovePercentageByRotationAndRate2[rotationIndex, financialIndex, setIndex] = 0.0F;
                this.proportionalPercentageByRotationAndRate2[rotationIndex, financialIndex, setIndex] = 0.0F;
                this.fromBelowPercentageByRotationAndRate2[rotationIndex, financialIndex, setIndex] = 0.0F;
            }

            if (thirdThinPrescription != null)
            {
                this.fromAbovePercentageByRotationAndRate3[rotationIndex, financialIndex, setIndex] = thirdThinPrescription.FromAbovePercentage;
                this.proportionalPercentageByRotationAndRate3[rotationIndex, financialIndex, setIndex] = thirdThinPrescription.ProportionalPercentage;
                this.fromBelowPercentageByRotationAndRate3[rotationIndex, financialIndex, setIndex] = thirdThinPrescription.FromBelowPercentage;
            }
            else
            {
                this.fromAbovePercentageByRotationAndRate3[rotationIndex, financialIndex, setIndex] = 0.0F;
                this.proportionalPercentageByRotationAndRate3[rotationIndex, financialIndex, setIndex] = 0.0F;
                this.fromBelowPercentageByRotationAndRate3[rotationIndex, financialIndex, setIndex] = 0.0F;
            }

            // increment set index
            ++setIndex;
            if (setIndex >= this.fifoLength)
            {
                setIndex = 0;
            }
            this.setIndexByRotationAndRate[rotationIndex, financialIndex] = setIndex;

            return true;
        }
    }
}
