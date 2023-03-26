using System.IO;

namespace DotNetAidLib.Core.IO.Archive.Core
{
    public interface IArchiveFactory
    {
        string DefaultExtension { get; }
        ArchiveFile NewArchiveInstance(FileInfo archiveFile);
    }
}