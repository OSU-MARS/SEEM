using Mars.Seem.Heuristics;
using Mars.Seem.Organon;
using Mars.Seem.Silviculture;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Mars.Seem.Cmdlets
{
    public class WriteCmdlet : Cmdlet
    {
        private bool openedExistingFile;

        [Parameter]
        public SwitchParameter Append;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string? CsvFile;

        [Parameter(HelpMessage = "Maximum size of output file in gigabytes.")]
        [ValidateRange(0.1F, 100.0F)]
        public float LimitGB { get; set; }

        public WriteCmdlet()
        {
            this.Append = false;
            this.LimitGB = 1.0F; // sanity default, may be set higher in derived classes
            this.openedExistingFile = false;
        }

        protected static void GetMetricConversions(Units inputUnits, out float areaConversionFactor, out float dbhConversionFactor, out float heightConversionFactor)
        {
            switch (inputUnits)
            {
                case Units.English:
                    areaConversionFactor = Constant.AcresPerHectare;
                    dbhConversionFactor = Constant.CentimetersPerInch;
                    heightConversionFactor = Constant.MetersPerFoot;
                    break;
                case Units.Metric:
                    areaConversionFactor = 1.0F;
                    dbhConversionFactor = 1.0F;
                    heightConversionFactor = 1.0F;
                    break;
                default:
                    throw new NotSupportedException("Unhandled units " + inputUnits + ".");
            }
        }

        protected long GetMaxFileSizeInBytes()
        {
            return (long)(1E9F * this.LimitGB);
        }

        protected StreamWriter GetWriter()
        {
            FileMode fileMode = this.Append ? FileMode.Append : FileMode.Create;
            if (fileMode == FileMode.Append)
            {
                this.openedExistingFile = File.Exists(this.CsvFile);
            }
            return new StreamWriter(new FileStream(this.CsvFile!, fileMode, FileAccess.Write, FileShare.Read));
        }

        protected bool ShouldWriteHeader()
        {
            return this.openedExistingFile == false;
        }
    }
}
