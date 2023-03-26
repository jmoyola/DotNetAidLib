using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Configuration;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Enums;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.OperatingSystem.Core;
using Mono.Unix.Native;

namespace DotNetAidLib.OperatingSystem.Imp
{
    public class LinuxOperatingSystem : Core.OperatingSystem
    {
        private static LinuxOperatingSystem _Instance;

        private static readonly Regex shutRebootRegex =
            new Regex(
                @"^([^\s]+)\s+system\s+([^\s]+)\s+([^\s]+)\s+(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}[+-]\d{4})(\s-\s(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}[+-]\d{4}))?",
                RegexOptions.Multiline);

        private readonly FileInfo hwclockFI;
        private readonly FileInfo lastFI;
        private readonly FileInfo runlevelFI;
        private readonly FileInfo shutdownFileInfo;
        private readonly FileInfo systemctlFI;
        private readonly FileInfo systemdShutdownScheduled;
        private readonly FileInfo whoFI;

        protected LinuxOperatingSystem()
        {
            runlevelFI = EnvironmentHelper.SearchInPath("runlevel");
            systemctlFI = EnvironmentHelper.SearchInPath("systemctl");
            hwclockFI = EnvironmentHelper.SearchInPath("hwclock");
            shutdownFileInfo = EnvironmentHelper.SearchInPath("shutdown");
            systemdShutdownScheduled = new FileInfo("/run/systemd/shutdown/scheduled");
            whoFI = EnvironmentHelper.SearchInPath("who");
            lastFI = EnvironmentHelper.SearchInPath("last");
        }

