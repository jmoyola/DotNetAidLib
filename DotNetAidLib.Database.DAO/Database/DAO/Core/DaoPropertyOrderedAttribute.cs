using System;

namespace DotNetAidLib.Database.DAO.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class DaoPropertyOrderedAttribute : DaoPropertyAttribute
    {
        public DaoPropertyOrderedAttribute()
            : base(null)
        {
        }

        public DaoPropertyOrderedAttribute(string columnName)
            : base(columnName)
        {
        }

        public uint Order { get; set; }
    }
}