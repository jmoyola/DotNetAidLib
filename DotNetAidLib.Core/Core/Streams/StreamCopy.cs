using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.IO.Streams
{
    public class StreamCopy
    {
        private int blockSize = 512;
        private bool cancel;
        private Stream destination;
        private int progressRefreshRate = 1024 * 1024;
        private Stream source;

        public StreamCopy()
        {
        }

        public StreamCopy(Stream source, Stream destination)
        {
            this.source = source;
            this.destination = destination;
        }

        public int BlockSize
        {
            get => blockSize;
            set
            {
                Assert.GreaterThan(value, 0, nameof(value));
                blockSize = value;
            }
        }

        public int ProgressRefreshRate
        {
            get => progressRefreshRate;
            set
            {
                Assert.GreaterThan(value, 0, nameof(value));
                progressRefreshRate = value;
            }
        }

        public Stream Source
        {
            get => source;
            set
            {
                Assert.NotNull(value, nameof(value));
                if (!value.CanRead)
                    throw new IOException("Source stream is not readable.");
                source = value;
            }
        }

        public Stream Destination
        {
            get => destination;
            set
            {
                Assert.NotNull(value, nameof(value));
                if (!value.CanWrite)
                    throw new IOException("Source stream is not writable.");
                destination = value;
            }
        }

        public event StreamCopyProgressEventHandler StreamCopyProgress;

        protected void OnStreamCopyProgressEvent(StreamCopyProgressEventArgs args)
        {
            if (StreamCopyProgress != null)
                StreamCopyProgress(this, args);
        }

        public void Cancel()
        {
            cancel = true;
        }

        public async Task<long> CopyAsync()
        {
            return await new Task<long>(() => Copy());
        }

        public long Copy()
        {
            Assert.NotNull(source, "Source");
            Assert.NotNull(destination, "Destination");


            var buffer = new byte[blockSize];
            long progress = 0;
            long length = -1;
            if (source.CanSeek)
                length = source.Length;

            StreamCopyProgressEventArgs cpea = null;

            cancel = false;
            var d = DateTime.Now;

            var progressRefresh = 0;
            double bytesSeconds = 0;

            cpea = new StreamCopyProgressEventArgs(length, 0, 0, cancel);
            OnStreamCopyProgressEvent(cpea);
            if (!cpea.Cancel)
            {
                Thread.Sleep(1);
                var n = source.Read(buffer, 0, blockSize);
                while (!cancel && n > 0)
                {
                    destination.Write(buffer, 0, n);
                    progress += n;
                    progressRefresh += n;
                    bytesSeconds = Math.Round(n / DateTime.Now.Subtract(d).TotalSeconds, 2);
                    if (progressRefresh > progressRefreshRate)
                    {
                        progressRefresh = 0;
                        cpea = new StreamCopyProgressEventArgs(length, progress, bytesSeconds, cancel);
                        OnStreamCopyProgressEvent(cpea);
                        if (cpea.Cancel)
                            break;
                    }

                    d = DateTime.Now;
                    n = source.Read(buffer, 0, blockSize);
                }

                cpea = new StreamCopyProgressEventArgs(length, progress, bytesSeconds, cancel);
                OnStreamCopyProgressEvent(cpea);
            }

            return progress;
        }
    }
}