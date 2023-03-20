using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace DotNetAidLib.Core.Plugins{
	public class PluginException : Exception
	{
		public PluginException() : base()
		{
		}

		public PluginException(string message) : base(message)
		{
		}

		public PluginException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}