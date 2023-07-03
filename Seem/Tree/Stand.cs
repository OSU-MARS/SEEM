using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mars.Seem.Tree
{
    public class Stand
    {
        public float AccessDistanceInM { get; set; }
        public float AccessSlopeInPercent { get; set; }
        public float AreaInHa { get; set; }
        public float CorridorLengthInM { get; set; }
        public float CorridorLengthInMTethered { get; private set; } // m, mean
        public float CorridorLengthInMUntethered { get; private set; } // m, mean
        public float ForwardingDistanceOnRoad { get; set; } // m, mean distance from corridor to log unload point
        public float MeanYardingDistanceFactor { get; set; }
        public string? Name { get; set; }
        public float? PlantingDensityInTreesPerHectare { get; set; }
        public float SlopeInPercent { get; set; }

        public SortedList<FiaCode, Trees> TreesBySpecies { get; private init; }

        public Stand()
        {
            this.AccessDistanceInM = Constant.HarvestCost.DefaultAccessDistanceInM;
            this.AccessSlopeInPercent = Constant.HarvestCost.DefaultAccessSlopeInPercent;
            // this.CorridorLength, ForwardingDistanceInStandTethered, and ForwardingDistanceInStandUntethered
            this.SetCorridorLength(Constant.HarvestCost.DefaultForwardingDistanceInStandTethered, Constant.HarvestCost.DefaultForwardingDistanceInStandUntethered);
            this.AreaInHa = Constant.HarvestCost.DefaultHarvestUnitSizeInHa;
            this.ForwardingDistanceOnRoad = Constant.HarvestCost.DefaultForwardingDistanceOnRoad;
            this.MeanYardingDistanceFactor = Constant.HarvestCost.MeanYardingDistanceFactor;
            this.Name = null;
            this.PlantingDensityInTreesPerHectare = null;
            this.SlopeInPercent = Constant.HarvestCost.DefaultSlopeInPercent;
            this.TreesBySpecies = new SortedList<FiaCode, Trees>();
        }

        public Stand(Stand other)
        {
            this.AccessDistanceInM = other.AccessDistanceInM;
            this.AccessSlopeInPercent = other.AccessSlopeInPercent;
            this.AreaInHa = other.AreaInHa;
            // this.CorridorLength, ForwardingDistanceInStandTethered, and ForwardingDistanceInStandUntethered
            this.SetCorridorLength(other.CorridorLengthInMTethered, other.CorridorLengthInMUntethered);
            this.ForwardingDistanceOnRoad = other.ForwardingDistanceOnRoad;
            this.MeanYardingDistanceFactor = other.MeanYardingDistanceFactor;
            this.Name = other.Name;
            this.PlantingDensityInTreesPerHectare = other.PlantingDensityInTreesPerHectare;
            this.SlopeInPercent = other.SlopeInPercent;
            this.TreesBySpecies = new(other.TreesBySpecies.Count);

            foreach (KeyValuePair<FiaCode, Trees> species in other.TreesBySpecies)
            {
                this.TreesBySpecies.Add(species.Key, new Trees(species.Value));
            }
        }

        public virtual Stand Clone()
        {
            return new Stand(this);
        }

        public float GetLiveBiomass()
        {
            float liveBiomass = 0.0F;
            foreach (Trees treesOfSpecies in this.TreesBySpecies.Values)
            {
                liveBiomass += PoudelRegressions.GetLiveBiomass(treesOfSpecies);
            }
            return liveBiomass;
        }

        public float GetQuadraticMeanDiameterInCentimeters()
        {
            // While simplified formulas are often given for QMD, arguably its complete form is
            //   QMD = sqrt(1 / ((1/100)^2 * pi/4) * BAH / TPH))
            // BAH is the basal area in m² per hectare, TPH is the number of trees per hectare and (1/100)^2 * pi/4 = 0.0000785 is the metric
            // forester's constant converting BAH to sum(EF * DBH^2) where DBH is each tree's diameter at breast height in cm and EF is the tree's
            // expansion factor. In cases where the expansion factor is constant, either due to a single radius with fixed plots or use of a single
            // prism factor, then for n trees TPH = EF * n and BAH = sum(EF * (1/100)^2 * pi/4 * DBH^2) = EF * (1/100)^2 * pi/4 * sum(DBH^2). The
            // expansion factor and (1/100)^2 * pi/4 then cancel out, reducing to the often used formula QMD = sqrt(sum(DBH^2) / n).
            //
            // When different trees have varying expansion factors the calculation of QMD becomes the weighted quadratic mean
            //  QMD = sqrt(1 / ((1/100)^2 * pi/4) * sum(EF * (1/100)^2 * pi/4 * DBH^2) / sum(EF)))
            //      = sqrt(sum(EF * DBH^2) / sum(EF))
            float metricSumExpansionFactorDbhSquared = 0.0F; // cm²/ha, no need to convert to m²/ha since QMD is returned in cm
            float metricSumExpansionFactorPerHa = 0.0F;
            foreach (Trees treesOfSpecies in this.TreesBySpecies.Values)
            {
                float diameterToCentimetersMultiplier = 1.0F;
                float hectareExpansionFactorMultiplier = 1.0F;
                if (treesOfSpecies.Units == Units.English)
                {
                    diameterToCentimetersMultiplier = Constant.CentimetersPerInch;
                    hectareExpansionFactorMultiplier = Constant.AcresPerHectare;
                }

                float treeSpeciesSumExpansionFactorDbhSquared = 0.0F;
                float treeSpeciesSumExpansionFactor = 0.0F;
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    float dbhInCm = diameterToCentimetersMultiplier * treesOfSpecies.Dbh[treeIndex];
                    float expansionFactorPerHa = hectareExpansionFactorMultiplier * treesOfSpecies.LiveExpansionFactor[treeIndex];
                    treeSpeciesSumExpansionFactorDbhSquared += expansionFactorPerHa * dbhInCm * dbhInCm;
                    treeSpeciesSumExpansionFactor += expansionFactorPerHa;
                }
                metricSumExpansionFactorDbhSquared += treeSpeciesSumExpansionFactorDbhSquared;
                metricSumExpansionFactorPerHa += treeSpeciesSumExpansionFactor;
            }

            if (metricSumExpansionFactorPerHa == 0.0F)
            {
                return 0.0F;
            }
            float qmdInCentimeters =  MathF.Sqrt(metricSumExpansionFactorDbhSquared / metricSumExpansionFactorPerHa);
            Debug.Assert((qmdInCentimeters > 0.0F) && (qmdInCentimeters < 200.0F));
            return qmdInCentimeters;
        }

        public float GetTopHeightInMeters()
        {
            // if needed, could also use H160
            // Garcia O, Batho A. 2005. Top Height Estimation in Lodgepole Pine Sample Plots. Western Journal of Applied Forestry 20(1):64-68.
            //   https://doi.org/10.1093/wjaf/20.1.64
            Units standUnits = this.GetUnits();
            float treesForTopHeight = 100.0F;
            if (standUnits == Units.English)
            {
                treesForTopHeight = 40.0F;
            }

            SortedList<float, float> expansionFactorByHeight = new();
            float topTrees = 0.0F;
            float minimumHeight = Single.MinValue;
            foreach (Trees treesOfSpecies in this.TreesBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    float height = treesOfSpecies.Height[treeIndex];
                    if (height < minimumHeight)
                    {
                        continue;
                    }

                    float expansionFactor = treesOfSpecies.LiveExpansionFactor[treeIndex];
                    if (expansionFactorByHeight.ContainsKey(height))
                    {
                        expansionFactorByHeight[height] += expansionFactor;
                    }
                    else
                    {
                        expansionFactorByHeight.Add(height, expansionFactor);
                    }
                    topTrees += expansionFactor;

                    if (topTrees > treesForTopHeight)
                    {
                        KeyValuePair<float, float> shortestExpansionFactor = expansionFactorByHeight.First();
                        float excessExpansionFactor = topTrees - treesForTopHeight;
                        if (shortestExpansionFactor.Value < excessExpansionFactor)
                        {
                            expansionFactorByHeight.RemoveAt(0);
                            topTrees -= shortestExpansionFactor.Value;

                            minimumHeight = expansionFactorByHeight.First().Key;
                        }
                    }
                }
            }

            if (topTrees <= 0.0F)
            {
                return 0.0F;
            }

            float topHeight = 0.0F;
            float expansionFactorToSkip = topTrees - treesForTopHeight;
            foreach (KeyValuePair<float, float> expansionFactorForHeight in expansionFactorByHeight)
            {
                float expansionFactor = expansionFactorForHeight.Value;
                if (expansionFactorToSkip > 0.0F)
                {
                    if (expansionFactor > expansionFactorToSkip)
                    {
                        expansionFactor -= expansionFactorToSkip;
                        expansionFactorToSkip = 0.0F;
                    }
                    else
                    {
                        expansionFactorToSkip -= expansionFactor;
                        continue;
                    }
                }
                topHeight += expansionFactor * expansionFactorForHeight.Key;
            }
            topHeight /= MathF.Min(topTrees, treesForTopHeight);

            if (standUnits == Units.English)
            {
                topHeight *= Constant.MetersPerFoot;
            }

            Debug.Assert(topHeight > 0.0F);
            return topHeight;
        }

        public int GetTreeRecordCount()
        {
            int treeRecords = 0;
            foreach (Trees treesOfSpecies in this.TreesBySpecies.Values)
            {
                treeRecords += treesOfSpecies.Count;
            }
            return treeRecords;
        }

        public Units GetUnits()
        {
            if (this.TreesBySpecies.Count < 1)
            {
                throw new NotSupportedException("Stand units are indeterminate if no trees are present.");
            }

            Units? units = null;
            foreach (Trees treesOfSpecies in this.TreesBySpecies.Values)
            {
                if (units == null)
                {
                    units = treesOfSpecies.Units;
                }
                else if (units.Value != treesOfSpecies.Units)
                {
                    throw new NotSupportedException("Stand units are indeterminate when multiple units are in use.");
                }
            }
            return units!.Value;
        }

        public void SetCorridorLength(float forwardingDistanceInStandTethered, float forwardingDistanceInStandUntethered)
        {
            if (forwardingDistanceInStandTethered < 0.0F)
            {
                throw new ArgumentOutOfRangeException(nameof(forwardingDistanceInStandUntethered));
            }
            if (forwardingDistanceInStandUntethered < 0.0F)
            {
                throw new ArgumentOutOfRangeException(nameof(forwardingDistanceInStandUntethered));
            }

            this.CorridorLengthInM = forwardingDistanceInStandTethered + forwardingDistanceInStandUntethered;
            this.CorridorLengthInMTethered = forwardingDistanceInStandTethered;
            this.CorridorLengthInMUntethered = forwardingDistanceInStandUntethered;
        }
    }
}
