using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetAidLib.Core.Network.Server
{
    public class TCPServerException:Exception
    {
		public TCPServerException() { }
		public TCPServerException(String message):base(message) { }
		public TCPServerException(String message, Exception innerException) : base(message, innerException) { }
    }
}
