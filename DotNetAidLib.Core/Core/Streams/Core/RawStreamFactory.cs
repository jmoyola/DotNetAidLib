using System;
using System.IO;
using System.Runtime.InteropServices;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.IO.Streams.Imp;

namespace DotNetAidLib.Core.IO.Streams
{
    public abstract class RawStreamFactory : IDisposable
    {
        public RawStreamFactory(string path)
        {
            Assert.NotNullOrEmpty(path, nameof(path));
            Path = path;
        }

        public string Path { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract FileStream Open(FileAccess fileAccess);

        public static RawStreamFactory Instance(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new WinRawStreamFactory(path);
            return new PosixRawStreamFactory(path);
        }


        // Interfaz disposable y su implementación
        ~RawStreamFactory()
        {
            Dispose(false);
        }

        public abstract void Dispose(bool disposing);
    }
}