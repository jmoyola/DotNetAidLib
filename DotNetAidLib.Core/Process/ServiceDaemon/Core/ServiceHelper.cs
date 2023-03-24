using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using DotNetAidLib.Core.Proc;
using Microsoft.Win32;
using DotNetAidLib.Core.Process.ServiceDaemon.Imp;

namespace DotNetAidLib.Core.Process.ServiceDaemon.Core
{
    public delegate void AsyncExceptionHandler(object sender, Exception ex);

    public enum ServiceTarget {
        Halt=0,
        SingleUser=1,
        MultiUserNonGraphical=3,
        MultiUserGraphical =5,
        Reboot=6
    }

    public enum ServiceRestart {
        No,
        Always,
        On_success,
        On_failure
    }

    public abstract class ServiceHelper
	{
		private static ServiceHelper _Instance = null;

		public abstract ServiceDaemon Install (String serviceID, String serviceName, String serviceDescription, String executablePath, String executableWorkingDirectory, String runAsUser, IEnumerable<String> serviceDepends, IEnumerable<ServiceTarget> targets, ServiceRestart restart, IEnumerable<String> mountPointsDepended=null);
        public abstract void Uninstall(String serviceID);

        public abstract IEnumerable<ServiceDaemon> GetServices ();
        public abstract ServiceDaemon GetService(String serviceID);

        public static System.Diagnostics.Process ExecuteProcessIfIsRunningServiceName(string runningServiceName, string executablePath, string args, bool RedirectStandardInput, bool RedirectStandardOutput, bool UseShellExecute, bool CreateNoWindows)
		{
			while (!ServiceHelper.Instance().GetService(runningServiceName).Status.Equals (ServiceControllerStatus.Running))
				Thread.Sleep (1);
				return ProcessHelper.ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput, UseShellExecute, CreateNoWindows);
		}

		public static System.Diagnostics.Process ExecuteProcessAssertByRunningServiceName(string runningServiceName, string executablePath, string args, bool RedirectStandardInput, bool RedirectStandardOutput, bool UseShellExecute, bool CreateNoWindows)
		{
			if (ServiceHelper.Instance().GetService(runningServiceName).Status.Equals(ServiceControllerStatus.Running)) {
				return ProcessHelper.ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput, UseShellExecute, CreateNoWindows);
			} else {
				throw new Exception("Service Name '" + runningServiceName + "' is not running.");
			}
		}

        public static void Enable(IList<String> services)
        {
            IList<ServiceDaemon> serviceDaemons = services.Select(v =>
                ServiceHelper.Instance().GetService(v)).ToList();

            foreach (ServiceDaemon service in serviceDaemons)
                if (service != null)
                    service.Enable();
        }

        public static void Disable(IList<String> services)
        {
            IList<ServiceDaemon> serviceDaemons = services.Select(v =>
                ServiceHelper.Instance().GetService(v)).ToList();

            foreach (ServiceDaemon service in serviceDaemons)
                if (service != null)
                    service.Disable();
        }

        public static void Mask(IList<String> services)
        {
            IList<ServiceDaemon> serviceDaemons = services.Select(v =>
                ServiceHelper.Instance().GetService(v)).ToList();

            foreach (ServiceDaemon service in serviceDaemons)
                if (service != null)
                    service.Mask();
        }

        public static void Unmask(IList<String> services)
        {
            IList<ServiceDaemon> serviceDaemons = services.Select(v =>
                ServiceHelper.Instance().GetService(v)).ToList();

            foreach (ServiceDaemon service in serviceDaemons)
                if (service != null)
                    service.Unmask();
        }

        public static void Start(IList<String> services, int timeoutMs = 10)
        {
            IList<ServiceDaemon> serviceDaemons = services.Select(v =>
                ServiceHelper.Instance().GetService(v)).ToList();

            foreach (ServiceDaemon service in serviceDaemons)
            {
                if (service != null
                    && service.Status != System.ServiceProcess.ServiceControllerStatus.Running)
                    service.Start(10);
            }
        }

        public static void Stop(IList<String> services, int timeoutMs = 10)
        {
            IList<ServiceDaemon> serviceDaemons = services.Select(v =>
                ServiceHelper.Instance().GetService(v)).ToList();

            foreach (ServiceDaemon service in serviceDaemons)
            {
                if (service != null
                    && service.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                    service.Stop(10);
            }
        }

        public static ServiceHelper Instance(){
			if (_Instance == null) {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _Instance = WindowsSCServiceHelper.Instance();
                }
                else
                {
                    try
                    {
                        _Instance = SystemDServiceHelper.Instance();
                    }
                    catch
                    {
                        _Instance = SystemVServiceHelper.Instance();
                    }
                }
			}

			return _Instance;	
		}
    }
}

