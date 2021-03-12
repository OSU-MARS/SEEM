using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "SnagsAndLogs")]
    public class WriteSnagsAndLogs : WriteStandTrajectory
    {
        protected override void ProcessRecord()
        {
            this.ValidateParameters();

            using StreamWriter writer = this.GetWriter();

            // header
            bool runsSpecified = this.Runs != null;
            StringBuilder line = new();
            if (this.ShouldWriteHeader())
            {
                line.Append("stand,heuristic");

                HeuristicParameters? heuristicParametersForHeader = null;
                if (runsSpecified)
                {
                    heuristicParametersForHeader = this.Runs![0].HighestHeuristicParameters;
                }
                else if (this.Trajectories![0].Heuristic != null)
                {
                    heuristicParametersForHeader = this.Trajectories[0].Heuristic!.GetParameters();
                }

                if (heuristicParametersForHeader != null)
                {
                    string heuristicParameters = heuristicParametersForHeader.GetCsvHeader();
                    if (String.IsNullOrEmpty(heuristicParameters) == false)
                    {
                        // TODO: if needed, check if heuristics have different parameters
                        line.Append("," + heuristicParameters);
                    }
                }

                line.Append(",discount rate,first thin,second thin,rotation,stand age,species,diameter class,snags,logs");
                writer.WriteLine(line);
            }

            // rows for periods
            int maxIndex = runsSpecified ? this.Runs!.Count : this.Trajectories!.Count;
            for (int runOrTrajectoryIndex = 0; runOrTrajectoryIndex < maxIndex; ++runOrTrajectoryIndex)
            {
                OrganonStandTrajectory highestTrajectory = this.GetHighestTrajectoryAndLinePrefix(runOrTrajectoryIndex, out StringBuilder linePrefix);

                SnagLogTable snagsAndLogs = new(highestTrajectory, this.MaximumDiameter, this.DiameterClassSize);
                for (int periodIndex = 0; periodIndex < highestTrajectory.PlanningPeriods; ++periodIndex)
                {
                    OrganonStand? stand = highestTrajectory.StandByPeriod[periodIndex];
                    Debug.Assert(stand != null);
                    string standAge = stand.AgeInYears.ToString(CultureInfo.InvariantCulture);

                    foreach (KeyValuePair<FiaCode, float[,]> speciesAndSnags in snagsAndLogs.SnagsPerHectareBySpeciesAndDiameterClass)
                    {
                        FiaCode species = speciesAndSnags.Key;
                        float[,] logs = snagsAndLogs.LogsPerHectareBySpeciesAndDiameterClass[species];
                        float[,] snags = speciesAndSnags.Value;
                        string standAgeAndSpeciesCode = standAge + "," + species.ToFourLetterCode();
                        for (int diameterClassIndex = 0; diameterClassIndex < snagsAndLogs.DiameterClasses; ++diameterClassIndex)
                        {
                            string diameter = snagsAndLogs.GetDiameter(diameterClassIndex).ToString("0.0", CultureInfo.InvariantCulture);
                            string snagsPerHectare = snags[periodIndex, diameterClassIndex].ToString("0.00", CultureInfo.InvariantCulture);
                            string logsPerHectare = logs[periodIndex, diameterClassIndex].ToString("0.00", CultureInfo.InvariantCulture);

                            line.Clear();
                            line.Append(linePrefix + "," +
                                        standAgeAndSpeciesCode + "," +
                                        diameter + "," +
                                        snagsPerHectare + "," +
                                        logsPerHectare);
                            writer.WriteLine(line);
                        }
                    }
                }
            }
        }
    }
}
