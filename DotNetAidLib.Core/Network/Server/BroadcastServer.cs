using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Diagnostics;

namespace DotNetAidLib.Core.Network.Server
{
	public class BroadcastServer
	{
		private int _Port = 9898;
		private int _Delay = 5000;
		Thread thServer;
		private bool _Started = false;
		private const uint MAX_IDLE_TIME = 60000;

		public BroadcastServer (int port, int delay)
		{
			if (port < 1000)
				throw new Exception ("Port must be greatest or equal to 1000.");
			if (delay < 5000)
				throw new Exception ("Delay must be greatest or equal to 5000.");
			
			_Port = port;
			_Delay = delay;
		}

		public bool Started{
			get{ return _Started;}
		}
			
		public int Port{
			get{ 
				return _Port;
			}
		}

		public int Delay{
			get{ 
				return _Delay;
			}
		}

		public byte[] Message{ get; set;}

		public void Start()
		{
			if (_Started)
				throw new Exception ("Server is already started.");

			_Started = true;
			thServer = new Thread (new ThreadStart (this.StartServer));
			thServer.Start ();
		}

		public void Stop()
		{
			if (!_Started)
				throw new Exception ("Server is already stopped.");
			
			_Started = false;
		}

		private void StartServer()
		{
            while (_Started){
                Thread.Sleep(_Delay);
                if (_Started && Message != null && Message.Length > 0){
                    try{
                        IPEndPoint epBroadcast = new IPEndPoint(IPAddress.Broadcast, _Port);
                        UdpClient udpClient = new UdpClient(0);
                        udpClient.Send(Message, Message.Length, epBroadcast);
                        udpClient.Close();
                    }
                    catch(Exception ex) {
                        Debug.WriteLine("Error sending broadcast message: " + ex.Message, ex);
                    }
                }
            }
		}
	}
}

