using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DotNetAidLib.Core.Serializer
{
    public class BinarySerializer : ISerializer
    {
        private readonly BinaryFormatter bf = new BinaryFormatter();

        public Stream Serialize<T>(T v)
        {
            var ms = new MemoryStream();
            bf.Serialize(ms, v);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public void Serialize<T>(T v, Stream output)
        {
            bf.Serialize(output, v);
        }

        public T Deserialize<T>(Stream v)
        {
            return (T) bf.Deserialize(v);
        }
    }
}