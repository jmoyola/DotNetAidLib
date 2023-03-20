using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using DotNetAidLib.Core.Develop;

namespace System.Data
{
    public class DataReaderWriter
    {
        private Stream baseStream;
        private DataReaderWriterAdapter adapter;

        public DataReaderWriter(Stream baseStream, DataReaderWriterAdapter adapter) {
            Assert.NotNull( baseStream, nameof(baseStream));
            Assert.NotNull( adapter, nameof(adapter));

            this.baseStream = baseStream;
            this.adapter = adapter;
        }

        public Stream BaseStream
        {
            get { return baseStream; }
        }

        public DataReaderWriterAdapter Adapter
        {
            get
            {
                return adapter;
            }
        }

        public void Write(IDataReader dr) {
            this.Write(dr, "table");
        }

        public void Write(IDataReader dr, String tableName)
        {

            adapter.Init(this.baseStream, dr, tableName);

            adapter.BeforeSerialize();

            int rowIndex = -1;
            while (dr.Read())
            {
                rowIndex++;
                adapter.BeforeSerializeRow(rowIndex);
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    adapter.BeforeSerializeColumn(i);
                    adapter.SerializeColumn(i);
                    adapter.AfterSerializeColumn(i);
                }
                adapter.AfterSerializeRow(rowIndex);
            }
            adapter.AfterSerialize();
        }

        public void Flush() {
            this.adapter.Flush();
        }
    }
}
