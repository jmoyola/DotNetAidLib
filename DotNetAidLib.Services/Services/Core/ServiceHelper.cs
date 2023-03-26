using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using DotNetAidLib.Core.Proc;
using DotNetAidLib.Services.Imp;

namespace DotNetAidLib.Services.Core
{
    public delegate void AsyncExceptionHandler(object sender, Exception ex);

    public enum ServiceTarget
    {
        Halt = 0,
        SingleUser = 1,
        MultiUserNonGraphical = 3,
        MultiUserGraphical = 5,
        Reboot = 6
    }

    public enum ServiceRestart
    {
        No,
        Always,
        On_success,
        On_failure
    }

    public abstract class ServiceHelper
    {
        private static ServiceHelper _Instance;

        public abstract ServiceDaemon Install(string serviceID, string serviceName, string serviceDescription,
            string executablePath, string executableWorkingDirectory, string runAsUser,
            IEnumerable<string> serviceDepends, IEnumerable<ServiceTarget> targets, ServiceRestart restart,
            IEnumerable<string> mountPointsDepended = null);

        public abstract void Uninstall(string serviceID);

        public abstract IEnumerable<ServiceDaemon> GetServices();
        public abstract ServiceDaemon GetService(string serviceID);

        public static Process ExecuteProcessIfIsRunningServiceName(string runningServiceName, string executablePath,
            string args, bool RedirectStandardInput, bool RedirectStandardOutput, bool UseShellExecute,
            bool CreateNoWindows)
        {
            while (!Instance().GetService(runningServiceName).Status.Equals(ServiceControllerStatus.Running))
                Thread.Sleep(1);
            return ProcessHelper.ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput,
                UseShellExecute, CreateNoWindows);
        }

        public static Process ExecuteProcessAssertByRunningServiceName(string runningServiceName, string executablePath,
            string args, bool RedirectStandardInput, bool RedirectStandardOutput, bool UseShellExecute,
            bool CreateNoWindows)
        {
            if (Instance().GetService(runningServiceName).Status.Equals(ServiceControllerStatus.Running))
                return ProcessHelper.ExecuteProcess(executablePath, args, RedirectStandardInput, RedirectStandardOutput,
                    UseShellExecute, CreateNoWindows);
            throw new Exception("Service Name '" + runningServiceName + "' is not running.");
        }

        public static void Enable(IList<string> services)
        {
            IList<ServiceDaemon> serviceDaemons = services.Select(v =>
                Instance().GetService(v)).ToList();

            foreach (var service in serviceDaemons)
                if (service != null)
                    service.Enable();
        }

        public static void Disable(IList<string> services)
        {
            IList<ServiceDaemon> serviceDaemons = services.Select(v =>
                Instance().GetService(v)).ToList();

            foreach (var service in serviceDaemons)
                if (service != null)
                    service.Disable();
        }

        public static void Mask(IList<string> services)
        {
            IList<ServiceDaemon> serviceDaemons = services.Select(v =>
                Instance().GetService(v)).ToList();

            foreach (var service in serviceDaemons)
                if (service != null)
                    service.Mask();
        }

        public static void Unmask(IList<string> services)
        {
            IList<ServiceDaemon> serviceDaemons = services.Select(v =>
                Instance().GetService(v)).ToList();

            foreach (var service in serviceDaemons)
                if (service != null)
                    service.Unmask();
        }

        public static void Start(IList<string> services, int timeoutMs = 10)
        {
            IList<ServiceDaemon> serviceDaemons = services.Select(v =>
                Instance().GetService(v)).ToList();

            foreach (var service in serviceDaemons)
                if (service != null
                    && service.Status != ServiceControllerStatus.Running)
                    service.Start(10);
        }

        public static void Stop(IList<string> services, int timeoutMs = 10)
        {
            IList<ServiceDaemon> serviceDaemons = services.Select(v =>
                Instance().GetService(v)).ToList();

            foreach (var service in serviceDaemons)
                if (service != null
                    && service.Status == ServiceControllerStatus.Running)
                    service.Stop(10);
        }

        public static ServiceHelper Instance()
        {
            if (_Instance == null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    _Instance = WindowsSCServiceHelper.Instance();
                else
                    try
                    {
                        _Instance = SystemDServiceHelper.Instance();
                    }
                    catch
                    {
                        _Instance = SystemVServiceHelper.Instance();
                    }
            }

            return _Instance;
        }
    }
}