using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.IO.Archive.Core;

namespace DotNetAidLib.Core.IO.Archive.Tar
{
    public enum TarCompression
    {
        auto,
        none,
        bzip2,
        lzma,
        gzip,
        lzip,
        compress
    }

    public class TarCommand
    {
        private static readonly Regex contentRegex =
            new Regex(@"^([^\s]+)\s+([^/]+)/([^\s]+)\s+([^\s]+)\s+(\d\d\d\d-\d\d-\d\d\s\d\d:\d\d)\s+([^\s]+)$");

        private readonly FileInfo tarCommandFile;

        public TarCommand()
            : this(false)
        {
        }

        public TarCommand(bool adminExecute)
            : this(EnvironmentHelper.SearchInPath("tar"), adminExecute)
        {
        }

        public TarCommand(FileInfo tarCommandFile, bool adminExecute)
        {
            Assert.Exists(tarCommandFile);
            this.tarCommandFile = tarCommandFile;
        }

        public void Create(DirectoryInfo source, FileInfo destination, bool recursive, TarCompression compression,
            bool relativeDestinationPath, bool includeContentFolder = true, bool numericOwners = false,
            string forceOwner = null, string forceGroup = null)
        {
            try
            {
                Assert.Exists(source, nameof(source));

                var arguments = "";

                arguments += "-c";
                //arguments += " -v";
                arguments += numericOwners ? " --numeric-owner" : "";

                arguments += string.IsNullOrEmpty(forceOwner) ? "" : " --owner=" + forceOwner;
                arguments += string.IsNullOrEmpty(forceGroup) ? "" : " --group=" + forceGroup;
                arguments += " --" + (recursive ? "" : "no-") + "recursion";

                arguments += GetCompressionflag(compression);
                arguments += " -f \"" + destination.FullName + "\"";
                if (relativeDestinationPath)
                {
                    if (!includeContentFolder)
                    {
                        arguments += " -C \"" + source.FullName + "\"";
                        arguments += " .";
                    }
                    else
                    {
                        arguments += " -C \"" + source.Parent.FullName + "\"";
                        arguments += " \"" + source.Name + "\"";
                    }
                }
                else
                {
                    arguments += " \"" + source.FullName + "\"";
                }


                tarCommandFile.CmdExecuteSync(arguments);
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error creating tar file: " + ex.Message, ex);
            }
        }

        public void Extract(FileInfo source, DirectoryInfo destination, bool preserveOwners, bool preservePermissions,
            TarCompression compression)
        {
            try
            {
                Assert.Exists(source, nameof(source));

                var arguments = "-C \"" + destination.FullName + "\"";
                arguments += " -x";
                arguments += " --overwrite";
                //arguments += " -v";
                arguments += GetCompressionflag(compression);
                arguments += preserveOwners ? " --same-owner" : "";
                arguments += preservePermissions ? " -p" : "";

                arguments += " -f \"" + source.FullName + "\"";

                tarCommandFile.CmdExecuteSync(arguments);
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error extracting from tar file: " + ex.Message, ex);
            }
        }

        public IEnumerable<ArchivePart> Content(FileInfo tarfile, TarCompression compression)
        {
            try
            {
                IList<ArchivePart> ret = new List<ArchivePart>();

                var arguments = "-t";

                arguments += " -v"; // Habilita usuarios, permisos, etc...
                arguments += GetCompressionflag(compression);
                arguments += " -f \"" + tarfile.FullName + "\"";


                var p = tarCommandFile.GetCmdProcess(arguments);
                p.Start();
                while (!p.HasExited)
                {
                    var line = p.StandardOutput.ReadLine();
                    var m = contentRegex.Match(line);

                    if (m.Success)
                    {
                        var ap = new ArchivePart(
                            Path.GetFileName(m.Groups[6].Value),
                            m.Groups[6].Value,
                            -1,
                            long.Parse(m.Groups[4].Value),
                            DateTime.ParseExact(m.Groups[5].Value, "yyyy-MM-dd HH:mm",
                                CultureInfo.InvariantCulture.DateTimeFormat));
                        ap.Permissions = m.Groups[1].Value;
                        ap.Owner = m.Groups[2].Value;
                        ap.Group = m.Groups[3].Value;
                        ret.Add(ap);
                    }
                }

                if (p.ExitCode != 0)
                    throw new Exception("Error executing command '" + p.ProcessName +
                                        (arguments == null ? "" : " " + arguments) + "': " +
                                        p.StandardError.ReadToEnd());

                return ret;
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error listing tar file: " + ex.Message, ex);
            }
        }

        private string GetCompressionflag(TarCompression compression)
        {
            var ret = "";

            switch (compression)
            {
                case TarCompression.auto:
                    ret += " -a";
                    break;
                case TarCompression.bzip2:
                    ret += " -j";
                    break;
                case TarCompression.gzip:
                    ret += " -z";
                    break;
                case TarCompression.lzip:
                    ret += " --lzip";
                    break;
                case TarCompression.lzma:
                    ret += " -J";
                    break;
                case TarCompression.compress:
                    ret += " -Z";
                    break;
            }

            return ret;
        }
    }
}