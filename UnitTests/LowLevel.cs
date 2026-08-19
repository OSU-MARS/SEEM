using Apache.Arrow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mars.Seem.Extensions;
using Mars.Seem.Optimization;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using Mars.Seem.Output;

namespace Mars.Seem.Test
{
    [DeploymentItem("financial scenarios.xlsx")]
    [TestClass]
    public class LowLevel
    {
        [TestMethod]
        public void ArrowExtensions()
        {
            FiaCode[] dataFia = [ FiaCode.AbiesAmabalis, FiaCode.AbiesLasiocarpa, FiaCode.PinusLambertiana, FiaCode.TsugaMertensiana ];
            UInt16Array arrowFia = dataFia.AsArrowArray();
            float[] dataFloat = [ Single.NegativeInfinity, Single.MinValue, -1.0F, 0.0F, 1.0F, Single.MaxValue, Single.PositiveInfinity ];
            FloatArray arrowFloat = dataFloat.AsArrowArray();
            double[] dataDouble = [ Double.NegativeInfinity, Double.MinValue, -1.0, 0.0, 1.0, Double.MaxValue, Double.PositiveInfinity ];
            DoubleArray arrowDouble = dataDouble.AsArrowArray();

            sbyte[] dataInt8 = [ sbyte.MinValue, -1, 0, 1, sbyte.MaxValue ];
            Int8Array arrowInt8 = dataInt8.AsArrowArray();
            Int16[] dataInt16 = [ Int16.MinValue, -1, 0, 1, Int16.MaxValue ];
            Int16Array arrowInt16 = dataInt16.AsArrowArray();
            Int32[] dataInt32 = [ Int32.MinValue, -1, 0, 1, Int32.MaxValue ];
            Int32Array arrowInt32 = dataInt32.AsArrowArray();
            Int64[] dataInt64 = [ Int64.MinValue, -1, 0, 1, Int64.MaxValue ];
            Int64Array arrowInt64 = dataInt64.AsArrowArray();

            byte[] dataUInt8 = [ byte.MinValue, 2, 3, 4, 5, byte.MaxValue ];
            UInt8Array arrowUInt8 = dataUInt8.AsArrowArray();
            UInt16[] dataUInt16 = [ UInt16.MinValue, 1, 2, 3, 4, 5, UInt16.MaxValue ];
            UInt16Array arrowUInt16 = dataUInt16.AsArrowArray();
            UInt32[] dataUInt32 = [ UInt32.MinValue, 1, 2, 3, 4, 5, UInt32.MaxValue ];
            UInt32Array arrowUInt32 = dataUInt32.AsArrowArray();
            UInt64[] dataUInt64 = [ UInt64.MinValue, 1, 2, 3, 4, 5, UInt64.MaxValue ];
            UInt64Array arrowUInt64 = dataUInt64.AsArrowArray();

            ReadOnlySpan<UInt16> arrowValuesFia = arrowFia.Values;
            for (int index = 0; index < dataFia.Length; ++index)
            {
                Assert.IsTrue(arrowValuesFia[index] == (UInt16)dataFia[index], $"FiaCode marshaling failed at index {index}. {arrowValuesFia[index]} does not match {dataFia[index]}.");
            }

            ReadOnlySpan<float> arrowValuesFloat = arrowFloat.Values;
            ReadOnlySpan<double> arrowValuesDouble = arrowDouble.Values;
            Assert.IsTrue((arrowFloat.Length == dataFloat.Length) && (arrowValuesFloat.Length == dataFloat.Length), "Float length.");
            Assert.IsTrue((arrowDouble.Length == dataDouble.Length) && (arrowValuesDouble.Length == dataDouble.Length), "Double length.");
            for (int index = 0; index < dataFloat.Length; ++index)
            {
                Assert.IsTrue(arrowValuesFloat[index] == dataFloat[index], $"Float marshaling failed at index {index}. {arrowValuesFloat[index]} does not match {dataFloat[index]}.");
                Assert.IsTrue(arrowValuesDouble[index] == dataDouble[index], $"Double marshaling failed at index {index}. {arrowValuesDouble[index]} does not match {dataDouble[index]}.");
            }

            ReadOnlySpan<sbyte> arrowValuesInt8 = arrowInt8.Values;
            ReadOnlySpan<Int16> arrowValuesInt16 = arrowInt16.Values;
            ReadOnlySpan<Int32> arrowValuesInt32 = arrowInt32.Values;
            ReadOnlySpan<Int64> arrowValuesInt64 = arrowInt64.Values;
            Assert.IsTrue((arrowInt8.Length == dataInt8.Length) && (arrowValuesInt8.Length == dataInt8.Length), "Int8 length.");
            Assert.IsTrue((arrowInt16.Length == dataInt16.Length) && (arrowValuesInt16.Length == dataInt16.Length), "Int16 length.");
            Assert.IsTrue((arrowInt32.Length == dataInt32.Length) && (arrowValuesInt32.Length == dataInt32.Length), "Int32 length.");
            Assert.IsTrue((arrowInt64.Length == dataInt64.Length) && (arrowValuesInt64.Length == dataInt64.Length), "Int64 length.");
            for (int index = 0; index < dataInt8.Length; ++index)
            {
                Assert.IsTrue(arrowValuesInt8[index] == dataInt8[index], $"Int8 marshaling failed at index {index}. {arrowValuesInt8[index]} does not match {dataInt8[index]}.");
                Assert.IsTrue(arrowValuesInt16[index] == dataInt16[index], $"Int16 marshaling failed at index {index}. {arrowValuesInt16[index]} does not match {dataInt16[index]}.");
                Assert.IsTrue(arrowValuesInt32[index] == dataInt32[index], $"Int32 marshaling failed at index {index}. {arrowValuesInt32[index]} does not match {dataInt32[index]}.");
                Assert.IsTrue(arrowValuesInt64[index] == dataInt64[index], $"Int64 marshaling failed at index {index}. {arrowValuesInt64[index]} does not match {dataInt64[index]}.");
            }

            ReadOnlySpan<byte> arrowValuesUInt8 = arrowUInt8.Values;
            ReadOnlySpan<UInt16> arrowValuesUInt16 = arrowUInt16.Values;
            ReadOnlySpan<UInt32> arrowValuesUInt32 = arrowUInt32.Values;
            ReadOnlySpan<UInt64> arrowValuesUInt64 = arrowUInt64.Values;
            Assert.IsTrue((arrowUInt8.Length == dataUInt8.Length) && (arrowValuesUInt8.Length == dataUInt8.Length), "UInt8 length.");
            Assert.IsTrue((arrowUInt16.Length == dataUInt16.Length) && (arrowValuesUInt16.Length == dataUInt16.Length), "UInt16 length.");
            Assert.IsTrue((arrowUInt32.Length == dataUInt32.Length) && (arrowValuesUInt32.Length == dataUInt32.Length), "UInt32 length.");
            Assert.IsTrue((arrowUInt64.Length == dataUInt64.Length) && (arrowValuesUInt64.Length == dataUInt64.Length), "UInt64 length.");
            for (int index = 0; index < dataUInt8.Length; ++index)
            {
                Assert.IsTrue(arrowValuesUInt8[index] == dataUInt8[index], $"UInt8 marshaling failed at index {index}. {arrowValuesUInt8[index]} does not match {dataUInt8[index]}.");
                Assert.IsTrue(arrowValuesUInt16[index] == dataUInt16[index], $"UInt16 marshaling failed at index {index}. {arrowValuesUInt16[index]} does not match {dataUInt16[index]}.");
                Assert.IsTrue(arrowValuesUInt32[index] == dataUInt32[index], $"UInt32 marshaling failed at index {index}. {arrowValuesUInt32[index]} does not match {dataUInt32[index]}.");
                Assert.IsTrue(arrowValuesUInt64[index] == dataUInt64[index], $"UInt64 marshaling failed at index {index}. {arrowValuesUInt64[index]} does not match {dataUInt64[index]}.");
            }
        }

