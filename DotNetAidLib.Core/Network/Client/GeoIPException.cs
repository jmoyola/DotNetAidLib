﻿using System;

namespace DotNetAidLib.Core.Network.Client{
	public class WebClientException : Exception
	{
		public WebClientException() : base()
		{
		}

		public WebClientException(string message) : base(message)
		{
		}

		public WebClientException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}