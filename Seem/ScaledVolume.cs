﻿using System;
using System.Collections.Generic;

namespace Osu.Cof.Ferm
{
    public class ScaledVolume
    {
        public float PreferredLogLengthInMeters { get; private init; }
        public Dictionary<FiaCode, TreeVolumeTable> VolumeBySpecies { get; private init; }

        public ScaledVolume(float maximumDiameterInCentimeters, float maximumHeightInMeters, float preferredLogLengthInMeters, bool scribnerFromLumberRecovery)
        {
            this.PreferredLogLengthInMeters = preferredLogLengthInMeters;
            this.VolumeBySpecies = new Dictionary<FiaCode, TreeVolumeTable>
            {
                { FiaCode.PseudotsugaMenziesii, new TreeVolumeTable(maximumDiameterInCentimeters, maximumHeightInMeters, preferredLogLengthInMeters, PoudelRegressions.GetDouglasFirDiameterInsideBark, scribnerFromLumberRecovery) }
            };
        }

        public void GetGradedVolume(Trees trees, int[] individualTreeSelection, int harvestPeriod, out double cubic2saw, out double cubic3saw, out double cubic4saw, out double scribner2saw, out double scribner3saw, out double scribner4saw)
        {
            TreeVolumeTable volumeTable = this.VolumeBySpecies[trees.Species];
            cubic2saw = 0.0;
            cubic3saw = 0.0;
            cubic4saw = 0.0;
            scribner2saw = 0.0;
            scribner3saw = 0.0;
            scribner4saw = 0.0;
            for (int compactedTreeIndex = 0; compactedTreeIndex < trees.Count; ++compactedTreeIndex)
            {
                int uncompactedTreeIndex = trees.UncompactedIndex[compactedTreeIndex];
                if (individualTreeSelection[uncompactedTreeIndex] != harvestPeriod)
                {
                    // tree was either removed previously or was retained rather than thinned
                    continue;
                }

                float dbhInCm = trees.Dbh[compactedTreeIndex];
                float heightInMeters = trees.Height[compactedTreeIndex];
                if (trees.Units == Units.English)
                {
                    dbhInCm *= Constant.CentimetersPerInch;
                    heightInMeters *= Constant.MetersPerFoot;
                }
                float expansionFactor = trees.LiveExpansionFactor[compactedTreeIndex];
                if (expansionFactor <= 0.0F)
                {
                    continue;
                }

                // compare greater than or equals to avoid overstep in bilinear interpolation
                if (dbhInCm >= volumeTable.MaximumDiameterInCentimeters)
                {
                    throw new NotSupportedException(trees.Species + " " + trees.Tag[compactedTreeIndex] + "'s diameter of " + dbhInCm.ToString("0.0") + " cm exceeds the species' volume table capacity of " + volumeTable.MaximumDiameterInCentimeters.ToString("0.0") + " cm.");
                }
                if (heightInMeters >= volumeTable.MaximumHeightInMeters)
                {
                    throw new NotSupportedException(trees.Species + " " + trees.Tag[compactedTreeIndex] + "'s height of " + heightInMeters.ToString("0.0") + " m exceeds the species' volume table capacity of " + volumeTable.MaximumHeightInMeters.ToString("0.0") + " m.");
                }

                // bilinear interpolation
                float dbhPosition = dbhInCm / volumeTable.DiameterClassSizeInCentimeters;
                int dbhIndex = (int)dbhPosition;
                float dbhFraction = dbhPosition - dbhIndex;

                float heightPosition = heightInMeters / volumeTable.HeightClassSizeInMeters;
                int heightIndex = (int)heightPosition;
                float heightFraction = heightPosition - heightIndex;

                float cubic2sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Cubic2Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Cubic2Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Cubic2Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Cubic2Saw[dbhIndex + 1, heightIndex + 1]);
                cubic2saw += expansionFactor * cubic2sawForTree;
                float cubic3sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Cubic3Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Cubic3Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Cubic3Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Cubic3Saw[dbhIndex + 1, heightIndex + 1]);
                cubic3saw += expansionFactor * cubic3sawForTree;
                float cubic4sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Cubic4Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Cubic4Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Cubic4Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Cubic4Saw[dbhIndex + 1, heightIndex + 1]);
                cubic4saw += expansionFactor * cubic4sawForTree;

