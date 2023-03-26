using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Process.ProcessInfo.Core;
using DotNetAidLib.Core.Streams;

namespace DotNetAidLib.Core.Process.ProcessInfo.Imp
{
    public class WindowsProcessInfo : Core.ProcessInfo
    {
        private static readonly FileInfo windowsKillFi = EnvironmentHelper.SearchInPath("windows-kill.exe");
        private static FileInfo taskListFi = EnvironmentHelper.SearchInPath("tasklist.exe");
        private static FileInfo taskKillFi = EnvironmentHelper.SearchInPath("taskkill.exe");
        private static Regex psRegex = new Regex(@"");

        public WindowsProcessInfo(int processId)
            : base(processId)
        {
            Assert.Exists(taskKillFi, nameof(taskListFi));
            Assert.Exists(taskKillFi, nameof(taskKillFi));
        }

        private static bool ParseTLProcessLine(IList<object> csvLine, WindowsProcessInfo process)
        {
            try
            {
                if (csvLine == null || csvLine.Count < 8)
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

                var sdate = DateTime.MinValue;
                var stime = TimeSpan.Zero;

                process.isRunning = true;
                process.name = csvLine[0].ToString().Trim();
                process.pID = int.Parse(csvLine[1].ToString().Trim());
                process.parentPID = int.Parse(csvLine[2].ToString().Trim());
                process.user = csvLine[3].ToString().Trim();
                process.cpu = decimal.Parse(csvLine[4].ToString().Trim(), CultureInfo.InvariantCulture.NumberFormat);
                process.memory = decimal.Parse(csvLine[5].ToString().Trim(), CultureInfo.InvariantCulture.NumberFormat);

                if (DateTime.TryParseExact(csvLine[6].ToString().Trim(),
                        new[] {"yyyy", "MMMdd", "HH:mm:ss"}, CultureInfo.InvariantCulture.DateTimeFormat,
                        DateTimeStyles.AssumeLocal, out sdate))
                    process.startTime = sdate;

                if (TimeSpan.TryParseExact(csvLine[7].ToString().Trim(),
                        new[] {@"mm\:ss", @"hh\:mm\:ss", @"d\-hh\:mm\:ss"}, CultureInfo.InvariantCulture.DateTimeFormat,
                        out stime))
                    process.useTime = stime;

                process.command = csvLine[8].ToString().Trim();

                return true;
            }
            catch (Exception ex)
            {
                throw new ProcessException(
                    "Error parsing info proccess for line '" + (csvLine == null ? "NULL" : csvLine.ToStringJoin(",")) +
                    "'", ex);
            }
        }

        public override Core.ProcessInfo Refresh()
        {
            StreamReader sr = null;

            try
            {
                var ret = new List<Core.ProcessInfo>();

                var psRet = taskListFi.CmdExecuteSync("/V / NH / FO CSV / FI \"PID eq " + pID + "\"");
                sr = psRet.ToStreamReader();
                var record = sr.ReadAllRecords(
                    new RecordsParserOptions(),
                    true).FirstOrDefault();
                ParseTLProcessLine(record, this);

                return this;
            }
            catch (Exception ex)
            {
                throw new ProcessException("Error in refresh infor for proccess id '" + pID + "'", ex);
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
        }

        public override void Send(int signal)
        {
            try
            {
                if (signal == ProcessSignals.SIGTERM || signal == ProcessSignals.SIGKILL)
                {
                    taskKillFi.CmdExecuteSync((signal == ProcessSignals.SIGKILL ? "/F " : "") + "/PID " + pID);
                }
                else
                {
                    if (!windowsKillFi.Exists)
                        throw new ProcessException("Command windows-kill.exe is missing.");

                    windowsKillFi.CmdExecuteSync("-" + signal + " " + pID);
                }
            }
            catch (Exception ex)
            {
                throw new ProcessException("Error sending signal '" + signal + "' to process id '" + pID + "'", ex);
            }
        }

        public new static Core.ProcessInfo[] GetProcessesFromName(string processName)
        {
            // tasklist /V /NH /FO CSV /FI "ImageName eq mysqld.exe"
            StreamReader sr = null;

            try
            {
                var ret = new List<Core.ProcessInfo>();

                var psRet = taskListFi.CmdExecuteSync("/V / NH / FO CSV / FI \"ImageName eq " + processName + "\"");
                sr = psRet.ToStreamReader();
                foreach (var record in sr.ReadAllRecords(
                             new RecordsParserOptions(), true))
                {
                    var pp = new WindowsProcessInfo(0);
                    if (ParseTLProcessLine(record, pp))
                        ret.Add(pp);
                }

                return ret.ToArray();
            }
            catch (Exception ex)
            {
                throw new ProcessException("Error getting processes from name '" + processName + "'", ex);
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
        }

        public new static Core.ProcessInfo GetProcess(int processId)
        {
            // tasklist /V /NH /FO CSV /FI "PID eq 6920"
            StreamReader sr = null;

            WindowsProcessInfo ret = null;
            try
            {
                var psRet = taskListFi.CmdExecuteSync("/V / NH / FO CSV / FI \"PID eq " + processId + "\"");

                sr = psRet.ToStreamReader();
                var record = sr.ReadAllRecords(
                    new RecordsParserOptions(), true).FirstOrDefault();
                if (record != null)
                {
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