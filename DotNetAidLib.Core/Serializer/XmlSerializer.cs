using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

namespace DotNetAidLib.Core.Serializer
{
    public class XmlSerializer : ISerializer
    {
        private System.Xml.Serialization.XmlSerializer bf = null;

        public System.IO.Stream Serialize<T>(T v)
        {
            MemoryStream ms = new MemoryStream();
            bf = new System.Xml.Serialization.XmlSerializer(typeof(T));
            bf.Serialize(ms, v);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public void Serialize<T>(T v, System.IO.Stream output)
        {
            bf = new System.Xml.Serialization.XmlSerializer(typeof(T));
            bf.Serialize(output, v);
        }

        public T Deserialize<T>(System.IO.Stream v)
        {
            bf = new System.Xml.Serialization.XmlSerializer(typeof(T));
            return ((T)(bf.Deserialize(v)));
        }
    }
}
