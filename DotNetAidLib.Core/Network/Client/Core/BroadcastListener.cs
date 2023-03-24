using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace DotNetAidLib.Core.Network.Client.Core
{
	public delegate void MessageArrivedEventHandler(Object sender, MessageArrivedEventArgs args);
	public class BroadcastListener
	{
		private int _Port = 9898;
		private IPAddress _From = IPAddress.Any;

		Thread thServer;
		private bool _Started = false;

		IPEndPoint epToListen = null;
		UdpClient udpClient = null;

		public event MessageArrivedEventHandler MessageArrived;

		protected void OnMessageArrived(MessageArrivedEventArgs args){
			if (MessageArrived != null)
				MessageArrived (this, args);
		}

		public BroadcastListener (IPAddress from, int port)
		{
			if (port < 1000)
				throw new Exception ("Port must be greatest or equal to 1000.");

			_From = from;
			_Port = port;
		}

		public bool Started{
			get{ return _Started;}
		}

		public int Port{
			get{ 
				return _Port;
			}
		}

		public void Start()
		{
			if (_Started)
				throw new Exception ("Listener is already started.");

			_Started = true;

			thServer = new Thread (new ThreadStart (this.StartListener));
			thServer.Start ();
		}

		public void Stop()
		{
			if (!_Started)
				throw new Exception ("Listener is already stopped.");
			
			_Started = false;
			udpClient.Close();
		}

		private void StartListener()
		{
			epToListen = new IPEndPoint(_From, _Port);
			udpClient = new UdpClient(epToListen);

			while (_Started)
			{
				Thread.Sleep(1);

				IPEndPoint epClient=new IPEndPoint(0,0);

				if (_Started && udpClient.Available >0){
					byte[] bMsg = udpClient.Receive(ref epClient);
					OnMessageArrived(new MessageArrivedEventArgs(epClient, bMsg));
				}
			}
		}
	}
}

