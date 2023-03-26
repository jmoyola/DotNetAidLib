using System;
using System.Data;
using System.IO;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Streams
{
    public class DataReaderStream : Stream
    {
        private readonly IDataReader dataReader;
        private readonly int fieldIndex;
        private readonly long length;
        private long possition;

        public DataReaderStream(IDataReader dataReader, string fieldName)
            : this(dataReader, dataReader.GetOrdinal(fieldName))
        {
        }

        public DataReaderStream(IDataReader dataReader, int fieldIndex)
        {
            Assert.NotNull(dataReader, nameof(dataReader));
            Assert.GreaterThan(fieldIndex, -1, nameof(fieldIndex));

            this.dataReader = dataReader;
            this.fieldIndex = fieldIndex;

            length = this.dataReader.GetBytes(this.fieldIndex, 0, null, 0, 0);
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => length;

        public override long Position
        {
            get => possition;
            set => throw new InvalidOperationException();
        }

        public override void Flush()
        {
            throw new InvalidOperationException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long ret = 0;

            if (possition < length)
            {
                ret = dataReader.GetBytes(fieldIndex, possition, buffer, offset, count);
                possition += ret;
            }

            return (int) ret;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException();
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
        }
    }
}