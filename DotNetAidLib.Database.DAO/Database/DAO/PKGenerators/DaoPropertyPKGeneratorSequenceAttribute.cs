using System;
using System.Data;
using DotNetAidLib.Database.DAO.Core;
using DotNetAidLib.Database.SQL.Core;

namespace DotNetAidLib.Database.DAO.PKGenerators
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DaoPropertyPKGeneratorSequenceAttribute : DaoPropertyPKGeneratorAttribute
    {
        public DaoPropertyPKGeneratorSequenceAttribute(string sequenceName)
        {
            SequenceName = sequenceName;
        }

        public string SequenceName { get; set; }

        public override void PreGeneration(DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute,
            object entity, IDbConnection cnx)
        {
        }

        public override void PostGeneration(DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute,
            object entity, IDbConnection cnx)
        {
            var sqlParser = SQLParserFactory.Instance(cnx);

            var value = cnx.CreateCommand("SELECT " + sqlParser.SequenceNextValue(SequenceName)).ExecuteScalar();
            value = Convert.ChangeType(value, propertyAttribute.PropertyInfo.PropertyType);
            propertyAttribute.PropertyInfo.SetValue(entity, value);
        }
    }
}