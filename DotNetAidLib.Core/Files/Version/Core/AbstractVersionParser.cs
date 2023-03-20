using System;
using System.IO;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.Files.Version.Core
{
    public abstract class AbstractVersionParser
    {
        private static String versionPattern = @"((\d+)(\.\d+)?(\.\d+)?(\.\d+)?)";

        public abstract bool IsMatch(System.IO.FileInfo file);
        public abstract System.Version GetVersion(System.IO.FileInfo file);

        public static String VersionPattern { get { return versionPattern; } }
        public static Regex VersionRegex { get; } = new Regex(versionPattern, RegexOptions.Multiline);

        public static System.Version Parse(String value)
        {
            if (VersionRegex.IsMatch(value))
                return Helper.TryFunc(() => new System.Version(VersionRegex.Match(value).Value), null);
            else
                return null;
        }
    }
}
