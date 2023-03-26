using System;
using System.IO;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files.Version.Core;

namespace DotNetAidLib.Core.Files.Version.Imp
{
    public class FileVersionParser : AbstractVersionParser
    {
        public Func<FileInfo, bool> isMatch;
        public Func<FileInfo, System.Version> versionFunction;

        public FileVersionParser(Func<FileInfo, bool> isMatch, Func<FileInfo, System.Version> versionFunction)
        {
            Assert.NotNull(isMatch, nameof(isMatch));
            Assert.NotNull(versionFunction, nameof(versionFunction));

            this.isMatch = isMatch;
            this.versionFunction = versionFunction;
        }

        public override System.Version GetVersion(FileInfo file)
        {
            Assert.Exists(file, nameof(file));
            return versionFunction.Invoke(file);
        }


        public override bool IsMatch(FileInfo file)
        {
            return isMatch.Invoke(file);
        }
    }
}