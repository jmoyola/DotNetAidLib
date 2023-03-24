using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetAidLib.Core.Network.Config.WLan.Client.Core
{
    public class WLanClientException:Exception
    {
		public WLanClientException() : base(){}
		public WLanClientException(String message) : base(message) { }
		public WLanClientException(String message, Exception innerException) : base(message, innerException) { }
    }
}
