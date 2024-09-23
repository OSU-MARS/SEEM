using Mars.Seem.Extensions;
using Mars.Seem.Tree;
using System.Xml;
using System;
using System.Collections.Generic;
using Mars.Seem.Organon;

namespace Mars.Seem.Data
{
    public class CruisedStands : TreeReader
    {
        private int nextTreeID;
        private readonly CruisedStandHeader standHeader;
        private readonly Dictionary<int, Stand> standsByID;
        private readonly CruisedTreeHeader treeHeader;

        public OrganonVariant OrganonVariant { get; private init; }
        // workaround https://github.com/PowerShell/PowerShell/issues/20066 with a Stands property rather than implementing IList<Stand>
        public IList<Stand> Stands { get; private init; }

        public CruisedStands(TreeModel growthModel)
        {
            this.nextTreeID = 0;
            this.standHeader = new();
            this.standsByID = [];
            this.treeHeader = new();

            this.OrganonVariant = OrganonVariant.Create(growthModel);
            this.Stands = [];
        }

        public int Count
        {
            get { return this.Stands.Count; }
        }

        // https://github.com/PowerShell/PowerShell/issues/20066
        //bool ICollection<Stand>.IsReadOnly
        //{ 
        //    get { return false; } 
        //}

        //public Stand this[int index]
        //{
        //    get { return this.Stands[index]; }
        //    set { this.Stands[index] = value; }
        //}

        //void ICollection<Stand>.Add(Stand item)
        //{
        //    this.Stands.Add(item);
        //}

        //void ICollection<Stand>.Clear()
        //{
        //    this.Stands.Clear();
        //}

        //bool ICollection<Stand>.Contains(Stand item)
        //{
        //    return this.Stands.Contains(item);
        //}

        //void ICollection<Stand>.CopyTo(Stand[] array, int arrayIndex)
        //{
        //    this.Stands.CopyTo(array, arrayIndex);
        //}

        //bool ICollection<Stand>.Remove(Stand item)
        //{
        //    return this.Stands.Remove(item);
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return this.Stands.GetEnumerator();
        //}

        //IEnumerator<Stand> IEnumerable<Stand>.GetEnumerator()
        //{
        //    return this.Stands.GetEnumerator();
        //}

        //int IList<Stand>.IndexOf(Stand item)
        //{
        //    return this.Stands.IndexOf(item);
        //}

        //void IList<Stand>.Insert(int index, Stand item)
        //{
        //    this.Stands.Insert(index, item);
        //}

        //void IList<Stand>.RemoveAt(int index)
        //{
        //    this.Stands.RemoveAt(index);
        //}

