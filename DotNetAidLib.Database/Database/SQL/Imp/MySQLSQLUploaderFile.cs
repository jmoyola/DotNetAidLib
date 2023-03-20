using System;
using System.Data;
using System.IO;
using DotNetAidLib.Database.SQL.Core;
using DotNetAidLib.Database;

namespace DotNetAidLib.Database.SQL.Imp
{
    public class MySQLSQLUploaderFile: SQLUploaderFile
    {
        private FileInfo tempFile = null;

        public MySQLSQLUploaderFile(IDbConnection cnx, SourceType sourceType, String sourcePath)
            :base(cnx, sourceType, sourcePath)
        {
            if (sourceType == SourceType.Local)
                throw new NotImplementedException();
        }


        protected override void OnDispose() {
            // Si existe archivo temporal, se elimina
            if (tempFile != null)
                tempFile.Delete();
        }

        public override string ToString()
        {
            FileInfo path = null;
            bool closedConnection=this.Cnx.State == ConnectionState.Closed;

            try
            {

                path = new FileInfo(this.SourcePath);

                if (closedConnection)
                    this.Cnx.Open();

                // Verificamos si hay restricciones de carpeta local de subida
                DBNullable<String> localTmpFolder = this.Cnx.CreateCommand()
                    .ExecuteScalar<String>("SELECT @@secure_file_priv;");

                // Si hay restricciones de carpeta local de subida, copiamos allí en archivo temporal
                if (localTmpFolder.HasValue && !String.IsNullOrEmpty(localTmpFolder))
                {
                    tempFile = path.CopyTo(
                            localTmpFolder.Value + (localTmpFolder.Value.EndsWith("" + System.IO.Path.DirectorySeparatorChar, StringComparison.InvariantCulture) ? "" : "" + System.IO.Path.DirectorySeparatorChar)
                            + Guid.NewGuid().ToString()
                            );
                    return "LOAD_FILE('" + tempFile.FullName + "')";
                }
                else
                    return "LOAD_FILE('" + path + "')";
            }
            catch (Exception ex)
            {
                throw new SQLException("Error getting content from file.", ex);
            }
            finally {
                if (closedConnection)
                    this.Cnx.Close();
            }
        }
    }
}
