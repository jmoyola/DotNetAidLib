using System;
namespace DotNetAidLib.Core.Helpers
{
    public interface IStartable
    {
        bool Started { get; }
        void Start ();
        void Stop ();
    }
}
