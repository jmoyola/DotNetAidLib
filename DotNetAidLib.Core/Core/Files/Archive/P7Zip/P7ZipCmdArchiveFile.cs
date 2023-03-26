using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.IO.Archive.Core;

namespace DotNetAidLib.Core.IO.Archive.P7Zip
{
    public class P7ZipCmdArchiveFile : ArchiveFile
    {
        private static FileInfo _SystemP7ZipCmdFile;
        private readonly FileInfo _P7ZipCmdFile;

        public P7ZipCmdArchiveFile(FileInfo archiveFile)
            : this(SystemP7ZipCmdFile, archiveFile)
        {
        }

        public P7ZipCmdArchiveFile(FileInfo p7zipCmdFile, FileInfo archiveFile) : base(archiveFile)
        {
            if (p7zipCmdFile == null || !p7zipCmdFile.Exists)
                throw new ArchiveException("Can't find 7zip/p7zip" +
                                           (p7zipCmdFile == null ? "" : " in path '" + p7zipCmdFile.FullName + "'") +
                                           ".");
            _P7ZipCmdFile = p7zipCmdFile;
            compressionLevel = 5;
        }

        public static FileInfo SystemP7ZipCmdFile
        {
            get
            {
                if (_SystemP7ZipCmdFile == null)
                {
                    // Preparamos la ruta al ejecutable del compresor p7zip
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        _SystemP7ZipCmdFile =
                            new FileInfo(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName +
                                         Path.DirectorySeparatorChar + "7za.exe");
                        if (_SystemP7ZipCmdFile == null)
                            _SystemP7ZipCmdFile = EnvironmentHelper.SearchInPath("7za.exe");
                    }
                    else
                    {
                        _SystemP7ZipCmdFile = new FileInfo(new FileInfo(Path.DirectorySeparatorChar + "usr" +
                                                                        Path.DirectorySeparatorChar + "bin" +
                                                                        Path.DirectorySeparatorChar + "7z").FullName);
                        if (_SystemP7ZipCmdFile == null)
                            _SystemP7ZipCmdFile = EnvironmentHelper.SearchInPath("7z");
                    }
                }

                return _SystemP7ZipCmdFile;
            }
        }

        public override void Open(ArchiveOpenMode openMode)
        {
            base.Open(openMode);
            _OpenMode = openMode;
        }

        public override void Close()
        {
            base.Close();
            _OpenMode = ArchiveOpenMode.Close;
            ;
        }

        public override void Add(FileInfo fileToAdd, DirectoryInfo relativePath)
        {
            base.Add(fileToAdd, relativePath);
            System.Diagnostics.Process p = null;
            try
            {
                var relativePathAux = relativePath.FullName;
                if (!relativePathAux.EndsWith("" + Path.DirectorySeparatorChar))
                    relativePathAux += Path.DirectorySeparatorChar;

                p = _P7ZipCmdFile.GetCmdProcess("a -y \"" + _File.FullName + "\"" +
                                                (string.IsNullOrEmpty(Password) ? "" : " -p" + Password + " -mhc") +
                                                " -mx=" + CompressionLevel + " \"" +
                                                fileToAdd.FullName.Substring(relativePathAux.Length) + "\"");
                p.StartInfo.WorkingDirectory = relativePath.FullName;
                p.Start();
                p.WaitForExit();
                if (p.ExitCode != 0)
                    throw new ArchiveException(p.StandardError.ReadToEnd());
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error adding file to archive", ex);
            }
            finally
            {
                p.Dispose();
            }
        }

        public override void Remove(ArchivePart archivePart)
        {
            base.Remove(archivePart);
            System.Diagnostics.Process p = null;
            try
            {
                p = _P7ZipCmdFile.GetCmdProcess("d -y \"" + _File.FullName + "\"" +
                                                (string.IsNullOrEmpty(Password) ? "" : " -p" + Password + " -mhc") +
                                                " \"" + archivePart.FullName + "\"");
                p.Start();
                p.WaitForExit();
                if (p.ExitCode != 0)
                    throw new ArchiveException(p.StandardError.ReadToEnd());
            }
            catch (Exception ex)
            {
                throw new Exception("Error removing file from archive", ex);
            }
            finally
            {
                p.Dispose();
            }
        }