        private void ParseStandRow(int rowIndex, string[] rowAsStrings)
        {
            if (rowIndex == 0)
            {
                this.standHeader.Parse(rowAsStrings);
                return;
            }

            int standID = Int32.Parse(rowAsStrings[this.standHeader.ID]);
            float areaInHa = Single.Parse(rowAsStrings[this.standHeader.Area]);
            if (Single.IsNaN(areaInHa) || (areaInHa <= 0.0F) || (areaInHa > 1000.0F))
            {
                throw new XmlException("Stand area of " + areaInHa + " ha is unexpectedly large or small.", null, rowIndex + 1, this.standHeader.Area);
            }

            float siteIndexInM = Single.Parse(rowAsStrings[this.standHeader.SiteIndex]);
            if (Single.IsNaN(siteIndexInM) || (siteIndexInM <= 0.0F) || (siteIndexInM > 200.0F))
            {
                throw new XmlException("Site index of " + siteIndexInM + " m is unexpectedly large or small.", null, rowIndex + 1, this.standHeader.SiteIndex);
            }

            string ageAsString = rowAsStrings[this.standHeader.Age];
            int ageInYears = -1;
            if (String.IsNullOrWhiteSpace(ageAsString) == false)
            {
                ageInYears = Int32.Parse(ageAsString);
                if (Single.IsNaN(ageInYears) || (ageInYears < 0) || (ageInYears > 1000))
                {
                    throw new XmlException("Age of " + ageInYears + " years is unexpectedly large or small.", null, rowIndex + 1, this.standHeader.Age);
                }
            }

            float slopeInPercent = Single.Parse(rowAsStrings[this.standHeader.SlopeInPercent]);
            if (Single.IsNaN(slopeInPercent) || (slopeInPercent < 0.0F) || (slopeInPercent > 200.0F))
            {
                throw new XmlException("Slope of " + slopeInPercent + " % is unexpectedly large or small.", null, rowIndex + 1, this.standHeader.SlopeInPercent);
            }

            float forwardingRoadDistanceInM = Single.Parse(rowAsStrings[this.standHeader.ForwardingRoad]);
            if (Single.IsNaN(forwardingRoadDistanceInM) || (forwardingRoadDistanceInM < 0.0F) || (forwardingRoadDistanceInM > 5000.0F))
            {
                throw new XmlException("On road forwarding distance of " + forwardingRoadDistanceInM + " m is unexpectedly large or small.", null, rowIndex + 1, this.standHeader.ForwardingRoad);
            }

            float forwardingUntetheredDistanceInM = Single.Parse(rowAsStrings[this.standHeader.ForwardingUnthered]);
            if (Single.IsNaN(forwardingUntetheredDistanceInM) || (forwardingUntetheredDistanceInM < 0.0F) || (forwardingUntetheredDistanceInM > 2000.0F))
            {
                throw new XmlException("Untethered forwarding distance of " + forwardingUntetheredDistanceInM + " m is unexpectedly large or small.", null, rowIndex + 1, this.standHeader.ForwardingUnthered);
            }

            float forwardingTetheredDistanceInM = Single.Parse(rowAsStrings[this.standHeader.ForwardingTethered]);
            if (Single.IsNaN(forwardingTetheredDistanceInM) || (forwardingTetheredDistanceInM < 0.0F) || (forwardingTetheredDistanceInM > 2000.0F))
            {
                throw new XmlException("Tethered forwarding distance of " + forwardingTetheredDistanceInM + " m is unexpectedly large or small.", null, rowIndex + 1, this.standHeader.ForwardingTethered);
            }

            float meanYardingDistanceFactor = Single.Parse(rowAsStrings[this.standHeader.YardingFactor]);
            if (Single.IsNaN(meanYardingDistanceFactor) || (meanYardingDistanceFactor < 0.0F) || (meanYardingDistanceFactor > 2.0F))
            {
                throw new XmlException("Yarding distance factor of " + meanYardingDistanceFactor + " is unexpectedly large or small.", null, rowIndex + 1, this.standHeader.YardingFactor);
            }

            float plantingDensityInTph = Single.Parse(rowAsStrings[this.standHeader.PlantingDensityPerHa]);
            if (Single.IsNaN(plantingDensityInTph) || (plantingDensityInTph < 0.0F) || (plantingDensityInTph > 5000.0F))
            {
                throw new XmlException("Planting density of " + plantingDensityInTph + " trees per hectare is unexpectedly large or small.", null, rowIndex + 1, this.standHeader.PlantingDensityPerHa);
            }

            OrganonStand stand = new(this.OrganonVariant, ageInYears, Constant.FeetPerMeter * siteIndexInM)
            {
                // minimum distance to road > 0 could be converted to access distance
                // This seems best done by an external caller.
                // AccessDistanceInM = 0,
                // AccessSlopeInPercent = 0,
                // AgeInYears is passed as constructor argument and flowed to BreastHeightAgeInYears
                AreaInHa = areaInHa,
                // CorridorLengthInM, CorridorLengthInMTethered, CorridorLengthInMUntethered are set by SetCorridorLength()
                // DouglasFirSiteConstants and HemlockSiteIndexInFeet are set internally
                ForwardingDistanceOnRoad = forwardingRoadDistanceInM,
                MeanYardingDistanceFactor = meanYardingDistanceFactor,
                Name = standID.ToString(),
                PlantingDensityInTreesPerHectare = plantingDensityInTph,
                // RedAlderGrowthEffectiveAge and RedAlderSiteIndexInFeet are set by SetRedAlderSiteIndexAndGrowthEffectiveAge()
                // SiteIndexInFeet is passed as constructor argument
                SlopeInPercent = slopeInPercent
            };
            stand.SetCorridorLength(forwardingTetheredDistanceInM, forwardingUntetheredDistanceInM);
            // other set calls are necessarily deferred until trees have been read
            this.standsByID.Add(standID, stand);
        }

