using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files.Version.Core;
using DotNetAidLib.Core.Streams;

namespace DotNetAidLib.Core.Files.Version.Imp
{
    public class PythonVersionParser : AbstractVersionParser
    {
        public override System.Version GetVersion(FileInfo file)
        {
            Assert.Exists(file, nameof(file));

            try
            {
                String version = file.OpenText().Grep("version='" + VersionPattern + "'", true).FirstOrDefault();
                return Parse(version);
            }
            catch { return null; }
        }

        private static Regex pyFile = new Regex(@".py$", RegexOptions.IgnoreCase);
        public override bool IsMatch(FileInfo file)
        {
            return pyFile.IsMatch(file.Name);
        }
    }
}
