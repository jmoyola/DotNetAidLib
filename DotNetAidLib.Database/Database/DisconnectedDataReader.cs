using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Database
{
    public class DRColumnInfo
    {
        public DRColumnInfo(string name, string dataType)
        {
            Name = name;
            DataType = dataType;
        }

        public string Name { get; }

        public string DataType { get; }
    }

    public class DRResult : IEnumerable<object[]>
    {
        private DRResult(IDataReader dr)
        {
            LoadCurrent(dr);
        }

        public DataTable SchemaTable { get; private set; }

        public IList<DRColumnInfo> Columns { get; private set; }

        public IList<object[]> Rows { get; private set; }

        public IEnumerator<object[]> GetEnumerator()
        {
            return Rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Rows.GetEnumerator();
        }

        public int GetOrdinal(string name)
        {
            return Columns.IndexWhere(v => v.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public void LoadCurrent(IDataReader dr)
        {
            Assert.NotNull(dr, nameof(dr));

            Rows = new List<object[]>();
            Columns = new List<DRColumnInfo>();

            for (var i = 0; i < dr.FieldCount; i++)
                Columns.Add(new DRColumnInfo(dr.GetName(i), dr.GetDataTypeName(i)));

            SchemaTable = dr.GetSchemaTable();

            while (dr.Read())
            {
                var v = new object[SchemaTable.Columns.Count];
                dr.GetValues(v);
                Rows.Add(v);
            }
        }

        public static IList<DRResult> Load(IDataReader dr, bool closeAlterLoad = true)
        {
            Assert.NotNull(dr, nameof(dr));

            List<DRResult> ret = null;

            do
            {
                ret = new List<DRResult>();
                var drInfo = new DRResult(dr);
                ret.Add(drInfo);
            } while (dr.NextResult());

            if (closeAlterLoad)
                dr.Close();

            return ret;
        }
    }

    public class DisconnectedDataReader : IDataReader
    {
        private bool disposed;
        private IList<DRResult> results;
        private IEnumerator<DRResult> resultsEnumerator;
        private IEnumerator<object[]> rowsEnumerator;


        public DisconnectedDataReader(IDataReader dr, bool closeAlterLoad = true)
        {
            results = DRResult.Load(dr);
            resultsEnumerator = results.GetEnumerator();

            if (resultsEnumerator.MoveNext())
                rowsEnumerator = resultsEnumerator.Current.GetEnumerator();
            else
                rowsEnumerator = null;
        }

        private object[] Current
        {
            get
            {
                if (rowsEnumerator == null)
                    throw new IndexOutOfRangeException("No more result.");

                return rowsEnumerator.Current;
            }
        }

        public object this[int i] => Current[i];

        public object this[string name] => Current[resultsEnumerator.Current.GetOrdinal(name)];

        public int Depth => throw new NotImplementedException();

        public bool IsClosed => true;


        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public bool GetBoolean(int i)
        {
            return (bool) Current[i];
        }

        public byte GetByte(int i)
        {
            return (byte) Current[i];
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            var v = (byte[]) Current[i];
            v.ToList().CopyTo((int) fieldOffset, buffer, bufferoffset, length);
            return v.Length - (fieldOffset + length);
        }

        public char GetChar(int i)
        {
            return (char) Current[i];
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            var v = (char[]) Current[i];
            v.ToList().CopyTo((int) fieldoffset, buffer, bufferoffset, length);
            return v.Length - (fieldoffset + length);
        }


        public DateTime GetDateTime(int i)
        {
            return (DateTime) Current[i];
        }

        public decimal GetDecimal(int i)
        {
            return (decimal) Current[i];
        }

        public double GetDouble(int i)
        {
            return (double) Current[i];
        }

        public Type GetFieldType(int i)
        {
            return Current[i].GetType();
        }

        public float GetFloat(int i)
        {
            return (float) Current[i];
        }

        public Guid GetGuid(int i)
        {
            return (Guid) Current[i];
        }

        public short GetInt16(int i)
        {
            return (short) Current[i];
        }

        public int GetInt32(int i)
        {
            return (int) Current[i];
        }

        public long GetInt64(int i)
        {
            return (long) Current[i];
        }

        public string GetString(int i)
        {
            return (string) Current[i];
        }

        public object GetValue(int i)
        {
            return Current[i];
        }

        public int GetValues(object[] values)
        {
            Current.CopyTo(values, 0);
            return Current.Length;
        }

        public bool IsDBNull(int i)
        {
            return Current[i] is DBNull;
        }

        public IDataReader GetData(int i)
        {
            return new DisconnectedDataReader((DbDataReader) Current[i]);
        }

        public string GetDataTypeName(int i)
        {
            return resultsEnumerator.Current.Columns[i].DataType;
        }

        public string GetName(int i)
        {
            return resultsEnumerator.Current.Columns[i].Name;
        }

        public int GetOrdinal(string name)
        {
            return resultsEnumerator.Current.GetOrdinal(name);
        }

        public DataTable GetSchemaTable()
        {
            return resultsEnumerator.Current.SchemaTable;
        }

        public int RecordsAffected => resultsEnumerator.Current.Rows.Count;
        public int FieldCount => resultsEnumerator.Current.Columns.Count;

        public bool NextResult()
        {
            var ret = resultsEnumerator.MoveNext();

            if (ret)
                rowsEnumerator = resultsEnumerator.Current.GetEnumerator();
            else
                rowsEnumerator = null;

            return ret;
        }

        public bool Read()
        {
            return rowsEnumerator.MoveNext();
        }

        protected void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                rowsEnumerator.Dispose();
                resultsEnumerator.Dispose();

                rowsEnumerator = null;
                ;
                resultsEnumerator = null;

                results = null;
                disposed = true;
            }
        }
    }
}