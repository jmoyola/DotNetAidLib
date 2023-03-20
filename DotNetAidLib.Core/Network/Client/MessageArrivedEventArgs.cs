using System;
using System.Net;

namespace DotNetAidLib.Core.Network.Client
{
	public class MessageArrivedEventArgs:EventArgs
	{
		private IPEndPoint _IPEndPoint;
		private byte[] _Message;
		public MessageArrivedEventArgs (IPEndPoint ipEndPoint, byte[] message)
		{
			this._IPEndPoint = ipEndPoint;
			this._Message = message;
		}

		public byte[] Message{
			get{ 
				return _Message;
			}
		}
		public IPEndPoint IPEndPoint{
			get{ 
				return _IPEndPoint;
			}
		}
	}
}

