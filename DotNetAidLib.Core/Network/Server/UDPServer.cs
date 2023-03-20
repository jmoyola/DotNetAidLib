using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DotNetAidLib.Core.Network.Server
{
	public abstract class UDPServer
	{
		private IPAddress _Address=IPAddress.Any;
		private int _Port=8000;

		private UdpClient _Listener; 
		private bool _Started = false;

		public UDPServer ()
		{
		}

		public UDPServer (IPAddress address, int port):this()
		{
			_Address = address;
			_Port = port;
		}

		private static Object oStarted=new Object();
		public bool Started{
			get{
				lock (oStarted) {
					return _Started;
				}
			}
		}

		public IPAddress Address{
			get{return _Address;}
			set{ 
				if (this.Started)
					throw new UDPServerException ("You must before to stop the server to change this property.");
				_Address = value;
			}
		}

		public int Port{
			get{return _Port;}
			set{ 
				if (this.Started)
					throw new UDPServerException ("You must before to stop the server to change this property.");
				_Port = value;
			}
		}

		public virtual void Start(){
			try{
				if (this.Started)
					throw new UDPServerException ("Server is already started.");
				Thread Start_Thread = new Thread (new ThreadStart(Start_ThreadStart));
				Start_Thread.Start ();

				this._Listener = new UdpClient(this._Port);
				this._Started = true;
			}
			catch(Exception ex){
				throw new UDPServerException ("Error starting the server.\r\n"+ ex.Message ,ex);
			}
		}

		private void Start_ThreadStart(){
			 
			IPEndPoint clientEndPoint=null;

			while(this.Started){
				Thread.Sleep(1);
				byte[] dataReceived=this._Listener.Receive(ref clientEndPoint);

				Thread OnClientArrived_Thread =new Thread(
					new ParameterizedThreadStart(OnClientArrived_ThreadStart));
				OnClientArrived_Thread.Start(
					new Object[]{clientEndPoint, dataReceived});

			}
		}

		private void OnClientArrived_ThreadStart(Object o){
			Object[] ao = (Object[])o;
			IPEndPoint clientEndPoint = (IPEndPoint)ao [0];
			byte[] dataReceived = (byte[])ao [1];

			this.OnClientArrived(clientEndPoint, dataReceived);
		}

		public virtual void Stop(){
			if (!this._Started)
				throw new UDPServerException ("Server is already stopped.");
			this._Started = false;
		}

		protected abstract void OnClientArrived(IPEndPoint clientEndPoint, byte[] dataReceived);
	}
}

