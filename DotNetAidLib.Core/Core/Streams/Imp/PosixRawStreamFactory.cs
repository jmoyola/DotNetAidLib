using System.IO;
using System.Runtime.InteropServices;

namespace DotNetAidLib.Core.IO.Streams.Imp
{
    public class PosixRawStreamFactory : RawStreamFactory
    {
        private readonly FileInfo file;

        public PosixRawStreamFactory(string path)
            : base(path)
        {
            file = new FileInfo(path);
            if (!file.Exists)
                throw new IOException("Unable to access drive/partition '" + path + "' . Win32 Error Code " +
                                      Marshal.GetLastWin32Error());
        }

        public override FileStream Open(FileAccess fileAccess)
        {
            return file.Open(FileMode.Open, fileAccess);
        }

        public override void Dispose(bool disposing)
        {
        }
    }
}