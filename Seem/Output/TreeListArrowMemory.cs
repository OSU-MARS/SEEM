using Apache.Arrow;
using Apache.Arrow.Types;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Mars.Seem.Extensions;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;

namespace Mars.Seem.Output
{
    public class TreeListArrowMemory : ArrowMemory
    {
        public const string StandFieldName = "stand";
        public const string PlotFieldName = "plot";
        public const string TagFieldName = "tag";
        public const string SpeciesFieldName = "species";
        public const string YearFieldName = "year";
        public const string StandAgeFieldName = "standAge";
        public const string DbhFieldName = "dbh";
        public const string HeightFieldName = "height";
        public const string CrownRatioFieldName = "crownRatio";
        public const string LiveExpansionFactorFieldName = "liveExpansionFactor";
        public const string DeadExpansionFactorFieldName = "deadExpansionFactor";

        public TreeListArrowMemory(List<StandTrajectory> trajectories, int? startYear) 
            : base(TreeListArrowMemory.CreateSchema(), maximumBatchLength: 10 * 1000 * 1000)
        {
            this.TotalNumberOfRecords = 0;

            // find record batch sizes
            List<(int batchStartIndex, int batchEndIndex, int recordsInBatch)> recordBatchPlans = [];
            int trajectoryStartIndex = 0;
            int treeTimestepsInBatch = 0;
            for (int trajectoryIndex = 0; trajectoryIndex < trajectories.Count; ++trajectoryIndex)
            {
                StandTrajectory trajectory = trajectories[trajectoryIndex];
                int treeTimestepsInTrajectory = 0;
                for (int periodIndex = 0; periodIndex < trajectory.StandByPeriod.Length; ++periodIndex)
                {
                    Stand? stand = trajectory.StandByPeriod[periodIndex];
                    if (stand != null)
                    {
                        treeTimestepsInTrajectory += stand.GetTreeRecordCount();
                    }
                }

                int batchSizeWithTrajectory = treeTimestepsInBatch + treeTimestepsInTrajectory;
                if (batchSizeWithTrajectory < this.MaximumBatchLength)
                {
                    treeTimestepsInBatch = batchSizeWithTrajectory;
                }
                else
                {
                    recordBatchPlans.Add((trajectoryStartIndex, trajectoryIndex, treeTimestepsInBatch));
                    trajectoryStartIndex = trajectoryIndex;
                    treeTimestepsInBatch = treeTimestepsInTrajectory;
                }
            }

            recordBatchPlans.Add((trajectoryStartIndex, trajectories.Count, treeTimestepsInBatch));

            // create record batches
            for (int batchIndex = 0; batchIndex < recordBatchPlans.Count; ++batchIndex)
            {
                (trajectoryStartIndex, int trajectoryEndIndex, treeTimestepsInBatch) = recordBatchPlans[batchIndex];

                TreeListBatch batch = new(treeTimestepsInBatch);
                int startIndexInRecordBatch = 0;
                for (int trajectoryIndex = trajectoryStartIndex; trajectoryIndex < trajectoryEndIndex; ++trajectoryIndex)
                {
                    StandTrajectory trajectory = trajectories[trajectoryIndex];
                    startIndexInRecordBatch = TreeListArrowMemory.CopyStandTrajectoryToArrowBatch(trajectory, startYear, batch, startIndexInRecordBatch);
                }

                this.RecordBatches.Add(new(this.Schema, batch.AsArrowArrays(), treeTimestepsInBatch));
                this.TotalNumberOfRecords += treeTimestepsInBatch;
            }
        }

