using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Organon.Heuristics
{
    public class SimulatedAnnealing : Heuristic
    {
        public float Alpha { get; set; }
        public float FinalTemperature { get; set; }
        public float InitialTemperature { get; set; }
        public int IterationsPerTemperature { get; set; }

        public SimulatedAnnealing(Stand stand, OrganonConfiguration organonConfiguration, int harvestPeriods, int planningPeriods, Objective objective)
            :  base(stand, organonConfiguration, harvestPeriods, planningPeriods, objective)
        {
            this.FinalTemperature = 100.0F;
            this.InitialTemperature = 10000.0F;
            this.IterationsPerTemperature = 10;

            int defaultIterations = 1000 * 1000;
            double temperatureSteps = (double)(defaultIterations / this.IterationsPerTemperature);
            this.Alpha = 1.0F / (float)Math.Pow(this.InitialTemperature / this.FinalTemperature, 1.0F / temperatureSteps);

            this.ObjectiveFunctionByIteration = new List<float>(defaultIterations)
            {
                this.BestObjectiveFunction
            };
        }

        public override string GetColumnName()
        {
            return "SimulatedAnnealing";
        }

        public override TimeSpan Run()
        {
            if ((this.Alpha <= 0.0) || (this.Alpha >= 1.0))
            {
                throw new ArgumentOutOfRangeException(nameof(this.Alpha));
            }
            if (this.FinalTemperature <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.FinalTemperature));
            }
            if (this.InitialTemperature <= this.FinalTemperature)
            {
                throw new ArgumentOutOfRangeException(nameof(this.InitialTemperature));
            }
            if (this.IterationsPerTemperature <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.IterationsPerTemperature));
            }
            if (this.Objective.HarvestPeriodSelection != HarvestPeriodSelection.NoneOrLast)
            {
                throw new NotSupportedException(nameof(this.Objective.HarvestPeriodSelection));
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            float acceptanceProbabilityScalingFactor = 1.0F / (float)byte.MaxValue;
            float currentObjectiveFunction = this.BestObjectiveFunction;
            //float harvestPeriodScalingFactor = ((float)this.CurrentTrajectory.HarvestPeriods - Constant.RoundToZeroTolerance) / (float)byte.MaxValue;
            float temperature = this.InitialTemperature;
            float treeIndexScalingFactor = ((float)this.TreeRecordCount - Constant.RoundToZeroTolerance) / (float)UInt16.MaxValue;

            StandTrajectory candidateTrajectory = new StandTrajectory(this.CurrentTrajectory);
            for (double currentTemperature = this.InitialTemperature; currentTemperature > this.FinalTemperature; currentTemperature *= this.Alpha)
            {
                for (int iterationAtTemperature = 0; iterationAtTemperature < this.IterationsPerTemperature; ++iterationAtTemperature)
                {
                    int treeIndex = (int)(treeIndexScalingFactor * this.GetTwoPseudorandomBytesAsFloat());
                    int currentHarvestPeriod = this.CurrentTrajectory.IndividualTreeSelection[treeIndex];
                    int candidateHarvestPeriod = currentHarvestPeriod == 0 ? this.CurrentTrajectory.HarvestPeriods - 1 : 0;
                    //int candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsFloat());
                    //while (candidateHarvestPeriod == currentHarvestPeriod)
                    //{
                    //    candidateHarvestPeriod = (int)(harvestPeriodScalingFactor * this.GetPseudorandomByteAsFloat());
                    //}
                    Debug.Assert(candidateHarvestPeriod >= 0);

                    candidateTrajectory.IndividualTreeSelection[treeIndex] = candidateHarvestPeriod;
                    candidateTrajectory.Simulate();

                    float candidateObjectiveFunction = this.GetObjectiveFunction(candidateTrajectory);
                    bool acceptMove = candidateObjectiveFunction > currentObjectiveFunction;
                    if (acceptMove == false)
                    {
                        double candidateObjectiveFunctionChange = candidateObjectiveFunction - currentObjectiveFunction;
                        double exponent = candidateObjectiveFunctionChange / temperature;
                        if (exponent < 10.0F)
                        {
                            // exponent is small enough not to round acceptance probabilities down to zero
                            // 1/e^10 accepts 1 in 22,026 moves.
                            double acceptanceProbability = 1.0F / (double)Math.Exp(exponent);
                            double moveProbability = acceptanceProbabilityScalingFactor * this.GetPseudorandomByteAsFloat();
                            if (moveProbability < acceptanceProbability)
                            {
                                acceptMove = true;
                            }
                        }
                    }

                    if (acceptMove)
                    {
                        currentObjectiveFunction = candidateObjectiveFunction;
                        this.CurrentTrajectory.Copy(candidateTrajectory);
                        if (currentObjectiveFunction > this.BestObjectiveFunction)
                        {
                            this.BestObjectiveFunction = currentObjectiveFunction;
                            this.BestTrajectory.Copy(this.CurrentTrajectory);
                        }
                    }
                    else
                    {
                        candidateTrajectory.SetTreeSelection(treeIndex, currentHarvestPeriod);
                    }

                    this.ObjectiveFunctionByIteration.Add(currentObjectiveFunction);
                }
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
