using System;
using System.IO;
using System.Threading.Tasks;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.IO.Streams
{
    public delegate void StreamCopyProgressEventHandler(Object sender, StreamCopyProgressEventArgs args);
    public class StreamCopyProgressEventArgs : EventArgs {
        private long length;
        private long progress;
        private double bytesPerSecond;
        private bool cancel;

        public StreamCopyProgressEventArgs(long length, long progress, double bytesPerSecond, bool cancel) {
            this.length = length;
            this.progress = progress;
            this.bytesPerSecond = bytesPerSecond;
            this.cancel = cancel;
        }

        public bool Cancel{
            get{
                return cancel;
            }
            set{
                cancel=value;
            }
        }

        public long Length{
            get{
                return length;
            }
        }

        public long Progress
        {
            get
            {
                return progress;
            }
        }

        public double BytesPerSecond{
            get{
                return bytesPerSecond;
            }
        }

        public TimeSpan Remain {
            get
            {
                TimeSpan ret = new TimeSpan(0);

                if (this.length > -1)
                    ret = TimeSpan.FromSeconds((this.length - this.progress) / this.bytesPerSecond);

                return ret;
            }
        }

    }
}
