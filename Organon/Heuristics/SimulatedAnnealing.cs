using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Heuristics
{
    public class SimulatedAnnealing : Heuristic
    {
        public float Alpha { get; set; }
        public float FinalTemperature { get; set; }
        public float InitialTemperature { get; set; }
        public int IterationsPerTemperature { get; set; }

        public SimulatedAnnealing(OrganonStand stand, OrganonConfiguration organonConfiguration, int planningPeriods, Objective objective)
            :  base(stand, organonConfiguration, planningPeriods, objective)
        {
            this.FinalTemperature = 100.0F;
            this.InitialTemperature = 10000.0F;
            this.IterationsPerTemperature = 10;

            int defaultIterations = 1000 * 1000;
            float temperatureSteps = (float)(defaultIterations / this.IterationsPerTemperature);
            this.Alpha = 1.0F / MathF.Pow(this.InitialTemperature / this.FinalTemperature, 1.0F / temperatureSteps);

            this.ObjectiveFunctionByMove = new List<float>(defaultIterations)
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
            float treeIndexScalingFactor = ((float)this.GetInitialTreeRecordCount() - Constant.RoundToZeroTolerance) / (float)UInt16.MaxValue;

            OrganonStandTrajectory candidateTrajectory = new OrganonStandTrajectory(this.CurrentTrajectory);
            for (double currentTemperature = this.InitialTemperature; currentTemperature > this.FinalTemperature; currentTemperature *= this.Alpha)
            {
                for (int iterationAtTemperature = 0; iterationAtTemperature < this.IterationsPerTemperature; ++iterationAtTemperature)
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

                    float candidateObjectiveFunction = this.GetObjectiveFunction(candidateTrajectory);
                    bool acceptMove = candidateObjectiveFunction > currentObjectiveFunction;
                    if (acceptMove == false)
                    {
                        float candidateObjectiveFunctionChange = candidateObjectiveFunction - currentObjectiveFunction;
                        float exponent = -candidateObjectiveFunctionChange / temperature;
                        if (exponent < 10.0F)
                        {
                            // exponent is small enough not to round acceptance probabilities down to zero
                            // 1/e^10 accepts 1 in 22,026 moves.
                            float acceptanceProbability = 1.0F / MathV.Exp(exponent);
                            float moveProbability = acceptanceProbabilityScalingFactor * this.GetPseudorandomByteAsFloat();
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

                    this.ObjectiveFunctionByMove.Add(currentObjectiveFunction);
                }
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
