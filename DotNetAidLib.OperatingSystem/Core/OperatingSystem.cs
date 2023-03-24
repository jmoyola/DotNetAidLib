using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Context;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Introspection;
using Library.AAA.Core;
using Library.AAA.Imp;
using Library.OperatingSystem.Imp;

namespace Library.OperatingSystem.Core
{
	public abstract class OperatingSystem
	{
		
		protected OperatingSystem ()
		{
		}

		public bool IsDotNetCore
		{
			get => !System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.StartsWith("NET Framework", StringComparison.InvariantCultureIgnoreCase);
		}
		public abstract DateTime LastBoot { get; }
        public abstract IEnumerable<OSPeriod> Reboots { get; }
        public abstract IEnumerable<OSPeriod> Shutdowns { get; }
        public void Shutdown (OSShutdownType shutdownType)
        {
            this.Shutdown (shutdownType, new TimeSpan (0), null);
        }

        public abstract void Shutdown (OSShutdownType shutdownType, TimeSpan schedule, String message);
        public abstract OSRunLevel RunLevel { get;}
        public abstract OSScheduleShutdownInfo ScheduleShutdown { get; }

        private static AuthenticationFactory createProcessAuthFactory = null;
        public Process CreateProcess(System.IO.FileInfo fileToExecute, String arguments)
        {
            if (createProcessAuthFactory == null) {
                createProcessAuthFactory = new AuthenticationFactory();
                createProcessAuthFactory.Providers.Add(new EnvVarAuthUserPasswordProvider());
            }
            UserPasswordIdentity id = (UserPasswordIdentity)createProcessAuthFactory.GetIdentity().FirstOrDefault();

            if(id!=null)
                return CreateProcess(fileToExecute, arguments, id.UserName, id.Password, id.Domain);
            else
                return CreateProcess(fileToExecute, arguments, null, null, null);
        }

        public abstract Process CreateProcess(System.IO.FileInfo fileToExecute, String arguments, String userName, String password, String domain);

        public String AdminExecute(System.IO.FileInfo fileToExecute, String arguments){
            return AdminExecute(fileToExecute, arguments, false, null);
        }

        public String AdminExecute(System.IO.FileInfo fileToExecute, String arguments, bool ignoreExitCode, IDictionary<String, String> environmentVariables) {
			if (ContextFactory.Instance().Attributes.ContainsKey("cmdExecuteSyncCredentials"))
			{
				System.Net.NetworkCredential nc = (System.Net.NetworkCredential)ContextFactory.Instance().Attributes["cmdExecuteSyncCredentials"];
                return AdminExecute(fileToExecute, arguments, nc.UserName, nc.Password, nc.Domain, ignoreExitCode, environmentVariables);
			}
			else
                return AdminExecute(fileToExecute, arguments, null, null, null, ignoreExitCode, environmentVariables);
            // throw new Exception("Context factory is missing NetworkCredentials in attribute 'cmdExecuteSyncCredentials'.");
        }

        public String AdminExecute(System.IO.FileInfo fileToExecute, String arguments, String userName, String password, String domain)
        {
            return AdminExecute(fileToExecute, arguments, userName, password, domain, false);
        }

        public String AdminExecute(System.IO.FileInfo fileToExecute, String arguments, String userName, String password, String domain, bool ignoreExitCode)
        {
            return AdminExecute(fileToExecute, arguments, userName, password, domain, ignoreExitCode, null);
        }

		public String AdminExecuteCmd(String command, String arguments)
        {
			return AdminExecuteCmd(command, arguments, false, null);
        }

		public String AdminExecuteCmd(String command, String arguments, bool ignoreExitCode, IDictionary<String, String> environmentVariables)
        {
            if (ContextFactory.Instance().Attributes.ContainsKey("cmdExecuteSyncCredentials"))
            {
                System.Net.NetworkCredential nc = (System.Net.NetworkCredential)ContextFactory.Instance().Attributes["cmdExecuteSyncCredentials"];
				return AdminExecuteCmd(command, arguments, nc.UserName, nc.Password, nc.Domain, ignoreExitCode, environmentVariables);
            }
            else throw new Exception("Context factory is missing NetworkCredentials in attribute 'cmdExecuteSyncCredentials'.");
        }

		public String AdminExecuteCmd(String command, String arguments, String userName, String password, String domain)
        {
			return AdminExecuteCmd(command, arguments, userName, password, domain, false);
        }

		public String AdminExecuteCmd(String command, String arguments, String userName, String password, String domain, bool ignoreExitCode)
        {
			return AdminExecuteCmd(command, arguments, userName, password, domain, ignoreExitCode, null);
        }

		public abstract String ExecuteCmd(String command, String arguments, bool ignoreExitCode, IDictionary<String, String> environmentVariables, int timeoutMs = -1);
        public abstract String Execute (System.IO.FileInfo fileToExecute, String arguments, bool ignoreExitCode, IDictionary<String, String> environmentVariables, int timeoutMs=-1);

        public abstract String AdminExecuteCmd (String command, String arguments, String userName, String password, String domain, bool ignoreExitCode, IDictionary<String, String> environmentVariables, int timeoutMs = -1);
        public abstract String AdminExecute (System.IO.FileInfo fileToExecute, String arguments, String userName, String password, String domain, bool ignoreExitCode, IDictionary<String, String> environmentVariables, int timeoutMs = -1);

        public static OperatingSystem Instance(){
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return WindowsOperatingSystem.Instance ();
			else
				return LinuxOperatingSystem.Instance ();
		}

		public void AdminTextWrite(System.IO.FileInfo outputFile, String text)
		{
			if (ContextFactory.Instance().Attributes.ContainsKey("cmdExecuteSyncCredentials"))
			{
				System.Net.NetworkCredential nc = (System.Net.NetworkCredential)ContextFactory.Instance().Attributes["cmdExecuteSyncCredentials"];
				AdminTextWrite(outputFile,text, nc.UserName, nc.Password, nc.Domain);
			}
			else
				throw new Exception("Context factory is missing NetworkCredentials in attribute 'cmdExecuteSyncCredentials'.");
		}

		public abstract void AdminTextWrite(System.IO.FileInfo outputFile, String text, String userName, String password, String domain);

		public String AdminTextRead(System.IO.FileInfo inputFile) {
			if (ContextFactory.Instance().Attributes.ContainsKey("cmdExecuteSyncCredentials"))
			{
				System.Net.NetworkCredential nc = (System.Net.NetworkCredential)ContextFactory.Instance().Attributes["cmdExecuteSyncCredentials"];
				return AdminTextRead(inputFile, nc.UserName, nc.Password, nc.Domain);
			}
			else
				throw new Exception("Context factory is missing NetworkCredentials in attribute 'cmdExecuteSyncCredentials'.");
		}

		public abstract String AdminTextRead(System.IO.FileInfo inputFile, String userName, String password, String domain);

		public abstract void ChangeHostName(String newHostName);

        public abstract void AdminCopy(String from, String to, bool recursive, bool preserveDates, bool preserveOwners, bool preserveRights, bool preserveOthers, bool followLinks = false);

        public abstract String GetUserDirectory(String user);

        public abstract OSDistribution DistributionInfo { get; }
        public abstract String SystemArchitecture { get; }

        public enum ClockSource { SystemClock, RealTimeClock }
        public abstract DateTime GetDateTime(ClockSource clockSource);

        public virtual Int64 Millis()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public virtual Int64 Micros()
        {
            return DateTime.Now.Ticks / (TimeSpan.TicksPerMillisecond / 1000);
        }

        public abstract IEnumerable<SerialPortInfo> SerialPorts { get; }
    }
}

