using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using DotNetAidLib.Core.Configuration;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.Process.ServiceDaemon.Imp
{
    public class SystemDServiceDaemon:DotNetAidLib.Core.Process.ServiceDaemon.Core.ServiceDaemon
	{
        private static FileInfo systemctlFile = null;

		public SystemDServiceDaemon(String id)
        :base(id){
            systemctlFile = EnvironmentHelper.SearchInPath("systemctl");
            if (systemctlFile == null || !systemctlFile.Exists)
                throw new Exception("'systemctlFile' command is missing.");
        }

		public override void Enable(){
			systemctlFile.CmdExecuteSync("enable " + ID + ".service");
		}

		public override void Disable(){
            systemctlFile.CmdExecuteSync("disable " + ID + ".service");
		}

        public override void Mask()
        {
            systemctlFile.CmdExecuteSync("mask " + ID + ".service");
        }

        public override void Unmask()
        {
            systemctlFile.CmdExecuteSync("unmask " + ID + ".service");
        }

        public override void Start(int timeoutMs)
		{
            systemctlFile.CmdExecuteSync("start " + ID + ".service");
		}

		public override void Stop(int timeoutMs)
		{
            systemctlFile.CmdExecuteSync("stop " + ID + ".service");
		}

		public override void Pause(int timeoutMs)
        {
		}

		public override void Resume(int timeoutMs)
        {
		}

        public override bool IsServiceInstalled
        {
            get
            {
                return SystemDServiceHelper.Instance().GetServices().Any(v => v.Name == ID);
            }
		}

        public override String ConfigPath
        {
            get
            {
                String ret = null;
                FileInfo f = SystemDServiceHelper.SystemUnitsDirectory.Files(ID + ".service").FirstOrDefault();
                if (f != null)
                    ret = f.FullName;
                return ret;
            }
        }

        public override String ExecutablePath
		{
            get
            {
                FileInfo fi = new FileInfo(ConfigPath);

                IniXConfigurationFile ini = IniXConfigurationFile.Instance(fi);
                ini.Load();
                return ResolvExecutablePath(ini.GetGroup("Service")["ExecStart"]);
            }
		}

        public override IEnumerable<DotNetAidLib.Core.Process.ServiceDaemon.Core.ServiceDaemon> ServicesDepended
        {
            get
            {
                FileInfo fi = new FileInfo(ConfigPath);

                IniXConfigurationFile ini = IniXConfigurationFile.Instance(fi);
                ini.Load();
                if(ini.GetGroup("Service").ContainsKey("ExecStart"))
                    return ini.GetGroup("Service")["ExecStart"].Split(' ').Select(v=>new SystemDServiceDaemon(v));
                else
                    return new List<SystemDServiceDaemon>();
            }
        }

        public override String Name
        {
            get
            {
                return ID;
            }
        }

        public override String Description
        {
            get
            {
                FileInfo fi = new FileInfo(ConfigPath);

                IniXConfigurationFile ini = IniXConfigurationFile.Instance(fi);
                ini.Load();
                if (ini.GetGroup("Unit").ContainsKey("Description"))
                    return ini.GetGroup("Unit")["Description"];
                else
                    return null;
            }
        }
        public override ServiceControllerStatus Status{
            get
            {
                String res = systemctlFile.CmdExecuteSync("--no-pager status " + ID + ".service", -1, true);

                if (res.IndexOf("Active: active", StringComparison.InvariantCultureIgnoreCase) > -1)
                    return ServiceControllerStatus.Running;
                else
                    return ServiceControllerStatus.Stopped;
            }
		}

        public override String StatusDescription
        {
            get
            {
                String res = systemctlFile.CmdExecuteSync("--no-pager status " + ID + ".service", -1, true);
                int iDescription = res.IndexOf("\n\n", StringComparison.InvariantCulture);

                if (iDescription == -1)
                    return null;
                else
                    return res.Substring(iDescription+2);
            }
        }
    }
}

