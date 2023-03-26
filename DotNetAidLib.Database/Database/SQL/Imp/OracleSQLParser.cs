using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Database.SQL.Core;

namespace DotNetAidLib.Database.SQL.Imp
{
    public class OracleSQLParser : ISQLParser
    {
        private static ISQLParser _Instance;

        private OracleSQLParser()
        {
        }

        public string ProviderFactoryName => "odp.net";

        public string LastInsertId()
        {
            throw new NotImplementedException();
        }

        public string CreateSchema(string schemaName, bool ifNotExists)
        {
            return "CREATE SCHEMA" + (ifNotExists ? " IF NOT EXISTS" : "") + " `" + schemaName + "`";
        }

        public string CreateSchema(DataSet dSet, bool ifNotExists)
        {
            return CreateSchema(dSet.DataSetName, ifNotExists);
        }

        public string DropSchema(string schemaName, bool ifExists)
        {
            return "DROP SCHEMA" + (ifExists ? " IF EXISTS" : "") + " `" + schemaName + "`";
        }

        public string DropSchema(DataSet dSet, bool ifExists)
        {
            return DropSchema(dSet.DataSetName, ifExists);
        }

        public string CreateTable(DataTable dTable, bool ifNotExists)
        {
            var ret = "";

            foreach (DataColumn dc in dTable.Columns)
                if (dc.DataType.IsEnum)
                    ret += "CREATE TYPE mood AS ENUM(" + Enum.GetValues(dc.DataType).Cast<string>()
                        .Select(v => "'" + v + "'").ToStringJoin(", ") + ");";

            ret += "CREATE TABLE" + (ifNotExists ? " IF NOT EXISTS" : "") + " `" + dTable.DataSet.DataSetName + "`.`" +
                   dTable.TableName + "` (";

            foreach (DataColumn dc in dTable.Columns)
                ret += DataColumnParse(dc) + ", ";

            ret += "PRIMARY KEY(" + string.Join(", ", dTable.PrimaryKey.Select(v => v.ColumnName)) + ")";
            ret += ");";

            return ret;
        }

        public string DataColumnParse(DataColumn dc)
        {
            var ret = dc.ColumnName + " ";
            if (dc.DataType.Equals(typeof(string)))
            {
                if (dc.MaxLength == -1)
                    ret += "TEXT";
                else
                    ret += "VARCHAR(" + dc.MaxLength + ")";
            }
            else if (dc.DataType.Equals(typeof(DateTime)))
            {
                ret += "TIMESTAMP";
            }
            else if (dc.DataType.Equals(typeof(TimeSpan)))
            {
                ret += "TIMESPAN";
            }
            else if (dc.DataType.Equals(typeof(bool)))
            {
                ret += "BOOL";
            }
            else if (dc.DataType.Equals(typeof(ulong)))
            {
                ret += "UNSIGNED BIGINT";
            }
            else if (dc.DataType.Equals(typeof(long)))
            {
                ret += "BIGINT";
            }
            else if (dc.DataType.Equals(typeof(uint)))
            {
                ret += "UNSIGNED INTEGER";
            }
            else if (dc.DataType.Equals(typeof(int)))
            {
                ret += "INTEGER";
            }
            else if (dc.DataType.Equals(typeof(ushort)))
            {
                ret += "UNSIGNED SMALLINT";
            }
            else if (dc.DataType.Equals(typeof(short)))
            {
                ret += "SMALLINT";
            }
            else if (dc.DataType.Equals(typeof(byte)))
            {
                ret += "SMALLINT";
            }
            else if (dc.DataType.Equals(typeof(float)))
            {
                ret += "REAL";
            }
            else if (dc.DataType.Equals(typeof(double)))
            {
                ret += "DOUBLE";
            }
            else if (dc.DataType.Equals(typeof(decimal)))
            {
                ret += "DECIMAL";
            }
            else if (dc.DataType.IsEnum)
            {
                ret += dc.DataType.Name;
            }
            else if (dc.DataType.Equals(typeof(byte[])))
            {
                ret += "BYTEA";
            }
            else
            {
                throw new Exception("Data column type '" + dc.DataType.Name + "' is not allowed for SQLParser '" +
                                    GetType().Name + "'.");
            }

            ret += " " + (dc.AllowDBNull ? "" : "NOT ") + "NULL";

            if (dc.DefaultValue != null && !string.IsNullOrEmpty(dc.DefaultValue.ToString()))
                ret += " DEFAULT " + DataColumnValueParse(dc, dc.DefaultValue);

            return ret;
        }

        public string DataColumnValueParse(DataColumn dc, object value)
        {
            var ret = "";
            if (value == null)
                ret = "NULL";
            else if (dc.DataType.Equals(typeof(string)))
                ret += "'" + value + "'";
            else if (dc.DataType.Equals(typeof(DateTime)))
                ret += "'" + ((DateTime) value).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            else if (dc.DataType.IsEnum)
                ret += "'" + ((Enum) value) + "'";
            else
                ret += value.ToString();
            return ret;
        }

        public string ActiveSchema(string schemaName)
        {
            return "SET search_path TO " + schemaName + ";";
        }
        // pg_get_serial_sequence(table_name, column_name)
        //SELECT last_value FROM your_sequence_name;


        public string CurrentId(string tableName, string columnName)
        {
            return "SELECT last_value FROM pg_get_serial_sequence('" + tableName + "','" + columnName + "');";
        }

        public string ChangeCurrentId(string tableName, string columnName, ulong id)
        {
            return "ALTER SEQUENCE pg_get_serial_sequence('" + tableName + "','" + columnName + "') RESTART WITH " +
                   id + ";";
        }

        public IEnumerable<DbIndex> GetTableIndexes(IDbConnection cnx, string schemaName, string tableName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbConstrait> GetTableConstraits(IDbConnection cnx, string schemaName, string tableName)
        {
            throw new NotImplementedException();
        }

        public SQLUploaderFile GetContentFromFile(IDbConnection cnx, SourceType fileSource, string path)
        {
            return new PostgreSQLUploaderFile(cnx, fileSource, path);
        }

        public string Top(string select, int rowCount)
        {
            return select.Trim() + " LIMIT " + rowCount;
        }

        public string SequenceNextValue(string sequenceName)
        {
            return sequenceName + ".nextval from dual";
        }

        public string SequenceCurrentValue(string sequenceName)
        {
            return sequenceName + ".currval from dual";
        }

        public static ISQLParser Instance()
        {
            if (_Instance == null)
                _Instance = new OracleSQLParser();

            return _Instance;
        }
    }
}