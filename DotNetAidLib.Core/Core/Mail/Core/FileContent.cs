using System;
using System.IO;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.MIME;

namespace DotNetAidLib.Core.Mail
{
    public enum ContentType
    {
        Linked,
        Embedded
    }

    public class FileContent : IDisposable
    {
        private readonly byte[] content;
        private readonly FileInfo file;
        private readonly Stream stream;

        private bool _disposed;

        public FileContent(string pathToFile, ContentType contentType = ContentType.Embedded)
            : this(new FileInfo(pathToFile))
        {
        }

        public FileContent(FileInfo file, ContentType contentType = ContentType.Embedded)
        {
            Assert.Exists(file, nameof(file));

            ContentType = contentType;
            Name = file.Name;
            MimeType = MIMEType.Instance().MimeTypeFromFileName(Name);
            this.file = file;
        }

        public FileContent(string name, Stream stream, ContentType contentType = ContentType.Embedded,
            string mimeType = null)
        {
            Assert.NotNullOrEmpty(name, nameof(name));
            Assert.NotNull(stream, nameof(stream));

            ContentType = contentType;
            Name = name;
            this.stream = stream;
            MimeType = string.IsNullOrEmpty(mimeType) ? MIMEType.Instance().MimeTypeFromFileName(Name) : mimeType;
        }

        public FileContent(string name, byte[] content, ContentType contentType = ContentType.Embedded,
            string mimeType = null)
        {
            ContentType = contentType;
            Assert.NotNullOrEmpty(name, nameof(name));
            Assert.NotNull(content, nameof(content));

            Name = name;
            this.content = content;
            MimeType = string.IsNullOrEmpty(mimeType) ? MIMEType.Instance().MimeTypeFromFileName(Name) : mimeType;
        }

        public string Name { get; }

        public string MimeType { get; }

        public ContentType ContentType { get; }

        public Stream Stream
        {
            get
            {
                if (file != null)
                    return file.OpenRead();
                if (content != null)
                    return new MemoryStream(content);

                return stream;
            }
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            return Name + " " + MimeType + " " + ContentType;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                // TODO: dispose managed state (managed objects).
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposed = true;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            return typeof(FileContent).IsAssignableFrom(obj.GetType()) &&
                   ((FileContent) obj).Name == Name;
        }

        public static implicit operator FileContent(FileInfo file)
        {
            return new FileContent(file);
        }

        public static implicit operator FileContent(string filePath)
        {
            return new FileContent(filePath);
        }
    }
}