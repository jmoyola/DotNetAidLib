using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using DotNetAidLib.Core.IO.Archive.Core;
using DotNetAidLib.Core.IO.Archive.Tar;

namespace DotNetAidLib.Core.IO.Archive.Zip
{
    public class GZipArchiveFile : ArchiveFile
    {
        private GZipStream gzipFileStream;
        private TarReader tarReader;
        private TarWriter tarWriter;

        public GZipArchiveFile(FileInfo archiveFile) : base(archiveFile)
        {
        }

        public override void Open(ArchiveOpenMode openMode)
        {
            base.Open(openMode);

            try
            {
                if (openMode.Equals(ArchiveOpenMode.OpenCreate))
                {
                    gzipFileStream = new GZipStream(new FileStream(File.FullName, FileMode.OpenOrCreate),
                        CompressionLevelParse(CompressionLevel));
                    tarWriter = new TarWriter(gzipFileStream);
                }
                else if (openMode.Equals(ArchiveOpenMode.OpenRead))
                {
                    gzipFileStream = new GZipStream(new FileStream(File.FullName, FileMode.Open),
                        CompressionLevelParse(CompressionLevel));
                    tarReader = new TarReader(gzipFileStream);
                }
                else if (openMode.Equals(ArchiveOpenMode.OpenUpdate))
                {
                    throw new ArchiveException("Operation not supported.");
                }

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
                gzipFileStream.Close();
                if (tarWriter != null)
                    tarWriter.Dispose();

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

            FileStream fs = null;
            try
            {
                if (!_OpenMode.Equals(ArchiveOpenMode.OpenCreate))
                    throw new ArchiveException("Archive file open mode is not in OpenCreate mode.");

                var username = "username";
                var groupname = "groupname";
                var permissions = Convert.ToInt32("0777", 8);

                /*
                if (!Helpers.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)()){
                    Mono.Unix.UnixFileInfo uFile=new Mono.Unix.UnixFileInfo(fileToAdd.FullName);
                    username = uFile.OwnerUser.UserName;
                    groupname = uFile.OwnerGroup.GroupName;
                    permissions = (int)uFile.FileSpecialAttributes + (int)uFile.FileAccessPermissions;
                }
                */

                fs = fileToAdd.Open(FileMode.Open);
                tarWriter.Write(fs, fileToAdd.Length, fileToAdd.Name, username, groupname, permissions,
                    fileToAdd.LastWriteTime);
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error adding file to archive", ex);
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }

        public override void Remove(ArchivePart archivePart)
        {
            throw new ArchiveException("Operation not supported.");
        }

        public override IList<ArchivePart> Content()
        {
            var ret = base.Content();

            try
            {
                if (!_OpenMode.Equals(ArchiveOpenMode.OpenRead))
                    throw new ArchiveException("Archive file open mode is not in OpenRead mode.");

                while (tarReader.MoveNext(true))
                    ret.Add(new ArchivePart(tarReader.FileInfo.FileName, tarReader.FileInfo.FileName,
                        tarReader.FileInfo.HeaderSize, tarReader.FileInfo.SizeInBytes,
                        tarReader.FileInfo.LastModification));

                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting content from archive", ex);
            }
        }

        public override void Get(ArchivePart archivePart, DirectoryInfo outDirectory, bool recursive = false)
        {
            base.Get(archivePart, outDirectory, recursive);
            FileStream fs = null;

            try
            {
                while (tarReader.MoveNext(false))
                    if (archivePart.FullName == tarReader.FileInfo.FileName)
                    {
                        var destFile = new FileInfo(Path.Combine(outDirectory.FullName, archivePart.FullName));
                        if (!destFile.Directory.Exists)
                            destFile.Directory.Create();

                        fs = destFile.OpenWrite();
                        tarReader.Read(fs);
                        break;
                    }

                if (fs == null)
                    throw new ArchiveException("This part isn't in archive file.");
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error getting file from archive", ex);
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }

        public override void Get(ArchivePart archivePart, FileInfo outFile)
        {
            base.Get(archivePart, outFile);
            FileStream fs = null;

            try
            {
                while (tarReader.MoveNext(false))
                    if (archivePart.FullName == tarReader.FileInfo.FileName)
                    {
                        fs = outFile.OpenWrite();
                        tarReader.Read(fs);
                        break;
                    }

                if (fs == null)
                    throw new ArchiveException("This part isn't in archive file.");
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error getting file from archive", ex);
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }
    }
}