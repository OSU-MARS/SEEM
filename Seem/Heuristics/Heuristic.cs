using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Ferm.Heuristics
{
    public abstract class Heuristic : PseudorandomizingTask
    {
        public OrganonStandTrajectory?[,] BestTrajectoryByRotationAndRate { get; private init; }
        public OrganonStandTrajectory CurrentTrajectory { get; private init; } // could be protected but left public for test access
        public FinancialValueTrajectory FinancialValue { get; protected set; }
        public RunParameters RunParameters { get; private init; }

        protected Heuristic(OrganonStand stand, RunParameters runParameters, bool evaluatesAcrossRotationsAndDiscountRates)
        {
            if ((runParameters.RotationLengths.Count < 1) || (runParameters.DiscountRates.Count < 1))
            {
                throw new ArgumentOutOfRangeException(nameof(runParameters));
            }

            int discountRateCapacity = 1;
            int lastSimulationPeriod = runParameters.MaximizeForPlanningPeriod;
            int rotationLengthCapacity = 1;
            if (evaluatesAcrossRotationsAndDiscountRates)
            {
                discountRateCapacity = runParameters.DiscountRates.Count;
                lastSimulationPeriod = runParameters.RotationLengths.Max();
                rotationLengthCapacity = runParameters.RotationLengths.Count;
            }

            this.BestTrajectoryByRotationAndRate = new OrganonStandTrajectory[rotationLengthCapacity, discountRateCapacity];
            OrganonStandTrajectory? trajectoryToCloneToCurrent = null;
            for (int rotationIndex = 0; rotationIndex < rotationLengthCapacity; ++rotationIndex)
            {
                if (evaluatesAcrossRotationsAndDiscountRates)
                {
                    int endOfRotationPeriod = runParameters.RotationLengths[rotationIndex];
                    if (endOfRotationPeriod <= runParameters.LastThinPeriod)
                    {
                        continue; // not a valid rotation length because it doesn't include the last thinning scheduled
                    }
                }

                for (int discountRateIndex = 0; discountRateIndex < discountRateCapacity; ++discountRateIndex)
                {
                    OrganonStandTrajectory trajectory = new(stand, runParameters.OrganonConfiguration, runParameters.TimberValue, lastSimulationPeriod)
                    {
                        Heuristic = this
                    };
                    trajectory.Treatments.CopyFrom(runParameters.Treatments);

                    this.BestTrajectoryByRotationAndRate[rotationIndex, discountRateIndex] = trajectory;
                    trajectoryToCloneToCurrent = trajectory;
                }
            }
            // depending on the thinnings specified this.BestTrajectoryByRotationAndRate[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.DiscountRateIndex]
            // can be null
            if (trajectoryToCloneToCurrent == null)
            {
                throw new ArgumentOutOfRangeException(nameof(runParameters), "No valid rotation lengths found.");
            }

            this.CurrentTrajectory = new OrganonStandTrajectory(trajectoryToCloneToCurrent);
            this.CurrentTrajectory.Name += "Current";

            this.FinancialValue = new(rotationLengthCapacity, discountRateCapacity);
            this.RunParameters = runParameters;
        }

        protected void CopyTreeGrowthToBestTrajectory(OrganonStandTrajectory trajectory)
        {
            this.CopyTreeGrowthToBestTrajectory(trajectory, Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.DiscountRateIndex);
        }

        protected void CopyTreeGrowthToBestTrajectory(OrganonStandTrajectory trajectory, int rotationIndex, int discountRateIndex)
        {
            OrganonStandTrajectory bestTrajectory = this.GetBestTrajectory(rotationIndex, discountRateIndex);
            bestTrajectory.CopyTreeGrowthFrom(trajectory);
        }

        protected OrganonStandTrajectory GetBestTrajectory(int rotationIndex, int discountRateIndex)
        {
            OrganonStandTrajectory? bestTrajectory = this.BestTrajectoryByRotationAndRate[rotationIndex, discountRateIndex];
            Debug.Assert(bestTrajectory != null);
            return bestTrajectory;
        }

        public OrganonStandTrajectory GetBestTrajectoryWithDefaulting(HeuristicResultPosition position)
        {
            if (this.BestTrajectoryByRotationAndRate.Length == 1)
            {
                return this.GetBestTrajectory(Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.DiscountRateIndex);
            }
            return this.GetBestTrajectory(position.RotationIndex, position.DiscountRateIndex);
        }

        public abstract string GetName();

        public float GetFinancialValue(StandTrajectory trajectory, int discountRateIndex) // public for test code access
        {
            return this.GetFinancialValue(trajectory, trajectory.PlanningPeriods - 1, discountRateIndex);
        }

        protected float GetFinancialValue(StandTrajectory trajectory, int endOfRotationPeriodIndex, int discountRateIndex)
        {
            Debug.Assert(trajectory.PeriodLengthInYears > 0);

            // find objective function value
            // Volume objective functions are in m³/ha or MBF/ac.
            float financialValue;
            if ((this.RunParameters.TimberObjective == TimberObjective.LandExpectationValue) ||
                (this.RunParameters.TimberObjective == TimberObjective.NetPresentValue))
            {
                // net present value of first rotation
                // Harvest and standing volumes are in board feet and prices are in MBF, hence multiplications by 0.001.
                // TODO: support per species pricing
                float discountRate = this.RunParameters.DiscountRates[discountRateIndex];
                financialValue = trajectory.GetNetPresentValue(endOfRotationPeriodIndex, discountRate);

                if (this.RunParameters.TimberObjective == TimberObjective.LandExpectationValue)
                {
                    int rotationLengthInYears = trajectory.GetEndOfPeriodAge(endOfRotationPeriodIndex);
                    float presentToFutureConversionFactor = TimberValue.GetAppreciationFactor(discountRate, rotationLengthInYears);
                    float landExpectationValue = presentToFutureConversionFactor * financialValue / (presentToFutureConversionFactor - 1.0F);
                    financialValue = landExpectationValue;
                }

                // convert from US$/ha to USk$/ha
                financialValue *= 0.001F;
            }
            else if (this.RunParameters.TimberObjective == TimberObjective.ScribnerVolume)
            {
                // TODO: move this out of financial objective calculations, left here as legacy support for now 
                // direct volume addition
                financialValue = 0.0F;
                for (int periodIndex = 1; periodIndex < trajectory.PlanningPeriods; ++periodIndex)
                {
                    financialValue += trajectory.ThinningVolume.GetScribnerTotal(periodIndex);
                }
                financialValue += trajectory.StandingVolume.GetScribnerTotal(endOfRotationPeriodIndex);
            }
            else
            {
                throw new NotSupportedException("Unhandled timber objective " + this.RunParameters.TimberObjective + ".");
            }

            return financialValue;
        }

        protected float[,] GetFinancialValueByRotationAndRate(StandTrajectory trajectory)
        {
            float[,] financialValueByDiscountRate = new float[this.RunParameters.RotationLengths.Count, this.RunParameters.DiscountRates.Count];
            for (int rotationIndex = 0; rotationIndex < this.RunParameters.RotationLengths.Count; ++rotationIndex)
            {
                int endOfRotationPeriod = this.RunParameters.RotationLengths[rotationIndex];
                for (int discountRateIndex = 0; discountRateIndex < this.RunParameters.DiscountRates.Count; ++discountRateIndex)
                {
                    financialValueByDiscountRate[rotationIndex, discountRateIndex] = this.GetFinancialValue(trajectory, endOfRotationPeriod, discountRateIndex);
                }
            }
            return financialValueByDiscountRate;
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
        public abstract HeuristicPerformanceCounters Run(HeuristicResultPosition position, HeuristicResults results);
    }

    public abstract class Heuristic<TParameters> : Heuristic where TParameters : HeuristicParameters
    {
        public TParameters HeuristicParameters { get; private init; }

        public Heuristic(OrganonStand stand, TParameters heuristicParameters, RunParameters runParameters, bool evaluatesAcrossRotationsAndDiscountRates)
            : base(stand, runParameters, evaluatesAcrossRotationsAndDiscountRates)
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

        protected int ConstructTreeSelection(HeuristicResultPosition position, HeuristicResults results)
        {
            // default to fully random construction if there is no existing solution
            float constructionGreediness = Constant.Grasp.FullyRandomConstructionForMaximization;
            // attempt to find an existing solution with the same set of thinning timings
            // If found, the existing solution's discount rate and number of planning periods may differ.
            if (results.TryFindSolutionsMatchingThinnings(position, out HeuristicSolutionPool? existingSolutions))
            {
                Debug.Assert(existingSolutions.SolutionsInPool > 0);
                OrganonStandTrajectory eliteSolution = existingSolutions.GetEliteSolution(position);
                this.CurrentTrajectory.CopyTreeGrowthFrom(eliteSolution);
                constructionGreediness = this.HeuristicParameters.ConstructionGreediness;
            }

            return this.ConstructTreeSelection(constructionGreediness);
        }

        protected virtual float EvaluateInitialSelection(int moveCapacity, HeuristicPerformanceCounters perfCounters)
        {
            this.FinancialValue.SetMoveCapacity(moveCapacity);

            perfCounters.GrowthModelTimesteps += this.CurrentTrajectory.Simulate();
            this.BestTrajectoryByRotationAndRate[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.DiscountRateIndex]!.CopyTreeGrowthFrom(this.CurrentTrajectory);

            float financialValue = this.GetFinancialValue(this.CurrentTrajectory, Constant.HeuristicDefault.DiscountRateIndex);
            this.FinancialValue.AddMove(Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.DiscountRateIndex, financialValue, financialValue);
            return financialValue;
        }

        public override HeuristicParameters GetParameters()
        {
            return this.HeuristicParameters;
        }
    }
}
