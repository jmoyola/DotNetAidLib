using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Database
{
    public class DbField
    {
        public DbField(Type type, int index, string name)
        {
        }

        public DbField(Type type, int index, string name, string dbType)
        {
            Type = type;
            Index = index;
            Name = name;
            DbType = dbType;
        }

        public Type Type { get; }

        public int Index { get; }

        public string Name { get; }

        public string DbType { get; }

        public override string ToString()
        {
            return Index + "/" + Name + "/" + Type.Name + "/" + DbType;
        }

        public static IList<DbField> FromDataReader(IDataReader dr)
        {
            Assert.NotNull(dr, nameof(dr));

            IList<DbField> ret = new List<DbField>();

            for (var i = 0; i < dr.FieldCount; i++)
                ret.Add(new DbField(dr.GetFieldType(i), i, dr.GetName(i), dr.GetDataTypeName(i)));

            return ret;
        }
    }

    public class DbRow : List<object>
    {
        private readonly IList<DbField> fields;

        public DbRow()
        {
        }

        public DbRow(IList<DbField> fields)
        {
            Assert.NotNull(fields, nameof(fields));

            this.fields = fields;
        }

        public object this[string name]
        {
            get => this[GetOrdinal(name)];
            set => this[GetOrdinal(name)] = value;
        }

        public IList<DbField> Fields => AsReadOnly().Cast<DbField>().ToList();

        public int GetOrdinal(string name)
        {
            if (fields == null)
                throw new Exception("Don't exists fields info.");

            var f = fields.FirstOrDefault(v => v.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if (f == null)
                throw new Exception("Don't exists field name '" + name + "'.");
            return f.Index;
        }

        public override string ToString()
        {
            return fields.Select((v, i) =>
                    (DBNull.Value.Equals(this[i]) ? "NULL" : this[i].ToString()) +
                    " (" + v + ")")
                .ToStringJoin(", ");
        }
    }
}