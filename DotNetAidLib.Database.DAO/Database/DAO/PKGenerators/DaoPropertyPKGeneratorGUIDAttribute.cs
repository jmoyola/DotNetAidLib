using System;
using System.Data;
using DotNetAidLib.Database.DAO.Core;

namespace DotNetAidLib.Database.DAO.PKGenerators
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DaoPropertyPKGeneratorGUIDAttribute : DaoPropertyPKGeneratorAttribute
    {
        public override void PreGeneration(DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute,
            object entity, IDbConnection cnx)
        {
            var actualguid = (Guid) propertyAttribute.PropertyInfo.GetValue(entity);
            if (Guid.Empty.Equals(actualguid))
                propertyAttribute.PropertyInfo.SetValue(entity, Guid.NewGuid());
        }

        public override void PostGeneration(DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute,
            object entity, IDbConnection cnx)
        {
        }
    }
}