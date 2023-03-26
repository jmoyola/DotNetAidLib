using System.Text.RegularExpressions;

namespace DotNetAidLib.Core.Files
{
    public class FileFilter
    {
        public FileFilter(string value)
        {
            Value = value;
        }

        public string Value { get; set; }

        public Regex GetRegex()
        {
            return new Regex(Value.Replace(".", "\\.").Replace("?", ".").Replace("*", ".*"));
        }

        public bool IsMatch(string fileName)
        {
            return GetRegex().IsMatch(fileName);
        }

        public static implicit operator FileFilter(string v)
        {
            return new FileFilter(v);
        }
    }
}