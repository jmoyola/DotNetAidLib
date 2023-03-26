using System;
using System.IO;
using System.Net.Sockets;
using DotNetAidLib.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Logger.Core;

namespace DotNetAidLib.Logger.Writers
{
    public class TCPLoggerWriter : NetLoggerWriter
    {
        private static readonly object oWriteLog = new object();

        public TCPLoggerWriter()
        {
        }

        public TCPLoggerWriter(string server, int port) : base(server, port)
        {
        }

        public override void InitConfiguration(IApplicationConfigGroup configGroup)
        {
            base.InitConfiguration(configGroup);
        }

        public override void WriteLog(LogEntry logEntry)
        {
            lock (oWriteLog)
            {
                TcpClient cli = null;
                try
                {
                    cli = new TcpClient();
                    cli.Connect(Server, Port);
                    Stream cliSt = cli.GetStream();

                    logEntry.ToXml(cliSt);
                    cliSt.WriteByte(4);
                    // Escribimos la marca de eot

                    cliSt.Flush();
                    cliSt.Close();
                }
                catch (Exception ex)
                {
                    throw new LoggerException("Error writing log to TCP log server", ex, logEntry);
                }
                finally
                {
                    try
                    {
                        cli.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }


        ~TCPLoggerWriter()
        {
        }
    }
}