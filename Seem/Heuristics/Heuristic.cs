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

        protected int GetInitialTreeRecordCount()
        {
            OrganonStand initialStand = this.CurrentTrajectory.StandByPeriod[0] ?? throw new NotSupportedException("Initial stand infomation is missing.");
            return initialStand.GetTreeRecordCount();
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
                float firstRotationNetPresentValue = -trajectory.TimberValue.FixedReforestationCostPerHectare - trajectory.TimberValue.SeedlingCost * trajectory.PlantingDensityInTreesPerHectare;
                for (int periodIndex = 1; periodIndex < trajectory.PlanningPeriods; ++periodIndex)
                {
                    firstRotationNetPresentValue += trajectory.ThinningVolume.NetPresentValue[periodIndex];
                }
                firstRotationNetPresentValue += trajectory.StandingVolume.NetPresentValue[^1];

                // convert from US$/ac to USk$/ac
                objectiveFunction = firstRotationNetPresentValue;

                if (this.Objective.TimberObjective == TimberObjective.LandExpectationValue)
                {
                    int rotationLengthInYears = trajectory.GetRotationLength();
                    float presentToFutureConversionFactor = MathF.Pow(1.0F + trajectory.TimberValue.DiscountRate, rotationLengthInYears);
                    float landExpectationValue = presentToFutureConversionFactor * firstRotationNetPresentValue / (presentToFutureConversionFactor - 1.0F);
                    objectiveFunction = landExpectationValue;
                }

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

            IHarvest? harvest = this.CurrentTrajectory.Configuration.Treatments.Harvests.FirstOrDefault();
            if ((harvest == null) || (harvest is ThinByIndividualTreeSelection == false))
            {
                // attempt to randomize an individual tree selection without a corresponding harvest present
                throw new NotSupportedException("No harvest is specified or the first harvest is not thinning by individual tree selection.");
            }

            int initialTreeRecordCount = this.GetInitialTreeRecordCount();
            float percentageScalingFactor = 100.0F / byte.MaxValue;
            if (this.Objective.HarvestPeriodSelection == HarvestPeriodSelection.All)
            {
                float harvestPeriodScalingFactor = (this.CurrentTrajectory.HarvestPeriods - Constant.RoundTowardsZeroTolerance) / proportionalPercentage;
                for (int treeIndex = 0; treeIndex < initialTreeRecordCount; ++treeIndex)
                {
                    float treePercentage = percentageScalingFactor * this.GetPseudorandomByteAsFloat();
                    if (treePercentage < proportionalPercentage)
                    {
                        int harvestPeriod = (int)(harvestPeriodScalingFactor * treePercentage);
                        this.CurrentTrajectory.SetTreeSelection(treeIndex, harvestPeriod);
                    }
                    else
                    {
                        this.CurrentTrajectory.SetTreeSelection(treeIndex, 0);
                    }
                }
            }
            else if (this.Objective.HarvestPeriodSelection == HarvestPeriodSelection.NoneOrLast)
            {
                for (int treeIndex = 0; treeIndex < initialTreeRecordCount; ++treeIndex)
                {
                    int harvestPeriod = percentageScalingFactor * this.GetPseudorandomByteAsFloat() < proportionalPercentage ? this.CurrentTrajectory.HarvestPeriods - 1 : 0;
                    this.CurrentTrajectory.SetTreeSelection(treeIndex, harvestPeriod);
                }
            }
            else
            {
                throw new NotSupportedException(String.Format("Unhandled harvest period selection {0}.", this.Objective.HarvestPeriodSelection));
            }
        }

        public void RandomizeTreeSelectionFrom(float perturbBy, Population eliteSolutions)
        {
            if ((perturbBy < 0.0F) || (perturbBy > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(perturbBy));
            }
            if (this.Objective.HarvestPeriodSelection == HarvestPeriodSelection.All)
            {
                throw new NotSupportedException();
            }

            IHarvest? harvest = this.CurrentTrajectory.Configuration.Treatments.Harvests.FirstOrDefault();
            if ((harvest == null) || (harvest is ThinByIndividualTreeSelection == false))
            {
                // attempt to randomize an individual tree selection without a corresponding harvest present
                throw new NotSupportedException("No harvest is specified or the first harvest is not individual tree selection.");
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
                    harvestPeriod = harvestPeriod == 0 ? eliteSolutions.HarvestPeriods : 0;
                }
                this.CurrentTrajectory.SetTreeSelection(treeIndex, harvestPeriod);
            }
        }

        public abstract TimeSpan Run();
    }
}
