using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files.Version.Core;
using DotNetAidLib.Core.Proc;
using DotNetAidLib.Core.Streams;
using DotNetAidLib.Core.Time;
using Mono.Unix;
using Mono.Unix.Native;

namespace DotNetAidLib.Core.Files
{
    public static class FilesHelper
    {
        public static bool SetPermissionToAll(this DirectoryInfo v)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var dSecurity = v.GetAccessControl();
                    dSecurity.AddAccessRule(
                        new FileSystemAccessRule(
                            new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                            FileSystemRights.FullControl,
                            InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                            PropagationFlags.NoPropagateInherit,
                            AccessControlType.Allow));
                    v.SetAccessControl(dSecurity);
                }
                else
                {
                    var ufs = new UnixFileInfo(v.FullName);
                    ufs.FileAccessPermissions = FileAccessPermissions.AllPermissions;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }


        public static void CreateAll(this DirectoryInfo v, DirectorySecurity directorySecurity)
        {
            if (v.Parent != null && !v.Parent.Exists)
                v.Parent.CreateAll(directorySecurity);

            v.Create(directorySecurity);
        }

        public static bool IsExecutable(this FileInfo v)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                && (v.Extension.Equals(".exe", StringComparison.InvariantCultureIgnoreCase)
                    || v.Extension.Equals(".com", StringComparison.InvariantCultureIgnoreCase)
                    || v.Extension.Equals(".bat", StringComparison.InvariantCultureIgnoreCase)))
            {
                return true;
            }

            UnixFileSystemInfo ufi = new UnixFileInfo(v.FullName);
            return ufi.CanAccess(AccessModes.X_OK);
        }

        public static FileSystemInfo PathToFileSystemInfo(this string v)
        {
            try
            {
                if (!(File.Exists(v) || Directory.Exists(v)))
                    return new FSInfo(v);

                if (File.GetAttributes(v).HasFlag(FileAttributes.Directory))
                    return new DirectoryInfo(v);
                return new FileInfo(v);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting FileSystemInfo form path '" + v + "'.", ex);
            }
        }

        public static bool CanRead(this DirectoryInfo v)
        {
            try
            {
                v.GetFiles();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool CanWrite(this DirectoryInfo v)
        {
            try
            {
                var tempFile = v.GetFile(DateTime.Now.ToStringISO8601(true) + ".tmp");
                tempFile.CreateText().WriteFluent("test").Close();
                tempFile.Delete();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool CanWrite(this FileInfo v)
        {
            try
            {
                File.Open(v.FullName, FileMode.Open, FileAccess.Write);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool CanRead(this FileInfo v)
        {
            try
            {
                File.Open(v.FullName, FileMode.Open, FileAccess.Read);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static FileInfo CopyTo(this FileInfo v, string destFileName, bool safeCopy = false,
            bool overwrite = false)
        {
            FileInfo aux = null;
            if (!safeCopy)
            {
                aux = CopyTo(v, destFileName, overwrite);
            }
            else
            {
                aux = v.CopyTo(destFileName + ".tmp", overwrite);
                aux.MoveTo(destFileName);
            }

            aux.Refresh();
            return aux;
        }

        public static void MoveTo(this FileInfo v, string destFileName, bool overwrite = false)
        {
            var aux = new FileInfo(destFileName);
            if (aux.Exists && overwrite)
                aux.Delete();

            v.MoveTo(aux.FullName);
            aux.Refresh();
        }

        public static T Version<T>(this FileInfo v, Func<FileInfo, T> versionParser)
        {
            Assert.NotNull(versionParser, nameof(versionParser));
            return versionParser.Invoke(v);
        }

        public static System.Version Version(this FileInfo v)
        {
            var vf = VersionFactory.Instance();
            return vf.GetVersion(v);
        }

        public static T RefreshFluent<T>(this T v) where T : FileSystemInfo
        {
            v.Refresh();
            return v;
        }

        public static void CopyTo(this DirectoryInfo v, string destination, int depth = int.MaxValue)
        {
            CopyRecursive(v, destination, 0, depth);
        }

        private static void CopyRecursive(FileSystemInfo src, string destination, int level, int depth = int.MaxValue)
        {
            if (level < depth)
            {
                if (src.IsFile())
                {
                    src.Cast<FileInfo>().CopyTo(destination);
                }
                else
                {
                    if (!Directory.Exists(destination))
                        Directory.CreateDirectory(destination);

                    foreach (var fsi in src.GetChildrens())
                        CopyRecursive(fsi, destination + Path.DirectorySeparatorChar + fsi.Name, level + 1, depth);
                }
            }
        }

        public static bool IsFile(this FileSystemInfo v)
        {
            if (v.Exists)
                return !File.GetAttributes(v.FullName).HasFlag(FileAttributes.Directory);
            return false;
        }

        public static bool IsDirectory(this FileSystemInfo v)
        {
            if (v.Exists)
                return File.GetAttributes(v.FullName).HasFlag(FileAttributes.Directory);
            return false;
        }

        public static T Cast<T>(this FileSystemInfo v) where T : FileSystemInfo
        {
            if (v is T)
                return (T) v;
            throw new InvalidCastException("FileSystemInfo is not '" + typeof(T).Name + "'");
        }

        public static IEnumerable<FileSystemInfo> GetChildrens(this FileSystemInfo v)
        {
            var ret = new List<FileSystemInfo>();
            if (v.IsDirectory())
            {
                ret.AddRange(v.Cast<DirectoryInfo>().GetDirectories());
                ret.AddRange(v.Cast<DirectoryInfo>().GetFiles());
            }

            return ret;
        }

        public static IEnumerable<FileSystemInfo> GetAll(this DirectoryInfo v, string searchPattern)
        {
            var ret = new List<FileSystemInfo>();
            ret.AddRange(v.GetDirectories(searchPattern));
            ret.AddRange(v.GetFiles(searchPattern));
            return ret;
        }

        public static DirectoryInfo GetDirectory(this DirectoryInfo v, string name)
        {
            return new DirectoryInfo(v.FullName + Path.DirectorySeparatorChar + name);
        }

        public static FileInfo GetFile(this DirectoryInfo v, string name)
        {
            return new FileInfo(v.FullName + Path.DirectorySeparatorChar + name);
        }

        public static bool EqualTo(this FileInfo v, FileInfo fileToCompare)
        {
            if (v.Length != fileToCompare.Length)
                return false;

            if (string.Equals(v.FullName, fileToCompare.FullName, StringComparison.OrdinalIgnoreCase))
                return true;

            using (var fs1 = v.OpenRead())
            using (var fs2 = fileToCompare.OpenRead())
            {
                for (var i = 0; i < v.Length; i++)
                    if (fs1.ReadByte() != fs2.ReadByte())
                        return false;
            }

            return true;
        }

        public static void WaitUntilUnlock(this FileInfo file)
        {
            file.WaitUntilUnlock(0);
        }

        public static void WaitUntilUnlock(this FileInfo file, int timeoutMs)
        {
            Assert.Exists(file, nameof(file));
            var blocked = true;
            Stream st = null;
            TimeOutWatchDog towd = null;

            if (timeoutMs > 0)
                towd = new TimeOutWatchDog(timeoutMs);

            while (blocked)
            {
                if (towd != null)
                    towd.IsTimeOut(true, true);
                try
                {
                    st = file.Open(FileMode.Open);
                    blocked = false;
                }
                catch
                {
                    blocked = true;
                }

                Thread.Sleep(1000);
            }

            st.Close();
        }

        public static FileInfo FindFile(this IEnumerable<DirectoryInfo> v, string fileName)
        {
            FileInfo ret = null;
            foreach (var d in v)
            {
                ret = d.GetFiles(fileName).FirstOrDefault(f => f.Name == fileName);
                if (ret != null)
                    break;
            }

            return ret;
        }

        public static FileInfo[] Files(this IEnumerable<DirectoryInfo> v)
        {
            return v.Files(null, SearchOption.TopDirectoryOnly);
        }

        public static FileInfo[] Files(this IEnumerable<DirectoryInfo> v, string searchPattern)
        {
            return v.Files(searchPattern, SearchOption.TopDirectoryOnly);
        }

        public static FileInfo[] Files(this IEnumerable<DirectoryInfo> v, string searchPattern,
            SearchOption searchOptions)
        {
            var ret = new List<FileInfo>();
            foreach (var d in v)
                if (searchPattern == null)
                    ret.AddRange(d.GetFiles());
                else
                    ret.AddRange(d.GetFiles(searchPattern, searchOptions));

            return ret.ToArray();
        }

        public static byte[] CalculateHash(this FileInfo v)
        {
            byte[] ret;

            HashAlgorithm hashAlgorithm = SHA1.Create();
            hashAlgorithm.Initialize();
            v.CalculateHash(hashAlgorithm);
            ret = hashAlgorithm.Hash;
            hashAlgorithm.Dispose();

            return ret;
        }

        public static HashAlgorithm CalculateHash(this FileInfo v, HashAlgorithm hashAlgorithm)
        {
            var fs = new FileStream(v.FullName, FileMode.Open);
            hashAlgorithm.ComputeHash(fs);
            fs.Close();
            return hashAlgorithm;
        }

        public static byte[] CalculateHash(this DirectoryInfo v)
        {
            byte[] ret;

            HashAlgorithm hashAlgorithm = SHA1.Create();
            hashAlgorithm.Initialize();
            v.CalculateHash(hashAlgorithm);
            ret = hashAlgorithm.Hash;
            hashAlgorithm.Dispose();

            return ret;
        }

        public static void CreateAll(this DirectoryInfo v)
        {
            if (v.Parent != null && !v.Parent.Exists)
                v.Parent.CreateAll();

            v.Create();
        }

        public static HashAlgorithm CalculateHash(this DirectoryInfo v, HashAlgorithm hashAlgorithm)
        {
            foreach (var di in v.GetDirectories())
                di.CalculateHash(hashAlgorithm);
            foreach (var fi in v.GetFiles())
                fi.CalculateHash(hashAlgorithm);
            return hashAlgorithm;
        }

        public static FileInfo RandomTempFile(this FileInfo v)
        {
            return v.RandomTempFile(".tmp");
        }

        public static FileInfo RandomTempFile(this FileInfo v, string extension)
        {
            var tempPath = Path.GetTempPath();
            if (!tempPath.EndsWith("" + Path.DirectorySeparatorChar, StringComparison.InvariantCultureIgnoreCase))
                tempPath = tempPath + Path.DirectorySeparatorChar;
            var ret = new FileInfo(tempPath + Guid.NewGuid() + extension);
            return ret;
        }

        public static FileInfo ToTempDirectory(this FileInfo v)
        {
            var tempPath = Path.GetTempPath();
            if (!tempPath.EndsWith("" + Path.DirectorySeparatorChar, StringComparison.InvariantCultureIgnoreCase))
                tempPath = tempPath + Path.DirectorySeparatorChar;
            var ret = new FileInfo(tempPath + v.Name);
            return ret;
        }

        public static DirectoryInfo RandomTempDirectory(this DirectoryInfo v)
        {
            return v.RandomTempDirectory(".tmp");
        }

        public static DirectoryInfo RandomTempDirectory(this DirectoryInfo v, string extension)
        {
            var tempPath = Path.GetTempPath();
            if (!tempPath.EndsWith("" + Path.DirectorySeparatorChar, StringComparison.InvariantCultureIgnoreCase))
                tempPath = tempPath + Path.DirectorySeparatorChar;
            var ret = new DirectoryInfo(tempPath + Guid.NewGuid() + extension);
            return ret;
        }

        public static DirectoryInfo ToTempDirectory(this DirectoryInfo v)
        {
            var tempPath = Path.GetTempPath();
            if (!tempPath.EndsWith("" + Path.DirectorySeparatorChar, StringComparison.InvariantCultureIgnoreCase))
                tempPath = tempPath + Path.DirectorySeparatorChar;
            var ret = new DirectoryInfo(tempPath + v.Name);
            return ret;
        }

        public static System.Diagnostics.Process GetCmdProcess(this FileInfo v, ref StreamWriter input)
        {
            return v.GetCmdProcess(null, ref input);
        }


        public static System.Diagnostics.Process GetCmdProcess(this FileInfo v)
        {
            return v.GetCmdProcess(null);
        }

        public static System.Diagnostics.Process GetCmdProcess(this FileInfo v, string arguments,
            ref StreamWriter input)
        {
            ProcessStartInfo psi = null;
            psi = new ProcessStartInfo(v.FullName);
            if (!string.IsNullOrEmpty(arguments))
                psi.Arguments = arguments;

            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            var ret = new System.Diagnostics.Process();
            if (input != null)
                psi.RedirectStandardInput = true;


            ret.StartInfo = psi;

            if (input != null)
                input = ret.StandardInput;

            return ret;
        }

        public static System.Diagnostics.Process GetCmdProcess(this FileInfo v, string arguments)
        {
            var psi = new ProcessStartInfo(v.FullName);

            if (!string.IsNullOrEmpty(arguments))
                psi.Arguments = arguments;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            var ret = new System.Diagnostics.Process();

            ret.StartInfo = psi;

            return ret;
        }

        public static void WriteText(this FileInfo v, string content, Encoding encoding = null,
            bool overrideIfExists = false)
        {
            if (v.RefreshFluent().Exists && !overrideIfExists)
                throw new Exception("File already exists");

            if (encoding == null)
                encoding = Encoding.Default;

            using (var fs = v.Open(FileMode.OpenOrCreate, FileAccess.Write))
            using (var sw = new StreamWriter(fs, encoding))
            {
                sw.Write(content);
            }
        }

        public static void AppendText(this FileInfo v, string content, Encoding encoding = null)
        {
            if (!v.RefreshFluent().Exists)
                throw new Exception("File don't exists");

            if (encoding == null)
                encoding = Encoding.Default;

            using (var fs = v.Open(FileMode.Append, FileAccess.Write))
            using (var sw = new StreamWriter(fs, encoding))
            {
                sw.Write(content);
            }
        }

        public static string CmdExecuteSync(this FileInfo v, string arguments,
            int timeoutMs = -1, bool ignoreExitCode = false)
        {
            System.Diagnostics.Process p = null;

            try
            {
                string ret = null;

                if (!string.IsNullOrEmpty(arguments))
                    p = v.GetCmdProcess(arguments);
                else
                    p = v.GetCmdProcess();

                p.Start();
                if (timeoutMs > -1)
                    p.WaitForExit(timeoutMs);
                else
                    p.WaitForExit();

                if (!p.HasExited)
                    p.Kill();

                var processExitCode = p.ExitCode;

                if (!ignoreExitCode && processExitCode != 0)
                    throw new Exception("Exit code non zero '" + processExitCode + "' executing command '" +
                                        v.FullName + (arguments == null ? "" : " " + arguments) + "': " +
                                        p.StandardError.ReadToEnd());

                ret = p.StandardOutput.ReadToEnd();
                p.Close();
                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                p.Dispose();
            }
        }

        public static string CmdExecuteSync(this FileInfo v)
        {
            return v.CmdExecuteSync(null);
        }

        public static System.Diagnostics.Process CmdExecuteAsync(this FileInfo v, string arguments)
        {
            System.Diagnostics.Process p;
            if (!string.IsNullOrEmpty(arguments))
                p = v.GetCmdProcess(arguments);
            else
                p = v.GetCmdProcess();
            p.Start();
            return p;
        }
    }
}