using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Osu.Cof.Ferm.Data
{
    public class PlotWithHeight
    {
        public List<int> Age { get; private set; }
        public List<float> DbhInCentimeters { get; private set; }
        public List<float> ExpansionFactorInTph { get; private set; }
        public List<float> HeightInMeters { get; private set; }
        public string Name { get; set; }
        public List<FiaCode> Species { get; private set; }
        public List<int> TreeID { get; private set; }

        public PlotWithHeight(string xlsxFilePath, string worksheetName)
        {
            this.Age = new List<int>();
            this.DbhInCentimeters = new List<float>();
            this.ExpansionFactorInTph = new List<float>();
            this.HeightInMeters = new List<float>();
            this.Name = Path.GetFileNameWithoutExtension(xlsxFilePath);
            this.Species = new List<FiaCode>();
            this.TreeID = new List<int>();

            XlsxReader reader = new XlsxReader();
            reader.ReadWorksheet(xlsxFilePath, worksheetName, this.ParseRow);
        }

        private void ParseRow(int rowIndex, string[] rowAsStrings)
        {
            if ((rowIndex == 0) || (rowAsStrings[Constant.Plot.ColumnIndex.Tree] == null))
            {
                return;
            }

            FiaCode species = FiaCodeExtensions.Parse(rowAsStrings[Constant.Plot.ColumnIndex.Species]);
            this.Species.Add(species);
            // for now, ignore plot
            this.TreeID.Add(Int32.Parse(rowAsStrings[Constant.Plot.ColumnIndex.Tree]));
            this.Age.Add(Int32.Parse(rowAsStrings[Constant.Plot.ColumnIndex.Age]));
            this.DbhInCentimeters.Add(0.1F * Single.Parse(rowAsStrings[Constant.Plot.ColumnIndex.DbhInMillimeters]));
            this.ExpansionFactorInTph.Add(Single.Parse(rowAsStrings[Constant.Plot.ColumnIndex.ExpansionFactor]));
            this.HeightInMeters.Add(0.1F * Single.Parse(rowAsStrings[Constant.Plot.ColumnIndex.HeightInDecimeters]));
        }

        public OrganonStand ToOrganonStand(OrganonConfiguration configuration, float siteIndex)
        {
            return this.ToOrganonStand(configuration, siteIndex, this.TreeID.Count);
        }

        public OrganonStand ToOrganonStand(OrganonConfiguration configuration, float siteIndex, int treesInStand)
        {
            if (treesInStand > this.TreeID.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(treesInStand));
            }

            List<int> uniqueAges = this.Age.Distinct().ToList();
            if (uniqueAges.Count != 1)
            {
                throw new NotSupportedException("Multi-age stands not currently supported.");
            }

            Dictionary<FiaCode, int> restrictedTreeCountBySpecies = new Dictionary<FiaCode, int>();
            for (int treeIndex = 0; treeIndex < treesInStand; ++treeIndex)
            {
                FiaCode species = this.Species[treeIndex];
                if (restrictedTreeCountBySpecies.TryGetValue(species, out int count) == false)
                {
                    restrictedTreeCountBySpecies.Add(species, 1);
                }
                else
                {
                    restrictedTreeCountBySpecies[species] = ++count;
                }
            }

            OrganonStand stand = new OrganonStand(uniqueAges[0], siteIndex)
            {
                Name = this.Name
            };
            foreach (KeyValuePair<FiaCode, int> treesOfSpecies in restrictedTreeCountBySpecies)
            {
                stand.TreesBySpecies.Add(treesOfSpecies.Key, new Trees(treesOfSpecies.Key, treesOfSpecies.Value, Units.English));
            }

            // add trees to stand with placeholder crown ratio
            float defaultCrownRatio = 0.6F;
            for (int treeIndex = 0; treeIndex < treesInStand; ++treeIndex)
            {
                Trees treesOfSpecies = stand.TreesBySpecies[this.Species[treeIndex]];
                Debug.Assert(treesOfSpecies.Capacity > treesOfSpecies.Count);
                float dbhInInches = Constant.InchesPerCm * this.DbhInCentimeters[treeIndex];
                float heightInFeet = Constant.FeetPerMeter * this.HeightInMeters[treeIndex];
                float liveExpansionFactor = Constant.HectaresPerAcre * this.ExpansionFactorInTph[treeIndex];

                treesOfSpecies.Add(this.TreeID[treeIndex], dbhInInches, heightInFeet, defaultCrownRatio, liveExpansionFactor);
            }

            // estimate crown ratio
            if (configuration.Variant.TreeModel == TreeModel.OrganonSwo)
            {
                // TODO: if needed, add support for old index for NWO and SMC Pacific madrone
                throw new NotImplementedException("Old tree index not computed.");
            }

            stand.SetDefaultAndMortalitySiteIndices(configuration.Variant.TreeModel);
            stand.SetRedAlderSiteIndexAndGrowthEffectiveAge();
            stand.SetSdiMax(configuration);

            float defaultOldIndex = 0.0F;
            OrganonStandDensity density = new OrganonStandDensity(stand, configuration.Variant);
            foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
            {
                // initialize crown ratio from Organon variant
                for (int treeIndex = 0; treeIndex < treesInStand; ++treeIndex)
                {
                    float dbhInInches = treesOfSpecies.Dbh[treeIndex];
                    float heightInFeet = treesOfSpecies.Height[treeIndex];
                    float crownCompetitionFactorLarger = density.GetCrownCompetitionFactorLarger(dbhInInches);
                    float heightToCrownBase = configuration.Variant.GetHeightToCrownBase(treesOfSpecies.Species, heightInFeet, dbhInInches, crownCompetitionFactorLarger, density.BasalAreaPerAcre, stand.SiteIndex, stand.HemlockSiteIndex, defaultOldIndex);
                    float crownRatio = (heightInFeet - heightToCrownBase) / heightInFeet;
                    Debug.Assert(crownRatio >= 0.0F);
                    Debug.Assert(crownRatio <= 1.0F);

                    treesOfSpecies.CrownRatio[treeIndex] = crownRatio;
                }

                // initialize crown ratio from FVS-PN dubbing
                // https://www.fs.fed.us/fmsc/ftp/fvs/docs/overviews/FVSpn_Overview.pdf, section 4.3.1
                // https://sourceforge.net/p/open-fvs/code/HEAD/tree/trunk/pn/crown.f#l67
                // for live > 1.0 inch DBH
                //   estimated crown ratio = d0 + d1 * 100.0 * SDI / SDImax
                //   PSME d0 = 5.666442, d1 = -0.025199
                if ((stand.TreesBySpecies.Count != 1) || (treesOfSpecies.Species != FiaCode.PseudotsugaMenziesii))
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

            return stand;
        }
    }
}
