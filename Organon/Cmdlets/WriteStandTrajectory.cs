using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
            StringBuilder line = new StringBuilder("stand,year,TPA,BA,standing,harvested,NPV");
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

                for (int periodIndex = 0; periodIndex < trajectory.PlanningPeriods; ++periodIndex)
                {
                    line.Clear();

                    // get density and volumes
                    OrganonStandDensity density = trajectory.DensityByPeriod[periodIndex];
                    float standingVolume = volumeUnitMultiplier * trajectory.StandingVolumeByPeriod[periodIndex];
                    float harvestVolume = 0.0F;
                    if (trajectory.HarvestVolumesByPeriod.Length > periodIndex)
                    {
                        harvestVolume = volumeUnitMultiplier * trajectory.HarvestVolumesByPeriod[periodIndex];
                    }

                    // NPV
                    float netPresentValue = 0.0F;
                    if (periodIndex > 0)
                    {
                        float thinBoardFeetPerAcre = trajectory.HarvestVolumesByPeriod[periodIndex];
                        if (thinBoardFeetPerAcre > 0.0F)
                        {
                            netPresentValue = timberValue.GetPresentValueOfThinScribner(thinBoardFeetPerAcre, periodIndex - 1, trajectory.PeriodLengthInYears);
                        }
                        else
                        {
                            netPresentValue = timberValue.GetPresentValueOfFinalHarvestScribner(trajectory.StandingVolumeByPeriod[periodIndex], periodIndex - 1, trajectory.PeriodLengthInYears);
                        }
                    }

                    line.Append(trajectoryName + "," + 
                                trajectory.PeriodLengthInYears * periodIndex + "," +
                                density.TreesPerAcre.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                density.BasalAreaPerAcre.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                standingVolume.ToString("0.000", CultureInfo.InvariantCulture) + "," + 
                                harvestVolume.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                netPresentValue.ToString("0", CultureInfo.InvariantCulture));
                    writer.WriteLine(line);
                }
            }
        }
    }
}
