using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DotNetAidLib.Core.IO.Streams
{
    public class UDPBroadcastServerStream : Stream
    {
        private static readonly object oStarted = new object();
        private IPAddress _ListenAddress = IPAddress.Any;
        private int _Port = 8000;

        private bool _Started;
        private readonly Queue<byte> inputQueue = new Queue<byte>();
        private IPAddress[] localIpAddresses;
        private readonly object oLock = new object();
        private UdpClient udpClient;

        public UDPBroadcastServerStream(int port)
            : this(IPAddress.Any, port)
        {
        }

        public UDPBroadcastServerStream(IPAddress listenAddress, int port)
        {
            ListenAddress = listenAddress;
            Port = port;
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

        public IPAddress ListenAddress
        {
            get => _ListenAddress;
            set
            {
                if (Started)
                    throw new UDPBroadcastServerStreamException(
                        "You must before to stop the server to change this property.");

                if (value == null)
                    throw new UDPBroadcastServerStreamException("Can't be null.");

                _ListenAddress = value;
            }
        }

        public int Port
        {
            get => _Port;
            set
            {
                if (Started)
                    throw new UDPBroadcastServerStreamException(
                        "You must before to stop the server to change this property.");

                if (value < 0 || value > 65535)
                    throw new UDPBroadcastServerStreamException("Value must be between 1 and 65535.");

                _Port = value;
            }
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => -1;

        public override long Position
        {
            get => throw new TCPServerStreamException("This stream can not get position.");
            set => throw new TCPServerStreamException("This stream can not set position.");
        }

        public int BytesToRead
        {
            get
            {
                lock (oLock)
                {
                    return inputQueue.Count;
                }
            }
        }

        public bool DiscardLocal { get; set; } = false;

        public virtual void Start()
        {
            try
            {
                if (Started)
                    throw new UDPBroadcastServerStreamException("Server is already started.");

                var ipEntry = Dns.GetHostEntry(Dns.GetHostName());
                localIpAddresses = ipEntry.AddressList;

                udpClient = new UdpClient();
                udpClient.Client.Bind(new IPEndPoint(ListenAddress, Port));
                _Started = true;
                var Start_Thread = new Thread(Start_ThreadStart);
                Start_Thread.Start();
            }
            catch (Exception ex)
            {
                throw new UDPBroadcastServerStreamException("Error starting the server.\r\n" + ex.Message, ex);
            }
        }

        private void Start_ThreadStart()
        {
            IPEndPoint clientEndPoint = null;

            while (Started)
            {
                Thread.Sleep(1);
                var dataReceived = udpClient.Receive(ref clientEndPoint);
                OnClientArrived_ThreadStart(clientEndPoint, dataReceived);
            }
        }

        private void OnClientArrived_ThreadStart(IPEndPoint clientEndPoint, byte[] dataReceived)
        {
            lock (oLock)
            {
                if (DiscardLocal && Array.Exists(localIpAddresses, v => v.Equals(clientEndPoint.Address)))
                    return;

                for (var n = 0; n < dataReceived.Length; n++)
                    inputQueue.Enqueue(dataReceived[n]);
            }
        }

        public virtual void Stop()
        {
            if (!_Started)
                throw new UDPBroadcastServerStreamException("Server is already stopped.");
            _Started = false;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (oLock)
            {
                var n = 0;
                for (n = 0; n < count && inputQueue.Count > 0; n++)
                    buffer[offset + n] = inputQueue.Dequeue();
                return n;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new UDPBroadcastServerStreamException("This stream can not seek.");
        }

        public override void SetLength(long value)
        {
            throw new UDPBroadcastServerStreamException("This stream can not setlength.");
        }

        public override void Flush()
        {
        }


        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (oLock)
            {
                var bs = new byte [count];
                Array.Copy(buffer, offset, bs, 0, count);
                udpClient.Send(bs, bs.Length, new IPEndPoint(IPAddress.Broadcast, Port));
            }
        }
    }
}