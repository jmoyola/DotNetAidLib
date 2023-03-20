using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files.Version.Core;
using DotNetAidLib.Core.IO.Archive.Core;
using DotNetAidLib.Core.IO.Archive.Zip;
using DotNetAidLib.Core.Streams;
using DotNetAidLib.Core.Files;

namespace DotNetAidLib.Core.Files.Version.Imp
{
    public class JavaVersionParser : AbstractVersionParser
    {
        public override System.Version GetVersion(FileInfo file)
        {
            Assert.Exists(file, nameof(file));

            FileInfo tmpFile = null;
            ArchiveFile jaf = null;
            IArchiveFactory zf = null;

            try
            {
                zf = ZipArchiveFactory.Instance();
                jaf = zf.NewArchiveInstance(file);
                tmpFile = file.RandomTempFile();
                jaf.Open(ArchiveOpenMode.OpenRead);
                jaf.Get(new ArchivePart("MANIFEST.MF", "META-INF/MANIFEST.MF"), tmpFile);
                String version = tmpFile.OpenText().Grep("Bundle-Version", true).FirstOrDefault();
                return Parse(version);
            }
            catch {
                return null;
            }
            finally
            {
                jaf.Close();
                tmpFile.Delete();
            }
        }

        private static Regex javaFile = new Regex(@".jar$", RegexOptions.IgnoreCase);
        public override bool IsMatch(FileInfo file)
        {
            return javaFile.IsMatch(file.Name);
        }
    }

}
