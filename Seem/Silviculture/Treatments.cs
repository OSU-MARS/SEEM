using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Silviculture
{
    public class Treatments
    {
        public List<Harvest> Harvests { get; private init; }

        public Treatments()
        {
            this.Harvests = new();
        }

        protected Treatments(Treatments other)
            : this()
        {
            foreach (Harvest harvest in other.Harvests)
            {
                this.Harvests.Add(harvest.Clone());
            }
        }

        public virtual Treatments Clone()
        {
            return new(this);
        }

        protected void CopyFrom(Treatments other)
        {
            int maxHarvestToHarvestCopyIndex = other.Harvests.Count < this.Harvests.Count ? other.Harvests.Count : this.Harvests.Count;
            for (int index = 0; index < maxHarvestToHarvestCopyIndex; ++index)
            {
                Harvest otherHarvest = other.Harvests[index];
                Harvest thisHarvest = this.Harvests[index];
                if (thisHarvest.TryCopyFrom(otherHarvest) == false)
                {
                    this.Harvests[index] = otherHarvest.Clone();
                }
            }
            if (this.Harvests.Count > other.Harvests.Count)
            {
                this.Harvests.RemoveRange(other.Harvests.Count, this.Harvests.Count - other.Harvests.Count);
            }
            else if (other.Harvests.Count > this.Harvests.Count)
            {
                for (int index = this.Harvests.Count; index < other.Harvests.Count; ++index)
                {
                    this.Harvests.Add(other.Harvests[index].Clone());
                }
            }
        }

        public IList<int> GetHarvestPeriods()
        {
            IList<int> thinningPeriods = this.GetThinningPeriods();
            thinningPeriods.Add(Constant.NoHarvestPeriod);
            return thinningPeriods;
        }

        // could also implement this on HeuristicResultPosition
        public List<int> GetThinningPeriods()
        {
            List<int> thinningPeriods = new(this.Harvests.Count + 1);
            foreach (Harvest harvest in this.Harvests)
            {
                Debug.Assert(((harvest is ThinByIndividualTreeSelection) || (harvest is ThinByPrescription)) &&
                             (thinningPeriods.Contains(harvest.Period) == false));
                thinningPeriods.Add(harvest.Period);
            }
            return thinningPeriods;
        }
    }
}
