using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.IO.Streams
{
    public class MultiStream:Stream
    {
        private IList<Stream> streams;
        public MultiStream(IList<Stream> streams)
        {
            Assert.NotNullOrEmpty( streams, nameof(streams));
            this.streams = streams;
        }
        public override void Flush()
        {
            this.streams.ToList().ForEach(v=>v.Flush());
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.streams.ToList().ForEach(v=>v.Write(buffer, offset, count));
        }

        public override bool CanRead { get=>false; }
        public override bool CanSeek { get=>false; }
        public override bool CanWrite { get=>true; }
        public override long Length { get=>throw new System.NotImplementedException(); }
        public override long Position {
            get=>throw new System.NotImplementedException();
            set=>throw new System.NotImplementedException();
        }
    }
}