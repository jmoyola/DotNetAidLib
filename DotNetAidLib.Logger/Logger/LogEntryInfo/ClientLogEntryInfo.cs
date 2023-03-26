using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Logger.Core;

namespace DotNetAidLib.Logger.LogEntryInfo
{
    public class ClientLogEntryInfo : ILogEntryInfo
    {
        public ClientLogEntryInfo(string clientInfo)
        {
            ClientInfo = clientInfo;
        }

        public string ClientInfo { get; set; }

        public string Name => "Client Info";
        public string ShortName => "CINFO";

        public string GetInfo(LogEntry logEntry)
        {
            return ClientInfo;
        }

        public void InitConfiguration(IApplicationConfigGroup cfgGroup)
        {
            if (ClientInfo == null)
            {
                if (cfgGroup.ConfigurationExist("clientInfo"))
                    ClientInfo = cfgGroup.GetConfiguration<string>("clientInfo").Value;
            }
            else
            {
                cfgGroup.AddConfiguration("clientInfo", ClientInfo, true);
            }
        }

        public void SaveConfiguration(IApplicationConfigGroup configGroup)
        {
            configGroup.AddConfiguration("clientInfo", ClientInfo, true);
        }
    }
}