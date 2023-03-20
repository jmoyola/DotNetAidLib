using System;
using System.Data;
using System.IO;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Streams
{
    public class DataReaderStream:Stream
    {
        private IDataReader dataReader;
        private int fieldIndex;
        private long possition=0;
        private long length;

        public DataReaderStream(IDataReader dataReader, String fieldName)
            :this(dataReader, dataReader.GetOrdinal(fieldName)) {}

        public DataReaderStream (IDataReader dataReader, int fieldIndex)
        {
            Assert.NotNull (dataReader, nameof(dataReader));
            Assert.GreaterThan (fieldIndex, -1, nameof (fieldIndex));

            this.dataReader = dataReader;
            this.fieldIndex = fieldIndex;

            this.length =this.dataReader.GetBytes(this.fieldIndex, 0, null, 0, 0);
        }

        public override bool CanRead {
            get {
                return true;
            }
        }

        public override bool CanSeek {
            get {
                return false;
            }
        }

        public override bool CanWrite {
            get {
                return false;
            }
        }

        public override long Length {
            get {
                return this.length;
            }
        }

        public override long Position {
            get {
                return this.possition;
                }
            set {
                throw new InvalidOperationException ();
            }
        }

        public override void Flush ()
        {
            throw new InvalidOperationException ();
        }

        public override int Read (byte [] buffer, int offset, int count)
        {
            long ret = 0;

            if (possition < this.length)
            {
                ret = this.dataReader.GetBytes(this.fieldIndex, possition, buffer, offset, count);
                possition += ret;
            }
            return (int)ret;
        }

        public override long Seek (long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException ();
        }

        public override void SetLength (long value)
        {
            throw new InvalidOperationException ();
        }

        public override void Write (byte [] buffer, int offset, int count)
        {
            throw new InvalidOperationException ();
        }
    }
}
