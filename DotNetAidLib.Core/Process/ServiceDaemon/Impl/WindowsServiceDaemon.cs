using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Process.ServiceDaemon.Core;
using Microsoft.Win32;

namespace DotNetAidLib.Core.Process.ServiceDaemon.Imp
{
    public class WindowsServiceDaemon:DotNetAidLib.Core.Process.ServiceDaemon.Core.ServiceDaemon
    {
        private static FileInfo scFile = null;

        public WindowsServiceDaemon(String name)
        :base(name){
            scFile = EnvironmentHelper.SearchInPath("sc.exe");
            if (scFile == null || !scFile.Exists)
                throw new DotNetAidLib.Core.Process.ServiceDaemon.Core.ServiceDaemonException("Can't find 'sc.exe' program.");
        }

        public override bool IsServiceInstalled
        {
            get
            {
                ServiceController serv = ServiceController.GetServices().FirstOrDefault((ServiceController v) => v.ServiceName == ID);
                if ((serv == null))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public override string ExecutablePath
        {
            get
            {
                RegistryKey serviceSKey = GetServiceRegistryRoot();
                object pathObject = serviceSKey.GetValue("ImagePath");
                if (pathObject != null)
                    return pathObject.ToString().Replace("\"", "");

                return null;
            }
        }

        public override ServiceControllerStatus Status
        {
            get
            {
                ServiceController srv = ServiceController.GetServices().FirstOrDefault((ServiceController v) => v.ServiceName == ID);

                if (((srv != null)))
                {
                    return srv.Status;
                }
                else
                    throw new Exception("Can't find service name '" + ID + "'");
            }
        }

        public override string Name
        {
            get
            {

                RegistryKey serviceSKey = GetServiceRegistryRoot();
                object pathObject = serviceSKey.GetValue("DisplayName");
                if (pathObject != null)
                    return pathObject.ToString();

                return null;
            }
        }

        public override string Description
        {
            get
            {

                RegistryKey serviceSKey = GetServiceRegistryRoot();
                object pathObject = serviceSKey.GetValue("Description");
                if (pathObject != null)
                    return pathObject.ToString();
                else
                    return null;
            }
        }

        public override string ConfigPath
        {
            get
            {
                return "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Services\\" + this.ID;
            }
        }

        public override IEnumerable<DotNetAidLib.Core.Process.ServiceDaemon.Core.ServiceDaemon> ServicesDepended
        {
            get
            {
                RegistryKey serviceSKey = GetServiceRegistryRoot();

                String[] pathObject = (String[])serviceSKey.GetValue("DependOnService");
                if (pathObject != null)
                    return pathObject.Select(v=>new WindowsServiceDaemon(v));

                return new List<WindowsServiceDaemon>();
            }
        }

        private RegistryKey GetServiceRegistryRoot()
        {
            RegistryKey serviceSKey = Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\services\\" + this.ID, true);
            if (serviceSKey == null)
                throw new Exception("Don't exists service '" + ID + "'.");
            return serviceSKey;
        }

        public override void Disable()
        {
            scFile.CmdExecuteSync("config " + this.ID + " start= disabled");
        }

        public override void Enable()
        {
            scFile.CmdExecuteSync("config " + this.ID + " start= auto");
        }

        public override void Mask()
        {
            // OperatingSystem.Core.OperatingSystem.Instance().AdminExecute(scFile, "config " + this.ID + " start= disabled");
        }

        public override void Unmask()
        {
            // OperatingSystem.Core.OperatingSystem.Instance().AdminExecute(scFile, "config " + this.ID + " start= auto");
        }

        public override void Pause(int timeoutMs)
        {
            scFile.CmdExecuteSync("pause " + this.ID);
        }


        public override void Resume(int timeoutMs)
        {
            scFile.CmdExecuteSync("resume " + this.ID);
        }

        public override void Start(int timeoutMs)
        {
            scFile.CmdExecuteSync("start " + this.ID);
        }

        public override void Stop(int timeoutMs)
        {
            scFile.CmdExecuteSync("stop " + this.ID);
        }

    }
}
