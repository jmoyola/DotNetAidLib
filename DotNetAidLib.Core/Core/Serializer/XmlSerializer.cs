using System.IO;

namespace DotNetAidLib.Core.Serializer
{
    public class XmlSerializer : ISerializer
    {
        private System.Xml.Serialization.XmlSerializer bf;

        public Stream Serialize<T>(T v)
        {
            var ms = new MemoryStream();
            bf = new System.Xml.Serialization.XmlSerializer(typeof(T));
            bf.Serialize(ms, v);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public void Serialize<T>(T v, Stream output)
        {
            bf = new System.Xml.Serialization.XmlSerializer(typeof(T));
            bf.Serialize(output, v);
        }

        public T Deserialize<T>(Stream v)
        {
            bf = new System.Xml.Serialization.XmlSerializer(typeof(T));
            return (T) bf.Deserialize(v);
        }
    }
}