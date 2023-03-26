using System;
using System.Data;
using DotNetAidLib.Database.DAO.Core;
using DotNetAidLib.Database.SQL.Core;

namespace DotNetAidLib.Database.DAO.PKGenerators
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DaoPropertyPKGeneratorIdentityAttribute : DaoPropertyPKGeneratorAttribute
    {
        public override void PreGeneration(DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute,
            object entity, IDbConnection cnx)
        {
        }

        public override void PostGeneration(DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute,
            object entity, IDbConnection cnx)
        {
            var sqlParser = SQLParserFactory.Instance(cnx);
            var lii = sqlParser.LastInsertId();
            object value = null;

            value = cnx.CreateCommand("SELECT " + lii).ExecuteScalar();


            value = Convert.ChangeType(value, propertyAttribute.PropertyInfo.PropertyType);
            propertyAttribute.PropertyInfo.SetValue(entity, value);
        }
    }
}