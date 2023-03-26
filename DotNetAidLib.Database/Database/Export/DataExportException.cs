using System;

namespace DotNetAidLib.Database.Export
{
    public class DataExportException : Exception
    {
        public DataExportException(string message) : base(message)
        {
        }

        public DataExportException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}