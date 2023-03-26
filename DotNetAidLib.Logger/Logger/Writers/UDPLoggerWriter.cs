using System;
using System.Net.Sockets;
using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Logger.Core;

namespace DotNetAidLib.Logger.Writers
{
    public class UDPLoggerWriter : NetLoggerWriter
    {
        public UDPLoggerWriter()
        {
        }

        public UDPLoggerWriter(string server, int port) : base(server, port)
        {
        }

        public override void InitConfiguration(IApplicationConfigGroup configGroup)
        {
            base.InitConfiguration(configGroup);
        }

        public override void WriteLog(LogEntry logEntry)
        {
            UdpClient cli = null;
            try
            {
                cli = new UdpClient();

                cli.Connect(Server, Port);
                var aBuf = logEntry.ToXmlByteArray();
                cli.Send(aBuf, aBuf.Length);
                cli.Close();
            }
            catch (Exception ex)
            {
                throw new LoggerException("Error writing log to UDP log server", ex, logEntry);
            }
        }


        ~UDPLoggerWriter()
        {
        }
    }
}