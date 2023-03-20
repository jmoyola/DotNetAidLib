using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using DotNetAidLib.Core.Configuration.ApplicationConfig.Core;
using DotNetAidLib.Database;

namespace DotNetAidLib.Core.Configuration.ApplicationConfig.Ado{
	public class AdoApplicationConfig : AdoApplicationConfigGroup, IApplicationConfig
	{
		private static DbProviderFactory m_DbProvider = null;
		private static String FQN_CONFIG_TABLENAME = "configurationGroup";

		private static string ROOT_CONFIG_GROUP_NAME = "rootConfigurationGroup";
		private DbConnection m_Cnx = null;
		private DataTable m_Table = null;
		private DbDataAdapter m_DAadapter = null;

        private Nullable<DateTime> m_LastSavedTime = new Nullable<DateTime>();

        private DbCommandBuilder m_CmdBuilder = null;
		public AdoApplicationConfig(DbProviderFactory DbProvider, string connectionString, string userLevel)
            : base(null, null, userLevel)
		{

			m_DbProvider = DbProvider;
			m_Cnx = m_DbProvider.CreateConnection();
			m_Cnx.ConnectionString = connectionString;

			m_UserLevel = userLevel;

			this.InitSettings();
			this.Load();

			m_DataRow = m_Table.AsEnumerable().Where(row => DBNull.Value.Equals(row["ancestorUserLevel"]) && DBNull.Value.Equals(row["ancestorUserName"]) && DBNull.Value.Equals(row["ancestorNodeType"]) && "-1".Equals(row["ancestorConfigKey"])).FirstOrDefault();
		}

        public DateTime? LastSavedTime
        {
            get
            {
                return m_LastSavedTime;
            }
        }

        public List<Type> KnownTypes{
			get{
				return AdoApplicationConfigFactory.KnownTypes;
			}
		}

		public DbConnection Connection {
			get { return m_Cnx; }
		}

		public void Load()
		{
			try {
				InitSettings();

				m_Cnx.Open();

				m_DAadapter = m_DbProvider.CreateDataAdapter();
				m_CmdBuilder = m_DbProvider.CreateCommandBuilder();
				m_CmdBuilder.DataAdapter = m_DAadapter;
				DbCommand cmd = m_DbProvider.CreateCommand();
				cmd.CommandText = "SELECT * from " + FQN_CONFIG_TABLENAME + " WHERE userLevel='" + m_UserLevel + "' AND userName='" + Environment.UserName + "'";
				cmd.Connection = m_Cnx;

				m_DAadapter.SelectCommand = cmd;
				m_DAadapter.UpdateCommand = m_CmdBuilder.GetUpdateCommand();
				m_DAadapter.DeleteCommand = m_CmdBuilder.GetDeleteCommand();
				m_DAadapter.InsertCommand = m_CmdBuilder.GetInsertCommand();

				m_Table = new DataTable();
				m_DAadapter.Fill(m_Table);
			} catch (Exception ex) {
				throw new ApplicationConfigException("Error loading configuration from database.\r\n" + ex.ToString());
			} finally {
				try {
					m_Cnx.Close();
				} catch {
				}
			}

		}

		public void Save()
		{
			m_DAadapter.Update(m_Table);
            m_LastSavedTime = DateTime.Now;
        }

		protected virtual string SQLApplicationConfigTableCreateSentence(string tableName)
		{
			string cmdTx = "\nCREATE TABLE IF NOT EXISTS @TableName (\n    ancestorNodeType int null, \n    ancestorUserLevel varchar(256) null, \n    ancestorUserName varchar(256) null, \n    ancestorConfigKey varchar(128) not null, \n    nodeType int not null, \n    userLevel varchar(128) not null, \n    userName varchar(128) not null, \n    dateOfCreation datetime not null, \n    dateOfModification datetime not null, \n    configKey varchar(128) not null, \n    configInfo varchar(256) not null, \n    configValue text null, \n    valueType varchar(256) null, \n    version varchar(20) null, \n    PRIMARY KEY(ancestorConfigKey, nodeType, userLevel, userName, configKey)\n)\n".Replace("@TableName", tableName);

			return cmdTx;
		}

		private void InitSettings()
		{
			try {
				// Creamos la tabla si no existe
				m_Cnx.Open();

				DbCommand cmd = m_Cnx.CreateCommand();
				cmd.CommandText = this.SQLApplicationConfigTableCreateSentence(FQN_CONFIG_TABLENAME);
				cmd.ExecuteNonQuery();
				// Verificamos si existe nodo raíz para el usuario y nivel actual
				cmd.CommandText = "SELECT COUNT(userName) FROM " + FQN_CONFIG_TABLENAME + " WHERE ancestorUserName IS NULL AND ancestorUserLevel IS NULL AND ancestorNodeType IS NULL AND ancestorConfigKey='-1' AND nodeType=" + Convert.ToInt32(AdoNodeType._GROUP_NODE) + " AND userLevel='" + m_UserLevel + "' AND userName='" + Environment.UserName + "'";
				bool existeNodoRaiz = Convert.ToInt32(cmd.ExecuteScalar()) == 1;

				// Si no existe, lo creamos
				if ((!existeNodoRaiz)) {
					cmd.CommandText = "INSERT INTO " + FQN_CONFIG_TABLENAME + " (ancestorConfigKey, nodeType,userLevel,userName,dateOfCreation,dateOfModification,configKey,configInfo,configValue,valueType,version " + ") VALUES (" + "'-1'," + Convert.ToInt32(AdoNodeType._GROUP_NODE) + ",'" + m_UserLevel + "','" + Environment.UserName + "',now(),now(),'" + ROOT_CONFIG_GROUP_NAME + "',null,null,null,null)";

					cmd.ExecuteNonQuery();
				}
			} catch (Exception ex) {
				throw new ApplicationConfigException("Error creating config table in database.\r\n" + ex.ToString());
			} finally {
				try {
					m_Cnx.Close();
				} catch {
				}
			}
		}


		public static void InitProvider(DbConnection connection)
		{
			if ((m_DbProvider == null)) {
				string name_space = connection.GetType().Namespace;
				m_DbProvider = DbProviderFactories.GetFactory(name_space);
				if ((m_DbProvider == null)) {
					throw new ApplicationConfigException("Could not locate factory matching supplied DbConnection");
				}
			}
		}
	}
}