using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace DotNetAidLib.Configuration.ApplicationConfig.Core{
	public class ApplicationConfigException : Exception
	{

		public ApplicationConfigException() : base()
		{
		}

		public ApplicationConfigException(string msg) : base(msg)
		{
		}

        public ApplicationConfigException(string msg, Exception innerException) : base(msg, innerException)
        {
        }

    }
}