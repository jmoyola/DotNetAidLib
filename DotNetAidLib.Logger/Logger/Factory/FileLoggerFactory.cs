using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Configuration.ApplicationConfig.Xml;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Logger.Client;
using DotNetAidLib.Logger.Core;
using DotNetAidLib.Logger.Writers;

namespace DotNetAidLib.Logger.Factory
{
    public class FileLoggerFactory
    {
        public static ILogger Instance(FileLoggerWriter fileLoggerWriter, IApplicationConfig applicationConfig)
        {
            ILogger ret;

            if (Client.Logger.InstanceExists(fileLoggerWriter.LogFile.FullName))
            {
                ret = Client.Logger.Instance(fileLoggerWriter.LogFile.FullName);
            }
            else
            {
                var loggerWriters = new LoggerWriters();
                loggerWriters.Add(DebugLoggerWriter.Instance());
                loggerWriters.Add(fileLoggerWriter);
                ret = Client.Logger.Instance(fileLoggerWriter.LogFile.FullName, loggerWriters, applicationConfig);
            }

            return ret;
        }

        public static ILogger Instance()
        {
            return Instance(FileLoggerWriter.Instance());
        }

        public static ILogger Instance(FileLoggerWriter fileLoggerWriter)
        {
            IApplicationConfig cfg = null;
            cfg = XmlApplicationConfigFactory.Instance(FileLocation.UserApplicationDataFolder);
            return Instance(fileLoggerWriter, cfg);
        }

        public static ILogger Instance(IApplicationConfig applicationConfig)
        {
            return Instance(FileLoggerWriter.Instance(), applicationConfig);
        }
    }
}