using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Mars.Seem.Extensions
{
    internal static partial class NativeMethods
    {
        private const int ProcessorInformation = 11;
        private const uint STATUS_SUCCESS = 0;

        // from https://www.pinvoke.net/default.aspx/powrprof.callntpowerinformation
        // CallNtPowerInformation() (apparently) reports only non-turbo information and therefore always returns constant values.
        public static ProcessorPowerInformation CallNtPowerInformation()
        {
            int procCount = Environment.ProcessorCount;
            PROCESSOR_POWER_INFORMATION[] nativePowerInfo = new PROCESSOR_POWER_INFORMATION[procCount];
            uint ntstatus = NativeMethods.CallNtPowerInformation(NativeMethods.ProcessorInformation,
                                                                 IntPtr.Zero,
                                                                 0,
                                                                 nativePowerInfo,
                                                                 (uint)(nativePowerInfo.Length * Marshal.SizeOf(typeof(PROCESSOR_POWER_INFORMATION))));
            if (ntstatus != STATUS_SUCCESS)
            {
                throw new Win32Exception((int)ntstatus, "P/Invoke of CallNtPowerInformation() failed.");
            }

            ProcessorPowerInformation managedPowerInfo = new(nativePowerInfo);
            return managedPowerInfo;
        }

        [LibraryImport("powrprof.dll", SetLastError = true)]
        private static partial UInt32 CallNtPowerInformation(Int32 InformationLevel,
                                                             IntPtr lpInputBuffer,
                                                             UInt32 nInputBufferSize,
                                                             /* .NET 8.0 [In, Out] */ PROCESSOR_POWER_INFORMATION[] lpOutputBuffer, // https://github.com/dotnet/runtime/issues/90785
                                                             UInt32 nOutputBufferSize);

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESSOR_POWER_INFORMATION
        {
            public uint Number;
            public uint MaxMhz;
            public uint CurrentMhz;
            public uint MhzLimit;
            public uint MaxIdleState;
            public uint CurrentIdleState;
        }
    }

    // https://github.com/dotnet/roslyn-analyzers/issues/6802
    //internal static partial class NativeMethodsNet7
    //{
    //    [LibraryImport("powrprof.dll", SetLastError = true)]
    //    private static partial UInt32 CallNtPowerInformation(Int32 InformationLevel,
    //                                                         IntPtr lpInputBuffer,
    //                                                         UInt32 nInputBufferSize,
    //                                                         [MarshalAs(UnmanagedType.LPStruct, SizeParamIndex = 4)] PROCESSOR_POWER_INFORMATION[] lpOutputBuffer,
    //                                                         UInt32 nOutputBufferSize);

    //    [StructLayout(LayoutKind.Sequential)]
    //    public struct PROCESSOR_POWER_INFORMATION
    //    {
    //        public uint Number;
    //        public uint MaxMhz;
    //        public uint CurrentMhz;
    //        public uint MhzLimit;
    //        public uint MaxIdleState;
    //        public uint CurrentIdleState;
    //    }
    //}
}
