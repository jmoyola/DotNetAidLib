using System;

namespace DotNetAidLib.Core.IO.Archive.Tar
{
    public class TarException : Exception
    {
        public TarException(string message) : base(message)
        {
        }
    }
}