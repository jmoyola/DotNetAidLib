using System;
using System.Collections.Generic;
using System.IO;

namespace DotNetAidLib.Core.IO.Streams
{
    public class PeekStream : Stream
    {
        private readonly Stream baseStream;
        private readonly Queue<byte> fifo;

        public PeekStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            baseStream = stream;
            fifo = new Queue<byte>();
        }

        public override bool CanRead => baseStream.CanRead;

        public override bool CanWrite => baseStream.CanWrite;

        public override bool CanSeek => baseStream.CanSeek;

        public override long Length => baseStream.Length;

        public override long Position
        {
            get => baseStream.Position;
            set
            {
                fifo.Clear();
                baseStream.Position = value;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            fifo.Clear();
            return baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            baseStream.SetLength(value);
        }

        public int PeekByte()
        {
            var buffer = new byte [1];

            var ret = Peek(buffer, 0, 1);
            if (ret == 1)
                return buffer[0];
            return ret;
        }

        public int Peek(byte[] buffer, int offset, int count)
        {
            // Si no hay suficiente en el buffer para servir, se encola
            for (var i = 0; count > fifo.Count; i++)
            {
                var b = baseStream.ReadByte();
                if (b == -1) // Si es final de stream paramos de leer
                    break;
                fifo.Enqueue((byte) b);
            }

            var available = count > fifo.Count ? fifo.Count : count;
            Array.Copy(fifo.ToArray(), 0, buffer, offset, available);

            return available;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int ret;

            // Validate arguments
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count));


            for (ret = 0; ret < count && fifo.Count > 0; ret++)
                buffer[ret + offset] = fifo.Dequeue();

            count -= ret;

            if (count > 0)
                ret += baseStream.Read(buffer, offset + ret, count);

            // Return total bytes read
            return ret;
        }

        public override int ReadByte()
        {
            var buffer = new byte [1];

            var ret = Read(buffer, 0, 1);
            if (ret == 1)
                return buffer[0];
            return ret;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            baseStream.Write(buffer, offset, count);
        }

        public override void Flush()
        {
            baseStream.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) baseStream.Dispose();
            base.Dispose(disposing);
        }
    }
}