using System;
namespace DotNetAidLib.Database.SQL
{
    public class SQLLiteral
    {
        public SQLLiteral(String value)
        {
            this.Value = value;
        }
        public String Value { get; set; }
        public override string ToString()
        {
            return this.Value;
        }
        public override bool Equals(object obj)
        {
            return this.Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }
    }

}
