using System;
using System.Collections.Generic;

namespace DotNetAidLib.Configuration.ApplicationConfig.Core
{
    public interface IApplicationConfig : IApplicationConfigGroup
    {
        DateTime? LastSavedTime { get; }
        List<Type> KnownTypes { get; }
        void Load();
        void Save();
    }
}