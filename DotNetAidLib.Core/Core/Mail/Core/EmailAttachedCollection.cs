using System.Collections.Generic;
using System.IO;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Mail.Core
{
    public class EmailAttachedCollection : Dictionary<string, Stream>
    {
        public void AddMemory(FileInfo file)
        {
            AddMemory(file.Name, file);
        }

        public void AddMemory(string key, FileInfo file)
        {
            Assert.Exists(file, nameof(file));

            using (Stream source = file.OpenRead())
            {
                var ms = new MemoryStream();
                source.CopyTo(ms, 1024);
                ms.Seek(0, SeekOrigin.Begin);
                Add(key, ms);
            }
        }

        public Stream Add(FileInfo file)
        {
            return Add(file.Name, file);
        }

        public Stream Add(string key, FileInfo file)
        {
            Assert.Exists(file, nameof(file));

            Stream ms = new FileStream(file.FullName, FileMode.Open);
            Add(key, ms);

            return ms;
        }

        public FileInfo GetFile(string filename, string toFolder)
        {
            var ms = this[filename];

            if (ms.GetType() != typeof(MemoryStream))
                throw new InvalidDataException("This content is dinamic content.");

            var ret = new FileInfo(toFolder
                                   + (Path.DirectorySeparatorChar + filename));

            using (var fs = ret.Create())
            {
                ms.Position = 0;
                CopyStream(ms, fs);
            }

            return ret;
        }

        private static void CopyStream(Stream input, Stream output)
        {
            var buffer = new byte[1024];
            int len;
            len = input.Read(buffer, 0, buffer.Length);
            while (len > 0)
            {
                output.Write(buffer, 0, len);
                len = input.Read(buffer, 0, buffer.Length);
            }
        }
    }
}