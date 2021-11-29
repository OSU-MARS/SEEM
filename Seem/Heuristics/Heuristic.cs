using Mars.Seem.Optimization;
using Mars.Seem.Organon;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Mars.Seem.Heuristics
{
    public abstract class Heuristic : PseudorandomizingTask
    {
        protected OrganonStandTrajectory?[,] BestTrajectoryByRotationAndScenario { get; private init; }

        public float ConstructionGreediness { get; protected set; }
        public OrganonStandTrajectory CurrentTrajectory { get; private init; } // could be protected get but left public for test access
        public FinancialOptimizationTrajectory FinancialValue { get; protected set; }
        public RunParameters RunParameters { get; private init; }

        protected Heuristic(OrganonStand stand, RunParameters runParameters, bool evaluatesAcrossRotationsAndFinancialScenarios)
        {
            if ((runParameters.RotationLengths.Count < 1) || (runParameters.Financial.Count < 1))
            {
                throw new ArgumentOutOfRangeException(nameof(runParameters));
            }

            int financialScenarioCapacity = 1;
            int lastSimulationPeriod = runParameters.MaximizeForPlanningPeriod;
            int rotationLengthCapacity = 1;
            if (evaluatesAcrossRotationsAndFinancialScenarios)
            {
                financialScenarioCapacity = runParameters.Financial.Count;
                lastSimulationPeriod = runParameters.RotationLengths.Max();
                rotationLengthCapacity = runParameters.RotationLengths.Count;
            }

            // best trajectories are set as runs complete so elements are left as null
            this.BestTrajectoryByRotationAndScenario = new OrganonStandTrajectory[rotationLengthCapacity, financialScenarioCapacity];
            this.ConstructionGreediness = Constant.Grasp.NoConstruction;

            this.CurrentTrajectory = new(stand, runParameters.OrganonConfiguration, runParameters.TreeVolume, lastSimulationPeriod);
            this.CurrentTrajectory.Treatments.CopyFrom(runParameters.Treatments);

            this.FinancialValue = new(rotationLengthCapacity, financialScenarioCapacity, runParameters.MoveCapacity);
            this.RunParameters = runParameters;
        }

        protected void CopyTreeGrowthToBestTrajectory(StandTrajectoryCoordinate coordinate, OrganonStandTrajectory trajectory)
        {
            if (this.TryGetBestTrajectory(coordinate, out OrganonStandTrajectory? bestTrajectory) == false)
            {
                this.BestTrajectoryByRotationAndScenario[coordinate.RotationIndex, coordinate.FinancialIndex] = trajectory.Clone();
            }
            else
            {
                bestTrajectory.CopyTreeGrowthFrom(trajectory);
            }
        }

        public OrganonStandTrajectory GetBestTrajectory(StandTrajectoryCoordinate coordinate)
        {
            if (this.TryGetBestTrajectory(coordinate, out OrganonStandTrajectory? bestTrajectory) == false)
            {
                throw new InvalidOperationException("Best trajectory for rotation index " + coordinate.RotationIndex + " and financial scenario index " + coordinate.FinancialIndex + " is null.");
            }
            return bestTrajectory;
        }

        public abstract string GetName();

        public float GetFinancialValue(StandTrajectory trajectory, int financialIndex) // public for test code access
        {
            return this.GetFinancialValue(trajectory, financialIndex, trajectory.PlanningPeriods - 1);
        }

        protected float GetFinancialValue(StandTrajectory trajectory, int financialIndex, int endOfRotationPeriod)
        {
            Debug.Assert(trajectory.PeriodLengthInYears > 0);

            switch(this.RunParameters.TimberObjective)
            {
                case TimberObjective.LandExpectationValue:
                    // convert from US$/ha to USk$/ha
                    return 0.001F * this.RunParameters.Financial.GetLandExpectationValue(trajectory, financialIndex, endOfRotationPeriod);
                // net present value of first rotation
                case TimberObjective.NetPresentValue:
                    return 0.001F * this.RunParameters.Financial.GetNetPresentValue(trajectory, financialIndex, endOfRotationPeriod);
                // TODO: move this out of financial objective calculations
                case TimberObjective.ScribnerVolume:
                    // direct volume addition
                    float scribnerVolumeInMbf = 0.0F;
                    foreach (int periodIndex in trajectory.Treatments.GetThinningPeriods())
                    {
                        trajectory.RecalculateThinningVolumeIfNeeded(periodIndex);
                        scribnerVolumeInMbf += trajectory.GetTotalScribnerVolumeThinned(periodIndex);
                    }
                    trajectory.RecalculateStandingVolumeIfNeeded(endOfRotationPeriod);
                    scribnerVolumeInMbf += trajectory.GetTotalStandingScribnerVolume(endOfRotationPeriod);
                    return scribnerVolumeInMbf;
                default:
                    throw new NotSupportedException("Unhandled timber objective " + this.RunParameters.TimberObjective + ".");
            }
        }

        public virtual HeuristicMoveLog? GetMoveLog()
        {
            return null;
        }

        public abstract HeuristicParameters GetParameters();
        public abstract PrescriptionPerformanceCounters Run(StandTrajectoryCoordinate coordinate, HeuristicStandTrajectories trajectories);

        protected bool TryGetBestTrajectory(StandTrajectoryCoordinate coordinate, [NotNullWhen(true)] out OrganonStandTrajectory? bestTrajectory)
        {
            bestTrajectory = this.BestTrajectoryByRotationAndScenario[coordinate.RotationIndex, coordinate.FinancialIndex];
            return bestTrajectory != null;
        }
    }

    public abstract class Heuristic<TParameters> : Heuristic where TParameters : HeuristicParameters
    {
        protected TParameters HeuristicParameters { get; private init; }

        public Heuristic(OrganonStand stand, TParameters heuristicParameters, RunParameters runParameters, bool evaluatesAcrossRotationsAndFinancialScenarios)
            : base(stand, runParameters, evaluatesAcrossRotationsAndFinancialScenarios)
        {
            if ((heuristicParameters.MinimumConstructionGreediness < Constant.Grasp.FullyRandomConstructionForMaximization) || (heuristicParameters.MinimumConstructionGreediness > Constant.Grasp.FullyGreedyConstructionForMaximization))
            {
                throw new ArgumentOutOfRangeException(nameof(heuristicParameters), "Construction greediness is not between 0 and 1, inclusive.");
            }
            if ((heuristicParameters.InitialThinningProbability < 0.0F) || (heuristicParameters.InitialThinningProbability > 1.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(heuristicParameters), "Probability of thinning an individual tree is not between 0 and 1, inclusive.");
            }

            this.HeuristicParameters = heuristicParameters;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="constructionGreediness"></param>
        /// <returns>Number of trees randomized. If a randomization does not change a tree's selection it is still counted.</returns>
        /// <remarks>Exposed as a distinct method for AutocorrelatedWalk.</remarks>
        protected int ConstructTreeSelection(float constructionGreediness)
        {
            if ((constructionGreediness < Constant.Grasp.FullyRandomConstructionForMaximization) || 
                (constructionGreediness > Constant.Grasp.FullyGreedyConstructionForMaximization))
            {
                throw new ArgumentOutOfRangeException(nameof(constructionGreediness));
            }

            // check if there is a thinning to randomize
            List<int> thinningPeriods = this.CurrentTrajectory.Treatments.GetThinningPeriods();
            if (thinningPeriods.Count == 0)
            {
                // ensure no trees are selected for thinning since no thinning is specified
                // If needed, checking can be done for conflict between removing trees from thinning and construction greediness. At least for now, the 
                // scheduling of thins is treated as an override to greediness.
                this.CurrentTrajectory.DeselectAllTrees();
                return 0;
            }

            if (this.HeuristicParameters.MinimumConstructionGreediness == Constant.Grasp.FullyGreedyConstructionForMaximization)
            {
                // nothing to do
                // Could return sooner but it seems best not to bypass argument checking.
                return 0;
            }

            // determine likelihood of final harvest
            // If this.HeuristicParameters.InitialThinningProbability isn't set to 1 / (nThins + 1) this is bias randomized towards or
            // against retention to final harvest. This is expected on initial construction but presumably undesirable when modifying
            // elite solutions.
            float finalHarvestProbability = this.HeuristicParameters.InitialThinningProbability;
            if (constructionGreediness != Constant.Grasp.FullyRandomConstructionForMaximization)
            {
                finalHarvestProbability = 1.0F / (thinningPeriods.Count + 1);
            }

            // randomize tree selection at level indicated by constructionGreediness
            // If construction is primarily random, iterate through all trees and check each for randomization. This randomizes an inexact
            // number of trees but will likely be reasonably close to the requested number since a modification of a sufficient portion
            // of a presumably substantial number of trees has been requested.
            // If construction is nearly greedy, find the number of trees requested and perform that many randomizations. This is not
            // exact either but makes the number of randomizations performed a more consistent function of α.
            thinningPeriods.Sort(); // ensure thins are listed in chronological order for diameter checking in TryRandomizeTreeSelection()
            int initialTreeRecordCount = this.CurrentTrajectory.GetInitialTreeRecordCount();
            float treeIndexScalingFactor = (initialTreeRecordCount - Constant.RoundTowardsZeroTolerance) / UInt16.MaxValue;
            float treesToRandomize = initialTreeRecordCount * (1.0F - constructionGreediness);
            int treeSelectionsRandomized = 0; // for now, count randomizations which don't change a tree's harvest period as randomizations
            if (treesToRandomize < 10.0F)
            {
                int integerTreesToRandomize = (int)treesToRandomize;
                for (; treeSelectionsRandomized <= integerTreesToRandomize; ++treeSelectionsRandomized)
                {
                    // for now, assume sufficient trees that the probability of repeatedly randomizing a tree is unimportant
                    // If needed, this can be changed to sampling without replacement.
                    int uncompactedTreeIndex = (int)(treeIndexScalingFactor * this.Pseudorandom.GetTwoPseudorandomBytesAsFloat());
                    this.RandomizeTreeSelection(uncompactedTreeIndex, thinningPeriods, finalHarvestProbability);
                }

                float fractionalTreesToRandomize = treesToRandomize - integerTreesToRandomize;
                if (this.Pseudorandom.GetPseudorandomByteAsProbability() < fractionalTreesToRandomize)
                {
                    int uncompactedTreeIndex = (int)(treeIndexScalingFactor * this.Pseudorandom.GetTwoPseudorandomBytesAsFloat());
                    this.RandomizeTreeSelection(uncompactedTreeIndex, thinningPeriods, finalHarvestProbability);
                    ++treeSelectionsRandomized;
                }
            }
            else
            {
                for (int uncompactedTreeIndex = 0; uncompactedTreeIndex < initialTreeRecordCount; ++uncompactedTreeIndex)
                {
                    if (constructionGreediness != Constant.Grasp.FullyRandomConstructionForMaximization)
                    {
                        float modificationProbability = this.Pseudorandom.GetPseudorandomByteAsProbability();
                        if (modificationProbability < constructionGreediness)
                        {
                            continue;
                        }
                    }

                    this.RandomizeTreeSelection(uncompactedTreeIndex, thinningPeriods, finalHarvestProbability);
                    ++treeSelectionsRandomized;
                }
            }

            return treeSelectionsRandomized;
        }

        protected int ConstructTreeSelection(StandTrajectoryCoordinate coordinate, HeuristicStandTrajectories trajectories)
        {
            // attempt to find an existing solution with the same set of thinning timings
            // If found, the existing solution's discount rate and number of planning periods may differ.
            if (trajectories.TryGetSelfOrFindNearestNeighbor(coordinate, out SilviculturalPrescriptionPool? existingSolutions, out StandTrajectoryCoordinate? positionOfExistingSolutions))
            {
                IndividualTreeSelectionBySpecies eliteTreeSelection = existingSolutions.GetRandomEliteTreeSelection(); // throws if pool is empty
                this.CurrentTrajectory.CopyTreeSelectionFrom(eliteTreeSelection);

                // assuming an existing elite solution at this position has reached first order convergence, must randomize at least two trees
                // to potentially shift to some other domain of attraction
                // The actual number of trees randomized is nondeterministic, the construction process may not necessarily randomize trees to
                // a different harvest timing, and the size of the domain of attraction is unknown. Therefore, set the minimum randomization to
                // be several trees more than the minimum of two.
                int initialTreeRecordCount = this.CurrentTrajectory.GetInitialTreeRecordCount();
                float maximumConstructionGreediness = (initialTreeRecordCount - Constant.Grasp.MinimumTreesRandomized) / (float)initialTreeRecordCount;
                if (maximumConstructionGreediness <= this.HeuristicParameters.MinimumConstructionGreediness)
                {
                    // fall back to permitting fully greedy construction if only a few trees are present or when the minimum greediness is
                    // especially high
                    maximumConstructionGreediness = Constant.Grasp.FullyGreedyConstructionForMaximization;
                }
                if (coordinate == positionOfExistingSolutions)
                {
                    // if one elite solution is available, begin this search with semi-greeding construction
                    this.ConstructionGreediness = trajectories.GraspReactivity.GetConstructionGreediness(this.HeuristicParameters.MinimumConstructionGreediness, maximumConstructionGreediness);
                    // TODO: if two or more elite solutions are available, within position path relinking is possible
                    // this.ConstructionGreediness = Constant.Grasp.FullyRandomConstructionForMaximization;
                }
                else
                {
                    // default to minimal randomization since no solution has yet been accepted at this position
                    // Fully greedy construction is suitable the first time an existing elite solution is evaluated at a new position.
                    // However, within the current codebase it's not known how many other threads may have already initiated searches at this
                    // position, may initiate searches before this search completes, and which existing elite solutions from may currenty be
                    // under evaluation. For now, minimal randomization is therefore applied to reduce the probability of performing duplicate
                    // searches.
                    this.ConstructionGreediness = maximumConstructionGreediness;
                    // this.ConstructionGreediness = this.HeuristicParameters.MinimumConstructionGreediness;
                    // this.ConstructionGreediness = Constant.Grasp.FullyGreedyConstructionForMaximization;
                    // this.ConstructionGreediness = Constant.Grasp.FullyRandomConstructionForMaximization;

                    // sync prescription to new thinning timings
                    if (coordinate.FirstThinPeriodIndex != positionOfExistingSolutions.FirstThinPeriodIndex)
                    {
                        int currentThinPeriod = trajectories.FirstThinPeriods[coordinate.FirstThinPeriodIndex];
                        int existingSolutionThinPeriod = trajectories.FirstThinPeriods[positionOfExistingSolutions.FirstThinPeriodIndex];
                        this.CurrentTrajectory.ChangeThinningPeriod(existingSolutionThinPeriod, currentThinPeriod);
                    }
                    if (coordinate.SecondThinPeriodIndex != positionOfExistingSolutions.SecondThinPeriodIndex)
                    {
                        int currentThinPeriod = trajectories.SecondThinPeriods[coordinate.SecondThinPeriodIndex];
                        int existingSolutionThinPeriod = trajectories.SecondThinPeriods[positionOfExistingSolutions.SecondThinPeriodIndex];
                        this.CurrentTrajectory.ChangeThinningPeriod(existingSolutionThinPeriod, currentThinPeriod);
                    }
                    if (coordinate.ThirdThinPeriodIndex != positionOfExistingSolutions.ThirdThinPeriodIndex)
                    {
                        int currentThinPeriod = trajectories.ThirdThinPeriods[coordinate.ThirdThinPeriodIndex];
                        int existingSolutionThinPeriod = trajectories.ThirdThinPeriods[positionOfExistingSolutions.ThirdThinPeriodIndex];
                        this.CurrentTrajectory.ChangeThinningPeriod(existingSolutionThinPeriod, currentThinPeriod);
                    }
                }
            }
            else
            {
                // default to fully random construction if no solution has yet been found for this combination of thinnings
                this.ConstructionGreediness = Constant.Grasp.FullyRandomConstructionForMaximization;
            }

            // check harvest periods match the position this tree selection is being generated for
            trajectories.VerifyStandEntries(this.CurrentTrajectory, coordinate);

            return this.ConstructTreeSelection(this.ConstructionGreediness);
        }

        protected virtual float EvaluateInitialSelection(StandTrajectoryCoordinate coordinate, int moveCapacity, PrescriptionPerformanceCounters perfCounters)
        {
            this.FinancialValue.SetMoveCapacity(moveCapacity);

            perfCounters.GrowthModelTimesteps += this.CurrentTrajectory.Simulate();
            Debug.Assert(this.BestTrajectoryByRotationAndScenario[Constant.HeuristicDefault.CoordinateIndex, Constant.HeuristicDefault.CoordinateIndex] == null);
            this.BestTrajectoryByRotationAndScenario[Constant.HeuristicDefault.CoordinateIndex, Constant.HeuristicDefault.CoordinateIndex] = this.CurrentTrajectory.Clone();

            float financialValue = this.GetFinancialValue(this.CurrentTrajectory, coordinate.FinancialIndex);
            this.FinancialValue.TryAddMove(coordinate, financialValue, financialValue);
            return financialValue;
        }

        public override TParameters GetParameters()
        {
            return this.HeuristicParameters;
        }

        protected int GetOneOptCandidateRandom(int currentHarvestPeriod, IList<int> harvestPeriods)
        {
            if (harvestPeriods.Count == 2)
            {
                int candidateHarvestPeriod = currentHarvestPeriod == harvestPeriods[0] ? harvestPeriods[1] : harvestPeriods[0];
                return candidateHarvestPeriod;
            }

            if (harvestPeriods.Count == 3)
            {
                // treat harvest periods as a circular buffer and generate first candidate
                bool incrementIndex = this.Pseudorandom.GetPseudorandomByteAsProbability() < 0.5F;
                int candidateHarvestPeriod;
                if (currentHarvestPeriod == harvestPeriods[0])
                {
                    candidateHarvestPeriod = incrementIndex ? harvestPeriods[2] : harvestPeriods[1];
                }
                else if (currentHarvestPeriod == harvestPeriods[1])
                {
                    candidateHarvestPeriod = incrementIndex ? harvestPeriods[0] : harvestPeriods[2];
                }
                else
                {
                    candidateHarvestPeriod = incrementIndex ? harvestPeriods[0] : harvestPeriods[1]; // currentHarvestPeriod == harvestPeriods[2]
                }
                return candidateHarvestPeriod;
            }

            throw new NotSupportedException("More than two thins is not currently supported.");
        }

        // for now, if a tree is too large to be eligible for any thin checking it is still considered a randomization
        protected void RandomizeTreeSelection(int uncompactedTreeIndex, IList<int> thinningPeriods, float finalHarvestProbability)
        {
            int harvestPeriod = Constant.RegenerationHarvestPeriod;
            float harvestProbability = this.Pseudorandom.GetPseudorandomByteAsProbability();
            if (harvestProbability < finalHarvestProbability)
            {
                // TODO: support unequal harvest period probabilities
                float thinIndexScalingFactor = (thinningPeriods.Count - Constant.RoundTowardsZeroTolerance) / finalHarvestProbability;
                int thinIndex = (int)(thinIndexScalingFactor * harvestProbability);
                harvestPeriod = thinningPeriods[thinIndex];
            }

            this.CurrentTrajectory.SetTreeSelection(uncompactedTreeIndex, harvestPeriod);
        }
    }
}
