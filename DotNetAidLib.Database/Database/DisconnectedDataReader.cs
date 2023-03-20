using System;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Database
{
    public class DRColumnInfo {
        private String name;
        private String dataType;

        public DRColumnInfo(String name, String dataType) {
            this.name = name;
            this.dataType = dataType;
        }

        public string Name { get => name;}
        public string DataType { get => dataType;}
    }
    public class DRResult:IEnumerable<Object[]>
    {
        private DataTable schemaTable;
        private IList<DRColumnInfo> columns = null;
        private IList<Object[]> rows = null;

        public DataTable SchemaTable { get => schemaTable; }
        public IList<DRColumnInfo> Columns { get => columns; }
        public IList<object[]> Rows { get => rows; }

        private DRResult(IDataReader dr) {
            this.LoadCurrent(dr);
        }

        public int GetOrdinal(string name) => this.columns.IndexWhere(v => v.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

        public void LoadCurrent(IDataReader dr)
        {
            Assert.NotNull( dr, nameof(dr));

            this.rows = new List<Object[]>();
            this.columns = new List<DRColumnInfo>();

            for (int i = 0; i < dr.FieldCount; i++)
                this.columns.Add(new DRColumnInfo(dr.GetName(i), dr.GetDataTypeName(i) ));

            this.schemaTable = dr.GetSchemaTable();

            while (dr.Read()){
                Object[] v = new object[this.schemaTable.Columns.Count];
                dr.GetValues(v);
                this.rows.Add(v);
            }
        }

        public static IList<DRResult> Load(IDataReader dr, bool closeAlterLoad=true)
        {
            Assert.NotNull( dr, nameof(dr));

            List<DRResult> ret = null;

            do
            {
                ret = new List<DRResult>();
                DRResult drInfo = new DRResult(dr);
                ret.Add(drInfo);
            } while (dr.NextResult());

            if (closeAlterLoad)
                dr.Close();

            return ret;
        }

        public IEnumerator<object[]> GetEnumerator(){
            return this.rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return this.rows.GetEnumerator();
        }
    }

    public class DisconnectedDataReader:IDataReader
    {
        private IList<DRResult> results = null;
        private IEnumerator<Object[]> rowsEnumerator = null;
        private IEnumerator<DRResult> resultsEnumerator = null;


        public DisconnectedDataReader(IDataReader dr, bool closeAlterLoad = true)
        {
            results = DRResult.Load(dr);
            resultsEnumerator = results.GetEnumerator();

            if (resultsEnumerator.MoveNext())
                rowsEnumerator = resultsEnumerator.Current.GetEnumerator();
            else
                rowsEnumerator = null;
        }

        public object this[int i] => this.Current[i];

        public object this[string name] => this.Current[this.resultsEnumerator.Current.GetOrdinal(name)];

        public int Depth => throw new NotImplementedException();

        public bool IsClosed => true;


        public void Close()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private bool disposed = false;
        protected void Dispose(bool disposing){
            if (this.disposed)
                return;
            if (disposing) {
                this.rowsEnumerator.Dispose();
                this.resultsEnumerator.Dispose();

                this.rowsEnumerator = null; ;
                this.resultsEnumerator=null;

                this.results = null;
                this.disposed = true;
            }
        }

        public bool GetBoolean(int i) => (bool)this.Current[i];
        public byte GetByte(int i) => (byte)this.Current[i];
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length){
            byte[] v = (byte[])this.Current[i];
            v.ToList().CopyTo((int)fieldOffset, buffer, (int)bufferoffset, length);
            return v.Length - (fieldOffset+length);
        }
        public char GetChar(int i) => (char)this.Current[i];
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length){
            char[] v = (char[])this.Current[i];
            v.ToList().CopyTo((int)fieldoffset, buffer, (int)bufferoffset, length);
            return v.Length - (fieldoffset + length);
        }

            
        public DateTime GetDateTime(int i) => (DateTime)this.Current[i];
        public decimal GetDecimal(int i) => (decimal)this.Current[i];
        public double GetDouble(int i) => (double)this.Current[i];
        public Type GetFieldType(int i) => this.Current[i].GetType();
        public float GetFloat(int i) => (float)this.Current[i];
        public Guid GetGuid(int i) => (Guid)this.Current[i];
        public short GetInt16(int i) => (short)this.Current[i];
        public int GetInt32(int i) => (int)this.Current[i];
        public long GetInt64(int i) => (long)this.Current[i];
        public string GetString(int i) => (String)this.Current[i];
        public object GetValue(int i) => this.Current[i];
        public int GetValues(object[] values)
        {
            this.Current.CopyTo(values, 0);
            return this.Current.Length;
        }
        public bool IsDBNull(int i) => this.Current[i] is DBNull;
        public IDataReader GetData(int i) => new DisconnectedDataReader((DbDataReader)this.Current[i]);

        public string GetDataTypeName(int i) => this.resultsEnumerator.Current.Columns[i].DataType;
        public string GetName(int i) => this.resultsEnumerator.Current.Columns[i].Name;
        public int GetOrdinal(string name) => this.resultsEnumerator.Current.GetOrdinal(name);
        public DataTable GetSchemaTable() => this.resultsEnumerator.Current.SchemaTable;
        public int RecordsAffected => this.resultsEnumerator.Current.Rows.Count;
        public int FieldCount => this.resultsEnumerator.Current.Columns.Count;

        private Object[] Current {
            get{
                if (this.rowsEnumerator == null)
                    throw new IndexOutOfRangeException("No more result.");

                return this.rowsEnumerator.Current;
            }
        }

        public bool NextResult(){
            bool ret = this.resultsEnumerator.MoveNext();

            if (ret)
                rowsEnumerator = resultsEnumerator.Current.GetEnumerator();
            else
                rowsEnumerator = null;

            return ret;
        }

        public bool Read()
        {
            return this.rowsEnumerator.MoveNext();
        }
    }
}
