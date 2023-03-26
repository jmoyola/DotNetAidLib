using System.Collections.Generic;
using System.Data;

namespace DotNetAidLib.Database.SQL.Core
{
    public struct DbIndex
    {
        public string IndexName;
        public bool IsPKIndex;
        public DbIndexColumn[] Columns;
        public string Comment;
    }

    public struct DbIndexColumn
    {
        public string ColumnName;
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
        public string ConstraitName;
        public string ReferenceTableSchema;
        public string ReferenceTableName;
        public DbConstraitRule UpdateRule;
        public DbConstraitRule DeleteRule;
        public DbConstraitReferenceColumn[] ReferenceColumns;
        public string Comment;
    }

    public struct DbConstraitReferenceColumn
    {
        public string ColumnName;
        public string ReferenceColumnName;
    }

    public enum SourceType
    {
        Local,
        Server
    }

    public interface ISQLParser
    {
        string ProviderFactoryName { get; }
        string CreateSchema(string schemaName, bool ifNotExists);
        string CreateSchema(DataSet dSet, bool ifNotExists);
        string DropSchema(string schemaName, bool ifExists);
        string DropSchema(DataSet dSet, bool ifExists);
        string CreateTable(DataTable dTable, bool ifNotExists);
        string DataColumnParse(DataColumn dc);
        string DataColumnValueParse(DataColumn dc, object value);
        string ActiveSchema(string schemaName);
        string LastInsertId();

        string CurrentId(string tableName, string columnName);
        string ChangeCurrentId(string tableName, string columnName, ulong id);
        IEnumerable<DbIndex> GetTableIndexes(IDbConnection cnx, string schemaName, string tableName);
        IEnumerable<DbConstrait> GetTableConstraits(IDbConnection cnx, string schemaName, string tableName);
        SQLUploaderFile GetContentFromFile(IDbConnection cnx, SourceType fileSource, string path);
        string Top(string select, int rowCount);
        string SequenceNextValue(string sequenceName);
        string SequenceCurrentValue(string sequenceName);
    }
}