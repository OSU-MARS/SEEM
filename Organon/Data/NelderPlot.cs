using System;
using System.Collections.Generic;

namespace Osu.Cof.Organon.Data
{
    public class NelderPlot
    {
        public List<float> DbhInCentimeters { get; private set; }
        public List<float> HeightInMeters { get; private set; }
        public List<FiaCode> Species { get; private set; }
        public List<int> TreeID { get; private set; }

        public NelderPlot(string xlsxFilePath, string worksheetName)
        {
            this.DbhInCentimeters = new List<float>();
            this.HeightInMeters = new List<float>();
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
            FiaCode species = (rowAsStrings[Constant.Nelder.ColumnIndex.Species]) switch
            {
                "PSME" => FiaCode.PseudotsugaMenziesii,
                _ => throw new NotSupportedException(String.Format("Unhandled species '{0}'.", rowAsStrings[Constant.Nelder.ColumnIndex.Species])),
            };
            this.Species.Add(species);
            this.TreeID.Add(Int32.Parse(rowAsStrings[Constant.Nelder.ColumnIndex.Tree]));
        }

        public Stand ToStand(float siteIndex)
        {
            return this.ToStand(siteIndex, this.TreeID.Count);
        }

        public Stand ToStand(float siteIndex, int treesInStand)
        {
            if (treesInStand > this.TreeID.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(treesInStand));
            }

            Stand stand = new Stand(20, treesInStand, siteIndex);
            for (int treeIndex = 0; treeIndex < treesInStand; ++treeIndex)
            {
                stand.CrownRatio[treeIndex] = 0.75F;
                stand.Dbh[treeIndex] = Constant.InchesPerCm * this.DbhInCentimeters[treeIndex];
                stand.Height[treeIndex] = Constant.FeetPerMeter * this.HeightInMeters[treeIndex];
                stand.LiveExpansionFactor[treeIndex] = 0.6F;
                stand.Species[treeIndex] = this.Species[treeIndex];
                stand.Tag[treeIndex] = this.TreeID[treeIndex];
            }
            return stand;
        }
    }
}
