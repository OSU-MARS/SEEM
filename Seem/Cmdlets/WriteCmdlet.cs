using Osu.Cof.Ferm.Heuristics;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    public class WriteCmdlet : Cmdlet
    {
        protected const string RateAndAgeCsvHeader = "discountRate,thin1,thin2,thin3,rotation";

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

        protected static HeuristicParameters GetFirstHeuristicParameters(HeuristicResultSet? results)
        {
            Debug.Assert(results != null);

            Heuristic? highHeuristic = results!.SolutionIndex[results!.Distributions[0]].High;
            if (highHeuristic == null)
            {
                throw new NotSupportedException("Can't obtain heuristic parameters.");
            }
            return highHeuristic.GetParameters();
        }

        protected long GetMaxFileSizeInBytes()
        {
            return (long)(1E9F * this.LimitGB);
        }

        protected static string GetRateAndAgeCsvValues(StandTrajectory trajectory, float discountRate)
        {
            string discountRateAsString = discountRate.ToString(CultureInfo.InvariantCulture);

            int firstThinAgeAsInteger = trajectory.GetFirstThinAge();
            string? firstThinAge = firstThinAgeAsInteger != -1 ? firstThinAgeAsInteger.ToString(CultureInfo.InvariantCulture) : null;
            int secondThinAgeAsInteger = trajectory.GetSecondThinAge();
            string? secondThinAge = secondThinAgeAsInteger != -1 ? secondThinAgeAsInteger.ToString(CultureInfo.InvariantCulture) : null;
            int thirdThinAgeAsInteger = trajectory.GetThirdThinAge();
            string? thirdThinAge = thirdThinAgeAsInteger != -1 ? thirdThinAgeAsInteger.ToString(CultureInfo.InvariantCulture) : null;
            int rotationLengthAsInteger = trajectory.GetRotationLength();
            string rotationLength = rotationLengthAsInteger.ToString(CultureInfo.InvariantCulture);

            return discountRateAsString + "," + firstThinAge + "," + secondThinAge + "," + thirdThinAge + "," + rotationLength;
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

        protected static void GetBasalAreaConversion(Units inputUnits, Units outputUnits, out float basalAreaConversionFactor)
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

        protected static void GetDimensionConversions(Units inputUnits, Units outputUnits, out float areaConversionFactor, out float dbhConversionFactor, out float heightConversionFactor)
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