        public override IList<ArchivePart> Content()
        {
            System.Diagnostics.Process p = null;
            var ret = base.Content();
            var p7zOutputListRegex =
                new Regex(@"^(\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2})\s([^\s]+)\s([\s\d]{12})\s([\s\d]{12})\s(.+)$",
                    RegexOptions.Multiline);
            try
            {
                p = _P7ZipCmdFile.GetCmdProcess("l \"" + _File.FullName + "\"" +
                                                (string.IsNullOrEmpty(Password)
                                                    ? ""
                                                    : " -p" + Password)); // + " -mhc"));
                p.Start();
                p.WaitForExit();
                if (p.ExitCode != 0)
                    throw new ArchiveException(p.StandardError.ReadToEnd());
                var contentlist = p.StandardOutput.ReadToEnd();

                foreach (Match m in p7zOutputListRegex.Matches(contentlist))
                {
                    var ap = new ArchivePart(
                        Path.GetFileName(m.Groups[5].Value.Trim()),
                        m.Groups[5].Value.Trim(),
                        0,
                        long.Parse(m.Groups[3].Value.Trim()),
                        DateTime.ParseExact(m.Groups[1].Value.Trim(), "yyyy-MM-dd HH:mm:ss",
                            CultureInfo.InvariantCulture.DateTimeFormat)
                    );
                    long compressedLength = 0;
                    long.TryParse(m.Groups[4].Value.Trim(), out compressedLength);
                    ap.CompressedLength = compressedLength;

                    var attrs = m.Groups[2].Value.Trim();

                    ap.Attributes = attrs.CharPositionToEnumFlag<ArchivePartAttributes>('.');
                    ret.Add(ap);
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting content from archive", ex);
            }
            finally
            {
                p.Dispose();
            }
        }

        public override void Get(ArchivePart archivePart, DirectoryInfo outDirectory, bool recursive = false)
        {
            System.Diagnostics.Process p = null;
            base.Get(archivePart, outDirectory, recursive);
            try
            {
                if ((archivePart.Attributes & ArchivePartAttributes.Directory) == ArchivePartAttributes.Directory)
                {
                    if (recursive)
                    {
                        p = _P7ZipCmdFile.GetCmdProcess("x" + " -y -o\"" + outDirectory.FullName + "\"" + " \"" +
                                                        _File.FullName + "\"" +
                                                        (string.IsNullOrEmpty(Password)
                                                            ? ""
                                                            : " -p" + Password + " -mhc") + " \"" +
                                                        archivePart.FullName + "\"");
                        p.Start();
                        p.WaitForExit();
                        if (p.ExitCode != 0)
                            throw new ArchiveException(p.StandardError.ReadToEnd());
                    }
                    else
                    {
                        new DirectoryInfo(Path.Combine(outDirectory.FullName, archivePart.FullName)).Create();
                    }
                }
                else
                {
                    Get(archivePart, new FileInfo(Path.Combine(outDirectory.FullName, archivePart.FullName)));
                }
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error getting file from archive", ex);
            }
            finally
            {
                if (p != null)
                    p.Dispose();
            }
        }

        public override void Get(ArchivePart archivePart, FileInfo outFile)
        {
            System.Diagnostics.Process p = null;
            base.Get(archivePart, outFile);
            try
            {
                p = _P7ZipCmdFile.GetCmdProcess("e" + " -y -o\"" + outFile.Directory.FullName + "\"" + " \"" +
                                                _File.FullName + "\"" +
                                                (string.IsNullOrEmpty(Password) ? "" : " -p" + Password + " -mhc") +
                                                " \"" + archivePart.FullName + "\"");
                p.Start();
                p.WaitForExit();
                if (p.ExitCode != 0)
                    throw new ArchiveException(p.StandardError.ReadToEnd());

                // Renombramos el archivo
                var fo = new FileInfo(outFile.Directory.FullName + Path.DirectorySeparatorChar + archivePart.Name);
                fo.MoveTo(outFile.FullName);
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error getting file from archive", ex);
            }
            finally
            {
                p.Dispose();
            }
        }

        public override void GetAll(DirectoryInfo outDirectory)
        {
            if (OpenMode.Equals(ArchiveOpenMode.Close))
                throw new ArchiveException("Archive is already closed.");
            if (!OpenMode.Equals(ArchiveOpenMode.OpenRead))
                throw new ArchiveException("Archive must be opened in read mode.");

            System.Diagnostics.Process p = null;

            try
            {
                p = _P7ZipCmdFile.GetCmdProcess("x" + " -y -o\"" + outDirectory.FullName + "\"" + " \"" +
                                                _File.FullName + "\"" +
                                                (string.IsNullOrEmpty(Password) ? "" : " -p" + Password + " -mhc"));
                p.Start();
                p.WaitForExit();
                if (p.ExitCode != 0)
                    throw new ArchiveException(p.StandardError.ReadToEnd());
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error getting all files from archive", ex);
            }
            finally
            {
                p.Dispose();
            }
        }
    }
}