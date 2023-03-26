using System;
using System.Collections.Generic;
using System.Data;

namespace DotNetAidLib.Database.Upgrade
{
    public enum DBTableIncluding
    {
        ALL,
        INCLUDING,
        EXCLUDING
    }

    public class DBModelDataSet : DataSet
    {
        protected IDbConnection _DbConnection;
        private readonly DBTableIncluding _DbTableIncluding = DBTableIncluding.ALL;


        public DBModelDataSet(string dataSetName, DBTableIncluding dbTableIncluding, IList<string> dataBaseTables,
            IDbConnection dbConnection)
            : base(dataSetName)
        {
            _DbConnection = dbConnection;
            _DbTableIncluding = dbTableIncluding;
            DataBaseTables = dataBaseTables;
        }

        public IList<string> DataBaseTables { get; set; }

        public virtual void RefreshSchemaFromDataBase()
        {
            try
            {
                if (_DbTableIncluding.Equals(DBTableIncluding.ALL))
                {
                    this.FillSchemaAllTables(_DbConnection, true);
                }
                else
                {
                    if (_DbTableIncluding.Equals(DBTableIncluding.INCLUDING))
                        this.FillSchemaIncludingTables(_DbConnection, DataBaseTables, true);
                    else if (_DbTableIncluding.Equals(DBTableIncluding.EXCLUDING))
                        this.FillSchemaExcludingTables(_DbConnection, DataBaseTables, true);
                }
            }
            catch (Exception ex)
            {
                throw new DataException("Error refreshing schema from database: " + ex.Message, ex);
            }
        }
    }
}