using System;

namespace DotNetAidLib.Database.Upgrade
{
    public class UpgradeException : Exception
    {
        public UpgradeException()
        {
        }

        public UpgradeException(string message) : base(message)
        {
        }

        public UpgradeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}