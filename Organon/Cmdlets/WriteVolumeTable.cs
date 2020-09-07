using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "VolumeTable")]
    public class WriteVolumeTable : WriteCmdlet
    {
        [Parameter]
        [ValidateNotNull]
        public TimberValue TimberValue { get; set; }

        public WriteVolumeTable()
        {
            this.TimberValue = TimberValue.Default;
        }

        protected override void ProcessRecord()
        {
            using StreamWriter writer = this.GetWriter();

            // header
            if (this.ShouldWriteHeader())
            {
                StringBuilder line = new StringBuilder();
                line.Append("log length,species,height,DBH,cubic 2S,cubic 3S,cubic 4S,scribner 2S,scribner 3S,scribner 4S");
                writer.WriteLine(line);
            }

            this.WriteScaledVolume(writer, this.TimberValue.ScaledVolumeRegenerationHarvest);
            this.WriteScaledVolume(writer, this.TimberValue.ScaledVolumeThinning);
        }

        private void WriteScaledVolume(StreamWriter writer, ScaledVolume scaledVolume)
        {
            StringBuilder line = new StringBuilder();

            string logLengthAsString = scaledVolume.PreferredLogLengthInMeters.ToString(CultureInfo.InvariantCulture);
            foreach (KeyValuePair<FiaCode, ScaledVolume.TreeVolumeTable> species in scaledVolume.VolumeBySpecies)
            {
                string speciesPrefix = String.Concat(logLengthAsString, ",", FiaCodeExtensions.ToFourLetterCode(species.Key));
                ScaledVolume.TreeVolumeTable perTreeValue = species.Value;
                for (int heightIndex = 0; heightIndex < perTreeValue.HeightClasses; ++heightIndex)
                {
                    string height = perTreeValue.GetHeight(heightIndex).ToString(CultureInfo.InvariantCulture);
                    for (int dbhIndex = 0; dbhIndex < perTreeValue.DiameterClasses; ++dbhIndex)
                    {
                        string dbh = perTreeValue.GetDiameter(dbhIndex).ToString(CultureInfo.InvariantCulture);
                        string cubic2saw = perTreeValue.Cubic2Saw[dbhIndex, heightIndex].ToString(CultureInfo.InvariantCulture);
                        string cubic3saw = perTreeValue.Cubic3Saw[dbhIndex, heightIndex].ToString(CultureInfo.InvariantCulture);
                        string cubic4saw = perTreeValue.Cubic4Saw[dbhIndex, heightIndex].ToString(CultureInfo.InvariantCulture);
                        string scribner2saw = perTreeValue.Scribner2Saw[dbhIndex, heightIndex].ToString(CultureInfo.InvariantCulture);
                        string scribner3saw = perTreeValue.Scribner3Saw[dbhIndex, heightIndex].ToString(CultureInfo.InvariantCulture);
                        string scribner4saw = perTreeValue.Scribner4Saw[dbhIndex, heightIndex].ToString(CultureInfo.InvariantCulture);

                        line.Clear();
                        line.Append(speciesPrefix + "," + height + "," + dbh + "," + cubic2saw + "," + cubic3saw + "," + cubic4saw + "," + scribner2saw + "," + scribner3saw + "," + scribner4saw);
                        writer.WriteLine(line);
                    }
                }
            }
        }
    }
}