using System;
using System.IO;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Streams
{
    public class Base64StreamWriter:Stream
    {
        private StreamWriter streamWriter;
        private Base64FormattingOptions formattingOptions = Base64FormattingOptions.None;

        public Base64StreamWriter(StreamWriter streamWriter)
            : this(streamWriter, Base64FormattingOptions.None) { }

        public Base64StreamWriter(StreamWriter streamWriter, Base64FormattingOptions formattingOptions)
        {
            Assert.NotNull(streamWriter, nameof(streamWriter));
            Assert.NotNull(formattingOptions, nameof(formattingOptions));

            this.streamWriter = streamWriter;
            this.formattingOptions = formattingOptions;
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override long Length
        {
            get
            {
                throw new InvalidOperationException();
            }
        }

        public override long Position
        {
            get
            {
                throw new InvalidOperationException();
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override void Flush()
        {
            this.streamWriter.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
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
            streamWriter.Write(System.Convert.ToBase64String(buffer, offset, count, this.formattingOptions));
        }
    }
}
