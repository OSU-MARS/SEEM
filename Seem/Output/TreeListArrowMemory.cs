using Apache.Arrow;
using Apache.Arrow.Types;
using Mars.Seem.Extensions;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mars.Seem.Output
{
    public class TreeListArrowMemory : ArrowMemory
    {
        private byte[]? stand;
        private byte[]? plot;
        private byte[]? tag;
        private byte[]? species;
        private byte[]? year;
        private byte[]? standAge;
        private byte[]? dbh;
        private byte[]? height;
        private byte[]? crownRatio;
        private byte[]? liveExpansionFactor;
        private byte[]? deadExpansionFactor;

        public TreeListArrowMemory(List<StandTrajectory> trajectories, int? startYear) 
            : base(TreeListArrowMemory.CreateSchema(), TreeListArrowMemory.DefaultMaximumRecordsPerBatch)
        {
            this.stand = null;
            this.plot = null;
            this.tag = null;
            this.species = null;
            this.year = null;
            this.standAge = null;
            this.dbh = null;
            this.height = null;
            this.crownRatio = null;
            this.liveExpansionFactor = null;
            this.deadExpansionFactor = null;

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
                this.AppendNewBatch(treeTimestepsInBatch);

                int startIndexInRecordBatch = 0;
                for (int trajectoryIndex = trajectoryStartIndex; trajectoryIndex < trajectoryEndIndex; ++trajectoryIndex)
                {
                    StandTrajectory trajectory = trajectories[trajectoryIndex];
                    startIndexInRecordBatch = this.Add(trajectory, startYear, startIndexInRecordBatch);
                }

                this.TotalNumberOfRecords += treeTimestepsInBatch;
            }
        }

        private int Add(StandTrajectory trajectory, int? startYear, int startIndexInRecordBatch)
        {
            if (UInt32.TryParse(trajectory.Name, out UInt32 standID) == false)
            {
                throw new NotSupportedException("Stand trajectory name '" + trajectory.Name + "' could not be converted to an unsigned 32 bit integer. For the moment, trajectory names are required to be stand IDs.");
            }

            Span<UInt32> batchStand = MemoryMarshal.Cast<byte, UInt32>(this.stand);
            Span<Int32> batchPlot = MemoryMarshal.Cast<byte, Int32>(this.plot);
            Span<Int32> batchTag = MemoryMarshal.Cast<byte, Int32>(this.tag);
            Span<FiaCode> batchSpecies = MemoryMarshal.Cast<byte, FiaCode>(this.species);
            Span<Int16> batchYear = MemoryMarshal.Cast<byte, Int16>(this.year);
            Span<Int16> batchStandAge = MemoryMarshal.Cast<byte, Int16>(this.standAge);
            Span<float> batchDbh = MemoryMarshal.Cast<byte, float>(this.dbh);
            Span<float> batchHeight = MemoryMarshal.Cast<byte, float>(this.height);
            Span<float> batchCrownRatio = MemoryMarshal.Cast<byte, float>(this.crownRatio);
            Span<float> batchLiveExpansionFactor = MemoryMarshal.Cast<byte, float>(this.liveExpansionFactor);
            Span<float> batchDeadExpansionFactor = MemoryMarshal.Cast<byte, float>(this.deadExpansionFactor);

            Int16 year = startYear != null ? (Int16)startYear.Value : Constant.NoDataInt16;
            Int16 standAge = (Int16)trajectory.PeriodZeroAgeInYears;
            Int16 periodLengthInYears = (Int16)trajectory.PeriodLengthInYears;
            int recordIndex = startIndexInRecordBatch;
            for (int periodIndex = 0; periodIndex < trajectory.StandByPeriod.Length; ++periodIndex)
            {
                Stand stand = trajectory.StandByPeriod[periodIndex] ?? throw new NotSupportedException("Stand information missing for period " + periodIndex + ".");
                for (int speciesIndex = 0; speciesIndex < stand.TreesBySpecies.Count; ++speciesIndex)
                {
                    Trees treesOfSpecies = stand.TreesBySpecies.Values[speciesIndex];
                    int treeCount = treesOfSpecies.Count;

                    batchStand.Slice(recordIndex, treeCount).Fill(standID);
                    treesOfSpecies.Plot[..treeCount].CopyTo(batchPlot[recordIndex..]);
                    treesOfSpecies.Tag[..treeCount].CopyTo(batchTag[recordIndex..]);
                    batchSpecies.Slice(recordIndex, treeCount).Fill(treesOfSpecies.Species);
                    batchYear.Slice(recordIndex, treeCount).Fill(year);
                    batchStandAge.Slice(recordIndex, treeCount).Fill(standAge);

                    treesOfSpecies.CrownRatio[..treeCount].CopyTo(batchCrownRatio[recordIndex..]);

                    (float diameterToCmMultiplier, float heightToMetersMultiplier, float hectareExpansionFactorMultiplier) = treesOfSpecies.Units.GetConversionToMetric();
                    for (int treeIndex = 0; treeIndex < treeCount; ++recordIndex, ++treeIndex)
                    {
                        batchDbh[recordIndex] = diameterToCmMultiplier * treesOfSpecies.Dbh[treeIndex];
                        batchHeight[recordIndex] = heightToMetersMultiplier * treesOfSpecies.Height[treeIndex];
                        batchLiveExpansionFactor[recordIndex] = hectareExpansionFactorMultiplier * treesOfSpecies.LiveExpansionFactor[treeIndex];
                        batchDeadExpansionFactor[recordIndex] = hectareExpansionFactorMultiplier * treesOfSpecies.DeadExpansionFactor[treeIndex];
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

        private void AppendNewBatch(int capacityInRecords)
        {
            this.stand = new byte[capacityInRecords * sizeof(UInt32)];
            this.plot = new byte[capacityInRecords * sizeof(Int32)];
            this.tag = new byte[capacityInRecords * sizeof(Int32)];
            this.species = new byte[capacityInRecords * sizeof(FiaCode)];
            this.year = new byte[capacityInRecords * sizeof(Int16)];
            this.standAge = new byte[capacityInRecords * sizeof(Int16)];
            this.dbh = new byte[capacityInRecords * sizeof(float)];
            this.height = new byte[capacityInRecords * sizeof(float)];
            this.crownRatio = new byte[capacityInRecords * sizeof(float)];
            this.liveExpansionFactor = new byte[capacityInRecords * sizeof(float)];
            this.deadExpansionFactor = new byte[capacityInRecords * sizeof(float)];

            IArrowArray[] arrowArrays =
            [
                ArrowArrayExtensions.WrapInUInt32(this.stand),
                ArrowArrayExtensions.WrapInInt32(this.plot),
                ArrowArrayExtensions.WrapInInt32(this.tag),
                ArrowArrayExtensions.WrapInUInt16(this.species),
                ArrowArrayExtensions.WrapInInt16(this.year),
                ArrowArrayExtensions.WrapInInt16(this.standAge),
                ArrowArrayExtensions.WrapInFloat(this.dbh),
                ArrowArrayExtensions.WrapInFloat(this.height),
                ArrowArrayExtensions.WrapInFloat(this.crownRatio),
                ArrowArrayExtensions.WrapInFloat(this.liveExpansionFactor),
                ArrowArrayExtensions.WrapInFloat(this.deadExpansionFactor)
            ];

            this.RecordBatches.Add(new(this.Schema, arrowArrays, capacityInRecords));
        }

        private static Schema CreateSchema()
        {
            List<Field> fields =
            [
                new("stand", UInt32Type.Default, false),
                new("plot", Int32Type.Default, false),
                new("tag", Int32Type.Default, false),
                new("species", Int16Type.Default, false),
                new("year", UInt16Type.Default, false),
                new("standAge", UInt16Type.Default, false),
                new("dbh", FloatType.Default, false),
                new("height", FloatType.Default, false),
                new("crownRatio", FloatType.Default, false),
                new("liveExpansionFactor", FloatType.Default, false),
                new("deadExpansionFactor", FloatType.Default, false)
            ];

            Dictionary<string, string> metadata = new()
            {
                { "stand", "stand ID" },
                { "plot", "plot ID" },
                { "tag", "tree ID" },
                { "species", "Integer code for tree species, currently a USFS FIA code (US Forest Service Forest Inventory and Analysis, 16 bit)." },
                { "year", "calendar year, CE, if specified" },
                { "standAge", "nominal age of dominant and codominant trees in stand, years" },
                { "dbh", "diameter at breast height, cm" },
                { "height", "tree height, m" },
                { "crownRatio", "crown ratio, fraction of height" },
                { "liveExpansionFactor", "live trees per hectare" },
                { "deadExpansionFactor", "newly dead trees and snags per hectare" }
            };

            return new Schema(fields, metadata);
        }
    }
}
