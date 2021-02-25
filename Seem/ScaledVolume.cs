using System;
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
                { FiaCode.PseudotsugaMenziesii, new TreeVolumeTable(maximumDiameterInCentimeters, maximumHeightInMeters, preferredLogLengthInMeters, this.GetDouglasFirDiameterInsideBark, scribnerFromLumberRecovery) }
            };
        }

        private float GetDouglasFirDiameterInsideBark(float dbhInCm, float heightInM, float evaluationHeightInM)
        {
            // Poudel K, Temesgen H, Gray AN. 2018. Estimating upper stem diameters and volume of Douglas-fir and Western hemlock
            //   trees in the Pacific northwest. Forest Ecosystems 5:16. https://doi.org/10.1186/s40663-018-0134-2
            // Table 4: M4 (Kozak 2004) form
            // regression's fitted diameter range: 1.8-114 cm
            //                     height range: 2.4-64 m
            // allow extrapolation beyond fit range in lieu of a better predictor but block height-diameter ratios so high b3 becomes
            // negative and table generation fails
            if (dbhInCm > 135.0F)
            {
                throw new ArgumentOutOfRangeException(nameof(dbhInCm), "Diameter of " + dbhInCm.ToString("0.0") + " cm exceeds limit of 135.0 cm.");
            }
            if (heightInM > 75.0F)
            {
                throw new ArgumentOutOfRangeException(nameof(heightInM), "Height of " + heightInM.ToString("0.0") + " m exceeds limit of 75.0 m.");
            }
            if ((evaluationHeightInM < 0.0F) || (evaluationHeightInM > heightInM))
            {
                throw new ArgumentOutOfRangeException(nameof(evaluationHeightInM), "Evaluation height of " + evaluationHeightInM.ToString("0.00") + " m exceeds tree height of " + heightInM.ToString("0.00") + " m.");
            }

            const float b1 = 1.04208F;
            const float b2 = 0.99771F;
            const float b3 = -0.03111F;
            const float b4 = 0.53788F;
            const float b5 = -1.01291F;
            const float b6 = 0.56813F;
            const float b7 = 4.96019F;
            const float b8 = 0.04124F;
            const float b9 = -0.34417F;
            float t = evaluationHeightInM / heightInM;
            float k = 1.3F / heightInM;
            float oneMinusCubeRootT = 1 - MathV.Pow(t, 1.0F / 3.0F);
            float tkRatio = oneMinusCubeRootT / (1 - MathV.Pow(k, 1.0F / 3.0F));
            float dibInCm = b1 * MathV.Pow(dbhInCm, b2) * MathV.Pow(heightInM, b3) * MathV.Pow(tkRatio, b4 * t * t * t * t + b5 / MathV.Exp(dbhInCm / heightInM) + b6 * MathV.Pow(tkRatio, 0.1F) + b7 / dbhInCm + b8 * MathV.Pow(heightInM, oneMinusCubeRootT) + b9 * tkRatio);
            // float dibMathFcheck = b1 * MathF.Pow(dbhInCm, b2) * MathF.Pow(heightInM, b3) * MathF.Pow(tkRatio, b4 * t * t * t * t + b5 / MathF.Exp(dbhInCm / heightInM) + b6 * MathF.Pow(tkRatio, 0.1F) + b7 / dbhInCm + b8 * MathF.Pow(heightInM, 1 - MathF.Pow(evaluationHeightInM / heightInM, 1.0F / 3.0F)) + b9 * tkRatio);
            return dibInCm;
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