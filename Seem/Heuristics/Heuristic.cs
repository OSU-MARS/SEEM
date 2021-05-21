using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public abstract class Heuristic : PseudorandomizingTask
    {
        public List<float> AcceptedObjectiveFunctionByMove { get; protected set; }
        public float BestObjectiveFunction { get; protected set; }
        public OrganonStandTrajectory BestTrajectory { get; private init; }
        public List<float> CandidateObjectiveFunctionByMove { get; protected set; }
        public OrganonStandTrajectory CurrentTrajectory { get; private init; }
        public RunParameters RunParameters { get; private init; }

        protected Heuristic(OrganonStand stand, OrganonConfiguration organonConfiguration, HeuristicParameters heuristicParameters, RunParameters runParameters)
        {
            this.AcceptedObjectiveFunctionByMove = new List<float>();
            this.BestObjectiveFunction = Single.MinValue; // for now, assume maximization of objective function

            this.BestTrajectory = new OrganonStandTrajectory(stand, organonConfiguration, heuristicParameters.TimberValue, runParameters.PlanningPeriods)
            {
                Heuristic = this
            };

            this.CandidateObjectiveFunctionByMove = new List<float>();

            this.CurrentTrajectory = new OrganonStandTrajectory(this.BestTrajectory);
            this.CurrentTrajectory.Name = this.CurrentTrajectory.Name + "Current";

            this.RunParameters = runParameters;
        }

        public abstract string GetName();

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

        public abstract HeuristicPerformanceCounters Run(HeuristicSolutionPosition position, HeuristicSolutionIndex solutionIndex);
    }

    public abstract class Heuristic<TParameters> : Heuristic where TParameters : HeuristicParameters
    {
        public TParameters HeuristicParameters { get; private init; }

        public Heuristic(OrganonStand stand, OrganonConfiguration organonConfiguration, TParameters heuristicParameters, RunParameters runParameters)
            : base(stand, organonConfiguration, heuristicParameters, runParameters)
        {
            if ((heuristicParameters.ConstructionRandomness < 0.0F) || (heuristicParameters.ConstructionRandomness > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(heuristicParameters), "Construction greediness is not between 0 and 1, inclusive.");
            }
            if ((heuristicParameters.InitialThinningProbability < 0.0F) || (heuristicParameters.InitialThinningProbability > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(heuristicParameters), "Probability of thinning an individual tree is not between 0 and 1, inclusive.");
            }

            this.HeuristicParameters = heuristicParameters;
        }

        // exposed as a distinct method for AutocorrelatedWalk
        protected void ConstructTreeSelection(float constructionRandomness)
        {
            // check if there is a thinning to randomize
            IList<int> thinningPeriods = this.CurrentTrajectory.Configuration.Treatments.GetValidThinningPeriods();
            if (thinningPeriods[0] != Constant.NoHarvestPeriod)
            {
                throw new NotSupportedException("First thinning selection is a harvest. Expected it to be the no harvest option.");
            }
            if (thinningPeriods.Count == 1)
            {
                // ensure no trees are selected for thinning since no thinning is specified
                // If needed, checking can be done for conflict between removing trees from thinning and construction greediness. At least for now, the 
                // scheduling of thins is treated as an override to greediness.
                this.CurrentTrajectory.DeselectAllTrees();
                return;
            }
            thinningPeriods.RemoveAt(0);

            if (this.HeuristicParameters.ConstructionRandomness == Constant.GraspDefault.FullyGreedyConstruction)
            {
                // nothing to do
                // Could return sooner but it seems best not to bypass argument checking.
                return;
            }

            // randomize tree selection at level indicated by constructionRandomness
            int initialTreeRecordCount = this.CurrentTrajectory.GetInitialTreeRecordCount();
            float thinIndexScalingFactor = (thinningPeriods.Count - Constant.RoundTowardsZeroTolerance) / this.HeuristicParameters.InitialThinningProbability;
            for (int treeIndex = 0; treeIndex < initialTreeRecordCount; ++treeIndex)
            {
                if (constructionRandomness != 1.0F)
                {
                    float modificationProbability = this.GetPseudorandomByteAsProbability();
                    if (modificationProbability < constructionRandomness)
                    {
                        continue;
                    }
                }

                int thinningPeriod = Constant.NoHarvestPeriod;
                float harvestProbability = this.GetPseudorandomByteAsProbability();
                if (harvestProbability < this.HeuristicParameters.InitialThinningProbability)
                {
                    // probability falls into the harvest fraction, for now choose equally among available harvest periods
                    // TODO: support unequal harvest period probabilities
                    int periodIndex = (int)(thinIndexScalingFactor * harvestProbability);
                    thinningPeriod = thinningPeriods[periodIndex];
                }
                this.CurrentTrajectory.SetTreeSelection(treeIndex, thinningPeriod);
            }
        }

        protected void ConstructTreeSelection(HeuristicSolutionPosition position, HeuristicSolutionIndex solutionIndex)
        {
            // if there is no existing solution, default to fully random construction
            HeuristicSolutionPool existingSolutions = solutionIndex[position];
            float constructionRandomness = existingSolutions.Highest == null ? Constant.GraspDefault.FullyRandomConstruction : this.HeuristicParameters.ConstructionRandomness;

            // if construction isn't fully random, clone the best existing solution for now
            if (constructionRandomness != Constant.GraspDefault.FullyRandomConstruction)
            {
                // default to maximization
                // If needed, a this.IsMaxmizing property or such could be added.
                this.CurrentTrajectory.CopyFrom(existingSolutions.Highest!.BestTrajectory);
            }

            this.ConstructTreeSelection(constructionRandomness);
        }

        protected virtual void EvaluateInitialSelection(int moveCapacity, HeuristicPerformanceCounters perfCounters)
        {
            this.AcceptedObjectiveFunctionByMove.Capacity = moveCapacity;
            this.CandidateObjectiveFunctionByMove.Capacity = moveCapacity;

            perfCounters.GrowthModelTimesteps += this.CurrentTrajectory.Simulate();
            this.BestTrajectory.CopyFrom(this.CurrentTrajectory);
            this.BestObjectiveFunction = this.GetObjectiveFunction(this.CurrentTrajectory);
            this.AcceptedObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
            this.CandidateObjectiveFunctionByMove.Add(this.BestObjectiveFunction);
        }

        public float GetObjectiveFunction(StandTrajectory trajectory)
        {
            Debug.Assert(trajectory.PeriodLengthInYears > 0);

            // find objective function value
            // Volume objective functions are in m³/ha or MBF/ac.
            float objectiveFunction;
            if ((this.RunParameters.TimberObjective == TimberObjective.LandExpectationValue) ||
                (this.RunParameters.TimberObjective == TimberObjective.NetPresentValue))
            {
                // net present value of first rotation
                // Harvest and standing volumes are in board feet and prices are in MBF, hence multiplications by 0.001.
                // TODO: support per species pricing
                objectiveFunction = trajectory.GetNetPresentValue(this.RunParameters.DiscountRate);

                if (this.RunParameters.TimberObjective == TimberObjective.LandExpectationValue)
                {
                    int rotationLengthInYears = trajectory.GetRotationLength();
                    float presentToFutureConversionFactor = TimberValue.GetAppreciationFactor(this.RunParameters.DiscountRate, rotationLengthInYears);
                    float landExpectationValue = presentToFutureConversionFactor * objectiveFunction / (presentToFutureConversionFactor - 1.0F);
                    objectiveFunction = landExpectationValue;
                }

                // convert from US$/ha to USk$/ha
                objectiveFunction *= 0.001F;
            }
            else if (this.RunParameters.TimberObjective == TimberObjective.ScribnerVolume)
            {
                // direct volume addition
                objectiveFunction = 0.0F;
                for (int periodIndex = 1; periodIndex < trajectory.PlanningPeriods; ++periodIndex)
                {
                    objectiveFunction += trajectory.ThinningVolume.GetScribnerTotal(periodIndex);
                }
                objectiveFunction += trajectory.StandingVolume.GetScribnerTotal(trajectory.PlanningPeriods - 1);
            }
            else
            {
                throw new NotSupportedException("Unhandled timber objective " + this.RunParameters.TimberObjective + ".");
            }

            return objectiveFunction;
        }
    }
}
