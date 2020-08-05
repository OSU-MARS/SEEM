using System;
using System.IO;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    public class WriteCmdlet : Cmdlet
    {
        private bool openedExistingFile;

        [Parameter]
        public SwitchParameter Append;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string CsvFile;

        public WriteCmdlet()
        {
            this.Append = false;
            this.openedExistingFile = false;
        }

        protected StreamWriter GetWriter()
        {
            FileMode fileMode = this.Append ? FileMode.Append : FileMode.Create;
            if (fileMode == FileMode.Append)
            {
                this.openedExistingFile = File.Exists(this.CsvFile);
            }
            return new StreamWriter(new FileStream(this.CsvFile, fileMode, FileAccess.Write, FileShare.Read));
        }

        protected void GetBasalAreaConversion(Units inputUnits, Units outputUnits, out float basalAreaConversionFactor)
        {
            if (inputUnits == outputUnits)
            {
                basalAreaConversionFactor = 1.0F;
                return;
            }

            // English to metric
            if ((inputUnits == Units.English) && (outputUnits == Units.Metric))
            {
                basalAreaConversionFactor = Constant.AcresPerHectare * Constant.MetersPerFoot * Constant.MetersPerFoot;
                return;
            }

            throw new NotSupportedException();
        }

        protected void GetDimensionConversions(Units inputUnits, Units outputUnits, out float areaConversionFactor, out float dbhConversionFactor, out float heightConversionFactor)
        {
            if (inputUnits == outputUnits)
            {
                areaConversionFactor = 1.0F;
                dbhConversionFactor = 1.0F;
                heightConversionFactor = 1.0F;
                return;
            }

            // English to metric
            if ((inputUnits == Units.English) && (outputUnits == Units.Metric))
            {
                areaConversionFactor = Constant.AcresPerHectare;
                dbhConversionFactor = Constant.CentimetersPerInch;
                heightConversionFactor = Constant.MetersPerFoot;
                return;
            }

            throw new NotSupportedException();
        }

        protected bool ShouldWriteHeader()
        {
            return this.openedExistingFile == false;
        }
    }
}
