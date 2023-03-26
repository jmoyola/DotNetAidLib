using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Streams;
using DotNetAidLib.Services.Core;

namespace DotNetAidLib.Services.Imp
{
    public class SystemVServiceDaemon : ServiceDaemon
    {
        private static FileInfo updateRCFile;

        public SystemVServiceDaemon(string id)
            : base(id)
        {
            updateRCFile = EnvironmentHelper.SearchInPath("update-rc.d");
            if (updateRCFile == null || updateRCFile.Exists)
                throw new Exception("'update-rc.d' command is missing.");
        }

        public override bool IsServiceInstalled
        {
            get
            {
                var initd = new DirectoryInfo("/etc/init.d");
                var serv = initd.GetFiles(ID).FirstOrDefault();
                if (serv == null)
                    return false;
                return true;
            }
        }


        public override string ExecutablePath
        {
            get
            {
                try
                {
                    if (!IsServiceInstalled) throw new Exception("Don't exists service '" + ID + "'.");

                    var serviceFile = new FileInfo("/etc/init.d/" + ID);
                    var m = serviceFile.OpenText().LineSearch(new Regex("^PIDFILE=(.+)$"), true);
                    if (m.Success)
                        return ResolvExecutablePath(m.Value);
                    return null;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error resuming service '" + ID + "'." + ex.Message, ex);
                }
            }
        }

        public override IEnumerable<ServiceDaemon> ServicesDepended => new List<SystemVServiceDaemon>();

        public override ServiceControllerStatus Status
        {
            get
            {
                var fInitd = new FileInfo("/etc/init.d/" + ID);
                var outCmd = fInitd.CmdExecuteSync("status");
                if (outCmd.IndexOf("active") > 0)
                    return ServiceControllerStatus.Running;
                if (outCmd.IndexOf("inactive") > 0)
                    return ServiceControllerStatus.Stopped;
                return ServiceControllerStatus.Stopped;
            }
        }

        public override string Name => ID;

        public override string Description => ID;

        public override string ConfigPath => "/etc/init.d/" + ID;

        public override void Enable()
        {
            updateRCFile.CmdExecuteSync(ID + " defaults");
        }

        public override void Disable()
        {
            updateRCFile.CmdExecuteSync(ID + " remove");
        }

        public override void Mask()
        {
            updateRCFile.CmdExecuteSync(ID + " disable");
        }

        public override void Unmask()
        {
            updateRCFile.CmdExecuteSync(ID + " enable");
        }

        public override void Start(int timeoutMs)
        {
            try
            {
                if (!IsServiceInstalled) throw new Exception("Don't exists service '" + ID + "'.");

                var serviceFile = new FileInfo("/etc/init.d/" + ID);
                serviceFile.CmdExecuteSync("start");
            }
            catch (Exception ex)
            {
                throw new Exception("Error starting service '" + ID + "'." + ex.Message, ex);
            }
        }


        public override void Stop(int timeoutMs)
        {
            try
            {
                if (!IsServiceInstalled) throw new Exception("Don't exists service '" + ID + "'.");

                var serviceFile = new FileInfo("/etc/init.d/" + ID);
                serviceFile.CmdExecuteSync("stop");
            }
            catch (Exception ex)
            {
                throw new Exception("Error stopping service '" + ID + "'." + ex.Message, ex);
            }
        }

        public override void Pause(int timeoutMs)
        {
        }

        public override void Resume(int timeoutMs)
        {
        }
    }
}