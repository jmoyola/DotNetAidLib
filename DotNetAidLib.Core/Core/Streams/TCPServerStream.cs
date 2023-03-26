using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DotNetAidLib.Core.IO.Streams
{
    public class TCPServerStream : Stream
    {
        private static readonly object oStarted = new object();
        private IPAddress _ListenAddress = IPAddress.Any;
        private int _Port = 8000;

        private bool _Started;
        private readonly Queue<byte> inputQueue = new Queue<byte>();
        private readonly object oRWLock = new object();
        private readonly object oTCPListenerLock = new object();
        private readonly IList<TcpClient> tcpClients = new List<TcpClient>();
        private TcpListener tcpListener;

        public TCPServerStream(int port)
            : this(IPAddress.Any, port)
        {
        }

        public TCPServerStream(IPAddress listenAddress, int port)
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
                    throw new TCPServerStreamException("You must before to stop the server to change this property.");

                if (value == null)
                    throw new TCPServerStreamException("Can't be null.");

                _ListenAddress = value;
            }
        }

        public int Port
        {
            get => _Port;
            set
            {
                if (Started)
                    throw new TCPServerStreamException("You must before to stop the server to change this property.");

                if (value < 0 || value > 65535)
                    throw new TCPServerStreamException("Value must be between 1 and 65535.");

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
                lock (oTCPListenerLock)
                {
                    return inputQueue.Count;
                }
            }
        }

        public virtual void Start()
        {
            try
            {
                if (Started)
                    throw new TCPServerStreamException("Server is already started.");

                tcpListener = new TcpListener(ListenAddress, Port);
                tcpListener.Start();
                _Started = true;
                var tcpListenerThread = new Thread(TCPListenerThread);
                tcpListenerThread.Start();
                var tcpClientThread = new Thread(TCPClientThread);
                tcpClientThread.Start();
            }
            catch (Exception ex)
            {
                throw new TCPServerStreamException("Error starting the server.\r\n" + ex.Message, ex);
            }
        }

        private void TCPListenerThread()
        {
            while (Started)
            {
                Thread.Sleep(1);

                lock (oTCPListenerLock)
                {
                    if (tcpListener.Pending())
                        tcpClients.Add(tcpListener.AcceptTcpClient());

                    for (var i = tcpClients.Count; i >= 0; i--)
                    {
                        var tcpClient = tcpClients[i];

                        if (!tcpClient.Connected) // Si el cliente no está conextado, lo eliminamos y continuamos
                            tcpClients.Remove(tcpClient);
                    }
                }
            }
        }

        private void TCPClientThread()
        {
            var buffer = new byte [1024];
            while (Started)
            {
                Thread.Sleep(1);
                lock (oTCPListenerLock)
                {
                    foreach (var tcpClient in tcpClients)
                        if (!tcpClient.Connected && tcpClient.Available > 0) // Si el cliente está conextado
                            lock (oRWLock)
                            {
                                var b = tcpClient.GetStream().Read(buffer, 0, buffer.Length);
                                while (b > 0)
                                {
                                    for (var n = 0; n < b; n++)
                                        inputQueue.Enqueue(buffer[n]);
                                    b = tcpClient.GetStream().Read(buffer, 0, buffer.Length);
                                }
                            }
                }
            }
        }

        public virtual void Stop()
        {
            if (!_Started)
                throw new TCPServerStreamException("Server is already stopped.");
            _Started = false;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (oRWLock)
            {
                var n = 0;
                for (n = 0; n < count && inputQueue.Count > 0; n++)
                    buffer[offset + n] = inputQueue.Dequeue();
                return n;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new TCPServerStreamException("This stream can not seek.");
        }

        public override void SetLength(long value)
        {
            throw new TCPServerStreamException("This stream can not setlength.");
        }

        public override void Flush()
        {
            lock (oTCPListenerLock)
            {
                for (var i = tcpClients.Count; i >= 0; i--)
                {
                    var tcpClient = tcpClients[i];

                    if (tcpClient.Connected) // Si el cliente está conextado
                        tcpClient.GetStream().Flush();
                }
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (oTCPListenerLock)
            {
                foreach (var tcpClient in tcpClients)
                    if (tcpClient.Connected) // Si el cliente está conectado
                        lock (oRWLock)
                        {
                            tcpClient.GetStream().Write(buffer, offset, count);
                        }
            }
        }
    }
}