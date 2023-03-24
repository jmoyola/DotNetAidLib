using System;
using System.IO;
using System.Runtime.InteropServices;
using DotNetAidLib.Core.IO.Streams.Imp;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.IO.Streams
{
    public abstract class RawStreamFactory:IDisposable
    {
        private String path;

        public RawStreamFactory(String path){
            Assert.NotNullOrEmpty( path, nameof(path));
            this.path = path;
        }

        public String Path
        {
            get{
                return path;
            }
        }

        public abstract FileStream Open(FileAccess fileAccess);

        public static RawStreamFactory Instance(String path) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new WinRawStreamFactory(path);
            else
                return new PosixRawStreamFactory(path);
        }


        // Interfaz disposable y su implementación
        ~RawStreamFactory()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract void Dispose(bool disposing);
    }
}
