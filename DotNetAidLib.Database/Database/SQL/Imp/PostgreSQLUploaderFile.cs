using System.Data;
using DotNetAidLib.Database.SQL.Core;

namespace DotNetAidLib.Database.SQL.Imp
{
    public class PostgreSQLUploaderFile : SQLUploaderFile
    {
        public PostgreSQLUploaderFile(IDbConnection cnx, SourceType sourceType, string sourcePath)
            : base(cnx, sourceType, sourcePath)
        {
        }


        protected override void OnDispose()
        {
        }

        public override string ToString()
        {
            return "lo_import('" + SourcePath + "')";
        }
    }
}