        [TestMethod]
        public void ArrowSerialization()
        {
            // stands
            WriteStandTrajectoryContext maximumWriteContext = new(harvestsOnly: false, heuristicParameters: false, noTreeGrowth: false, noFinancial: false, noCarbon: false, noHarvestCosts: false, noTimberSorts: false, noEquipmentProductivity: false, diameterClassSize: Constant.Bucking.VolumeTableDiameterClassSizeInCentimeters, maximumDiameter: Constant.Bucking.VolumeTableMaximumDiameterToLogInCentimeters);
            WriteStandTrajectoryContext noTreeGrowthWriteContext = new(harvestsOnly: false, heuristicParameters: false, noTreeGrowth: true, noFinancial: false, noCarbon: false, noHarvestCosts: false, noTimberSorts: false, noEquipmentProductivity: false, diameterClassSize: Constant.Bucking.VolumeTableDiameterClassSizeInCentimeters, maximumDiameter: Constant.Bucking.VolumeTableMaximumDiameterToLogInCentimeters);
            WriteStandTrajectoryContext noFinancialWriteContext = new(harvestsOnly: false, heuristicParameters: false, noTreeGrowth: false, noFinancial: true, noCarbon: false, noHarvestCosts: false, noTimberSorts: false, noEquipmentProductivity: false, diameterClassSize: Constant.Bucking.VolumeTableDiameterClassSizeInCentimeters, maximumDiameter: Constant.Bucking.VolumeTableMaximumDiameterToLogInCentimeters);
            WriteStandTrajectoryContext noHarvestCostWriteContext = new(harvestsOnly: false, heuristicParameters: false, noTreeGrowth: false, noFinancial: false, noCarbon: false, noHarvestCosts: true, noTimberSorts: false, noEquipmentProductivity: false, diameterClassSize: Constant.Bucking.VolumeTableDiameterClassSizeInCentimeters, maximumDiameter: Constant.Bucking.VolumeTableMaximumDiameterToLogInCentimeters);
            WriteStandTrajectoryContext noTimberSortsWriteContext = new(harvestsOnly: false, heuristicParameters: false, noTreeGrowth: false, noFinancial: false, noCarbon: false, noHarvestCosts: false, noTimberSorts: true, noEquipmentProductivity: false, diameterClassSize: Constant.Bucking.VolumeTableDiameterClassSizeInCentimeters, maximumDiameter: Constant.Bucking.VolumeTableMaximumDiameterToLogInCentimeters);
            WriteStandTrajectoryContext noEquipmentProductivityWriteContext = new(harvestsOnly: false, heuristicParameters: false, noTreeGrowth: false, noFinancial: false, noCarbon: false, noHarvestCosts: false, noTimberSorts: false, noEquipmentProductivity: true, diameterClassSize: Constant.Bucking.VolumeTableDiameterClassSizeInCentimeters, maximumDiameter: Constant.Bucking.VolumeTableMaximumDiameterToLogInCentimeters);
            WriteStandTrajectoryContext minimumWriteContext = new(harvestsOnly: false, heuristicParameters: false, noTreeGrowth: true, noFinancial: true, noCarbon: true, noHarvestCosts: true, noTimberSorts: true, noEquipmentProductivity: true, diameterClassSize: Constant.Bucking.VolumeTableDiameterClassSizeInCentimeters, maximumDiameter: Constant.Bucking.VolumeTableMaximumDiameterToLogInCentimeters);

            StandTrajectoryArrowMemory maximumTrajectoryArrow = new(maximumWriteContext, totalNumberOfRecords: 1);
            StandTrajectoryArrowMemory noTreeGrowthTrajectoryArrow = new(noTreeGrowthWriteContext, totalNumberOfRecords: 1);
            StandTrajectoryArrowMemory noFinancialTrajectoryArrow = new(noFinancialWriteContext, totalNumberOfRecords: 1);
            StandTrajectoryArrowMemory noHarvestCostsTrajectoryArrow = new(noHarvestCostWriteContext, totalNumberOfRecords: 1);
            StandTrajectoryArrowMemory noTimberSortsTrajectoryArrow = new(noTimberSortsWriteContext, totalNumberOfRecords: 1);
            StandTrajectoryArrowMemory noEquipmentProductivityTrajectoryArrow = new(noEquipmentProductivityWriteContext, totalNumberOfRecords: 1);
            StandTrajectoryArrowMemory minimumTrajectoryArrow = new(minimumWriteContext, totalNumberOfRecords: 1);

            int maximumTrajectoryArrowColumns = maximumTrajectoryArrow.Schema.FieldsList.Count;
            int noTreeGrowthTrajectoryArrowColumns = noTreeGrowthTrajectoryArrow.Schema.FieldsList.Count;
            int noFinancialTrajectoryArrowColumns = noFinancialTrajectoryArrow.Schema.FieldsList.Count;
            int noHarvestCostsTrajectoryArrowColumns = noHarvestCostsTrajectoryArrow.Schema.FieldsList.Count;
            int noTimberSortsTrajectoryArrowColumns = noTimberSortsTrajectoryArrow.Schema.FieldsList.Count;
            int noEquipmentProductivityTrajectoryArrowColumns = noEquipmentProductivityTrajectoryArrow.Schema.FieldsList.Count;
            int minimumTrajectoryArrowColumns = minimumTrajectoryArrow.Schema.FieldsList.Count;

            int maximumBytesPerStandTrajectoryRecord = maximumTrajectoryArrow.GetUncompressedBytesPerRow();
            int noTreeGrowthBytesPerStandTrajectoryRecord = noTreeGrowthTrajectoryArrow.GetUncompressedBytesPerRow();
            int noFinancialBytesPerStandTrajectoryRecord = noFinancialTrajectoryArrow.GetUncompressedBytesPerRow();
            int noHarvestCostsBytesPerStandTrajectoryRecord = noHarvestCostsTrajectoryArrow.GetUncompressedBytesPerRow();
            int noTimberSortsBytesPerStandTrajectoryRecord = noTimberSortsTrajectoryArrow.GetUncompressedBytesPerRow();
            int noEquipmentProductivityBytesPerStandTrajectoryRecord = noEquipmentProductivityTrajectoryArrow.GetUncompressedBytesPerRow();
            int minimumBytesPerStandTrajectoryRecord = minimumTrajectoryArrow.GetUncompressedBytesPerRow();

            Assert.IsTrue(maximumTrajectoryArrowColumns == 143, $"StandTrajectoryBatch(max) has {maximumTrajectoryArrowColumns} columns.");
            Assert.IsTrue(noTreeGrowthTrajectoryArrowColumns == 131, $"StandTrajectoryBatch(noTreeGrowth) has {noTreeGrowthTrajectoryArrowColumns} columns.");
            Assert.IsTrue(noFinancialTrajectoryArrowColumns == 141, $"StandTrajectoryBatch(noFinancial) has {noFinancialTrajectoryArrowColumns} columns.");
            Assert.IsTrue(noHarvestCostsTrajectoryArrowColumns == 120, $"StandTrajectoryBatch(noHarvestCosts) has {noHarvestCostsTrajectoryArrowColumns} columns.");
            Assert.IsTrue(noTimberSortsTrajectoryArrowColumns == 119, $"StandTrajectoryBatch(noTimberSorts) has {noTimberSortsTrajectoryArrowColumns} columns.");
            Assert.IsTrue(noEquipmentProductivityTrajectoryArrowColumns == 69, $"StandTrajectoryBatch(noEquipmentProductivity) has {noEquipmentProductivityTrajectoryArrowColumns} columns.");
            Assert.IsTrue(minimumTrajectoryArrowColumns == 8, $"StandTrajectoryBatch(minimum) has {minimumTrajectoryArrowColumns} columns.");

            Assert.IsTrue(maximumBytesPerStandTrajectoryRecord == 527, "StandTrajectoryBatch.GetBytesPerRecord(max)");
            Assert.IsTrue(noTreeGrowthBytesPerStandTrajectoryRecord == 479, "StandTrajectoryBatch.GetBytesPerRecord(noTreeGrowth)");
            Assert.IsTrue(noFinancialBytesPerStandTrajectoryRecord == 519, "StandTrajectoryBatch.GetBytesPerRecord(noFinancial)");
            Assert.IsTrue(noHarvestCostsBytesPerStandTrajectoryRecord == 441, "StandTrajectoryBatch.GetBytesPerRecord(noHarvestCosts)");
            Assert.IsTrue(noTimberSortsBytesPerStandTrajectoryRecord == 431, "StandTrajectoryBatch.GetBytesPerRecord(noTimberSorts)");
            Assert.IsTrue(noEquipmentProductivityBytesPerStandTrajectoryRecord == 258, "StandTrajectoryBatch.GetBytesPerRecord(noEquipmentProductivity)");
            Assert.IsTrue(minimumBytesPerStandTrajectoryRecord == 20, "StandTrajectoryBatch.GetBytesPerRecord(minimum)");

            Assert.IsTrue(maximumTrajectoryArrow.RecordBatches.Count == 0, $"maximumTrajectoryArrowMemory.RecordBatches.Count = {maximumTrajectoryArrow.RecordBatches.Count}.");
            Assert.IsTrue(maximumTrajectoryArrow.RecordCount == 0, $"maximumTrajectoryArrowMemory.RecordCount = {maximumTrajectoryArrow.RecordCount}.");
            Assert.IsTrue(maximumTrajectoryArrow.Schema.FieldsList.Count == 143, $"maximumTrajectoryArrowMemory.Schema.FieldsList.Count = {maximumTrajectoryArrow.Schema.FieldsList.Count}.");
            Assert.IsTrue(maximumTrajectoryArrow.Schema.Metadata.Count == maximumTrajectoryArrow.Schema.FieldsList.Count, $"maximumTrajectoryArrowMemory.Schema.Metadata.Count = {maximumTrajectoryArrow.Schema.Metadata.Count} does not match the number of fields in the schema ({maximumTrajectoryArrow.Schema.FieldsList.Count}).");
            Assert.IsTrue(maximumTrajectoryArrow.TotalNumberOfRecords == 1, $"maximumTrajectoryArrowMemory.TotalNumberOfRecords = {maximumTrajectoryArrow.TotalNumberOfRecords}.");

            // trees
            // Not currently anything in TreeListArrowMemory that's easily tested at a low level as constructors require stand trajectories.
        }