        public override OSRunLevel RunLevel
        {
            get
            {
                var runlevel = OSRunLevel.MultiUser;

                try
                {
                    // Systemd
                    if (systemctlFI.RefreshFluent().Exists)
                    {
                        var aux = systemctlFI.CmdExecuteSync("get-default").RegexGroupsMatches(@"([^.\s]+).target")[1];
                        Enum.TryParse(aux.Replace("-", ""), true, out runlevel);
                    } // SystemV
                    else if (runlevelFI.RefreshFluent().Exists)
                    {
                        var aAux = runlevelFI.CmdExecuteSync().TrimEnd('\r', '\n').Split(' ');
                        var iRunLevel = int.Parse(aAux[1]);
                        if ((iRunLevel == 2) | (iRunLevel == 4))
                            iRunLevel = 3;
                        runlevel = (OSRunLevel) iRunLevel;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error parsing runlevel.", ex);
                }

                return runlevel;
            }
        }

        public override OSScheduleShutdownInfo ScheduleShutdown
        {
            get
            {
                OSScheduleShutdownInfo ret = null;

                OSShutdownType shutdownType;
                var schedule = TimeSpan.Zero;
                string wallMessage = null;
                var warnWall = false;

                if (systemdShutdownScheduled.RefreshFluent().Exists)
                    try
                    {
                        shutdownType = OSShutdownType.PowerOff;


                        var cfg = IniConfigurationFile.Instance(systemdShutdownScheduled);
                        cfg.Load();

                        IDictionary<string, string> values = cfg[""];
                        if (values.ContainsKey("MODE"))
                            shutdownType = values["MODE"].ToEnum<OSShutdownType>(true);
                        if (values.ContainsKey("USEC"))
                            schedule = new DateTime(1970, 1, 1).AddMilliseconds(long.Parse(values["USEC"]) / 1000)
                                .ToLocalTime()
                                .Subtract(DateTime.Now);
                        if (values.ContainsKey("WARN_WALL"))
                            warnWall = "1".Equals(values["WARN_WALL"]);
                        if (values.ContainsKey("WALL_MESSAGE"))
                            wallMessage = values["WALL_MESSAGE"];

                        ret = new OSScheduleShutdownInfo(shutdownType, schedule, warnWall, wallMessage);
                    }
                    catch //(Exception ex)
                    {
                        //throw new Exception("Error parsing shutdown file '" + systemdShutdownScheduled.FullName + "'.", ex);
                        ret = null;
                    }

                return ret;
            }
        }

        public override DateTime LastBoot =>
            DateTime.ParseExact(
                whoFI.CmdExecuteSync("--boot").RegexMatches(@"\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}")[0]
                , "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture.DateTimeFormat);


        public override IEnumerable<OSPeriod> Reboots => GetShutReboots("reboot");

        public override IEnumerable<OSPeriod> Shutdowns => GetShutReboots("shutdown");

        public static IDictionary<string, string> ExecuteInvariantLanguageEnv =>
            new Dictionary<string, string> {{"LC_ALL", "C"}};

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

                var lsbRelease = EnvironmentHelper.SearchInPath("lsb_release");
                if (lsbRelease != null)
                {
                    var lsb = Regex
                        .Matches(lsbRelease.CmdExecuteSync("-a"), @"^([^\n:]+):([^\n]+)$", RegexOptions.Multiline)
                        .Cast<Match>()
                        .ToDictionary(m => m.Groups[1].Value.Trim(), m => m.Groups[2].Value.Trim());

                    id = lsb.GetValueIfExists("Distributor ID", "UNKNOW");
                    var aReleaseRevition = lsb.GetValueIfExists("Release", "UNKNOW").Split('.');
                    release = aReleaseRevition[0];
                    if (aReleaseRevition.Length > 1)
                        revision = aReleaseRevition[1];
                    codename = lsb.GetValueIfExists("Codename", "UNKNOW");
                }

                var uname = EnvironmentHelper.SearchInPath("uname");
                if (uname != null)
                {
                    architecture = uname.CmdExecuteSync("--machine").Replace("\n", "");
                    operatingSystem = uname.CmdExecuteSync("--operating-system").Replace("\n", "");
                    kernel = uname.CmdExecuteSync("--kernel-release").Replace("\n", "");
                }

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

        public override string SystemArchitecture
        {
            get
            {
                var ret = EnvironmentHelper.SearchInPath("uname").CmdExecuteSync("-i").Replace("\n", "");
                return ret;
            }
        }

        public override IEnumerable<SerialPortInfo> SerialPorts =>
            PosixSerialPortInfo.SerialPorts;

        public new static LinuxOperatingSystem Instance()
        {
            if (_Instance == null)
                _Instance = new LinuxOperatingSystem();

            return _Instance;
        }

        public override void Shutdown(OSShutdownType shutdownType, TimeSpan schedule, string message)
        {
            if (schedule.Minutes < 0)
                throw new Exception("Only 0 or future time is allowed (in minutes)");

            if (shutdownType == OSShutdownType.Cancel)
                shutdownFileInfo.CmdExecuteSync("-c");
            else
                shutdownFileInfo.CmdExecuteSync("--" + shutdownType.ToString().ToLower()
                                                     + " +" + (int) schedule.TotalMinutes +
                                                     (message == null ? "" : " " + message));
        }

        private IEnumerable<OSPeriod> GetShutReboots(string type)
        {
            Assert.NotNullOrEmpty(type, nameof(type));

            var cmd = lastFI.CmdExecuteSync("--system --time-format iso " + type);
            var mm = shutRebootRegex.Matches(cmd).Cast<Match>();
            return mm.Select(v => new OSPeriod(v.Groups[1].Value, v.Groups[3].Value,
                    DateTime.ParseExact(v.Groups[4].Value, "yyyy-MM-ddTHH:mm:sszzz",
                        CultureInfo.InvariantCulture.DateTimeFormat),
                    string.IsNullOrEmpty(v.Groups[6].Value)
                        ? new DateTime?()
                        : DateTime.ParseExact(v.Groups[6].Value, "yyyy-MM-ddTHH:mm:sszzz",
                            CultureInfo.InvariantCulture.DateTimeFormat)
                )
            );
        }

        public override string ExecuteCmd(string command, string arguments, bool ignoreExitCode,
            IDictionary<string, string> environmentVariables, int timeoutMs = -1)
        {
            return Execute(EnvironmentHelper.SearchInPath("sh"), "-c \"" + command + " " + arguments + "\"",
                ignoreExitCode, environmentVariables, timeoutMs);
        }

        public override string Execute(FileInfo fileToExecute, string arguments, bool ignoreExitCode,
            IDictionary<string, string> environmentVariables, int timeoutMs = -1)
        {
            string ret = null;

            Process p = null;

            ProcessStartInfo psi = null;

            try
            {
                p = new Process();
                psi = new ProcessStartInfo();

                // Si es el usuario root y quiere ejecutar como él (root), se ejecuta sin mas
                psi.FileName = fileToExecute.FullName;
                psi.Arguments = arguments;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                if (environmentVariables != null)
                    foreach (var kv in environmentVariables)
                        psi.EnvironmentVariables[kv.Key] = kv.Value;

                p.StartInfo = psi;
                p.Start();

                if (timeoutMs > -1)
                    p.WaitForExit(timeoutMs);
                else
                    p.WaitForExit();

                if (!p.HasExited)
                    throw new TimeoutException("Timeout error: Execution time is more of '" + timeoutMs +
                                               "' milliseconds.");

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

                // Si no hay credenciales o es el usuario root y quiere ejecutar como él (root), se ejecuta sin mas
                if (string.IsNullOrEmpty(userName)
                    || (userName.Equals("root", StringComparison.CurrentCultureIgnoreCase)
                        && "0".Equals(Environment.GetEnvironmentVariable("EUID"),
                            StringComparison.InvariantCultureIgnoreCase))
                   )
                {
                    psi.FileName = fileToExecute.FullName;
                    if (!string.IsNullOrEmpty(arguments))
                        psi.Arguments = arguments;
                }
                else // Si no, se ejecuta con sudo
                {
                    var fiSudo = EnvironmentHelper.SearchInPath("sudo");
                    if (fiSudo == null)
                        throw new Exception("'sudo' is not installed in the system.");

                    psi.FileName = fiSudo.FullName;
                    psi.Arguments = ((!string.IsNullOrEmpty(password) ? " --stdin --reset-timestamp" : "")
                                     + (!string.IsNullOrEmpty(userName) &&
                                        !userName.Equals("sudo", StringComparison.InvariantCulture)
                                         ? " --user=" + userName
                                         : "")
                                     + " \"" + fileToExecute.FullName + "\""
                                     + (!string.IsNullOrEmpty(arguments) ? " " + arguments : "")
                        ).Trim();
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
            return AdminExecute(EnvironmentHelper.SearchInPath("sh"), "-c " + command + " " + arguments, userName,
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
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.UseShellExecute = false;
                if (environmentVariables != null)
                    foreach (var kv in environmentVariables)
                        p.StartInfo.EnvironmentVariables[kv.Key] = kv.Value;

                p.Start();

                if (!string.IsNullOrEmpty(password) && !p.HasExited)
                    p.StandardInput.Write(password + "\n\n\n");

                if (timeoutMs > -1)
                    p.WaitForExit(timeoutMs);
                else
                    p.WaitForExit();

                if (!p.HasExited)
                    throw new TimeoutException("Timeout error: Execution time is more of '" + timeoutMs +
                                               "' milliseconds.");

                var processExitCode = p.ExitCode;

                if (!ignoreExitCode && processExitCode != 0)
                    throw new OperatingSystemException("Exit code non zero '" + processExitCode +
                                                       "' executing command '" + fileToExecute.FullName +
                                                       (arguments == null ? "" : " " + arguments) + "': " +
                                                       p.StandardError.ReadToEnd());

                ret = p.StandardOutput.ReadToEnd();

                return ret;
            }
            catch (Exception ex)
            {
                throw new OperatingSystemException(
                    "Error executing '" + fileToExecute.FullName + "' with admin privileges (sudo): " + ex.Message, ex);
            }
            finally
            {
                p.Dispose();
            }
        }

        public override void AdminTextWrite(FileInfo outputFile, string text, string userName, string password,
            string domain)
        {
            var tempFile = new FileInfo(".").RandomTempFile();
            var sw = tempFile.CreateText();
            sw.Write(text);
            sw.Flush();
            sw.Close();

            var mvFileInfo = EnvironmentHelper.SearchInPath("mv");
            var arguments = "'" + tempFile.FullName + "' '" + outputFile.FullName + "'";
            AdminExecute(mvFileInfo, arguments, userName, password, domain);
        }

        public override string AdminTextRead(FileInfo inputFile, string userName, string password, string domain)
        {
            var catFileInfo = EnvironmentHelper.SearchInPath("sh");
            var arguments = "-c \"cat '" + inputFile.FullName + "'\"";
            return AdminExecute(catFileInfo, arguments, userName, password, domain);
        }

        private string Scape(string value)
        {
            return value.Replace("\"", @"\""").Replace("'", @"\'").Replace("\t", @"\t").Replace("\n", @"\n")
                .Replace("\r", @"\r");
        }

        // public override void ChangeHostName(String newHostName) {
        // 	try
        // 	{
        // 		FileInfo hostsFileInfo = new FileInfo("/etc/hosts");
        // 		FileInfo hostnameFileInfo = EnvironmentHelper.SearchInPath("hostname");
        // 		FileInfo sedFileInfo = EnvironmentHelper.SearchInPath("sed");
        // 		Assert.NotNull(hostnameFileInfo);
        // 		Assert.NotNull(sedFileInfo);
        // 		this.AdminExecute(hostnameFileInfo, newHostName);
        // 		TabConfigurationFile tcf = TabConfigurationFile.Instance(hostsFileInfo);
        // 		tcf.Load();
        // 		if(!tcf.Any(v=>v.Value!=null && v.Value[0]=="127.0.1.1" && v.Value[1] == newHostName))
        // 			this.AdminExecute(sedFileInfo, "-i '$a 127.0.1.1    " + newHostName + "' " + hostsFileInfo.FullName);
        // 		if (!tcf.Any(v => v.Value != null && v.Value[0] == "127.0.0.1" && v.Value[1] == newHostName))
        // 			this.AdminExecute(sedFileInfo, "-i '$a 127.0.0.1    " + newHostName + "' " + hostsFileInfo.FullName);
        // 	}
        // 	catch (Exception ex)
        // 	{
        // 		throw new OperatingSystemException("Error setting hostname to '" + newHostName + "'", ex);
        // 	}
        //       }

        public override void ChangeHostName(string newHostName)
        {
            try
            {
                var hostsFileInfo = new FileInfo("/etc/hosts");
                var hostnameCtlFI = EnvironmentHelper.SearchInPath("hostnamectl");
                var sedFileInfo = EnvironmentHelper.SearchInPath("sed");
                Assert.NotNull(hostnameCtlFI);
                Assert.NotNull(sedFileInfo);

                // Cambiamos hostname
                AdminExecute(hostnameCtlFI, "set-hostname " + newHostName);

                var tcf = TabConfigurationFile.Instance(hostsFileInfo);
                tcf.Load();
                if (!tcf.Any(v => v.Value != null && v.Value[0] == "127.0.1.1" && v.Value[1] == newHostName))
                    AdminExecute(sedFileInfo, "-i '$a 127.0.1.1    " + newHostName + "' " + hostsFileInfo.FullName);
                if (!tcf.Any(v => v.Value != null && v.Value[0] == "127.0.0.1" && v.Value[1] == newHostName))
                    AdminExecute(sedFileInfo, "-i '$a 127.0.0.1    " + newHostName + "' " + hostsFileInfo.FullName);
            }
            catch (Exception ex)
            {
                throw new OperatingSystemException("Error setting hostname to '" + newHostName + "'", ex);
            }
        }

        public override void AdminCopy(string from, string to, bool recursive, bool preserveDates, bool preserveOwners,
            bool preserveRights, bool preserveOthers, bool followLinks = false)
        {
            var cpFile = EnvironmentHelper.SearchInPath("cp");
            IList<string> preserverList = new List<string>();
            if (preserveDates)
                preserverList.Add("timestamps");
            if (preserveOwners)
                preserverList.Add("ownership");
            if (preserveRights)
                preserverList.Add("mode");
            if (preserveOthers)
            {
                // preserverList.Add("context"); // Es para SE Linux (si no está, da error)
                preserverList.Add("links");
                preserverList.Add("xattr");
            }

            var parameters = "--" + (followLinks ? "" : "no-") + "dereference";
            parameters += recursive ? " --recursive" : "";
            parameters += preserverList.Count > 0 ? " --preserve=" + preserverList.ToStringJoin(",") : "";
            parameters += " " + from + " " + to;
            AdminExecute(cpFile, parameters);
        }

        public override string GetUserDirectory(string user)
        {
            var getentFI = EnvironmentHelper.SearchInPath("getent");
            string ret = null;
            try
            {
                if (getentFI == null)
                    throw new OperatingSystemException("getent program is missing.");

                ret = getentFI
                    .CmdExecuteSync("passwd " + user);
                var aret = ret.Split(':');
                if (ret.Length > 5)
                    ret = aret[5];
                else
                    ret = "/home/" + user;

                return ret;
            }
            catch (Exception ex)
            {
                throw new OperatingSystemException("Error retrieving user directory.", ex);
            }
        }

        private static string GetValuePair(string keyValue, char separator)
        {
            string ret = null;
            try
            {
                ret = keyValue.Split(separator)[1].Trim();
            }
            catch
            {
            }

            return ret;
        }

        [DllImport("libc")] // Linux
        private static extern int prctl(int option, byte[] arg2, IntPtr arg3,
            IntPtr arg4, IntPtr arg5);

        [DllImport("libc")] // BSD
        private static extern void setproctitle(byte[] fmt, byte[] str_arg);

        public static void ChangeProcessName(string newName)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                try
                {
                    if (prctl(15 /* PR_SET_NAME */, Encoding.ASCII.GetBytes(newName + "\0"),
                            IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) != 0)
                        throw new ApplicationException("Error setting process name: " +
                                                       Stdlib.GetLastError());
                }
                catch (EntryPointNotFoundException)
                {
                    setproctitle(Encoding.ASCII.GetBytes("%s\0"),
                        Encoding.ASCII.GetBytes(newName + "\0"));
                }
        }

        public override DateTime GetDateTime(ClockSource clockSource)
        {
            var ret = default(DateTime);

            if (clockSource == ClockSource.SystemClock)
            {
                ret = DateTime.Now;
            }
            else if (clockSource == ClockSource.RealTimeClock)
            {
                string cl = null;
                cl = AdminExecute(hwclockFI, "-r", false, new Dictionary<string, string> {{"LC_ALL", "en_US"}});
                var m = Regex.Match(cl, @"\d{2}\s\D{3}\s\d{4}\s\d{2}\:\d{2}\:\d{2}\s(AM|PM)");
                if (!m.Success)
                    m = Regex.Match(cl, @"(\d{4}-\d{2}-\d{2}\s\d{2}\:\d{2}\:\d{2}\.\d+)");
                if (m.Success)
                    if (!DateTime.TryParseExact(m.Value
                            , new[]
                            {
                                "dd MMM yyyy hh:mm:ss tt", "dd MMM yyyy hh:mm:ss TT",
                                "yyyy-MM-dd HH:mm:ss.ff", "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss.ffff",
                                "yyyy-MM-dd HH:mm:ss.fffff", "yyyy-MM-dd HH:mm:ss.ffffff"
                            }
                            , CultureInfo.InvariantCulture.DateTimeFormat
                            , DateTimeStyles.AssumeLocal
                            , out ret))
                        ret = DateTime.Now;
            }

            return ret;
        }
    }
}