using System;
using System.IO;
using System.Security.Cryptography;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Cryptography.CheckSum
{
    public class ChecksumStream:Stream
    {
        private HashAlgorithm readAlgorithm;
        private HashAlgorithm writeAlgorithm;
        private Stream baseStream;

        public ChecksumStream (HashAlgorithm readWriteAlgorithm, Stream baseStream)
        :this(readWriteAlgorithm, readWriteAlgorithm, baseStream) {}

        public ChecksumStream (HashAlgorithm readAlgorithm, HashAlgorithm writeAlgorithm, Stream baseStream)
        {
            Assert.NotNull( readAlgorithm, nameof(readAlgorithm));
            Assert.NotNull( writeAlgorithm, nameof(writeAlgorithm));
            Assert.NotNull( baseStream, nameof(baseStream));

            this.readAlgorithm = readAlgorithm;
            this.writeAlgorithm = writeAlgorithm;
            this.baseStream = baseStream;
        }

        public Stream BaseStream {
            get { return baseStream; }
        }

        public HashAlgorithm ReadAlgorithm {
            get { return readAlgorithm; }
        }

        public HashAlgorithm WriteAlgorithm {
            get { return WriteAlgorithm; }
        }

        public override bool CanRead {
            get {
                return this.baseStream.CanRead;
            }
        }

        public override bool CanSeek {
            get {
                return this.baseStream.CanSeek;
            }
        }

        public override bool CanWrite {
            get {
                return this.baseStream.CanWrite;
            }
        }

        public override long Length {
            get {
                return this.baseStream.Length;
            }
        }

        public override long Position {
            get { return this.baseStream.Position; }
            set { this.baseStream.Position = value; }
        }

        public override void Flush ()
        {
            this.baseStream.Flush();
        }

        public override long Seek (long offset, SeekOrigin origin)
        {
            return this.baseStream.Seek (offset, origin);
        }

        public override void SetLength (long value)
        {
            this.baseStream.SetLength (value);
        }

        public override int Read (byte [] buffer, int offset, int count)
        {
            return this.baseStream.Read (buffer, offset, count);
        }

        public int ReadChecksum (byte [] buffer, int offset, int count)
        {
            int ret= this.baseStream.Read (buffer, offset, count);
            this.readAlgorithm.ComputeHash (buffer, offset, count);

            return ret;
        }

        public int ReadByteChecksum ()
        {
            int value=this.baseStream.ReadByte ();
            if(value>-1)
                this.readAlgorithm.ComputeHash (new byte [] { (byte)value }, 0, 1);

            return value;
        }

        public override void Write (byte [] buffer, int offset, int count)
        {
            this.baseStream.Write (buffer, offset, count);
        }

        public void WriteByteChecksum (byte value)
        {
            this.baseStream.WriteByte(value);
            this.writeAlgorithm.ComputeHash (new byte [] { value }, 0, 1);
        }

        public void WriteChecksum (byte [] buffer, int offset, int count)
        {
            this.baseStream.Write (buffer, offset, count);
            this.writeAlgorithm.ComputeHash (buffer, offset, count);
        }
    }
}
