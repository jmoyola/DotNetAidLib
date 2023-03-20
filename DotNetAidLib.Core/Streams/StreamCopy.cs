using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.IO.Streams
{
    public class StreamCopy
    {
        private Stream source=null;
        private Stream destination=null;
        private int blockSize = 512;
        private int progressRefreshRate = 1024 * 1024;
        private bool cancel = false;

        public event StreamCopyProgressEventHandler StreamCopyProgress;
        protected void OnStreamCopyProgressEvent(StreamCopyProgressEventArgs args) {
            if (this.StreamCopyProgress != null)
                StreamCopyProgress(this, args);
        }

        public StreamCopy(){
        }

        public StreamCopy(Stream source, Stream destination){
            this.source = source;
            this.destination = destination;
        }

        public int BlockSize{
            get{
                return this.blockSize;
            }
            set{
                Assert.GreaterThan(value, 0, nameof(value));
                this.blockSize=value;
            }

        }

        public int ProgressRefreshRate{
            get{
                return this.progressRefreshRate;
            }
            set{
                Assert.GreaterThan(value, 0, nameof(value));
                this.progressRefreshRate = value;
            }

        }

        public Stream Source{
            get { return this.source; }
            set {
                Assert.NotNull( value, nameof(value));
                if (!value.CanRead)
                    throw new IOException("Source stream is not readable.");
                this.source = value;
            }
        }

        public Stream Destination{
            get { return this.destination; }
            set {
                Assert.NotNull( value, nameof(value));
                if (!value.CanWrite)
                    throw new IOException("Source stream is not writable.");
                this.destination = value;
            }
        }

        public void Cancel() {
            this.cancel = true;
        }

        public async Task<long> CopyAsync() {
            return await new Task<long>(() => Copy());
        }

        public long Copy(){
            Assert.NotNull(this.source, "Source");
            Assert.NotNull(this.destination, "Destination");


            byte[] buffer = new byte[blockSize];
            long progress = 0;
            long length = -1;
            if (source.CanSeek)
                length = source.Length;
            
            StreamCopyProgressEventArgs cpea = null;

            this.cancel = false;
            DateTime d = DateTime.Now;

            int progressRefresh = 0;
            double bytesSeconds = 0;

            cpea = new StreamCopyProgressEventArgs(length, 0, 0, this.cancel);
            this.OnStreamCopyProgressEvent(cpea);
            if (!cpea.Cancel){
                Thread.Sleep(1);
                int n = this.source.Read(buffer, 0, blockSize);
                while (!this.cancel && n > 0)
                {
                    this.destination.Write(buffer, 0, n);
                    progress += n;
                    progressRefresh += n;
                    bytesSeconds = Math.Round((n / DateTime.Now.Subtract(d).TotalSeconds),2);
                    if (progressRefresh > progressRefreshRate){
                        progressRefresh = 0;
                        cpea = new StreamCopyProgressEventArgs(length, progress, bytesSeconds, this.cancel);
                        this.OnStreamCopyProgressEvent(cpea);
                        if (cpea.Cancel)
                            break;
                    }
                    d = DateTime.Now;
                    n = this.source.Read(buffer, 0, blockSize);
                }

                cpea = new StreamCopyProgressEventArgs(length, progress, bytesSeconds, this.cancel);
                this.OnStreamCopyProgressEvent(cpea);
            }
            return progress;
        }


    }
}
