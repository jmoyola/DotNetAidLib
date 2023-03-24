using System;

namespace Library.OperatingSystem.Core
{
	public class OperatingSystemException:Exception
	{
		public OperatingSystemException ():base()
		{
		}
		public OperatingSystemException (String message)
			:base(message)
		{
		}
		public OperatingSystemException (String message, Exception innerException)
			:base(message,innerException)
		{
		}
	}
}