        private void ParseTreeRow(int rowIndex, string[] rowAsStrings)
        {
            if (rowIndex == 0)
            {
                this.treeHeader.Parse(rowAsStrings);

                if (this.treeHeader.Age != -1)
                {
                    // currently no way of tracking individual tree ages
                    throw new NotSupportedException(nameof(this.treeHeader.Age));
                }

                return;
            }

            string standIDasString = rowAsStrings[this.treeHeader.Stand];
            if (String.IsNullOrEmpty(standIDasString))
            {
                return; // assume end of data in file
            }

            int standID = Int32.Parse(standIDasString);
            if (this.standsByID.TryGetValue(standID, out Stand? stand) == false)
            {
                throw new XmlException("Stand ID " + standID + " is not present in stands list.", null, rowIndex + 1, this.treeHeader.Stand);
            }

            FiaCode species = FiaCodeExtensions.Parse(rowAsStrings[this.treeHeader.Species]);
            int plot = Constant.Default.PlotID;
            if (this.treeHeader.Plot >= 0)
            {
                plot = Int32.Parse(rowAsStrings[this.treeHeader.Plot]);
            }

            int tag = this.nextTreeID;
            if (this.treeHeader.Tag >= 0)
            {
                tag = Int32.Parse(rowAsStrings[this.treeHeader.Tag]);
            }

            float dbh = Single.Parse(rowAsStrings[this.treeHeader.Dbh]);
            float height = Single.Parse(rowAsStrings[this.treeHeader.Height]);
            float expansionFactor = Single.Parse(rowAsStrings[this.treeHeader.ExpansionFactor]);

            TreeConditionCode code = TreeConditionCode.Live;
            if (this.treeHeader.Codes >= 0)
            {
                code = TreeConditionCodeExtensions.Parse(rowAsStrings[this.treeHeader.Codes]);
            }

            // TODO: heightToBrokenTop?

            // add tree with placeholder crown ratio
            if (stand.TreesBySpecies.TryGetValue(species, out Trees? treesOfSpecies) == false)
            {
                treesOfSpecies = new Trees(species, minimumSize: 1, Units.Metric);
                stand.TreesBySpecies.Add(species, treesOfSpecies);
            }

            treesOfSpecies.Add(plot, tag, dbh, height, this.DefaultCrownRatio, expansionFactor, code);
            ++this.nextTreeID;
        }

        public void Read(string xlsxFilePath, string standWorksheetName, string treesWorksheetName)
        {
            // read stands and trees
            XlsxReader reader = new();
            XlsxReader.ReadWorksheet(xlsxFilePath, standWorksheetName, this.ParseStandRow);
            XlsxReader.ReadWorksheet(xlsxFilePath, treesWorksheetName, this.ParseTreeRow);

            // set height to crown base on all trees
            // filter any stands without trees on the assumption that they were not cruised
            this.Stands.Clear();
            foreach (Stand stand in this.standsByID.Values)
            {
                if (stand.TreesBySpecies.Count > 0)
                {
                    if (stand is OrganonStand organonStand)
                    {
                        if (organonStand.GetUnits() == Units.Metric)
                        {
                            organonStand.SetUnits(Units.English);
                        }
                        organonStand.SetRedAlderSiteIndexAndGrowthEffectiveAge();
                        organonStand.SetHeightToCrownBase(this.OrganonVariant);
                    }
                    this.Stands.Add(stand);
                }
            }

            // reset for next read
            this.standsByID.Clear();
        }

        private class CruisedStandHeader
        {
            public int ID { get; set; }
            public int Age { get; set; }
            public int Area { get; set; }
            public int ForwardingRoad { get; set; }
            public int ForwardingUnthered { get; set; }
            public int ForwardingTethered { get; set; }
            public int PlantingDensityPerHa { get; set; }
            public int SiteIndex { get; set; }
            public int SlopeInPercent { get; set; }
            public int YardingFactor { get; set; }

            public CruisedStandHeader()
            {
                this.Age = -1;
                this.Area = -1;
                this.ForwardingRoad = -1;
                this.ForwardingUnthered = -1;
                this.ForwardingTethered = -1;
                this.ID = -1;
                this.PlantingDensityPerHa = -1;
                this.SiteIndex = -1;
                this.SlopeInPercent = -1;
                this.YardingFactor = -1;
            }

