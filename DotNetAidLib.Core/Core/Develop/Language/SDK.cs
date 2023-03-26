using System;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Develop.Language
{
    public struct ProgrammingLanguage
    {
        public ProgrammingLanguageType Type;
        public int Version;
    }

    public enum ProgrammingLanguageType
    {
        CSharp,
        VBNet,
        Java,
        Python,
        PHP
    }

    public interface ISDK
    {
        IEnumerable<ProgrammingLanguage> Languages { get; }
        Version Version { get; }
    }

    public class NetFrameworkSDK : ISDK
    {
        public IEnumerable<ProgrammingLanguage> Languages => new[]
        {
            new ProgrammingLanguage {Type = ProgrammingLanguageType.CSharp, Version = 6},
            new ProgrammingLanguage {Type = ProgrammingLanguageType.VBNet, Version = 6}
        };

        public Version Version => new Version(4, 7, 2);
    }

    public class NetCoreSDK : ISDK
    {
        public IEnumerable<ProgrammingLanguage> Languages => new[]
        {
            new ProgrammingLanguage {Type = ProgrammingLanguageType.CSharp, Version = 6},
            new ProgrammingLanguage {Type = ProgrammingLanguageType.VBNet, Version = 6}
        };

        public Version Version => new Version(5, 0);
    }

    public class NetStandardSDK : ISDK
    {
        public IEnumerable<ProgrammingLanguage> Languages => new[]
        {
            new ProgrammingLanguage {Type = ProgrammingLanguageType.CSharp, Version = 6},
            new ProgrammingLanguage {Type = ProgrammingLanguageType.VBNet, Version = 6}
        };

        public Version Version => new Version(2, 0);
    }

    public class JavaSDK : ISDK
    {
        public IEnumerable<ProgrammingLanguage> Languages => new[]
        {
            new ProgrammingLanguage {Type = ProgrammingLanguageType.Java, Version = 8}
        };

        public Version Version => new Version(11, 0);
    }
}