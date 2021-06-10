using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public abstract class Heuristic : PseudorandomizingTask
    {
        public List<List<float>> AcceptedFinancialValueByDiscountRateAndMove { get; protected set; }
        public OrganonStandTrajectory BestTrajectory { get; private init; }
        public List<List<float>> CandidateFinancialValueByDiscountRateAndMove { get; protected set; }
        public OrganonStandTrajectory CurrentTrajectory { get; private init; }
        public List<float> HighestFinancialValueByDiscountRate { get; protected set; }
        public RunParameters RunParameters { get; private init; }

        protected Heuristic(OrganonStand stand, RunParameters runParameters)
        {
            this.AcceptedFinancialValueByDiscountRateAndMove = new() { new List<float>() };

            this.BestTrajectory = new OrganonStandTrajectory(stand, runParameters.OrganonConfiguration, runParameters.TimberValue, runParameters.LastPlanningPeriod)
            {
                Heuristic = this
            };
            this.BestTrajectory.Treatments.CopyFrom(runParameters.Treatments);

            this.CandidateFinancialValueByDiscountRateAndMove = new() { new List<float>() };

            this.CurrentTrajectory = new OrganonStandTrajectory(this.BestTrajectory);
            this.CurrentTrajectory.Name = this.CurrentTrajectory.Name + "Current";

            this.HighestFinancialValueByDiscountRate = new(runParameters.DiscountRates.Count);
            this.RunParameters = runParameters;

            for (int discountRateIndex = 0; discountRateIndex < runParameters.DiscountRates.Count; ++discountRateIndex)
            {
                this.HighestFinancialValueByDiscountRate.Add(Single.MinValue);
            }
        }

        public abstract string GetName();

        public float GetFinancialValue(StandTrajectory trajectory, int discountRateIndex)
        {
            return this.GetFinancialValue(trajectory, this.RunParameters.DiscountRates[discountRateIndex]);
        }

        protected float GetFinancialValue(StandTrajectory trajectory, float discountRate)
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
                objectiveFunction = trajectory.GetNetPresentValue(discountRate, this.RunParameters.MaximizeForPlanningPeriod);

                if (this.RunParameters.TimberObjective == TimberObjective.LandExpectationValue)
                {
                    int rotationLengthInYears = trajectory.GetEndOfPeriodAge(this.RunParameters.MaximizeForPlanningPeriod);
                    float presentToFutureConversionFactor = TimberValue.GetAppreciationFactor(discountRate, rotationLengthInYears);
                    float landExpectationValue = presentToFutureConversionFactor * objectiveFunction / (presentToFutureConversionFactor - 1.0F);
                    objectiveFunction = landExpectationValue;
                }

                // convert from US$/ha to USk$/ha
                objectiveFunction *= 0.001F;
            }
            else if (this.RunParameters.TimberObjective == TimberObjective.ScribnerVolume)
            {
                // TODO: move this out of financial objective calculations, left here as legacy support for now 
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

        protected List<float> GetFinancialValueByDiscountRate(StandTrajectory trajectory)
        {
            List<float> objectiveFunctionByDiscountRate = new(this.RunParameters.DiscountRates.Count);
            for (int discountRateIndex = 0; discountRateIndex < this.RunParameters.DiscountRates.Count; ++discountRateIndex)
            {
                objectiveFunctionByDiscountRate.Add(this.GetFinancialValue(trajectory, this.RunParameters.DiscountRates[discountRateIndex]));
            }
            return objectiveFunctionByDiscountRate;
        }

        protected int GetOneOptCandidateRandom(int currentHarvestPeriod, IList<int> thinningPeriods)
        {
            Debug.Assert((thinningPeriods.Count == 2) || (thinningPeriods.Count == 3));
            if (thinningPeriods.Count == 2)
            {
                return currentHarvestPeriod == thinningPeriods[0] ? thinningPeriods[1] : thinningPeriods[0];
            }

            bool incrementIndex = this.Pseudorandom.GetPseudorandomByteAsProbability() < 0.5F;
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

        public abstract HeuristicParameters GetParameters();
        public abstract HeuristicPerformanceCounters Run(HeuristicResultPosition position, HeuristicResults solutionIndex);
    }

    public abstract class Heuristic<TParameters> : Heuristic where TParameters : HeuristicParameters
    {
        public TParameters HeuristicParameters { get; private init; }

        public Heuristic(OrganonStand stand, TParameters heuristicParameters, RunParameters runParameters)
            : base(stand, runParameters)
        {
            if ((heuristicParameters.ConstructionGreediness < Constant.Grasp.FullyRandomConstructionForMaximization) || (heuristicParameters.ConstructionGreediness > Constant.Grasp.FullyGreedyConstructionForMaximization))
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
        protected int ConstructTreeSelection(float constructionGreediness)
        {
            if ((constructionGreediness <Constant.Grasp.FullyRandomConstructionForMaximization) || (constructionGreediness > Constant.Grasp.FullyGreedyConstructionForMaximization))
            {
                throw new ArgumentOutOfRangeException(nameof(constructionGreediness));
            }

            // check if there is a thinning to randomize
            IList<int> thinningPeriods = this.CurrentTrajectory.Treatments.GetValidThinningPeriods();
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
                return 0;
            }
            thinningPeriods.RemoveAt(0);

            if (this.HeuristicParameters.ConstructionGreediness == Constant.Grasp.FullyGreedyConstructionForMaximization)
            {
                // nothing to do
                // Could return sooner but it seems best not to bypass argument checking.
                return 0;
            }

            // randomize tree selection at level indicated by constructionGreediness
            int initialTreeRecordCount = this.CurrentTrajectory.GetInitialTreeRecordCount();
            float thinIndexScalingFactor = (thinningPeriods.Count - Constant.RoundTowardsZeroTolerance) / this.HeuristicParameters.InitialThinningProbability;
            int treeSelectionsRandomized = 0;
            for (int treeIndex = 0; treeIndex < initialTreeRecordCount; ++treeIndex)
            {
                if (constructionGreediness != Constant.Grasp.FullyRandomConstructionForMaximization)
                {
                    float modificationProbability = this.Pseudorandom.GetPseudorandomByteAsProbability();
                    if (modificationProbability < constructionGreediness)
                    {
                        continue;
                    }
                }

                int thinningPeriod = Constant.NoHarvestPeriod;
                float harvestProbability = this.Pseudorandom.GetPseudorandomByteAsProbability();
                if (harvestProbability < this.HeuristicParameters.InitialThinningProbability)
                {
                    // probability falls into the harvest fraction, for now choose equally among available harvest periods
                    // TODO: support unequal harvest period probabilities
                    int periodIndex = (int)(thinIndexScalingFactor * harvestProbability);
                    thinningPeriod = thinningPeriods[periodIndex];
                }
                this.CurrentTrajectory.SetTreeSelection(treeIndex, thinningPeriod);
                ++treeSelectionsRandomized;
            }

            return treeSelectionsRandomized;
        }

        protected int ConstructTreeSelection(HeuristicResultPosition position, HeuristicResults solutionIndex)
        {
            // default to fully random construction if there is no existing solution
            float constructionGreediness = Constant.Grasp.FullyRandomConstructionForMaximization;
            // attempt to find an existing solution with the same set of thinning timings
            // If found, the existing solution's discount rate and number of planning periods may differ.
            if (solutionIndex.TryFindSolutionsMatchingThinnings(position, out HeuristicSolutionPool? existingSolutions))
            {
                Debug.Assert(existingSolutions.SolutionsInPool > 0);
                OrganonStandTrajectory eliteSolution = existingSolutions.GetEliteSolution();
                this.CurrentTrajectory.CopyTreeGrowthFrom(eliteSolution);
                constructionGreediness = this.HeuristicParameters.ConstructionGreediness;
            }

            return this.ConstructTreeSelection(constructionGreediness);
        }

        protected virtual void EvaluateInitialSelection(int discountRateIndex, int moveCapacity, HeuristicPerformanceCounters perfCounters)
        {
            this.AcceptedFinancialValueByDiscountRateAndMove[discountRateIndex].Capacity = moveCapacity;
            this.CandidateFinancialValueByDiscountRateAndMove[discountRateIndex].Capacity = moveCapacity;

            perfCounters.GrowthModelTimesteps += this.CurrentTrajectory.Simulate();
            this.BestTrajectory.CopyTreeGrowthFrom(this.CurrentTrajectory);
            float objectiveFunction = this.GetFinancialValue(this.CurrentTrajectory, discountRateIndex);
            this.AcceptedFinancialValueByDiscountRateAndMove[discountRateIndex].Add(objectiveFunction);
            this.CandidateFinancialValueByDiscountRateAndMove[discountRateIndex].Add(objectiveFunction);
            this.HighestFinancialValueByDiscountRate[discountRateIndex] = objectiveFunction;
        }

        public override HeuristicParameters GetParameters()
        {
            return this.HeuristicParameters;
        }
    }
}
