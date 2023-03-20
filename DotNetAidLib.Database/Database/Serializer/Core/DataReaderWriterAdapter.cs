using System;
using System.Data;
using System.IO;

namespace System.Data
{
    public abstract class DataReaderWriterAdapter 
    {
        public DataReaderWriterAdapter () {
        }

        public abstract void Init(Stream baseStream, IDataReader dataReader, String tableName);
        public virtual void BeforeSerialize () { }
        public virtual void AfterSerialize () { }
        public virtual void BeforeSerializeRow (int rowIndex) { }
        public virtual void AfterSerializeRow (int rowIndex) { }
        public virtual void BeforeSerializeColumn (int columnIndex) { }
        public abstract void SerializeColumn (int columnIndex);
        public virtual void AfterSerializeColumn (int columnIndex) { }
        public virtual void Flush() { }
    }
}
