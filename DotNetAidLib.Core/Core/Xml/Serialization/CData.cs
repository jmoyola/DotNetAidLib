using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Xml.Serialization
{
    public class CData : IXmlSerializable
    {
        private string value;

        public CData(string value)
        {
            Value = value;
        }

        public string Value
        {
            get => value;
            set
            {
                Assert.NotNull(value, nameof(value));
                this.value = value;
            }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteCData(value);
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            value = reader.ReadElementContentAsString();
        }

        public static implicit operator CData(string v)
        {
            return new CData(v);
        }

        public static implicit operator string(CData v)
        {
            return v.value;
        }

        public override string ToString()
        {
            return value;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null &&
                ((obj is string && value.Equals((string) obj))
                 ||
                 (typeof(CData).IsAssignableFrom(obj.GetType()) && value.Equals(((CData) obj).value))))
                return true;
            return false;
        }

        public bool Equals(string value)
        {
            return this.value.Equals(value);
        }

        public bool Equals(string value, StringComparison comparison)
        {
            return this.value.Equals(value, comparison);
        }
    }
}