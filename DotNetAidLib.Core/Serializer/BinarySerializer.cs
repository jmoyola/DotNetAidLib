using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace DotNetAidLib.Core.Serializer
{
    public class BinarySerializer : ISerializer
    {
        private BinaryFormatter bf = new BinaryFormatter();

        public System.IO.Stream Serialize<T>(T v)
        {
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, v);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public void Serialize<T>(T v, System.IO.Stream output)
        {
            bf.Serialize(output, v);
        }

        public T Deserialize<T>(System.IO.Stream v)
        {
            return ((T)(bf.Deserialize(v)));
        }
    }
}
