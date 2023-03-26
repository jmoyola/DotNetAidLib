using System;
using System.Data;
using System.IO;
using DotNetAidLib.Database.SQL.Core;

namespace DotNetAidLib.Database.SQL.Imp
{
    public class SQLANSIUploaderFile : SQLUploaderFile
    {
        private FileInfo tempFile;

        public SQLANSIUploaderFile(IDbConnection cnx, SourceType sourceType, string sourcePath)
            : base(cnx, sourceType, sourcePath)
        {
            if (sourceType == SourceType.Local)
                throw new NotImplementedException();
        }


        protected override void OnDispose()
        {
            // Si existe archivo temporal, se elimina
            if (tempFile != null)
                tempFile.Delete();
        }

        public override string ToString()
        {
            FileInfo path = null;
            var closedConnection = Cnx.State == ConnectionState.Closed;

            try
            {
                path = new FileInfo(SourcePath);

                if (closedConnection)
                    Cnx.Open();

                // Verificamos si hay restricciones de carpeta local de subida
                var localTmpFolder = Cnx.CreateCommand()
                    .ExecuteScalar<string>("SELECT @@secure_file_priv;");

                // Si hay restricciones de carpeta local de subida, copiamos allí en archivo temporal
                if (localTmpFolder.HasValue && !string.IsNullOrEmpty(localTmpFolder))
                {
                    tempFile = path.CopyTo(
                        localTmpFolder.Value + (localTmpFolder.Value.EndsWith("" + Path.DirectorySeparatorChar,
                                                 StringComparison.InvariantCulture)
                                                 ? ""
                                                 : "" + Path.DirectorySeparatorChar)
                                             + Guid.NewGuid()
                    );
                    return "LOAD_FILE('" + tempFile.FullName + "')";
                }
                else
                {
                    return "LOAD_FILE('" + path + "')";
                }
            }
            catch (Exception ex)
            {
                throw new SQLException("Error getting content from file.", ex);
            }
            finally
            {
                if (closedConnection)
                    Cnx.Close();
            }
        }
    }
}