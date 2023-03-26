using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Enums;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.OperatingSystem.Core;
using Microsoft.Win32;

namespace DotNetAidLib.OperatingSystem.Imp
{
    public class WindowsOperatingSystem : Core.OperatingSystem
    {
        public enum SymbolicLinkType
        {
            File = 0,
            Directory = 1
        }

        private const uint FILE_READ_EA = 0x0008;
        private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x2000000;

        private static WindowsOperatingSystem _Instance;

        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        private OSRunLevel actualOSRunLevel = OSRunLevel.Graphical;

        protected WindowsOperatingSystem()
        {
            try // Incompatibilidad con windows 10
            {
                SystemEvents.SessionEnding += SystemEvents_SessionEnding;
            }
            catch
            {
            }
        }

        public override OSDistribution DistributionInfo
        {
            get
            {
                var id = "UNKNOW";
                var architecture = "UNKNOW";
                var operatingSystem = "UNKNOW";
                var release = "UNKNOW";
                var revision = "";
                var codename = "UNKNOW";
                var kernel = "UNKNOW";

                var ret = new Dictionary<string, string>();

                //Get Operating system information.
                var os = Environment.OSVersion;
                //Get version information about the os.
                ret.Add("Version", os.Version.ToString());
                ret.Add("Platform", os.Platform.ToString());
                var vs = os.Version;

                if (os.Platform == PlatformID.Win32Windows)
                    //This is a pre-NT version of Windows
                    switch (vs.Minor)
                    {
                        case 0:
                            release = "95";
                            break;
                        case 10:
                            if (vs.Revision.ToString() == "2222A")
                                release = "98SE";
                            else
                                release = "98";
                            break;
                        case 90:
                            release = "Me";
                            break;
                    }
                else if (os.Platform == PlatformID.Win32NT)
                    switch (vs.Major)
                    {
                        case 3:
                            release = "NT 3.51";
                            break;
                        case 4:
                            release = "NT 4.0";
                            break;
                        case 5:
                            if (vs.Minor == 0)
                                release = "2000";
                            else
                                release = "XP";
                            break;
                        case 6:
                            if (vs.Minor == 0)
                                release = "Vista";
                            else if (vs.Minor == 1)
                                release = "7";
                            else if (vs.Minor == 2)
                                release = "8";
                            else
                                release = "8.1";
                            break;
                        case 10:
                            release = "10";
                            break;
                        case 11:
                            release = "11";
                            break;
                    }

                id = "Microsoft Windows";
                operatingSystem = "Windows";
                architecture = Environment.Is64BitOperatingSystem ? "64" : "32";
                revision = "ServicePack " + os.ServicePack;
                //codename = "";
                kernel = Environment.OSVersion.Version.ToString();

                return new OSDistribution(
                    id,
                    architecture,
                    operatingSystem,
                    release,
                    revision,
                    codename,
                    kernel
                );
            }
        }

        public override string SystemArchitecture => Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");


        public override OSRunLevel RunLevel => actualOSRunLevel;

        public override OSScheduleShutdownInfo ScheduleShutdown => null;

        public override DateTime LastBoot => new DateTime(0);

        public override IEnumerable<OSPeriod> Reboots
        {
            get { return new OSPeriod[] { }; }
        }

        public override IEnumerable<OSPeriod> Shutdowns
        {
            get { return new OSPeriod[] { }; }
        }

        public override IEnumerable<SerialPortInfo> SerialPorts =>
            WindowsSerialPortInfo.SerialPorts;

        ~WindowsOperatingSystem()
        {
            try
            {
                // Incompatibilidad con windows 10
                SystemEvents.SessionEnding -= SystemEvents_SessionEnding;
            }
            catch
            {
            }
        }

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint GetFinalPathNameByHandle(IntPtr hFile,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszFilePath, uint cchFilePath, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string filename,
            [MarshalAs(UnmanagedType.U4)] uint access,
            [MarshalAs(UnmanagedType.U4)] FileShare share,
            IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] uint flagsAndAttributes,
            IntPtr templateFile);

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

        [DllImport("kernel32.dll", EntryPoint = "CreateSymbolicLinkW", CharSet = CharSet.Unicode)]
        public static extern int CreateSymbolicLink(
            [MarshalAs(UnmanagedType.LPTStr)] string lpSymlinkFileName,
            [MarshalAs(UnmanagedType.LPTStr)] string lpTargetFileName,
            SymbolicLinkType dwFlags);

        [DllImport("kernel32.dll", EntryPoint = "CreateHardLinkW", CharSet = CharSet.Unicode)]
        public static extern bool CreateHardLink(
            [MarshalAs(UnmanagedType.LPTStr)] string lpFileName,
            [MarshalAs(UnmanagedType.LPTStr)] string lpExistingFileName,
            IntPtr mustBeNull);

