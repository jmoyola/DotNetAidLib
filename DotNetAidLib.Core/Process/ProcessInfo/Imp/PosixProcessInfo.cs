using System;
using DotNetAidLib.Core.Process.ProcessInfo.Core;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.Process.ProcessInfo.Imp
{
    public class PosixProcessInfo: ProcessInfo.Core.ProcessInfo
    {
        private static FileInfo psFi = EnvironmentHelper.SearchInPath("ps");
        private static FileInfo killFi = EnvironmentHelper.SearchInPath("kill");
        private static Regex psRegex = new Regex(@"");

        public PosixProcessInfo(int processId)
            :base(processId){
            Assert.Exists(killFi, nameof(killFi));
            Assert.Exists(psFi, nameof(psFi));
        }

        public override ProcessInfo.Core.ProcessInfo Refresh() {
            try
            {
                String psRet=psFi.CmdExecuteSync("-ww -p " + this.pID + " -o comm:64 -o ,%p,%P, -o user:64 -o ,%C, -o %mem -o ,%x,%t,%a");
                String[] m = psRet.Split(',');

                ParsePSProcessLine(m, this);

                return this;
            }
            catch (Exception ex)
            {
                throw new ProcessException("Error in refresh info for proccess id '" + this.pID + "'", ex);
            }
        }

        private static bool ParsePSProcessLine(String[] csvLine, Imp.PosixProcessInfo process)
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
                else
                {
                    DateTime sdate = DateTime.MinValue;
                    TimeSpan stime = TimeSpan.Zero;

                    process.isRunning = true;
                    process.name = csvLine[0].Trim();
                    process.pID = Int32.Parse(csvLine[1].Trim());
                    process.parentPID = Int32.Parse(csvLine[2].Trim());
                    process.user = csvLine[3].Trim();
                    process.cpu = Decimal.Parse(csvLine[4].Trim(), CultureInfo.InvariantCulture.NumberFormat);
                    process.memory = Decimal.Parse(csvLine[5].Trim(), CultureInfo.InvariantCulture.NumberFormat);

                    if(DateTime.TryParseExact(csvLine[6].Trim(),
                        new string[] { "yyyy", "MMMdd", "HH:mm:ss" }, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeLocal, out sdate))
                        process.startTime = sdate;

                    if(TimeSpan.TryParseExact(csvLine[7].Trim(),
                        new string[] { @"mm\:ss", @"hh\:mm\:ss", @"d\-hh\:mm\:ss" }, CultureInfo.InvariantCulture.DateTimeFormat, out stime))
                        process.useTime = stime;

                    process.command = csvLine[8].Trim();

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new ProcessException("Error parsing info proccess for line '" + (csvLine==null?"NULL":csvLine.ToStringJoin(",")) + "'", ex);
            }
        }

        public override void Send(int signal) {
            try {
                killFi.CmdExecuteSync("-" + signal + " " + this.pID);
            }
            catch(Exception ex) {
                throw new ProcessException("Error sending signal '" + signal + "' to process id '" + this.pID + "'", ex);
            }
        }

        public static new ProcessInfo.Core.ProcessInfo[] GetProcessesFromName(String processName) {
            try {
                List<ProcessInfo.Core.ProcessInfo> ret = new List<ProcessInfo.Core.ProcessInfo>();

                String psRet = psFi.CmdExecuteSync("-ww -C " + processName + " -o comm:64 -o ,%p,%P, -o user:64 -o ,%C, -o %mem -o ,%x,%t,%a");
                foreach (String csvLine in psRet.GetLines().Skip(1))
                {
                    String[] m = csvLine.Split(',');
                    PosixProcessInfo pp = new PosixProcessInfo(0);
                    if (ParsePSProcessLine(m, pp))
                        ret.Add(pp);
                }

                return ret.ToArray();
            }
            catch(Exception ex) {
                throw new ProcessException("Error getting processes from name '" + processName + "'", ex);
            }
        }

        public static new ProcessInfo.Core.ProcessInfo GetProcess(int processId){
            PosixProcessInfo ret = null;
            try {


                String psRet = psFi.CmdExecuteSync("-ww -p " + processId + " -o comm:64 -o ,%p,%P, -o user:64 -o ,%C, -o %mem -o ,%x,%t,%a");
                String csvLine = psRet.GetLines().Skip(1).FirstOrDefault();

                String[] m = csvLine.Split(',');
                ret = new PosixProcessInfo(0);
                if (!ParsePSProcessLine(m, ret))
                    ret=null;
            }
            catch {
                ret = null;
            }

            return ret;
        }

    }
}
