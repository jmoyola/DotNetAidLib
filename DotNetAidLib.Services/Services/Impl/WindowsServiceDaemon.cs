using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Services.Core;
using Microsoft.Win32;

namespace DotNetAidLib.Services.Imp
{
    public class WindowsServiceDaemon : ServiceDaemon
    {
        private static FileInfo scFile;

        public WindowsServiceDaemon(string name)
            : base(name)
        {
            scFile = EnvironmentHelper.SearchInPath("sc.exe");
            if (scFile == null || !scFile.Exists)
                throw new ServiceDaemonException("Can't find 'sc.exe' program.");
        }

        public override bool IsServiceInstalled
        {
            get
            {
                var serv = ServiceController.GetServices().FirstOrDefault(v => v.ServiceName == ID);
                if (serv == null)
                    return false;
                return true;
            }
        }

        public override string ExecutablePath
        {
            get
            {
                var serviceSKey = GetServiceRegistryRoot();
                var pathObject = serviceSKey.GetValue("ImagePath");
                if (pathObject != null)
                    return pathObject.ToString().Replace("\"", "");

                return null;
            }
        }

        public override ServiceControllerStatus Status
        {
            get
            {
                var srv = ServiceController.GetServices().FirstOrDefault(v => v.ServiceName == ID);

                if (srv != null)
                    return srv.Status;
                throw new Exception("Can't find service name '" + ID + "'");
            }
        }

        public override string Name
        {
            get
            {
                var serviceSKey = GetServiceRegistryRoot();
                var pathObject = serviceSKey.GetValue("DisplayName");
                if (pathObject != null)
                    return pathObject.ToString();

                return null;
            }
        }

        public override string Description
        {
            get
            {
                var serviceSKey = GetServiceRegistryRoot();
                var pathObject = serviceSKey.GetValue("Description");
                if (pathObject != null)
                    return pathObject.ToString();
                return null;
            }
        }

        public override string ConfigPath => "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Services\\" + ID;

        public override IEnumerable<ServiceDaemon> ServicesDepended
        {
            get
            {
                var serviceSKey = GetServiceRegistryRoot();

                var pathObject = (string[]) serviceSKey.GetValue("DependOnService");
                if (pathObject != null)
                    return pathObject.Select(v => new WindowsServiceDaemon(v));

                return new List<WindowsServiceDaemon>();
            }
        }

        private RegistryKey GetServiceRegistryRoot()
        {
            var serviceSKey = Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\services\\" + ID, true);
            if (serviceSKey == null)
                throw new Exception("Don't exists service '" + ID + "'.");
            return serviceSKey;
        }

        public override void Disable()
        {
            scFile.CmdExecuteSync("config " + ID + " start= disabled");
        }

        public override void Enable()
        {
            scFile.CmdExecuteSync("config " + ID + " start= auto");
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
            scFile.CmdExecuteSync("pause " + ID);
        }


        public override void Resume(int timeoutMs)
        {
            scFile.CmdExecuteSync("resume " + ID);
        }

        public override void Start(int timeoutMs)
        {
            scFile.CmdExecuteSync("start " + ID);
        }

        public override void Stop(int timeoutMs)
        {
            scFile.CmdExecuteSync("stop " + ID);
        }
    }
}