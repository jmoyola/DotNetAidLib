using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

namespace DotNetAidLib.Core.Serializer
{
    public class JsonDCSerializer : ISerializer
    {
        protected List<Type> _KnowTypes = new List<Type>();

        private DataContractJsonSerializer bf;

        public JsonDCSerializer()
        {
        }

        public JsonDCSerializer(List<Type> knowTypes)
        {
            KnowTypes = knowTypes;
        }

        public List<Type> KnowTypes
        {
            get => _KnowTypes;
            set => _KnowTypes = value;
        }

        public Stream Serialize<T>(T v)
        {
            var ms = new MemoryStream();
            bf = new DataContractJsonSerializer(typeof(T), _KnowTypes);
            bf.WriteObject(ms, v);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public void Serialize<T>(T v, Stream output)
        {
            bf = new DataContractJsonSerializer(typeof(T), _KnowTypes);
            bf.WriteObject(output, v);
        }

        public T Deserialize<T>(Stream v)
        {
            bf = new DataContractJsonSerializer(typeof(T), _KnowTypes);
            return (T) bf.ReadObject(v);
        }
    }
}