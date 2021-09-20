using Osu.Cof.Ferm.Organon;
using Osu.Cof.Ferm.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Ferm.Heuristics
{
    public abstract class Heuristic : PseudorandomizingTask
    {
        public OrganonStandTrajectory?[,] BestTrajectoryByRotationAndScenario { get; private init; }
        public float ConstructionGreediness { get; protected set; }
        public OrganonStandTrajectory CurrentTrajectory { get; private init; } // could be protected but left public for test access
        public FinancialValueTrajectory FinancialValue { get; protected set; }
        public RunParameters RunParameters { get; private init; }

        protected Heuristic(OrganonStand stand, RunParameters runParameters, bool evaluatesAcrossRotationsAndDiscountRates)
        {
            if ((runParameters.RotationLengths.Count < 1) || (runParameters.Financial.Count < 1))
            {
                throw new ArgumentOutOfRangeException(nameof(runParameters));
            }

            int financialScenarioCapacity = 1;
            int lastSimulationPeriod = runParameters.MaximizeForPlanningPeriod;
            int rotationLengthCapacity = 1;
            if (evaluatesAcrossRotationsAndDiscountRates)
            {
                financialScenarioCapacity = runParameters.Financial.Count;
                lastSimulationPeriod = runParameters.RotationLengths.Max();
                rotationLengthCapacity = runParameters.RotationLengths.Count;
            }

            this.BestTrajectoryByRotationAndScenario = new OrganonStandTrajectory[rotationLengthCapacity, financialScenarioCapacity];
            OrganonStandTrajectory? trajectoryToCloneToCurrent = null;
            for (int rotationIndex = 0; rotationIndex < rotationLengthCapacity; ++rotationIndex)
            {
                if (evaluatesAcrossRotationsAndDiscountRates)
                {
                    int endOfRotationPeriod = runParameters.RotationLengths[rotationIndex];
                    if (endOfRotationPeriod <= runParameters.LastThinPeriod) // if needed, use < instead of <=, see also PrescriptionEnumeration.EvaluateThinningPrescriptions()
                    {
                        continue; // not a valid rotation length because it doesn't include the last thinning scheduled
                    }
                }

                for (int financialIndex = 0; financialIndex < financialScenarioCapacity; ++financialIndex)
                {
                    OrganonStandTrajectory trajectory = new(stand, runParameters.OrganonConfiguration, runParameters.TreeVolume, lastSimulationPeriod)
                    {
                        Heuristic = this
                    };
                    trajectory.Treatments.CopyFrom(runParameters.Treatments);

                    this.BestTrajectoryByRotationAndScenario[rotationIndex, financialIndex] = trajectory;
                    trajectoryToCloneToCurrent = trajectory;
                }
            }
            // depending on the thinnings specified this.BestTrajectoryByRotationAndRate[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.FinancialIndex]
            // can be null
            if (trajectoryToCloneToCurrent == null)
            {
                throw new ArgumentOutOfRangeException(nameof(runParameters), "No valid rotation lengths found.");
            }

            this.CurrentTrajectory = new OrganonStandTrajectory(trajectoryToCloneToCurrent);
            this.CurrentTrajectory.Name += "Current";

            this.ConstructionGreediness = Constant.Grasp.NoConstruction;
            this.FinancialValue = new(rotationLengthCapacity, financialScenarioCapacity, runParameters.MoveCapacity);
            this.RunParameters = runParameters;
        }

        protected void CopyTreeGrowthToBestTrajectory(OrganonStandTrajectory trajectory)
        {
            this.CopyTreeGrowthToBestTrajectory(trajectory, Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.FinancialIndex);
        }

        protected void CopyTreeGrowthToBestTrajectory(OrganonStandTrajectory trajectory, int rotationIndex, int financialIndex)
        {
            OrganonStandTrajectory bestTrajectory = this.GetBestTrajectory(rotationIndex, financialIndex);
            bestTrajectory.CopyTreeGrowthFrom(trajectory);
        }

        public OrganonStandTrajectory FindNearestBestTrajectory(HeuristicResultPosition position)
        {
            // trivial case: there is only one trajectory since this heuristic does not evaluate across rotations and scenarios
            if (this.BestTrajectoryByRotationAndScenario.Length == 1)
            {
                return this.GetBestTrajectory(Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.FinancialIndex);
            }

            // for now, search only within rotation lengths on the expectation all financial scenarios are always evaluated
            // Other assumptions, for now:
            //  - An arbitrarily large change in rotation length remains nearer than a financial scenario increment or decrement.
            //  - At time of writing, the only possibility is short rotations aren't evaluated and it's needful to search only among
            //    longer rotations. For now, rotations searched in the order listed rather than sorting to find the closest rotation
            //    lengths.
            // Can't use a BreadthFirstEnumerator here as the data structures don't match.
            // TODO: define closeness based on similarlity of financial values
            for (int rotationIndex = position.RotationIndex; rotationIndex < this.RunParameters.RotationLengths.Count; ++rotationIndex)
            {
                OrganonStandTrajectory? trajectory = this.BestTrajectoryByRotationAndScenario[rotationIndex, position.FinancialIndex];
                if (trajectory != null)
                {
                    return trajectory;
                }
            }
            for (int rotationIndex = position.RotationIndex - 1; rotationIndex >= 0; --rotationIndex)
            {
                OrganonStandTrajectory? trajectory = this.BestTrajectoryByRotationAndScenario[rotationIndex, position.FinancialIndex];
                if (trajectory != null)
                {
                    return trajectory;
                }
            }

            throw new InvalidOperationException("No trajectories found within search range of rotation index " + position.RotationIndex + " and financial index " + position.FinancialIndex + ".");
        }

        protected OrganonStandTrajectory GetBestTrajectory(int rotationIndex, int financialIndex)
        {
            OrganonStandTrajectory? bestTrajectory = this.BestTrajectoryByRotationAndScenario[rotationIndex, financialIndex];
            if (bestTrajectory == null)
            {
                throw new InvalidOperationException("Trajectory for rotation index " + rotationIndex + " and financial scenario index " + financialIndex + " is null. This may indicate an indexing error or failure to accept a solution at this position. Is the rotation length at this index valid for the combination of thins this heuristic optimized for? The highest financial value at these indices is " + this.FinancialValue.GetHighestValue(rotationIndex, financialIndex) + ".");
            }
            return bestTrajectory;
        }

        public OrganonStandTrajectory GetBestTrajectoryWithDefaulting(HeuristicResultPosition position)
        {
            if (this.BestTrajectoryByRotationAndScenario.Length == 1)
            {
                return this.GetBestTrajectory(Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.FinancialIndex);
            }
            return this.GetBestTrajectory(position.RotationIndex, position.FinancialIndex);
        }

        public abstract string GetName();

        public float GetFinancialValue(StandTrajectory trajectory, int financialIndex) // public for test code access
        {
            return this.GetFinancialValue(trajectory, trajectory.PlanningPeriods - 1, financialIndex);
        }

        protected float GetFinancialValue(StandTrajectory trajectory, int endOfRotationPeriodIndex, int financialIndex)
        {
            Debug.Assert(trajectory.PeriodLengthInYears > 0);

            // find objective function value
            if ((this.RunParameters.TimberObjective == TimberObjective.LandExpectationValue) ||
                (this.RunParameters.TimberObjective == TimberObjective.NetPresentValue))
            {
                // net present value of first rotation
                float financialValue = this.RunParameters.Financial.GetNetPresentValue(trajectory, financialIndex, endOfRotationPeriodIndex);

                if (this.RunParameters.TimberObjective == TimberObjective.LandExpectationValue)
                {
                    int rotationLengthInYears = trajectory.GetEndOfPeriodAge(endOfRotationPeriodIndex);
                    float presentToFutureConversionFactor = this.RunParameters.Financial.GetAppreciationFactor(financialIndex, rotationLengthInYears);
                    float landExpectationValue = presentToFutureConversionFactor * financialValue / (presentToFutureConversionFactor - 1.0F);
                    financialValue = landExpectationValue;
                }

                // convert from US$/ha to USk$/ha
                financialValue *= 0.001F;
                return financialValue;
            }
            if (this.RunParameters.TimberObjective == TimberObjective.ScribnerVolume)
            {
                // TODO: move this out of financial objective calculations, left here as legacy support for now 
                // direct volume addition
                float scribnerVolumeInMbf = 0.0F;
                foreach (int periodIndex in trajectory.Treatments.GetThinningPeriods())
                {
                    trajectory.RecalculateThinningVolumeIfNeeded(periodIndex);
                    scribnerVolumeInMbf += trajectory.GetTotalScribnerVolumeThinned(periodIndex);
                }

                trajectory.RecalculateStandingVolumeIfNeeded(endOfRotationPeriodIndex);
                scribnerVolumeInMbf += trajectory.GetTotalStandingScribnerVolume(endOfRotationPeriodIndex);
                return scribnerVolumeInMbf;
            }

            throw new NotSupportedException("Unhandled timber objective " + this.RunParameters.TimberObjective + ".");
        }

        protected float[,] GetFinancialValueByRotationAndScenario(StandTrajectory trajectory)
        {
            float[,] financialValueByScenario = new float[this.RunParameters.RotationLengths.Count, this.RunParameters.Financial.Count];
            for (int rotationIndex = 0; rotationIndex < this.RunParameters.RotationLengths.Count; ++rotationIndex)
            {
                int endOfRotationPeriod = this.RunParameters.RotationLengths[rotationIndex];
                for (int financialIndex = 0; financialIndex < this.RunParameters.Financial.Count; ++financialIndex)
                {
                    financialValueByScenario[rotationIndex, financialIndex] = this.GetFinancialValue(trajectory, endOfRotationPeriod, financialIndex);
                }
            }
            return financialValueByScenario;
        }

        public virtual HeuristicMoveLog? GetMoveLog()
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

        protected int ConstructTreeSelection(HeuristicResultPosition position, HeuristicResults results)
        {
            // attempt to find an existing solution with the same set of thinning timings
            // If found, the existing solution's discount rate and number of planning periods may differ.
            if (results.TryGetSelfOrFindNearestNeighbor(position, out HeuristicSolutionPool? existingSolutions, out HeuristicResultPosition? positionOfExistingSolutions))
            {
                OrganonStandTrajectory eliteSolution = existingSolutions.SelectSolutionAndFindNearestStandTrajectory(position); // throws if pool is empty
                this.CurrentTrajectory.CopyTreeGrowthFrom(eliteSolution);

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
                if (position == positionOfExistingSolutions)
                {
                    // if one elite solution is available, begin this search with semi-greeding construction
                    this.ConstructionGreediness = results.GraspReactivity.GetConstructionGreediness(this.HeuristicParameters.MinimumConstructionGreediness, maximumConstructionGreediness);
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
                    if (position.FirstThinPeriodIndex != positionOfExistingSolutions.FirstThinPeriodIndex)
                    {
                        int currentThinPeriod = results.FirstThinPeriods[position.FirstThinPeriodIndex];
                        int existingSolutionThinPeriod = results.FirstThinPeriods[positionOfExistingSolutions.FirstThinPeriodIndex];
                        this.CurrentTrajectory.ChangeThinningPeriod(existingSolutionThinPeriod, currentThinPeriod);
                    }
                    if (position.SecondThinPeriodIndex != positionOfExistingSolutions.SecondThinPeriodIndex)
                    {
                        int currentThinPeriod = results.SecondThinPeriods[position.SecondThinPeriodIndex];
                        int existingSolutionThinPeriod = results.SecondThinPeriods[positionOfExistingSolutions.SecondThinPeriodIndex];
                        this.CurrentTrajectory.ChangeThinningPeriod(existingSolutionThinPeriod, currentThinPeriod);
                    }
                    if (position.ThirdThinPeriodIndex != positionOfExistingSolutions.ThirdThinPeriodIndex)
                    {
                        int currentThinPeriod = results.ThirdThinPeriods[position.ThirdThinPeriodIndex];
                        int existingSolutionThinPeriod = results.ThirdThinPeriods[positionOfExistingSolutions.ThirdThinPeriodIndex];
                        this.CurrentTrajectory.ChangeThinningPeriod(existingSolutionThinPeriod, currentThinPeriod);
                    }
                }
            }
            else
            {
                // default to fully random construction if no solution has yet been found for this combination of thinnings
                this.ConstructionGreediness = Constant.Grasp.FullyRandomConstructionForMaximization;
            }

            return this.ConstructTreeSelection(this.ConstructionGreediness);
        }

        protected virtual float EvaluateInitialSelection(HeuristicResultPosition position, int moveCapacity, HeuristicPerformanceCounters perfCounters)
        {
            this.FinancialValue.SetMoveCapacity(moveCapacity);

            perfCounters.GrowthModelTimesteps += this.CurrentTrajectory.Simulate();
            this.BestTrajectoryByRotationAndScenario[Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.FinancialIndex]!.CopyTreeGrowthFrom(this.CurrentTrajectory);

            float financialValue = this.GetFinancialValue(this.CurrentTrajectory, position.FinancialIndex);
            this.FinancialValue.TryAddMove(Constant.HeuristicDefault.RotationIndex, Constant.HeuristicDefault.FinancialIndex, financialValue, financialValue);
            return financialValue;
        }

        public override HeuristicParameters GetParameters()
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
            int harvestPeriod = Constant.NoHarvestPeriod;
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
