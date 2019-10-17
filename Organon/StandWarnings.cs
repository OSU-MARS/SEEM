namespace Osu.Cof.Organon
{
    public class StandWarnings
    {
        // 2
        public bool BigSixHeightAbovePotential { get; set; }
        // 5
        public bool LessThan50TreeRecords { get; set; }
        // 1
        public bool MortalitySiteIndexOutOfRange { get; set; }
        // 4
        public bool OtherSpeciesBasalAreaTooHigh { get; set; }
        // 0
        public bool PrimarySiteIndexOutOfRange { get; set; }
        // 6 - more than half of big six trees are old or stand age is above limit supported by variant
        public bool TreesOld { get; set; }
        // 5
        public bool TreesYoung { get; set; }

        public StandWarnings()
        {
            this.BigSixHeightAbovePotential = false;
            this.LessThan50TreeRecords = false;
            this.MortalitySiteIndexOutOfRange = false;
            this.OtherSpeciesBasalAreaTooHigh = false;
            this.PrimarySiteIndexOutOfRange = false;
            this.TreesOld = false;
            this.TreesYoung = false;
        }

        public StandWarnings(StandWarnings other)
        {
            this.BigSixHeightAbovePotential = other.BigSixHeightAbovePotential;
            this.LessThan50TreeRecords = other.LessThan50TreeRecords;
            this.MortalitySiteIndexOutOfRange = other.MortalitySiteIndexOutOfRange;
            this.OtherSpeciesBasalAreaTooHigh = other.OtherSpeciesBasalAreaTooHigh;
            this.PrimarySiteIndexOutOfRange = other.PrimarySiteIndexOutOfRange;
            this.TreesOld = other.TreesOld;
            this.TreesYoung = other.TreesYoung;
        }
    }
}
