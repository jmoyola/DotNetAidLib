using System;
using System.Collections.Generic;
using System.IO;

namespace DotNetAidLib.Core.IO.Streams
{
    public class PeekStream : Stream
    {
        private Stream baseStream;
        private Queue<byte> fifo;

        public PeekStream (Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException (nameof(stream));
               
            this.baseStream = stream;
            this.fifo = new Queue<byte> ();
        }

        public override bool CanRead {
            get {
                return this.baseStream.CanRead;
            }
        }

        public override bool CanWrite {
            get {
                return this.baseStream.CanWrite;
            }
        }

        public override bool CanSeek {
            get {
                return this.baseStream.CanSeek;
            }
        }

        public override long Length {
            get {
                return this.baseStream.Length;
            }
        }

        public override long Position {
            get {
                return this.baseStream.Position;
            }
            set {
                this.fifo.Clear ();
                this.baseStream.Position=value;
            }
        }

        public override long Seek (long offset, SeekOrigin origin)
        {
            this.fifo.Clear ();
            return this.baseStream.Seek (offset, origin);
        }

        public override void SetLength (long value)
        {
            this.baseStream.SetLength(value);
        }

        public int PeekByte () {
            byte [] buffer = new byte [1];

            int ret = this.Peek (buffer, 0, 1);
            if (ret == 1)
                return buffer [0];
            else
                return ret;
        }

        public int Peek (byte [] buffer, int offset, int count) {
            // Si no hay suficiente en el buffer para servir, se encola
            for (int i = 0;count > this.fifo.Count;i++) {
                int b = this.baseStream.ReadByte ();
                if (b == -1) // Si es final de stream paramos de leer
                    break;
                fifo.Enqueue ((byte)b);
            }

            int available = (count > this.fifo.Count ? this.fifo.Count : count);
            Array.Copy(fifo.ToArray(), 0, buffer, offset, available);

            return available;
        }

        public override int Read (byte [] buffer, int offset, int count)
        {
            int ret;

            // Validate arguments
            if (buffer == null)
                throw new ArgumentNullException (nameof(buffer));

            if (offset < 0)
                throw new ArgumentOutOfRangeException (nameof(offset));

            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException (nameof(count));



            for (ret = 0; (ret < count) && (this.fifo.Count>0); ret++)
                buffer [ret + offset] = this.fifo.Dequeue ();

            count -= ret;

            if (count > 0)
                ret += this.baseStream.Read (buffer, offset+ret, count);

            // Return total bytes read
            return ret;
        }

        public override int ReadByte ()
        {
            byte [] buffer = new byte [1];

            int ret = this.Read (buffer, 0, 1);
            if (ret == 1)
                return buffer [0];
            else
                return ret;
        }

        public override void Write (byte [] buffer, int offset, int count)
        {
            this.baseStream.Write(buffer, offset, count);
        }

        public override void Flush ()
        {
            this.baseStream.Flush ();
        }

        protected override void Dispose (bool disposing)
        {
            if (disposing) {
                baseStream.Dispose ();
            }
            base.Dispose (disposing);
        }
    }
}
