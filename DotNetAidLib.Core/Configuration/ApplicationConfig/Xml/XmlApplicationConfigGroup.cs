using System;
using System.Collections.Generic;
using System.Xml;
using System.Globalization;
using DotNetAidLib.Core.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Core.Configuration.ApplicationConfig.Xml{
	public class XmlApplicationConfigGroup : AbstractApplicationConfigGroup
	{
		private static string CONFIG_GROUP_NODE_NAME = "configurationGroup";

		private static string CONFIG_ELEMENT_NODE_NAME = "configurationElement";

		protected XmlNode m_XmlBaseGroupNode = null;
		protected XmlApplicationConfigGroup(IApplicationConfigGroup parent, XmlNode XmlBaseGroupNode)
            :base(parent)
		{
			m_XmlBaseGroupNode = XmlBaseGroupNode;
		}

		public override DateTime DateOfCreation {
			get { return DateTime.Parse(m_XmlBaseGroupNode.Attributes["dateOfCreation"].Value, CultureInfo.InvariantCulture.DateTimeFormat); }
		}

		public override DateTime DateOfModification {
			get { return DateTime.Parse(m_XmlBaseGroupNode.Attributes["dateOfModification"].Value, CultureInfo.InvariantCulture.DateTimeFormat); }
		}

		public override string GroupName {
			get { return m_XmlBaseGroupNode.Attributes["key"].Value; }
		}

        public override string GroupInfo
        {
            get {
                if (m_XmlBaseGroupNode.Attributes["info"] != null)
                    return m_XmlBaseGroupNode.Attributes["info"].Value;
                else
                    return null;
            }
            set
            {
                // Se especifica el nombre de la configuracion
                if ((m_XmlBaseGroupNode.Attributes["info"] == null))
                {
                    m_XmlBaseGroupNode.Attributes.Append((XmlAttribute)m_XmlBaseGroupNode.OwnerDocument.CreateNode(XmlNodeType.Attribute, "info", ""));
                }
                m_XmlBaseGroupNode.Attributes["info"].Value = value;
            }

        }

        public override IEnumerable<IApplicationConfigGroup> Groups {
			get {
				List<IApplicationConfigGroup> ret = new List<IApplicationConfigGroup>();
				foreach (XmlNode node in m_XmlBaseGroupNode.SelectNodes("./" + CONFIG_GROUP_NODE_NAME)) {
					ret.Add(new XmlApplicationConfigGroup(this, node));
				}
				return ret;
			}
		}

        public override System.Collections.Generic.IEnumerable<IConfig<Object>> Configurations {
            get {
                List<XmlConfig<Object>> ret = new List<XmlConfig<Object>>();
                foreach (XmlNode node in m_XmlBaseGroupNode.SelectNodes("./" + CONFIG_ELEMENT_NODE_NAME))
                {
                    ret.Add(new XmlConfig<Object>(node));
                }
                return ret;
            }
        }

        public override IApplicationConfigGroup AddGroup(string groupName, bool ifNotExists)
		{
			XmlNode node = m_XmlBaseGroupNode.SelectSingleNode("./" + CONFIG_GROUP_NODE_NAME + "[@key=\"" + groupName + "\"]");

			// Si ya existe, error....
			if (((node != null))) {
				if ((!ifNotExists)) {
					throw new ApplicationConfigException("Subgroup with name '" + groupName + "' already exists in group '" + this.GroupName + "'.");
				}
			} else {
				// Se añade el nodo de la configuracion
				node = m_XmlBaseGroupNode.OwnerDocument.CreateNode(XmlNodeType.Element, CONFIG_GROUP_NODE_NAME, "");
				m_XmlBaseGroupNode.AppendChild(node);

				// Se especifica el nombre del grupo de configuracion
				if ((node.Attributes["key"] == null)) {
					node.Attributes.Append((XmlAttribute)node.OwnerDocument.CreateNode(XmlNodeType.Attribute, "key", ""));
				}
				node.Attributes["key"].Value = groupName;

                // Se especifica el info del grupo de configuracion
                if ((node.Attributes["info"] == null))
                {
                    node.Attributes.Append((XmlAttribute)node.OwnerDocument.CreateNode(XmlNodeType.Attribute, "info", ""));
                }
                node.Attributes["info"].Value = null;

                // Se especifica la fecha de creación del grupo de configuración
                if ((node.Attributes["dateOfCreation"] == null)) {
					node.Attributes.Append((XmlAttribute)node.OwnerDocument.CreateNode(XmlNodeType.Attribute, "dateOfCreation", ""));
				}
				node.Attributes["dateOfCreation"].Value = DateTime.Now.ToString(CultureInfo.InvariantCulture.DateTimeFormat);

				// Se especifica la fecha de modificación del grupo de configuración
				if ((node.Attributes["dateOfModification"] == null)) {
					node.Attributes.Append((XmlAttribute)node.OwnerDocument.CreateNode(XmlNodeType.Attribute, "dateOfModification", ""));
				}
				node.Attributes["dateOfModification"].Value = DateTime.Now.ToString(CultureInfo.InvariantCulture.DateTimeFormat);
			}

			return new XmlApplicationConfigGroup(this, node);
		}

		public override IApplicationConfigGroup GetGroup(string groupName)
		{
			XmlNode node = m_XmlBaseGroupNode.SelectSingleNode("./" + CONFIG_GROUP_NODE_NAME + "[@key=\"" + groupName + "\"]");

			// Si no existe, error....
			if ((node == null)) {
				throw new ApplicationConfigException("Subgroup with name '" + groupName + "' don't exists in group '" + this.GroupName + "'.");
			}

			return new XmlApplicationConfigGroup(this, node);
		}

		public override void RemoveGroup(string groupName, bool ifExist)
		{
			XmlNode node = m_XmlBaseGroupNode.SelectSingleNode("./" + CONFIG_GROUP_NODE_NAME + "[@key=\"" + groupName + "\"]");

			// Si no existe, error....
			if ((node == null)) {
				if ((!ifExist)) {
					throw new ApplicationConfigException("Subgroup with name '" + groupName + "' don't exists in group '" + this.GroupName + "'.");
				}
			} else {
				m_XmlBaseGroupNode.RemoveChild(node);
			}
		}

		public override void RemoveAllGroups()
		{
			XmlNodeList nodes = m_XmlBaseGroupNode.SelectNodes("./" + CONFIG_GROUP_NODE_NAME);

			foreach (XmlNode node in nodes) {
				m_XmlBaseGroupNode.RemoveChild(node);
			}
		}

		public override bool GroupExist(string groupName)
		{
			XmlNode node = m_XmlBaseGroupNode.SelectSingleNode("./" + CONFIG_GROUP_NODE_NAME + "[@key=\"" + groupName + "\"]");
			return (node != null);
		}

		public override Dictionary<string, Type> ConfigurationKeys {
			get {
				Dictionary<string, Type> ret = new Dictionary<string, Type>();
				foreach (XmlNode node in m_XmlBaseGroupNode.SelectNodes("./" + CONFIG_ELEMENT_NODE_NAME)) {
					ret.Add(node.Attributes["key"].Value, Type.GetType(node.Attributes["type"].Value));
				}
				return ret;
			}
		}

		public override bool ConfigurationExist(string key)
		{
			return (m_XmlBaseGroupNode.SelectSingleNode("./" + CONFIG_ELEMENT_NODE_NAME + "[@key=\"" + key + "\"]") != null);
		}

		public override IConfig<T> GetConfiguration<T>(string key)
		{
			try {
				XmlNode node = m_XmlBaseGroupNode.SelectSingleNode("./" + CONFIG_ELEMENT_NODE_NAME + "[@key=\"" + key + "\"]");
				if ((node == null)) {
					throw new ApplicationConfigException("Configuration with key '" + key + "' don't exists in group '" + this.GroupName + "'.");
				} else {
					return new XmlConfig<T>(node);
				}
			} catch (Exception ex) {
				throw new ApplicationConfigException("Error getting configuration from Xml element with '" + key + "' key in parent xml node path '" + m_XmlBaseGroupNode.BaseURI + "'." + ex.ToString());
			}
		}

		//Public Overrides Function AddConfiguration(ByVal key As String, ByVal Value As Object, ByVal modifyIfExist As Boolean) As IConfig(Of Object)

		//End Function

		public override IConfig<T> AddConfiguration<T>(string key, T value, bool modifyIfExist)
		{
			try {
				XmlNode node = m_XmlBaseGroupNode.SelectSingleNode("./" + CONFIG_ELEMENT_NODE_NAME + "[@key=\"" + key + "\"]");

				// Si ya existe
				if (((node != null))) {
					// En caso de que se desee modificar, se elimina su contenido
					if ((modifyIfExist)) {
						node.RemoveAll();
						// Error en caso de que no se desee modificar
					} else {
						throw new ApplicationConfigException("Configuration with key '" + key + "' already exists in group '" + this.GroupName + "'.");
					}
				} else {
					// Se añade el nodo de la configuracion
					node = m_XmlBaseGroupNode.OwnerDocument.CreateNode(XmlNodeType.Element, CONFIG_ELEMENT_NODE_NAME, "");
					m_XmlBaseGroupNode.AppendChild(node);
				}

				return new XmlConfig<T>(node, key, value);
			} catch (Exception ex) {
				throw new ApplicationConfigException("Error adding configuration to Xml element with key '" + key + "' in parent xml node path '" + m_XmlBaseGroupNode.BaseURI + "'." + ex.ToString());
			}
		}

		public override IConfig<T> SetConfiguration<T>(string key, T value, bool createIfNotExists)
		{
			try {
				XmlNode node = m_XmlBaseGroupNode.SelectSingleNode("./" + CONFIG_ELEMENT_NODE_NAME + "[@key=\"" + key + "\"]");
				// Si no existe la configuracion
				if ((node == null)) {
					// Si no se crea si no existe, error....
					if ((!createIfNotExists)) {
						throw new ApplicationConfigException("Configuration with key '" + key + "' don't exists in group '" + this.GroupName + "'.");
						// Si se crea si no existe, pues eso....
					} else {
						node = m_XmlBaseGroupNode.OwnerDocument.CreateNode(XmlNodeType.Element, CONFIG_ELEMENT_NODE_NAME, "");
						m_XmlBaseGroupNode.AppendChild(node);
					}
					// Si existe, se borra todo su contenido
				} else {
					node.RemoveAll();
				}

				return new XmlConfig<T>(node, key, value);
			} catch (Exception ex) {
				throw new ApplicationConfigException("Error setting configuration to Xml element with key '" + key + "' in parent xml node path '" + m_XmlBaseGroupNode.BaseURI + "'." + ex.ToString());
			}
		}

		public override void RemoveConfiguration(string key, bool ifExist)
		{
			XmlNode node = m_XmlBaseGroupNode.SelectSingleNode("./" + CONFIG_ELEMENT_NODE_NAME + "[@key=\"" + key + "\"]");
			// Si no existe la configuracion
			if ((node == null)) {
				// Si procede, error...
				if ((!ifExist)) {
					throw new ApplicationConfigException("Configuration with key '" + key + "' don't exists in group '" + this.GroupName + "'.");
				}
			} else {
				// Si existe, se borra de su padre....
				node.ParentNode.RemoveChild(node);
			}
		}

		public override void RemoveAllConfigurations()
		{
			foreach (XmlNode node in m_XmlBaseGroupNode.SelectNodes("./" + CONFIG_ELEMENT_NODE_NAME)) {
				node.ParentNode.RemoveChild(node);
			}
		}
	}
}