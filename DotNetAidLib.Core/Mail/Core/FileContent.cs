using System;
using System.IO;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Mail
{
    public enum ContentType
    {
        Linked,
        Embedded,
    }
    public class FileContent:IDisposable
    {
        private ContentType _contentType;
        private readonly String name;
        private readonly String mimeType;
        private readonly FileInfo file;
        private readonly byte[] content;
        private readonly Stream stream;
        
        public FileContent(String pathToFile, ContentType contentType=ContentType.Embedded)
        :this(new FileInfo(pathToFile)) { }

        public FileContent(FileInfo file, ContentType contentType=ContentType.Embedded)
        {
            Assert.Exists(file, nameof(file));

            this._contentType = contentType;
            this.name = file.Name;
            this.mimeType = MIME.MIMEType.Instance().MimeTypeFromFileName(this.name);
            this.file = file;
        }

        public FileContent(String name, Stream stream, ContentType contentType=ContentType.Embedded, String mimeType=null)
        {
            Assert.NotNullOrEmpty( name, nameof(name));
            Assert.NotNull ( stream, nameof(stream));

            this._contentType = contentType;
            this.name = name;
            this.stream = stream;
            this.mimeType=(string.IsNullOrEmpty(mimeType)?MIME.MIMEType.Instance().MimeTypeFromFileName(this.name):mimeType);
        }

        public FileContent(String name, byte[] content, ContentType contentType=ContentType.Embedded, String mimeType=null)
        {
            this._contentType = contentType;
            Assert.NotNullOrEmpty( name, nameof(name));
            Assert.NotNull ( content, nameof(content));

            this.name = name;
            this.content = content;
            this.mimeType=(string.IsNullOrEmpty(mimeType)?MIME.MIMEType.Instance().MimeTypeFromFileName(this.name):mimeType);
        }

        public string Name => this.name;
        public string MimeType => this.mimeType;
        public ContentType ContentType => this._contentType;
        
        public Stream Stream
        {
            get
            {
                if (this.file != null)
                    return this.file.OpenRead();
                else if (this.content != null)
                    return new MemoryStream(this.content);
                
                return this.stream;
            }
        }

        public override string ToString()
        {
            return this.name + " " + this.mimeType + " " + this._contentType.ToString();
        }

        private bool _disposed = false;
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed)
                return;

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
                if (this.stream != null) {
                    this.stream.Close();
                    this.stream.Dispose();
                }
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            this._disposed = true;
        }

        public override int GetHashCode()
        {
            return this.name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            
            return typeof(FileContent).IsAssignableFrom(obj.GetType()) &&
                   ((FileContent)obj).name==this.name;
        }
        
        public static implicit operator FileContent(FileInfo file)
        {
            return new FileContent(file);
        }
        
        public static implicit operator FileContent(String filePath)
        {
            return new FileContent(filePath);
        }
    }
}