using System;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using DotNetAidLib.Core.IO.Streams;

namespace DotNetAidLib.Core.IO.Streams.Imp
{
    public class WinRawStreamFactory:RawStreamFactory
    {
        private const int FILE_ATTRIBUTE_SYSTEM = 0x4;
        private const int FILE_FLAG_SEQUENTIAL_SCAN = 0x8;

        private SafeFileHandle device;

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(string fileName, [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess, [MarshalAs(UnmanagedType.U4)] FileShare fileShare, IntPtr securityAttributes, [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition, int flags, IntPtr template);

        public WinRawStreamFactory(String path)
            :base(path){
            this.device = CreateFile(path, FileAccess.ReadWrite, FileShare.Write | FileShare.Read | FileShare.Delete, IntPtr.Zero, FileMode.Open, FILE_ATTRIBUTE_SYSTEM | FILE_FLAG_SEQUENTIAL_SCAN, IntPtr.Zero);
            if (device.IsInvalid)
                throw new IOException("Unable to access drive. Win32 Error Code " + Marshal.GetLastWin32Error());
        }

        public override FileStream Open(FileAccess fileAccess){
            return new FileStream(device, fileAccess);
        }

        public override void Dispose(bool disposing) {
            if(disposing)
                device.Dispose();
        }

        public static string GetPhysicalDevicePathFromDriveLetter(char DriveLetter)
        {
            ManagementClass devs = new ManagementClass(@"Win32_Diskdrive");
            {
                ManagementObjectCollection moc = devs.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    foreach (ManagementObject b in mo.GetRelated("Win32_DiskPartition"))
                    {
                        foreach (ManagementBaseObject c in b.GetRelated("Win32_LogicalDisk"))
                        {
                            string DevName = string.Format("{0}", c["Name"]);
                            if (DevName[0] == DriveLetter)
                                return string.Format("{0}", mo["DeviceId"]);
                        }
                    }
                }
            }
            return "";
        }
    }
}
