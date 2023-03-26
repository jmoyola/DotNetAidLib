using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Proc;
using DotNetAidLib.Services.Core;

namespace DotNetAidLib.Services.Imp
{
    public class SystemVServiceHelper : ServiceHelper
    {
        private static SystemVServiceHelper _Instance;

        private SystemVServiceHelper()
        {
        }

        public new static SystemVServiceHelper Instance()
        {
            if (_Instance == null)
                _Instance = new SystemVServiceHelper();
            return _Instance;
        }

        public override ServiceDaemon Install(string serviceID, string serviceName, string serviceDescription,
            string executablePath, string executableWorkingDirectory, string runAsUser,
            IEnumerable<string> servicesDepended, IEnumerable<ServiceTarget> targets, ServiceRestart restart,
            IEnumerable<string> mountPointsDepended = null)
        {
            Assert.NotNullOrEmpty(serviceID, nameof(serviceID));
            Assert.NotNullOrEmpty(serviceName, nameof(serviceName));
            Assert.NotNullOrEmpty(executablePath, nameof(executablePath));
            Assert.NotNullOrEmpty(executableWorkingDirectory, nameof(executableWorkingDirectory));

            if (serviceDescription == null)
                serviceDescription = serviceID + " service";

            // Carpeta del archivo de id de proceso
            DirectoryInfo lockDirectory;
            lockDirectory = new DirectoryInfo("/var/lock");
            if (!lockDirectory.Exists)
                lockDirectory = new DirectoryInfo("/run/lock");

            var lockFile = new FileInfo(lockDirectory.FullName + Path.DirectorySeparatorChar + serviceID + ".lock");
            var monoFile = EnvironmentHelper.SearchInPath("which").CmdExecuteSync("mono").Replace("\r\n", "")
                .Replace("\n", "");
            var monoServiceFile = EnvironmentHelper.SearchInPath("which").CmdExecuteSync("mono-service")
                .Replace("\r\n", "").Replace("\n", "");

            var fi = new FileInfo("/etc/init.d/" + serviceID);

            if (fi.Exists)
                fi.Delete();

            ServiceTarget[] defaultStopTargets = {ServiceTarget.Halt, ServiceTarget.Reboot, ServiceTarget.SingleUser};
            ServiceTarget[] defaultStartTargets =
                {ServiceTarget.MultiUserNonGraphical, ServiceTarget.MultiUserGraphical};

            if (targets != null)
            {
                defaultStopTargets = defaultStopTargets.In(targets).OrderBy(v => v).ToArray();
                defaultStartTargets = defaultStartTargets.In(targets).ToArray().ToArray();
            }

            var appType = ProcessHelper.GetApplicationType(executablePath);
            if (servicesDepended == null || servicesDepended.Count() == 0)
                servicesDepended = new[] {"$syslog", "$local_fs", "$network", "$remote_fs", "$named", "$time"};
            var sw = fi.CreateText();
            sw.WriteLine("#!/bin/bash");
            sw.WriteLine("### BEGIN INIT INFO");
            sw.WriteLine("# Provides:          " + serviceID);
            sw.WriteLine("# Required-Start:    " + servicesDepended.ToStringJoin(" "));
            sw.WriteLine("# Required-Stop:     $syslog");
            sw.WriteLine("# Default-Start:     2 4 " +
                         defaultStartTargets.Select(v => (int) v).ToStringJoin(" ")); // 2 4 3 5;
            sw.WriteLine("# Default-Stop:      " + defaultStopTargets.Select(v => (int) v).ToStringJoin(" ")); // 0 1 6;
            sw.WriteLine("# Short-Description: " + serviceID);
            sw.WriteLine("# Description:       " + serviceDescription);
            sw.WriteLine("#");
            sw.WriteLine("### END INIT INFO");
            sw.WriteLine();
            sw.WriteLine("daemon_name=" + serviceID);
            sw.WriteLine();
            sw.WriteLine("PATH=/usr/local/sbin:/usr/local/bin:/sbin:/bin:/usr/sbin:/usr/bin");

            if (appType == ApplicationType.NativeAppication)
                sw.WriteLine("DAEMON=" + executablePath);
            else if (appType == ApplicationType.DotNetAppication)
                sw.WriteLine("DAEMON=" + monoFile);
            else if (appType == ApplicationType.DotNetService)
                sw.WriteLine("DAEMON=" + monoServiceFile);

            sw.WriteLine("NAME=" + serviceID);
            sw.WriteLine("DESC=" + serviceDescription);
            sw.WriteLine();
            sw.WriteLine(". /lib/lsb/init-functions");
            sw.WriteLine();
            sw.WriteLine("SERVICE_PID=$(cat " + lockFile.FullName + ")");
            sw.WriteLine();
            sw.WriteLine("case \"$1\" in");
            sw.WriteLine("start)");
            sw.WriteLine("log_daemon_msg \"Starting " + serviceID + "\"");
            sw.WriteLine("if [ -z \"${SERVICE_PID}\" ]; then");

            if (appType == ApplicationType.NativeAppication)
            {
                sw.WriteLine("${DAEMON}");
                sw.WriteLine("echo $! >>" + lockFile.FullName);
            }
            else if (appType == ApplicationType.DotNetAppication)
            {
                sw.WriteLine("${DAEMON}" + " " + executablePath);
                sw.WriteLine("echo $! >>" + lockFile.FullName);
            }
            else if (appType == ApplicationType.DotNetService)
            {
                sw.WriteLine("/bin/rm -f \"" + lockFile.FullName + "\"");
                sw.WriteLine("${DAEMON}" + " -n:" + serviceID + " -m:" + serviceID + " - l:\"" + lockFile.FullName +
                             "\" -d:\"" + executableWorkingDirectory + "\" \"" + executablePath + "\"");
            }

            sw.WriteLine("log_end_msg 0");
            sw.WriteLine("else -l ");
            sw.WriteLine("log_failure_msg \"" + serviceID + " is already running!\"");
            sw.WriteLine("exit 1");
            sw.WriteLine("fi");
            sw.WriteLine(";;");
            sw.WriteLine("stop)");
            sw.WriteLine("log_daemon_msg \"Stopping " + serviceID + "\"");
            sw.WriteLine("if [ -n \"${SERVICE_PID}\" ]; then");
            sw.WriteLine("kill ${SERVICE_PID}");
            sw.WriteLine("rm " + lockFile.FullName);
            sw.WriteLine("log_end_msg 0");
            sw.WriteLine("else");
            sw.WriteLine("log_failure_msg \"" + serviceID + " is not running\"");
            sw.WriteLine("exit 1");
            sw.WriteLine("fi");
            sw.WriteLine(";;");
            sw.WriteLine("restart)");
            sw.WriteLine("$0 stop");
            sw.WriteLine("sleep 1");
            sw.WriteLine("$0 start");
            sw.WriteLine("log_end_msg 0");
            sw.WriteLine(";;");
            sw.WriteLine("*)");
            sw.WriteLine("log_action_msg \"usage: $0 {start|stop|restart}\"");
            sw.WriteLine("esac");
            sw.WriteLine();
            sw.WriteLine("exit 0");
            sw.WriteLine();
            sw.WriteLine("log_end_msg 0");

            sw.Flush();
            sw.Close();

            // Cambiamos permisos a 775
            var chmodFile = EnvironmentHelper.SearchInPath("chmod");
            chmodFile.CmdExecuteSync("775 " + fi.FullName);

            return new SystemVServiceDaemon(serviceID);
        }

        public override void Uninstall(string serviceID)
        {
            var fi = new FileInfo("/etc/init.d/" + serviceID);
            if (fi.Exists)
                fi.Delete();
        }

        public override IEnumerable<ServiceDaemon> GetServices()
        {
            var initd = new DirectoryInfo("/etc/init.d");
            return initd.GetFiles().Select(v => new SystemVServiceDaemon(v.Name));
        }

        public override ServiceDaemon GetService(string serviceID)
        {
            return new SystemVServiceDaemon(serviceID);
        }
    }
}