using System;
using System.IO;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Streams
{
    public class Base64StreamReader : Stream
    {
        private Base64FormattingOptions formattingOptions = Base64FormattingOptions.None;
        private StreamReader streamReader;

        public Base64StreamReader(StreamReader streamReader)
            : this(streamReader, Base64FormattingOptions.None)
        {
        }

        public Base64StreamReader(StreamReader streamReader, Base64FormattingOptions formattingOptions)
        {
            Assert.NotNull(streamReader, nameof(streamReader));
            Assert.NotNull(formattingOptions, nameof(formattingOptions));

            this.streamReader = streamReader;
            this.formattingOptions = formattingOptions;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new InvalidOperationException();

        public override long Position
        {
            get => throw new InvalidOperationException();
            set => throw new InvalidOperationException();
        }

        public override void Flush()
        {
            throw new InvalidOperationException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            //Todo: hay que hacerlo
            throw new NotImplementedException();
            //return System.Convert.FromBase64CharArray(.0);
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