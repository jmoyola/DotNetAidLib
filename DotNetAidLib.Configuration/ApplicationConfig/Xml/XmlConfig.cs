using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Xml;
using System.Runtime.Serialization;
using System.IO;
using System.Globalization;
using System.Reflection;
using DotNetAidLib.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Configuration.ApplicationConfig.Xml{
	public class XmlConfig<T> : IConfig<T>
	{
		protected XmlNode m_XmlBaseConfigurationNode = null;
		protected internal XmlConfig(XmlNode xmlBaseConfigurationNode)
		{
			m_XmlBaseConfigurationNode = xmlBaseConfigurationNode;
		}

		protected internal XmlConfig(XmlNode xmlBaseConfigurationNode, string key, T value) : this(xmlBaseConfigurationNode)
		{
			this.Key = key;
			this.Value = value;
		}

		public DateTime DateOfCreation {
			get { return DateTime.Parse(m_XmlBaseConfigurationNode.Attributes["dateOfCreation"].Value, CultureInfo.InvariantCulture.DateTimeFormat); }
		}

		public DateTime DateOfModification {
			get { return DateTime.Parse(m_XmlBaseConfigurationNode.Attributes["dateOfModification"].Value, CultureInfo.InvariantCulture.DateTimeFormat); }
		}

		public Version Version {
			get { return new Version(m_XmlBaseConfigurationNode.Attributes["version"].Value); }
		}

		public Type Type {
			get {
				return Type.GetType (m_XmlBaseConfigurationNode.Attributes ["type"].Value);
			}
		}

		public string Key {
			get { return m_XmlBaseConfigurationNode.Attributes["key"].Value; }
			set {
				// Se especifica el nombre de la configuracion
				if ((m_XmlBaseConfigurationNode.Attributes["key"] == null)) {
					m_XmlBaseConfigurationNode.Attributes.Append((XmlAttribute)m_XmlBaseConfigurationNode.OwnerDocument.CreateNode(XmlNodeType.Attribute, "key", ""));
				}
				m_XmlBaseConfigurationNode.Attributes["key"].Value = value;
			}
		}

        public string Info
        {
            get {
                if (m_XmlBaseConfigurationNode.Attributes["info"] == null)
                    return null;
                else
                    return m_XmlBaseConfigurationNode.Attributes["info"].Value;
            }
            set
            {
                // Se especifica el nombre de la configuracion
                if ((m_XmlBaseConfigurationNode.Attributes["info"] == null))
                {
                    m_XmlBaseConfigurationNode.Attributes.Append((XmlAttribute)m_XmlBaseConfigurationNode.OwnerDocument.CreateNode(XmlNodeType.Attribute, "info", ""));
                }
                m_XmlBaseConfigurationNode.Attributes["info"].Value = value;
            }
        }

        public T Value {
			get { return (T)XmlDeserializeFromNode(m_XmlBaseConfigurationNode.FirstChild, this.Type); }
			set {
				if(value == null)
					m_XmlBaseConfigurationNode.AppendChild(m_XmlBaseConfigurationNode.OwnerDocument.ImportNode(XmlSerializeToNode(value, typeof(T)), true));
				else
					m_XmlBaseConfigurationNode.AppendChild(m_XmlBaseConfigurationNode.OwnerDocument.ImportNode(XmlSerializeToNode(value), true));
				// Se especifica el tipo de dato
				if ((m_XmlBaseConfigurationNode.Attributes["type"] == null)) {
					m_XmlBaseConfigurationNode.Attributes.Append((XmlAttribute)m_XmlBaseConfigurationNode.OwnerDocument.CreateNode(XmlNodeType.Attribute, "type", ""));
				}
				if (((value != null))) {
					m_XmlBaseConfigurationNode.Attributes["type"].Value = value.GetType().AssemblyQualifiedName;
				} else {
					m_XmlBaseConfigurationNode.Attributes["type"].Value = typeof(T).AssemblyQualifiedName;
				}


				// Se especifica la fecha de creación (si procede)
				if ((m_XmlBaseConfigurationNode.Attributes["dateOfCreation"] == null)) {
					m_XmlBaseConfigurationNode.Attributes.Append((XmlAttribute)m_XmlBaseConfigurationNode.OwnerDocument.CreateNode(XmlNodeType.Attribute, "dateOfCreation", ""));
					m_XmlBaseConfigurationNode.Attributes["dateOfCreation"].Value = DateTime.Now.ToString(CultureInfo.InvariantCulture.DateTimeFormat);
				}

				// Se especifica la fecha de modificación
				if ((m_XmlBaseConfigurationNode.Attributes["dateOfModification"] == null)) {
					m_XmlBaseConfigurationNode.Attributes.Append((XmlAttribute)m_XmlBaseConfigurationNode.OwnerDocument.CreateNode(XmlNodeType.Attribute, "dateOfModification", ""));
				}
				m_XmlBaseConfigurationNode.Attributes["dateOfModification"].Value = DateTime.Now.ToString(CultureInfo.InvariantCulture.DateTimeFormat);

				// Se especifica la versión del ensamblado
				if ((m_XmlBaseConfigurationNode.Attributes["version"] == null)) {
					m_XmlBaseConfigurationNode.Attributes.Append((XmlAttribute)m_XmlBaseConfigurationNode.OwnerDocument.CreateNode(XmlNodeType.Attribute, "version", ""));
				}
				m_XmlBaseConfigurationNode.Attributes["version"].Value =typeof(T).Assembly.GetName().Version.ToString();

			}
		}


		public static XmlNode XmlSerializeToNode(object value)
		{
			if ((value == null)) {
				return XmlSerializeToNode(value, typeof(object));
			} else {
				return XmlSerializeToNode(value, value.GetType());
			}
		}

		public static XmlNode XmlSerializeToNode(object value, Type objectType)
		{
			try {
				if(XmlApplicationConfigFactory.KnownTypes.IndexOf(objectType)==-1)
					XmlApplicationConfigFactory.KnownTypes.Add(objectType);
				
				DataContractSerializer dcs = new DataContractSerializer(objectType, XmlApplicationConfigFactory.KnownTypes.ToArray());
				MemoryStream ms = new MemoryStream();
				dcs.WriteObject(ms, value);
				// Prueba
				//ms.Seek(0, SeekOrigin.Begin);
				//StreamReader sr = new StreamReader(ms);
				//string ser = sr.ReadToEnd();

				ms.Seek(0, SeekOrigin.Begin);
				XmlDocument xdoc = new XmlDocument();
				xdoc.Load(ms);
				ms.Close();

				return xdoc.DocumentElement;
			} catch (Exception ex) {
				throw new ApplicationConfigException("Error serializing element to Xml.", ex);
			}

		}
			
		public static object XmlDeserializeFromNode(XmlNode xmlNode, Type objectType)
		{
			try {
				object ret = null;

				if(XmlApplicationConfigFactory.KnownTypes.IndexOf(objectType)==-1)
					XmlApplicationConfigFactory.KnownTypes.Add(objectType);

				DataContractSerializer dcs = new DataContractSerializer(objectType, XmlApplicationConfigFactory.KnownTypes.ToArray());
				XmlDocument xdoc = new XmlDocument();
				xdoc.AppendChild(xdoc.ImportNode(xmlNode, true));
				MemoryStream ms = new MemoryStream();
				xdoc.Save(ms);
				ms.Seek(0, SeekOrigin.Begin);
				ret = dcs.ReadObject(ms);
				ms.Close();
				return ret;
			} catch (Exception ex) {
				throw new ApplicationConfigException("Error deserializing element from Xml.", ex);
			}
		}

	}
}