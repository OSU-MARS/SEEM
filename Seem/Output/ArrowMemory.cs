using Apache.Arrow;
using Apache.Arrow.Types;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mars.Seem.Output
{
    public class ArrowMemory
    {
        public int MaximumBatchLength { get; private init; }
        public List<RecordBatch> RecordBatches { get; private init; }
        public int RecordCount { get; protected set; }
        public Schema Schema { get; protected init; }
        public int TotalNumberOfRecords { get; protected set; }

        protected ArrowMemory(Schema schema)
            : this(schema, (Int32.MaxValue - 1024 * 1024) / ArrowMemory.GetUncompressedBytesPerRecord(schema)) // 1MB buffer for schema, https://github.com/apache/arrow-dotnet/issues/179
        {
            // nothing to do
        }

        protected ArrowMemory(Schema schema, int maximumBatchLength)
        {
            if ((maximumBatchLength < 10 * 1000) || (maximumBatchLength > 270 * 1000 * 1000)) // sanity bounds, >270M => <8 bytes/record @ 2GB maximum batch size
            {
                throw new ArgumentOutOfRangeException(nameof(maximumBatchLength), $"Record batch size of {maximumBatchLength} is unexpectedly large or small.");
            }

            this.MaximumBatchLength = maximumBatchLength;
            this.RecordBatches = [];
            this.RecordCount = 0;
            this.Schema = schema;
            this.TotalNumberOfRecords = 0;
        }

        protected static TArray? MaybeGetArray<TArray>(string name, Schema schema, IArrowArray[] fields) where TArray : class, IArrowArray
        {
            if (schema.FieldsLookup.Contains(name))
            {
                return (TArray)fields[schema.GetFieldIndex(name)];
            }

            return null;
        }

        public int GetUncompressedBytesPerRow()
        {
            return ArrowMemory.GetUncompressedBytesPerRecord(this.Schema);
        }

        public static int GetUncompressedBytesPerRecord(Schema schema)
        {
            int fixedWidthBitsPerRow = 0;
            for (int fieldIndex = 0; fieldIndex < schema.FieldsList.Count; ++fieldIndex)
            {
                IArrowType fieldType = schema.FieldsList[fieldIndex].DataType;
                if (fieldType.IsFixedWidth == false)
                {
                    throw new NotSupportedException($"Unhandled type {fieldType.TypeId} for field {fieldType.Name}.");
                }
                FixedWidthType fixedWidthType = (FixedWidthType)fieldType;
                fixedWidthBitsPerRow += fixedWidthType.BitWidth;
            }

            return fixedWidthBitsPerRow / 8;
        }
    }
}
