using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class SimulatedAnnealing : Heuristic
    {
        public float Alpha { get; set; }
        public float FinalProbability { get; set; }
        public float InitialProbability { get; set; }
        public int Iterations { get; set; }
        public int IterationsPerTemperature { get; set; }
        public int ProbabilityWindowLength { get; set; }
        public int ReheatAfter { get; set; }
        public float ReheatBy { get; set; }

        public SimulatedAnnealing(OrganonStand stand, OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective)
            :  base(stand, organonConfiguration, planningPeriods, objective)
        {
            int treeRecords = stand.GetTreeRecordCount();
            this.Alpha = 0.925F;
            this.FinalProbability = 0.0F;
            this.InitialProbability = 0.0F;
            this.Iterations = 10 * treeRecords;
            this.IterationsPerTemperature = 10;
            this.ProbabilityWindowLength = 10;
            this.ReheatAfter = treeRecords;
            this.ReheatBy = 0.75F;

            // float temperatureSteps = (float)(defaultIterations / this.IterationsPerTemperature);
            // this.Alpha = 1.0F / MathF.Pow(this.InitialAcceptProbability / this.FinalAcceptProbability, 1.0F / temperatureSteps);

            this.ObjectiveFunctionByMove = new List<float>(this.Iterations)
            {
                this.BestObjectiveFunction
            };
        }

        public override string GetName()
        {
            return "SimulatedAnnealing";
        }

        public override TimeSpan Run()
        {
            if ((this.Alpha <= 0.0) || (this.Alpha >= 1.0))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Alpha));
            }
            if (this.FinalProbability < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.FinalProbability));
            }
            if (this.InitialProbability < this.FinalProbability)
            {
                throw new ArgumentOutOfRangeException(nameof(this.InitialProbability));
            }
            if (this.IterationsPerTemperature < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(this.IterationsPerTemperature));
            }
            if (this.Objective.HarvestPeriodSelection != HarvestPeriodSelection.NoneOrLast)
            {
                throw new NotSupportedException(nameof(this.Objective.HarvestPeriodSelection));
            }
            if (this.ReheatAfter < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.ReheatAfter));
            }
            if (this.ReheatBy < 0.0F)
            {
                throw new ArgumentOutOfRangeException(nameof(this.ReheatBy));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            float currentObjectiveFunction = this.BestObjectiveFunction;
            //float harvestPeriodScalingFactor = ((float)this.CurrentTrajectory.HarvestPeriods - Constant.RoundToZeroTolerance) / (float)byte.MaxValue;
            int iterationsSinceReheatOrObjectiveChange = 0;
            float meanAcceptanceProbability = this.InitialProbability;
            float movingAverageOfObjectiveChange = -1.0F;
            float movingAverageMemory = 1.0F - 1.0F / this.ProbabilityWindowLength;
            float treeIndexScalingFactor = ((float)this.GetInitialTreeRecordCount() - Constant.RoundTowardsZeroTolerance) / (float)UInt16.MaxValue;
            float unityScalingFactor = 1.0F / (float)byte.MaxValue;

            OrganonStandTrajectory candidateTrajectory = new OrganonStandTrajectory(this.CurrentTrajectory);
            for (int iteration = 1; (iteration < this.Iterations) && (meanAcceptanceProbability >= this.FinalProbability); meanAcceptanceProbability *= this.Alpha)
            {
                float logMeanAcceptanceProbability = Single.NegativeInfinity;
                if (meanAcceptanceProbability > 0.0F)
                {
                    logMeanAcceptanceProbability = MathV.Ln(meanAcceptanceProbability);
                }

                for (int iterationAtTemperature = 0; iterationAtTemperature < this.IterationsPerTemperature; ++iteration, ++iterationAtTemperature)
                {
                    int treeIndex = (int)(treeIndexScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                    int currentHarvestPeriod = this.CurrentTrajectory.GetTreeSelection(treeIndex);
                    int candidateHarvestPeriod = currentHarvestPeriod == 0 ? this.CurrentTrajectory.HarvestPeriods - 1 : 0;
                    //int candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsFloat());
                    //while (candidateHarvestPeriod == currentHarvestPeriod)
                    //{
                    //    candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsFloat());
                    //}
                    Debug.Assert(candidateHarvestPeriod >= 0);

                    candidateTrajectory.SetTreeSelection(treeIndex, candidateHarvestPeriod);
                    candidateTrajectory.Simulate();
                    ++iterationsSinceReheatOrObjectiveChange;

                    float candidateObjectiveFunction = this.GetObjectiveFunction(candidateTrajectory);

                    bool acceptMove = candidateObjectiveFunction > currentObjectiveFunction;
                    // require at least one improving move be accepted to set moving average before accepting disimproving moves
                    if ((acceptMove == false) && (logMeanAcceptanceProbability > Single.NegativeInfinity) && (movingAverageOfObjectiveChange > 0.0F))
                    {
                        // objective function increase is negative and log of acceptance probability is negative or zero, so exponent is positive or zero
                        float objectiveFunctionIncrease = candidateObjectiveFunction - currentObjectiveFunction;
                        float exponent = logMeanAcceptanceProbability * objectiveFunctionIncrease / movingAverageOfObjectiveChange;
                        Debug.Assert(exponent >= 0.0F);
                        if (exponent < 10.0F)
                        {
                            // exponent is small enough not to round acceptance probabilities down to zero
                            // 1/e^10 accepts 1 in 22,026 moves.
                            float acceptanceProbability = 1.0F / MathV.Exp(exponent);
                            float moveProbability = unityScalingFactor * this.GetPseudorandomByteAsFloat();
                            if (moveProbability < acceptanceProbability)
                            {
                                acceptMove = true;
                            }
                        }
                    }

                    if (acceptMove)
                    {
                        float objectiveFunctionChange = MathF.Abs(currentObjectiveFunction - candidateObjectiveFunction);
                        if (movingAverageOfObjectiveChange < 0.0F)
                        {
                            // acceptance of first move
                            movingAverageOfObjectiveChange = objectiveFunctionChange;
                        }
                        else
                        {
                            // all subsequent acceptances
                            movingAverageOfObjectiveChange = movingAverageMemory * movingAverageOfObjectiveChange + (1.0F - movingAverageMemory) * objectiveFunctionChange;
                        }

                        currentObjectiveFunction = candidateObjectiveFunction;
                        this.CurrentTrajectory.CopyFrom(candidateTrajectory);
                        if (currentObjectiveFunction > this.BestObjectiveFunction)
                        {
                            this.BestObjectiveFunction = currentObjectiveFunction;
                            this.BestTrajectory.CopyFrom(this.CurrentTrajectory);
                        }

                        iterationsSinceReheatOrObjectiveChange = 0;
                        Debug.Assert(movingAverageOfObjectiveChange > 0.0F);
                    }
                    else
                    {
                        candidateTrajectory.SetTreeSelection(treeIndex, currentHarvestPeriod);
                    }

                    this.ObjectiveFunctionByMove.Add(currentObjectiveFunction);

                    if (iterationsSinceReheatOrObjectiveChange > this.ReheatAfter)
                    {
                        // while it's unlikely alpha would be close enough to 1 and reheat intervals short enough to drive the acceptance probability
                        // above one, it is possible
                        meanAcceptanceProbability = Math.Min(meanAcceptanceProbability + this.ReheatBy, 1.0F);
                        logMeanAcceptanceProbability = MathV.Ln(meanAcceptanceProbability);
                        iterationsSinceReheatOrObjectiveChange = 0;
                    }
                    if (this.ObjectiveFunctionByMove.Count == this.ChainFrom)
                    {
                        this.BestTrajectoryByMove.Add(this.ChainFrom, new StandTrajectory(this.BestTrajectory));
                    }
                }
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
