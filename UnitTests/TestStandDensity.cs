﻿using Mars.Seem.Organon;
using System.IO;

namespace Mars.Seem.Test
{
    internal class TestStandDensity(OrganonStand stand, OrganonVariant variant) : OrganonStandDensity(variant, stand)
    {
        public StreamWriter WriteToCsv(string filePath, OrganonVariant variant, int year)
        {
            FileStream stream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            StreamWriter writer = new(stream);
            writer.WriteLine("variant,year,TPH,basal area,CCF");
            this.WriteToCsv(writer, variant, year);
            return writer;
        }

        public void WriteToCsv(StreamWriter writer, OrganonVariant variant, int year)
        {
            writer.WriteLine("{0},{1},{2},{3},{4}",
                             variant.TreeModel, year, this.TreesPerHa, this.BasalAreaPerHa,
                             this.CrownCompetitionFactor);
        }
    }
}