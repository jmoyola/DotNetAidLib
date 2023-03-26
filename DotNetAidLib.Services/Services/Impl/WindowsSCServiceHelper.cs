using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Services.Core;

namespace DotNetAidLib.Services.Imp
{
    public class WindowsSCServiceHelper : ServiceHelper
    {
        private static FileInfo scFile;
        private static WindowsSCServiceHelper _Instance;

        private WindowsSCServiceHelper()
        {
            scFile = EnvironmentHelper.SearchInPath("sc.exe");
            if (scFile == null || !scFile.Exists)
                throw new ServiceDaemonException("Can't find 'sc.exe' program.");
        }

        public new static WindowsSCServiceHelper Instance()
        {
            if (_Instance == null)
                _Instance = new WindowsSCServiceHelper();
            return _Instance;
        }

        public override ServiceDaemon Install(string serviceID, string serviceName, string serviceDescription,
            string executablePath, string executableWorkingDirectory, string runAsUser,
            IEnumerable<string> servicesDepended, IEnumerable<ServiceTarget> targets, ServiceRestart restart,
            IEnumerable<string> mountPointsDepended = null)
        {
            string args = null;

            args = "create " + serviceID + " binpath= \"" + executablePath + "\" displayname= \"" + serviceName +
                   "\" type= share start= auto error= normal";

            if (servicesDepended != null && servicesDepended.Count() > 0)
                args += " depend= " + servicesDepended.ToStringJoin(" ");

            if (!string.IsNullOrEmpty(runAsUser))
            {
                var userPassword = runAsUser.Split(':');
                args += " obj= " + userPassword[0].Trim();
                if (userPassword.Length > 1)
                    args += " password= " + userPassword[1].Trim();
            }

            scFile.CmdExecuteSync(args);

            // Descripcion
            args = "description " + serviceID + " " + serviceDescription;
            scFile.CmdExecuteSync(args);

            // Restart
            if (restart == ServiceRestart.On_failure)
            {
                args = "failure " + serviceID + " reset= 60 actions= restart/5000/restart/10000/restart/30000";
                scFile.CmdExecuteSync(args);
            }

            return new WindowsServiceDaemon(serviceID);
        }

        public override void Uninstall(string serviceID)
        {
            string args = null;

            args = "delete " + serviceID;
            scFile.CmdExecuteSync(args);
        }

        public override IEnumerable<ServiceDaemon> GetServices()
        {
            return ServiceController.GetServices().Select(v => new WindowsServiceDaemon(v.ServiceName));
        }

        public override ServiceDaemon GetService(string serviceID)
        {
            return new WindowsServiceDaemon(serviceID);
        }
    }
}