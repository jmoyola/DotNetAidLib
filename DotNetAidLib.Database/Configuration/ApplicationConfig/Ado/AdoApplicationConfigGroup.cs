using System;
using System.Collections.Generic;
using System.Data;

using System.Linq;
using DotNetAidLib.Core.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Core.Configuration.ApplicationConfig.Ado{
	public class AdoApplicationConfigGroup : AbstractApplicationConfigGroup
	{

		protected internal enum AdoNodeType
		{
			_GROUP_NODE = 0,
			_CONFIGURATION_NODE = 1
		}

		protected DataRow m_DataRow = null;

		protected string m_UserLevel;
		protected AdoApplicationConfigGroup(IApplicationConfigGroup parent,  DataRow dataRow, string userLevel)
            :base(parent)
		{
			m_DataRow = dataRow;
			m_UserLevel = userLevel;
		}

		public override DateTime DateOfCreation {
			get { return (DateTime)m_DataRow["dateOfCreation"]; }
		}

		public override DateTime DateOfModification {
			get { return (DateTime)m_DataRow["dateOfModification"]; }
		}

		public override string GroupName {
			get { return (String)m_DataRow["configKey"]; }
		}

        public override string GroupInfo
        {
            get {
                if (m_DataRow.Table.Columns.IndexOf("configInfo") > -1)
                    return (String)m_DataRow["configInfo"];
                else
                    return null;
            }
            set
            {
                if (m_DataRow.Table.Columns.IndexOf("configInfo") > -1)
                    m_DataRow["configInfo"]=value;
            }
        }

        public override IEnumerable<IApplicationConfigGroup> Groups {
			get {
				List<IApplicationConfigGroup> ret = new List<IApplicationConfigGroup>();

				IEnumerable<DataRow> rhijos = this.GetChildrems(AdoNodeType._GROUP_NODE);
				foreach (DataRow row in rhijos) {
					ret.Add(new AdoApplicationConfigGroup(this, row, m_UserLevel));
				}
				return ret;
			}
		}

        public override System.Collections.Generic.IEnumerable<IConfig<Object>> Configurations
        {
            get
            {
                List<AdoConfig<Object>> ret = new List<AdoConfig<Object>>();

                IEnumerable<DataRow> rhijos = this.GetChildrems(AdoNodeType._CONFIGURATION_NODE);
                foreach (DataRow row in rhijos)
                {
                    ret.Add(new AdoConfig<Object>(row));
                }
                return ret;
            }
        }

        public override IApplicationConfigGroup AddGroup(string groupName, bool ifNotExists)
		{

			DataRow retrow = this.GetChildrem(AdoNodeType._GROUP_NODE, groupName);

			// Si ya existe, error....
			if (((retrow != null))) {
				if ((!ifNotExists)) {
					throw new ApplicationConfigException("Subgroup with name '" + groupName + "' already exists in group '" + this.GroupName + "'.");
				}
			} else {
				// Creamos la fila de configuracion
				retrow = m_DataRow.Table.NewRow();

				// Se especifica el nombre del grupo de configuracion
				retrow["configKey"] = groupName;

                // Se especifica la fecha de creación del grupo de configuración
                retrow["dateOfCreation"] = DateTime.Now;

				// Se especifica la fecha de modificación del grupo de configuración
				retrow["dateOfModification"] = DateTime.Now;

				// Se especifica su padre
				retrow["ancestorNodeType"] = m_DataRow["nodeType"];
				retrow["ancestorUserLevel"] = m_DataRow["userLevel"];
				retrow["ancestorUserName"] = m_DataRow["userName"];
				retrow["ancestorConfigKey"] = m_DataRow["configKey"];

				//Especificamos que es un grupo
				retrow["nodeType"] = AdoNodeType._GROUP_NODE;

				//Especificamos su nivel
				retrow["userLevel"] = m_UserLevel;

				//Especificamos su nombre de usuario
				retrow["userName"] = Environment.UserName;

				// Se añade la fila de configuracion
				m_DataRow.Table.Rows.Add(retrow);
			}

			return new AdoApplicationConfigGroup(this, retrow, m_UserLevel);
		}

		public override IApplicationConfigGroup GetGroup(string groupName)
		{
			DataRow retrow = this.GetChildrem(AdoNodeType._GROUP_NODE, groupName);

			// Si no existe, error....
			if ((retrow == null)) {
				throw new ApplicationConfigException("Subgroup with name '" + groupName + "' don't exists in group '" + this.GroupName + "'.");
			}

			return new AdoApplicationConfigGroup(this, retrow, m_UserLevel);
		}

		public override void RemoveGroup(string groupName, bool ifExist)
		{
			DataRow retrow = this.GetChildrem(AdoNodeType._GROUP_NODE, groupName);

			// Si no existe, error....
			if ((retrow == null)) {
				if ((!ifExist)) {
					throw new ApplicationConfigException("Subgroup with name '" + groupName + "' don't exists in group '" + this.GroupName + "'.");
				}
			} else {
				// Creamos el grupo y borramos en profundidad
				IApplicationConfigGroup gAux = new AdoApplicationConfigGroup(this, retrow, m_UserLevel);
				gAux.RemoveAllGroups();
				gAux.RemoveAllConfigurations();
				retrow.Delete();
			}
		}

		public override void RemoveAllGroups()
		{
			IEnumerable<DataRow> rows = this.GetChildrems(AdoNodeType._GROUP_NODE);

			foreach (DataRow r in rows) {
				IApplicationConfigGroup gAux = new AdoApplicationConfigGroup(this, r, m_UserLevel);
				gAux.RemoveAllGroups();
				gAux.RemoveAllConfigurations();
				r.Delete();
			}
		}

		public override bool GroupExist(string groupName)
		{
			DataRow retrow = this.GetChildrem(AdoNodeType._GROUP_NODE, groupName);
			return (retrow != null);
		}

		public override Dictionary<string, Type> ConfigurationKeys {
			get {
				Dictionary<string, Type> ret = new Dictionary<string, Type>();

				IEnumerable<DataRow> rhijos = this.GetChildrems(AdoNodeType._CONFIGURATION_NODE);

				foreach (DataRow row in rhijos) {
					ret.Add(row["configKey"].ToString(), Type.GetType(row["valueType"].ToString()));
				}
				return ret;
			}
		}

		public override bool ConfigurationExist(string key)
		{
			DataRow retrow = this.GetChildrem(AdoNodeType._CONFIGURATION_NODE, key);
			return (retrow != null);
		}

		public override IConfig<T> GetConfiguration<T>(string key)
		{
			try {
				DataRow retrow = this.GetChildrem(AdoNodeType._CONFIGURATION_NODE, key);

				if ((retrow == null)) {
					throw new ApplicationConfigException("Configuration with key '" + key + "' don't exists in group '" + this.GroupName + "'.");
				} else {
					return new AdoConfig<T>(retrow);
				}
			} catch (Exception ex) {
				throw new ApplicationConfigException("Error getting configuration from Datarow element with '" + key + "' key in parent row node '" + this.GroupName + "'." + ex.ToString());
			}
		}

		public override IConfig<T> AddConfiguration<T>(string key, T value, bool modifyIfExist)
		{
			try {
				DataRow retrow = this.GetChildrem(AdoNodeType._CONFIGURATION_NODE, key);

				// Si ya existe, error....
				if (((retrow != null))) {
					if ((!modifyIfExist)) {
						throw new ApplicationConfigException("Subgroup with name '" + GroupName + "' already exists in group '" + this.GroupName + "'.");
					}
				} else {
					// Creamos la fila de configuracion
					retrow = m_DataRow.Table.NewRow();

					// Se especifica el nombre del grupo de configuracion
					retrow["configKey"] = key;

					// Se especifica la fecha de creación del grupo de configuración
					retrow["dateOfCreation"] = DateTime.Now;

					// Se especifica la fecha de modificación del grupo de configuración
					retrow["dateOfModification"] = DateTime.Now;

					// Se especifica su padre
					retrow["ancestorNodeType"] = m_DataRow["nodeType"];
					retrow["ancestorUserLevel"] = m_DataRow["userLevel"];
					retrow["ancestorUserName"] = m_DataRow["userName"];
					retrow["ancestorConfigKey"] = m_DataRow["configKey"];

					//Especificamos que es un grupo
					retrow["nodeType"] = AdoNodeType._CONFIGURATION_NODE;

					//Especificamos su nivel
					retrow["userLevel"] = m_UserLevel;

					//Especificamos su nombre de usuario
					retrow["userName"] = Environment.UserName;


					// Se añade la fila de configuracion
					m_DataRow.Table.Rows.Add(retrow);
				}

				return new AdoConfig<T>(retrow, key, value);
			} catch (Exception ex) {
				throw new ApplicationConfigException("Error adding configuration to datarow element with key '" + key + "' in parent row node '" + this.GroupName + "'." + ex.ToString());
			}
		}

		public override IConfig<T> SetConfiguration<T>(string key, T value, bool createIfNotExists)
		{
			try {
				DataRow retrow = this.GetChildrem(AdoNodeType._CONFIGURATION_NODE, key);

				// Si no existe la configuracion
				if ((retrow == null)) {
					// Si no se crea si no existe, error....
					if ((!createIfNotExists)) {
						throw new ApplicationConfigException("Configuration with key '" + key + "' don't exists in group '" + this.GroupName + "'.");
						// Si se crea si no existe, pues eso....
					} else {
						// Creamos la fila de configuracion
						retrow = m_DataRow.Table.NewRow();

						// Se especifica el nombre del grupo de configuracion
						retrow["configKey"] = key;

						// Se especifica la fecha de creación del grupo de configuración
						retrow["dateOfCreation"] = DateTime.Now;

						// Se especifica la fecha de modificación del grupo de configuración
						retrow["dateOfModification"] = DateTime.Now;

						// Se especifica su padre
						retrow["ancestorNodeType"] = m_DataRow["nodeType"];
						retrow["ancestorUserLevel"] = m_DataRow["userLevel"];
						retrow["ancestorUserName"] = m_DataRow["userName"];
						retrow["ancestorConfigKey"] = m_DataRow["configKey"];

						//Especificamos que es un grupo
						retrow["nodeType"] = AdoNodeType._CONFIGURATION_NODE;

						//Especificamos su nivel
						retrow["userLevel"] = m_UserLevel;

						//Especificamos su nombre de usuario
						retrow["userName"] = Environment.UserName;


						// Se añade la fila de configuracion
						m_DataRow.Table.Rows.Add(retrow);
					}
				}

				return new AdoConfig<T>(retrow, key, value);
			} catch (Exception ex) {
				throw new ApplicationConfigException("Error setting configuration to Xml element with key '" + key + "' in parent row node '" + this.GroupName + "'." + ex.ToString());
			}
		}

		public override void RemoveConfiguration(string key, bool ifExist)
		{
			DataRow retrow = this.GetChildrem(AdoNodeType._CONFIGURATION_NODE, key);
			// Si no existe la configuracion
			if ((retrow == null)) {
				// Si procede, error...
				if ((!ifExist)) {
					throw new ApplicationConfigException("Configuration with key '" + key + "' don't exists in group '" + this.GroupName + "'.");
				}
			} else {
				// Si existe, se borra de su padre....
				retrow.Delete();
			}


		}

		public override void RemoveAllConfigurations()
		{
			IEnumerable<DataRow> rhijos = this.GetChildrems(AdoNodeType._CONFIGURATION_NODE);

			foreach (DataRow drow in rhijos) {
				drow.Delete();
			}
		}

		private IEnumerable<DataRow> GetChildrems(AdoNodeType tipoNodo)
		{

			return m_DataRow.Table.AsEnumerable().Where(row => !(row.RowState.Equals(DataRowState.Deleted)) && !(row.RowState.Equals(DataRowState.Detached)) && m_DataRow["nodeType"].Equals(row["ancestorNodeType"]) && m_DataRow["userLevel"].Equals(row["ancestorUserLevel"]) && m_DataRow["userName"].Equals(row["ancestorUserName"]) && m_DataRow["configKey"].Equals(row["ancestorConfigKey"]) && Convert.ToInt32(tipoNodo) == Convert.ToInt32(row["nodeType"]) && m_UserLevel.Equals(m_DataRow["userLevel"]) && Environment.UserName.Equals(m_DataRow["userName"]));
		}


		private DataRow GetChildrem(AdoNodeType tipoNodo, string configKey)
		{
			return m_DataRow.Table.AsEnumerable().Where(row => !(row.RowState.Equals(DataRowState.Deleted)) && !(row.RowState.Equals(DataRowState.Detached)) && m_DataRow["nodeType"].Equals(row["ancestorNodeType"]) && m_DataRow["userLevel"].Equals(row["ancestorUserLevel"]) && m_DataRow["userName"].Equals(row["ancestorUserName"]) && m_DataRow["configKey"].Equals(row["ancestorConfigKey"]) && configKey.Equals(row["configKey"]) && Convert.ToInt32(tipoNodo).Equals(row["nodeType"]) && m_DataRow["userLevel"].Equals(m_UserLevel) && m_DataRow["userName"].Equals(Environment.UserName)).FirstOrDefault();
		}
	}
}