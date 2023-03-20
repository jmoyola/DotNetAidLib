using System;
using System.Xml.Serialization;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Xml.Serialization
{
    public class CData: IXmlSerializable
    {
        private string value;
        
        public CData(string value)
        {
            this.Value = value;
        }

        public string Value
        {
            get=>this.value;
            set
            {
                Assert.NotNull( value, nameof(value));
                this.value = value;
            }
        }

        void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteCData(this.value);
        }

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            this.value = reader.ReadElementContentAsString();
        }
        
        public static implicit operator CData(String v)
        {
            return new CData(v);
        }

        public static implicit operator String(CData v)
        {
            return v.value;
        }

        public override string ToString()
        {
            return this.value;
        }

        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null &&
                (obj is String && this.value.Equals((string)obj)
                ||
                typeof(CData).IsAssignableFrom(obj.GetType()) && this.value.Equals(((CData)obj).value)))
                return true;
            return false;
        }

        public bool Equals(String value)
        {
            return this.value.Equals(value);
        }
        
        public bool Equals(String value, StringComparison comparison)
        {
            return this.value.Equals(value, comparison);
        }
    }

}