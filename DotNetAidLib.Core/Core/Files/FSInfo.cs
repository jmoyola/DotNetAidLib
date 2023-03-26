using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.Files
{
    public class FSInfo : FileSystemInfo
    {
        private const uint FILE_READ_EA = 0x0008;
        private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x2000000;

        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public FSInfo(string path)
        {
            FullPath = path;
        }

        public bool IsValid
        {
            get
            {
                try
                {
                    var fs = new FileInfo(FullName);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool IsSymbolicLink
        {
            get
            {
                if (Exists)
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        return Attributes.HasFlag(FileAttributes.ReparsePoint);
                    else
                        // borrado por problemas con mono.unix en net standard v2
                        // return UnixFileSystemInfo.GetFileSystemEntry(this.FullName).IsSymbolicLink;
                        return !EnvironmentHelper.SearchInPath("readlink", true)
                            .CmdExecuteSync("-fn " + FullName).Equals(FullName);
                return false;
            }
        }

        public FSInfo FSParent
        {
            get
            {
                var ret = Path.GetDirectoryName(FullName);
                if (string.IsNullOrEmpty(ret))
                    return null;
                return new FSInfo(ret);
            }
        }

        public override bool Exists => File.Exists(FullName) || Directory.Exists(FullName);

        public override string Name => Path.GetFileName(FullName);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint GetFinalPathNameByHandle(IntPtr hFile,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszFilePath, uint cchFilePath, uint dwFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string filename,
            [MarshalAs(UnmanagedType.U4)] uint access,
            [MarshalAs(UnmanagedType.U4)] FileShare share,
            IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] uint flagsAndAttributes,
            IntPtr templateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        public string GetFinalPathName(string path)
        {
            var h = CreateFile(path,
                FILE_READ_EA,
                FileShare.ReadWrite | FileShare.Delete,
                IntPtr.Zero,
                FileMode.Open,
                FILE_FLAG_BACKUP_SEMANTICS,
                IntPtr.Zero);
            if (h == INVALID_HANDLE_VALUE)
                throw new Win32Exception();

            try
            {
                var sb = new StringBuilder(1024);
                var res = GetFinalPathNameByHandle(h, sb, 1024, 0);
                if (res == 0)
                    throw new Win32Exception();

                return sb.ToString();
            }
            finally
            {
                CloseHandle(h);
            }
        }

        public FSInfo GetTarget()
        {
            if (!IsSymbolicLink)
                return null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new FSInfo(GetFinalPathName(FullName));
            // borrado por problemas con mono.unix en net standard v2
            // return new FSInfo(UnixPath.GetRealPath(this.FullName));
            return new FSInfo(EnvironmentHelper.SearchInPath("readlink", true)
                .CmdExecuteSync("-fn " + FullName));
        }

        public IEnumerable<FSInfo> Content()
        {
            var ret = new List<FSInfo>();
            if (this.IsDirectory())
            {
                ret.AddRange(new DirectoryInfo(FullName).GetDirectories().Select(v => new FSInfo(v.FullName)));
                ret.AddRange(new DirectoryInfo(FullName).GetFiles().Select(v => new FSInfo(v.FullName)));
            }

            return ret;
        }

        public IEnumerable<FSInfo> Content(string pattern)
        {
            var ret = new List<FSInfo>();
            if (this.IsDirectory())
            {
                ret.AddRange(new DirectoryInfo(FullName).GetDirectories(pattern).Select(v => new FSInfo(v.FullName)));
                ret.AddRange(new DirectoryInfo(FullName).GetFiles(pattern).Select(v => new FSInfo(v.FullName)));
            }

            return ret;
        }

        public long Length()
        {
            if (!Exists)
                return -1;

            if (this.IsDirectory())
                return 0;
            return new FileInfo(FullName).Length;
        }

        public override void Delete()
        {
            if (Exists)
                if (this.IsDirectory())
                    Directory.Delete(FullName, true);
                else
                    File.Delete(FullName);
        }

        public void MoveTo(string destinationPath)
        {
            if (Exists)
                if (this.IsDirectory())
                    Directory.Move(FullName, destinationPath);
                else
                    File.Move(FullName, destinationPath);
        }

        public void CopyTo(string destinationPath, bool overwrite = false, int depth = int.MaxValue)
        {
            if (Exists)
                if (this.IsDirectory())
                    new DirectoryInfo(FullName).CopyTo(destinationPath, depth);
                else
                    File.Copy(FullName, destinationPath, overwrite);
        }

        public void CreateLink(string linkPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                CreateHardLink(linkPath, FullName, IntPtr.Zero);
            else
                // borrado por problemas con mono.unix en net standard v2
                //UnixFileSystemInfo.GetFileSystemEntry(this.FullName).CreateLink(linkPath);
                EnvironmentHelper.SearchInPath("ln", true)
                    .CmdExecuteSync(FullName + " " + linkPath);
        }

        [DllImport("kernel32.dll", EntryPoint = "CreateSymbolicLinkW", CharSet = CharSet.Unicode)]
        private static extern int CreateSymbolicLink(
            [MarshalAs(UnmanagedType.LPTStr)] string lpSymlinkFileName,
            [MarshalAs(UnmanagedType.LPTStr)] string lpTargetFileName,
            int isDirectory);

        [DllImport("kernel32.dll", EntryPoint = "CreateHardLinkW", CharSet = CharSet.Unicode)]
        private static extern bool CreateHardLink(
            [MarshalAs(UnmanagedType.LPTStr)] string lpFileName,
            [MarshalAs(UnmanagedType.LPTStr)] string lpExistingFileName,
            IntPtr mustBeNull);

        public void CreateSymbolicLink(string linkPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                CreateSymbolicLink(linkPath, FullName, this.IsFile() ? 0 : 1);
            else
                // borrado por problemas con mono.unix en net standard v2
                // UnixFileSystemInfo.GetFileSystemEntry(this.FullName).CreateSymbolicLink(linkPath);
                EnvironmentHelper.SearchInPath("ln", true)
                    .CmdExecuteSync("-s " + FullName + " " + linkPath);
        }

        public void CreateDirectory()
        {
            Directory.CreateDirectory(FullPath);
        }

        public FileStream CreateFile()
        {
            return File.Create(FullPath);
        }

        public StreamWriter CreateText()
        {
            return File.CreateText(FullPath);
        }

        public FileStream OpenFile(FileMode fileMode)
        {
            return File.Open(FullPath, fileMode);
        }

        public StreamReader OpenText()
        {
            return File.OpenText(FullPath);
        }

        public new void Refresh()
        {
            base.Refresh();
        }

        public override string ToString()
        {
            return FullName;
        }

        public static bool TryParse(string path, out FSInfo fs)
        {
            var ret = false;
            fs = new FSInfo(path);
            ret = fs.IsValid;
            if (!ret)
                fs = null;
            return ret;
        }

        public static FSInfo Parse(string path)
        {
            var ret = new FSInfo(path);
            if (!ret.IsValid)
                throw new IOException("Path '" + path + "' is not valid.");

            return ret;
        }
    }
}