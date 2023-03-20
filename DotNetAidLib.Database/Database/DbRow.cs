using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Data;
using System.Linq;
using System.Data.Common;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Database
{
    public class DbField {
        private Type type;
        private int index;
        private String name;
        private String dbType;

        public DbField(Type type, int index, String name) {
        }
        public DbField(Type type, int index, String name, String dbType){
            this.type = type;
            this.index = index;
            this.name = name;
            this.dbType = dbType;
        }

        public Type Type { get => this.type;}
        public int Index { get => this.index;}
        public string Name { get => this.name;}
        public string DbType { get => this.dbType; }

        public override string ToString()
        {
            return this.index + "/" + this.name + "/" + this.type.Name + "/" + this.dbType;
        }

        public static IList<DbField> FromDataReader(IDataReader dr)
        {
            Assert.NotNull( dr, nameof(dr));

            IList<DbField> ret = new List<DbField>();

            for (int i = 0; i < dr.FieldCount; i++)
                ret.Add(new DbField(dr.GetFieldType(i), i, dr.GetName(i), dr.GetDataTypeName(i)));

            return ret;
        }
    }

    public class DbRow : List<Object>
    {
        IList<DbField> fields = null;

        public DbRow()
        { }

        public DbRow(IList<DbField> fields){
            Assert.NotNull( fields, nameof(fields));

            this.fields = fields;
        }

        public int GetOrdinal(String name) {
            if (this.fields == null)
                throw new Exception("Don't exists fields info.");

            DbField f= this.fields.FirstOrDefault(v => v.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if (f == null)
                throw new Exception("Don't exists field name '" + name + "'.");
            return f.Index;
        }

        public Object this[String name]{
            get{
                return this[this.GetOrdinal(name)];
            }
            set{
                this[this.GetOrdinal(name)] = value;
            }
        }

        public IList<DbField> Fields {
            get { return this.AsReadOnly().Cast<DbField>().ToList(); }
        }

        public override string ToString()
        {
            return this.fields.Select((v, i) =>
                    (DBNull.Value.Equals(this[i]) ? "NULL" : this[i].ToString()) +
                    " (" + v.ToString() + ")")
                .ToStringJoin(", ");
        }
    }
}
