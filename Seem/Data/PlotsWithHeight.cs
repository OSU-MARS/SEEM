using Mars.Seem.Extensions;
using Mars.Seem.Organon;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Mars.Seem.Data
{
    public class PlotsWithHeight
    {
        private int ageColumnIndex;
        private int dbhColumnIndex;
        private float dbhScaleFactor;
        private readonly float defaultCrownRatio;
        private readonly float defaultExpansionFactorPerHa;
        private int expansionFactorColumnIndex;
        private int heightColumnIndex;
        private float heightScaleFactor;
        private int plotColumnIndex;
        private readonly IList<int> plotIDs;
        private int speciesColumnIndex;
        private readonly Dictionary<int, Stand> standByAge;
        private int treeColumnIndex;
        private int treeConditionColumnIndex;

        public PlotsWithHeight(IList<int> plotIDs)
        {
            if (plotIDs.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(plotIDs));
            }

            this.ageColumnIndex = -1;
            this.dbhColumnIndex = -1;
            this.dbhScaleFactor = 1.0F;
            this.defaultCrownRatio = 0.5F;
            this.defaultExpansionFactorPerHa = -1.0F;
            this.expansionFactorColumnIndex = -1;
            this.heightColumnIndex = -1;
            this.heightScaleFactor = 1.0F;
            this.plotColumnIndex = -1;
            this.plotIDs = plotIDs;
            this.speciesColumnIndex = -1;
            this.standByAge = new Dictionary<int, Stand>();
            this.treeColumnIndex = -1;
            this.treeConditionColumnIndex = -1;
        }

        public PlotsWithHeight(IList<int> plotIDs, float defaultExpansionFactorPerHa)
            : this(plotIDs)
        {
            if ((defaultExpansionFactorPerHa <= 0.0F) || (defaultExpansionFactorPerHa > Constant.Maximum.ExpansionFactorPerHa))
            {
                throw new ArgumentOutOfRangeException(nameof(defaultExpansionFactorPerHa));
            }

            this.defaultExpansionFactorPerHa = defaultExpansionFactorPerHa;
        }

        private void ParseRow(int rowIndex, string[] rowAsStrings)
        {
            if (rowIndex == 0)
            {
                // parse header
                for (int columnIndex = 0; columnIndex < rowAsStrings.Length; ++columnIndex)
                {
                    string columnHeader = rowAsStrings[columnIndex];
                    if (String.IsNullOrWhiteSpace(columnHeader))
                    {
                        break;
                    }
                    if (columnHeader.Equals("species", StringComparison.OrdinalIgnoreCase))
                    {
                        this.speciesColumnIndex = columnIndex;
                    }
                    else if (columnHeader.Equals("plot", StringComparison.OrdinalIgnoreCase))
                    {
                        this.plotColumnIndex = columnIndex;
                    }
                    else if (columnHeader.Equals("tree", StringComparison.OrdinalIgnoreCase))
                    {
                        this.treeColumnIndex = columnIndex;
                    }
                    else if (columnHeader.Equals("age", StringComparison.OrdinalIgnoreCase))
                    {
                        this.ageColumnIndex = columnIndex;
                    }
                    else if (columnHeader.StartsWith("dbh", StringComparison.OrdinalIgnoreCase))
                    {
                        this.dbhColumnIndex = columnIndex;
                        if (columnHeader.EndsWith("mm", StringComparison.Ordinal))
                        {
                            this.dbhScaleFactor = 0.1F; // convert from mm to cm, otherwise assume cm
                        }
                    }
                    else if (columnHeader.Equals("height", StringComparison.OrdinalIgnoreCase) ||
                             columnHeader.Equals("height, m", StringComparison.OrdinalIgnoreCase))
                    {
                        this.heightColumnIndex = columnIndex;
                    }
                    else if (columnHeader.Equals("height, dm", StringComparison.OrdinalIgnoreCase))
                    {
                        this.heightColumnIndex = columnIndex;
                        this.heightScaleFactor = 0.1F; // convert from dm to m, otherwise assume m
                    }
                    else if (columnHeader.StartsWith("expansion factor", StringComparison.OrdinalIgnoreCase))
                    {
                        this.expansionFactorColumnIndex = columnIndex; // assume trees per hectare, if not specified default expansion factor is used
                    }
                    else if (columnHeader.Equals("treecond", StringComparison.OrdinalIgnoreCase))
                    {
                        this.treeConditionColumnIndex = columnIndex;
                    }
                    else
                    {
                        // ignore column for now
                    }
                }

                // check header
                if (this.speciesColumnIndex < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.speciesColumnIndex), "Species column not found.");
                }
                if (this.plotColumnIndex < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.plotColumnIndex), "Plot column not found.");
                }
                if (this.treeColumnIndex < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.treeColumnIndex), "Tree number column not found.");
                }
                if (this.ageColumnIndex < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ageColumnIndex), "Tree age column not found.");
                }
                if (this.dbhColumnIndex < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.dbhColumnIndex), "DBH column not found.");
                }
                if (this.heightColumnIndex < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.heightColumnIndex), "Height column not found.");
                }
                if ((this.expansionFactorColumnIndex < 0) && (this.defaultExpansionFactorPerHa <= 0.0F))
                {
                    throw new ArgumentOutOfRangeException(nameof(this.expansionFactorColumnIndex), "Expansion factor column not found or default expansion factor not specified.");
                }

                return;
            }

            if (rowAsStrings[this.treeColumnIndex] == null)
            {
                return; // assume end of data in file
            }

            // filter by plot
            int plot = Int32.Parse(rowAsStrings[this.plotColumnIndex]);
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

            // for now, exclude dead trees
            // TODO: include dead for snag calculations
            if (this.treeConditionColumnIndex >= 0)
            {
                if (String.IsNullOrWhiteSpace(rowAsStrings[this.treeConditionColumnIndex]) == false)
                {
                    int treeCondition = Int32.Parse(rowAsStrings[this.treeConditionColumnIndex]);
                    if (treeCondition == Constant.MalcolmKnapp.TreeCondition.Dead)
                    {
                        return;
                    }
                }
            }

            // parse data
            int ageInYears = Int32.Parse(rowAsStrings[this.ageColumnIndex]);
            if (this.standByAge.TryGetValue(ageInYears, out Stand? plotAtAge) == false)
            {
                plotAtAge = new Stand();
                this.standByAge.Add(ageInYears, plotAtAge);
            }

            FiaCode species = FiaCodeExtensions.Parse(rowAsStrings[this.speciesColumnIndex]);
            if (plotAtAge.TreesBySpecies.TryGetValue(species, out Trees? treesOfSpecies) == false)
            {
                treesOfSpecies = new Trees(species, 1, Units.Metric);
                plotAtAge.TreesBySpecies.Add(species, treesOfSpecies);
            }

            // for now, assume data is clean so that tag numbers are unique within each plot
            int tag = Int32.Parse(rowAsStrings[this.treeColumnIndex]);
            string dbhAsString = rowAsStrings[this.dbhColumnIndex];
            float dbhInCm = Single.NaN;
            if ((String.IsNullOrWhiteSpace(dbhAsString) == false) && (String.Equals(dbhAsString, "NA", StringComparison.OrdinalIgnoreCase) == false))
            {
                dbhInCm = this.dbhScaleFactor * Single.Parse(dbhAsString);
            }
            string heightAsString = rowAsStrings[this.heightColumnIndex];
            float heightInM = Single.NaN;
            if ((String.IsNullOrWhiteSpace(heightAsString) == false) && (String.Equals(heightAsString, "NA", StringComparison.OrdinalIgnoreCase) == false))
            {
                heightInM = this.heightScaleFactor * Single.Parse(heightAsString);
            }
            float expansionFactorPerHa = this.defaultExpansionFactorPerHa;
            if (this.expansionFactorColumnIndex >= 0)
            {
                expansionFactorPerHa = Single.Parse(rowAsStrings[this.expansionFactorColumnIndex]);
            }

            // add trees with placeholder crown ratio
            treesOfSpecies.Add(plot, tag, dbhInCm, heightInM, this.defaultCrownRatio, expansionFactorPerHa);
        }

        public void Read(string xlsxFilePath, string worksheetName)
        {
            XlsxReader reader = new();
            XlsxReader.ReadWorksheet(xlsxFilePath, worksheetName, this.ParseRow);
        }

        public OrganonStand ToOrganonStand(OrganonConfiguration configuration, int ageInYears, float siteIndexInM)
        {
            return this.ToOrganonStand(configuration, ageInYears, siteIndexInM, Int32.MaxValue);
        }

        public OrganonStand ToOrganonStand(OrganonConfiguration configuration, int ageInYears, float siteIndexInM, int maximumTreeRecords)
        {
            if ((ageInYears < 0) || (ageInYears > 1000))
            {
                throw new ArgumentOutOfRangeException(nameof(ageInYears));
            }
            if ((siteIndexInM < 0.0F) || (siteIndexInM > Constant.Maximum.SiteIndexInM))
            {
                throw new ArgumentOutOfRangeException(nameof(siteIndexInM));
            }
            if (maximumTreeRecords < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumTreeRecords));
            }

            // copy trees from plot to Organon stand with default crown ratios
            // For now, when the stand size is limited this just copies the first n trees encountered rather than subsampling the plot.
            // Can move this to a Trees.CopyFrom() and Trees.ChangeUnits() if needed.
            Stand plotAtAge = this.standByAge[ageInYears];
            int maximumTreesToCopy = Math.Min(plotAtAge.GetTreeRecordCount(), maximumTreeRecords);
            int treesCopied = 0;
            StringBuilder plotIDsAsString = new(this.plotIDs[0].ToString(CultureInfo.InvariantCulture));
            for (int index = 1; index < this.plotIDs.Count; ++index)
            {
                plotIDsAsString.Append("y" + this.plotIDs[index].ToString(CultureInfo.InvariantCulture));
            }
            OrganonStand organonStand = new(ageInYears, Constant.FeetPerMeter * siteIndexInM)
            {
                Name = plotIDsAsString.ToString()
            };
            foreach (Trees plotTreesOfSpecies in plotAtAge.TreesBySpecies.Values)
            {
                if (organonStand.TreesBySpecies.TryGetValue(plotTreesOfSpecies.Species, out Trees? standTreesOfSpecies) == false)
                {
                    int minimumSize = Math.Min(maximumTreesToCopy - treesCopied, plotTreesOfSpecies.Count);
                    standTreesOfSpecies = new Trees(plotTreesOfSpecies.Species, minimumSize, Units.English);
                    organonStand.TreesBySpecies.Add(plotTreesOfSpecies.Species, standTreesOfSpecies);
                }
                for (int treeIndex = 0; treeIndex < plotTreesOfSpecies.Count; ++treeIndex)
                {
                    int plot = plotTreesOfSpecies.Plot[treeIndex];
                    int tag = plotTreesOfSpecies.Tag[treeIndex];
                    float dbhInInches = Constant.InchesPerCentimeter * plotTreesOfSpecies.Dbh[treeIndex];
                    float heightInFeet = Constant.FeetPerMeter * plotTreesOfSpecies.Height[treeIndex];
                    float liveExpansionFactorPerAcre = Constant.HectaresPerAcre * plotTreesOfSpecies.LiveExpansionFactor[treeIndex];
                    if (Single.IsNaN(dbhInInches) || (dbhInInches <= 0.0F) ||
                        Single.IsNaN(heightInFeet) || (heightInFeet <= 0.0F))
                    {
                        throw new NotSupportedException("Tree " + tag + " has a missing, zero, or negative height or diameter at age " + ageInYears + ".");
                    }

                    standTreesOfSpecies.Add(plot, tag, dbhInInches, heightInFeet, defaultCrownRatio, liveExpansionFactorPerAcre);
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

            // estimate crown ratio
            if (configuration.Variant.TreeModel == TreeModel.OrganonSwo)
            {
                // TODO: if needed, add support for old index for NWO and SMC Pacific madrone
                throw new NotImplementedException("Old tree index not computed.");
            }

            organonStand.EnsureSiteIndicesSet(configuration.Variant);
            organonStand.SetRedAlderSiteIndexAndGrowthEffectiveAge();
            organonStand.SetSdiMax(configuration);

            float defaultOldIndex = 0.0F;
            OrganonStandDensity density = new(configuration.Variant, organonStand);
            foreach (Trees organonTreesOfSpecies in organonStand.TreesBySpecies.Values)
            {
                // initialize crown ratio from Organon variant
                Debug.Assert(organonTreesOfSpecies.Units == Units.English);
                for (int treeIndex = 0; treeIndex < organonTreesOfSpecies.Count; ++treeIndex)
                {
                    float dbhInInches = organonTreesOfSpecies.Dbh[treeIndex];
                    float heightInFeet = organonTreesOfSpecies.Height[treeIndex];
                    float crownCompetitionFactorLarger = density.GetCrownCompetitionFactorLarger(dbhInInches);
                    float heightToCrownBase = configuration.Variant.GetHeightToCrownBase(organonTreesOfSpecies.Species, heightInFeet, dbhInInches, crownCompetitionFactorLarger, density.BasalAreaPerAcre, organonStand.SiteIndexInFeet, organonStand.HemlockSiteIndexInFeet, defaultOldIndex);
                    float crownRatio = (heightInFeet - heightToCrownBase) / heightInFeet;
                    Debug.Assert(crownRatio >= 0.0F);
                    Debug.Assert(crownRatio <= 1.0F);

                    organonTreesOfSpecies.CrownRatio[treeIndex] = crownRatio;
                }

                // initialize crown ratio from FVS-PN dubbing
                // https://www.fs.fed.us/fmsc/ftp/fvs/docs/overviews/FVSpn_Overview.pdf, section 4.3.1
                // https://sourceforge.net/p/open-fvs/code/HEAD/tree/trunk/pn/crown.f#l67
                // for live > 1.0 inch DBH
                //   estimated crown ratio = d0 + d1 * 100.0 * SDI / SDImax
                //   PSME d0 = 5.666442, d1 = -0.025199
                if ((organonStand.TreesBySpecies.Count != 1) || (organonTreesOfSpecies.Species != FiaCode.PseudotsugaMenziesii))
                {
                    throw new NotImplementedException();
                }

                // FVS-PN crown ratio dubbing for Douglas-fir
                // Resulted in 0.28% less volume than Organon NWO on Malcolm Knapp Nelder 1 at stand age 70.
                // float qmd = stand.GetQuadraticMeanDiameter();
                // float reinekeSdi = density.TreesPerAcre * MathF.Pow(0.1F * qmd, 1.605F);
                // float reinekeSdiMax = MathF.Exp((stand.A1 - Constant.NaturalLogOf10) / stand.A2);
                // float meanCrownRatioFvs = 5.666442F - 0.025199F * 100.0F * reinekeSdi / reinekeSdiMax;
                // Debug.Assert(meanCrownRatioFvs >= 0.0F);
                // Debug.Assert(meanCrownRatioFvs <= 10.0F); // FVS uses a 0 to 10 range, so 10 = 100% crown ratio
                // float weibullA = 0.0F;
                // float weibullB = -0.012061F + 1.119712F * meanCrownRatioFvs;
                // float weibullC = 3.2126F;
                // int[] dbhOrder = treesOfSpecies.GetDbhSortOrder();

                // for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                // {
                //     float dbhFraction = (float)dbhOrder[treeIndex] / (float)treesOfSpecies.Count;
                //     float fvsCrownRatio = weibullA + weibullB * MathV.Pow(-1.0F * MathV.Ln(1.0F - dbhFraction), 1.0F / weibullC);
                //     Debug.Assert(fvsCrownRatio >= 0.0F);
                //     Debug.Assert(fvsCrownRatio <= 10.0F);

                //     treesOfSpecies.CrownRatio[treeIndex] = 0.1F * fvsCrownRatio;
                // }
            }

            // used for checking sensitivity to data order
            // Ordering not currently advantageous, so disabled for now.
            // foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            // {
            //    treesOfSpecies.SortByDbh();
            // }

            return organonStand;
        }
    }
}
