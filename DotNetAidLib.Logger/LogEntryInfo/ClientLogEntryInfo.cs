using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using DotNetAidLib.Logger.Core;
using DotNetAidLib.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Logger.LogEntryInfo{
    public class ClientLogEntryInfo: ILogEntryInfo{
        private string _ClientInfo = null;

        public String Name { get => "Client Info"; }
        public String ShortName { get => "CINFO"; }
        
        public ClientLogEntryInfo(string clientInfo){
            this._ClientInfo = clientInfo;
        }

        public String GetInfo(LogEntry logEntry) {
            return _ClientInfo;
        }

        public void InitConfiguration(IApplicationConfigGroup cfgGroup)
        {
            if (this._ClientInfo == null)
            {
                if (cfgGroup.ConfigurationExist("clientInfo"))
                {
                    _ClientInfo = cfgGroup.GetConfiguration<string>("clientInfo").Value;
                }
            }
            else{
                cfgGroup.AddConfiguration<string>("clientInfo", _ClientInfo, true);
            }
        }

        public void SaveConfiguration(IApplicationConfigGroup configGroup)
        {
            configGroup.AddConfiguration<string>("clientInfo", _ClientInfo, true);
        }

        public string ClientInfo
        {
            get
            {
                return _ClientInfo;
            }

            set
            {
                _ClientInfo = value;
            }
        }


    }
}