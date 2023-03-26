using System.Collections.Generic;
using System.IO;
using DotNetAidLib.Core.Files.Version.Imp;

namespace DotNetAidLib.Core.Files.Version.Core
{
    public class VersionFactory
    {
        private static VersionFactory instance;

        protected VersionFactory()
        {
            VersionParsers = new List<AbstractVersionParser>();

            VersionParsers.Add(new DotNetVersionParser());
            VersionParsers.Add(new JavaVersionParser());
            VersionParsers.Add(new PythonVersionParser());

            VersionParsers.Add(new ExecutableVersionParser(v => true, "-v"));
            VersionParsers.Add(new ExecutableVersionParser(v => true, "--version"));
        }

        public IList<AbstractVersionParser> VersionParsers { get; }

        public System.Version GetVersion(FileInfo file)
        {
            var ret = new System.Version(0, 0);

            foreach (var versionParser in VersionParsers)
                if (versionParser.IsMatch(file))
                {
                    ret = versionParser.GetVersion(file);
                    break;
                }

            return ret;
        }

        public static VersionFactory Instance()
        {
            if (instance == null)
                instance = new VersionFactory();

            return instance;
        }
    }
}