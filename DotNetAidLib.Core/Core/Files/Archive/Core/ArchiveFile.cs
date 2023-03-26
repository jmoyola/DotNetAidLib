using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.IO.Archive.Core
{
    public abstract class ArchiveFile
    {
        protected FileInfo _File;
        protected ArchiveOpenMode _OpenMode = ArchiveOpenMode.Close;
        protected string _Password;
        protected Encoding _TextEncoding = Encoding.UTF8;
        protected int compressionLevel = 5;

        public ArchiveFile(FileInfo file)
        {
            _File = file;
        }

        public FileInfo File => _File;

        public ArchiveOpenMode OpenMode => _OpenMode;

        public virtual string Password
        {
            get => _Password;
            set => _Password = value;
        }

        public virtual int CompressionLevel
        {
            get => compressionLevel;
            set
            {
                Assert.BetweenOrEqual(value, 0, 9, nameof(value));
                compressionLevel = value;
            }
        }

        public Encoding TextEncoding
        {
            get => _TextEncoding;
            set => _TextEncoding = value;
        }

        protected CompressionLevel CompressionLevelParse(int level)
        {
            var ret = System.IO.Compression.CompressionLevel.Fastest;
            if (level == 0)
                ret = System.IO.Compression.CompressionLevel.NoCompression;
            else if (level <= 5)
                ret = System.IO.Compression.CompressionLevel.Fastest;
            else if (level > 5)
                ret = System.IO.Compression.CompressionLevel.Optimal;

            return ret;
        }

        public virtual void Open(ArchiveOpenMode openMode)
        {
            if (!_OpenMode.Equals(ArchiveOpenMode.Close))
                throw new ArchiveException("Archive is already open in mode " + _OpenMode + ".");
        }

        public virtual void Close()
        {
            if (_OpenMode.Equals(ArchiveOpenMode.Close))
                throw new ArchiveException("Archive is already closed.");
        }


        public void Add(DirectoryInfo directoryToAdd)
        {
            Assert.Exists(directoryToAdd, nameof(directoryToAdd));

            AddRecursive(directoryToAdd, directoryToAdd);
        }

        private void AddRecursive(DirectoryInfo rootDirectory, DirectoryInfo directoryToAdd)
        {
            Assert.Exists(rootDirectory, nameof(rootDirectory));
            Assert.Exists(directoryToAdd, nameof(directoryToAdd));

            foreach (var d in directoryToAdd.GetDirectories())
                AddRecursive(rootDirectory, d);
            foreach (var f in directoryToAdd.GetFiles())
                Add(f, rootDirectory);
        }

        public void Add(FileInfo fileToAdd)
        {
            Assert.Exists(fileToAdd, nameof(fileToAdd));

            Add(fileToAdd, fileToAdd.Directory);
        }

        public virtual void Add(FileInfo fileToAdd, DirectoryInfo relativePath)
        {
            if (OpenMode.Equals(ArchiveOpenMode.Close))
                throw new ArchiveException("Archive is already closed.");
            if (OpenMode.Equals(ArchiveOpenMode.OpenRead))
                throw new ArchiveException("Archive must be opened in create or update mode.");

            Assert.Exists(fileToAdd, nameof(fileToAdd));
            Assert.Exists(relativePath, nameof(relativePath));
        }

        public virtual void Remove(ArchivePart archivePart)
        {
            if (OpenMode.Equals(ArchiveOpenMode.Close))
                throw new ArchiveException("Archive is already closed.");
            if (!OpenMode.Equals(ArchiveOpenMode.OpenUpdate))
                throw new ArchiveException("Archive must be opened in update mode.");

            Assert.NotNull(archivePart, nameof(archivePart));
        }

        public virtual IList<ArchivePart> Content()
        {
            if (OpenMode.Equals(ArchiveOpenMode.Close))
                throw new ArchiveException("Archive is already closed.");
            if (!OpenMode.Equals(ArchiveOpenMode.OpenRead))
                throw new ArchiveException("Archive must be opened in read mode.");
            return new List<ArchivePart>();
        }

        public virtual void Get(ArchivePart archivePart, FileInfo outFile)
        {
            if (OpenMode.Equals(ArchiveOpenMode.Close))
                throw new ArchiveException("Archive is already closed.");
            if (!OpenMode.Equals(ArchiveOpenMode.OpenRead))
                throw new ArchiveException("Archive must be opened in read mode.");

            Assert.NotNull(archivePart, nameof(archivePart));
            Assert.NotNull(outFile, nameof(outFile));

            if ((archivePart.Attributes & ArchivePartAttributes.Directory) == ArchivePartAttributes.Directory)
                throw new ArchiveException("archivePart is Directory");
        }

        public virtual void Get(ArchivePart archivePart, DirectoryInfo outDirectory, bool recursive = false)
        {
            if (OpenMode.Equals(ArchiveOpenMode.Close))
                throw new ArchiveException("Archive is already closed.");
            if (!OpenMode.Equals(ArchiveOpenMode.OpenRead))
                throw new ArchiveException("Archive must be opened in read mode.");

            Assert.NotNull(archivePart, nameof(archivePart));
            Assert.Exists(outDirectory, nameof(outDirectory));
        }

        public void GetAll(IEnumerable<ArchivePart> archiveParts, DirectoryInfo outDirectory)
        {
            if (OpenMode.Equals(ArchiveOpenMode.Close))
                throw new ArchiveException("Archive is already closed.");
            if (!OpenMode.Equals(ArchiveOpenMode.OpenRead))
                throw new ArchiveException("Archive must be opened in read mode.");

            Assert.NotNull(archiveParts, nameof(archiveParts));
            Assert.Exists(outDirectory, nameof(outDirectory));

            foreach (var part in archiveParts)
                Get(part, outDirectory);
        }

        public virtual void GetAll(DirectoryInfo outDirectory)
        {
            Assert.Exists(outDirectory, nameof(outDirectory));
            var content = Content();
            GetAll(content, outDirectory);
        }
    }
}