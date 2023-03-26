using System.IO;
using DotNetAidLib.Core.IO.Archive.Core;

namespace DotNetAidLib.Core.IO.Archive.Zip
{
    public class ZipArchiveFactory : IArchiveFactory
    {
        private static IArchiveFactory _Instance;

        private ZipArchiveFactory()
        {
        }

        public ArchiveFile NewArchiveInstance(FileInfo archiveFile)
        {
            return new ZipArchiveFile(archiveFile);
        }

        public string DefaultExtension => "zip";

        public static IArchiveFactory Instance()
        {
            if (_Instance == null)
                _Instance = new ZipArchiveFactory();
            return _Instance;
        }
    }
}