using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Process.ServiceDaemon.Core;
using DotNetAidLib.Core.Streams;

namespace DotNetAidLib.Core.Process.ServiceDaemon.Imp
{

	public class SystemVServiceDaemon:DotNetAidLib.Core.Process.ServiceDaemon.Core.ServiceDaemon
	{
		private static FileInfo updateRCFile = null;

		public SystemVServiceDaemon(String id)
        :base(id)
        { 
			updateRCFile = EnvironmentHelper.SearchInPath("update-rc.d");
			if (updateRCFile == null || updateRCFile.Exists)
				throw new Exception("'update-rc.d' command is missing.");
		}

		public override void Enable(){
			updateRCFile.CmdExecuteSync(ID + " defaults");
		}

		public override void Disable(){
			updateRCFile.CmdExecuteSync (ID + " remove");
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
			try{
				if(!IsServiceInstalled){
					throw new Exception("Don't exists service '" + ID + "'.");
				} else {
					FileInfo serviceFile = new FileInfo ("/etc/init.d/" + ID);
					serviceFile.CmdExecuteSync ("start");
				}
			}
			catch(Exception ex){
				throw new Exception("Error starting service '" + ID + "'." + ex.Message, ex);
			}
		}


		public override void Stop(int timeoutMs)
		{
			try{
				if(!IsServiceInstalled){
					throw new Exception("Don't exists service '" + ID + "'.");
				} else {
					FileInfo serviceFile = new FileInfo ("/etc/init.d/" + ID);
					serviceFile.CmdExecuteSync ("stop");
				}
			}
			catch(Exception ex){
				throw new Exception("Error stopping service '" + ID + "'." + ex.Message, ex);
			}
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
                DirectoryInfo initd = new DirectoryInfo("/etc/init.d");
                FileInfo serv = initd.GetFiles(ID).FirstOrDefault();
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



		public override String ExecutablePath
		{
            get
            {
                try
                {
                    if (!IsServiceInstalled)
                    {
                        throw new Exception("Don't exists service '" + ID + "'.");
                    }
                    else
                    {
                        FileInfo serviceFile = new FileInfo("/etc/init.d/" + ID);
                        Match m = serviceFile.OpenText().LineSearch(new Regex("^PIDFILE=(.+)$"), true);
                        if (m.Success)
                            return ResolvExecutablePath(m.Value);
                        else
                            return null;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error resuming service '" + ID + "'." + ex.Message, ex);
                }
            }
		}

        public override IEnumerable<DotNetAidLib.Core.Process.ServiceDaemon.Core.ServiceDaemon> ServicesDepended
        {
            get
            {
                return new List<SystemVServiceDaemon>();
            }
        }

        public override ServiceControllerStatus Status{
            get
            {
                FileInfo fInitd = new FileInfo("/etc/init.d/" + ID);
                String outCmd = fInitd.CmdExecuteSync("status");
                if (outCmd.IndexOf("active") > 0)
                    return ServiceControllerStatus.Running;
                else if (outCmd.IndexOf("inactive") > 0)
                    return ServiceControllerStatus.Stopped;
                else
                    return ServiceControllerStatus.Stopped;
            }
		}

        public override string Name
        {
            get
            {
                return this.ID;
            }
        }

        public override string Description
        {
            get
            {
                return this.ID;
            }
        }

        public override string ConfigPath
        {
            get
            {
                return "/etc/init.d/" + this.ID;
            }
        }
    }
}

