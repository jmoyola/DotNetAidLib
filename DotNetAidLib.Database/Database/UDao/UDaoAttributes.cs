using System;

namespace DotNetAidLib.Database.UDao
{
    public class UDaoEntityAttribute : Attribute
    {
        public string TableName { get; set; } = null;
    }

    public class UDaoPropertyAttribute : Attribute
    {
        public string ColumnName { get; set; } = null;
    }

    public class UDaoPKAttribute : UDaoPropertyAttribute
    {
        public int Order { get; set; } = 0;
    }
}