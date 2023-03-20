using System;
using System.Diagnostics;
using System.IO;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files.Version.Core;
using DotNetAidLib.Core.Files;

namespace DotNetAidLib.Core.Files.Version.Imp
{
    public class ExecutableVersionParser : AbstractVersionParser
    {
        public String parameter;
        public Func<String, System.Version> outputPostprocess;
        public Func<FileInfo, bool> isMatch;

        public ExecutableVersionParser(Func<FileInfo, bool> isMatch)
            : this(isMatch, null, null) { }
        public ExecutableVersionParser(Func<FileInfo, bool> isMatch, String parameter)
            : this(isMatch, parameter, null) { }
        public ExecutableVersionParser(Func<FileInfo, bool> isMatch, String parameter, Func<String, System.Version> outputPostprocess)
        {
            Assert.NotNull( isMatch, nameof(isMatch));
            this.isMatch = isMatch;

            this.parameter = parameter;
            this.outputPostprocess = outputPostprocess;
        }

        public override System.Version GetVersion(FileInfo file)
        {
            Assert.Exists(file, nameof(file));
            try
            {
                String ret;

                ret = file.CmdExecuteSync(this.parameter, 2000);

                if (outputPostprocess != null)
                    return outputPostprocess.Invoke(ret);
                else
                    return Parse(ret);
                    
            }
            catch { return null; }
        }


        public override bool IsMatch(FileInfo file)
        {
            return file.IsExecutable() &&  this.isMatch.Invoke(file);
        }
    }
}
