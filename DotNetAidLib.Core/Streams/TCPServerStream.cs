using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DotNetAidLib.Core.IO.Streams
{
    public class TCPServerStream:Stream
    {
        IList<TcpClient> tcpClients = new List<TcpClient> ();
        private Object oTCPListenerLock = new object ();
        private Object oRWLock = new object ();
        private Queue<byte> inputQueue = new Queue<byte> ();
        private TcpListener tcpListener;
        private IPAddress _ListenAddress = IPAddress.Any;
        private int _Port = 8000;

        private bool _Started = false;

        public TCPServerStream (int port)
            :this(IPAddress.Any, port){}

        public TCPServerStream (IPAddress listenAddress, int port)
        {
            this.ListenAddress = listenAddress;
            this.Port = port;
        }

        private static Object oStarted = new Object ();
        public bool Started {
            get {
                lock (oStarted) {
                    return _Started;
                }
            }
        }

        public IPAddress ListenAddress {
            get { return _ListenAddress; }
            set {
                if (this.Started)
                    throw new TCPServerStreamException ("You must before to stop the server to change this property.");

                if (value == null)
                    throw new TCPServerStreamException ("Can't be null.");

                _ListenAddress = value;
            }
        }

        public int Port {
            get { return _Port; }
            set {
                if (this.Started)
                    throw new TCPServerStreamException ("You must before to stop the server to change this property.");

                if (value < 0 || value > 65535)
                    throw new TCPServerStreamException ("Value must be between 1 and 65535.");

                _Port = value;
            }
        }

        public virtual void Start ()
        {
            try {
                if (this.Started)
                    throw new TCPServerStreamException ("Server is already started.");
                    
                this.tcpListener = new TcpListener(this.ListenAddress, this.Port);
                this.tcpListener.Start ();
                this._Started = true;
                Thread tcpListenerThread = new Thread (new ThreadStart (TCPListenerThread));
                tcpListenerThread.Start ();
                Thread tcpClientThread = new Thread (new ThreadStart (TCPClientThread));
                tcpClientThread.Start ();
            } catch (Exception ex) {
                throw new TCPServerStreamException ("Error starting the server.\r\n" + ex.Message, ex);
            }
        }

        private void TCPListenerThread ()
        {
            while (this.Started) {
                Thread.Sleep (1);

                lock (oTCPListenerLock) {
                    if (tcpListener.Pending ())
                        this.tcpClients.Add (tcpListener.AcceptTcpClient ());

                    for (int i = this.tcpClients.Count; i >= 0; i--) {
                        TcpClient tcpClient = this.tcpClients [i];

                        if (!tcpClient.Connected) { // Si el cliente no está conextado, lo eliminamos y continuamos
                            this.tcpClients.Remove (tcpClient);
                            continue;
                        }
                    }
                }
            }
        }

        private void TCPClientThread ()
        {
            byte [] buffer = new byte [1024];
            while (this.Started) {
                Thread.Sleep (1);
                lock (oTCPListenerLock) {
                    foreach (TcpClient tcpClient in this.tcpClients) {
                        if (!tcpClient.Connected && tcpClient.Available > 0) { // Si el cliente está conextado
                            lock (oRWLock) {
                                int b = tcpClient.GetStream ().Read (buffer, 0, buffer.Length);
                                while (b > 0) {
                                    for (int n = 0; n < b; n++)
                                        this.inputQueue.Enqueue (buffer [n]);
                                    b = tcpClient.GetStream ().Read (buffer, 0, buffer.Length);
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void Stop ()
        {
            if (!this._Started)
                throw new TCPServerStreamException ("Server is already stopped.");
            this._Started = false;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => -1;

        public override long Position {
            get {
                throw new TCPServerStreamException("This stream can not get position.");
            }
            set
            {
                throw new TCPServerStreamException("This stream can not set position.");
            }
        }

        public override int Read (byte [] buffer, int offset, int count)
        {
            lock (oRWLock) {
                int n = 0;
                for (n = 0; (n < count) && (this.inputQueue.Count > 0); n++)
                    buffer [offset + n] = this.inputQueue.Dequeue ();
                return n;
            }
        }

        public override long Seek (long offset, SeekOrigin origin)
        {
            throw new TCPServerStreamException ("This stream can not seek.");
        }

        public override void SetLength (long value)
        {
            throw new TCPServerStreamException ("This stream can not setlength.");
        }

        public override void Flush () {
            lock (oTCPListenerLock) {
                for (int i = this.tcpClients.Count; i >= 0; i--) {
                    TcpClient tcpClient = this.tcpClients [i];

                    if (tcpClient.Connected) // Si el cliente está conextado
                        tcpClient.GetStream ().Flush ();
                }
            }
        }

        public override void Write (byte [] buffer, int offset, int count)
        {
            lock (oTCPListenerLock) {
                foreach (TcpClient tcpClient in this.tcpClients) {
                    if (tcpClient.Connected) { // Si el cliente está conectado
                        lock (oRWLock) {
                            tcpClient.GetStream ().Write (buffer, offset, count);
                        }
                    }
                }
            }
        }

        public int BytesToRead {
            get {
                lock (oTCPListenerLock) {
                    return this.inputQueue.Count;
                }
            }
        }
    }
}
