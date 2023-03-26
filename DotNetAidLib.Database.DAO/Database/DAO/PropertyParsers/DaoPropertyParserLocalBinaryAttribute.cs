using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Streams;
using DotNetAidLib.Database.DAO.Core;

namespace DotNetAidLib.Database.DAO.PropertyParsers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DaoPropertyParserLocalBinaryAttribute : DaoPropertyParserAttribute
    {
        public DaoPropertyParserLocalBinaryAttribute()
            : this(null)
        {
        }

        public DaoPropertyParserLocalBinaryAttribute(string localBinaryFolder)
        {
            LocalBinaryFolder = localBinaryFolder;
        }

        public string LocalBinaryFolder { get; }

        public override object Deserialize(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, IDataReader row)
        {
            var ms = new MemoryStream();
            Stream fs = null;
            string binaryId = null;
            try
            {
                binaryId = GetBinaryIdFromDatareader(daoSession, row, entityAttribute, propertyAttribute);

                var binaryFile = new FileInfo(
                    GetBinaryBaseFolder(daoSession, entityAttribute, propertyAttribute).FullName
                    + Path.DirectorySeparatorChar + binaryId);

                fs = binaryFile.Open(FileMode.Open, FileAccess.Read);
                fs.ReadAll(ms);
                ms.Seek(0, SeekOrigin.Begin);

                return ms.ToArray(); //row.GetAllBytes(propertyAttribute.ColumnName);
            }
            catch (Exception ex)
            {
                throw new DaoException(
                    "Error reading local binary property '" + propertyAttribute.PropertyInfo.Name + "' for entity '" +
                    entityAttribute.EntityType.Name + "'", ex);
            }
            finally
            {
                try
                {
                    if (fs != null)
                        fs.Close();
                    if (ms != null)
                        ms.Close();
                }
                catch
                {
                }
            }
        }

        public override object SerializeBeforeInsert(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, object entity)
        {
            return DBNull.Value;
        }

        public override void SerializeAfterInsert(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, object entity)
        {
            Stream fs = null;
            string binaryId = null;

            try
            {
                binaryId = GetBinaryIdFromEntity(daoSession, entity, propertyAttribute);

                var binaryContent = (byte[]) propertyAttribute.PropertyInfo.GetValue(entity);

                var binaryFile = new FileInfo(
                    GetBinaryBaseFolder(daoSession, entityAttribute, propertyAttribute).FullName
                    + Path.DirectorySeparatorChar + binaryId);

                fs = binaryFile.Open(FileMode.OpenOrCreate);
                fs.WriteAll(binaryContent);
                fs.Flush();
            }
            catch (Exception ex)
            {
                throw new DaoException(
                    "Error creating/updating local binary property '" + propertyAttribute.PropertyInfo.Name +
                    "' for entity '" + entity.GetType().Name + "'", ex);
            }
            finally
            {
                try
                {
                    if (fs != null)
                        fs.Close();
                }
                catch
                {
                }
            }
        }

        public override object SerializeBeforeUpdate(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, object entity)
        {
            return DBNull.Value;
        }

        private DirectoryInfo GetBinaryBaseFolder(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute)
        {
            DirectoryInfo binaryBaseFolder = null;

            // Si tiene etiqueta de carpeta, es mandatoria
            if (!string.IsNullOrEmpty(LocalBinaryFolder))
            {
                binaryBaseFolder = new DirectoryInfo(LocalBinaryFolder);
            }
            else
            {
                // Probamos con el atributo del contexto
                if (daoSession.Context.Properties.ContainsKey("localBinaryFolder"))
                    binaryBaseFolder = (DirectoryInfo) daoSession.Context.Properties["localBinaryFolder"];
            }

            // Si no hemos podido obtener la carpeta, error...
            if (binaryBaseFolder == null)
                throw new Exception(
                    "Can't get binary base folder from Dao context properties (property name 'localBinaryFolder') or DaoPropertyLocalBinaryParser (constructor parameter 'localBinaryFolder')");

            // Obtenemos la ruta completa de la carpeta a esta entidad
            binaryBaseFolder = new DirectoryInfo(binaryBaseFolder.FullName
                                                 + Path.DirectorySeparatorChar + daoSession.DBConnection.Database
                                                 + Path.DirectorySeparatorChar + entityAttribute.EntityType.Name
            );

            // Si no existe la ruta, se crea
            if (!binaryBaseFolder.Exists)
                binaryBaseFolder.Create();

            binaryBaseFolder.Refresh();

            return binaryBaseFolder;
        }

        public string GetBinaryIdFromEntity(DaoSession daoSession, object entity,
            DaoPropertyAttribute propertyAttribute)
        {
            var daoInstance = DaoHelper.DaoInstance(entity.GetType());
            var ids = (object[]) daoInstance.GetType()
                .GetMethod("GetIdFromEntity", BindingFlags.Public | BindingFlags.Instance)
                .Invoke(daoInstance, new[] {entity});
            var binaryId = ids.ToStringJoin("_");
            //String binaryId = dao.GetIdFromEntity((T)entity).ToStringJoin("_");

            binaryId += "_" + propertyAttribute.ColumnName;

            return binaryId;
        }

        public string GetBinaryIdFromDatareader(DaoSession daoSession, IDataReader row,
            DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute)
        {
            string binaryId = null;

            var pkAttributes = DaoPropertyAttribute.GetPropertyAttributes(entityAttribute.EntityType)
                .Where(v => v is DaoPropertyPKAttribute).Cast<DaoPropertyPKAttribute>();
            foreach (var pkAttribute in pkAttributes.OrderBy(v => v.Order))
            {
                binaryId += "_";
                binaryId += row.IsDBNull(pkAttribute.ColumnName) ? "" : row[pkAttribute.ColumnName].ToString();
            }

            binaryId += "_" + propertyAttribute.ColumnName;

            return binaryId.Substring(1);
        }
    }
}