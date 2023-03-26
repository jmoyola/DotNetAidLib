using System.IO;
using DotNetAidLib.Core.IO.Archive.Core;

namespace DotNetAidLib.Core.IO.Archive.Zip
{
    public class GZipArchiveFactory : IArchiveFactory
    {
        private static IArchiveFactory _Instance;

        private GZipArchiveFactory()
        {
        }

        public ArchiveFile NewArchiveInstance(FileInfo archiveFile)
        {
            return new GZipArchiveFile(archiveFile);
        }

        public string DefaultExtension => "gz";

        public static IArchiveFactory Instance()
        {
            if (_Instance == null)
                _Instance = new GZipArchiveFactory();
            return _Instance;
        }
    }
}