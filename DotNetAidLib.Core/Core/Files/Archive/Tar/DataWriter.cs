using System.IO;

namespace DotNetAidLib.Core.IO.Archive.Tar
{
    internal class DataWriter : IArchiveDataWriter
    {
        private readonly long size;
        private readonly Stream stream;
        private long remainingBytes;

        public DataWriter(Stream data, long dataSizeInBytes)
        {
            size = dataSizeInBytes;
            remainingBytes = size;
            stream = data;
        }

        public int Write(byte[] buffer, int count)
        {
            if (remainingBytes == 0)
            {
                CanWrite = false;
                return -1;
            }

            int bytesToWrite;
            if (remainingBytes - count < 0)
                bytesToWrite = (int) remainingBytes;
            else
                bytesToWrite = count;
            stream.Write(buffer, 0, bytesToWrite);
            remainingBytes -= bytesToWrite;
            return bytesToWrite;
        }

        public bool CanWrite { get; private set; } = true;
    }
}