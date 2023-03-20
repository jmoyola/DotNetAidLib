using System;
using System.Collections.Generic;
using System.IO;
using DotNetAidLib.Core.Files.Version.Imp;

namespace DotNetAidLib.Core.Files.Version.Core
{
    public class VersionFactory
    {
        private static VersionFactory instance = null;

        private IList<AbstractVersionParser> versionParsers = null;

        protected VersionFactory()
        {
            this.versionParsers = new List<AbstractVersionParser>();

            versionParsers.Add(new DotNetVersionParser());
            versionParsers.Add(new JavaVersionParser());
            versionParsers.Add(new PythonVersionParser());

            versionParsers.Add(new ExecutableVersionParser(v => true, "-v"));
            versionParsers.Add(new ExecutableVersionParser(v => true, "--version"));

        }

        public IList<AbstractVersionParser> VersionParsers {
            get { return versionParsers; }
        }

        public System.Version GetVersion(FileInfo file) {
            System.Version ret = new System.Version(0, 0);

            foreach (AbstractVersionParser versionParser in versionParsers) {
                if (versionParser.IsMatch(file)){
                    ret = versionParser.GetVersion(file);
                    break;
                }
            }

            return ret;
        }

        public static VersionFactory Instance() {
            if (instance == null)
                instance = new VersionFactory();

            return instance;
        }
    }
}
