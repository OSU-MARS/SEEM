using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Organon.Heuristics
{
    public class TabuSearch : Heuristic
    {
        public int Iterations { get; set; }
        public int Tenure { get; set; }

        public TabuSearch(Stand stand, OrganonConfiguration organonConfiguration, int harvestPeriods, int planningPeriods)
            :  base(stand, organonConfiguration, harvestPeriods, planningPeriods)
        {
            this.Iterations = stand.TreeRecordCount;
            this.Tenure = (int)(0.3 * stand.TreeRecordCount);

            this.ObjectiveFunctionByIteration = new List<float>(1000)
            {
                this.BestObjectiveFunction
            };
        }

        public override TimeSpan Run()
        {
            if (this.Iterations < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Iterations));
            }
            if (this.Tenure < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Tenure));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int[,] remainingTabuTenures = new int[this.TreeRecordCount, this.CurrentTrajectory.HarvestPeriods];
            float currentObjectiveFunction = this.BestObjectiveFunction;

            StandTrajectory candidateTrajectory = new StandTrajectory(this.CurrentTrajectory);
            //double tenureScalingFactor = ((double)this.Tenure - Constant.RoundToZeroTolerance) / (double)byte.MaxValue;
            for (int neighborhoodEvaluation = 0; neighborhoodEvaluation < this.Iterations; ++neighborhoodEvaluation)
            {
                // evaluate potential moves in neighborhood
                float bestCandidateObjectiveFunction = float.MinValue;
                int bestUnitIndex = -1;
                int bestHarvestPeriod = -1;
                float bestNonTabuCandidateObjectiveFunction = float.MinValue;
                int bestNonTabuUnitIndex = -1;
                int bestNonTabuHarvestPeriod = -1;
                for (int treeIndex = 0; treeIndex < this.TreeRecordCount; ++treeIndex)
                {
                    int currentHarvestPeriod = this.CurrentTrajectory.IndividualTreeSelection[treeIndex];
                    for (int harvestPeriodIndex = 0; harvestPeriodIndex < this.CurrentTrajectory.HarvestPeriods; ++harvestPeriodIndex)
                    {
                        float candidateObjectiveFunction = float.MinValue;
                        if (harvestPeriodIndex != currentHarvestPeriod)
                        {
                            candidateTrajectory.SetTreeSelection(treeIndex, harvestPeriodIndex);
                            candidateTrajectory.Simulate();
                            candidateObjectiveFunction = this.GetObjectiveFunction(candidateTrajectory);
                            candidateTrajectory.SetTreeSelection(treeIndex, currentHarvestPeriod);
                        }

                        if (candidateObjectiveFunction > bestCandidateObjectiveFunction)
                        {
                            bestCandidateObjectiveFunction = candidateObjectiveFunction;
                            bestUnitIndex = treeIndex;
                            bestHarvestPeriod = harvestPeriodIndex;
                        }

                        int tabuTenure = remainingTabuTenures[treeIndex, harvestPeriodIndex];
                        if ((tabuTenure == 0) && (candidateObjectiveFunction > bestNonTabuCandidateObjectiveFunction))
                        {
                            bestNonTabuCandidateObjectiveFunction = candidateObjectiveFunction;
                            bestNonTabuUnitIndex = treeIndex;
                            bestNonTabuHarvestPeriod = harvestPeriodIndex;
                        }

                        if (tabuTenure > 0)
                        {
                            remainingTabuTenures[treeIndex, harvestPeriodIndex] = tabuTenure - 1;
                        }
                    }
                }

                // make best move and update tabu table
                // other possibilities: 1) make unit tabu, 2) uncomment stochastic tenure
                if (bestCandidateObjectiveFunction > this.BestObjectiveFunction)
                {
                    // always accept best candidate if it improves upon the best solution
                    this.BestObjectiveFunction = bestCandidateObjectiveFunction;
                    currentObjectiveFunction = bestCandidateObjectiveFunction;

                    this.CurrentTrajectory.Copy(candidateTrajectory);
                    remainingTabuTenures[bestUnitIndex, bestHarvestPeriod] = this.Tenure;
                    // remainingTabuTenures[bestUnitIndex, bestHarvestPeriod] = (int)(tenureScalingFactor * this.GetPseudorandomByteAsDouble()) + 1;

                    this.BestTrajectory.Copy(this.CurrentTrajectory);
                }
                else if (bestNonTabuUnitIndex != -1)
                {
                    // otherwise, accept the best non-tabu move when one exists
                    // Existence is quite likely since (n trees) * (n periods) > tenure in most configurations.
                    currentObjectiveFunction = bestNonTabuCandidateObjectiveFunction;

                    this.CurrentTrajectory.Copy(candidateTrajectory);
                    remainingTabuTenures[bestNonTabuUnitIndex, bestNonTabuHarvestPeriod] = this.Tenure;
                    // remainingTabuTenures[bestNonTabuUnitIndex, bestNonTabuHarvestPeriod] = (int)(tenureScalingFactor * this.GetPseudorandomByteAsDouble()) + 1;
                }

                this.ObjectiveFunctionByIteration.Add(currentObjectiveFunction);
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
