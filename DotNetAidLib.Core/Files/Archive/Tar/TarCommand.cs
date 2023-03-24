using System.Collections.Generic;
using System;
using System.IO;
using DotNetAidLib.Core.Develop;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.IO.Archive.Core;

namespace DotNetAidLib.Core.IO.Archive.Tar
{
    public enum TarCompression {
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
        private FileInfo tarCommandFile = null;
        private static System.Text.RegularExpressions.Regex contentRegex = new System.Text.RegularExpressions.Regex (@"^([^\s]+)\s+([^/]+)/([^\s]+)\s+([^\s]+)\s+(\d\d\d\d-\d\d-\d\d\s\d\d:\d\d)\s+([^\s]+)$");
        public TarCommand()
            : this(false) { }

        public TarCommand(bool adminExecute)
            :this(EnvironmentHelper.SearchInPath("tar"), adminExecute){}
        
        public TarCommand(FileInfo tarCommandFile, bool adminExecute){
            Assert.Exists(tarCommandFile);
            this.tarCommandFile = tarCommandFile;
        }

        public void Create(DirectoryInfo source, FileInfo destination, bool recursive, TarCompression compression, bool relativeDestinationPath, bool includeContentFolder=true, bool numericOwners=false, String forceOwner=null, String forceGroup = null){
            try
            {
                Assert.Exists( source, nameof(source));

                string arguments = "";

                arguments += "-c";
                //arguments += " -v";
                arguments += (numericOwners?" --numeric-owner":"");

                arguments += (String.IsNullOrEmpty(forceOwner) ? "" : " --owner=" + forceOwner);
                arguments += (String.IsNullOrEmpty(forceGroup) ? "" : " --group=" + forceGroup);
                arguments += " --" + (recursive ? "" : "no-") + "recursion";

                arguments += GetCompressionflag(compression);
                arguments += " -f \"" + destination.FullName + "\"";
                if (relativeDestinationPath){
                    if (!includeContentFolder){
                        arguments += " -C \"" + source.FullName + "\"";
                        arguments += " .";
                    }
                    else{
                        arguments += " -C \"" + source.Parent.FullName + "\"";
                        arguments += " \"" + source.Name + "\"";
                    }
                }
                else
                    arguments += " \"" + source.FullName + "\"";


                this.tarCommandFile.CmdExecuteSync(arguments);

            }
            catch (Exception ex) {
                throw new ArchiveException("Error creating tar file: " + ex.Message, ex);
            }
        }

        public void Extract(FileInfo source, DirectoryInfo destination, bool preserveOwners, bool preservePermissions, TarCompression compression){
            try{
                Assert.Exists( source, nameof(source));

                string arguments = "-C \"" + destination.FullName + "\"";
                arguments += " -x";
                arguments += " --overwrite"; 
                //arguments += " -v";
                arguments += GetCompressionflag(compression);
                arguments += (preserveOwners ? " --same-owner" : "");
                arguments += (preservePermissions ? " -p" : "");

                arguments += " -f \"" + source.FullName + "\"";
                
                this.tarCommandFile.CmdExecuteSync(arguments);
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error extracting from tar file: " + ex.Message, ex);
            }

        }

        public IEnumerable<ArchivePart> Content(FileInfo tarfile, TarCompression compression)
        {
            try{
                IList<ArchivePart> ret = new List<ArchivePart>();

                string arguments = "-t";

                arguments += " -v"; // Habilita usuarios, permisos, etc...
                arguments += GetCompressionflag(compression);
                arguments += " -f \"" + tarfile.FullName + "\"";


                System.Diagnostics.Process p = this.tarCommandFile.GetCmdProcess(arguments);
                p.Start();
                while(!p.HasExited){
                    String line = p.StandardOutput.ReadLine();
                    Match m=contentRegex.Match(line);

                    if (m.Success)
                    {
                        ArchivePart ap = new ArchivePart(
                            Path.GetFileName(m.Groups[6].Value),
                            m.Groups[6].Value,
                            -1,
                            Int64.Parse(m.Groups[4].Value),
                            DateTime.ParseExact(m.Groups[5].Value, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture.DateTimeFormat));
                        ap.Permissions = m.Groups[1].Value;
                        ap.Owner = m.Groups[2].Value;
                        ap.Group = m.Groups[3].Value;
                        ret.Add(ap);
                    }
                }
                if (p.ExitCode != 0)
                    throw new Exception("Error executing command '" + p.ProcessName + (arguments == null ? "" : " " + arguments) + "': " + p.StandardError.ReadToEnd());
                    
                return ret;
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error listing tar file: " + ex.Message, ex);
            }
        }

        private string GetCompressionflag(TarCompression compression) {
            String ret="";

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