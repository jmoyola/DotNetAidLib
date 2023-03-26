using System;
using System.IO;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files.Version.Core;

namespace DotNetAidLib.Core.Files.Version.Imp
{
    public class ExecutableVersionParser : AbstractVersionParser
    {
        public Func<FileInfo, bool> isMatch;
        public Func<string, System.Version> outputPostprocess;
        public string parameter;

        public ExecutableVersionParser(Func<FileInfo, bool> isMatch)
            : this(isMatch, null, null)
        {
        }

        public ExecutableVersionParser(Func<FileInfo, bool> isMatch, string parameter)
            : this(isMatch, parameter, null)
        {
        }

        public ExecutableVersionParser(Func<FileInfo, bool> isMatch, string parameter,
            Func<string, System.Version> outputPostprocess)
        {
            Assert.NotNull(isMatch, nameof(isMatch));
            this.isMatch = isMatch;

            this.parameter = parameter;
            this.outputPostprocess = outputPostprocess;
        }

        public override System.Version GetVersion(FileInfo file)
        {
            Assert.Exists(file, nameof(file));
            try
            {
                string ret;

                ret = file.CmdExecuteSync(parameter, 2000);

                if (outputPostprocess != null)
                    return outputPostprocess.Invoke(ret);
                return Parse(ret);
            }
            catch
            {
                return null;
            }
        }


        public override bool IsMatch(FileInfo file)
        {
            return file.IsExecutable() && isMatch.Invoke(file);
        }
    }
}