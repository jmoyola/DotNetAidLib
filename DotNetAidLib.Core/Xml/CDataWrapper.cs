using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.ComponentModel;

namespace DotNetAidLib.Core.Xml{
	[XmlSchemaProvider("GenerateSchema")]
	public sealed class CDataWrapper : IXmlSerializable
	{
		// implicit to/from string
		public static implicit operator string(CDataWrapper value)
		{
			return value == null ? null : value.Value;
		}

		public static implicit operator CDataWrapper(string value)
		{
			return value == null ? null : new CDataWrapper { Value = value };
		}

		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		// return "xs:string" as the type in scheme generation
		public static XmlQualifiedName GenerateSchema(XmlSchemaSet xs)
		{
			return XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String).QualifiedName;
		}

		// "" => <Node/>
		// "Foo" => <Node><![CDATA[Foo]]></Node>
		public void WriteXml(XmlWriter writer)
		{
			if (!string.IsNullOrEmpty(Value))
			{
                if(Value.IndexOfAny(new char[]{'<','>'})>-1)
				    writer.WriteCData(Value);
                else
                    writer.WriteString(Value);
			}
		}

		// <Node/> => ""
		// <Node></Node> => ""
		// <Node>Foo</Node> => "Foo"
		// <Node><![CDATA[Foo]]></Node> => "Foo"
		public void ReadXml(XmlReader reader)
		{
			if (reader.IsEmptyElement)
			{
				Value = "";
			}
			else
			{
				reader.Read();

				switch (reader.NodeType)
				{
				case XmlNodeType.EndElement:
					Value = ""; // empty after all...
					break;
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
					Value = reader.ReadContentAsString();
					break;
				default:
					throw new InvalidOperationException("Expected text/cdata");
				}
			}
		}

		// underlying value
		public string Value { get; set; }
		public override string ToString()
		{
			return Value;
		}
	}
}