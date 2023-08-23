using Mars.Seem.Silviculture;
using System;
using System.Collections.Generic;

namespace Mars.Seem.Tree
{
    public class MeasuredStandTrajectory : StandTrajectory<Stand, StandDensity, Treatments>
    {
        private readonly IList<int> measurementAges;

        public MeasuredStandTrajectory(Stand stand, TreeScaling treeVolume, IList<int> measurementAges)
            : base(stand, treeVolume, measurementAges.Count - 1)
        {
            this.measurementAges = measurementAges;

            this.DensityByPeriod[0] = new StandDensity(stand);
            this.Name = stand.Name;
            // this.PeriodLengthInYears probably not well defined due to irregular measurement intervals
            this.PeriodZeroAgeInYears = measurementAges[0];
            this.StandByPeriod[0] = stand; // for now, assume shallow copy is acceptable
            this.StandByPeriod[0]!.Name += 0;
        }

        public MeasuredStandTrajectory(MeasuredStandTrajectory other)
            : base(other)
        {
            this.measurementAges = other.measurementAges; // for now, assume shallow copy is acceptable
        }

        public override MeasuredStandTrajectory Clone()
        {
            return new(this);
        }

        public override float GetBasalAreaThinnedPerHa(int periodIndex)
        {
            if (this.Treatments.Harvests.Count != 0)
            {
                // for now, measured stands are assumed not to have silvicultural treatments within their measurement intervals
                throw new NotSupportedException();
            }
            return 0.0F;
        }

        public override int GetStartOfPeriodAge(int period)
        {
            return this.measurementAges[period - 1];
        }

        public override int GetEndOfPeriodAge(int period)
        {
            return this.measurementAges[period];
        }

        public override int Simulate()
        {
            throw new NotSupportedException();
        }
    }
}
