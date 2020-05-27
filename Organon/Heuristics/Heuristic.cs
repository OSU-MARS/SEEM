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
        public OrganonStandTrajectory BestTrajectory { get; private set; }
        public Dictionary<int, StandTrajectory> BestTrajectoryByMove { get; private set; }
        public int ChainFrom { get; set; }
        public OrganonStandTrajectory CurrentTrajectory { get; private set; }
        public Objective Objective { get; private set; }
        public List<float> ObjectiveFunctionByMove { get; protected set; }

        protected Heuristic(OrganonStand stand, OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective)
        {
            this.BestTrajectory = new OrganonStandTrajectory(stand, organonConfiguration, planningPeriods, objective.VolumeUnits);
            this.BestTrajectoryByMove = new Dictionary<int, StandTrajectory>();
            this.ChainFrom = -1;
            this.Objective = objective;

            this.BestTrajectory.Simulate();
            this.BestObjectiveFunction = this.GetObjectiveFunction(this.BestTrajectory);
            this.CurrentTrajectory = new OrganonStandTrajectory(this.BestTrajectory);

            this.BestTrajectory.Name = this.BestTrajectory.Name + "Best";
            this.BestTrajectory.Heuristic = this;
            this.CurrentTrajectory.Name = this.CurrentTrajectory.Name + "Current";
            this.CurrentTrajectory.Heuristic = this;
        }

        public virtual void CopySelectionsFrom(StandTrajectory trajectory)
        {
            if (trajectory.TreeSelectionChangedSinceLastSimulation)
            {
                throw new ArgumentOutOfRangeException(nameof(trajectory));
            }

            this.BestObjectiveFunction = this.GetObjectiveFunction(trajectory);
            this.BestTrajectory.CopySelectionsFrom(trajectory);
            this.BestTrajectoryByMove.Clear();
            this.CurrentTrajectory.CopySelectionsFrom(trajectory);
            this.ObjectiveFunctionByMove.Clear();
            this.ObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
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

        public virtual string GetParameterHeaderForCsv()
        {
            return null;
        }

        public virtual string GetParametersForCsv()
        {
            return null;
        }

        public void RandomizeSelections(float selectionProbability)
        {
            if ((selectionProbability < 0.0F) || (selectionProbability > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(selectionProbability));
            }

            IHarvest harvest = this.CurrentTrajectory.Configuration.Treatments.Harvests.FirstOrDefault();
            if ((harvest == null) || (harvest is ThinByIndividualTreeSelection == false))
            {
                // attempt to randomize an individual tree selection without a corresponding harvest present
                throw new ArgumentOutOfRangeException(nameof(this.CurrentTrajectory.Configuration.Treatments.Harvests));
            }

            int initialTreeRecordCount = this.GetInitialTreeRecordCount();
            float unityScalingFactor = 1.0F / byte.MaxValue;
            if (this.Objective.HarvestPeriodSelection == HarvestPeriodSelection.All)
            {
                float harvestPeriodScalingFactor = (this.CurrentTrajectory.HarvestPeriods - Constant.RoundTowardsZeroTolerance) / selectionProbability;
                for (int treeIndex = 0; treeIndex < initialTreeRecordCount; ++treeIndex)
                {
                    float treeProbability = unityScalingFactor * this.GetPseudorandomByteAsFloat();
                    if (treeProbability < selectionProbability)
                    {
                        int harvestPeriod = (int)(harvestPeriodScalingFactor * treeProbability);
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
                    int harvestPeriod = unityScalingFactor * this.GetPseudorandomByteAsFloat() < selectionProbability ? this.CurrentTrajectory.HarvestPeriods - 1 : 0;
                    this.CurrentTrajectory.SetTreeSelection(treeIndex, harvestPeriod);
                }
            }
            else
            {
                throw new NotSupportedException(String.Format("Unhandled harvest period selection {0}.", this.Objective.HarvestPeriodSelection));
            }

            this.CurrentTrajectory.Simulate();
            this.BestTrajectory.CopyFrom(this.CurrentTrajectory);

            this.BestObjectiveFunction = this.GetObjectiveFunction(this.CurrentTrajectory);
            if (this.ObjectiveFunctionByMove.Count > 0)
            {
                this.ObjectiveFunctionByMove[0] = this.BestObjectiveFunction;
            }
        }

        public abstract TimeSpan Run();
    }
}
