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
        public FSInfo (String path)
        {
            this.FullPath = path;
        }

        public bool IsValid
        {
            get
            {
                try
                {
                    FileInfo fs = new FileInfo(this.FullName);
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
                if (this.Exists)
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        return this.Attributes.HasFlag(FileAttributes.ReparsePoint);
                    else
                        // borrado por problemas con mono.unix en net standard v2
                        // return UnixFileSystemInfo.GetFileSystemEntry(this.FullName).IsSymbolicLink;
                        return !EnvironmentHelper.SearchInPath("readlink", true)
                            .CmdExecuteSync("-fn " + this.FullName).Equals(this.FullName);
                return false;
            }
        }

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint GetFinalPathNameByHandle(IntPtr hFile, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszFilePath, uint cchFilePath, uint dwFlags);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string filename,
            [MarshalAs(UnmanagedType.U4)] uint access,
            [MarshalAs(UnmanagedType.U4)] FileShare share,
            IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] uint flagsAndAttributes,
            IntPtr templateFile);
        
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);
        
        private const uint FILE_READ_EA = 0x0008;
        private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x2000000;

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
        
        public FSInfo GetTarget() {
            if (!this.IsSymbolicLink)
                return null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new FSInfo(GetFinalPathName(this.FullName));
            else
                // borrado por problemas con mono.unix en net standard v2
                // return new FSInfo(UnixPath.GetRealPath(this.FullName));
                return new FSInfo(EnvironmentHelper.SearchInPath("readlink", true)
                    .CmdExecuteSync("-fn " + this.FullName));

                
        }

        public IEnumerable<FSInfo> Content()
        {
            List<FSInfo> ret = new List<FSInfo>();
            if (this.IsDirectory())
            {
                ret.AddRange(new DirectoryInfo(this.FullName).GetDirectories().Select(v => new FSInfo(v.FullName)));
                ret.AddRange(new DirectoryInfo(this.FullName).GetFiles().Select(v => new FSInfo(v.FullName)));
            }
            return ret;
        }

        public IEnumerable<FSInfo> Content(String pattern) {
                List<FSInfo> ret = new List<FSInfo> ();
                if (this.IsDirectory()) {
                    ret.AddRange (new DirectoryInfo (this.FullName).GetDirectories (pattern).Select (v => new FSInfo (v.FullName)));
                    ret.AddRange (new DirectoryInfo (this.FullName).GetFiles (pattern).Select (v => new FSInfo (v.FullName)));
                }
                return ret;
        }

        public FSInfo FSParent
        {
            get
            {
                String ret = Path.GetDirectoryName(this.FullName);
                if (String.IsNullOrEmpty(ret))
                    return null;
                else
                    return new FSInfo(ret);
            }
        }
        
        public override bool Exists => (File.Exists (this.FullName) || Directory.Exists (this.FullName));

        public override string Name => Path.GetFileName (this.FullName);

        public long Length()
        {
            if (!this.Exists)
                return -1;

            if (this.IsDirectory())
                return 0;
            else
                return new FileInfo(this.FullName).Length;
        }

        public override void Delete ()
        {
            if (this.Exists)
                if (this.IsDirectory())
                    Directory.Delete (this.FullName, true);
                else
                    File.Delete (this.FullName);
        }

        public void MoveTo (String destinationPath)
        {
            if (this.Exists)
                if (this.IsDirectory())
                    Directory.Move(this.FullName, destinationPath);
                else
                    File.Move(this.FullName, destinationPath);
        }

        public void CopyTo(String destinationPath, bool overwrite=false, int depth=Int32.MaxValue)
        {
            if (this.Exists)
                if (this.IsDirectory())
                    new DirectoryInfo(this.FullName).CopyTo(destinationPath, depth);
                else
                    File.Copy(this.FullName, destinationPath, overwrite);
        }

        public void CreateLink(String linkPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                CreateHardLink(linkPath, this.FullName, IntPtr.Zero);
            else
                // borrado por problemas con mono.unix en net standard v2
                //UnixFileSystemInfo.GetFileSystemEntry(this.FullName).CreateLink(linkPath);
                EnvironmentHelper.SearchInPath("ln", true)
                    .CmdExecuteSync(this.FullName + " " + linkPath);
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

        public void CreateSymbolicLink(String linkPath){
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                CreateSymbolicLink(linkPath, this.FullName, this.IsFile()?0:1);
            else
                // borrado por problemas con mono.unix en net standard v2
                // UnixFileSystemInfo.GetFileSystemEntry(this.FullName).CreateSymbolicLink(linkPath);
                EnvironmentHelper.SearchInPath("ln", true)
                .CmdExecuteSync("-s " + this.FullName + " " + linkPath);

        }

        public void CreateDirectory (){
            Directory.CreateDirectory(this.FullPath);
        }

        public FileStream CreateFile () {
            return File.Create (this.FullPath);
        }

        public StreamWriter CreateText (){
            return File.CreateText (this.FullPath);
        }

        public FileStream OpenFile (FileMode fileMode){
            return File.Open(this.FullPath, fileMode);
        }

        public StreamReader OpenText (){
            return File.OpenText (this.FullPath);
        }

        public new void Refresh () {
            base.Refresh ();
        }

        public override string ToString (){
            return this.FullName;
        }

        public static bool TryParse(String path, out FSInfo fs) {
            bool ret = false;
            fs = new FSInfo(path);
            ret=fs.IsValid;
            if (!ret)
                fs = null;
            return ret;
        }

        public static FSInfo Parse(String path)
        {
            FSInfo ret = new FSInfo(path);
            if (!ret.IsValid)
                throw new IOException("Path '" + path + "' is not valid.");

            return ret;
        }
    }
}
