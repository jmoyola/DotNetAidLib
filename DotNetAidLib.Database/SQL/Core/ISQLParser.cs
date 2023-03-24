using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DotNetAidLib.Database.SQL.Imp;


namespace DotNetAidLib.Database.SQL.Core
{
    public struct DbIndex
    {
        public String IndexName;
        public bool IsPKIndex;
        public DbIndexColumn[] Columns;
        public String Comment;
    }

    public struct DbIndexColumn
    {
        public String ColumnName;
        public bool Unique;
        public bool Nullable;
    }

    public enum DbConstraitRule
    {
        NO_ACTION,
        RESTRICT,
        CASCADE,
        SET_NULL
    }

    public struct DbConstrait
    {
        public String ConstraitName;
        public String ReferenceTableSchema;
        public String ReferenceTableName;
        public DbConstraitRule UpdateRule;
        public DbConstraitRule DeleteRule;
        public DbConstraitReferenceColumn[] ReferenceColumns;
        public String Comment;
    }

    public struct DbConstraitReferenceColumn
    {
        public String ColumnName;
        public String ReferenceColumnName;
    }

    public enum SourceType { Local, Server }

    public interface ISQLParser
	{
		String ProviderFactoryName { get; }
        String CreateSchema(String schemaName, bool ifNotExists);
		String CreateSchema(DataSet dSet, bool ifNotExists);
		String DropSchema(String schemaName, bool ifExists);
		String DropSchema(DataSet dSet, bool ifExists);
		String CreateTable(DataTable dTable, bool ifNotExists);
        String DataColumnParse(DataColumn dc);
		String DataColumnValueParse(DataColumn dc, Object value);
        String ActiveSchema(String schemaName);
        String LastInsertId();

        String CurrentId(String tableName, String columnName);
		String ChangeCurrentId(String tableName, String columnName, UInt64 id);
        IEnumerable<DbIndex> GetTableIndexes(IDbConnection cnx, String schemaName, String tableName);
        IEnumerable<DbConstrait> GetTableConstraits(IDbConnection cnx, String schemaName, String tableName);
        SQLUploaderFile GetContentFromFile(IDbConnection cnx, SourceType fileSource, String path);
        String Top(String select, int rowCount);
        String SequenceNextValue(String sequenceName);
        String SequenceCurrentValue(String sequenceName);
    }
}

