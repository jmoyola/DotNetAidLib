using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DotNetAidLib.Core.Network.Server
{
    public abstract class UDPServer
    {
        private static readonly object oStarted = new object();
        private IPAddress _Address = IPAddress.Any;

        private UdpClient _Listener;
        private int _Port = 8000;
        private bool _Started;

        public UDPServer()
        {
        }

        public UDPServer(IPAddress address, int port) : this()
        {
            _Address = address;
            _Port = port;
        }

        public bool Started
        {
            get
            {
                lock (oStarted)
                {
                    return _Started;
                }
            }
        }

        public IPAddress Address
        {
            get => _Address;
            set
            {
                if (Started)
                    throw new UDPServerException("You must before to stop the server to change this property.");
                _Address = value;
            }
        }

        public int Port
        {
            get => _Port;
            set
            {
                if (Started)
                    throw new UDPServerException("You must before to stop the server to change this property.");
                _Port = value;
            }
        }

        public virtual void Start()
        {
            try
            {
                if (Started)
                    throw new UDPServerException("Server is already started.");
                var Start_Thread = new Thread(Start_ThreadStart);
                Start_Thread.Start();

                _Listener = new UdpClient(_Port);
                _Started = true;
            }
            catch (Exception ex)
            {
                throw new UDPServerException("Error starting the server.\r\n" + ex.Message, ex);
            }
        }

        private void Start_ThreadStart()
        {
            IPEndPoint clientEndPoint = null;

            while (Started)
            {
                Thread.Sleep(1);
                var dataReceived = _Listener.Receive(ref clientEndPoint);

                var OnClientArrived_Thread = new Thread(
                    OnClientArrived_ThreadStart);
                OnClientArrived_Thread.Start(
                    new object[] {clientEndPoint, dataReceived});
            }
        }

        private void OnClientArrived_ThreadStart(object o)
        {
            var ao = (object[]) o;
            var clientEndPoint = (IPEndPoint) ao[0];
            var dataReceived = (byte[]) ao[1];

            OnClientArrived(clientEndPoint, dataReceived);
        }

        public virtual void Stop()
        {
            if (!_Started)
                throw new UDPServerException("Server is already stopped.");
            _Started = false;
        }

        protected abstract void OnClientArrived(IPEndPoint clientEndPoint, byte[] dataReceived);
    }
}