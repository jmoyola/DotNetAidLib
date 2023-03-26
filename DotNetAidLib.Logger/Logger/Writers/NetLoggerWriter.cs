using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Logger.Core;

namespace DotNetAidLib.Logger.Writers
{
    public abstract class NetLoggerWriter : LoggerWriter
    {
        public NetLoggerWriter()
        {
            MaxLogPriorityVerboseInDebugTime = LogPriorityLevels._ERROR;
            MaxLogPriorityVerboseInRunTime = LogPriorityLevels._ERROR;
        }

        public NetLoggerWriter(string server, int port) : this()
        {
            Server = server;
            Port = port;
        }

        public string Server { get; set; }

        public int Port { get; set; }

        public override void InitConfiguration(IApplicationConfigGroup configGroup)
        {
            base.InitConfiguration(configGroup);

            if (configGroup.ConfigurationExist("Server"))
                Server = configGroup.GetConfiguration<string>("Server").Value;
            else
                configGroup.AddConfiguration("Server", Server, true);

            if (configGroup.ConfigurationExist("Port"))
                Port = configGroup.GetConfiguration<int>("Port").Value;
            else
                configGroup.AddConfiguration("Port", Port, true);
        }

        public override void SaveConfiguration(IApplicationConfigGroup configGroup)
        {
            base.SaveConfiguration(configGroup);

            configGroup.AddConfiguration("Server", Server, true);

            configGroup.AddConfiguration("Port", Port, true);
        }


        ~NetLoggerWriter()
        {
        }

        public override string ToString()
        {
            return GetType().Name + ": " + Server + ":" + Port;
        }
    }
}