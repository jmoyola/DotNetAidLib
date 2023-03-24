using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DotNetAidLib.Core.Data;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Database.SQL.Imp;


namespace DotNetAidLib.Database.SQL.Core
{
	public abstract class SQLParserFactory
	{
		public static ISQLParser Instance(IDbConnection cnx) {
			return Instance(cnx.GetDbProviderFactory());
		}

		public static ISQLParser Instance(DBProviderConnector dbProviderConnector) {
			return Instance(dbProviderConnector.DBProviderFactory);
		}
		public static ISQLParser Instance(DbProviderFactory dbProviderFactory)
		{
			Assert.NotNull( dbProviderFactory, nameof(dbProviderFactory));

			if (dbProviderFactory.GetType().Name.Equals("MySqlClientFactory"))
				return MySQLParser.Instance();
			else if (dbProviderFactory.GetType().Name.Equals("Npgsq"))
				return PostgreSQLParser.Instance();
            else if (dbProviderFactory.GetType().FullName.Equals("Oracle.ManagedDataAccess.Client"))
                return OracleSQLParser.Instance();
            else
                return SQLANSIParser.Instance();
		}
	}
}

