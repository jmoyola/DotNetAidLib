using System;
using System.IO;
namespace DotNetAidLib.Core.Serializer
{
    public interface ISerializer
    {

        T Deserialize<T>(Stream v);

        Stream Serialize<T>(T v);

        void Serialize<T>(T v, Stream output);
    }
}
