using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mars.Seem.Optimization
{
    // get basic statistics from a list of values sampling a distribution
    // Immutable, so could be made a struct if needed.
    public class DistributionStatistics
    {
        public int Count{ get; private init; }
        public float Maximum{ get; private init; }
        public float Minimum{ get; private init; }
        public float Mean{ get; private init; }
        public float Median{ get; private init; }

        public float? LowerQuartile{ get; private init; }
        public float? UpperQuartile{ get; private init; }

        public float? TenthPercentile{ get; private init; }
        public float? NinetiethPercentile{ get; private init; }

        public float? TwoPointFivePercentile{ get; private init; }
        public float? NinetySevenPointFivePercentile{ get; private init; }

        // if not already sorted ascending, values will be reordered to ascending
        public DistributionStatistics(List<float> sampledValues)
        {
            sampledValues.Sort(); // sort ascending

            // find statistics
            // count, min, max, and mean
            this.Count = sampledValues.Count;
            this.Maximum = sampledValues[^1];
            this.Minimum = sampledValues[0];
            this.Mean = 0.0F;
            foreach (float objective in sampledValues)
            {
                this.Mean += objective;
            }
            this.Mean /= this.Count;

            // median
            bool exactMedian = (sampledValues.Count % 2) == 1;
            if (exactMedian)
            {
                this.Median = sampledValues[sampledValues.Count / 2]; // x.5 truncates to x, matching middle element due to zero based indexing
            }
            else
            {
                int halfIndex = sampledValues.Count / 2;
                this.Median = 0.5F * sampledValues[halfIndex - 1] + 0.5F * sampledValues[halfIndex];

                Debug.Assert(this.Median >= sampledValues[0]);
                Debug.Assert(this.Median <= sampledValues[^1]);
            }

            // quantiles
            if (sampledValues.Count > 4)
            {
                bool exactQuartiles = (sampledValues.Count % 4) == 0;
                if (exactQuartiles)
                {
                    this.LowerQuartile = sampledValues[sampledValues.Count / 4];
                    this.UpperQuartile = sampledValues[3 * sampledValues.Count / 4];
                }
                else
                {
                    float lowerQuartilePosition = 0.25F * sampledValues.Count;
                    float ceilingIndex = MathF.Ceiling(lowerQuartilePosition);
                    float floorIndex = MathF.Floor(lowerQuartilePosition);
                    float ceilingWeight = 1.0F + lowerQuartilePosition - ceilingIndex;
                    float floorWeight = 1.0F - lowerQuartilePosition + floorIndex;
                    this.LowerQuartile = floorWeight * sampledValues[(int)floorIndex] + ceilingWeight * sampledValues[(int)ceilingIndex];

                    float upperQuartilePosition = 0.75F * sampledValues.Count;
                    ceilingIndex = MathF.Ceiling(upperQuartilePosition);
                    floorIndex = MathF.Floor(upperQuartilePosition);
                    ceilingWeight = 1.0F + upperQuartilePosition - ceilingIndex;
                    floorWeight = 1.0F - upperQuartilePosition + floorIndex;
                    this.UpperQuartile = floorWeight * sampledValues[(int)floorIndex] + ceilingWeight * sampledValues[(int)ceilingIndex];

                    Debug.Assert(this.LowerQuartile >= sampledValues[0]);
                    Debug.Assert(this.LowerQuartile <= Median);
                    Debug.Assert(this.UpperQuartile >= Median);
                    Debug.Assert(this.UpperQuartile <= sampledValues[^1]);
                }

                if (sampledValues.Count > 9)
                {
                    bool exactPercentiles = (sampledValues.Count % 10) == 0;
                    if (exactPercentiles)
                    {
                        this.TenthPercentile = sampledValues[sampledValues.Count / 10];
                        this.NinetiethPercentile = sampledValues[9 * sampledValues.Count / 10];
                    }
                    else
                    {
                        float tenthPercentilePosition = 0.1F * sampledValues.Count;
                        float ceilingIndex = MathF.Ceiling(tenthPercentilePosition);
                        float floorIndex = MathF.Floor(tenthPercentilePosition);
                        float ceilingWeight = 1.0F + tenthPercentilePosition - ceilingIndex;
                        float floorWeight = 1.0F - tenthPercentilePosition + floorIndex;
                        this.TenthPercentile = floorWeight * sampledValues[(int)floorIndex] + ceilingWeight * sampledValues[(int)ceilingIndex];

                        float ninetiethPercentilePosition = 0.9F * sampledValues.Count;
                        ceilingIndex = MathF.Ceiling(ninetiethPercentilePosition);
                        floorIndex = MathF.Floor(ninetiethPercentilePosition);
                        ceilingWeight = 1.0F + ninetiethPercentilePosition - ceilingIndex;
                        floorWeight = 1.0F - ninetiethPercentilePosition + floorIndex;
                        this.NinetiethPercentile = floorWeight * sampledValues[(int)floorIndex] + ceilingWeight * sampledValues[(int)ceilingIndex];

                        Debug.Assert(this.TenthPercentile >= sampledValues[0]);
                        Debug.Assert(this.TenthPercentile <= Median);
                        Debug.Assert(this.NinetiethPercentile >= Median);
                        Debug.Assert(this.NinetiethPercentile <= sampledValues[^1]);
                    }

                    if (sampledValues.Count > 39)
                    {
                        exactPercentiles = (sampledValues.Count % 40) == 0;
                        if (exactPercentiles)
                        {
                            this.TwoPointFivePercentile = sampledValues[sampledValues.Count / 40];
                            this.NinetySevenPointFivePercentile = sampledValues[39 * sampledValues.Count / 40];
                        }
                        else
                        {
                            float twoPointFivePercentilePosition = 0.025F * sampledValues.Count;
                            float ceilingIndex = MathF.Ceiling(twoPointFivePercentilePosition);
                            float floorIndex = MathF.Floor(twoPointFivePercentilePosition);
                            float ceilingWeight = 1.0F + twoPointFivePercentilePosition - ceilingIndex;
                            float floorWeight = 1.0F - twoPointFivePercentilePosition + floorIndex;
                            float twoPointFivePercentile = floorWeight * sampledValues[(int)floorIndex] + ceilingWeight * sampledValues[(int)ceilingIndex];
                            this.TwoPointFivePercentile = twoPointFivePercentile;

                            float ninetySevenPointFivePercentilePosition = 0.975F * sampledValues.Count;
                            ceilingIndex = MathF.Ceiling(ninetySevenPointFivePercentilePosition);
                            floorIndex = MathF.Floor(ninetySevenPointFivePercentilePosition);
                            ceilingWeight = 1.0F + ninetySevenPointFivePercentilePosition - ceilingIndex;
                            floorWeight = 1.0F - ninetySevenPointFivePercentilePosition + floorIndex;
                            float ninetySevenPointFivePercentile = floorWeight * sampledValues[(int)floorIndex] + ceilingWeight * sampledValues[(int)ceilingIndex];
                            this.NinetySevenPointFivePercentile = ninetySevenPointFivePercentile;

                            Debug.Assert(twoPointFivePercentile >= sampledValues[0]);
                            Debug.Assert(twoPointFivePercentile <= Median);
                            Debug.Assert(ninetySevenPointFivePercentile >= Median);
                            Debug.Assert(ninetySevenPointFivePercentile <= sampledValues[^1]);
                        }
                    }
                }
            }
        }
    }
}