            public void Parse(string[] rowAsStrings)
            {
                // parse header
                for (int columnIndex = 0; columnIndex < rowAsStrings.Length; ++columnIndex)
                {
                    string columnHeader = rowAsStrings[columnIndex];
                    if (String.IsNullOrWhiteSpace(columnHeader))
                    {
                        break;
                    }

                    switch (columnHeader)
                    {
                        case "id":
                            this.ID = columnIndex;
                            break;
                        case "area":
                            this.Area = columnIndex;
                            break;
                        case "siteIndex":
                            this.SiteIndex = columnIndex;
                            break;
                        case "age":
                            this.Age = columnIndex;
                            break;
                        case "slopeInPercent":
                            this.SlopeInPercent = columnIndex;
                            break;
                        case "forwardingRoad":
                            this.ForwardingRoad = columnIndex;
                            break;
                        case "forwardingUntethered":
                            this.ForwardingUnthered = columnIndex;
                            break;
                        case "forwardingTethered":
                            this.ForwardingTethered = columnIndex;
                            break;
                        case "yardingFactor":
                            this.YardingFactor = columnIndex;
                            break;
                        case "plantingDensityPerHa":
                            this.PlantingDensityPerHa = columnIndex;
                            break;
                        // currently ignored fields
                        case "slopeAbove100PercentFraction":
                            break;
                        default:
                            throw new NotSupportedException("Unhandled column " + columnHeader + ".");
                    }
                }

                // check header
                if (this.ID < 0)
                {
                    throw new XmlException("Stand ID column not found.");
                }
                if (this.Area < 0)
                {
                    throw new XmlException("Area column not found.");
                }
                if (this.SiteIndex < 0)
                {
                    throw new XmlException("Site index column not found.");
                }
                if (this.Age < 0)
                {
                    throw new XmlException("Age column not found.");
                }
                if (this.SlopeInPercent < 0)
                {
                    throw new XmlException("Slope column not found.");
                }
                if (this.ForwardingRoad < 0)
                {
                    throw new XmlException("Forwarding distance on road column not found.");
                }
                if (this.ForwardingUnthered < 0)
                {
                    throw new XmlException("Untethred forwarding distance column not found.");
                }
                if (this.ForwardingTethered < 0)
                {
                    throw new XmlException("Tethered forwarding distance not found.");
                }
                if (this.YardingFactor < 0)
                {
                    throw new XmlException("Yarding factor column not found.");
                }
                if (this.PlantingDensityPerHa < 0)
                {
                    throw new XmlException("Replanting density column not found.");
                }
            }
        }

        private class CruisedTreeHeader
        {
            public int Age { get; set; }
            public int Codes { get; set; }
            public int Dbh { get; set; }
            public int ExpansionFactor { get; set; }
            public int Height { get; set; }
            public int HeightToBrokenTop { get; set; }
            public int Plot { get; set; }
            public int Species { get; set; }
            public int Stand { get; set; }
            public int Tag { get; set; }

            public CruisedTreeHeader()
            {
                this.Age = -1;
                this.Codes = -1;
                this.Dbh = -1;
                this.ExpansionFactor = -1;
                this.Height = -1;
                this.HeightToBrokenTop = -1;
                this.Plot = -1;
                this.Species = -1;
                this.Stand = -1;
                this.Tag = -1;
            }

            public void Parse(string[] rowAsStrings)
            {
                // parse header
                for (int columnIndex = 0; columnIndex < rowAsStrings.Length; ++columnIndex)
                {
                    string columnHeader = rowAsStrings[columnIndex];
                    if (String.IsNullOrWhiteSpace(columnHeader))
                    {
                        break;
                    }

                    switch (columnHeader)
                    {
                        case "stand":
                            this.Stand = columnIndex;
                            break;
                        case "plot":
                            this.Plot = columnIndex;
                            break;
                        case "tag":
                            this.Tag = columnIndex;
                            break;
                        case "species":
                            this.Species = columnIndex;
                            break;
                        case "age":
                            this.Age = columnIndex;
                            break;
                        case "dbh":
                            this.Dbh = columnIndex;
                            break;
                        case "height":
                            this.Height = columnIndex;
                            break;
                        case "expansionFactor":
                            this.ExpansionFactor = columnIndex;
                            break;
                        case "codes":
                            this.Codes = columnIndex;
                            break;
                        case "heightToBrokenTop":
                            this.HeightToBrokenTop = columnIndex;
                            break;
                        default:
                            throw new NotSupportedException("Unhandled column " + columnHeader + ".");
                    }
                }

                // check header
                if (this.Stand < 0)
                {
                    throw new XmlException("Stand column not found.");
                }
                if (this.Species < 0)
                {
                    throw new XmlException("Species column not found.");
                }
                if (this.Dbh < 0)
                {
                    throw new XmlException("DBH column not found.");
                }
                if (this.Height < 0)
                {
                    throw new XmlException("Height column not found.");
                }
                if (this.ExpansionFactor < 0)
                {
                    throw new XmlException("Expansion factor column not found.");
                }
                if (this.Codes < 0)
                {
                    throw new XmlException("Codes column not found.");
                }
                if (this.HeightToBrokenTop < 0)
                {
                    throw new XmlException("Height to broken top column not found.");
                }

                // age is optional as age may be defined at the stand level
                // plot is optional as it's not currently used
                // tag is optional
            }
        }
    }
}
