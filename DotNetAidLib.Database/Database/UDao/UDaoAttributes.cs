using System;
namespace DotNetAidLib.Database.UDao
{
    public class UDaoEntityAttribute : Attribute
    {
        public String TableName { get; set; } = null;
    }
    public class UDaoPropertyAttribute : Attribute
    {
        public String ColumnName { get; set; } = null;
    }
    public class UDaoPKAttribute : UDaoPropertyAttribute
    {
        public int Order { get; set; } = 0;
    }
}
