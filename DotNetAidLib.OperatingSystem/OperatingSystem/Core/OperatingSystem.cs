using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using DotNetAidLib.Core.AAA.Core;
using DotNetAidLib.Core.AAA.Imp;
using DotNetAidLib.Core.Context;
using DotNetAidLib.OperatingSystem.Imp;

namespace DotNetAidLib.OperatingSystem.Core
{
    public abstract class OperatingSystem
    {
        public enum ClockSource
        {
            SystemClock,
            RealTimeClock
        }

        private static AuthenticationFactory createProcessAuthFactory;

        public bool IsDotNetCore =>
            !RuntimeInformation.FrameworkDescription.StartsWith("NET Framework",
                StringComparison.InvariantCultureIgnoreCase);

        public abstract DateTime LastBoot { get; }
        public abstract IEnumerable<OSPeriod> Reboots { get; }
        public abstract IEnumerable<OSPeriod> Shutdowns { get; }
        public abstract OSRunLevel RunLevel { get; }
        public abstract OSScheduleShutdownInfo ScheduleShutdown { get; }

        public abstract OSDistribution DistributionInfo { get; }
        public abstract string SystemArchitecture { get; }

        public abstract IEnumerable<SerialPortInfo> SerialPorts { get; }

        public void Shutdown(OSShutdownType shutdownType)
        {
            Shutdown(shutdownType, new TimeSpan(0), null);
        }

        public abstract void Shutdown(OSShutdownType shutdownType, TimeSpan schedule, string message);

        public Process CreateProcess(FileInfo fileToExecute, string arguments)
        {
            if (createProcessAuthFactory == null)
            {
                createProcessAuthFactory = new AuthenticationFactory();
                createProcessAuthFactory.Providers.Add(new EnvVarAuthUserPasswordProvider());
            }

            var id = (UserPasswordIdentity) createProcessAuthFactory.GetIdentity().FirstOrDefault();

            if (id != null)
                return CreateProcess(fileToExecute, arguments, id.UserName, id.Password, id.Domain);
            return CreateProcess(fileToExecute, arguments, null, null, null);
        }

        public abstract Process CreateProcess(FileInfo fileToExecute, string arguments, string userName,
            string password, string domain);

        public string AdminExecute(FileInfo fileToExecute, string arguments)
        {
            return AdminExecute(fileToExecute, arguments, false, null);
        }

        public string AdminExecute(FileInfo fileToExecute, string arguments, bool ignoreExitCode,
            IDictionary<string, string> environmentVariables)
        {
            if (ContextFactory.Instance().Attributes.ContainsKey("cmdExecuteSyncCredentials"))
            {
                var nc = (NetworkCredential) ContextFactory.Instance().Attributes["cmdExecuteSyncCredentials"];
                return AdminExecute(fileToExecute, arguments, nc.UserName, nc.Password, nc.Domain, ignoreExitCode,
                    environmentVariables);
            }

            return AdminExecute(fileToExecute, arguments, null, null, null, ignoreExitCode, environmentVariables);
            // throw new Exception("Context factory is missing NetworkCredentials in attribute 'cmdExecuteSyncCredentials'.");
        }

        public string AdminExecute(FileInfo fileToExecute, string arguments, string userName, string password,
            string domain)
        {
            return AdminExecute(fileToExecute, arguments, userName, password, domain, false);
        }

        public string AdminExecute(FileInfo fileToExecute, string arguments, string userName, string password,
            string domain, bool ignoreExitCode)
        {
            return AdminExecute(fileToExecute, arguments, userName, password, domain, ignoreExitCode, null);
        }

        public string AdminExecuteCmd(string command, string arguments)
        {
            return AdminExecuteCmd(command, arguments, false, null);
        }

        public string AdminExecuteCmd(string command, string arguments, bool ignoreExitCode,
            IDictionary<string, string> environmentVariables)
        {
            if (ContextFactory.Instance().Attributes.ContainsKey("cmdExecuteSyncCredentials"))
            {
                var nc = (NetworkCredential) ContextFactory.Instance().Attributes["cmdExecuteSyncCredentials"];
                return AdminExecuteCmd(command, arguments, nc.UserName, nc.Password, nc.Domain, ignoreExitCode,
                    environmentVariables);
            }

            throw new Exception(
                "Context factory is missing NetworkCredentials in attribute 'cmdExecuteSyncCredentials'.");
        }

        public string AdminExecuteCmd(string command, string arguments, string userName, string password, string domain)
        {
            return AdminExecuteCmd(command, arguments, userName, password, domain, false);
        }

        public string AdminExecuteCmd(string command, string arguments, string userName, string password, string domain,
            bool ignoreExitCode)
        {
            return AdminExecuteCmd(command, arguments, userName, password, domain, ignoreExitCode, null);
        }

        public abstract string ExecuteCmd(string command, string arguments, bool ignoreExitCode,
            IDictionary<string, string> environmentVariables, int timeoutMs = -1);

        public abstract string Execute(FileInfo fileToExecute, string arguments, bool ignoreExitCode,
            IDictionary<string, string> environmentVariables, int timeoutMs = -1);

        public abstract string AdminExecuteCmd(string command, string arguments, string userName, string password,
            string domain, bool ignoreExitCode, IDictionary<string, string> environmentVariables, int timeoutMs = -1);

        public abstract string AdminExecute(FileInfo fileToExecute, string arguments, string userName, string password,
            string domain, bool ignoreExitCode, IDictionary<string, string> environmentVariables, int timeoutMs = -1);

        public static OperatingSystem Instance()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return WindowsOperatingSystem.Instance();
            return LinuxOperatingSystem.Instance();
        }

        public void AdminTextWrite(FileInfo outputFile, string text)
        {
            if (ContextFactory.Instance().Attributes.ContainsKey("cmdExecuteSyncCredentials"))
            {
                var nc = (NetworkCredential) ContextFactory.Instance().Attributes["cmdExecuteSyncCredentials"];
                AdminTextWrite(outputFile, text, nc.UserName, nc.Password, nc.Domain);
            }
            else
            {
                throw new Exception(
                    "Context factory is missing NetworkCredentials in attribute 'cmdExecuteSyncCredentials'.");
            }
        }

        public abstract void AdminTextWrite(FileInfo outputFile, string text, string userName, string password,
            string domain);

        public string AdminTextRead(FileInfo inputFile)
        {
            if (ContextFactory.Instance().Attributes.ContainsKey("cmdExecuteSyncCredentials"))
            {
                var nc = (NetworkCredential) ContextFactory.Instance().Attributes["cmdExecuteSyncCredentials"];
                return AdminTextRead(inputFile, nc.UserName, nc.Password, nc.Domain);
            }

            throw new Exception(
                "Context factory is missing NetworkCredentials in attribute 'cmdExecuteSyncCredentials'.");
        }

        public abstract string AdminTextRead(FileInfo inputFile, string userName, string password, string domain);

        public abstract void ChangeHostName(string newHostName);

        public abstract void AdminCopy(string from, string to, bool recursive, bool preserveDates, bool preserveOwners,
            bool preserveRights, bool preserveOthers, bool followLinks = false);

        public abstract string GetUserDirectory(string user);
        public abstract DateTime GetDateTime(ClockSource clockSource);

        public virtual long Millis()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public virtual long Micros()
        {
            return DateTime.Now.Ticks / (TimeSpan.TicksPerMillisecond / 1000);
        }
    }
}