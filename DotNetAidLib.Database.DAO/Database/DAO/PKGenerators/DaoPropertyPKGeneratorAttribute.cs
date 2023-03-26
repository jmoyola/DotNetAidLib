using System;
using System.Data;
using DotNetAidLib.Database.DAO.Core;

namespace DotNetAidLib.Database.DAO.PKGenerators
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class DaoPropertyPKGeneratorAttribute : Attribute
    {
        public abstract void PreGeneration(DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute,
            object entity, IDbConnection cnx);

        public abstract void PostGeneration(DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute,
            object entity, IDbConnection cnx);
    }
}