        public new static WindowsOperatingSystem Instance()
        {
            if (_Instance == null)
                _Instance = new WindowsOperatingSystem();

            return _Instance;
        }

        public override void Shutdown(OSShutdownType shutdownType, TimeSpan schedule, string message)
        {
            if (schedule.Minutes < 0)
                throw new Exception("Only 0 or future time is allowed (in minutes)");

            var shutdownFileInfo = EnvironmentHelper.SearchInPath("shutdown");
            if (shutdownType == OSShutdownType.Cancel)
            {
                shutdownFileInfo.CmdExecuteSync("-a");
            }
            else
            {
                if (shutdownType == OSShutdownType.PowerOff)
                    shutdownFileInfo.CmdExecuteSync("-s" +
                                                    (schedule == null ? "" : " -t " + (int) schedule.TotalSeconds));
                else if (shutdownType == OSShutdownType.Reboot)
                    shutdownFileInfo.CmdExecuteSync("-r" +
                                                    (schedule == null ? "" : " -t " + (int) schedule.TotalSeconds));
            }
        }

        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            if (e.Reason == SessionEndReasons.SystemShutdown) actualOSRunLevel = OSRunLevel.PowerOff;
        }

        public override string ExecuteCmd(string command, string arguments, bool ignoreExitCode,
            IDictionary<string, string> environmentVariables, int timeoutMs = -1)
        {
            return Execute(EnvironmentHelper.SearchInPath("cmd.exe"), "/C " + command + " " + arguments, ignoreExitCode,
                environmentVariables, timeoutMs);
        }

        public override string Execute(FileInfo fileToExecute, string arguments, bool ignoreExitCode,
            IDictionary<string, string> environmentVariables, int timeoutMs = -1)
        {
            string ret = null;

            Process p = null;

            try
            {
                p = fileToExecute.GetCmdProcess(arguments);
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                if (environmentVariables != null)
                    foreach (var kv in environmentVariables)
                        p.StartInfo.EnvironmentVariables[kv.Key] = kv.Value;

                p.Start();

                if (timeoutMs > -1)
                    p.WaitForExit(timeoutMs);
                else
                    p.WaitForExit();

                if (!ignoreExitCode && p.ExitCode != 0)
                    throw new OperatingSystemException(p.StandardError.ReadToEnd() + " " +
                                                       p.StandardOutput.ReadToEnd());

                ret = p.StandardOutput.ReadToEnd();

                return ret;
            }
            catch (Exception ex)
            {
                throw new OperatingSystemException(
                    "Error executing command '" + fileToExecute.FullName + (arguments == null ? "" : " " + arguments) +
                    "'.", ex);
            }
            finally
            {
                p.Dispose();
            }
        }

        public override Process CreateProcess(FileInfo fileToExecute, string arguments, string userName,
            string password, string domain)
        {
            try
            {
                Assert.Exists(fileToExecute, nameof(fileToExecute));
                var p = new Process();
                var psi = new ProcessStartInfo();

                // Si no hay credenciales
                if (string.IsNullOrEmpty(userName))
                {
                    psi.FileName = fileToExecute.FullName;
                    psi.Arguments = arguments;
                }
                else
                {
                    psi.FileName = fileToExecute.FullName;
                    psi.Arguments = arguments;
                    psi.UserName = userName;
                    if (!string.IsNullOrEmpty(password))
                        psi.Password = password.ToSecureString();
                    if (!string.IsNullOrEmpty(domain))
                        psi.Domain = domain;
                }

                p.StartInfo = psi;

                return p;
            }
            catch (Exception ex)
            {
                throw new OperatingSystemException("Error creating process.", ex);
            }
        }

        public override string AdminExecuteCmd(string command, string arguments, string userName, string password,
            string domain, bool ignoreExitCode, IDictionary<string, string> environmentVariables, int timeoutMs = -1)
        {
            return AdminExecute(EnvironmentHelper.SearchInPath("cmd.exe"), "/C " + command + " " + arguments, userName,
                password, domain, ignoreExitCode, environmentVariables, timeoutMs);
        }

        public override string AdminExecute(FileInfo fileToExecute, string arguments, string userName, string password,
            string domain, bool ignoreExitCode, IDictionary<string, string> environmentVariables, int timeoutMs = -1)
        {
            string ret = null;

            Process p = null;

            try
            {
                p = CreateProcess(fileToExecute, arguments, userName, password, domain);
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                if (environmentVariables != null)
                    foreach (var kv in environmentVariables)
                        p.StartInfo.EnvironmentVariables[kv.Key] = kv.Value;

                p.Start();

                if (timeoutMs > -1)
                    p.WaitForExit(timeoutMs);
                else
                    p.WaitForExit();

                if (!ignoreExitCode && p.ExitCode != 0)
                    throw new OperatingSystemException("Error executing command '" + fileToExecute.FullName +
                                                       (arguments == null ? "" : " " + arguments) + "': " +
                                                       p.StandardError.ReadToEnd());
                ret = p.StandardOutput.ReadToEnd();
                p.StandardOutput.Close();
                return ret;
            }
            catch (Exception ex)
            {
                throw new OperatingSystemException(
                    "Error executing '" + fileToExecute.FullName + "' with admin privileges: " + ex.Message, ex);
            }
            finally
            {
                p.Dispose();
            }
        }

        public override void AdminTextWrite(FileInfo outputFile, string text, string userName, string password,
            string domain)
        {
            throw new NotImplementedException();
        }

        public override string AdminTextRead(FileInfo inputFile, string userName, string password, string domain)
        {
            throw new NotImplementedException();
        }

        public override void ChangeHostName(string newHostName)
        {
            throw new NotImplementedException();
        }

        public override void AdminCopy(string from, string to, bool recursive, bool preserveDates, bool preserveOwners,
            bool preserveRights, bool preserveOthers, bool followLinks = false)
        {
            var xcopyFile = EnvironmentHelper.SearchInPath("xcopy.exe");

            var parameters = "/H";
            parameters += recursive ? " /E" : "";
            parameters += preserveOwners || preserveRights ? " /O" : "";
            parameters += preserveOthers ? " /K /X" : "";
            parameters += " " + from + " " + to;
            AdminExecute(xcopyFile, parameters);
        }
        /*
        public IEnumerable<String> GetUsers()
        {
            var ret=GetWMIQuery("select * from Win32_UserAccount");
            foreach (var v in ret) {
                v["SID"].Value;
                v["Caption"].Value;
                v["Description"].Value;
                v["Name"].Value;
                v["FullName"].Value;
                v["Domain"].Value;
            }
        }
        */

        public override string GetUserDirectory(string user)
        {
            string ret = null;
            try
            {
                ret = Environment.GetEnvironmentVariable("%HOMEDRIVE%")
                      + Environment.GetEnvironmentVariable("%HOMEPATH%");

                return ret;
            }
            catch (Exception ex)
            {
                throw new OperatingSystemException("Error retrieving user directory.", ex);
            }
        }

        public static IEnumerable<PropertyDataCollection> GetWMIQuery(string wmiQuery)
        {
            return GetWMIQuery(wmiQuery, true);
        }

        public static IEnumerable<PropertyDataCollection> GetWMIQuery(string wmiQuery, bool debug)
        {
            return GetWMIQuery(null, wmiQuery, debug);
        }

        public static IEnumerable<ManagementObject> GetWMIQueryObjects(string scope, string wmiQuery, bool debug)
        {
            IEnumerable<ManagementObject> ret;
            ManagementObjectSearcher query = null;
            if (string.IsNullOrEmpty(scope))
                query = new ManagementObjectSearcher(wmiQuery);
            else
                query = new ManagementObjectSearcher(scope, wmiQuery);
            var moc = query.Get();
            ret = moc.Cast<ManagementObject>();
            query.Dispose();

            if (debug)
            {
                Debug.WriteLine("");
                Debug.WriteLine("WMI query '" + (scope == null ? "" : scope + "/") + wmiQuery + "' result:");
                foreach (var mo in ret) Debug.WriteLine(mo.ToString());
            }

            return ret;
        }

        public static IEnumerable<PropertyDataCollection> GetWMIQuery(string scope, string wmiQuery, bool debug)
        {
            var ret = new List<PropertyDataCollection>();
            ManagementObjectSearcher query = null;
            if (string.IsNullOrEmpty(scope))
                query = new ManagementObjectSearcher(wmiQuery);
            else
                query = new ManagementObjectSearcher(scope, wmiQuery);
            var moc = query.Get();
            foreach (ManagementObject mo in moc)
                ret.Add(mo.Properties);
            query.Dispose();

            if (debug)
            {
                Debug.WriteLine("");
                Debug.WriteLine("WMI query '" + (scope == null ? "" : scope + "/") + wmiQuery + "' result:");
                for (var n = 0; n < ret.Count; n++)
                {
                    var pdc = ret[n];
                    Debug.WriteLine("Element '" + n +
                                    " --------------------------------------------------------------------------------");
                    foreach (var pd in pdc)
                        Debug.WriteLine(pd.Name + " (" + pd.Type.GetBaseName() + ") = " +
                                        (pd.Value == null ? "[null]" : pd.Value));
                }
            }

            return ret;
        }

        public override DateTime GetDateTime(ClockSource clockSource)
        {
            return DateTime.Now;
        }
    }
}