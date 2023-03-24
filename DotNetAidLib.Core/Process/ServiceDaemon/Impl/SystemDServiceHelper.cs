using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Configuration;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Proc;
using DotNetAidLib.Core.Process.ServiceDaemon.Core;

namespace DotNetAidLib.Core.Process.ServiceDaemon.Imp
{
    public class SystemDServiceHelper : ServiceHelper
    {
        private static IList<DirectoryInfo> _SystemUnitsDirectory;
        private static FileInfo systemctlFile = null;

        private static SystemDServiceHelper _Instance = null;
        public static new SystemDServiceHelper Instance()
        {
            if (_Instance == null)
                _Instance = new SystemDServiceHelper();
            return _Instance;
        }

        static SystemDServiceHelper(){
            _SystemUnitsDirectory = new List<DirectoryInfo>();

            _SystemUnitsDirectory.Add(new DirectoryInfo("/etc/systemd/system"));

            if (new DirectoryInfo("/usr/lib/systemd/system").Exists
                && new DirectoryInfo("/usr/lib/systemd/system").GetFiles().Length>0)
                _SystemUnitsDirectory.Add(new DirectoryInfo("/usr/lib/systemd/system"));
            else if (new DirectoryInfo("/lib/systemd/system").Exists
                && new DirectoryInfo("/lib/systemd/system").GetFiles().Length>0)
                _SystemUnitsDirectory.Add(new DirectoryInfo("/lib/systemd/system"));
            if (new DirectoryInfo("/run/systemd/system").Exists
                && new DirectoryInfo("/run/systemd/system").GetFiles().Length>0)
                _SystemUnitsDirectory.Add(new DirectoryInfo("/run/systemd/system"));
        }

        private SystemDServiceHelper(){
            systemctlFile = EnvironmentHelper.SearchInPath("systemctl");
            if (systemctlFile == null || !systemctlFile.Exists)
                throw new Exception("'systemctlFile' command is missing.");
        }

        public static DirectoryInfo[] SystemUnitsDirectory {
            get { return _SystemUnitsDirectory.ToArray(); }
        }

        public override DotNetAidLib.Core.Process.ServiceDaemon.Core.ServiceDaemon Install(String serviceID, String serviceName, String serviceDescription, String executablePath, String executableWorkingDirectory, String runAsUser, IEnumerable<String> servicesDepended, IEnumerable<ServiceTarget> targets, ServiceRestart restart, IEnumerable<String> mountPointsDepended = null)
        {
            Assert.NotNullOrEmpty( serviceID, nameof(serviceID));
            Assert.NotNullOrEmpty( serviceName, nameof(serviceName));
            Assert.NotNullOrEmpty( executablePath, nameof(executablePath));
            Assert.NotNullOrEmpty( executableWorkingDirectory, nameof(executableWorkingDirectory));

            if (serviceDescription == null)
				serviceDescription = serviceName + " service";

            // Carpeta del archivo de id de proceso
            DirectoryInfo lockDirectory;
            lockDirectory = new DirectoryInfo ("/var/lock");
            if (!lockDirectory.Exists)
                lockDirectory = new DirectoryInfo("/run/lock");

			FileInfo lockFile = new FileInfo(lockDirectory.FullName + Path.DirectorySeparatorChar + serviceName + ".lock");

			String monoFile =EnvironmentHelper.SearchInPath("which").CmdExecuteSync("mono").Replace("\r\n", "").Replace("\n", "");
            String monoServiceFile = EnvironmentHelper.SearchInPath("which").CmdExecuteSync("mono-service").Replace("\r\n", "").Replace("\n", "");

            // Servicios de los que se depende
            if (servicesDepended == null)
                servicesDepended = new String[] { "network.target" };

            Dictionary<ServiceTarget, String> systemdTargets = new Dictionary<ServiceTarget, string>()
            {
                {ServiceTarget.Halt, "poweroff.target"},
                {ServiceTarget.SingleUser, "rescue.target"},
                {ServiceTarget.MultiUserNonGraphical, "multi-user.target"},
                {ServiceTarget.MultiUserGraphical, "graphical.target"},
                {ServiceTarget.Reboot, "reboot.target"}
            };

            IEnumerable<String> defaultTargets = new List<String> { systemdTargets[ServiceTarget.MultiUserNonGraphical] };
            if (targets != null)
                defaultTargets = targets.Select(v => systemdTargets[v]);
            

            FileInfo fi=new FileInfo(SystemUnitsDirectory[0].FullName + Path.DirectorySeparatorChar + serviceName + ".service");

			if (fi.Exists)
				fi.Delete ();

            ApplicationType appType = ProcessHelper.GetApplicationType(executablePath);

            IniConfigurationFile ini=IniConfigurationFile.Instance(fi);
			ini.AddGroup ("Unit")
				.AddFluent("Description", serviceDescription)
				.AddFluent ("After", servicesDepended.ToStringJoin(" "));
            Dictionary<String, String> serviceGroup = ini.AddGroup("Service");

            if (appType == ApplicationType.NativeAppication)
                serviceGroup.Add("Type", "simple");
            else if (appType == ApplicationType.DotNetAppication)
                serviceGroup.Add("Type", "forking");
            else if (appType == ApplicationType.DotNetService)
                serviceGroup.Add("Type", "forking");

            serviceGroup.Add("TimeoutStopSec", "20");
            serviceGroup.Add("Restart", restart.ToString().Replace("_","-").ToLower());
            serviceGroup.Add("WorkingDirectory", executableWorkingDirectory);

            if(mountPointsDepended!=null && mountPointsDepended.Count()>0)
                serviceGroup.Add("RequiredMountsFor", mountPointsDepended.ToStringJoin(" "));

            if (appType == ApplicationType.NativeAppication)
                serviceGroup.Add("ExecStart", executablePath);
            else if (appType == ApplicationType.DotNetAppication)
                serviceGroup.Add("ExecStart", monoFile + " " + executablePath);
            else if (appType == ApplicationType.DotNetService)
            {
                //serviceGroup.Add("ExecStart", monoServiceFile + " -n:" + serviceID + " -m:" + serviceID + " -d:" + executableWorkingDirectory + " " + executablePath);
                serviceGroup.Add("ExecStartPre", "/bin/rm -f " + lockFile.FullName); // Antes borramos el archivo de bloqueo si existe
                serviceGroup.Add("ExecStart", monoServiceFile + " -n:" + serviceID + " -m:" + serviceID + " -l:" + lockFile.FullName + " -d:" + executableWorkingDirectory + " " + executablePath);
            }
            if(!String.IsNullOrEmpty(runAsUser))
                serviceGroup.Add("User", runAsUser);
            ini.AddGroup ("Install")
				.AddFluent ("WantedBy", defaultTargets.ToStringJoin(" "));

			ini.Save ();

			// Cambiamos permisos a 775
			FileInfo chmodFile = EnvironmentHelper.SearchInPath("chmod");
			chmodFile.CmdExecuteSync ("775 " + ini.File.FullName);

            return new SystemDServiceDaemon(serviceID);
		}