                float scribner2sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Scribner2Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Scribner2Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Scribner2Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Scribner2Saw[dbhIndex + 1, heightIndex + 1]);
                scribner2saw += expansionFactor * scribner2sawForTree;
                float scribner3sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Scribner3Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Scribner3Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Scribner3Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Scribner3Saw[dbhIndex + 1, heightIndex + 1]);
                scribner3saw += expansionFactor * scribner3sawForTree;
                float scribner4sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Scribner4Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Scribner4Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Scribner4Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Scribner4Saw[dbhIndex + 1, heightIndex + 1]);
                scribner4saw += expansionFactor * scribner4sawForTree;
            }
        }

        public void GetGradedVolume(Trees trees, out double cubic2saw, out double cubic3saw, out double cubic4saw, out double scribner2saw, out double scribner3saw, out double scribner4saw)
        {
            TreeVolumeTable volumeTable = this.VolumeBySpecies[trees.Species];
            cubic2saw = 0.0;
            cubic3saw = 0.0;
            cubic4saw = 0.0;
            scribner2saw = 0.0;
            scribner3saw = 0.0;
            scribner4saw = 0.0;
            for (int compactedTreeIndex = 0; compactedTreeIndex < trees.Count; ++compactedTreeIndex)
            {
                float dbhInCm = trees.Dbh[compactedTreeIndex];
                float heightInMeters = trees.Height[compactedTreeIndex];
                if (trees.Units == Units.English)
                {
                    dbhInCm *= Constant.CentimetersPerInch;
                    heightInMeters *= Constant.MetersPerFoot;
                }
                float expansionFactor = trees.LiveExpansionFactor[compactedTreeIndex];
                if (expansionFactor <= 0.0F)
                {
                    continue;
                }

                if (dbhInCm >= volumeTable.MaximumDiameterInCentimeters)
                {
                    throw new NotSupportedException(trees.Species + " " + trees.Tag[compactedTreeIndex] + "'s diameter of " + dbhInCm.ToString("0.0") + "  cm exceeds the species' volume table capacity of " + volumeTable.MaximumDiameterInCentimeters.ToString("0.0") + " cm.");
                }
                if (heightInMeters >= volumeTable.MaximumHeightInMeters)
                {
                    throw new NotSupportedException(trees.Species + " " + trees.Tag[compactedTreeIndex] + "'s height of " + heightInMeters.ToString("0.0") + "  m exceeds the species' volume table capacity of " + volumeTable.MaximumHeightInMeters.ToString("0.0") + " m.");
                }

                // bilinear interpolation
                float dbhPosition = dbhInCm / volumeTable.DiameterClassSizeInCentimeters;
                int dbhIndex = (int)dbhPosition;
                float dbhFraction = dbhPosition - dbhIndex;

                float heightPosition = heightInMeters / volumeTable.HeightClassSizeInMeters;
                int heightIndex = (int)heightPosition;
                float heightFraction = heightPosition - heightIndex;

                float cubic2sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Cubic2Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Cubic2Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Cubic2Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Cubic2Saw[dbhIndex + 1, heightIndex + 1]);
                cubic2saw += expansionFactor * cubic2sawForTree;
                float cubic3sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Cubic3Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Cubic3Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Cubic3Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Cubic3Saw[dbhIndex + 1, heightIndex + 1]);
                cubic3saw += expansionFactor * cubic3sawForTree;
                float cubic4sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Cubic4Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Cubic4Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Cubic4Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Cubic4Saw[dbhIndex + 1, heightIndex + 1]);
                cubic4saw += expansionFactor * cubic4sawForTree;

                float scribner2sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Scribner2Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Scribner2Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Scribner2Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Scribner2Saw[dbhIndex + 1, heightIndex + 1]);
                scribner2saw += expansionFactor * scribner2sawForTree;
                float scribner3sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Scribner3Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Scribner3Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Scribner3Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Scribner3Saw[dbhIndex + 1, heightIndex + 1]);
                scribner3saw += expansionFactor * scribner3sawForTree;
                float scribner4sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Scribner4Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Scribner4Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Scribner4Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Scribner4Saw[dbhIndex + 1, heightIndex + 1]);
                scribner4saw += expansionFactor * scribner4sawForTree;
            }
        }

        public void GetScribnerVolume(Trees trees, int[] individualTreeSelection, int harvestPeriod, out double scribner2saw, out double scribner3saw, out double scribner4saw)
        {
            TreeVolumeTable volumeTable = this.VolumeBySpecies[trees.Species];
            scribner2saw = 0.0;
            scribner3saw = 0.0;
            scribner4saw = 0.0;
            for (int compactedTreeIndex = 0; compactedTreeIndex < trees.Count; ++compactedTreeIndex)
            {
                int uncompactedTreeIndex = trees.UncompactedIndex[compactedTreeIndex];
                if (individualTreeSelection[uncompactedTreeIndex] != harvestPeriod)
                {
                    // tree was either harvested previously or was retained rather than thinned
                    continue;
                }

                float dbhInCm = trees.Dbh[compactedTreeIndex];
                float heightInMeters = trees.Height[compactedTreeIndex];
                if (trees.Units == Units.English)
                {
                    dbhInCm *= Constant.CentimetersPerInch;
                    heightInMeters *= Constant.MetersPerFoot;
                }
                float expansionFactor = trees.LiveExpansionFactor[compactedTreeIndex];
                if (expansionFactor <= 0.0F)
                {
                    continue;
                }

                if (dbhInCm >= volumeTable.MaximumDiameterInCentimeters)
                {
                    throw new NotSupportedException(trees.Species + " " + trees.Tag[compactedTreeIndex] + "'s diameter of " + dbhInCm.ToString("0.0") + "  cm exceeds the species' volume table capacity of " + volumeTable.MaximumDiameterInCentimeters.ToString("0.0") + " cm.");
                }
                if (heightInMeters >= volumeTable.MaximumHeightInMeters)
                {
                    throw new NotSupportedException(trees.Species + " " + trees.Tag[compactedTreeIndex] + "'s height of " + heightInMeters.ToString("0.0") + "  m exceeds the species' volume table capacity of " + volumeTable.MaximumHeightInMeters.ToString("0.0") + " m.");
                }

                // bilinear interpolation
                float dbhPosition = dbhInCm / volumeTable.DiameterClassSizeInCentimeters;
                int dbhIndex = (int)dbhPosition;
                float dbhFraction = dbhPosition - dbhIndex;

                float heightPosition = heightInMeters / volumeTable.HeightClassSizeInMeters;
                int heightIndex = (int)heightPosition;
                float heightFraction = heightPosition - heightIndex;

                float scribner2sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Scribner2Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Scribner2Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Scribner2Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Scribner2Saw[dbhIndex + 1, heightIndex + 1]);
                scribner2saw += expansionFactor * scribner2sawForTree;
                float scribner3sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Scribner3Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Scribner3Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Scribner3Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Scribner3Saw[dbhIndex + 1, heightIndex + 1]);
                scribner3saw += expansionFactor * scribner3sawForTree;
                float scribner4sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Scribner4Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Scribner4Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Scribner4Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Scribner4Saw[dbhIndex + 1, heightIndex + 1]);
                scribner4saw += expansionFactor * scribner4sawForTree;
            }
        }

        public void GetScribnerVolume(Trees trees, out double scribner2saw, out double scribner3saw, out double scribner4saw)
        {
            TreeVolumeTable volumeTable = this.VolumeBySpecies[trees.Species];
            scribner2saw = 0.0;
            scribner3saw = 0.0;
            scribner4saw = 0.0;
            for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
            {
                float dbhInCm = trees.Dbh[treeIndex];
                float heightInMeters = trees.Height[treeIndex];
                if (trees.Units == Units.English)
                {
                    dbhInCm *= Constant.CentimetersPerInch;
                    heightInMeters *= Constant.MetersPerFoot;
                }
                float expansionFactor = trees.LiveExpansionFactor[treeIndex];
                if (expansionFactor <= 0.0F)
                {
                    continue;
                }

                if (dbhInCm >= volumeTable.MaximumDiameterInCentimeters)
                {
                    throw new NotSupportedException(trees.Species + " " + trees.Tag[treeIndex] + "'s diameter of " + dbhInCm.ToString("0.0") + "  cm exceeds the species' volume table capacity of " + volumeTable.MaximumDiameterInCentimeters.ToString("0.0") + " cm.");
                }
                if (heightInMeters >= volumeTable.MaximumHeightInMeters)
                {
                    throw new NotSupportedException(trees.Species + " " + trees.Tag[treeIndex] + "'s height of " + heightInMeters.ToString("0.0") + "  m exceeds the species' volume table capacity of " + volumeTable.MaximumHeightInMeters.ToString("0.0") + " m.");
                }

                // bilinear interpolation
                float dbhPosition = dbhInCm / volumeTable.DiameterClassSizeInCentimeters;
                int dbhIndex = (int)dbhPosition;
                float dbhFraction = dbhPosition - dbhIndex;

                float heightPosition = heightInMeters / volumeTable.HeightClassSizeInMeters;
                int heightIndex = (int)heightPosition;
                float heightFraction = heightPosition - heightIndex;

                float scribner2sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Scribner2Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Scribner2Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Scribner2Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Scribner2Saw[dbhIndex + 1, heightIndex + 1]);
                scribner2saw += expansionFactor * scribner2sawForTree;
                float scribner3sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Scribner3Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Scribner3Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Scribner3Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Scribner3Saw[dbhIndex + 1, heightIndex + 1]);
                scribner3saw += expansionFactor * scribner3sawForTree;
                float scribner4sawForTree = (1.0F - dbhFraction) * ((1.0F - heightFraction) * volumeTable.Scribner4Saw[dbhIndex, heightIndex] +
                                                                 heightFraction * volumeTable.Scribner4Saw[dbhIndex, heightIndex + 1]) +
                                         dbhFraction * ((1.0F - heightFraction) * volumeTable.Scribner4Saw[dbhIndex + 1, heightIndex] +
                                                        heightFraction * volumeTable.Scribner4Saw[dbhIndex + 1, heightIndex + 1]);
                scribner4saw += expansionFactor * scribner4sawForTree;
            }
        }
    }
}