namespace DotNetAidLib.Core.IO.Archive.Tar
{
    public interface IArchiveDataWriter
    {
        bool CanWrite { get; }

        /// <summary>
        ///     Write `length` bytes of data from `buffer` to corresponding archive.
        /// </summary>
        /// <param name="buffer">data storage</param>
        /// <param name="count">how many bytes to be written to the corresponding archive</param>
        int Write(byte[] buffer, int count);
    }

    public delegate void WriteDataDelegate(IArchiveDataWriter writer);
}