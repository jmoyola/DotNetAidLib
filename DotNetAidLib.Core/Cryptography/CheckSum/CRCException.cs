using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetAidLib.Core.Cryptography.CheckSum
{
    public class CRCException:Exception
    {
		public CRCException() : base(){}
		public CRCException(String message) : base(message) { }
		public CRCException(String message, Exception innerException) : base(message, innerException) { }
    }
}
