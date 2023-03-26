using System.Data;
using System.IO;

namespace DotNetAidLib.Database.Serializer.Core
{
    public abstract class DataReaderWriterAdapter
    {
        public abstract void Init(Stream baseStream, IDataReader dataReader, string tableName);

        public virtual void BeforeSerialize()
        {
        }

        public virtual void AfterSerialize()
        {
        }

        public virtual void BeforeSerializeRow(int rowIndex)
        {
        }

        public virtual void AfterSerializeRow(int rowIndex)
        {
        }

        public virtual void BeforeSerializeColumn(int columnIndex)
        {
        }

        public abstract void SerializeColumn(int columnIndex);

        public virtual void AfterSerializeColumn(int columnIndex)
        {
        }

        public virtual void Flush()
        {
        }
    }
}