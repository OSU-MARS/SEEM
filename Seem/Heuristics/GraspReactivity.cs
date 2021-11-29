using Mars.Seem.Optimization;
using System;

namespace Mars.Seem.Heuristics
{
    public class GraspReactivity : PseudorandomizingTask
    {
        public float GreedinessBinWidth { get; private init; }
        public int[] SelectionHistogram { get; private init; }
        public int[] RejectionHistogram { get; private init; }

        public GraspReactivity()
        {
            this.GreedinessBinWidth = 0.01F;
            int bins = (int)(1.0F / this.GreedinessBinWidth);
            this.SelectionHistogram = new int[bins];
            this.RejectionHistogram = new int[bins];
        }

        public void Add(float constructionGreediness, bool selectedAsEliteSolution)
        {
            if (constructionGreediness == Constant.Grasp.NoConstruction)
            {
                return;
            }
            else if ((constructionGreediness < Constant.Grasp.FullyRandomConstructionForMaximization) || (constructionGreediness > Constant.Grasp.FullyGreedyConstructionForMaximization))
            {
                throw new ArgumentOutOfRangeException(nameof(constructionGreediness));
            }

            int binIndex = (int)((constructionGreediness - Constant.RoundTowardsZeroTolerance) / this.GreedinessBinWidth);
            if (selectedAsEliteSolution)
            {
                ++this.SelectionHistogram[binIndex];
            }
            else
            {
                ++this.RejectionHistogram[binIndex];
            }
        }

        public float GetConstructionGreediness(float minimumGreediness, float maximumGreediness)
        {
            int minimumBinIndex = (int)((minimumGreediness - Constant.RoundTowardsZeroTolerance) / this.GreedinessBinWidth);
            int maximumBinIndex = (int)((maximumGreediness - Constant.RoundTowardsZeroTolerance) / this.GreedinessBinWidth);

            int total = 0;
            for (int binIndex = minimumBinIndex; binIndex <= maximumBinIndex; ++binIndex)
            {
                total += this.SelectionHistogram[binIndex] + 1;
            }

            float count = 0.0F;
            float startBinCumulativeProbability = 0.0F;
            float greediness = maximumGreediness;
            float greedinessCumulativeProbabilityPosition = this.Pseudorandom.GetTwoPseudorandomBytesAsProbability();
            for (int binIndex = minimumBinIndex; binIndex <= maximumBinIndex; ++binIndex)
            {
                count += this.SelectionHistogram[binIndex] + 1.0F;

                float endBinCumulativeProbability = count / total;
                if (endBinCumulativeProbability >= greedinessCumulativeProbabilityPosition)
                {
                    // linear interpolation to probability position within bin
                    float startBinGreediness = this.GreedinessBinWidth * binIndex;
                    float binPosition = (greedinessCumulativeProbabilityPosition - startBinCumulativeProbability) / (endBinCumulativeProbability - startBinCumulativeProbability);
                    greediness = MathF.Min(startBinGreediness + this.GreedinessBinWidth * binPosition, maximumGreediness);
                    break;
                }

                startBinCumulativeProbability = endBinCumulativeProbability;
            }

            return greediness;
        }
    }
}
