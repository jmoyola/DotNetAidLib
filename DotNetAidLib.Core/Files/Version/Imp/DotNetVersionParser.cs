using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files.Version.Core;


namespace DotNetAidLib.Core.Files.Version.Imp
{
    public class DotNetVersionParser : AbstractVersionParser
    {
        public override System.Version GetVersion(FileInfo file)
        {
            Assert.Exists(file, nameof(file));
            try
            {
                return new System.Version(FileVersionInfo.GetVersionInfo(file.FullName).FileVersion);
            }
            catch { return null; }
        }

        private static Regex dotNetFile = new Regex(@"(.exe)|(.dll)$", RegexOptions.IgnoreCase);
        public override bool IsMatch(FileInfo file)
        {
            return dotNetFile.IsMatch(file.Name);
        }
    }
}
