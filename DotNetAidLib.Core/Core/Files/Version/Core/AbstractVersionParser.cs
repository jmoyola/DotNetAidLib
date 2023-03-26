using System.IO;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.Files.Version.Core
{
    public abstract class AbstractVersionParser
    {
        public static string VersionPattern { get; } = @"((\d+)(\.\d+)?(\.\d+)?(\.\d+)?)";

        public static Regex VersionRegex { get; } = new Regex(VersionPattern, RegexOptions.Multiline);

        public abstract bool IsMatch(FileInfo file);
        public abstract System.Version GetVersion(FileInfo file);

        public static System.Version Parse(string value)
        {
            if (VersionRegex.IsMatch(value))
                return Helper.TryFunc(() => new System.Version(VersionRegex.Match(value).Value), null);
            return null;
        }
    }
}