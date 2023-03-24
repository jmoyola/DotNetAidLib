using System;
using DotNetAidLib.Core.Process.ProcessInfo.Core;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Streams;

namespace DotNetAidLib.Core.Process.ProcessInfo.Imp
{
    public class WindowsProcessInfo: ProcessInfo.Core.ProcessInfo
    {
        private static FileInfo windowsKillFi = EnvironmentHelper.SearchInPath("windows-kill.exe");
        private static FileInfo taskListFi = EnvironmentHelper.SearchInPath("tasklist.exe");
        private static FileInfo taskKillFi = EnvironmentHelper.SearchInPath("taskkill.exe");
        private static Regex psRegex = new Regex(@"");

        public WindowsProcessInfo(int processId)
            :base(processId){
            Assert.Exists(taskKillFi, nameof(taskListFi));
            Assert.Exists(taskKillFi, nameof(taskKillFi));
        }

        private static bool ParseTLProcessLine(IList<Object> csvLine, Imp.WindowsProcessInfo process)
        {
            try
            {

                if (csvLine==null || csvLine.Count < 8)
                {
                    process.isRunning = false;
                    process.name = null;
                    process.pID = 0;
                    process.parentPID = 0;
                    process.user = null;
                    process.cpu = 0;
                    process.memory = 0;
                    process.startTime = DateTime.MinValue;
                    process.useTime = TimeSpan.Zero;
                    process.command = null;

                    return true;
                }
                else
                {
                    DateTime sdate = DateTime.MinValue;
                    TimeSpan stime = TimeSpan.Zero;

                    process.isRunning = true;
                    process.name = csvLine[0].ToString().Trim();
                    process.pID = Int32.Parse(csvLine[1].ToString().Trim());
                    process.parentPID = Int32.Parse(csvLine[2].ToString().Trim());
                    process.user = csvLine[3].ToString().Trim();
                    process.cpu = Decimal.Parse(csvLine[4].ToString().Trim(), CultureInfo.InvariantCulture.NumberFormat);
                    process.memory = Decimal.Parse(csvLine[5].ToString().Trim(), CultureInfo.InvariantCulture.NumberFormat);

                    if (DateTime.TryParseExact(csvLine[6].ToString().Trim(),
                        new string[] { "yyyy", "MMMdd", "HH:mm:ss" }, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeLocal, out sdate))
                        process.startTime = sdate;

                    if (TimeSpan.TryParseExact(csvLine[7].ToString().Trim(),
                        new string[] { @"mm\:ss", @"hh\:mm\:ss", @"d\-hh\:mm\:ss" }, CultureInfo.InvariantCulture.DateTimeFormat, out stime))
                        process.useTime = stime;

                    process.command = csvLine[8].ToString().Trim();

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new ProcessException("Error parsing info proccess for line '" + (csvLine == null ? "NULL" : csvLine.ToStringJoin(",")) + "'", ex);
            }
        }

        public override ProcessInfo.Core.ProcessInfo Refresh() {
            StreamReader sr = null;

            try
            {
                List<ProcessInfo.Core.ProcessInfo> ret = new List<ProcessInfo.Core.ProcessInfo>();

                String psRet = taskListFi.CmdExecuteSync("/V / NH / FO CSV / FI \"PID eq " + this.pID + "\"");
                sr = psRet.ToStreamReader();
                IList<Object> record = sr.ReadAllRecords(
                    new RecordsParserOptions() {

                    },
                    true).FirstOrDefault();
                ParseTLProcessLine(record, this);

                return this;
            }
            catch (Exception ex)
            {
                throw new ProcessException("Error in refresh infor for proccess id '" + this.pID + "'", ex);
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
        }

        public override void Send(int signal) {
            try {
                if (signal == ProcessSignals.SIGTERM || signal == ProcessSignals.SIGKILL)
                {
                    taskKillFi.CmdExecuteSync((signal == ProcessSignals.SIGKILL ? "/F " : "") + "/PID " + this.pID);
                }
                else
                {
                    if (!windowsKillFi.Exists)
                        throw new ProcessException("Command windows-kill.exe is missing.");

                    windowsKillFi.CmdExecuteSync("-" + signal + " " + this.pID);
                }
            }
            catch(Exception ex) {
                throw new ProcessException("Error sending signal '" + signal + "' to process id '" + this.pID + "'", ex);
            }
        }

        public static new ProcessInfo.Core.ProcessInfo[] GetProcessesFromName(String processName)
        {
            // tasklist /V /NH /FO CSV /FI "ImageName eq mysqld.exe"
            StreamReader sr =null;

            try
            {
                List<ProcessInfo.Core.ProcessInfo> ret = new List<ProcessInfo.Core.ProcessInfo>();

                String psRet = taskListFi.CmdExecuteSync("/V / NH / FO CSV / FI \"ImageName eq " + processName + "\"");
                sr = psRet.ToStreamReader();
                foreach (IList<Object> record in sr.ReadAllRecords(
                    new RecordsParserOptions() { }
                    ,true)) {
                    WindowsProcessInfo pp = new WindowsProcessInfo(0);
                    if (ParseTLProcessLine(record, pp))
                        ret.Add(pp);
                }

                return ret.ToArray();
            }
            catch (Exception ex)
            {
                throw new ProcessException("Error getting processes from name '" + processName + "'", ex);
            }
            finally {
                if (sr != null)
                    sr.Close();
            }
        }

        public static new ProcessInfo.Core.ProcessInfo GetProcess(int processId)
        {
            // tasklist /V /NH /FO CSV /FI "PID eq 6920"
            StreamReader sr = null;

            WindowsProcessInfo ret = null;
            try
            {
                String psRet = taskListFi.CmdExecuteSync("/V / NH / FO CSV / FI \"PID eq " + processId + "\"");

                sr = psRet.ToStreamReader();
                IList<Object> record = sr.ReadAllRecords(
                    new RecordsParserOptions() { }
                    , true).FirstOrDefault();
                if(record!=null){
                    ret = new WindowsProcessInfo(0);
                    ParseTLProcessLine(record, ret);
                }

                return ret;
            }
            catch
            {
                ret = null;
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }

            return ret;
        }

    }
}