        [TestMethod]
        public void BreadthFirstEnumeration()
        {
            // 1D enumerations
            // basic 1-5 element arrays
            object object1 = new();
            BreadthFirstEnumerator<object> enumerator1D = new([ object1 ], origin: 0);
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object1));
            Assert.IsTrue(enumerator1D.MoveNext() == false);

            object object2 = new();
            enumerator1D = new([ object1, object2 ], origin: 0);
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object1));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object2));
            Assert.IsTrue(enumerator1D.MoveNext() == false);

            enumerator1D = new([ object1, object2 ], origin: 1);
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object2));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object1));
            Assert.IsTrue(enumerator1D.MoveNext() == false);

            object object3 = new();
            enumerator1D = new([ object1, object2, object3 ], origin: 1);
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object2));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object3));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object1));
            Assert.IsTrue(enumerator1D.MoveNext() == false);

            object object4 = new();
            enumerator1D = new([ object1, object2, object3, object4 ], origin: 3);
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object4));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object3));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object2));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object1));
            Assert.IsTrue(enumerator1D.MoveNext() == false);

            object object5 = new();
            enumerator1D = new([ object1, object2, object3, object4, object5 ], origin: 1);
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object2));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object3));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object1));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object4));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object5));
            Assert.IsTrue(enumerator1D.MoveNext() == false);

            // null skipping
            enumerator1D = new([ null, null, null ], 0);
            Assert.IsTrue(enumerator1D.MoveNext() == false);

            enumerator1D = new([ object1, null, null, object2, object3, null, object4, null, object5, null ], origin: 2);
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object2));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object3));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object1));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object4));
            Assert.IsTrue(enumerator1D.MoveNext() == true);
            Assert.IsTrue(Object.ReferenceEquals(enumerator1D.Current, object5));
            Assert.IsTrue(enumerator1D.MoveNext() == false);

            // 2D enumerations
            BreadthFirstEnumerator2D<object> enumerator2D = new([ [ object1 ] ], 0, 0);
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object1));
            Assert.IsTrue(enumerator2D.MoveNext() == false);

            enumerator2D = new([ [ object1, object2 ],
                                 [ object3, object4 ] ],
                               originX: 0, originY: 0); // balanced traversal, top left
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object1));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object3));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object2));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object4));
            Assert.IsTrue(enumerator2D.MoveNext() == false);

            enumerator2D = new([ [ object1, object2 ],
                                 [ object3, object4 ] ],
                               originX: 1, originY: 1); // balanced traversal, bottom right
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object4));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object2));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object3));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object1));
            Assert.IsTrue(enumerator2D.MoveNext() == false);

            enumerator2D = new([ [ object1, object2 ],
                                 [ object3, object4 ] ],
                               originX: 1, originY: 0); // balanced traversal, bottom left
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object3));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object1));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object4));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object2));
            Assert.IsTrue(enumerator2D.MoveNext() == false);

            enumerator2D = new([ [ object1, object2 ],
                                 [ object3, object4 ] ],
                               originX: 0, originY: 1); // balanced traversal, top right
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object2));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object4));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object1));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object3));
            Assert.IsTrue(enumerator2D.MoveNext() == false);

            object object6 = new();
            object object7 = new();
            object object8 = new();
            enumerator2D = new([ [ object1, object2, object3 ],
                                 [ object4, null, object5 ],
                                 [ object6, object7, object8 ] ],
                               originX: 1, originY: 1);
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object7));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object2));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object5));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object4));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object8));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object1));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object6));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object3));
            Assert.IsTrue(enumerator2D.MoveNext() == false);

            enumerator2D = new([ [ object1, object2, object3 ],
                                 [ object4, object5, object6 ],
                                 [ null, object7, object8 ] ],
                               originX: 0, originY: 1, BreadthFirstEnumerator2D.XFirst);
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object2));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object5));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object7));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object3));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object1));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object6));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object4));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object8));
            Assert.IsTrue(enumerator2D.MoveNext() == false);

            enumerator2D = new([ [ object1, object2, object3 ],
                                 [ object4, object5, object6 ],
                                 [ object7, null, object8 ] ],
                               originX: 2, originY: 1, BreadthFirstEnumerator2D.YFirst);
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object8));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object7));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object5));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object6));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object4));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object2));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object3));
            Assert.IsTrue(enumerator2D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator2D.Current, object1));
            Assert.IsTrue(enumerator2D.MoveNext() == false);

            // 3D enumerations
            BreadthFirstEnumerator3D<object> enumerator3D = new([ [ [ object1 ] ] ], 0, 0, 0);
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object1));
            Assert.IsTrue(enumerator3D.MoveNext() == false);

            enumerator3D = new([ [ [ object1, object2 ],
                                   [ object3, object4 ] ] ],
                               originX: 0, originY: 1, originZ: 0);
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object3));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object4));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object1));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object2));
            Assert.IsTrue(enumerator3D.MoveNext() == false);

            enumerator3D = new([ [ [ object1, object2 ],
                                   [ object3, object4 ] ],
                                 [ [ object5, object6 ],
                                   [ object7, object8 ] ] ],
                               originX: 1, originY: 1, originZ: 0);
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object7));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object8));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object5));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object3));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object6));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object4));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object1));
            Assert.IsTrue(enumerator3D.MoveNext());
            Assert.IsTrue(Object.ReferenceEquals(enumerator3D.Current, object2));
            Assert.IsTrue(enumerator3D.MoveNext() == false);
        }

        [TestMethod]
        public void FinancialScenarios()
        {
            FinancialScenarios financialScenarios = new();
            financialScenarios.Read("financial scenarios.xlsx", "parameterization");
            int expectedCount = 3;

            // properties of FinancialScenario
            Assert.IsTrue(financialScenarios.Count == expectedCount);
            Assert.IsTrue(financialScenarios.DiscountRate.Count == expectedCount);
            Assert.IsTrue(financialScenarios.DouglasFir2SawPondValuePerMbf.Count == expectedCount);
            Assert.IsTrue(financialScenarios.DouglasFir3SawPondValuePerMbf.Count == expectedCount);
            Assert.IsTrue(financialScenarios.DouglasFir4SawPondValuePerMbf.Count == expectedCount);
            Assert.IsTrue(financialScenarios.HarvestSystems.Count == expectedCount);
            Assert.IsTrue(financialScenarios.HarvestTaxPerMbf.Count == expectedCount);
            Assert.IsTrue(financialScenarios.Name.Count == expectedCount);
            Assert.IsTrue(financialScenarios.PropertyTaxAndManagementPerHectareYear.Count == expectedCount);
            Assert.IsTrue(financialScenarios.RedAlder2SawPondValuePerMbf.Count == expectedCount);
            Assert.IsTrue(financialScenarios.RedAlder3SawPondValuePerMbf.Count == expectedCount);
            Assert.IsTrue(financialScenarios.RedAlder4SawPondValuePerMbf.Count == expectedCount);
            Assert.IsTrue(financialScenarios.RegenerationHarvestCostPerHectare.Count == expectedCount);
            Assert.IsTrue(financialScenarios.ReleaseSprayCostPerHectare.Count == expectedCount);
            Assert.IsTrue(financialScenarios.SeedlingCost.Count == expectedCount);
            Assert.IsTrue(financialScenarios.SitePrepAndReplantingCostPerHectare.Count == expectedCount);
            Assert.IsTrue(financialScenarios.ThinningHarvestCostPerHectare.Count == expectedCount);
            Assert.IsTrue(financialScenarios.ShortLogPondValueMultiplier.Count == expectedCount);
            Assert.IsTrue(financialScenarios.TimberAppreciationRate.Count == expectedCount);
            Assert.IsTrue(financialScenarios.WesternRedcedarCamprunPondValuePerMbf.Count == expectedCount);
            Assert.IsTrue(financialScenarios.WhiteWood2SawPondValuePerMbf.Count == expectedCount);
            Assert.IsTrue(financialScenarios.WhiteWood3SawPondValuePerMbf.Count == expectedCount);
            Assert.IsTrue(financialScenarios.WhiteWood4SawPondValuePerMbf.Count == expectedCount);

            for (int financialIndex = 0; financialIndex < expectedCount; ++financialIndex)
            {
                Assert.IsTrue(financialScenarios.DiscountRate[financialIndex] == Constant.Financial.DefaultAnnualDiscountRate);
                Assert.IsTrue((financialScenarios.DouglasFir2SawPondValuePerMbf[financialIndex] < 750.0F) &&
                              (financialScenarios.DouglasFir2SawPondValuePerMbf[financialIndex] > financialScenarios.DouglasFir3SawPondValuePerMbf[financialIndex]));
                Assert.IsTrue((financialScenarios.DouglasFir3SawPondValuePerMbf[financialIndex] <= financialScenarios.DouglasFir2SawPondValuePerMbf[financialIndex]) &&
                              (financialScenarios.DouglasFir3SawPondValuePerMbf[financialIndex] > financialScenarios.DouglasFir4SawPondValuePerMbf[financialIndex]));
                Assert.IsTrue((financialScenarios.DouglasFir4SawPondValuePerMbf[financialIndex] <= financialScenarios.DouglasFir3SawPondValuePerMbf[financialIndex]) &&
                              (financialScenarios.DouglasFir4SawPondValuePerMbf[financialIndex] > 500.0F));
                Assert.IsTrue((financialScenarios.HarvestTaxPerMbf.Count == expectedCount) &&
                              (financialScenarios.HarvestTaxPerMbf[financialIndex] == Constant.HarvestCost.OregonForestProductsHarvestTax));
                Assert.IsTrue((financialScenarios.Name.Count == expectedCount) &&
                              (String.IsNullOrWhiteSpace(financialScenarios.Name[financialIndex]) == false));
                Assert.IsTrue((financialScenarios.PropertyTaxAndManagementPerHectareYear[financialIndex] < 75.0F) &&
                              (financialScenarios.PropertyTaxAndManagementPerHectareYear[financialIndex] > 30.0F));
                Assert.IsTrue((financialScenarios.RedAlder2SawPondValuePerMbf[financialIndex] < 782.0F) &&
                              (financialScenarios.RedAlder2SawPondValuePerMbf[financialIndex] > financialScenarios.RedAlder3SawPondValuePerMbf[financialIndex]));
                Assert.IsTrue((financialScenarios.RedAlder3SawPondValuePerMbf[financialIndex] <= financialScenarios.RedAlder2SawPondValuePerMbf[financialIndex]) &&
                              (financialScenarios.RedAlder3SawPondValuePerMbf[financialIndex] > financialScenarios.RedAlder4SawPondValuePerMbf[financialIndex]));
                Assert.IsTrue((financialScenarios.RedAlder4SawPondValuePerMbf[financialIndex] <= financialScenarios.RedAlder3SawPondValuePerMbf[financialIndex]) &&
                              (financialScenarios.RedAlder4SawPondValuePerMbf[financialIndex] > 419.0F));
                Assert.IsTrue((financialScenarios.RegenerationHarvestCostPerHectare[financialIndex] < 800.0F) &&
                              (financialScenarios.RegenerationHarvestCostPerHectare[financialIndex] > 400.0F));
                Assert.IsTrue((financialScenarios.ReleaseSprayCostPerHectare[financialIndex] < 500.0F) &&
                              (financialScenarios.ReleaseSprayCostPerHectare[financialIndex] > 200.0F));
                Assert.IsTrue((financialScenarios.SeedlingCost[financialIndex] < 2.00F) &&
                              (financialScenarios.SeedlingCost[financialIndex] > 0.25F));
                Assert.IsTrue((financialScenarios.ShortLogPondValueMultiplier[financialIndex] <= 1.00F) &&
                              (financialScenarios.ShortLogPondValueMultiplier[financialIndex] > 0.50F));
                Assert.IsTrue((financialScenarios.SitePrepAndReplantingCostPerHectare[financialIndex] < 1000.0F) &&
                              (financialScenarios.SitePrepAndReplantingCostPerHectare[financialIndex] > 500.0F));
                Assert.IsTrue((financialScenarios.ThinningHarvestCostPerHectare[financialIndex] < 500.0F) &&
                              (financialScenarios.ThinningHarvestCostPerHectare[financialIndex] > 250.0F));
                Assert.IsTrue((financialScenarios.TimberAppreciationRate[financialIndex] <= 0.03F) &&
                              (financialScenarios.TimberAppreciationRate[financialIndex] >= 0.00F));
                Assert.IsTrue((financialScenarios.WesternRedcedarCamprunPondValuePerMbf[financialIndex] < 2200.0F) &&
                              (financialScenarios.WesternRedcedarCamprunPondValuePerMbf[financialIndex] > 500.0F));
                Assert.IsTrue((financialScenarios.WhiteWood2SawPondValuePerMbf[financialIndex] < 600.0F) &&
                              (financialScenarios.WhiteWood2SawPondValuePerMbf[financialIndex] > financialScenarios.WhiteWood3SawPondValuePerMbf[financialIndex]));
                Assert.IsTrue((financialScenarios.WhiteWood3SawPondValuePerMbf[financialIndex] <= financialScenarios.WhiteWood2SawPondValuePerMbf[financialIndex]) &&
                              (financialScenarios.WhiteWood3SawPondValuePerMbf[financialIndex] > financialScenarios.WhiteWood4SawPondValuePerMbf[financialIndex]));
                Assert.IsTrue((financialScenarios.WhiteWood4SawPondValuePerMbf[financialIndex] <= financialScenarios.WhiteWood3SawPondValuePerMbf[financialIndex]) &&
                              (financialScenarios.WhiteWood4SawPondValuePerMbf[financialIndex] > 350.0F));
            }

            // regeneration harvest system
            for (int financialIndex = 0; financialIndex < expectedCount; ++financialIndex)
            {
                HarvestSystems harvestSystems = financialScenarios.HarvestSystems[financialIndex];
                Assert.IsTrue((harvestSystems.AddOnWinchCableLengthInM < 351.0F) &&
                              (harvestSystems.AddOnWinchCableLengthInM > 349.0F));
                Assert.IsTrue((harvestSystems.AnchorCostPerSMh < 75.0F) &&
                              (harvestSystems.AnchorCostPerSMh > 70.0F));
                Assert.IsTrue((harvestSystems.ChainsawBuckConstant < 100.0F) &&
                              (harvestSystems.ChainsawBuckConstant > 20.0F));
                Assert.IsTrue((harvestSystems.ChainsawBuckCostPerSMh < 125.0F) &&
                              (harvestSystems.ChainsawBuckCostPerSMh > 60.0F));
                Assert.IsTrue((harvestSystems.ChainsawBuckLinear < 100.0F) &&
                              (harvestSystems.ChainsawBuckLinear > 10.0F));
                Assert.IsTrue((harvestSystems.ChainsawBuckUtilization <= 1.0F) &&
                              (harvestSystems.ChainsawBuckUtilization > 0.5F));
                Assert.IsTrue((harvestSystems.ChainsawBuckQuadratic < 50.0F) &&
                              (harvestSystems.ChainsawBuckQuadratic > 10.0F));
                Assert.IsTrue((harvestSystems.ChainsawBuckQuadraticThreshold < 2.0F) &&
                              (harvestSystems.ChainsawBuckQuadraticThreshold > 0.5F));
                Assert.IsTrue((harvestSystems.ChainsawFellAndBuckCostPerSMh < 200.0F) &&
                              (harvestSystems.ChainsawFellAndBuckCostPerSMh > 100.0F));
                Assert.IsTrue((harvestSystems.ChainsawFellAndBuckConstant < 150.0F) &&
                              (harvestSystems.ChainsawFellAndBuckConstant > 30.0F));
                Assert.IsTrue((harvestSystems.ChainsawFellAndBuckLinear < 120.0F) &&
                              (harvestSystems.ChainsawFellAndBuckLinear > 20.0F));
                Assert.IsTrue((harvestSystems.ChainsawFellAndBuckUtilization < 1.0F) &&
                              (harvestSystems.ChainsawFellAndBuckUtilization > 0.1F));
                Assert.IsTrue((harvestSystems.ChainsawSlopeLinear < 0.1F) &&
                              (harvestSystems.ChainsawSlopeLinear > 0.0F));
                Assert.IsTrue((harvestSystems.ChainsawSlopeThresholdInPercent < 70.0F) &&
                              (harvestSystems.ChainsawSlopeThresholdInPercent > 30.0F));

                Assert.IsTrue((harvestSystems.CorridorWidth > 4.0F) && // machine width + movement variability
                              (harvestSystems.CorridorWidth < 23.0F)); // machine reach

                Assert.IsTrue((harvestSystems.CutToLengthHaulPayloadInKg < 30000.0F) &&
                              (harvestSystems.CutToLengthHaulPayloadInKg > 28000.0F));
                Assert.IsTrue((harvestSystems.CutToLengthHaulPerSMh < 145.0F) &&
                              (harvestSystems.CutToLengthHaulPerSMh > 95.0F));
                Assert.IsTrue((harvestSystems.CutToLengthRoundtripHaulSMh < 4.5F) &&
                              (harvestSystems.CutToLengthRoundtripHaulSMh > 2.5F));

                Assert.IsTrue((harvestSystems.FellerBuncherFellingConstant < 100.0F) &&
                              (harvestSystems.FellerBuncherFellingConstant > 5.0F));
                Assert.IsTrue((harvestSystems.FellerBuncherFellingLinear < 25.0F) &&
                              (harvestSystems.FellerBuncherFellingLinear > 1.0F));
                Assert.IsTrue((harvestSystems.FellerBuncherCostPerSMh < 500.0F) &&
                              (harvestSystems.FellerBuncherCostPerSMh > 100.0F));
                Assert.IsTrue((harvestSystems.FellerBuncherSlopeLinear < 0.1F) &&
                              (harvestSystems.FellerBuncherSlopeLinear > 0.0F));
                Assert.IsTrue((harvestSystems.FellerBuncherSlopeThresholdInPercent < 65.0F) &&
                              (harvestSystems.FellerBuncherSlopeThresholdInPercent > 20.0F));

                Assert.IsTrue((harvestSystems.ForwarderCostPerSMh < 500.0F) &&
                              (harvestSystems.ForwarderCostPerSMh > 100.0F));
                Assert.IsTrue((harvestSystems.ForwarderDriveWhileLoadingLogs < 0.9F) &&
                              (harvestSystems.ForwarderDriveWhileLoadingLogs > 0.7F));
                Assert.IsTrue((harvestSystems.ForwarderEmptyWeight <= 30000.0F) &&
                              (harvestSystems.ForwarderEmptyWeight > 15000.0F));
                Assert.IsTrue((harvestSystems.ForwarderLoadMeanLogVolume < 0.7F) &&
                              (harvestSystems.ForwarderLoadMeanLogVolume > 0.5F));
                Assert.IsTrue((harvestSystems.ForwarderLoadPayload < 1.1F) &&
                              (harvestSystems.ForwarderLoadPayload > 0.9F));
                Assert.IsTrue((harvestSystems.ForwarderMaximumPayloadInKg <= 20000.0F) &&
                              (harvestSystems.ForwarderMaximumPayloadInKg > 15000.0F));
                Assert.IsTrue((harvestSystems.ForwarderSpeedInStandLoadedTethered <= harvestSystems.ForwarderSpeedInStandLoadedUntethered) &&
                              (harvestSystems.ForwarderSpeedInStandLoadedTethered > 15.0F));
                Assert.IsTrue((harvestSystems.ForwarderSpeedInStandLoadedUntethered <= harvestSystems.ForwarderSpeedOnRoad) &&
                              (harvestSystems.ForwarderSpeedInStandLoadedUntethered > 20.0F));
                Assert.IsTrue((harvestSystems.ForwarderSpeedInStandUnloadedTethered <= harvestSystems.ForwarderSpeedInStandUnloadedUntethered) &&
                              (harvestSystems.ForwarderSpeedInStandUnloadedTethered > 20.0F));
                Assert.IsTrue((harvestSystems.ForwarderSpeedInStandUnloadedUntethered <= harvestSystems.ForwarderSpeedOnRoad) &&
                              (harvestSystems.ForwarderSpeedInStandUnloadedUntethered > 25.0F));
                Assert.IsTrue((harvestSystems.ForwarderSpeedOnRoad < 100.0F) &&
                              (harvestSystems.ForwarderSpeedOnRoad >= harvestSystems.ForwarderSpeedInStandUnloadedUntethered));
                Assert.IsTrue((harvestSystems.ForwarderTractiveForce < 250.0F) &&
                              (harvestSystems.ForwarderTractiveForce > 100.0F));
                Assert.IsTrue((harvestSystems.ForwarderUnloadLinearOneSort < harvestSystems.ForwarderUnloadLinearTwoSorts) &&
                              (harvestSystems.ForwarderUnloadLinearOneSort > 0.4F));
                Assert.IsTrue((harvestSystems.ForwarderUnloadLinearTwoSorts < harvestSystems.ForwarderUnloadLinearThreeSorts) &&
                              (harvestSystems.ForwarderUnloadLinearTwoSorts > harvestSystems.ForwarderUnloadLinearOneSort));
                Assert.IsTrue((harvestSystems.ForwarderUnloadLinearThreeSorts < 1.0F) &&
                              (harvestSystems.ForwarderUnloadLinearThreeSorts > harvestSystems.ForwarderUnloadLinearTwoSorts));
                Assert.IsTrue((harvestSystems.ForwarderUnloadMeanLogVolume < 0.6F) &&
                              (harvestSystems.ForwarderUnloadMeanLogVolume > 0.4F));
                Assert.IsTrue((harvestSystems.ForwarderUnloadPayload < 0.7F) &&
                              (harvestSystems.ForwarderUnloadPayload > 0.5F));
                Assert.IsTrue((harvestSystems.ForwarderUtilization < 1.0F) &&
                              (harvestSystems.ForwarderUtilization > 0.5F));

                Assert.IsTrue((harvestSystems.GrappleYardingConstantRegen < 500.0F) &&
                              (harvestSystems.GrappleYardingConstantRegen > 0.0F));
                Assert.IsTrue((harvestSystems.GrappleYardingConstantThin < 500.0F) &&
                              (harvestSystems.GrappleYardingConstantThin > 0.0F));
                Assert.IsTrue((harvestSystems.GrappleYardingLinearRegen < 5.0F) &&
                              (harvestSystems.GrappleYardingLinearRegen > 0.0F));
                Assert.IsTrue((harvestSystems.GrappleYardingLinearThin < 5.0F) &&
                              (harvestSystems.GrappleYardingLinearThin > 0.0F));
                Assert.IsTrue((harvestSystems.GrappleSwingYarderCostPerSMh < 500.0F) &&
                              (harvestSystems.GrappleSwingYarderCostPerSMh > 100.0F));
                Assert.IsTrue((harvestSystems.GrappleSwingYarderMaxPayload <= 8000.0F) &&
                              (harvestSystems.GrappleSwingYarderMaxPayload >= harvestSystems.GrappleSwingYarderMeanPayload));
                Assert.IsTrue((harvestSystems.GrappleSwingYarderMeanPayload <= harvestSystems.GrappleSwingYarderMaxPayload) &&
                              (harvestSystems.GrappleSwingYarderMeanPayload >= 1000.0F));
                Assert.IsTrue((harvestSystems.GrappleSwingYarderUtilization < 1.0F) &&
                              (harvestSystems.GrappleSwingYarderUtilization > 0.5F));
                Assert.IsTrue((harvestSystems.GrappleYoaderCostPerSMh < 500.0F) &&
                              (harvestSystems.GrappleYoaderCostPerSMh > 100.0F));
                Assert.IsTrue((harvestSystems.GrappleYoaderMaxPayload <= 4500.0F) &&
                              (harvestSystems.GrappleYoaderMaxPayload >= harvestSystems.GrappleYoaderMeanPayload));
                Assert.IsTrue((harvestSystems.GrappleYoaderMeanPayload <= harvestSystems.GrappleYoaderMaxPayload) &&
                              (harvestSystems.GrappleYoaderMeanPayload >= 1000.0F));
                Assert.IsTrue((harvestSystems.GrappleYoaderUtilization < 1.0F) &&
                              (harvestSystems.GrappleYoaderUtilization > 0.5F));

                Assert.IsTrue((harvestSystems.LoaderCostPerSMh < 500.0F) &&
                              (harvestSystems.LoaderCostPerSMh > 100.0F));
                Assert.IsTrue((harvestSystems.LoaderProductivity < 80000.0F) &&
                              (harvestSystems.LoaderProductivity > 20000.0F));
                Assert.IsTrue((harvestSystems.LoaderUtilization < 1.0F) &&
                              (harvestSystems.LoaderUtilization > 0.5F));

                Assert.IsTrue((harvestSystems.LongLogHaulPayloadInKg < 27000.0F) &&
                              (harvestSystems.LongLogHaulPayloadInKg > 25000.0F));
                Assert.IsTrue((harvestSystems.LongLogHaulPerSMh < 125.0F) &&
                              (harvestSystems.LongLogHaulPerSMh > 75.0F));
                Assert.IsTrue((harvestSystems.LongLogHaulRoundtripSMh < 4.5F) &&
                              (harvestSystems.LongLogHaulRoundtripSMh > 2.5F));

                Assert.IsTrue((harvestSystems.MachineMoveInOrOut < 600F) &&
                              (harvestSystems.MachineMoveInOrOut > 400F));

                Assert.IsTrue((harvestSystems.ProcessorBuckConstant < 100.0F) &&
                              (harvestSystems.ProcessorBuckConstant > 10.0F));
                Assert.IsTrue((harvestSystems.ProcessorBuckLinear < 100.0F) &&
                              (harvestSystems.ProcessorBuckLinear > 10.0F));
                Assert.IsTrue((harvestSystems.ProcessorBuckQuadratic1 < 10.0F) &&
                              (harvestSystems.ProcessorBuckQuadratic1 > 1.0F));
                Assert.IsTrue((harvestSystems.ProcessorBuckQuadratic2 < 10.0F) &&
                              (harvestSystems.ProcessorBuckQuadratic2 > 1.0F));
                Assert.IsTrue((harvestSystems.ProcessorBuckQuadraticThreshold1 < 10.0F) &&
                              (harvestSystems.ProcessorBuckQuadraticThreshold1 > 1.0F));
                Assert.IsTrue((harvestSystems.ProcessorBuckQuadraticThreshold2 < 10.0F) &&
                              (harvestSystems.ProcessorBuckQuadraticThreshold2 > 1.0F));
                Assert.IsTrue((harvestSystems.ProcessorCostPerSMh < 500.0F) &&
                              (harvestSystems.ProcessorCostPerSMh > 100.0F));
                Assert.IsTrue((harvestSystems.ProcessorUtilization < 1.0F) &&
                              (harvestSystems.ProcessorUtilization > 0.5F));

                Assert.IsTrue((harvestSystems.TrackedHarvesterCostPerSMh < 500.0F) &&
                              (harvestSystems.TrackedHarvesterCostPerSMh > 100.0F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterFellAndBuckConstant < 100.0F) &&
                              (harvestSystems.TrackedHarvesterFellAndBuckConstant > 10.0F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterFellAndBuckDiameterLimit < 90.0F) &&
                              (harvestSystems.TrackedHarvesterFellAndBuckDiameterLimit > 30.0F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterFellAndBuckLinear < 100.0F) &&
                              (harvestSystems.TrackedHarvesterFellAndBuckLinear > 10.0F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterFellAndBuckQuadratic1 < 10.0F) &&
                              (harvestSystems.TrackedHarvesterFellAndBuckQuadratic1 > 1.0F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterFellAndBuckQuadratic2 < 10.0F) &&
                              (harvestSystems.TrackedHarvesterFellAndBuckQuadratic2 > 1.0F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterQuadraticThreshold1 < 5.0F) &&
                              (harvestSystems.TrackedHarvesterQuadraticThreshold1 > 0.5F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterQuadraticThreshold2 < 7.5F) &&
                              (harvestSystems.TrackedHarvesterQuadraticThreshold2 > 0.5F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterSlopeLinear < 0.1F) &&
                              (harvestSystems.TrackedHarvesterSlopeLinear > 0.0F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterSlopeThresholdInPercent < 65.0F) &&
                              (harvestSystems.TrackedHarvesterSlopeThresholdInPercent > 20.0F));
                Assert.IsTrue((harvestSystems.TrackedHarvesterUtilization <= 1.0F) &&
                              (harvestSystems.TrackedHarvesterUtilization > 0.0F));

                Assert.IsTrue((harvestSystems.WheeledHarvesterCostPerSMh < 500.0F) &&
                              (harvestSystems.WheeledHarvesterCostPerSMh > 100.0F));
                Assert.IsTrue((harvestSystems.WheeledHarvesterFellAndBuckConstant < 100.0F) &&
                              (harvestSystems.WheeledHarvesterFellAndBuckConstant > 10.0F));
                Assert.IsTrue((harvestSystems.WheeledHarvesterFellAndBuckDiameterLimit < 90.0F) &&
                              (harvestSystems.WheeledHarvesterFellAndBuckDiameterLimit > 30.0F));
                Assert.IsTrue((harvestSystems.WheeledHarvesterFellAndBuckLinear < 100.0F) &&
                              (harvestSystems.WheeledHarvesterFellAndBuckLinear > 10.0F));
                Assert.IsTrue((harvestSystems.WheeledHarvesterFellAndBuckQuadratic < 10.0F) &&
                              (harvestSystems.WheeledHarvesterFellAndBuckQuadratic > 1.0F));
                Assert.IsTrue((harvestSystems.WheeledHarvesterQuadraticThreshold < 5.0F) &&
                              (harvestSystems.WheeledHarvesterQuadraticThreshold > 0.5F));
                Assert.IsTrue((harvestSystems.WheeledHarvesterSlopeLinear < 0.1F) &&
                              (harvestSystems.WheeledHarvesterSlopeLinear > 0.0F));
                Assert.IsTrue((harvestSystems.WheeledHarvesterSlopeThresholdInPercent < 65.0F) &&
                              (harvestSystems.WheeledHarvesterSlopeThresholdInPercent > 20.0F));
            }
        }

        [TestMethod]
        public void Native()
        {
            ProcessorPowerInformation powerInfo = NativeMethods.CallNtPowerInformation();
            int highestMaxMHz = Int32.MinValue;
            for (int thread = 0; thread < powerInfo.Threads; ++thread)
            {
                int currentIdleState = powerInfo.CurrentIdleState[thread];
                int currentMHz = powerInfo.CurrentMHz[thread];
                int maxMHz = powerInfo.MaxMHz[thread];
                int mhzLimit = powerInfo.MHzLimit[thread];
                int maxIdleState = powerInfo.MaxIdleState[thread];

                Assert.IsTrue((currentIdleState >= 0) && (currentIdleState <= maxIdleState));
                Assert.IsTrue((currentMHz >= 0) && (currentMHz <= maxMHz));
                Assert.IsTrue((maxMHz >= 1.5 * 1000) && (maxMHz <= 7.5 * 1000)); // 1.5-7.5 GHz: admit 1.6 GHz laptops to 5+ GHz desktops
                Assert.IsTrue((mhzLimit >= currentMHz) && (mhzLimit <= maxMHz));
                Assert.IsTrue((maxIdleState >= 0) && (maxIdleState < 4));

                if (highestMaxMHz < maxMHz)
                {
                    highestMaxMHz = maxMHz;
                }
            }

            float referenceGHz = powerInfo.GetPerformanceFrequencyInGHz();
            Assert.IsTrue((referenceGHz > 0.0009991F * highestMaxMHz) && (referenceGHz < 0.001001F * highestMaxMHz));
        }

        [TestMethod]
        public void RemoveZeroExpansionFactorTrees()
        {
            int treeCount = 147;
            Trees trees = new(FiaCode.PseudotsugaMenziesii, treeCount, Units.Metric);
            for (int treeIndex = 0; treeIndex < treeCount; ++treeIndex)
            {
                float treeIndexAsFloat = (float)treeIndex;
                float crownRatio = 0.5F;
                float expansionFactor = treeIndexAsFloat;
                if (treeIndex % 2 == 0)
                {
                    crownRatio = 0.01F;
                    expansionFactor = 0.0F;
                }
                trees.Add(1, treeIndex, treeIndexAsFloat, treeIndexAsFloat, crownRatio, expansionFactor, TreeConditionCode.Live);
                trees.DbhGrowth[treeIndex] = treeIndexAsFloat;
                trees.DeadExpansionFactor[treeIndex] = treeIndexAsFloat;
                trees.HeightGrowth[treeIndex] = treeIndexAsFloat;
            }
            Assert.IsTrue(trees.Count == treeCount);

            trees.RemoveZeroExpansionFactorTrees();
            Assert.IsTrue(trees.Count == treeCount / 2);
            for (int treeIndex = 0; treeIndex < trees.Count; ++treeIndex)
            {
                int tag = 2 * treeIndex + 1;
                float tagAsFloat = (float)tag;
                Assert.IsTrue(trees.CrownRatio[treeIndex] == 0.5F);
                Assert.IsTrue(trees.Dbh[treeIndex] == tagAsFloat);
                Assert.IsTrue(trees.DbhGrowth[treeIndex] == tagAsFloat);
                Assert.IsTrue(trees.DeadExpansionFactor[treeIndex] == tagAsFloat);
                Assert.IsTrue(trees.Height[treeIndex] == tagAsFloat);
                Assert.IsTrue(trees.HeightGrowth[treeIndex] == tagAsFloat);
                Assert.IsTrue(trees.LiveExpansionFactor[treeIndex] == tagAsFloat);
            }
            for (int treeIndex = trees.Count; treeIndex < trees.Capacity; ++treeIndex)
            {
                Assert.IsTrue(trees.LiveExpansionFactor[treeIndex] == 0.0F);
            }
        }
    }
}
