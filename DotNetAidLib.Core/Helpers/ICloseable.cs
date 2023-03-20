using System;
namespace DotNetAidLib.Core.Helpers
{
    public interface ICloseable
    {
        bool IsOpen{ get; }
        void Open ();
        void Close ();
    }
}
