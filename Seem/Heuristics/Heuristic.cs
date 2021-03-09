using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Ferm.Heuristics
{
    public abstract class Heuristic : PseudorandomizingTask
    {
        public List<float> AcceptedObjectiveFunctionByMove { get; protected set; }
        public float BestObjectiveFunction { get; protected set; }
        public OrganonStandTrajectory BestTrajectory { get; private init; }
        public OrganonStandTrajectory CurrentTrajectory { get; private init; }
        public Objective Objective { get; private init; }
        public List<float> CandidateObjectiveFunctionByMove { get; protected set; }

        protected Heuristic(OrganonStand stand, OrganonConfiguration organonConfiguration, Objective objective, HeuristicParameters parameters)
        {
            this.AcceptedObjectiveFunctionByMove = new List<float>();
            this.BestObjectiveFunction = Single.MinValue;

            this.BestTrajectory = new OrganonStandTrajectory(stand, organonConfiguration, parameters.TimberValue, objective.PlanningPeriods, parameters.UseFiaVolume)
            {
                Heuristic = this
            };

            this.CurrentTrajectory = new OrganonStandTrajectory(this.BestTrajectory);
            this.CurrentTrajectory.Name = this.CurrentTrajectory.Name + "Current";

            this.Objective = objective;
            this.CandidateObjectiveFunctionByMove = new List<float>();
        }

        public virtual void CopySelectionsFrom(StandTrajectory trajectory)
        {
            if (trajectory.TreeSelectionChangedSinceLastSimulation)
            {
                throw new ArgumentOutOfRangeException(nameof(trajectory));
            }

            this.BestObjectiveFunction = this.GetObjectiveFunction(trajectory);
            this.BestTrajectory.CopySelectionsFrom(trajectory);
            this.CurrentTrajectory.CopySelectionsFrom(trajectory);
            this.AcceptedObjectiveFunctionByMove.Clear();
            this.AcceptedObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
        }

        protected static int[] CreateSequentialArray(int length)
        {
            Debug.Assert(length > 0);

            int[] array = new int[length];
            for (int index = 0; index < length; ++index)
            {
                array[index] = index;
            }
            return array;
        }

        public abstract string GetName();

        public float GetObjectiveFunction(StandTrajectory trajectory)
        {
            Debug.Assert(trajectory.PeriodLengthInYears > 0);

            // find objective function value
            // Volume objective functions are in m³/ha or MBF/ac.
            float objectiveFunction;
            if ((this.Objective.TimberObjective == TimberObjective.LandExpectationValue) ||
                (this.Objective.TimberObjective == TimberObjective.NetPresentValue))
            {
                // net present value of first rotation
                // Harvest and standing volumes are in board feet and prices are in MBF, hence multiplications by 0.001.
                // TODO: support per species pricing
                float firstRotationNetPresentValue = trajectory.TimberValue.GetNetPresentReforestationValue(trajectory.PlantingDensityInTreesPerHectare);
                for (int periodIndex = 1; periodIndex < trajectory.PlanningPeriods; ++periodIndex)
                {
                    firstRotationNetPresentValue += trajectory.ThinningVolume.NetPresentValue[periodIndex];
                }
                firstRotationNetPresentValue += trajectory.StandingVolume.NetPresentValue[^1];

                objectiveFunction = firstRotationNetPresentValue;

                if (this.Objective.TimberObjective == TimberObjective.LandExpectationValue)
                {
                    int rotationLengthInYears = trajectory.GetRotationLength();
                    float presentToFutureConversionFactor = MathF.Pow(1.0F + trajectory.TimberValue.DiscountRate, rotationLengthInYears);
                    float landExpectationValue = presentToFutureConversionFactor * firstRotationNetPresentValue / (presentToFutureConversionFactor - 1.0F);
                    objectiveFunction = landExpectationValue;
                }

                // convert from US$/ha to USk$/ha
                objectiveFunction *= 0.001F;
            }
            else if (this.Objective.TimberObjective == TimberObjective.ScribnerVolume)
            {
                // direct volume addition
                objectiveFunction = 0.0F;
                for (int periodIndex = 1; periodIndex < trajectory.PlanningPeriods; ++periodIndex)
                {
                    objectiveFunction += trajectory.ThinningVolume.ScribnerTotal[periodIndex];
                }
                objectiveFunction += trajectory.StandingVolume.ScribnerTotal[^1];
            }
            else 
            {
                throw new NotSupportedException("Unhandled timber objective " + this.Objective.TimberObjective + ".");
            }

            return objectiveFunction;
        }

        protected int GetOneOptCandidateRandom(int currentHarvestPeriod, IList<int> thinningPeriods)
        {
            Debug.Assert((thinningPeriods.Count == 2) || (thinningPeriods.Count == 3));
            if (thinningPeriods.Count == 2)
            {
                return currentHarvestPeriod == thinningPeriods[0] ? thinningPeriods[1] : thinningPeriods[0];
            }

            bool incrementIndex = this.GetPseudorandomByteAsProbability() < 0.5F;
            if (currentHarvestPeriod == thinningPeriods[0])
            {
                return incrementIndex ? thinningPeriods[2] : thinningPeriods[1];
            }
            else if (currentHarvestPeriod == thinningPeriods[1])
            {
                return incrementIndex ? thinningPeriods[0] : thinningPeriods[2];
            }
            else
            {
                Debug.Assert(currentHarvestPeriod == thinningPeriods[2]);
                return incrementIndex ? thinningPeriods[0] : thinningPeriods[1];
            }
        }

        public virtual IHeuristicMoveLog? GetMoveLog()
        {
            return null;
        }

        public virtual HeuristicParameters? GetParameters()
        {
            return null;
        }

        public void RandomizeTreeSelection(float proportionalPercentage)
        {
            if ((proportionalPercentage < 0.0F) || (proportionalPercentage > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(proportionalPercentage));
            }

            IList<int> thinningPeriods = this.CurrentTrajectory.Configuration.Treatments.GetValidThinningPeriods();
            if (thinningPeriods[0] != Constant.NoHarvestPeriod)
            {
                throw new NotSupportedException("First thinning selection is a harvest. Expected it to be the no harvest option.");
            }
            if (thinningPeriods.Count == 1)
            {
                // attempt to randomize an individual tree selection without a corresponding harvest present
                throw new NotSupportedException("No harvest is specified or the first harvest is not thinning by individual tree selection.");
            }
            thinningPeriods.RemoveAt(0);

            float proportionalFraction = 0.01F * proportionalPercentage;
            int initialTreeRecordCount = this.CurrentTrajectory.GetInitialTreeRecordCount();
            float indexScalingFactor = (thinningPeriods.Count - Constant.RoundTowardsZeroTolerance ) / proportionalFraction;
            for (int treeIndex = 0; treeIndex < initialTreeRecordCount; ++treeIndex)
            {
                int thinningPeriod = Constant.NoHarvestPeriod;
                float probability = this.GetPseudorandomByteAsProbability();
                if (probability < proportionalFraction)
                {
                    // probability falls into the harvest fraction, choose equally among available harvest periods
                    int periodIndex = (int)(indexScalingFactor * probability);
                    thinningPeriod = thinningPeriods[periodIndex];
                }
                this.CurrentTrajectory.SetTreeSelection(treeIndex, thinningPeriod);
            }
        }

        public void RandomizeTreeSelectionFrom(float perturbBy, Population eliteSolutions)
        {
            if ((perturbBy < 0.0F) || (perturbBy > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(perturbBy));
            }

            IHarvest? harvest = this.CurrentTrajectory.Configuration.Treatments.Harvests.FirstOrDefault();
            if ((harvest == null) || (harvest is ThinByIndividualTreeSelection == false))
            {
                // attempt to randomize an individual tree selection without a corresponding harvest present
                throw new NotSupportedException("No harvest is specified or the first harvest is not individual tree selection.");
            }

            IList<int> thinningPeriods = this.CurrentTrajectory.Configuration.Treatments.GetValidThinningPeriods();
            if (thinningPeriods.Count != 2)
            {
                throw new NotSupportedException("Only a single thinning is currently supported.");
            }

            int maxPerturbationIndex = (int)(perturbBy * eliteSolutions.TreeCount) + 1;
            int[] treeSelection = eliteSolutions.IndividualTreeSelections[0];
            int[] treeIndices = Heuristic.CreateSequentialArray(eliteSolutions.TreeCount);
            this.Pseudorandom.Shuffle(treeIndices);
            for (int shuffleIndex = 0; shuffleIndex < treeIndices.Length; ++shuffleIndex)
            {
                int treeIndex = treeIndices[shuffleIndex];
                int harvestPeriod = treeSelection[treeIndex];
                if (shuffleIndex < maxPerturbationIndex)
                {
                    Debug.Assert((harvestPeriod == thinningPeriods[0]) || (harvestPeriod == thinningPeriods[1]));
                    harvestPeriod = harvestPeriod == thinningPeriods[0] ? thinningPeriods[1] : thinningPeriods[0];
                }
                this.CurrentTrajectory.SetTreeSelection(treeIndex, harvestPeriod);
            }
        }

        public abstract TimeSpan Run();
    }
}
