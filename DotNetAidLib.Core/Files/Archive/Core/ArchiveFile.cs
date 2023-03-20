using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.IO.Archive.Core
{
	public abstract class ArchiveFile
	{
		protected FileInfo _File;
		protected ArchiveOpenMode _OpenMode = ArchiveOpenMode.Close;
		protected String _Password;
        protected int compressionLevel=5;
		protected Encoding _TextEncoding=UnicodeEncoding.UTF8; 

		public ArchiveFile (FileInfo file)
		{
			_File = file;
		}

		public FileInfo File{
			get{ return _File;}
		}

		public ArchiveOpenMode OpenMode{
			get{ return _OpenMode; }
		}

		public virtual String Password{
			get{ return _Password;}
			set{ _Password = value; }
		}

        public virtual int CompressionLevel
        {
            get { return this.compressionLevel; }
            set
            {
                Assert.BetweenOrEqual(value, 0, 9, nameof(value));
                this.compressionLevel = value;
            }
        }

        protected System.IO.Compression.CompressionLevel CompressionLevelParse(int level)
        {
            System.IO.Compression.CompressionLevel ret = System.IO.Compression.CompressionLevel.Fastest;
            if (level == 0)
                ret = System.IO.Compression.CompressionLevel.NoCompression;
            else if (level <= 5)
                ret = System.IO.Compression.CompressionLevel.Fastest;
            else if (level > 5)
                ret = System.IO.Compression.CompressionLevel.Optimal;

            return ret;
        }

        public Encoding TextEncoding{
			get{ return _TextEncoding;}
			set{ _TextEncoding = value;}
		}

		public virtual void Open(ArchiveOpenMode openMode){
			if(!this._OpenMode.Equals(ArchiveOpenMode.Close))
				throw new ArchiveException("Archive is already open in mode " + this._OpenMode.ToString()+".");
		}

		public virtual void Close(){
			if(this._OpenMode.Equals(ArchiveOpenMode.Close))
				throw new ArchiveException("Archive is already closed.");
			
		}


		public void Add(DirectoryInfo directoryToAdd){
            Assert.Exists( directoryToAdd, nameof(directoryToAdd));

            this.AddRecursive(directoryToAdd, directoryToAdd);
		}

		private void AddRecursive(DirectoryInfo rootDirectory, DirectoryInfo directoryToAdd){
            Assert.Exists( rootDirectory, nameof(rootDirectory));
            Assert.Exists( directoryToAdd, nameof(directoryToAdd));

            foreach (DirectoryInfo d in directoryToAdd.GetDirectories())
				this.AddRecursive (rootDirectory, d);
			foreach (FileInfo f in directoryToAdd.GetFiles())
				this.Add(f, rootDirectory);
		}

		public void Add(FileInfo fileToAdd){
            Assert.Exists( fileToAdd, nameof(fileToAdd));

            this.Add (fileToAdd, fileToAdd.Directory);
		}

		public virtual void Add(FileInfo fileToAdd, DirectoryInfo relativePath){
			if(this.OpenMode.Equals(ArchiveOpenMode.Close))
				throw new ArchiveException("Archive is already closed.");
			else if(this.OpenMode.Equals(ArchiveOpenMode.OpenRead))
				throw new ArchiveException("Archive must be opened in create or update mode.");

            Assert.Exists( fileToAdd, nameof(fileToAdd));
            Assert.Exists( relativePath, nameof(relativePath));
        }

		public virtual void Remove(ArchivePart archivePart){
			if(this.OpenMode.Equals(ArchiveOpenMode.Close))
				throw new ArchiveException("Archive is already closed.");
			else if(!this.OpenMode.Equals(ArchiveOpenMode.OpenUpdate))
				throw new ArchiveException("Archive must be opened in update mode.");

            Assert.NotNull( archivePart, nameof(archivePart));
        }

		public virtual IList<ArchivePart> Content(){
			if(this.OpenMode.Equals(ArchiveOpenMode.Close))
				throw new ArchiveException("Archive is already closed.");
			else if(!this.OpenMode.Equals(ArchiveOpenMode.OpenRead))
				throw new ArchiveException("Archive must be opened in read mode.");
			return new List<ArchivePart> ();
		}

		public virtual void Get(ArchivePart archivePart, FileInfo outFile){
			if(this.OpenMode.Equals(ArchiveOpenMode.Close))
				throw new ArchiveException("Archive is already closed.");
			else if(!this.OpenMode.Equals(ArchiveOpenMode.OpenRead))
				throw new ArchiveException("Archive must be opened in read mode.");

            Assert.NotNull( archivePart, nameof(archivePart));
            Assert.NotNull( outFile, nameof(outFile));

            if ((archivePart.Attributes & ArchivePartAttributes.Directory) == ArchivePartAttributes.Directory)
                throw new ArchiveException("archivePart is Directory");
        }

        public virtual void Get(ArchivePart archivePart, DirectoryInfo outDirectory, bool recursive = false)
        {
            if (this.OpenMode.Equals(ArchiveOpenMode.Close))
                throw new ArchiveException("Archive is already closed.");
            else if (!this.OpenMode.Equals(ArchiveOpenMode.OpenRead))
                throw new ArchiveException("Archive must be opened in read mode.");

            Assert.NotNull( archivePart, nameof(archivePart));
            Assert.Exists( outDirectory, nameof(outDirectory));
        }

        public void GetAll(IEnumerable<ArchivePart> archiveParts, DirectoryInfo outDirectory){
			if(this.OpenMode.Equals(ArchiveOpenMode.Close))
				throw new ArchiveException("Archive is already closed.");
			else if(!this.OpenMode.Equals(ArchiveOpenMode.OpenRead))
				throw new ArchiveException("Archive must be opened in read mode.");

            Assert.NotNull( archiveParts, nameof(archiveParts));
            Assert.Exists( outDirectory, nameof(outDirectory));

            foreach (ArchivePart part in archiveParts)
                    this.Get(part, outDirectory);
		}

		public virtual void GetAll(DirectoryInfo outDirectory){
            Assert.Exists( outDirectory, nameof(outDirectory));
            IList<ArchivePart> content = this.Content();
            this.GetAll (content, outDirectory);
		}			
	}
}

