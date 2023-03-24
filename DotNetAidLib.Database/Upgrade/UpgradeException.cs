using System;

namespace DotNetAidLib.Database.Upgrade
{
	public class UpgradeException:Exception
	{
		public UpgradeException():base()
		{}
		public UpgradeException(String message):base(message)
		{}
		public UpgradeException(String message, Exception innerException):base(message,innerException)
		{}
	}
}