		public override void Uninstall(String serviceID){
            SystemDServiceDaemon serviceDaemon = new SystemDServiceDaemon(serviceID);
            serviceDaemon.Disable();
            foreach (FileInfo fi in GetUnitFile(serviceID))
            {
                if (fi.Exists)
                    fi.Delete();
            }
		}

        public static IList<FileInfo> GetUnitFile(string serviceName)
        {
            IEnumerable<FileInfo> serviceUnits = SystemUnitsDirectory.Files("*.service");
            return serviceUnits.Where(v => v.Name == serviceName+".service").ToList();
        }
 
		public override IEnumerable<DotNetAidLib.Core.Process.ServiceDaemon.Core.ServiceDaemon> GetServices()
		{
            IEnumerable<FileInfo> serviceUnits=SystemUnitsDirectory.Files("*.service");
            List<DotNetAidLib.Core.Process.ServiceDaemon.Core.ServiceDaemon> ret = new List<DotNetAidLib.Core.Process.ServiceDaemon.Core.ServiceDaemon>();

            System.Text.RegularExpressions.Regex regEx = new System.Text.RegularExpressions.Regex(@"([^\s]+)\s+([^\s]+)\s+([^\s]+)\s+([^\s]+)\s+(.*)\n", System.Text.RegularExpressions.RegexOptions.Multiline);
            String sCmd = systemctlFile.CmdExecuteSync("--no-pager --no-legend --type service --system list-units", 5000);
            foreach (System.Text.RegularExpressions.Match m in regEx.Matches(sCmd)) {
                String serviceID = m.Groups[1].Value;
                serviceID=serviceID.Substring(0, serviceID.LastIndexOf(".", StringComparison.InvariantCulture)); // quitamos .service del final
                String serviceInfo = m.Groups[5].Value.Trim();
                ret.Add(new SystemDServiceDaemon(serviceID));
            }

            return ret;
		}

        public override DotNetAidLib.Core.Process.ServiceDaemon.Core.ServiceDaemon GetService(String serviceID)
        {
            return new SystemDServiceDaemon(serviceID);
        }
    }
}

