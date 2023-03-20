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
using System.Data.Common;
using DotNetAidLib.Core.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Core.Configuration.ApplicationConfig.Ado{
	public class AdoConfig<T> : IConfig<T>
	{
		protected DataRow m_DataRow = null;
		protected internal AdoConfig(DataRow dataRow)
		{
			m_DataRow = dataRow;
		}

		protected internal AdoConfig(DataRow dataRow, string key, T value) : this(dataRow)
		{
			this.Key = key;
			this.Value = value;
		}

		public DateTime DateOfCreation {
			get { return (DateTime)m_DataRow["dateOfCreation"]; }
		}

		public DateTime DateOfModification {
			get { return (DateTime)m_DataRow["dateOfModification"]; }
		}

		public Version Version {
			get { return new Version(m_DataRow["version"].ToString()); }
		}

		public Type Type {
			get {
				return Type.GetType(m_DataRow["valueType"].ToString());
			}
		}

		public string Key {
			get { return m_DataRow["configKey"].ToString(); }
			set { m_DataRow["configKey"] = value; }
		}

        public string Info
        {
            get {
                if (m_DataRow.Table.Columns.IndexOf("configInfo") == -1)
                    return null;
                else
                    return m_DataRow["configInfo"].ToString();
            }
            set {
                if (m_DataRow.Table.Columns.IndexOf("configInfo") > -1)
                    m_DataRow["configInfo"] = value;
            }
        }

        public T Value {
			get { return (T)XmlDeserializeFromString(m_DataRow["configValue"].ToString(), this.Type); }

			set {
				m_DataRow["configValue"] = XmlSerializeToString(value, typeof(T));

				// Se especifica el tipo de dato
				if ((value == null)) {
					m_DataRow["valueType"] = typeof(T).AssemblyQualifiedName;
				} else {
					m_DataRow["valueType"]= value.GetType().AssemblyQualifiedName;
				}

				// Se especifica la fecha de creación (si procede)

				if ((!m_DataRow.HasVersion(DataRowVersion.Original))) {
					m_DataRow["dateOfCreation"] = DateTime.Now;
				}

				// Se especifica la fecha de modificación
				m_DataRow["dateOfModification"] = DateTime.Now;

				// Se especifica la versión del ensamblado
				m_DataRow["version"] =typeof(T).Assembly.GetName().Version.ToString();
			}
		}

		public static string XmlSerializeToString(object value)
		{
			if ((value == null)) {
				return XmlSerializeToString(value, typeof(object));
			} else {
				return XmlSerializeToString(value, value.GetType());
			}

		}

		public static string XmlSerializeToString(object value, Type objectType)
		{
			try {
				if(AdoApplicationConfigFactory.KnownTypes.IndexOf(objectType)==-1)
					AdoApplicationConfigFactory.KnownTypes.Add(objectType);
				
				DataContractSerializer dcs = new DataContractSerializer(objectType, AdoApplicationConfigFactory.KnownTypes.ToArray());
				MemoryStream ms = new MemoryStream();
				dcs.WriteObject(ms, value);
				// Prueba
				ms.Seek(0, SeekOrigin.Begin);
				StreamReader sr = new StreamReader(ms);
				string ser = sr.ReadToEnd();
				ms.Close();

				return ser;
			} catch (Exception ex) {
				throw new ApplicationConfigException("Error serializing element to Xml." + ex.ToString());
			}

		}

		public static object XmlDeserializeFromString(string v, Type objectType)
		{
			try {
				if(AdoApplicationConfigFactory.KnownTypes.IndexOf(objectType)==-1)
					AdoApplicationConfigFactory.KnownTypes.Add(objectType);
				
				object ret = null;

				DataContractSerializer dcs = new DataContractSerializer(objectType, AdoApplicationConfigFactory.KnownTypes.ToArray());

				MemoryStream ms = new MemoryStream();
				StreamWriter sw = new StreamWriter(ms);
				sw.Write(v);
				sw.Flush();
				ms.Seek(0, SeekOrigin.Begin);
				ret = dcs.ReadObject(ms);
				ms.Close();
				return ret;
			} catch (Exception ex) {
				throw new ApplicationConfigException("Error deserializing element from Xml." + ex.ToString());
			}
		}

	}
}