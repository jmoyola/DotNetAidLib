using System;

namespace DotNetAidLib.Configuration.ApplicationConfig.Core
{
    public interface IConfig<T>
    {
        DateTime DateOfCreation { get; }
        DateTime DateOfModification { get; }
        Version Version { get; }
        Type Type { get; }
        string Key { get; set; }
        string Info { get; set; }
        T Value { get; set; }
    }
}