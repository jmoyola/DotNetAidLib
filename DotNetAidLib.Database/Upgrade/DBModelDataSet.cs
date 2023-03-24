using System;
using System.Data;
using System.Data.Common;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Data;

namespace DotNetAidLib.Database.Upgrade
{
	public enum DBTableIncluding{
		ALL,
		INCLUDING,
		EXCLUDING
	}

	public class DBModelDataSet:DataSet
	{
		protected IDbConnection _DbConnection;
		private DBTableIncluding _DbTableIncluding=DBTableIncluding.ALL;
        private IList<String> _DataBaseTables = null;


        public DBModelDataSet (String dataSetName, DBTableIncluding dbTableIncluding, IList<String> dataBaseTables, IDbConnection dbConnection)
			:base(dataSetName)
		{
			this._DbConnection = dbConnection;
			this._DbTableIncluding = dbTableIncluding;
			this._DataBaseTables = dataBaseTables;
		}

        public IList<String> DataBaseTables{
            get { return _DataBaseTables; }
            set { _DataBaseTables = value; }
        }

		public virtual void RefreshSchemaFromDataBase(){
            try
            {
                if (this._DbTableIncluding.Equals(DBTableIncluding.ALL))
                    this.FillSchemaAllTables(_DbConnection, true);
                else
                {
                    if (this._DbTableIncluding.Equals(DBTableIncluding.INCLUDING))
                        this.FillSchemaIncludingTables(_DbConnection, this._DataBaseTables, true);
                    else if (this._DbTableIncluding.Equals(DBTableIncluding.EXCLUDING))
                        this.FillSchemaExcludingTables(_DbConnection, this._DataBaseTables, true);

                }
            }
            catch(Exception ex){
                throw new DataException("Error refreshing schema from database: " + ex.Message, ex);
            }
		}


	}
}

