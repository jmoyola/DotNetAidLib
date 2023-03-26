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

namespace DotNetAidLib.Core.Process.ProcessInfo.Imp
{
    public class PosixProcessInfo : Core.ProcessInfo
    {
        private static FileInfo psFi = EnvironmentHelper.SearchInPath("ps");
        private static FileInfo killFi = EnvironmentHelper.SearchInPath("kill");
        private static Regex psRegex = new Regex(@"");

        public PosixProcessInfo(int processId)
            : base(processId)
        {
            Assert.Exists(killFi, nameof(killFi));
            Assert.Exists(psFi, nameof(psFi));
        }

        public override Core.ProcessInfo Refresh()
        {
            try
            {
                var psRet = psFi.CmdExecuteSync("-ww -p " + pID +
                                                " -o comm:64 -o ,%p,%P, -o user:64 -o ,%C, -o %mem -o ,%x,%t,%a");
                var m = psRet.Split(',');

                ParsePSProcessLine(m, this);

                return this;
            }
            catch (Exception ex)
            {
                throw new ProcessException("Error in refresh info for proccess id '" + pID + "'", ex);
            }
        }

        private static bool ParsePSProcessLine(string[] csvLine, PosixProcessInfo process)
        {
            try
            {
                if (csvLine.Length < 8)
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
                process.name = csvLine[0].Trim();
                process.pID = int.Parse(csvLine[1].Trim());
                process.parentPID = int.Parse(csvLine[2].Trim());
                process.user = csvLine[3].Trim();
                process.cpu = decimal.Parse(csvLine[4].Trim(), CultureInfo.InvariantCulture.NumberFormat);
                process.memory = decimal.Parse(csvLine[5].Trim(), CultureInfo.InvariantCulture.NumberFormat);

                if (DateTime.TryParseExact(csvLine[6].Trim(),
                        new[] {"yyyy", "MMMdd", "HH:mm:ss"}, CultureInfo.InvariantCulture.DateTimeFormat,
                        DateTimeStyles.AssumeLocal, out sdate))
                    process.startTime = sdate;

                if (TimeSpan.TryParseExact(csvLine[7].Trim(),
                        new[] {@"mm\:ss", @"hh\:mm\:ss", @"d\-hh\:mm\:ss"}, CultureInfo.InvariantCulture.DateTimeFormat,
                        out stime))
                    process.useTime = stime;

                process.command = csvLine[8].Trim();

                return true;
            }
            catch (Exception ex)
            {
                throw new ProcessException(
                    "Error parsing info proccess for line '" + (csvLine == null ? "NULL" : csvLine.ToStringJoin(",")) +
                    "'", ex);
            }
        }

        public override void Send(int signal)
        {
            try
            {
                killFi.CmdExecuteSync("-" + signal + " " + pID);
            }
            catch (Exception ex)
            {
                throw new ProcessException("Error sending signal '" + signal + "' to process id '" + pID + "'", ex);
            }
        }

        public new static Core.ProcessInfo[] GetProcessesFromName(string processName)
        {
            try
            {
                var ret = new List<Core.ProcessInfo>();

                var psRet = psFi.CmdExecuteSync("-ww -C " + processName +
                                                " -o comm:64 -o ,%p,%P, -o user:64 -o ,%C, -o %mem -o ,%x,%t,%a");
                foreach (var csvLine in psRet.GetLines().Skip(1))
                {
                    var m = csvLine.Split(',');
                    var pp = new PosixProcessInfo(0);
                    if (ParsePSProcessLine(m, pp))
                        ret.Add(pp);
                }

                return ret.ToArray();
            }
            catch (Exception ex)
            {
                throw new ProcessException("Error getting processes from name '" + processName + "'", ex);
            }
        }

        public new static Core.ProcessInfo GetProcess(int processId)
        {
            PosixProcessInfo ret = null;
            try
            {
                var psRet = psFi.CmdExecuteSync("-ww -p " + processId +
                                                " -o comm:64 -o ,%p,%P, -o user:64 -o ,%C, -o %mem -o ,%x,%t,%a");
                var csvLine = psRet.GetLines().Skip(1).FirstOrDefault();

                var m = csvLine.Split(',');
                ret = new PosixProcessInfo(0);
                if (!ParsePSProcessLine(m, ret))
                    ret = null;
            }
            catch
            {
                ret = null;
            }

            return ret;
        }
    }
}