using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DotNetAidLib.Core.Network.Server
{
    public class BroadcastServer
    {
        private const uint MAX_IDLE_TIME = 60000;
        private Thread thServer;

        public BroadcastServer(int port, int delay)
        {
            if (port < 1000)
                throw new Exception("Port must be greatest or equal to 1000.");
            if (delay < 5000)
                throw new Exception("Delay must be greatest or equal to 5000.");

            Port = port;
            Delay = delay;
        }

        public bool Started { get; private set; }

        public int Port { get; } = 9898;

        public int Delay { get; } = 5000;

        public byte[] Message { get; set; }

        public void Start()
        {
            if (Started)
                throw new Exception("Server is already started.");

            Started = true;
            thServer = new Thread(StartServer);
            thServer.Start();
        }

        public void Stop()
        {
            if (!Started)
                throw new Exception("Server is already stopped.");

            Started = false;
        }

        private void StartServer()
        {
            while (Started)
            {
                Thread.Sleep(Delay);
                if (Started && Message != null && Message.Length > 0)
                    try
                    {
                        var epBroadcast = new IPEndPoint(IPAddress.Broadcast, Port);
                        var udpClient = new UdpClient(0);
                        udpClient.Send(Message, Message.Length, epBroadcast);
                        udpClient.Close();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error sending broadcast message: " + ex.Message, ex);
                    }
            }
        }
    }
}