        private static int CopyStandTrajectoryToArrowBatch(StandTrajectory trajectory, int? startYear, TreeListBatch batchArrays, int startIndexInRecordBatch)
        {
            if (UInt32.TryParse(trajectory.Name, out UInt32 standID) == false)
            {
                throw new NotSupportedException($"Stand trajectory name '{trajectory.Name}' could not be converted to an unsigned 32 bit integer. For the moment, trajectory names are required to be stand IDs.");
            }

            Int16 year = startYear != null ? (Int16)startYear.Value : Constant.NoDataInt16;
            Int16 standAge = (Int16)trajectory.PeriodZeroAgeInYears;
            Int16 periodLengthInYears = (Int16)trajectory.PeriodLengthInYears;
            int recordIndex = startIndexInRecordBatch;
            for (int periodIndex = 0; periodIndex < trajectory.StandByPeriod.Length; ++periodIndex)
            {
                Stand stand = trajectory.StandByPeriod[periodIndex] ?? throw new NotSupportedException($"Stand information missing for period {periodIndex}.");
                for (int speciesIndex = 0; speciesIndex < stand.TreesBySpecies.Count; ++speciesIndex)
                {
                    Trees treesOfSpecies = stand.TreesBySpecies.Values[speciesIndex];
                    int treeCount = treesOfSpecies.Count;

                    batchArrays.Stand.AsSpan().Slice(recordIndex, treeCount).Fill(standID);
                    treesOfSpecies.Plot[..treeCount].CopyTo(batchArrays.Plot.AsSpan().Slice(recordIndex, treeCount));
                    treesOfSpecies.Tag[..treeCount].CopyTo(batchArrays.Tag.AsSpan().Slice(recordIndex, treeCount));
                    batchArrays.Species.AsSpan().Slice(recordIndex, treeCount).Fill(treesOfSpecies.Species);
                    batchArrays.Year.AsSpan().Slice(recordIndex, treeCount).Fill(year);
                    batchArrays.StandAge.AsSpan().Slice(recordIndex, treeCount).Fill(standAge);

                    treesOfSpecies.CrownRatio[..treeCount].CopyTo(batchArrays.CrownRatio.AsSpan().Slice(recordIndex, treeCount));

                    (float diameterToCmMultiplier, float heightToMetersMultiplier, float hectareExpansionFactorMultiplier) = treesOfSpecies.Units.GetConversionToMetric();
                    for (int treeIndex = 0; treeIndex < treeCount; ++recordIndex, ++treeIndex)
                    {
                        batchArrays.Dbh[recordIndex] = diameterToCmMultiplier * treesOfSpecies.Dbh[treeIndex];
                        batchArrays.Height[recordIndex] = heightToMetersMultiplier * treesOfSpecies.Height[treeIndex];
                        batchArrays.LiveExpansionFactor[recordIndex] = hectareExpansionFactorMultiplier * treesOfSpecies.LiveExpansionFactor[treeIndex];
                        batchArrays.DeadExpansionFactor[recordIndex] = hectareExpansionFactorMultiplier * treesOfSpecies.DeadExpansionFactor[treeIndex];
                    }
                }

                if (year != Constant.NoDataInt16)
                {
                    year += periodLengthInYears;
                }

                standAge += periodLengthInYears;
            }

            return recordIndex;
        }

        private static Schema CreateSchema()
        {
            List<Field> fields =
            [
                new(TreeListArrowMemory.StandFieldName, UInt32Type.Default, false),
                new(TreeListArrowMemory.PlotFieldName, Int32Type.Default, false),
                new(TreeListArrowMemory.TagFieldName, Int32Type.Default, false),
                new(TreeListArrowMemory.SpeciesFieldName, Int16Type.Default, false),
                new(TreeListArrowMemory.YearFieldName, UInt16Type.Default, false),
                new(TreeListArrowMemory.StandAgeFieldName, UInt16Type.Default, false),
                new(TreeListArrowMemory.DbhFieldName, FloatType.Default, false),
                new(TreeListArrowMemory.HeightFieldName, FloatType.Default, false),
                new(TreeListArrowMemory.CrownRatioFieldName, FloatType.Default, false),
                new(TreeListArrowMemory.LiveExpansionFactorFieldName, FloatType.Default, false),
                new(TreeListArrowMemory.DeadExpansionFactorFieldName, FloatType.Default, false)
            ];

            Dictionary<string, string> metadata = new()
            {
                { TreeListArrowMemory.StandFieldName, "stand ID" },
                { TreeListArrowMemory.PlotFieldName, "plot ID" },
                { TreeListArrowMemory.TagFieldName, "tree ID" },
                { TreeListArrowMemory.SpeciesFieldName, "Integer code for tree species, currently a USFS FIA code (US Forest Service Forest Inventory and Analysis, 16 bit)." },
                { TreeListArrowMemory.YearFieldName, "calendar year, CE, if specified" },
                { TreeListArrowMemory.StandAgeFieldName, "nominal age of dominant and codominant trees in stand, years" },
                { TreeListArrowMemory.DbhFieldName, "diameter at breast height, cm" },
                { TreeListArrowMemory.HeightFieldName, "tree height, m" },
                { TreeListArrowMemory.CrownRatioFieldName, "crown ratio, fraction of height" },
                { TreeListArrowMemory.LiveExpansionFactorFieldName, "live trees per hectare" },
                { TreeListArrowMemory.DeadExpansionFactorFieldName, "newly dead trees and snags per hectare" }
            };

            return new Schema(fields, metadata);
        }

        public class TreeListBatch // mutable view of a record batch
        {
            public UInt32[] Stand { get; private init; }
            public Int32[] Plot { get; private init; }
            public Int32[] Tag { get; private init; }
            public FiaCode[] Species { get; private init; }
            public Int16[] Year { get; private init; }
            public Int16[] StandAge { get; private init; }
            public float[] Dbh { get; private init; }
            public float[] Height { get; private init; }
            public float[] CrownRatio { get; private init; }
            public float[] LiveExpansionFactor { get; private init; }
            public float[] DeadExpansionFactor { get; private init; }

