namespace Mars.Seem.Organon
{
    public class OrganonWarnings
    {
        // 2
        public bool BigSixHeightAbovePotential { get; set; }
        // 5
        public bool LessThan50TreeRecords { get; set; }
        // 1
        public bool HemlockSiteIndexOutOfRange { get; set; }
        // 4
        public bool OtherSpeciesBasalAreaTooHigh { get; set; }
        // 0
        public bool SiteIndexOutOfRange { get; set; }
        // 6 - more than half of big six trees are old or stand age is above limit supported by variant
        public bool TreesOld { get; set; }
        // 5
        public bool TreesYoung { get; set; }

        public OrganonWarnings()
        {
            this.BigSixHeightAbovePotential = false;
            this.LessThan50TreeRecords = false;
            this.HemlockSiteIndexOutOfRange = false;
            this.OtherSpeciesBasalAreaTooHigh = false;
            this.SiteIndexOutOfRange = false;
            this.TreesOld = false;
            this.TreesYoung = false;
        }

        public OrganonWarnings(OrganonWarnings other)
        {
            this.BigSixHeightAbovePotential = other.BigSixHeightAbovePotential;
            this.LessThan50TreeRecords = other.LessThan50TreeRecords;
            this.HemlockSiteIndexOutOfRange = other.HemlockSiteIndexOutOfRange;
            this.OtherSpeciesBasalAreaTooHigh = other.OtherSpeciesBasalAreaTooHigh;
            this.SiteIndexOutOfRange = other.SiteIndexOutOfRange;
            this.TreesOld = other.TreesOld;
            this.TreesYoung = other.TreesYoung;
        }
    }
}
