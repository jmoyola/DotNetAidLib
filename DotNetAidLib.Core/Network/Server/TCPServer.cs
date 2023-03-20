using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace DotNetAidLib.Core.Network.Server
{
	public abstract class TCPServer
	{
		private IPAddress _Address=IPAddress.Any;
		private int _Port=8000;
		private int _MaxClients=500;



		private TcpListener _Listener; 
		private bool _Started = false;

		private List<TcpClient> _Clients=new List<TcpClient>();

		public TCPServer ()
		{
			this.HandleClientMessages = false; 
		}

		public TCPServer (IPAddress address, int port, int maxClients):this()
		{
			_Address = address;
			_Port = port;
			_MaxClients = maxClients;
		}

		public TCPServer (IPAddress address, int port):this(address, port, 500)
		{
		}

		public bool HandleClientMessages{ get; set;}

		private static Object oStarted=new Object();
		public virtual bool Started{
			get{
				lock (oStarted) {
					return this._Started;
				}
			}
		}

		public IPAddress Address{
			get{return _Address;}
			set{ 
				if (this.Started)
					throw new TCPServerException ("Only can change this property with stopped server.");
				_Address = value;
			}
		}

		public int Port{
			get{return _Port;}
			set{ 
				if (this.Started)
                    throw new TCPServerException("Only can change this property with stopped server.");
				_Port = value;
			}
		}

		public int MaxClients{
			get{return _MaxClients;}
			set{ 
				if (this.Started)
                    throw new TCPServerException("Only can change this property with stopped server.");
				_MaxClients = value;
			}
		}

		public virtual void Start(){
			try{
				if (this.Started)
					throw new TCPServerException ("Server is already started.");
				Thread Start_Thread = new Thread (new ThreadStart(Start_ThreadStart));

				_Clients.Clear();
				this._Listener = new TcpListener (this._Address, this._Port);
				this._Listener.Start (_MaxClients);
				this._Started = true;

				Start_Thread.Start ();

				if(this.HandleClientMessages){
					Thread WaitingMessage_Thread = new Thread (new ThreadStart(WaitingMessage_ThreadStart));
					WaitingMessage_Thread.Start ();
				}
			}
			catch(Exception ex){
				throw new TCPServerException ("Error starting the server.\r\n"+ ex.Message ,ex);
			}
		}

		private void Start_ThreadStart(){

			while(this.Started){
				Thread.Sleep(1);
				if(this._Listener.Pending()){
					TcpClient client = this._Listener.AcceptTcpClient ();
					if(this.HandleClientMessages)
						_Clients.Add (client);
					Thread OnClientArrived_Thread =new Thread(
						new ParameterizedThreadStart(OnClientArrived_ThreadStart));
					OnClientArrived_Thread.Start(client);
				}
			}
			this._Listener.Stop();
		}

		private void WaitingMessage_ThreadStart(){
			while(this.Started){
				for(int i=_Clients.Count-1; i>-1; i--){
					Thread.Sleep (1);
					try{
						if (_Clients [i].Available > 0)
						{
							byte[] buffer=new byte[_Clients [i].Available];
							 _Clients [i].GetStream().Read(buffer,0,buffer.Length);

							Thread OnMessageArrived_Thread =new Thread(
								new ParameterizedThreadStart(OnMessageArrived_ThreadStart));
							OnMessageArrived_Thread.Start(new Object[]{buffer, _Clients [i]});
						}
					}
					catch(Exception ex){
						Debug.WriteLine ("Error: " + ex.ToString());
						_Clients.RemoveAt(i); 
					}
				}
			}
		}

		public virtual void Stop(){
			if (!this._Started)
				throw new TCPServerException ("Server is already stopped.");

			for(int i=_Clients.Count-1; i>-1; i--){
				try{
					_Clients [i].Close ();
					_Clients.Remove(_Clients [i]);
				}
				catch{}
			}

			this._Started = false;
		}

		private void OnClientArrived_ThreadStart(Object o){
			this.OnClientArrived((TcpClient)o);
		}

		protected abstract void OnClientArrived (TcpClient client);

		private void OnMessageArrived_ThreadStart(Object o){
			Object[] ao = (Object[])o;
			this.OnMessageArrived ((byte[])ao[0],(TcpClient)ao[1]);
		}

		protected virtual void OnMessageArrived (byte[] message, TcpClient client){}
	}
}

