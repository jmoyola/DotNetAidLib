using System.IO;
using System.Security.Cryptography;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Cryptography.CheckSum
{
    public class ChecksumStream : Stream
    {
        private readonly HashAlgorithm writeAlgorithm;

        public ChecksumStream(HashAlgorithm readWriteAlgorithm, Stream baseStream)
            : this(readWriteAlgorithm, readWriteAlgorithm, baseStream)
        {
        }

        public ChecksumStream(HashAlgorithm readAlgorithm, HashAlgorithm writeAlgorithm, Stream baseStream)
        {
            Assert.NotNull(readAlgorithm, nameof(readAlgorithm));
            Assert.NotNull(writeAlgorithm, nameof(writeAlgorithm));
            Assert.NotNull(baseStream, nameof(baseStream));

            ReadAlgorithm = readAlgorithm;
            this.writeAlgorithm = writeAlgorithm;
            BaseStream = baseStream;
        }

        public Stream BaseStream { get; }

        public HashAlgorithm ReadAlgorithm { get; }

        public HashAlgorithm WriteAlgorithm => WriteAlgorithm;

        public override bool CanRead => BaseStream.CanRead;

        public override bool CanSeek => BaseStream.CanSeek;

        public override bool CanWrite => BaseStream.CanWrite;

        public override long Length => BaseStream.Length;

        public override long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            BaseStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return BaseStream.Read(buffer, offset, count);
        }

        public int ReadChecksum(byte[] buffer, int offset, int count)
        {
            var ret = BaseStream.Read(buffer, offset, count);
            ReadAlgorithm.ComputeHash(buffer, offset, count);

            return ret;
        }

        public int ReadByteChecksum()
        {
            var value = BaseStream.ReadByte();
            if (value > -1)
                ReadAlgorithm.ComputeHash(new[] {(byte) value}, 0, 1);

            return value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
        }

        public void WriteByteChecksum(byte value)
        {
            BaseStream.WriteByte(value);
            writeAlgorithm.ComputeHash(new[] {value}, 0, 1);
        }

        public void WriteChecksum(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
            writeAlgorithm.ComputeHash(buffer, offset, count);
        }
    }
}