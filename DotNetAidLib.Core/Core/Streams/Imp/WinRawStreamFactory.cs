using System;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace DotNetAidLib.Core.IO.Streams.Imp
{
    public class WinRawStreamFactory : RawStreamFactory
    {
        private const int FILE_ATTRIBUTE_SYSTEM = 0x4;
        private const int FILE_FLAG_SEQUENTIAL_SCAN = 0x8;

        private readonly SafeFileHandle device;

        public WinRawStreamFactory(string path)
            : base(path)
        {
            device = CreateFile(path, FileAccess.ReadWrite, FileShare.Write | FileShare.Read | FileShare.Delete,
                IntPtr.Zero, FileMode.Open, FILE_ATTRIBUTE_SYSTEM | FILE_FLAG_SEQUENTIAL_SCAN, IntPtr.Zero);
            if (device.IsInvalid)
                throw new IOException("Unable to access drive. Win32 Error Code " + Marshal.GetLastWin32Error());
        }

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(string fileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess, [MarshalAs(UnmanagedType.U4)] FileShare fileShare,
            IntPtr securityAttributes, [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition, int flags,
            IntPtr template);

        public override FileStream Open(FileAccess fileAccess)
        {
            return new FileStream(device, fileAccess);
        }

        public override void Dispose(bool disposing)
        {
            if (disposing)
                device.Dispose();
        }

        public static string GetPhysicalDevicePathFromDriveLetter(char DriveLetter)
        {
            var devs = new ManagementClass(@"Win32_Diskdrive");
            {
                var moc = devs.GetInstances();
                foreach (ManagementObject mo in moc)
                foreach (ManagementObject b in mo.GetRelated("Win32_DiskPartition"))
                foreach (var c in b.GetRelated("Win32_LogicalDisk"))
                {
                    var DevName = string.Format("{0}", c["Name"]);
                    if (DevName[0] == DriveLetter)
                        return string.Format("{0}", mo["DeviceId"]);
                }
            }
            return "";
        }
    }
}