using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm
{
    // get basic statistics from a list of values sampling a distribution
    // Immutable, so could be made a struct if needed.
    public class Statistics
    {
        public int Count{ get; private init; }
        public float Maximum{ get; private init; }
        public float Minimum{ get; private init; }
        public float Mean{ get; private init; }
        public float Median{ get; private init; }

        public float? LowerQuartile{ get; private init; }
        public float? UpperQuartile{ get; private init; }

        public float? FifthPercentile{ get; private init; }
        public float? NinetyFifthPercentile{ get; private init; }

        public float? TwoPointFivePercentile{ get; private init; }
        public float? NinetySevenPointFivePercentile{ get; private init; }

        // if not already sorted ascending, values will be reordered to ascending
        public Statistics(List<float> values)
        {
            values.Sort(); // sort ascending

            // find statistics
            // count, min, max, and mean
            this.Count = values.Count;
            this.Maximum = values[^1];
            this.Minimum = values[0];
            this.Mean = 0.0F;
            foreach (float objective in values)
            {
                this.Mean += objective;
            }
            this.Mean /= this.Count;

            // median
            bool exactMedian = (values.Count % 2) == 1;
            if (exactMedian)
            {
                this.Median = values[values.Count / 2]; // x.5 truncates to x, matching middle element due to zero based indexing
            }
            else
            {
                int halfIndex = values.Count / 2;
                this.Median = 0.5F * values[halfIndex - 1] + 0.5F * values[halfIndex];

                Debug.Assert(Median >= values[0]);
                Debug.Assert(Median <= values[^1]);
            }

            // quantiles
            if (values.Count > 4)
            {
                bool exactQuartiles = (values.Count % 4) == 0;
                if (exactQuartiles)
                {
                    this.LowerQuartile = values[values.Count / 4];
                    this.UpperQuartile = values[3 * values.Count / 4];
                }
                else
                {
                    float lowerQuartilePosition = 0.25F * values.Count;
                    float ceilingIndex = MathF.Ceiling(lowerQuartilePosition);
                    float floorIndex = MathF.Floor(lowerQuartilePosition);
                    float ceilingWeight = 1.0F + lowerQuartilePosition - ceilingIndex;
                    float floorWeight = 1.0F - lowerQuartilePosition + floorIndex;
                    this.LowerQuartile = floorWeight * values[(int)floorIndex] + ceilingWeight * values[(int)ceilingIndex];

                    float upperQuartilePosition = 0.75F * values.Count;
                    ceilingIndex = MathF.Ceiling(upperQuartilePosition);
                    floorIndex = MathF.Floor(upperQuartilePosition);
                    ceilingWeight = 1.0F + upperQuartilePosition - ceilingIndex;
                    floorWeight = 1.0F - upperQuartilePosition + floorIndex;
                    this.UpperQuartile = floorWeight * values[(int)floorIndex] + ceilingWeight * values[(int)ceilingIndex];

                    Debug.Assert(LowerQuartile >= values[0]);
                    Debug.Assert(LowerQuartile <= Median);
                    Debug.Assert(UpperQuartile >= Median);
                    Debug.Assert(UpperQuartile <= values[^1]);
                }

                if (values.Count > 19)
                {
                    bool exactPercentiles = (values.Count % 20) == 0;
                    if (exactPercentiles)
                    {
                        this.FifthPercentile = values[values.Count / 20];
                        this.NinetyFifthPercentile = values[19 * values.Count / 20];
                    }
                    else
                    {
                        float fifthPercentilePosition = 0.05F * values.Count;
                        float ceilingIndex = MathF.Ceiling(fifthPercentilePosition);
                        float floorIndex = MathF.Floor(fifthPercentilePosition);
                        float ceilingWeight = 1.0F + fifthPercentilePosition - ceilingIndex;
                        float floorWeight = 1.0F - fifthPercentilePosition + floorIndex;
                        this.FifthPercentile = floorWeight * values[(int)floorIndex] + ceilingWeight * values[(int)ceilingIndex];

                        float ninetyFifthPercentilePosition = 0.95F * values.Count;
                        ceilingIndex = MathF.Ceiling(ninetyFifthPercentilePosition);
                        floorIndex = MathF.Floor(ninetyFifthPercentilePosition);
                        ceilingWeight = 1.0F + ninetyFifthPercentilePosition - ceilingIndex;
                        floorWeight = 1.0F - ninetyFifthPercentilePosition + floorIndex;
                        this.NinetyFifthPercentile = floorWeight * values[(int)floorIndex] + ceilingWeight * values[(int)ceilingIndex];

                        Debug.Assert(this.FifthPercentile >= values[0]);
                        Debug.Assert(this.FifthPercentile <= Median);
                        Debug.Assert(this.NinetyFifthPercentile >= Median);
                        Debug.Assert(this.NinetyFifthPercentile <= values[^1]);
                    }

                    if (values.Count > 39)
                    {
                        exactPercentiles = (values.Count % 40) == 0;
                        if (exactPercentiles)
                        {
                            this.TwoPointFivePercentile = values[values.Count / 40];
                            this.NinetySevenPointFivePercentile = values[39 * values.Count / 40];
                        }
                        else
                        {
                            float twoPointFivePercentilePosition = 0.025F * values.Count;
                            float ceilingIndex = MathF.Ceiling(twoPointFivePercentilePosition);
                            float floorIndex = MathF.Floor(twoPointFivePercentilePosition);
                            float ceilingWeight = 1.0F + twoPointFivePercentilePosition - ceilingIndex;
                            float floorWeight = 1.0F - twoPointFivePercentilePosition + floorIndex;
                            float twoPointFivePercentile = floorWeight * values[(int)floorIndex] + ceilingWeight * values[(int)ceilingIndex];
                            this.TwoPointFivePercentile = twoPointFivePercentile;

                            float ninetySevenPointFivePercentilePosition = 0.975F * values.Count;
                            ceilingIndex = MathF.Ceiling(ninetySevenPointFivePercentilePosition);
                            floorIndex = MathF.Floor(ninetySevenPointFivePercentilePosition);
                            ceilingWeight = 1.0F + ninetySevenPointFivePercentilePosition - ceilingIndex;
                            floorWeight = 1.0F - ninetySevenPointFivePercentilePosition + floorIndex;
                            float ninetySevenPointFivePercentile = floorWeight * values[(int)floorIndex] + ceilingWeight * values[(int)ceilingIndex];
                            this.NinetySevenPointFivePercentile = ninetySevenPointFivePercentile;

                            Debug.Assert(twoPointFivePercentile >= values[0]);
                            Debug.Assert(twoPointFivePercentile <= Median);
                            Debug.Assert(ninetySevenPointFivePercentile >= Median);
                            Debug.Assert(ninetySevenPointFivePercentile <= values[^1]);
                        }
                    }
                }
            }
        }
    }
}
