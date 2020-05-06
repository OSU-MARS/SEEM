using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "StandTrajectory")]
    public class WriteStandTrajectory : Cmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string CsvFile;

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public List<OrganonStandTrajectory> Trajectories { get; set; }

        protected override void ProcessRecord()
        {
            // TODO: support as input parameter
            TimberValue timberValue = new TimberValue();

            using FileStream stream = new FileStream(this.CsvFile, FileMode.Create, FileAccess.Write, FileShare.Read);
            using StreamWriter writer = new StreamWriter(stream);

            // header
            // TODO: check for mixed units and support TBH
            // TODO: snags per acre or hectare, live and dead QMD?
            StringBuilder line = new StringBuilder("stand,stand age,sim year,TPA,BA,standing,harvested,BA removed,NPV");
            writer.WriteLine(line);

            // rows for periods
            for (int trajectoryIndex = 0; trajectoryIndex < this.Trajectories.Count; ++trajectoryIndex)
            {
                OrganonStandTrajectory trajectory = this.Trajectories[trajectoryIndex];
                if (trajectory.VolumeUnits != VolumeUnits.ScribnerBoardFeetPerAcre)
                {
                    throw new NotSupportedException();
                }

                string trajectoryName = trajectory.Name;
                if (trajectoryName == null)
                {
                    trajectoryName = trajectoryIndex.ToString(CultureInfo.InvariantCulture);
                }
                float volumeUnitMultiplier = 1.0F;
                if (trajectory.VolumeUnits == VolumeUnits.ScribnerBoardFeetPerAcre)
                {
                    volumeUnitMultiplier = 0.001F;
                }

                int initialStandAge = trajectory.StandByPeriod[0].AgeInYears;
                for (int periodIndex = 0; periodIndex < trajectory.PlanningPeriods; ++periodIndex)
                {
                    line.Clear();

                    // get density and volumes
                    OrganonStandDensity density = trajectory.DensityByPeriod[periodIndex];
                    float standingVolume = volumeUnitMultiplier * trajectory.StandingVolumeByPeriod[periodIndex];
                    float harvestMbfPerAcre = 0.0F;
                    float basalAreaRemoved = 0.0F;
                    if (trajectory.HarvestVolumesByPeriod.Length > periodIndex)
                    {
                        harvestMbfPerAcre = volumeUnitMultiplier * trajectory.HarvestVolumesByPeriod[periodIndex];
                        basalAreaRemoved = trajectory.BasalAreaRemoved[periodIndex];
                    }

                    // NPV
                    float netPresentValue = 0.0F;
                    int periodsFromPresent = Math.Max(periodIndex - 1, 0);
                    if (harvestMbfPerAcre > 0.0F)
                    {
                        netPresentValue = timberValue.GetPresentValueOfThinScribner(trajectory.HarvestVolumesByPeriod[periodIndex], periodsFromPresent, trajectory.PeriodLengthInYears);
                    }
                    else
                    {
                        netPresentValue = timberValue.GetPresentValueOfFinalHarvestScribner(trajectory.StandingVolumeByPeriod[periodIndex], periodsFromPresent, trajectory.PeriodLengthInYears);
                    }

                    int simulationYear = trajectory.PeriodLengthInYears * periodIndex;
                    line.Append(trajectoryName + "," + 
                                (initialStandAge + simulationYear) + "," +
                                simulationYear + "," +
                                density.TreesPerAcre.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                density.BasalAreaPerAcre.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                standingVolume.ToString("0.000", CultureInfo.InvariantCulture) + "," + 
                                harvestMbfPerAcre.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                basalAreaRemoved.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                netPresentValue.ToString("0", CultureInfo.InvariantCulture));
                    writer.WriteLine(line);
                }
            }
        }
    }
}
