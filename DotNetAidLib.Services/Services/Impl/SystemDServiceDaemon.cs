using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using DotNetAidLib.Core.Configuration;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Services.Core;

namespace DotNetAidLib.Services.Imp
{
    public class SystemDServiceDaemon : ServiceDaemon
    {
        private static FileInfo systemctlFile;

        public SystemDServiceDaemon(string id)
            : base(id)
        {
            systemctlFile = EnvironmentHelper.SearchInPath("systemctl");
            if (systemctlFile == null || !systemctlFile.Exists)
                throw new Exception("'systemctlFile' command is missing.");
        }

        public override bool IsServiceInstalled
        {
            get { return SystemDServiceHelper.Instance().GetServices().Any(v => v.Name == ID); }
        }

        public override string ConfigPath
        {
            get
            {
                string ret = null;
                var f = SystemDServiceHelper.SystemUnitsDirectory.Files(ID + ".service").FirstOrDefault();
                if (f != null)
                    ret = f.FullName;
                return ret;
            }
        }

        public override string ExecutablePath
        {
            get
            {
                var fi = new FileInfo(ConfigPath);

                var ini = IniXConfigurationFile.Instance(fi);
                ini.Load();
                return ResolvExecutablePath(ini.GetGroup("Service")["ExecStart"]);
            }
        }

        public override IEnumerable<ServiceDaemon> ServicesDepended
        {
            get
            {
                var fi = new FileInfo(ConfigPath);

                var ini = IniXConfigurationFile.Instance(fi);
                ini.Load();
                if (ini.GetGroup("Service").ContainsKey("ExecStart"))
                    return ini.GetGroup("Service")["ExecStart"].Split(' ').Select(v => new SystemDServiceDaemon(v));
                return new List<SystemDServiceDaemon>();
            }
        }

        public override string Name => ID;

        public override string Description
        {
            get
            {
                var fi = new FileInfo(ConfigPath);

                var ini = IniXConfigurationFile.Instance(fi);
                ini.Load();
                if (ini.GetGroup("Unit").ContainsKey("Description"))
                    return ini.GetGroup("Unit")["Description"];
                return null;
            }
        }

        public override ServiceControllerStatus Status
        {
            get
            {
                var res = systemctlFile.CmdExecuteSync("--no-pager status " + ID + ".service", -1, true);

                if (res.IndexOf("Active: active", StringComparison.InvariantCultureIgnoreCase) > -1)
                    return ServiceControllerStatus.Running;
                return ServiceControllerStatus.Stopped;
            }
        }

        public override string StatusDescription
        {
            get
            {
                var res = systemctlFile.CmdExecuteSync("--no-pager status " + ID + ".service", -1, true);
                var iDescription = res.IndexOf("\n\n", StringComparison.InvariantCulture);

                if (iDescription == -1)
                    return null;
                return res.Substring(iDescription + 2);
            }
        }

        public override void Enable()
        {
            systemctlFile.CmdExecuteSync("enable " + ID + ".service");
        }

        public override void Disable()
        {
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
    }
}