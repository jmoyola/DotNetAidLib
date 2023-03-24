using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using System.Globalization;
using DotNetAidLib.Core.Develop;
using System.Net;

namespace DotNetAidLib.Core.Network.Client.Core
{
	public interface IPublicIPProvider
	{
		int PreferentOrder { get;}
		IPAddress Request ();
	}
}

