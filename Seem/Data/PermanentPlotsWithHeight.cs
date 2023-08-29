using Mars.Seem.Extensions;
using Mars.Seem.Organon;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Mars.Seem.Data
{
    public class PermanentPlotsWithHeight : TreeReader
    {
        private readonly PlotTreeHeader csvHeader;
        private readonly float defaultExpansionFactorPerHa;
        private readonly IList<int> plotIDs;
        private readonly SortedList<int, Stand> standByAge;

        public IList<FiaCode> ExcludeSpecies { get; set; }
        public SortedList<int, float> ExpansionFactorPerHaByAge { get; init; }
        public bool IncludeSpacingAndReplicateInTag { get; set; }

        public PermanentPlotsWithHeight(IList<int> plotIDs)
        {
            if (plotIDs.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(plotIDs));
            }

            this.csvHeader = new();
            this.defaultExpansionFactorPerHa = -1.0F;
            this.plotIDs = plotIDs;
            this.standByAge = new SortedList<int, Stand>();

            this.ExcludeSpecies = Array.Empty<FiaCode>();
            this.ExpansionFactorPerHaByAge = new();
            this.IncludeSpacingAndReplicateInTag = false;
        }

        public PermanentPlotsWithHeight(IList<int> plotIDs, float defaultExpansionFactorPerHa)
            : this(plotIDs)
        {
            if ((defaultExpansionFactorPerHa <= 0.0F) || (defaultExpansionFactorPerHa > Constant.Maximum.ExpansionFactorPerHa))
            {
                throw new ArgumentOutOfRangeException(nameof(defaultExpansionFactorPerHa));
            }

            this.defaultExpansionFactorPerHa = defaultExpansionFactorPerHa;
        }

        public IList<int> Ages
        {
            get { return this.standByAge.Keys; }
        }

        private void ParseRow(int rowIndex, string[] rowAsStrings)
        {
            if (rowIndex == 0)
            {
                this.csvHeader.Parse(rowAsStrings);

                if ((this.csvHeader.ExpansionFactor < 0) && (this.defaultExpansionFactorPerHa <= 0.0F))
                {
                    throw new ArgumentOutOfRangeException(nameof(this.csvHeader.ExpansionFactor), "Expansion factor column not found or default expansion factor not specified.");
                }
                if (this.IncludeSpacingAndReplicateInTag)
                {
                    if ((this.csvHeader.Spacing < 0) && (this.csvHeader.Replicate < 0))
                    {
                        throw new XmlException(nameof(this.IncludeSpacingAndReplicateInTag) + " is set but spacing and replicate columns are not available.");
                    }
                }

                return;
            }

            string tagAsString = rowAsStrings[this.csvHeader.Tree];
            if (String.IsNullOrEmpty(tagAsString))
            {
                return; // assume end of data in file
            }

            // filter by plot
            int plot = Int32.Parse(rowAsStrings[this.csvHeader.Plot]);
            bool treeInPlotList = false;
            for (int idIndex = 0; idIndex < this.plotIDs.Count; ++idIndex)
            {
                if (plot == this.plotIDs[idIndex])
                {
                    treeInPlotList = true;
                    break;
                }
            }
            if (treeInPlotList == false)
            {
                // tree is not in the list of plots specified
                return;
            }


            // filter by species
            FiaCode species = FiaCodeExtensions.Parse(rowAsStrings[this.csvHeader.Species]);
            if (this.ExcludeSpecies.Count > 0)
            {
                if (this.ExcludeSpecies.Contains(species))
                {
                    return;
                }
            }

            // for now, exclude dead and harvested trees
            // TODO: include dead for snag calculations
            string treeConditionAsString = rowAsStrings[this.csvHeader.TreeCondition];
            if (String.IsNullOrWhiteSpace(treeConditionAsString) == false)
            {
                int treeCondition = Int32.Parse(treeConditionAsString);
                if ((treeCondition == Constant.MalcolmKnapp.TreeCondition.Harvested) ||
                    (treeCondition == Constant.MalcolmKnapp.TreeCondition.Dead))
                {
                    return;
                }
            }

            // find plot and tree data structures
            int ageInYears = Int32.Parse(rowAsStrings[this.csvHeader.Age]);
            if (this.standByAge.TryGetValue(ageInYears, out Stand? plotAtAge) == false)
            {
                plotAtAge = new Stand();
                this.standByAge.Add(ageInYears, plotAtAge);
            }

            if (plotAtAge.TreesBySpecies.TryGetValue(species, out Trees? treesOfSpecies) == false)
            {
                treesOfSpecies = new(species, minimumSize: 1, Units.Metric);
                plotAtAge.TreesBySpecies.Add(species, treesOfSpecies);
            }

            // parse remaining data
            // for now, assume data is clean so that tag numbers are unique within each (sub)plot
            int tag = Int32.Parse(tagAsString);
            if (this.IncludeSpacingAndReplicateInTag)
            {
                if ((tag < 0) || (tag > 999))
                {
                    throw new XmlException("Setting " + nameof(this.IncludeSpacingAndReplicateInTag) + " requires tag numbers be in [0, 99].", null, rowIndex + 1, this.csvHeader.Replicate);
                }
                int replicate = Int32.Parse(rowAsStrings[this.csvHeader.Replicate]);
                if ((replicate < 0) || (replicate > 99))
                {
                    throw new XmlException("Setting " + nameof(this.IncludeSpacingAndReplicateInTag) + " requires replicate numbers in [0, 99].", null, rowIndex + 1, this.csvHeader.Replicate);
                }
                float spacing = Single.Parse(rowAsStrings[this.csvHeader.Spacing]);
                if (spacing <= 0.0F)
                {
                    throw new XmlException("Setting " + nameof(this.IncludeSpacingAndReplicateInTag) + " requires positive spacings.", null, rowIndex + 1, this.csvHeader.Spacing);
                }

                // form extended tag number as ssrrttt where ss = spacing in dm, r = replicate number, and tt = tag number
                tag += 100 * 1000 * (int)MathF.Round(10.0F * spacing) + 1000 * replicate;
            }

            string dbhAsString = rowAsStrings[this.csvHeader.Dbh];
            float dbhInCm = Single.NaN;
            if ((String.IsNullOrWhiteSpace(dbhAsString) == false) && (String.Equals(dbhAsString, "NA", StringComparison.OrdinalIgnoreCase) == false))
            {
                dbhInCm = Single.Parse(dbhAsString);
            }
            string heightAsString = rowAsStrings[this.csvHeader.Height];
            float heightInM = Single.NaN;
            if ((String.IsNullOrWhiteSpace(heightAsString) == false) && (String.Equals(heightAsString, "NA", StringComparison.OrdinalIgnoreCase) == false))
            {
                heightInM = Single.Parse(heightAsString);
            }
            float expansionFactorPerHa = this.defaultExpansionFactorPerHa;
            if (this.csvHeader.ExpansionFactor >= 0)
            {
                expansionFactorPerHa = Single.Parse(rowAsStrings[this.csvHeader.ExpansionFactor]);
            }
            else if (this.ExpansionFactorPerHaByAge.Count > 0)
            {
                if (this.ExpansionFactorPerHaByAge.TryGetValue(ageInYears, out float expansionFactorForAge))
                {
                    expansionFactorPerHa = expansionFactorForAge;
                }
            }

            // add trees with placeholder crown ratio
            treesOfSpecies.Add(plot, tag, dbhInCm, heightInM, this.DefaultCrownRatio, expansionFactorPerHa, TreeConditionCode.Live);
        }

        public void Read(string xlsxFilePath, string worksheetName)
        {
            XlsxReader reader = new();
            XlsxReader.ReadWorksheet(xlsxFilePath, worksheetName, this.ParseRow);
        }

        public OrganonStand ToOrganonStand(OrganonConfiguration configuration, int ageInYears, float primarySpeciesSiteIndexInM, float hemlockSiteIndexInM, int maximumTreeRecords, ImputationMethod imputationMethod)
        {
            if (this.Ages.Contains(ageInYears) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(ageInYears));
            }
            if ((primarySpeciesSiteIndexInM < 0.0F) || (primarySpeciesSiteIndexInM > Constant.Maximum.SiteIndexInM))
            {
                throw new ArgumentOutOfRangeException(nameof(primarySpeciesSiteIndexInM));
            }
            if ((hemlockSiteIndexInM < 0.0F) || (hemlockSiteIndexInM > Constant.Maximum.SiteIndexInM))
            {
                throw new ArgumentOutOfRangeException(nameof(primarySpeciesSiteIndexInM));
            }
            if (maximumTreeRecords < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumTreeRecords));
            }

            // find requested, preceeding, and following measurements
            HeightDiameterImputation imputation = new(ageInYears);
            Stand? plotAtAge = null;
            for (int ageIndex = 0; ageIndex < this.standByAge.Count; ++ageIndex)
            {
                int age = this.standByAge.Keys[ageIndex];
                plotAtAge = this.standByAge.Values[ageIndex];
                if (age == ageInYears)
                {
                    if (ageInYears < this.standByAge.Count - 1)
                    {
                        imputation.NextMeasurementAgeInYears = this.standByAge.Keys[ageIndex + 1];
                        imputation.PlotAtNextMeasurement = this.standByAge.Values[ageIndex + 1];
                    }
                    break;
                }

                imputation.PlotAtPreviousMeasurement = plotAtAge;
                imputation.PreviousMeasurementAgeInYears = age;
            }
            if (plotAtAge == null)
            {
                throw new ArgumentOutOfRangeException(nameof(ageInYears));
            }

            // create Organon stand
            int treesCopied = 0;
            StringBuilder plotIDsAsString = new(this.plotIDs[0].ToString(CultureInfo.InvariantCulture));
            for (int index = 1; index < this.plotIDs.Count; ++index)
            {
                plotIDsAsString.Append("y" + this.plotIDs[index].ToString(CultureInfo.InvariantCulture));
            }
            OrganonStand organonStand = new(configuration.Variant, ageInYears, Constant.FeetPerMeter * primarySpeciesSiteIndexInM)
            {
                HemlockSiteIndexInFeet = Constant.FeetPerMeter * hemlockSiteIndexInM,
                Name = plotIDsAsString.ToString()
            };

            // copy trees from plot to Organon stand with default crown ratios
            // For now, when the stand size is limited this just copies the first n trees encountered rather than subsampling the plot.
            // Can move this to a Trees.CopyFrom() and Trees.ChangeUnits() if needed.
            int maximumTreesToCopy = Math.Min(plotAtAge.GetTreeRecordCount(), maximumTreeRecords);
            for (int ageIndex = 0; ageIndex < plotAtAge.TreesBySpecies.Count; ++ageIndex)
            {
                // get or create Organon trees
                Trees plotTreesOfSpecies = plotAtAge.TreesBySpecies.Values[ageIndex];
                if (organonStand.TreesBySpecies.TryGetValue(plotTreesOfSpecies.Species, out Trees? standTreesOfSpecies) == false)
                {
                    int minimumSize = Math.Min(maximumTreesToCopy - treesCopied, plotTreesOfSpecies.Count);
                    standTreesOfSpecies = new Trees(plotTreesOfSpecies.Species, minimumSize, Units.English);
                    organonStand.TreesBySpecies.Add(plotTreesOfSpecies.Species, standTreesOfSpecies);
                }

                // copy trees
                // Depending on mortality, ingrowth, plot boundary shifts, and measurement errors trees of a given species may or may not
                // be present in previous and subsequent measurements. So the two lookups immediately below use TryGetValue().
                imputation.TryFindPreviousAndNextTrees(plotTreesOfSpecies.Species);
                for (int treeIndex = 0; treeIndex < plotTreesOfSpecies.Count; ++treeIndex)
                {
                    int plot = plotTreesOfSpecies.Plot[treeIndex];
                    int tag = plotTreesOfSpecies.Tag[treeIndex];
                    float dbhInInches = Constant.InchesPerCentimeter * plotTreesOfSpecies.Dbh[treeIndex];
                    float heightInFeet = Constant.FeetPerMeter * plotTreesOfSpecies.Height[treeIndex];
                    if (Single.IsNaN(dbhInInches) || (dbhInInches <= 0.0F) ||
                        Single.IsNaN(heightInFeet) || (heightInFeet <= 0.0F))
                    {
                        if (imputationMethod == ImputationMethod.None)
                        {
                            throw new NotSupportedException("Tree " + tag + " on plot " + plot + " has a missing, zero, or negative diameter or height at age " + ageInYears + ".");
                        }
                        else if (imputationMethod == ImputationMethod.SimpleLinearAssumeDead)
                        {
                            if (imputation.TryImputeSimpleLinear(plot, tag, ref dbhInInches, ref heightInFeet) == false)
                            {
                                continue; // imputation wasn't possible, assume dead
                            }
                            Debug.Assert((dbhInInches > 0.0F) && (heightInFeet > 0.0F));
                        }
                        else
                        {
                            throw new NotSupportedException("Unhandled imputation method " + imputationMethod + ".");
                        }
                    }

                    float liveExpansionFactorPerAcre = Constant.HectaresPerAcre * plotTreesOfSpecies.LiveExpansionFactor[treeIndex];
                    standTreesOfSpecies.Add(plot, tag, dbhInInches, heightInFeet, this.DefaultCrownRatio, liveExpansionFactorPerAcre, TreeConditionCode.Live);
                    if (++treesCopied >= maximumTreesToCopy)
                    {
                        break; // break inner for loop
                    }
                }
                if (++treesCopied >= maximumTreesToCopy)
                {
                    break; // break foreach
                }
            }

            organonStand.SetRedAlderSiteIndexAndGrowthEffectiveAge();
            //organonStand.SetSdiMax(configuration);
            organonStand.SetHeightToCrownBase(configuration.Variant);

            // used for checking sensitivity to data order
            // Ordering not currently advantageous, so disabled for now.
            // foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            // {
            //    treesOfSpecies.SortByDbh();
            // }

            return organonStand;
        }

        private class HeightDiameterImputation
        {
            public int AgeInYears { get; set; }
            public Stand? PlotAtPreviousMeasurement { get; set; }
            public Stand? PlotAtNextMeasurement { get; set; }
            public Trees? PreviousPlotTreesOfSpecies { get; set; }
            public int PreviousMeasurementAgeInYears { get; set; }
            public int NextMeasurementAgeInYears { get; set; }
            public Trees? NextPlotTreesOfSpecies { get; set; }

            public HeightDiameterImputation(int ageInYears)
            {
                this.AgeInYears = ageInYears;
            }

            public void TryFindPreviousAndNextTrees(FiaCode species)
            {
                this.PreviousPlotTreesOfSpecies = null;
                if (this.PlotAtPreviousMeasurement != null)
                {
                    if (this.PlotAtPreviousMeasurement.TreesBySpecies.TryGetValue(species, out Trees? previousPlotTreesOfSpecies))
                    {
                        this.PreviousPlotTreesOfSpecies = previousPlotTreesOfSpecies;
                    }
                }

                this.NextPlotTreesOfSpecies = null;
                if (this.PlotAtNextMeasurement != null)
                {
                    if (this.PlotAtNextMeasurement.TreesBySpecies.TryGetValue(species, out Trees? nextPlotTreesOfSpecies))
                    {
                        this.NextPlotTreesOfSpecies = nextPlotTreesOfSpecies;
                    }
                }
            }

            public bool TryImputeSimpleLinear(int plot, int tag, ref float dbhInInches, ref float heightInFeet)
            {
                // find previous and next measurements of tree, if available
                float previousMeasuredDbhInInches = Single.NaN;
                float previousMeasuredHeightInFeet = Single.NaN;
                float nextMeasuredDbhInInches = Single.NaN;
                float nextMeasuredHeightInFeet = Single.NaN;
                if (this.PreviousPlotTreesOfSpecies != null)
                {
                    int previousIndex = Array.BinarySearch(this.PreviousPlotTreesOfSpecies.Tag, tag);
                    if (previousIndex >= 0)
                    {
                        int previousPlot = this.PreviousPlotTreesOfSpecies.Plot[previousIndex];
                        if (previousPlot == plot)
                        {
                            previousMeasuredDbhInInches = Constant.InchesPerCentimeter * this.PreviousPlotTreesOfSpecies.Dbh[previousIndex];
                            previousMeasuredHeightInFeet = Constant.FeetPerMeter * this.PreviousPlotTreesOfSpecies.Height[previousIndex];
                        }
                        else
                        {
                            throw new NotImplementedException("Imputation over multiple plots is not currently supported.");
                        }
                    }
                }
                if (this.NextPlotTreesOfSpecies != null)
                {
                    int nextIndex = Array.BinarySearch(this.NextPlotTreesOfSpecies.Tag, tag);
                    if (nextIndex >= 0)
                    {
                        int nextPlot = this.NextPlotTreesOfSpecies.Plot[nextIndex];
                        if (nextPlot == plot)
                        {
                            nextMeasuredDbhInInches = Constant.InchesPerCentimeter * this.NextPlotTreesOfSpecies.Dbh[nextIndex];
                            nextMeasuredHeightInFeet = Constant.FeetPerMeter * this.NextPlotTreesOfSpecies.Height[nextIndex];
                        }
                        else
                        {
                            throw new NotImplementedException("Imputation over multiple plots is not currently supported.");
                        }
                    }
                }

                // if needed, impute diameter
                // If needed, this diameter imputation logic can be extended to consider height-diameter ratios.
                if (Single.IsNaN(dbhInInches) || (dbhInInches <= 0.0F))
                {
                    if (Single.IsNaN(nextMeasuredDbhInInches))
                    {
                        if (Single.IsNaN(previousMeasuredDbhInInches))
                        {
                            // throw new NotSupportedException("Could not impute diameter of tree " + tag + " on plot " + plot + " at age " + ageInYears + " as its diameter was not recoreded in either the previous or following stand measurement.");
                            return false; // tree isn't in previous or subsequent measurement; assume dead
                        }
                        else
                        {
                            // resort to constant annual diameter growth approximation as no other information is available
                            float annualDbhGrowthRate = previousMeasuredDbhInInches / this.PreviousMeasurementAgeInYears;
                            dbhInInches = annualDbhGrowthRate * this.AgeInYears;
                        }
                    }
                    else if (Single.IsNaN(previousMeasuredDbhInInches))
                    {
                        if (Single.IsNaN(nextMeasuredDbhInInches))
                        {
                            // throw new NotSupportedException("Could not impute diameter of tree " + tag + " on plot " + plot + " at age " + this.AgeInYears + " as its diameter was not recoreded in either the previous or following stand measurement.");
                            return false;
                        }

                        float annualDbhGrowthRate = nextMeasuredDbhInInches / this.NextMeasurementAgeInYears;
                        dbhInInches = annualDbhGrowthRate * nextMeasuredDbhInInches;
                    }
                    else
                    {
                        Debug.Assert((this.PreviousMeasurementAgeInYears > 0) && (this.AgeInYears > this.PreviousMeasurementAgeInYears) && (this.NextMeasurementAgeInYears > this.AgeInYears));

                        float annualDbhGrowthRate = (nextMeasuredDbhInInches - previousMeasuredDbhInInches) / (this.NextMeasurementAgeInYears - this.PreviousMeasurementAgeInYears);
                        int yearsOfGrowth = this.AgeInYears - this.PreviousMeasurementAgeInYears;
                        dbhInInches = previousMeasuredDbhInInches + annualDbhGrowthRate * yearsOfGrowth;
                    }
                }

                // if needed, impute height
                if (Single.IsNaN(heightInFeet) || (heightInFeet <= 0.0F))
                {
                    if (Single.IsNaN(nextMeasuredHeightInFeet))
                    {
                        if (Single.IsNaN(previousMeasuredHeightInFeet))
                        {
                            // tree is present in stand without any information as to how tall it is, resort to guess
                            // If needed, this can be changed to a species specific height-diameter regression.
                            Debug.Assert(Single.IsNaN(dbhInInches) == false);
                            heightInFeet = 75.0F / 12.0F * dbhInInches;
                        }
                        else
                        {
                            if (Single.IsNaN(previousMeasuredDbhInInches))
                            {
                                Debug.Assert(this.AgeInYears > 9);

                                // linear height growth approximation in lieu of any other information
                                float annualHeightGrowthRate = previousMeasuredHeightInFeet / this.PreviousMeasurementAgeInYears;
                                heightInFeet = annualHeightGrowthRate * this.AgeInYears;
                            }
                            else
                            {
                                // height-diameter ratio imputation
                                float previousHeightDiameterRatio = previousMeasuredHeightInFeet / previousMeasuredDbhInInches;
                                heightInFeet = previousHeightDiameterRatio * dbhInInches;
                            }
                            // could also check for a subsequent height-diameter ratio
                        }
                    }
                    else if (Single.IsNaN(previousMeasuredHeightInFeet))
                    {
                        if (Single.IsNaN(nextMeasuredHeightInFeet))
                        {
                            // tree is present in stand without any information as to how tall it is, resort to guess
                            // If needed, this can be changed to a species specific height-diameter regression.
                            Debug.Assert(Single.IsNaN(dbhInInches) == false);
                            heightInFeet = 75.0F / 12.0F * dbhInInches;
                        }
                        else
                        {
                            if (Single.IsNaN(previousMeasuredDbhInInches))
                            {
                                Debug.Assert(this.AgeInYears > 9);

                                // linear height growth approximation in lieu of any other information
                                float annualHeightGrowthRate = nextMeasuredHeightInFeet / this.NextMeasurementAgeInYears;
                                heightInFeet = annualHeightGrowthRate * this.AgeInYears;
                            }
                            else
                            {
                                // height-diameter ratio imputation
                                float previousHeightDiameterRatio = previousMeasuredHeightInFeet / previousMeasuredDbhInInches;
                                heightInFeet = previousHeightDiameterRatio * dbhInInches;
                            }
                            // could also check for a subsequent height-diameter ratio
                        }
                    }
                    else
                    {
                        Debug.Assert((this.PreviousMeasurementAgeInYears > 0) && (this.AgeInYears > this.PreviousMeasurementAgeInYears) && (this.NextMeasurementAgeInYears > this.AgeInYears));

                        // linear height imputation based on adjacent measurements
                        float annualHeightGrowthRate = (nextMeasuredHeightInFeet - previousMeasuredHeightInFeet) / (this.NextMeasurementAgeInYears - this.PreviousMeasurementAgeInYears);
                        int yearsOfGrowth = this.AgeInYears - this.PreviousMeasurementAgeInYears;
                        heightInFeet = previousMeasuredHeightInFeet + annualHeightGrowthRate * yearsOfGrowth;
                    }
                }

                return true;
            }
        }

        private class PlotTreeHeader
        {
            public int Age { get; set; }
            public int Dbh { get; set; }
            public int ExpansionFactor { get; set; }
            public int Height { get; set; }
            public int Plot { get; set; }
            public int Replicate { get; set; }
            public int Spacing { get; set; }
            public int Species { get; set; }
            public int Tree { get; set; }
            public int TreeCondition { get; set; }

            public PlotTreeHeader()
            {
                this.Age = -1;
                this.Dbh = -1;
                this.ExpansionFactor = -1;
                this.Height = -1;
                this.Plot = -1;
                this.Replicate = -1;
                this.Spacing = -1;
                this.Species = -1;
                this.Tree = -1;
                this.TreeCondition = -1;
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
                        case "species":
                            this.Species = columnIndex;
                            break;
                        case "plot":
                            this.Plot = columnIndex;
                            break;
                        case "spacing":
                            this.Spacing = columnIndex;
                            break;
                        case "replicate":
                            this.Replicate = columnIndex;
                            break;
                        case "tree":
                            this.Tree = columnIndex;
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
                        case "condition":
                            this.TreeCondition = columnIndex;
                            break;
                        // currently ignored columns
                        case "installation":
                        case "arc":
                        case "spoke":
                        case "x":
                        case "y":
                        case "measurement":
                        case "year":
                        case "heightCode":
                        case "heightToCrownBase":
                        case "maxCrownWidth":
                        case "damage":
                        case "leanSweepCode":
                        case "browseCode":
                        case "leaderCode":
                            break;
                        default:
                            throw new NotSupportedException("Unhandled column " + columnHeader + ".");
                    }

                    //else if (columnHeader.StartsWith("dbh", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    this.Dbh = columnIndex;
                    //    if (columnHeader.EndsWith("mm", StringComparison.Ordinal))
                    //    {
                    //        this.DbhScaleFactor = 0.1F; // convert from mm to cm, otherwise assume cm
                    //    }
                    //}
                    //else if (columnHeader.Equals("height", StringComparison.OrdinalIgnoreCase) ||
                    //         columnHeader.Equals("height, m", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    this.Height = columnIndex;
                    //}
                    //else if (columnHeader.Equals("height, dm", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    this.Height = columnIndex;
                    //    this.HeightScaleFactor = 0.1F; // convert from dm to m, otherwise assume m
                    //}
                }

                // check header
                if (this.Species < 0)
                {
                    throw new XmlException("Species column not found.");
                }
                if (this.Plot < 0)
                {
                    throw new XmlException("Plot column not found.");
                }
                if (this.Tree < 0)
                {
                    throw new XmlException("Tree number (tag ID) column not found.");
                }
                if (this.Age < 0)
                {
                    throw new XmlException("Tree age column not found.");
                }
                if (this.Dbh < 0)
                {
                    throw new XmlException("DBH column not found.");
                }
                if (this.Height < 0)
                {
                    throw new XmlException("Height column not found.");
                }
                if (this.TreeCondition < 0)
                {
                    throw new XmlException("Tree condition column not found.");
                }

                // expansion factor, spacing, and replicate columns are optional
            }
        }
    }
}
