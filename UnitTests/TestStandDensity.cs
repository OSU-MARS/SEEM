using Osu.Cof.Ferm.Organon;
using System.IO;

namespace Osu.Cof.Ferm.Test
{
    internal class TestStandDensity : OrganonStandDensity
    {
        public TestStandDensity(OrganonStand stand, OrganonVariant variant)
            : base(stand, variant)
        {
        }

        public StreamWriter WriteToCsv(string filePath, OrganonVariant variant, int year)
        {
            FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine("variant,year,TPH,basal area,CCF");
            this.WriteToCsv(writer, variant, year);
            return writer;
        }

        public void WriteToCsv(StreamWriter writer, OrganonVariant variant, int year)
        {
            writer.WriteLine("{0},{1},{2},{3},{4}",
                             variant.TreeModel, year, 2.47105F * this.TreesPerAcre, 2.47105F * 0.092903F * this.BasalAreaPerAcre,
                             this.CrownCompetitionFactor);
        }
    }
}