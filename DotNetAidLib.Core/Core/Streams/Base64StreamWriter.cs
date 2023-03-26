using System;
using System.IO;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Streams
{
    public class Base64StreamWriter : Stream
    {
        private readonly Base64FormattingOptions formattingOptions = Base64FormattingOptions.None;
        private readonly StreamWriter streamWriter;

        public Base64StreamWriter(StreamWriter streamWriter)
            : this(streamWriter, Base64FormattingOptions.None)
        {
        }

        public Base64StreamWriter(StreamWriter streamWriter, Base64FormattingOptions formattingOptions)
        {
            Assert.NotNull(streamWriter, nameof(streamWriter));
            Assert.NotNull(formattingOptions, nameof(formattingOptions));

            this.streamWriter = streamWriter;
            this.formattingOptions = formattingOptions;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new InvalidOperationException();

        public override long Position
        {
            get => throw new InvalidOperationException();
            set => throw new InvalidOperationException();
        }

        public override void Flush()
        {
            streamWriter.Flush();
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
            streamWriter.Write(Convert.ToBase64String(buffer, offset, count, formattingOptions));
        }
    }
}