using System.Data;
using System.IO;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Database.Serializer.Core
{
    public class DataReaderWriter
    {
        public DataReaderWriter(Stream baseStream, DataReaderWriterAdapter adapter)
        {
            Assert.NotNull(baseStream, nameof(baseStream));
            Assert.NotNull(adapter, nameof(adapter));

            BaseStream = baseStream;
            Adapter = adapter;
        }

        public Stream BaseStream { get; }

        public DataReaderWriterAdapter Adapter { get; }

        public void Write(IDataReader dr)
        {
            Write(dr, "table");
        }

        public void Write(IDataReader dr, string tableName)
        {
            Adapter.Init(BaseStream, dr, tableName);

            Adapter.BeforeSerialize();

            var rowIndex = -1;
            while (dr.Read())
            {
                rowIndex++;
                Adapter.BeforeSerializeRow(rowIndex);
                for (var i = 0; i < dr.FieldCount; i++)
                {
                    Adapter.BeforeSerializeColumn(i);
                    Adapter.SerializeColumn(i);
                    Adapter.AfterSerializeColumn(i);
                }

                Adapter.AfterSerializeRow(rowIndex);
            }

            Adapter.AfterSerialize();
        }

        public void Flush()
        {
            Adapter.Flush();
        }
    }
}