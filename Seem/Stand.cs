using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Osu.Cof.Ferm
{
    public class Stand
    {
        public string? Name { get; set; }

        public SortedDictionary<FiaCode, Trees> TreesBySpecies { get; private init; }

        public Stand()
        {
            this.Name = null;
            this.TreesBySpecies = new SortedDictionary<FiaCode, Trees>();
        }

        public float GetQuadraticMeanDiameter()
        {
            float dbhSumOfSquares = 0.0F;
            int treeCount = 1;
            foreach (Trees treesOfSpecies in this.TreesBySpecies.Values)
            {
                for (int treeIndex = 0; treeIndex < treesOfSpecies.Count; ++treeIndex)
                {
                    float dbh = treesOfSpecies.Dbh[treeIndex];
                    float expansionFactor = treesOfSpecies.LiveExpansionFactor[treeIndex];
                    if (expansionFactor > 0.0F)
                    {
                        dbhSumOfSquares += dbh * dbh;
                        ++treeCount;
                    }
                }
            }

            if (treeCount == 0)
            {
                return 0.0F;
            }
            float qmd =  MathF.Sqrt(dbhSumOfSquares / treeCount);
            Debug.Assert((qmd > 0.0F) && (qmd < 200.0F));
            return qmd;
        }

        public float GetTopHeight()
        {
            float treesForTopHeight = 100.0F;
            if (this.GetUnits() == Units.English)
            {
                treesForTopHeight = 40.0F;
            }

            SortedList<float, float> expansionFactorByHeight = new SortedList<float, float>();
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
    }
}
