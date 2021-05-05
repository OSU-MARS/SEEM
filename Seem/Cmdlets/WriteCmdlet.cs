using System;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    public class WriteCmdlet : Cmdlet
    {
        protected const string RateAndAgeCsvHeader = "discount rate,first thin,second thin,third thin,rotation";

        private bool openedExistingFile;

        [Parameter]
        public SwitchParameter Append;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string? CsvFile;

        public WriteCmdlet()
        {
            this.Append = false;
            this.openedExistingFile = false;
        }

        protected static string GetRateAndAgeCsvValues(StandTrajectory trajectory)
        {
            float discountRateAsFloat = trajectory.TimberValue.DiscountRate;
            string discountRate = discountRateAsFloat.ToString(CultureInfo.InvariantCulture);

            int firstThinAgeAsInteger = trajectory.GetFirstThinAge();
            string? firstThinAge = firstThinAgeAsInteger != -1 ? firstThinAgeAsInteger.ToString(CultureInfo.InvariantCulture) : null;
            int secondThinAgeAsInteger = trajectory.GetSecondThinAge();
            string? secondThinAge = secondThinAgeAsInteger != -1 ? secondThinAgeAsInteger.ToString(CultureInfo.InvariantCulture) : null;
            int thirdThinAgeAsInteger = trajectory.GetThirdThinAge();
            string? thirdThinAge = thirdThinAgeAsInteger != -1 ? thirdThinAgeAsInteger.ToString(CultureInfo.InvariantCulture) : null;
            int rotationLengthAsInteger = trajectory.GetRotationLength();
            string rotationLength = rotationLengthAsInteger.ToString(CultureInfo.InvariantCulture);

            return discountRate + "," + firstThinAge + "," + secondThinAge + "," + thirdThinAge + "," + rotationLength;
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
