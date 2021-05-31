using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation;

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
                writer.WriteLine("logLength,species,height,DBH,cubic2S,cubic3S,cubic4S,scribner2S,scribner3S,scribner4S");
            }

            this.WriteScaledVolume(writer, this.TimberValue.ScaledVolumeRegenerationHarvest);
            this.WriteScaledVolume(writer, this.TimberValue.ScaledVolumeThinning);
        }

        private void WriteScaledVolume(StreamWriter writer, ScaledVolume scaledVolume)
        {
            string logLengthAsString = scaledVolume.PreferredLogLengthInMeters.ToString(CultureInfo.InvariantCulture);
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            foreach (KeyValuePair<FiaCode, TreeVolumeTable> species in scaledVolume.VolumeBySpecies)
            {
                string speciesPrefix = String.Concat(logLengthAsString, ",", FiaCodeExtensions.ToFourLetterCode(species.Key));
                TreeVolumeTable volumeTable = species.Value;
                for (int heightIndex = 0; heightIndex < volumeTable.HeightClasses; ++heightIndex)
                {
                    string height = volumeTable.GetHeight(heightIndex).ToString(CultureInfo.InvariantCulture);
                    for (int dbhIndex = 0; dbhIndex < volumeTable.DiameterClasses; ++dbhIndex)
                    {
                        string dbh = volumeTable.GetDiameter(dbhIndex).ToString(CultureInfo.InvariantCulture);
                        string cubic2saw = volumeTable.Cubic2Saw[dbhIndex, heightIndex].ToString(CultureInfo.InvariantCulture);
                        string cubic3saw = volumeTable.Cubic3Saw[dbhIndex, heightIndex].ToString(CultureInfo.InvariantCulture);
                        string cubic4saw = volumeTable.Cubic4Saw[dbhIndex, heightIndex].ToString(CultureInfo.InvariantCulture);
                        string scribner2saw = volumeTable.Scribner2Saw[dbhIndex, heightIndex].ToString(CultureInfo.InvariantCulture);
                        string scribner3saw = volumeTable.Scribner3Saw[dbhIndex, heightIndex].ToString(CultureInfo.InvariantCulture);
                        string scribner4saw = volumeTable.Scribner4Saw[dbhIndex, heightIndex].ToString(CultureInfo.InvariantCulture);

                        writer.WriteLine(speciesPrefix + "," + 
                                         height + "," + 
                                         dbh + "," + 
                                         cubic2saw + "," + 
                                         cubic3saw + "," + 
                                         cubic4saw + "," + 
                                         scribner2saw + "," + 
                                         scribner3saw + "," + 
                                         scribner4saw);
                    }
                }

                if (writer.BaseStream.Length > maxFileSizeInBytes)
                {
                    this.WriteWarning("Write-VolumeTable: File size limit of " + this.LimitGB.ToString("0.00") + " GB exceeded.");
                    break;
                }
            }
        }
    }
}