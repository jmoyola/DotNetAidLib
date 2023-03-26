using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DotNetAidLib.Core.Network.Server
{
    public abstract class TCPServer
    {
        private static readonly object oStarted = new object();
        private IPAddress _Address = IPAddress.Any;

        private readonly List<TcpClient> _Clients = new List<TcpClient>();


        private TcpListener _Listener;
        private int _MaxClients = 500;
        private int _Port = 8000;
        private bool _Started;

        public TCPServer()
        {
            HandleClientMessages = false;
        }

        public TCPServer(IPAddress address, int port, int maxClients) : this()
        {
            _Address = address;
            _Port = port;
            _MaxClients = maxClients;
        }

        public TCPServer(IPAddress address, int port) : this(address, port, 500)
        {
        }

        public bool HandleClientMessages { get; set; }

        public virtual bool Started
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
                    throw new TCPServerException("Only can change this property with stopped server.");
                _Address = value;
            }
        }

        public int Port
        {
            get => _Port;
            set
            {
                if (Started)
                    throw new TCPServerException("Only can change this property with stopped server.");
                _Port = value;
            }
        }

        public int MaxClients
        {
            get => _MaxClients;
            set
            {
                if (Started)
                    throw new TCPServerException("Only can change this property with stopped server.");
                _MaxClients = value;
            }
        }

        public virtual void Start()
        {
            try
            {
                if (Started)
                    throw new TCPServerException("Server is already started.");
                var Start_Thread = new Thread(Start_ThreadStart);

                _Clients.Clear();
                _Listener = new TcpListener(_Address, _Port);
                _Listener.Start(_MaxClients);
                _Started = true;

                Start_Thread.Start();

                if (HandleClientMessages)
                {
                    var WaitingMessage_Thread = new Thread(WaitingMessage_ThreadStart);
                    WaitingMessage_Thread.Start();
                }
            }
            catch (Exception ex)
            {
                throw new TCPServerException("Error starting the server.\r\n" + ex.Message, ex);
            }
        }

        private void Start_ThreadStart()
        {
            while (Started)
            {
                Thread.Sleep(1);
                if (_Listener.Pending())
                {
                    var client = _Listener.AcceptTcpClient();
                    if (HandleClientMessages)
                        _Clients.Add(client);
                    var OnClientArrived_Thread = new Thread(
                        OnClientArrived_ThreadStart);
                    OnClientArrived_Thread.Start(client);
                }
            }

            _Listener.Stop();
        }

        private void WaitingMessage_ThreadStart()
        {
            while (Started)
                for (var i = _Clients.Count - 1; i > -1; i--)
                {
                    Thread.Sleep(1);
                    try
                    {
                        if (_Clients[i].Available > 0)
                        {
                            var buffer = new byte[_Clients[i].Available];
                            _Clients[i].GetStream().Read(buffer, 0, buffer.Length);

                            var OnMessageArrived_Thread = new Thread(
                                OnMessageArrived_ThreadStart);
                            OnMessageArrived_Thread.Start(new object[] {buffer, _Clients[i]});
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error: " + ex);
                        _Clients.RemoveAt(i);
                    }
                }
        }

        public virtual void Stop()
        {
            if (!_Started)
                throw new TCPServerException("Server is already stopped.");

            for (var i = _Clients.Count - 1; i > -1; i--)
                try
                {
                    _Clients[i].Close();
                    _Clients.Remove(_Clients[i]);
                }
                catch
                {
                }

            _Started = false;
        }

        private void OnClientArrived_ThreadStart(object o)
        {
            OnClientArrived((TcpClient) o);
        }

        protected abstract void OnClientArrived(TcpClient client);

        private void OnMessageArrived_ThreadStart(object o)
        {
            var ao = (object[]) o;
            OnMessageArrived((byte[]) ao[0], (TcpClient) ao[1]);
        }

        protected virtual void OnMessageArrived(byte[] message, TcpClient client)
        {
        }
    }
}