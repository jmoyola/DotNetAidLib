using System;

namespace DotNetAidLib.Core.IO.Archive.Core
{
	public class ArchiveException:Exception
	{
		public ArchiveException ():base()
		{
		}
		public ArchiveException (String message)
			:base(message)
		{
		}
		public ArchiveException (String message, Exception innerException)
			:base(message,innerException)
		{
		}
	}
}

