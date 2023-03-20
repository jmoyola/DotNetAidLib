using System;
using System.Data;
using DotNetAidLib.Database.SQL.Core;
using DotNetAidLib.Develop;

namespace DotNetAidLib.Database.SQL.Imp
{
    public class PostgreSQLUploaderFile: SQLUploaderFile
    {

        public PostgreSQLUploaderFile(IDbConnection cnx, SourceType sourceType, String sourcePath)
            :base(cnx, sourceType, sourcePath)
        {
        }


        protected override void OnDispose() {

        }

        public override string ToString()
        {
            return "lo_import('" + this.SourcePath + "')";
        }
    }
}
