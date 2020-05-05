namespace Osu.Cof.Ferm.Heuristics
{
    public class Objective : TimberValue
    {
        public HarvestPeriodSelection HarvestPeriodSelection { get; set; }
        public bool IsNetPresentValue { get; set; }

        public Objective()
        {
            this.HarvestPeriodSelection = HarvestPeriodSelection.NoneOrLast;
            this.IsNetPresentValue = false;
        }
    }
}
