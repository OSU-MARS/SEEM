using System;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Mars.Seem.Cmdlets
{
    public class WriteCmdlet : Cmdlet
    {
        // checking FileStream.Length becomes performance limiting when relatively low numbers of bytes are written per coordinate
        // This is most likely to occur when -HarvestsOnly and many -No switches are set to reduce the size of the output file. As a
        // mitigation, file size is estimated from the number of characters written and Length is called only periodicaly. It's assumed
        // all characters written are in the base UTF8 character set and therefore contribute one byte each to the file size. If this is
        // incorrect it's mostly likely incorrect only for the stand name and the resulting underprediction is assumed to be small enough
        // to be acceptable.
        protected const int StreamLengthSynchronizationInterval = 10 * 1000 * 1000; // 10 MB, as of .NET 5.0 checking every 1.0 MB is undesirably expensive

        private bool openedExistingFile;

        [Parameter]
        public SwitchParameter Append;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string? CsvFile;

        [Parameter(HelpMessage = "Approximate upper bound of output file size in gigabytes.  This limit is loosely enforced and maximum file sizes will typically be somewhat larger.")]
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
            FileStream stream = new(this.CsvFile!, fileMode, FileAccess.Write, FileShare.Read, Constant.Default.FileWriteBufferSizeInBytes, FileOptions.SequentialScan);
            return new StreamWriter(stream, Encoding.UTF8); // callers assume UTF8, see remarks for StreamLengthSynchronizationInterval
        }

        protected bool ShouldWriteHeader()
        {
            return this.openedExistingFile == false;
        }
    }
}
