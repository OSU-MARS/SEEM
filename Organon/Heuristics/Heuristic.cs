using Osu.Cof.Ferm.Cmdlets;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Ferm.Heuristics
{
    public abstract class Heuristic : PseudorandomizingTask
    {
        public float BestObjectiveFunction { get; protected set; }
        public List<float> AcceptedObjectiveFunctionByMove { get; protected set; }
        public OrganonStandTrajectory BestTrajectory { get; private set; }
        public OrganonStandTrajectory CurrentTrajectory { get; private set; }
        public Objective Objective { get; private set; }
        public List<float> CandidateObjectiveFunctionByMove { get; protected set; }

        protected Heuristic(OrganonStand stand, OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective)
        {
            this.BestObjectiveFunction = Single.MinValue;
            this.AcceptedObjectiveFunctionByMove = new List<float>();

            this.BestTrajectory = new OrganonStandTrajectory(stand, organonConfiguration, planningPeriods, objective.VolumeUnits)
            {
                Heuristic = this,
            };
            this.BestTrajectory.Name = this.BestTrajectory.Name + "Best";

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

        protected int[] CreateSequentialArray(int length)
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
            return this.CurrentTrajectory.StandByPeriod[0].GetTreeRecordCount();
        }

        public abstract string GetName();

        public float GetObjectiveFunction(StandTrajectory trajectory)
        {
            Debug.Assert(trajectory.PeriodLengthInYears > 0);

            // find objective function value
            // Volume objective functions are in m³/ha or MBF/ac.
            float objectiveFunction;
            if (this.Objective.IsLandExpectationValue)
            {
                if (trajectory.VolumeUnits == VolumeUnits.CubicMetersPerHectare)
                {
                    // TODO: also, check tree model is using a five year time step
                    throw new NotSupportedException();
                }

                // net present value of first rotation
                // Harvest and standing volumes are in board feet and prices are in MBF, hence multiplications by 0.001.
                // TODO: support per species pricing
                float firstRotationPresentValue = -this.Objective.ReforestationCostPerAcre;
                for (int periodIndex = 1; periodIndex < trajectory.HarvestVolumesByPeriod.Length; ++periodIndex)
                {
                    float thinVolumeInBoardFeet = trajectory.HarvestVolumesByPeriod[periodIndex];
                    if (thinVolumeInBoardFeet > 0.0)
                    {
                        int thinAge = trajectory.PeriodZeroAgeInYears + trajectory.PeriodLengthInYears * (periodIndex - 1);
                        firstRotationPresentValue += this.Objective.GetPresentValueOfThinScribner(thinVolumeInBoardFeet, thinAge);
                    }
                }

                // TODO: check if earlier final harvest provides higher NPV
                int rotationLength = trajectory.GetRotationLength();
                firstRotationPresentValue += this.Objective.GetPresentValueOfRegenerationHarvestScribner(trajectory.StandingVolumeByPeriod[^1], rotationLength);
                float landExpectationValue = this.Objective.FirstRotationToLandExpectationValue(firstRotationPresentValue, rotationLength);

                // convert from US$/ac to k$/ac
                objectiveFunction = 0.001F * landExpectationValue;
            }
            else
            {
                // direct volume addition
                objectiveFunction = 0.0F;
                for (int periodIndex = 1; periodIndex < trajectory.HarvestVolumesByPeriod.Length; ++periodIndex)
                {
                    objectiveFunction += trajectory.HarvestVolumesByPeriod[periodIndex];
                }
                objectiveFunction += trajectory.StandingVolumeByPeriod[^1];
                if (trajectory.VolumeUnits == VolumeUnits.ScribnerBoardFeetPerAcre)
                {
                    objectiveFunction *= 0.001F;
                }
            }

            return objectiveFunction;
        }

        public virtual HeuristicParameters GetParameters()
        {
            return null;
        }

        public void RandomizeTreeSelection(float proportionalPercentage)
        {
            if ((proportionalPercentage < 0.0F) || (proportionalPercentage > 100.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(proportionalPercentage));
            }

            IHarvest harvest = this.CurrentTrajectory.Configuration.Treatments.Harvests.FirstOrDefault();
            if ((harvest == null) || (harvest is ThinByIndividualTreeSelection == false))
            {
                // attempt to randomize an individual tree selection without a corresponding harvest present
                throw new ArgumentOutOfRangeException(nameof(this.CurrentTrajectory.Configuration.Treatments.Harvests));
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

            IHarvest harvest = this.CurrentTrajectory.Configuration.Treatments.Harvests.FirstOrDefault();
            if ((harvest == null) || (harvest is ThinByIndividualTreeSelection == false))
            {
                // attempt to randomize an individual tree selection without a corresponding harvest present
                throw new ArgumentOutOfRangeException(nameof(this.CurrentTrajectory.Configuration.Treatments.Harvests));
            }

            int maxPerturbationIndex = (int)(perturbBy * eliteSolutions.TreeCount) + 1;
            int[] treeSelection = eliteSolutions.IndividualTreeSelections[0];
            int[] treeIndices = this.CreateSequentialArray(eliteSolutions.TreeCount);
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
