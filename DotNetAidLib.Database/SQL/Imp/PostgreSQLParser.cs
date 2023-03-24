using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Database.SQL.Core;

namespace DotNetAidLib.Database.SQL.Imp
{
	public class PostgreSQLParser:ISQLParser
	{
		private static ISQLParser _Instance = null;

		private PostgreSQLParser() { }

		public String ProviderFactoryName
		{
			get { return "Npgsq"; }
		}

        public String LastInsertId()
        {
            return "@@Identity";
        }

        public String CreateSchema(String schemaName, bool ifNotExists)
        {
			return "CREATE SCHEMA" + (ifNotExists?" IF NOT EXISTS":"") + " `" + schemaName + "`";  
		}

		public String CreateSchema(DataSet dSet, bool ifNotExists)
		{
			return this.CreateSchema(dSet.DataSetName, ifNotExists);
		}

		public String DropSchema(String schemaName, bool ifExists)
		{
			return "DROP SCHEMA" + (ifExists ? " IF EXISTS" : "") + " `" + schemaName + "`";
		}

		public String DropSchema(DataSet dSet, bool ifExists)
		{
			return this.DropSchema(dSet.DataSetName, ifExists);
		}

        public String CreateTable(DataTable dTable, bool ifNotExists)
        {
			String ret = "";

			foreach (DataColumn dc in dTable.Columns) {
				if (dc.DataType.IsEnum) {
					ret+="CREATE TYPE mood AS ENUM(" + Enum.GetValues(dc.DataType).Cast<String>().Select(v => "'" + v + "'").ToStringJoin(", ") + ");";
				}
			}
				
			ret+= "CREATE TABLE" + (ifNotExists?" IF NOT EXISTS":"") + " `" + dTable.DataSet.DataSetName + "`.`" + dTable.TableName + "` (";

			foreach (DataColumn dc in dTable.Columns)
				ret += DataColumnParse (dc) + ", ";

			ret += "PRIMARY KEY(" +String.Join(", ", dTable.PrimaryKey.Select(v=>v.ColumnName)) + ")";
			ret+=");";

			return ret;
		}

        public String DataColumnParse(DataColumn dc)
		{
			String ret = dc.ColumnName + " ";
			if (dc.DataType.Equals(typeof(String)))
			{
				if (dc.MaxLength == -1)
					ret += "TEXT";
				else
					ret += "VARCHAR(" + dc.MaxLength + ")";
			}
			else if (dc.DataType.Equals(typeof(DateTime)))
				ret += "TIMESTAMP";
			else if (dc.DataType.Equals(typeof(TimeSpan)))
				ret += "TIMESPAN";
			else if (dc.DataType.Equals(typeof(bool)))
				ret += "BOOL";
			else if (dc.DataType.Equals(typeof(UInt64)))
				ret += "UNSIGNED BIGINT";
			else if (dc.DataType.Equals(typeof(Int64)))
				ret += "BIGINT";
			else if (dc.DataType.Equals(typeof(UInt32)))
				ret += "UNSIGNED INTEGER";
			else if (dc.DataType.Equals(typeof(Int32)))
				ret += "INTEGER";
			else if (dc.DataType.Equals(typeof(UInt16)))
				ret += "UNSIGNED SMALLINT";
			else if (dc.DataType.Equals(typeof(Int16)))
				ret += "SMALLINT";
			else if (dc.DataType.Equals(typeof(byte)))
				ret += "SMALLINT";
			else if (dc.DataType.Equals(typeof(float)))
				ret += "REAL";
			else if (dc.DataType.Equals(typeof(double)))
				ret += "DOUBLE";
			else if (dc.DataType.Equals(typeof(decimal)))
				ret += "DECIMAL";
			else if (dc.DataType.IsEnum)
				ret += dc.DataType.Name;
			else if (dc.DataType.Equals(typeof(byte[])))
				ret += "BYTEA";
			else
				throw new Exception("Data column type '" + dc.DataType.Name + "' is not allowed for SQLParser '" + this.GetType().Name + "'.");

			ret += " " + (dc.AllowDBNull ? "" : "NOT ") + "NULL";

			if (dc.DefaultValue != null && !String.IsNullOrEmpty(dc.DefaultValue.ToString()))
				ret += " DEFAULT " + DataColumnValueParse(dc, dc.DefaultValue);

			return ret;
		}

		public String DataColumnValueParse(DataColumn dc, Object value)
		{
			String ret = "";
			if (value == null)
				ret = "NULL";
			else if (dc.DataType.Equals (typeof(String))) {
				ret += "'" + value.ToString() + "'";
			}
			else if (dc.DataType.Equals (typeof(DateTime)))
				ret += "'" + ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
			else if (dc.DataType.IsEnum)
				ret += "'" + ((Enum)value).ToString() + "'";
			else
				ret += value.ToString();
			return ret;
		}
        public String ActiveSchema(String schemaName)
        {
			return "SET search_path TO " + schemaName +";";
        }
		// pg_get_serial_sequence(table_name, column_name)
		//SELECT last_value FROM your_sequence_name;


		public String CurrentId(String tableName, String columnName)
		{
			return "SELECT last_value FROM pg_get_serial_sequence('" + tableName + "','" + columnName + "');";
		}

		public String ChangeCurrentId(String tableName, String columnName, UInt64 id)
		{
			return "ALTER SEQUENCE pg_get_serial_sequence('" + tableName + "','" + columnName + "') RESTART WITH " + id + ";";
		}

		public static ISQLParser Instance() {
			if (_Instance == null)
				_Instance = new PostgreSQLParser();

			return _Instance;
		}

        public IEnumerable<DbIndex> GetTableIndexes(IDbConnection cnx, String schemaName, String tableName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbConstrait> GetTableConstraits(IDbConnection cnx, String schemaName, String tableName) {
            throw new NotImplementedException();
        }

        public SQLUploaderFile GetContentFromFile(IDbConnection cnx, SourceType fileSource, String path)
        {
            return new PostgreSQLUploaderFile(cnx, fileSource, path);
        }

        public String Top(String select, int rowCount)
        {
            return select.Trim() + " LIMIT " + rowCount;
        }

        public String SequenceNextValue(String sequenceName)
        {
            return "nextval('" + sequenceName + "')";
        }

        public String SequenceCurrentValue(String sequenceName)
        {
            return "currval('" + sequenceName + "')";
        }

    }
}

