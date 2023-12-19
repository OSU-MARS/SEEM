using Apache.Arrow;
using Apache.Arrow.Types;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mars.Seem.Output
{
    public class ArrowMemory
    {
        public const int DefaultMaximumRecordsPerBatch = 10 * 1000 * 1000;

        public int MaximumBatchLength { get; private init; }
        public List<RecordBatch> RecordBatches { get; private init; }
        public int RecordCount { get; protected set; }
        public Schema Schema { get; protected init; }
        public int TotalNumberOfRecords { get; protected set; }

        protected ArrowMemory(Schema schema, int maximumBatchLength)
        {
            if ((maximumBatchLength < 10 * 1000) || (maximumBatchLength > 100 * 1000 * 1000))
            {
                throw new ArgumentOutOfRangeException(nameof(maximumBatchLength), "Record batch size of " + maximumBatchLength + " is unexpectedly large or small.");
            }

            this.MaximumBatchLength = maximumBatchLength;

            this.RecordBatches = [];
            this.RecordCount = 0;
            this.Schema = schema;
        }

        // provide CopyFirstN() overloads specialized to source type
        // While CopyFirstN<TSource>() is cleaner at the member function level it's more complex for callers as often TSource
        // cannot be inferred.
        protected static void CopyFirstN(ReadOnlySpan<float> source, Memory<byte> field, int start, int count)
        {
            source[..count].CopyTo(MemoryMarshal.Cast<byte, float>(field.Span).Slice(start, count));
        }

        protected static void CopyFirstN(ReadOnlySpan<Int16> source, Memory<byte> field, int start, int count)
        {
            source[..count].CopyTo(MemoryMarshal.Cast<byte, Int16>(field.Span).Slice(start, count));
        }

        protected static void CopyFirstN(ReadOnlySpan<Int32> source, Memory<byte> field, int start, int count)
        {
            source[..count].CopyTo(MemoryMarshal.Cast<byte, Int32>(field.Span).Slice(start, count));
        }

        protected static void CopyFirstN(ReadOnlySpan<UInt16> source, Memory<byte> field, int start, int count)
        {
            source[..count].CopyTo(MemoryMarshal.Cast<byte, UInt16>(field.Span).Slice(start, count));
        }

        protected static void CopyFirstN(ReadOnlySpan<UInt32> source, Memory<byte> field, int start, int count)
        {
            source[..count].CopyTo(MemoryMarshal.Cast<byte, UInt32>(field.Span).Slice(start, count));
        }

        protected static void CopyN(ReadOnlySpan<float> source, int sourceStart, Memory<byte> field, int destinationStart, int count)
        {
            int sourceEnd = sourceStart + count;
            source[sourceStart..sourceEnd].CopyTo(MemoryMarshal.Cast<byte, float>(field.Span).Slice(destinationStart, count));
        }

        protected static void CopyN(ReadOnlySpan<Int16> source, int sourceStart, Memory<byte> field, int destinationStart, int count)
        {
            int sourceEnd = sourceStart + count;
            source[sourceStart..sourceEnd].CopyTo(MemoryMarshal.Cast<byte, Int16>(field.Span).Slice(destinationStart, count));
        }

        protected static void CopyN(ReadOnlySpan<UInt16> source, int sourceStart, Memory<byte> field, int destinationStart, int count)
        {
            int sourceEnd = sourceStart + count;
            source[sourceStart..sourceEnd].CopyTo(MemoryMarshal.Cast<byte, UInt16>(field.Span).Slice(destinationStart, count));
        }

        protected static void CopyN(ReadOnlySpan<UInt32> source, int sourceStart, Memory<byte> field, int destinationStart, int count)
        {
            int sourceEnd = sourceStart + count;
            source[sourceStart..sourceEnd].CopyTo(MemoryMarshal.Cast<byte, UInt32>(field.Span).Slice(destinationStart, count));
        }

        protected static void Fill(Memory<byte> field, IntegerType fieldType, Int32 value, int start, int count)
        {
            switch (fieldType.BitWidth)
            {
                case 8:
                    MemoryMarshal.Cast<byte, sbyte>(field.Span).Slice(start, count).Fill((sbyte)value);
                    break;
                case 16:
                    MemoryMarshal.Cast<byte, Int16>(field.Span).Slice(start, count).Fill((Int16)value);
                    break;
                case 32:
                    MemoryMarshal.Cast<byte, Int32>(field.Span).Slice(start, count).Fill(value);
                    break;
                case 64:
                    MemoryMarshal.Cast<byte, Int64>(field.Span).Slice(start, count).Fill((Int64)value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fieldType));
            }
        }

        protected static void Fill(Memory<byte> field, IntegerType fieldType, UInt32 value, int start, int count)
        {
            switch (fieldType.BitWidth)
            {
                case 8:
                    MemoryMarshal.Cast<byte, byte>(field.Span).Slice(start, count).Fill((byte)value);
                    break;
                case 16:
                    MemoryMarshal.Cast<byte, UInt16>(field.Span).Slice(start, count).Fill((UInt16)value);
                    break;
                case 32:
                    MemoryMarshal.Cast<byte, UInt32>(field.Span).Slice(start, count).Fill((UInt32)value);
                    break;
                case 64:
                    MemoryMarshal.Cast<byte, UInt64>(field.Span).Slice(start, count).Fill((UInt64)value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fieldType));
            }
        }

        protected static void Fill(Memory<byte> field, Int16 value, int start, int count)
        {
            MemoryMarshal.Cast<byte, Int16>(field.Span).Slice(start, count).Fill(value);
        }

        protected static void Fill(Memory<byte> field, Int32 value, int start, int count)
        {
            MemoryMarshal.Cast<byte, Int32>(field.Span).Slice(start, count).Fill(value);
        }

        protected static void Fill(Memory<byte> field, UInt32 value, int start, int count)
        {
            MemoryMarshal.Cast<byte, UInt32>(field.Span).Slice(start, count).Fill(value);
        }

        /// <summary>
        /// Get position and remaining space in current batch, assuming batch length has single record granularity.
        /// </summary>
        protected (int startIndexInCurrentBatch, int recordsToCopyToCurrentBatch) GetBatchIndicesForAdd(int recordsToAdd)
        {
            int startIndexInCurrentBatch = this.RecordCount % this.MaximumBatchLength;
            int capacityRemainingInRecordBatch = this.MaximumBatchLength - startIndexInCurrentBatch;
            int recordsToCopyToCurrentBatch = Int32.Min(recordsToAdd, capacityRemainingInRecordBatch);
            return (startIndexInCurrentBatch, recordsToCopyToCurrentBatch);
        }

        /// <summary>
        /// Get 
        /// </summary>
        protected int GetNextBatchLength()
        {
            int remainingCapacity = this.TotalNumberOfRecords - this.RecordCount;
            return Int32.Min(remainingCapacity, this.MaximumBatchLength);
        }

        public int GetUncompressedBytesPerRow()
        {
            int fixedWidthBitsPerRow = 0;
            for (int fieldIndex = 0; fieldIndex < this.Schema.FieldsList.Count; ++fieldIndex)
            {
                IArrowType fieldType = this.Schema.FieldsList[fieldIndex].DataType;
                if (fieldType.IsFixedWidth == false)
                {
                    throw new NotSupportedException("Unhandled type " + fieldType.TypeId + " for field " + fieldType.Name + ".");
                }
                FixedWidthType fixedWidthType = (FixedWidthType)fieldType;
                fixedWidthBitsPerRow += fixedWidthType.BitWidth;
            }

            return fixedWidthBitsPerRow / 8;
        }
    }
}
