using System;

namespace DotNetAidLib.Core.IO.Streams
{
    public delegate void StreamCopyProgressEventHandler(object sender, StreamCopyProgressEventArgs args);

    public class StreamCopyProgressEventArgs : EventArgs
    {
        public StreamCopyProgressEventArgs(long length, long progress, double bytesPerSecond, bool cancel)
        {
            Length = length;
            Progress = progress;
            BytesPerSecond = bytesPerSecond;
            Cancel = cancel;
        }

        public bool Cancel { get; set; }

        public long Length { get; }

        public long Progress { get; }

        public double BytesPerSecond { get; }

        public TimeSpan Remain
        {
            get
            {
                var ret = new TimeSpan(0);

                if (Length > -1)
                    ret = TimeSpan.FromSeconds((Length - Progress) / BytesPerSecond);

                return ret;
            }
        }
    }
}