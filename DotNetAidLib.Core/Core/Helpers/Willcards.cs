using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Helpers
{
    public static class Willcards
    {
        public static bool SQLMatch(string pattern, string value)
        {
            Assert.NotNull(pattern, nameof(pattern));
            Assert.NotNull(value, nameof(value));

            pattern = pattern
                .Replace("%", ".*")
                .Replace("_", ".");
            return value.RegexIsMatch(pattern);
        }

        public static bool FileMatch(string pattern, string value)
        {
            Assert.NotNull(pattern, nameof(pattern));
            Assert.NotNull(value, nameof(value));

            pattern = pattern
                .Replace("*", ".*")
                .Replace("?", ".");
            return value.RegexIsMatch(pattern);
        }
    }
}