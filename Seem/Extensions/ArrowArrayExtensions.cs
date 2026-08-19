using Apache.Arrow;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Mars.Seem.Extensions
{
    public static class ArrowArrayExtensions
    {
        public static UInt8Array AsArrowArray(this byte[] data)
        {
            return new UInt8Array(new ArrowBuffer(data), ArrowBuffer.Empty, data.Length, 0, 0);
        }

        public static UInt8Array AsArrowArray(this ChainsawCrewType[] data)
        {
            Debug.Assert(sizeof(ChainsawCrewType) == sizeof(byte));
            ReinterpretingMemoryManager<ChainsawCrewType, byte> reinterpretedData = new(data.AsMemory());
            return new UInt8Array(new ArrowBuffer(reinterpretedData.Memory), ArrowBuffer.Empty, data.Length, 0, 0);
        }

        public static DoubleArray AsArrowArray(this double[] data)
        {
            ReinterpretingMemoryManager<double, byte> reinterpretedData = new(data.AsMemory());
            return new DoubleArray(new ArrowBuffer(reinterpretedData.Memory), ArrowBuffer.Empty, data.Length, 0, 0);
        }

        public static UInt16Array AsArrowArray(this FiaCode[] data)
        {
            Debug.Assert(sizeof(FiaCode) == sizeof(UInt16));
            ReinterpretingMemoryManager<FiaCode, byte> reinterpretedData = new(data.AsMemory());
            return new UInt16Array(new ArrowBuffer(reinterpretedData.Memory), ArrowBuffer.Empty, data.Length, 0, 0);
        }

        public static FloatArray AsArrowArray(this float[] data)
        {
            ReinterpretingMemoryManager<float, byte> reinterpretedData = new(data.AsMemory());
            return new FloatArray(new ArrowBuffer(reinterpretedData.Memory), ArrowBuffer.Empty, data.Length, 0, 0);
        }

        public static UInt8Array AsArrowArray(this ForwarderLoadingMethod[] data)
        {
            Debug.Assert(sizeof(ForwarderLoadingMethod) == sizeof(byte));
            ReinterpretingMemoryManager<ForwarderLoadingMethod, byte> reinterpretedData = new(data.AsMemory());
            return new UInt8Array(new ArrowBuffer(reinterpretedData.Memory), ArrowBuffer.Empty, data.Length, 0, 0);
        }

        public static UInt8Array AsArrowArray(this HarvestSystemEquipment[] data)
        {
            Debug.Assert(sizeof(HarvestSystemEquipment) == sizeof(byte));
            ReinterpretingMemoryManager<HarvestSystemEquipment, byte> reinterpretedData = new(data.AsMemory());
            return new UInt8Array(new ArrowBuffer(reinterpretedData.Memory), ArrowBuffer.Empty, data.Length, 0, 0);
        }

        public static Int16Array AsArrowArray(this Int16[] data)
        {
            ReinterpretingMemoryManager<Int16, byte> reinterpretedData = new(data.AsMemory());
            return new Int16Array(new ArrowBuffer(reinterpretedData.Memory), ArrowBuffer.Empty, data.Length, 0, 0);
        }

        public static Int32Array AsArrowArray(this Int32[] data)
        {
            ReinterpretingMemoryManager<Int32, byte> reinterpretedData = new(data.AsMemory());
            return new Int32Array(new ArrowBuffer(reinterpretedData.Memory), ArrowBuffer.Empty, data.Length, 0, 0);
        }

        public static Int64Array AsArrowArray(this Int64[] data)
        {
            ReinterpretingMemoryManager<Int64, byte> reinterpretedData = new(data.AsMemory());
            return new Int64Array(new ArrowBuffer(reinterpretedData.Memory), ArrowBuffer.Empty, data.Length, 0, 0);
        }

        public static Int8Array AsArrowArray(this sbyte[] data)
        {
            ReinterpretingMemoryManager<sbyte, byte> reinterpretedData = new(data.AsMemory());
            return new Int8Array(new ArrowBuffer(reinterpretedData.Memory), ArrowBuffer.Empty, data.Length, 0, 0);
        }

        public static UInt16Array AsArrowArray(this UInt16[] data)
        {
            ReinterpretingMemoryManager<UInt16, byte> reinterpretedData = new(data.AsMemory());
            return new UInt16Array(new ArrowBuffer(reinterpretedData.Memory), ArrowBuffer.Empty, data.Length, 0, 0);
        }

        public static UInt32Array AsArrowArray(this UInt32[] data)
        {
            ReinterpretingMemoryManager<UInt32, byte> reinterpretedData = new(data.AsMemory());
            return new UInt32Array(new ArrowBuffer(reinterpretedData.Memory), ArrowBuffer.Empty, data.Length, 0, 0);
        }

        public static UInt64Array AsArrowArray(this UInt64[] data)
        {
            ReinterpretingMemoryManager<UInt64, byte> reinterpretedData = new(data.AsMemory());
            return new UInt64Array(new ArrowBuffer(reinterpretedData.Memory), ArrowBuffer.Empty, data.Length, 0, 0);
        }

        // Apache 12.0 does not support replacement dictionaries from C#, preventing string table implementation
        // As of 9.0, it appears the current state of support is the necessary C# classes exist but the dictionary batch required to
        // accompany the record batch is silently not written in feather files (https://arrow.apache.org/docs/status.html#ipc-format,
        // https://arrow.apache.org/docs/format/Columnar.html). The result is that, while writes from C# appear successful, reads in R
        // fail with Key error: Dictionary with id 1 not found.
        // See also https://github.com/apache/arrow/blob/master/csharp/src/Apache.Arrow/Ipc/ArrowStreamWriter.cs WriteDictionary(Field)
        //public static readonly DictionaryType StringTable256Type = new(Int32Type.Default, StringType.Default, false);
        // 
        //public static DictionaryArray MakeDictionaryColumn(Memory<byte> indicies, IList<string> values)
        //{
        //    StringArray.Builder valueArray = new();
        //    for (int valueIndex = 0; valueIndex < values.Count; ++valueIndex)
        //    {
        //        valueArray.Append(values[valueIndex]);
        //    }

        //    Int32Array indexArray = new(ArrowArrayExtensions.WrapInArrayData(UInt8Type.Default, indicies, indicies.Length));
        //    return new DictionaryArray(new(UInt8Type.Default, StringType.Default, false), indexArray, valueArray.Build());
        //}

        // work around C#'s lack of reinterpret_cast and Arrow's lack reinterpreting array/ArrowBuffer constructors
        // From https://stackoverflow.com/questions/54511330/how-can-i-cast-memoryt-to-another.
        private class ReinterpretingMemoryManager<TFrom, TTo> : MemoryManager<TTo>
            where TFrom : unmanaged
            where TTo : unmanaged
        {
            private readonly Memory<TFrom> from;

            public ReinterpretingMemoryManager(Memory<TFrom> from)
            {
                this.from = from;
            }

            public override Span<TTo> GetSpan()
            {
                return MemoryMarshal.Cast<TFrom, TTo>(from.Span);
            }

            protected override void Dispose(bool _)
            {
                // nothing to dispose
            }

            public override MemoryHandle Pin(int _)
            {
                throw new NotSupportedException();
            }

            public override void Unpin()
            {
                throw new NotSupportedException();
            }
        }
    }
}
