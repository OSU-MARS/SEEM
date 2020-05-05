using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Osu.Cof.Ferm.Data
{
    public class NelderPlot
    {
        public List<float> DbhInCentimeters { get; private set; }
        public List<float> HeightInMeters { get; private set; }
        public string Name { get; set; }
        public List<FiaCode> Species { get; private set; }
        public List<int> TreeID { get; private set; }

        public NelderPlot(string xlsxFilePath, string worksheetName)
        {
            this.DbhInCentimeters = new List<float>();
            this.HeightInMeters = new List<float>();
            this.Name = Path.GetFileNameWithoutExtension(xlsxFilePath);
            this.Species = new List<FiaCode>();
            this.TreeID = new List<int>();

            XlsxReader reader = new XlsxReader();
            reader.ReadWorksheet(xlsxFilePath, worksheetName, this.ParseRow);
        }

        private void ParseRow(int rowIndex, string[] rowAsStrings)
        {
            if ((rowIndex == 0) || (rowAsStrings[Constant.Nelder.ColumnIndex.Tree] == null))
            {
                return;
            }

            this.DbhInCentimeters.Add(0.1F * float.Parse(rowAsStrings[Constant.Nelder.ColumnIndex.DbhInMillimeters]));
            this.HeightInMeters.Add(0.1F * float.Parse(rowAsStrings[Constant.Nelder.ColumnIndex.HeightInDecimeters]));
            FiaCode species = FiaCodeExtensions.Parse(rowAsStrings[Constant.Nelder.ColumnIndex.Species]);
            this.Species.Add(species);
            this.TreeID.Add(Int32.Parse(rowAsStrings[Constant.Nelder.ColumnIndex.Tree]));
        }

        public OrganonStand ToStand(float siteIndex)
        {
            return this.ToStand(siteIndex, this.TreeID.Count);
        }

        public OrganonStand ToStand(float siteIndex, int treesInStand)
        {
            if (treesInStand > this.TreeID.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(treesInStand));
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

            // TODO: read tree ages from .csv and use them to set stand age
            OrganonStand stand = new OrganonStand(20, siteIndex)
            {
                Name = this.Name
            };
            foreach (KeyValuePair<FiaCode, int> treesOfSpecies in restrictedTreeCountBySpecies)
            {
                stand.TreesBySpecies.Add(treesOfSpecies.Key, new Trees(treesOfSpecies.Key, treesOfSpecies.Value, Units.English));
            }

            for (int treeIndex = 0; treeIndex < treesInStand; ++treeIndex)
            {
                Trees treesOfSpecies = stand.TreesBySpecies[this.Species[treeIndex]];
                Debug.Assert(treesOfSpecies.Capacity > treesOfSpecies.Count);
                float dbhInInches = Constant.InchesPerCm * this.DbhInCentimeters[treeIndex];
                float heightInFeet = Constant.FeetPerMeter * this.HeightInMeters[treeIndex];
                float liveExpansionFactor = 0.6F;

                // rough crown length estimate assuming 400 TPA (10.4 x 10.4 feet) from the linear regressions of
                // Curtis RO, Reukema DL. 1970. Crown Development and Site Estimates in a Douglas-Fir Plantation Spacing Test. 
                //   Forest Science 16(3):287-301.
                // TODO: These regressions are problematic as it has a negative, rather than positive intercept at zero DBH for spacings
                // of 10x10 feet and closer. They are also specific to Wind River.
                float crownLengthInFeet = 3.20F * dbhInInches + 1.0F;
                float crownRatio = crownLengthInFeet / heightInFeet;

                treesOfSpecies.Add(this.TreeID[treeIndex], dbhInInches, heightInFeet, crownRatio, liveExpansionFactor);
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
