using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DotNetAidLib.Core.Network.Client.Core
{
    public delegate void MessageArrivedEventHandler(object sender, MessageArrivedEventArgs args);

    public class BroadcastListener
    {
        private readonly IPAddress _From = IPAddress.Any;

        private IPEndPoint epToListen;

        private Thread thServer;
        private UdpClient udpClient;

        public BroadcastListener(IPAddress from, int port)
        {
            if (port < 1000)
                throw new Exception("Port must be greatest or equal to 1000.");

            _From = from;
            Port = port;
        }

        public bool Started { get; private set; }

        public int Port { get; } = 9898;

        public event MessageArrivedEventHandler MessageArrived;

        protected void OnMessageArrived(MessageArrivedEventArgs args)
        {
            if (MessageArrived != null)
                MessageArrived(this, args);
        }

        public void Start()
        {
            if (Started)
                throw new Exception("Listener is already started.");

            Started = true;

            thServer = new Thread(StartListener);
            thServer.Start();
        }

        public void Stop()
        {
            if (!Started)
                throw new Exception("Listener is already stopped.");

            Started = false;
            udpClient.Close();
        }

        private void StartListener()
        {
            epToListen = new IPEndPoint(_From, Port);
            udpClient = new UdpClient(epToListen);

            while (Started)
            {
                Thread.Sleep(1);

                var epClient = new IPEndPoint(0, 0);

                if (Started && udpClient.Available > 0)
                {
                    var bMsg = udpClient.Receive(ref epClient);
                    OnMessageArrived(new MessageArrivedEventArgs(epClient, bMsg));
                }
            }
        }
    }
}