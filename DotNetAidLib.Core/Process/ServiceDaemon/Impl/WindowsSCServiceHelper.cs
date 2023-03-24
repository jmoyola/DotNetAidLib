using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using Microsoft.Win32;
using DotNetAidLib.Core.Process.ServiceDaemon.Core;

namespace DotNetAidLib.Core.Process.ServiceDaemon.Imp
{

	public class WindowsSCServiceHelper:ServiceHelper
	{
        private static FileInfo scFile = null;
        private static WindowsSCServiceHelper _Instance = null;

		public static new WindowsSCServiceHelper Instance()
		{
			if (_Instance == null)
				_Instance = new WindowsSCServiceHelper();
			return _Instance;
		}

		private WindowsSCServiceHelper() {
            scFile = EnvironmentHelper.SearchInPath("sc.exe");
            if (scFile == null || !scFile.Exists)
                throw new DotNetAidLib.Core.Process.ServiceDaemon.Core.ServiceDaemonException("Can't find 'sc.exe' program.");
        }

        public override DotNetAidLib.Core.Process.ServiceDaemon.Core.ServiceDaemon Install (String serviceID, String serviceName, String serviceDescription, String executablePath, String executableWorkingDirectory, String runAsUser, IEnumerable<String> servicesDepended, IEnumerable<ServiceTarget> targets, ServiceRestart restart, IEnumerable<String> mountPointsDepended = null)
        {
            String args = null;

            args = "create " + serviceID + " binpath= \"" + executablePath + "\" displayname= \"" + serviceName + "\" type= share start= auto error= normal";

            if(servicesDepended!=null && servicesDepended.Count()>0)
                args += " depend= " + servicesDepended.ToStringJoin(" ");

            if (!String.IsNullOrEmpty (runAsUser)) {
                String[] userPassword = runAsUser.Split (':');
                args += " obj= " + userPassword[0].Trim();
                if (userPassword.Length>1)
                    args += " password= " + userPassword [1].Trim ();
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

		public override void Uninstall(String serviceID){
            String args = null;

            args = "delete " + serviceID;
            scFile.CmdExecuteSync(args);
        }

        public override IEnumerable<DotNetAidLib.Core.Process.ServiceDaemon.Core.ServiceDaemon> GetServices()
		{
			return ServiceController.GetServices ().Select (v =>new WindowsServiceDaemon(v.ServiceName));
		}

        public override DotNetAidLib.Core.Process.ServiceDaemon.Core.ServiceDaemon GetService(String serviceID)
        {
            return new WindowsServiceDaemon(serviceID);
        }
    }
}

