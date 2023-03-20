using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Logger.Core;
using DotNetAidLib.Core.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Core.Logger.LogEntryInfo{
    public class DriveFreeSpaceLogEntryInfo: ILogEntryInfo
	{
        private IList<DriveInfo> drives = new List<DriveInfo>();
        public DriveFreeSpaceLogEntryInfo(IList<String> driveIDs) {
            if (driveIDs != null)
                driveIDs.ToList().ForEach(v=>this.drives.Add(new DriveInfo(v)));
        }
        
        public String Name { get => "Drives Available Space"; }
        public String ShortName { get => "DRVAVA"; }
        
        public String GetInfo(LogEntry logEntry) {

            return this.drives.Select(v => (v.Name + (v.AvailableFreeSpace / 1024 / 1024))).ToStringJoin("/");
        }

        public void InitConfiguration(IApplicationConfigGroup cfgGroup)
        {
        }
        public void SaveConfiguration(IApplicationConfigGroup configGroup)
        {
        }
    }
}