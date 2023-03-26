using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.IO.Archive.Core;

namespace DotNetAidLib.Core.IO.Archive.Zip
{
    public class ZipArchiveFile : ArchiveFile
    {
        private ZipArchive zArchive;
        private FileStream zipFileStream;

        public ZipArchiveFile(FileInfo archiveFile) : base(archiveFile)
        {
        }

        public override int CompressionLevel
        {
            get => compressionLevel;
            set
            {
                Assert.Including(value, new[] {0, 5, 9}, nameof(value));
                compressionLevel = value;
            }
        }

        public override void Open(ArchiveOpenMode openMode)
        {
            base.Open(openMode);

            try
            {
                zipFileStream = new FileStream(File.FullName, FileMode.OpenOrCreate);
                if (openMode.Equals(ArchiveOpenMode.OpenCreate))
                    zArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Create, false, TextEncoding);
                else if (openMode.Equals(ArchiveOpenMode.OpenRead))
                    zArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Read, false, TextEncoding);
                else if (openMode.Equals(ArchiveOpenMode.OpenUpdate))
                    zArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Update, false, TextEncoding);

                _OpenMode = openMode;
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error opening archive in mode " + _OpenMode + ": " + ex.Message, ex);
            }
        }

        public override void Close()
        {
            base.Close();
            try
            {
                zArchive.Dispose();
                zipFileStream.Close();
                _OpenMode = ArchiveOpenMode.Close;
                ;
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error closing archive:\r\n" + ex.Message, ex);
            }
        }

        public override void Add(FileInfo fileToAdd, DirectoryInfo relativePath)
        {
            base.Add(fileToAdd, relativePath);

            try
            {
                var entryName = fileToAdd.FullName.Substring(relativePath.FullName.Length);
                var part = zArchive.CreateEntryFromFile(fileToAdd.FullName, entryName,
                    CompressionLevelParse(CompressionLevel));
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error adding file to archive", ex);
            }
        }

        public override void Remove(ArchivePart archivePart)
        {
            base.Remove(archivePart);
            try
            {
                var part = zArchive.Entries.FirstOrDefault(v => v.FullName == archivePart.FullName);
                if (part != null)
                    part.Delete();
            }
            catch (Exception ex)
            {
                throw new Exception("Error removing file from archive", ex);
            }
        }

        public override IList<ArchivePart> Content()
        {
            var ret = base.Content();

            try
            {
                foreach (var part in zArchive.Entries)
                    ret.Add(new ArchivePart(part.Name, part.FullName, part.CompressedLength, part.Length,
                        part.LastWriteTime));

                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting content from archive", ex);
            }
        }

        private string normalizeRelativePath(string path)
        {
            // replace slashs with os separator
            var ret = Regex.Replace(path, "[\\/]", Path.DirectorySeparatorChar + "");
            // add first slash if not exists
            ret = (ret[0] == Path.DirectorySeparatorChar ? "" : "" + Path.DirectorySeparatorChar) + ret;

            return ret;
        }

        public override void Get(ArchivePart archivePart, DirectoryInfo outDirectory, bool recursive = false)
        {
            base.Get(archivePart, outDirectory, recursive);
            try
            {
                var part = zArchive.Entries.FirstOrDefault(v => v.FullName == archivePart.FullName);
                if (part == null)
                    throw new ArchiveException("This part isn't in archive file.");

                var destFile = new FileInfo(outDirectory.FullName + normalizeRelativePath(archivePart.FullName));

                if (!destFile.Directory.Exists)
                    destFile.Directory.Create();

                part.ExtractToFile(destFile.FullName, true);
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error getting file from archive", ex);
            }
        }

        public override void Get(ArchivePart archivePart, FileInfo outFile)
        {
            base.Get(archivePart, outFile);
            try
            {
                var part = zArchive.Entries.FirstOrDefault(v => v.FullName == archivePart.FullName);
                if (part == null)
                    throw new ArchiveException("This part isn't in archive file.");

                part.ExtractToFile(outFile.FullName, true);
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error getting file from archive", ex);
            }
        }
    }
}