            public TreeListBatch(int capacityInRecords)
            {
                this.Stand = new UInt32[capacityInRecords];
                this.Plot = new Int32[capacityInRecords];
                this.Tag = new Int32[capacityInRecords];
                this.Species = new FiaCode[capacityInRecords];
                this.Year = new Int16[capacityInRecords];
                this.StandAge = new Int16[capacityInRecords];
                this.Dbh = new float[capacityInRecords];
                this.Height = new float[capacityInRecords];
                this.CrownRatio = new float[capacityInRecords];
                this.LiveExpansionFactor = new float[capacityInRecords];
                this.DeadExpansionFactor = new float[capacityInRecords];
            }

            public static void ReadToStandDictionary(RecordBatch arrowBatch, Dictionary<UInt32, Stand> standsByID, float defaultCrownRatio)
            {
                IArrowArray[] fields = [.. arrowBatch.Arrays];
                Schema schema = arrowBatch.Schema;

                UInt32Array? standIDarray = ArrowMemory.MaybeGetArray<UInt32Array>("standID", schema, fields);
                UInt32Array? treeIDarrray = ArrowMemory.MaybeGetArray<UInt32Array>("treeID", schema, fields);
                UInt16Array? fiaCodeArray = ArrowMemory.MaybeGetArray<UInt16Array>("fiaCode", schema, fields);
                FloatArray? heightArray = ArrowMemory.MaybeGetArray<FloatArray>("height", schema, fields);
                FloatArray? dbhArray = ArrowMemory.MaybeGetArray<FloatArray>("dbh", schema, fields);
                BooleanArray? isSnagArray = ArrowMemory.MaybeGetArray<BooleanArray>("isSnag", schema, fields);
                if ((standIDarray == null) || (treeIDarrray == null) || (fiaCodeArray == null) || (heightArray == null) || (dbhArray == null) || (isSnagArray == null))
                {
                    throw new ArgumentException($"Record batch has an unknown schema. Currently the only schema supported must contain the fields standID (UInt32), treeID (UInt32), fiaCode (UInt16), height (float), dbh (float), isSnag (bool).");
                }

                ReadOnlySpan<UInt32> standIDs = standIDarray.Values;
                ReadOnlySpan<UInt32> treeIDs = treeIDarrray.Values;
                ReadOnlySpan<UInt16> fiaCodes = fiaCodeArray.Values;
                ReadOnlySpan<float> heights = heightArray.Values;
                ReadOnlySpan<float> diametersAtBreastHeight = dbhArray.Values;
                ReadOnlySpan<byte> isSnagBytes = isSnagArray.Values;

                for (int treeIndex = 0; treeIndex < arrowBatch.Length; ++treeIndex)
                {
                    UInt32 standID = standIDs[treeIndex];
                    if (standsByID.TryGetValue(standID, out Stand? stand) == false)
                    {
                        throw new ArgumentOutOfRangeException(nameof(arrowBatch), $"Stand ID {standID} at record {treeIndex} is not present in stands dictionary. Is the dictionary complete and in sync with the tree list?");
                    }

                    UInt32 treeID = treeIDs[treeIndex];
                    FiaCode species = (FiaCode)fiaCodes[treeIndex];
                    float dbh = diametersAtBreastHeight[treeIndex];
                    float height = heights[treeIndex];
                    TreeConditionCode codes = BitUtility.GetBit(isSnagBytes, treeIndex) ? TreeConditionCode.Snag : TreeConditionCode.Live;

                    // add tree with placeholder crown ratio
                    if (stand.TreesBySpecies.TryGetValue(species, out Trees? treesOfSpecies) == false)
                    {
                        treesOfSpecies = new Trees(species, minimumSize: 1, Units.Metric);
                        stand.TreesBySpecies.Add(species, treesOfSpecies);
                    }

                    treesOfSpecies.Add(plot: 1, (Int32)treeID, dbh, height, defaultCrownRatio, 1.0F, codes);
                }
            }

            public IArrowArray[] AsArrowArrays() // Arrow's read only view
            {
                return [ this.Stand.AsArrowArray(),
                         this.Plot.AsArrowArray(),
                         this.Tag.AsArrowArray(),
                         this.Species.AsArrowArray(),
                         this.Year.AsArrowArray(),
                         this.StandAge.AsArrowArray(),
                         this.Dbh.AsArrowArray(),
                         this.Height.AsArrowArray(),
                         this.CrownRatio.AsArrowArray(),
                         this.LiveExpansionFactor.AsArrowArray(),
                         this.DeadExpansionFactor.AsArrowArray() ];
            }
        }
    }
}
