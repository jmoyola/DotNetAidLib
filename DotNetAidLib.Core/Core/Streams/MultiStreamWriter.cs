using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.IO.Streams
{
    public class MultiStream : Stream
    {
        private readonly IList<Stream> streams;

        public MultiStream(IList<Stream> streams)
        {
            Assert.NotNullOrEmpty(streams, nameof(streams));
            this.streams = streams;
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotImplementedException();

        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override void Flush()
        {
            streams.ToList().ForEach(v => v.Flush());
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            streams.ToList().ForEach(v => v.Write(buffer, offset, count));
        }
    }
}