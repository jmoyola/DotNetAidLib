using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetAidLib.Core.Network.Config.TcpIp.Core
{
    public class NetworkingException:Exception
    {
		public NetworkingException() : base(){}
		public NetworkingException(String message) : base(message) { }
		public NetworkingException(String message, Exception innerException) : base(message, innerException) { }
    }
}
