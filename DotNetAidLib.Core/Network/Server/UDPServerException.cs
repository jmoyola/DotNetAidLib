using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetAidLib.Core.Network.Server
{
    public class UDPServerException:Exception
    {
		public UDPServerException() { }
		public UDPServerException(String message):base(message) { }
		public UDPServerException(String message, Exception innerException) : base(message, innerException) { }
    }
}
