using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Serializer
{
    public class XmlDCSerializer : ISerializer
    {
        protected List<Type> _KnowTypes = new List<Type>();

        private DataContractSerializer bf = null;

        public XmlDCSerializer()
        {
        }

        public XmlDCSerializer(List<Type> knowTypes)
        {
            this.KnowTypes = knowTypes;
        }

        public List<Type> KnowTypes
        {
            get
            {
                return _KnowTypes;
            }
            set
            {
                _KnowTypes = value;
            }
        }

        public System.IO.Stream Serialize<T>(T v)
        {
            MemoryStream ms = new MemoryStream();
            bf = new DataContractSerializer(typeof(T), _KnowTypes);
            bf.WriteObject(ms, v);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public void Serialize<T>(T v, System.IO.Stream output)
        {
            bf = new DataContractSerializer(typeof(T), _KnowTypes);
            bf.WriteObject(output, v);
        }

        public T Deserialize<T>(System.IO.Stream v)
        {
            bf = new DataContractSerializer(typeof(T), _KnowTypes);
            return ((T)(bf.ReadObject(v)));
        }
    }
}
