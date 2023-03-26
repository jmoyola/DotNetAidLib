using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Logger.Core;

namespace DotNetAidLib.Logger.LogEntryInfo
{
    public class DriveFreeSpaceLogEntryInfo : ILogEntryInfo
    {
        private readonly IList<DriveInfo> drives = new List<DriveInfo>();

        public DriveFreeSpaceLogEntryInfo(IList<string> driveIDs)
        {
            if (driveIDs != null)
                driveIDs.ToList().ForEach(v => drives.Add(new DriveInfo(v)));
        }

        public string Name => "Drives Available Space";
        public string ShortName => "DRVAVA";

        public string GetInfo(LogEntry logEntry)
        {
            return drives.Select(v => v.Name + v.AvailableFreeSpace / 1024 / 1024).ToStringJoin("/");
        }

        public void InitConfiguration(IApplicationConfigGroup cfgGroup)
        {
        }

        public void SaveConfiguration(IApplicationConfigGroup configGroup)
        {
        }
    }
}