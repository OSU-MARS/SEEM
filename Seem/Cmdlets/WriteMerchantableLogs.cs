using Osu.Cof.Ferm.Extensions;
using Osu.Cof.Ferm.Heuristics;
using Osu.Cof.Ferm.Organon;
using Osu.Cof.Ferm.Tree;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Osu.Cof.Ferm.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "MerchantableLogs")]
    public class WriteMerchantableLogs : WriteStandTrajectory
    {
        protected override void ProcessRecord()
        {
            this.ValidateParameters();

            using StreamWriter writer = this.GetWriter();

            // header
            bool resultsSpecified = this.Results != null;
            if (this.ShouldWriteHeader())
            {
                HeuristicParameters? heuristicParameters = null;
                if (resultsSpecified)
                {
                    heuristicParameters = WriteCmdlet.GetFirstHeuristicParameters(this.Results);
                }
                else if (this.Trajectories![0].Heuristic != null)
                {
                    heuristicParameters = this.Trajectories[0].Heuristic!.GetParameters();
                }

                writer.WriteLine(WriteCmdlet.GetHeuristicAndPositionCsvHeader(heuristicParameters) + ",standAge,species,plot,tag,DBH,height,EF,log,gradeThin,topDibThin,cubicThin,gradeRegen,topDibRegen,cubicRegen");
            }

            // rows for periods
            long maxFileSizeInBytes = this.GetMaxFileSizeInBytes();
            int maxPositionIndex = this.GetMaxPositionIndex();
            for (int positionIndex = 0; positionIndex < maxPositionIndex; ++positionIndex)
            {
                OrganonStandTrajectory highTrajectory = this.GetHighestTrajectoryAndLinePrefix(positionIndex, out StringBuilder linePrefix, out int _, out int _);

                for (int periodIndex = 0; periodIndex < highTrajectory.PlanningPeriods; ++periodIndex)
                {
                    OrganonStand stand = highTrajectory.StandByPeriod[periodIndex] ?? throw new InvalidOperationException("Stand has not been simulated for period " + periodIndex + ".");
                    string standAge = stand.AgeInYears.ToString(CultureInfo.InvariantCulture);

                    foreach (Trees treesOfSpecies in stand.TreesBySpecies.Values)
                    {
                        string linePrefixForPeriodAndSpecies = linePrefix + "," + standAge + "," + treesOfSpecies.Species.ToFourLetterCode();

                        TreeSpeciesVolumeTable regenVolume = highTrajectory.TreeVolume.RegenerationHarvest.VolumeBySpecies[treesOfSpecies.Species];
                        TreeSpeciesVolumeTable thinningVolume = highTrajectory.TreeVolume.Thinning.VolumeBySpecies[treesOfSpecies.Species];
                        if (thinningVolume.MaximumLogs < regenVolume.MaximumLogs)
                        {
                            throw new NotSupportedException();
                        }

                        for (int compactedTreeIndex = 0; compactedTreeIndex < treesOfSpecies.Count; ++compactedTreeIndex)
                        {
                            float dbhInCm = treesOfSpecies.Dbh[compactedTreeIndex];
                            float heightInM = treesOfSpecies.Height[compactedTreeIndex];
                            float expansionFactorPerHa = treesOfSpecies.LiveExpansionFactor[compactedTreeIndex];
                            if (treesOfSpecies.Units == Units.English)
                            {
                                dbhInCm *= Constant.CentimetersPerInch;
                                heightInM *= Constant.MetersPerFoot;
                                expansionFactorPerHa *= Constant.AcresPerHectare;
                            }

                            string linePrefixForTree = linePrefixForPeriodAndSpecies + "," +
                                                       treesOfSpecies.Plot[compactedTreeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                                       treesOfSpecies.Tag[compactedTreeIndex].ToString(CultureInfo.InvariantCulture) + "," +
                                                       dbhInCm.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                                       heightInM.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                                       expansionFactorPerHa.ToString(CultureInfo.InvariantCulture);

                            int regenDiameterClass = regenVolume.ToDiameterIndex(dbhInCm);
                            int regenHeightClass = regenVolume.ToHeightIndex(heightInM);
                            int regenLogs2S = regenVolume.Logs2Saw[regenDiameterClass, regenHeightClass];
                            int regenLogs3S = regenVolume.Logs3Saw[regenDiameterClass, regenHeightClass];

                            int thinDiameterClass = thinningVolume.ToDiameterIndex(dbhInCm);
                            int thinHeightClass = thinningVolume.ToHeightIndex(heightInM);
                            int thinLogs2S = thinningVolume.Logs2Saw[thinDiameterClass, thinHeightClass];
                            int thinLogs3S = thinningVolume.Logs3Saw[thinDiameterClass, thinHeightClass];

                            for (int logIndex = 0; logIndex < thinningVolume.MaximumLogs; ++logIndex)
                            {
                                float thinLogVolume = thinningVolume.LogCubic[thinDiameterClass, thinHeightClass, logIndex];
                                if (thinLogVolume <= 0.0F)
                                {
                                    continue;
                                }

                                string thinGrade = "4S";
                                if (logIndex < thinLogs2S + thinLogs3S)
                                {
                                    thinGrade = "3S";
                                    if (logIndex < thinLogs2S)
                                    {
                                        thinGrade = "2S";
                                    }
                                }
                                float thinTopDiameter = thinningVolume.LogTopDiameter[regenDiameterClass, regenHeightClass, logIndex];

                                string regenGrade = String.Empty;
                                string regenLogVolume = String.Empty;
                                string regenTopDiameter = String.Empty;
                                if (regenVolume.MaximumLogs > logIndex)
                                {
                                    float logVolume = regenVolume.LogCubic[regenDiameterClass, regenHeightClass, logIndex];
                                    if (logVolume > 0.0F)
                                    {
                                        regenGrade = "4S";
                                        if (logIndex < regenLogs2S + regenLogs3S)
                                        {
                                            regenGrade = "3S";
                                            if (logIndex < regenLogs2S)
                                            {
                                                regenGrade = "2S";
                                            }
                                        }

                                        regenLogVolume = logVolume.ToString("0.000", CultureInfo.InvariantCulture);

                                        float topDiameter = regenVolume.LogTopDiameter[regenDiameterClass, regenHeightClass, logIndex];
                                        Debug.Assert(topDiameter > 0.0F);
                                        regenTopDiameter = topDiameter.ToString("0.0", CultureInfo.InvariantCulture);
                                    }
                                }

                                writer.WriteLine(linePrefixForTree + "," +
                                                 logIndex.ToString(CultureInfo.InvariantCulture) + "," +
                                                 thinGrade + "," +
                                                 thinTopDiameter.ToString("0.0", CultureInfo.InvariantCulture) + "," +
                                                 thinLogVolume.ToString("0.000", CultureInfo.InvariantCulture) + "," +
                                                 regenGrade + "," +
                                                 regenTopDiameter + "," +
                                                 regenLogVolume);
                            }
                        }
                    }
                }

                if (writer.BaseStream.Length > maxFileSizeInBytes)
                {
                    this.WriteWarning("Write-SnagsAndLogs: File size limit of " + this.LimitGB.ToString("0.00") + " GB exceeded.");
                    break;
                }
            }
        }
    }
}
