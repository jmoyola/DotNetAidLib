using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DotNetAidLib.Core.IO.Streams
{
    public class UDPBroadcastServerStream:Stream
    {
        private IPAddress [] localIpAddresses;
        private bool discardLocal = false;
        private Object oLock = new object ();
        private Queue<byte> inputQueue = new Queue<byte> ();
        private UdpClient udpClient;
        private IPAddress _ListenAddress = IPAddress.Any;
        private int _Port = 8000;

        private bool _Started = false;

        public UDPBroadcastServerStream (int port)
            :this(IPAddress.Any, port){}

        public UDPBroadcastServerStream (IPAddress listenAddress, int port)
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
                    throw new UDPBroadcastServerStreamException ("You must before to stop the server to change this property.");

                if (value == null)
                    throw new UDPBroadcastServerStreamException ("Can't be null.");

                _ListenAddress = value;
            }
        }

        public int Port {
            get { return _Port; }
            set {
                if (this.Started)
                    throw new UDPBroadcastServerStreamException ("You must before to stop the server to change this property.");

                if (value < 0 || value > 65535)
                    throw new UDPBroadcastServerStreamException ("Value must be between 1 and 65535.");

                _Port = value;
            }
        }

        public virtual void Start ()
        {
            try {
                if (this.Started)
                    throw new UDPBroadcastServerStreamException ("Server is already started.");

                IPHostEntry ipEntry = Dns.GetHostEntry (Dns.GetHostName ());
                this.localIpAddresses = ipEntry.AddressList;

                this.udpClient = new UdpClient ();
                this.udpClient.Client.Bind( new IPEndPoint(this.ListenAddress, this.Port));
                this._Started = true;
                Thread Start_Thread = new Thread (new ThreadStart (Start_ThreadStart));
                Start_Thread.Start ();
            } catch (Exception ex) {
                throw new UDPBroadcastServerStreamException ("Error starting the server.\r\n" + ex.Message, ex);
            }
        }

        private void Start_ThreadStart ()
        {

            IPEndPoint clientEndPoint = null;

            while (this.Started) {
                Thread.Sleep (1);
                byte [] dataReceived = this.udpClient.Receive (ref clientEndPoint);
                OnClientArrived_ThreadStart(clientEndPoint, dataReceived );
            }
        }

        private void OnClientArrived_ThreadStart (IPEndPoint clientEndPoint, byte [] dataReceived)
        {
            lock (oLock) {
                if (this.discardLocal && Array.Exists (this.localIpAddresses, v => v.Equals (clientEndPoint.Address)))
                     return;

                for (int n = 0; n < dataReceived.Length; n++)
                    this.inputQueue.Enqueue (dataReceived [n]);
            }
        }

        public virtual void Stop ()
        {
            if (!this._Started)
                throw new UDPBroadcastServerStreamException ("Server is already stopped.");
            this._Started = false;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => -1;

        public override long Position
        {
            get
            {
                throw new TCPServerStreamException("This stream can not get position.");
            }
            set
            {
                throw new TCPServerStreamException("This stream can not set position.");
            }
        }

        public override int Read (byte [] buffer, int offset, int count)
        {
            lock (oLock) {
                int n = 0;
                for (n = 0; (n < count) && (this.inputQueue.Count > 0); n++)
                    buffer [offset + n] = this.inputQueue.Dequeue ();
                return n;
            }
        }

        public override long Seek (long offset, SeekOrigin origin)
        {
            throw new UDPBroadcastServerStreamException ("This stream can not seek.");
        }

        public override void SetLength (long value)
        {
            throw new UDPBroadcastServerStreamException ("This stream can not setlength.");
        }

        public override void Flush () { }


        public override void Write (byte [] buffer, int offset, int count)
        {
            lock (oLock) {
                byte [] bs = new byte [count];
                Array.Copy (buffer, offset, bs, 0, count);
                this.udpClient.Send (bs, bs.Length, new IPEndPoint (IPAddress.Broadcast, Port));
            }
        }

        public int BytesToRead {
            get {
                lock (oLock) {
                    return this.inputQueue.Count;
                }
            }
        }

        public bool DiscardLocal {
            get { return discardLocal; }
            set {
                discardLocal = value;
            }
        }
    }
}
