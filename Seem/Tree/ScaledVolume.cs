using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Osu.Cof.Ferm.Tree
{
    public class ScaledVolume
    {
        public float PreferredLogLengthInMeters { get; private init; }
        public SortedList<FiaCode, TreeSpeciesVolumeTable> VolumeBySpecies { get; private init; }

        public ScaledVolume(TreeSpeciesVolumeTableParameters psmeParameters)
        {
            this.PreferredLogLengthInMeters = psmeParameters.PreferredLogLengthInMeters;
            this.VolumeBySpecies = new SortedList<FiaCode, TreeSpeciesVolumeTable>
            {
                { FiaCode.PseudotsugaMenziesii, new TreeSpeciesVolumeTable(psmeParameters) }
            };
        }

        public TreeSpeciesMerchantableVolumeForPeriod GetHarvestedVolume(Trees treesOfSpecies, IndividualTreeSelection individualTreeSelection, int harvestPeriod)
        {
            TreeSpeciesVolumeTable volumeTable = this.VolumeBySpecies[treesOfSpecies.Species];
            TreeSpeciesMerchantableVolumeForPeriod harvestVolume = new();
            for (int compactedTreeIndex = 0; compactedTreeIndex < treesOfSpecies.Count; ++compactedTreeIndex)
            {
                int uncompactedTreeIndex = treesOfSpecies.UncompactedIndex[compactedTreeIndex];
                if (individualTreeSelection[uncompactedTreeIndex] != harvestPeriod)
                {
                    // tree was either removed previously or was retained rather than thinned
                    continue;
                }

                float expansionFactorPerHa = treesOfSpecies.LiveExpansionFactor[compactedTreeIndex];
                if (expansionFactorPerHa <= 0.0F)
                {
                    continue; // units aren't important here
                }
                float dbhInCm = treesOfSpecies.Dbh[compactedTreeIndex];
                float heightInMeters = treesOfSpecies.Height[compactedTreeIndex];
                if (treesOfSpecies.Units == Units.English)
                {
                    dbhInCm *= Constant.CentimetersPerInch;
                    heightInMeters *= Constant.MetersPerFoot;
                    expansionFactorPerHa *= Constant.AcresPerHectare;
                }

                // compare greater than or equals to avoid overstep in bilinear interpolation
                if (dbhInCm >= volumeTable.MaximumDiameterInCentimeters)
                {
                    throw new NotSupportedException(treesOfSpecies.Species + " " + treesOfSpecies.Tag[compactedTreeIndex] + "'s diameter of " + dbhInCm.ToString("0.0") + " cm exceeds the species' volume table capacity of " + volumeTable.MaximumDiameterInCentimeters.ToString("0.0") + " cm.");
                }
                if (heightInMeters >= volumeTable.MaximumHeightInMeters)
                {
                    throw new NotSupportedException(treesOfSpecies.Species + " " + treesOfSpecies.Tag[compactedTreeIndex] + "'s height of " + heightInMeters.ToString("0.0") + " m exceeds the species' volume table capacity of " + volumeTable.MaximumHeightInMeters.ToString("0.0") + " m.");
                }

                // bilinear interpolation setup
                float dbhPosition = dbhInCm / volumeTable.DiameterClassSizeInCentimeters;
                int dbhIndex = (int)dbhPosition;
                float dbhFraction = dbhPosition - dbhIndex;

                float heightPosition = heightInMeters / volumeTable.HeightClassSizeInMeters;
                int heightIndex = (int)heightPosition;
                float heightFraction = heightPosition - heightIndex;

                // bilinear interpolation between height and diameter classes for cubic volume
                float cubic2sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Cubic2Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Cubic2Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Cubic2Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Cubic2Saw[dbhIndex + 1, heightIndex + 1]);
                harvestVolume.Cubic2Saw += expansionFactorPerHa * cubic2sawForTree;
                float cubic3sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Cubic3Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Cubic3Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Cubic3Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Cubic3Saw[dbhIndex + 1, heightIndex + 1]);
                harvestVolume.Cubic3Saw += expansionFactorPerHa * cubic3sawForTree;
                float cubic4sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Cubic4Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Cubic4Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Cubic4Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Cubic4Saw[dbhIndex + 1, heightIndex + 1]);
                harvestVolume.Cubic4Saw += expansionFactorPerHa * cubic4sawForTree;

                treesOfSpecies.MerchantableCubicVolumePerStem[compactedTreeIndex] = cubic2sawForTree + cubic3sawForTree + cubic4sawForTree;

                // bilinear interpolation for number of logs
                float logs2sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Logs2Saw[dbhIndex, heightIndex] +
                                                                heightFraction * volumeTable.Logs2Saw[dbhIndex, heightIndex + 1]) +
                                        dbhFraction * ((1.0F - heightFraction) * volumeTable.Logs2Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Logs2Saw[dbhIndex + 1, heightIndex + 1]);
                harvestVolume.Logs2Saw += expansionFactorPerHa * logs2sawForTree;
                float logs3sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Logs3Saw[dbhIndex, heightIndex] +
                                                                heightFraction * volumeTable.Logs3Saw[dbhIndex, heightIndex + 1]) +
                                        dbhFraction * ((1.0F - heightFraction) * volumeTable.Logs3Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Logs3Saw[dbhIndex + 1, heightIndex + 1]);
                harvestVolume.Logs3Saw += expansionFactorPerHa * logs3sawForTree;
                float logs4sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Logs4Saw[dbhIndex, heightIndex] +
                                                                heightFraction * volumeTable.Logs4Saw[dbhIndex, heightIndex + 1]) +
                                        dbhFraction * ((1.0F - heightFraction) * volumeTable.Logs4Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Logs4Saw[dbhIndex + 1, heightIndex + 1]);
                harvestVolume.Logs4Saw += expansionFactorPerHa * logs4sawForTree;

                // bilinear interpolation for Scribner volume
                float scribner2sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Scribner2Saw[dbhIndex, heightIndex] +
                                                                    heightFraction * volumeTable.Scribner2Saw[dbhIndex, heightIndex + 1]) +
                                            dbhFraction * ((1.0F - heightFraction) * volumeTable.Scribner2Saw[dbhIndex + 1, heightIndex] +
                                                           heightFraction * volumeTable.Scribner2Saw[dbhIndex + 1, heightIndex + 1]);
                harvestVolume.Scribner2Saw += expansionFactorPerHa * scribner2sawForTree;
                float scribner3sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Scribner3Saw[dbhIndex, heightIndex] +
                                                                    heightFraction * volumeTable.Scribner3Saw[dbhIndex, heightIndex + 1]) +
                                            dbhFraction * ((1.0F - heightFraction) * volumeTable.Scribner3Saw[dbhIndex + 1, heightIndex] +
                                                           heightFraction * volumeTable.Scribner3Saw[dbhIndex + 1, heightIndex + 1]);
                harvestVolume.Scribner3Saw += expansionFactorPerHa * scribner3sawForTree;
                float scribner4sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Scribner4Saw[dbhIndex, heightIndex] +
                                                                    heightFraction * volumeTable.Scribner4Saw[dbhIndex, heightIndex + 1]) +
                                            dbhFraction * ((1.0F - heightFraction) * volumeTable.Scribner4Saw[dbhIndex + 1, heightIndex] +
                                                           heightFraction * volumeTable.Scribner4Saw[dbhIndex + 1, heightIndex + 1]);
                harvestVolume.Scribner4Saw += expansionFactorPerHa * scribner4sawForTree;

                Debug.Assert((cubic2sawForTree >= 0.0F) && (cubic3sawForTree >= 0.0F) && (cubic4sawForTree >= 0.0F));
                Debug.Assert((scribner2sawForTree >= 0.0F) && (scribner3sawForTree >= 0.0F) && (scribner4sawForTree >= 0.0F));
            }

            harvestVolume.ConvertToMbf();
            return harvestVolume;
        }

        public TreeSpeciesMerchantableVolumeForPeriod GetStandingVolume(Trees treesOfSpecies)
        {
            TreeSpeciesVolumeTable volumeTable = this.VolumeBySpecies[treesOfSpecies.Species];
            TreeSpeciesMerchantableVolumeForPeriod standingVolume = new();
            for (int compactedTreeIndex = 0; compactedTreeIndex < treesOfSpecies.Count; ++compactedTreeIndex)
            {
                float expansionFactorPerHa = treesOfSpecies.LiveExpansionFactor[compactedTreeIndex];
                if (expansionFactorPerHa <= 0.0F)
                {
                    continue; // units aren't important here
                }
                float dbhInCm = treesOfSpecies.Dbh[compactedTreeIndex];
                float heightInMeters = treesOfSpecies.Height[compactedTreeIndex];
                if (treesOfSpecies.Units == Units.English)
                {
                    dbhInCm *= Constant.CentimetersPerInch;
                    heightInMeters *= Constant.MetersPerFoot;
                    expansionFactorPerHa *= Constant.AcresPerHectare;
                }

                // compare greater than or equals to avoid overstep in bilinear interpolation
                if (dbhInCm >= volumeTable.MaximumDiameterInCentimeters)
                {
                    throw new NotSupportedException(treesOfSpecies.Species + " " + treesOfSpecies.Tag[compactedTreeIndex] + "'s diameter of " + dbhInCm.ToString("0.00") + " cm exceeds the species' volume table capacity of " + volumeTable.MaximumDiameterInCentimeters.ToString("0.00") + " cm.");
                }
                if (heightInMeters >= volumeTable.MaximumHeightInMeters)
                {
                    throw new NotSupportedException(treesOfSpecies.Species + " " + treesOfSpecies.Tag[compactedTreeIndex] + "'s height of " + heightInMeters.ToString("0.00") + " m exceeds the species' volume table capacity of " + volumeTable.MaximumHeightInMeters.ToString("0.00") + " m.");
                }

                // bilinear interpolation setup
                float dbhPosition = dbhInCm / volumeTable.DiameterClassSizeInCentimeters;
                int dbhIndex = (int)dbhPosition;
                float dbhFraction = dbhPosition - dbhIndex;

                float heightPosition = heightInMeters / volumeTable.HeightClassSizeInMeters;
                int heightIndex = (int)heightPosition;
                float heightFraction = heightPosition - heightIndex;

                // bilinear interpolation for cubic volume
                float cubic2sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Cubic2Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Cubic2Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Cubic2Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Cubic2Saw[dbhIndex + 1, heightIndex + 1]);
                standingVolume.Cubic2Saw += expansionFactorPerHa * cubic2sawForTree;
                float cubic3sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Cubic3Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Cubic3Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Cubic3Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Cubic3Saw[dbhIndex + 1, heightIndex + 1]);
                standingVolume.Cubic3Saw += expansionFactorPerHa * cubic3sawForTree;
                float cubic4sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Cubic4Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Cubic4Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Cubic4Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Cubic4Saw[dbhIndex + 1, heightIndex + 1]);
                standingVolume.Cubic4Saw += expansionFactorPerHa * cubic4sawForTree;

                treesOfSpecies.MerchantableCubicVolumePerStem[compactedTreeIndex] = cubic2sawForTree + cubic3sawForTree + cubic4sawForTree;

                // bilinear interpolation for number of logs
                float logs2sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Logs2Saw[dbhIndex, heightIndex] +
                                                                heightFraction * volumeTable.Logs2Saw[dbhIndex, heightIndex + 1]) +
                                        dbhFraction * ((1.0F - heightFraction) * volumeTable.Logs2Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Logs2Saw[dbhIndex + 1, heightIndex + 1]);
                standingVolume.Logs2Saw += expansionFactorPerHa * logs2sawForTree;
                float logs3sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Logs3Saw[dbhIndex, heightIndex] +
                                                                heightFraction * volumeTable.Logs3Saw[dbhIndex, heightIndex + 1]) +
                                        dbhFraction * ((1.0F - heightFraction) * volumeTable.Logs3Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Logs3Saw[dbhIndex + 1, heightIndex + 1]);
                standingVolume.Logs3Saw += expansionFactorPerHa * logs3sawForTree;
                float logs4sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Logs4Saw[dbhIndex, heightIndex] +
                                                                heightFraction * volumeTable.Logs4Saw[dbhIndex, heightIndex + 1]) +
                                        dbhFraction * ((1.0F - heightFraction) * volumeTable.Logs4Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Logs4Saw[dbhIndex + 1, heightIndex + 1]);
                standingVolume.Logs4Saw += expansionFactorPerHa * logs4sawForTree;

                // bilinear interpolation for Scribner
                float scribner2sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Scribner2Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Scribner2Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Scribner2Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Scribner2Saw[dbhIndex + 1, heightIndex + 1]);
                standingVolume.Scribner2Saw += expansionFactorPerHa * scribner2sawForTree;
                float scribner3sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Scribner3Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Scribner3Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Scribner3Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Scribner3Saw[dbhIndex + 1, heightIndex + 1]);
                standingVolume.Scribner3Saw += expansionFactorPerHa * scribner3sawForTree;
                float scribner4sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Scribner4Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Scribner4Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Scribner4Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Scribner4Saw[dbhIndex + 1, heightIndex + 1]);
                standingVolume.Scribner4Saw += expansionFactorPerHa * scribner4sawForTree;

                Debug.Assert((cubic2sawForTree >= 0.0F) && (cubic3sawForTree >= 0.0F) && (cubic4sawForTree >= 0.0F));
                Debug.Assert((scribner2sawForTree >= 0.0F) && (scribner3sawForTree >= 0.0F) && (scribner4sawForTree >= 0.0F));
            }

            standingVolume.ConvertToMbf();
            return standingVolume;
        }
    }
}