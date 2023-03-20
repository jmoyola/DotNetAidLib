using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.IO.Archive.Core;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.IO.Archive.Zip
{
	public class ZipArchiveFile:ArchiveFile 
	{
		private FileStream zipFileStream=null;
		private ZipArchive zArchive=null;

		public ZipArchiveFile (FileInfo archiveFile):base(archiveFile){}

		public override void Open(ArchiveOpenMode openMode){
			base.Open(openMode);

			try{
				zipFileStream=new FileStream(this.File.FullName ,FileMode.OpenOrCreate);
				if(openMode.Equals(ArchiveOpenMode.OpenCreate))
					zArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Create,false,this.TextEncoding);
				else if(openMode.Equals(ArchiveOpenMode.OpenRead))
					zArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Read,false,this.TextEncoding);
				else if(openMode.Equals(ArchiveOpenMode.OpenUpdate))
					zArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Update,false,this.TextEncoding);

				this._OpenMode = openMode;
			}
			catch (Exception ex) {
				throw new ArchiveException("Error opening archive in mode " + this._OpenMode.ToString()+": " + ex.Message,ex);
			}

		}

		public override void Close(){
			base.Close ();
			try{
				zArchive.Dispose();
				zipFileStream.Close();
				this._OpenMode = ArchiveOpenMode.Close;;
			}
			catch (Exception ex) {
				throw new ArchiveException("Error closing archive:\r\n" + ex.Message , ex);
			}
		}

        public override int CompressionLevel
        {
            get { return this.compressionLevel; }
            set
            {
                Assert.Including(value, new int[] { 0, 5, 9 }, nameof(value));
                this.compressionLevel = value;
            }
        }

        public override void Add(FileInfo fileToAdd, DirectoryInfo relativePath){
			base.Add (fileToAdd, relativePath);

			try {
				String entryName=fileToAdd.FullName.Substring(relativePath.FullName.Length);
				ZipArchiveEntry part=zArchive.CreateEntryFromFile(fileToAdd.FullName, entryName, CompressionLevelParse(this.CompressionLevel));
			} catch (Exception ex) {
				throw new ArchiveException("Error adding file to archive", ex);
			}
		}

		public override void Remove(ArchivePart archivePart){
			base.Remove (archivePart);
			try {
				ZipArchiveEntry part=zArchive.Entries.FirstOrDefault(v => v.FullName==archivePart.FullName);
				if(part!=null)
					part.Delete();
			} catch (Exception ex) {
				throw new Exception("Error removing file from archive", ex);
			}
		}

		public override IList<ArchivePart> Content(){
			IList<ArchivePart> ret = base.Content();

			try {
                foreach (ZipArchiveEntry part in zArchive.Entries)
                    ret.Add(new ArchivePart(part.Name, part.FullName, part.CompressedLength, part.Length, part.LastWriteTime));
				
				return ret;
			} catch (Exception ex) {
				throw new Exception("Error getting content from archive", ex);
			}
		}

        private String normalizeRelativePath(String path) {
            // replace slashs with os separator
            String ret= Regex.Replace(path, "[\\/]", Path.DirectorySeparatorChar + "");
            // add first slash if not exists
            ret = (ret[0] == Path.DirectorySeparatorChar ? "" : "" + Path.DirectorySeparatorChar) + ret;

            return ret;
        }

        public override void Get(ArchivePart archivePart, DirectoryInfo outDirectory, bool recursive = false)
        {
			base.Get (archivePart, outDirectory, recursive);
			try {
                ZipArchiveEntry part=zArchive.Entries.FirstOrDefault(v => v.FullName==archivePart.FullName);
				if(part==null)
					throw new ArchiveException("This part isn't in archive file.");
                                        
                FileInfo destFile = new FileInfo(outDirectory.FullName + normalizeRelativePath(archivePart.FullName));

                if (!destFile.Directory.Exists)
                    destFile.Directory.Create();

                part.ExtractToFile(destFile.FullName, true);
			} catch (Exception ex) {
				throw new ArchiveException("Error getting file from archive", ex);
			}
		}

        public override void Get(ArchivePart archivePart, FileInfo outFile)
        {
            base.Get(archivePart, outFile);
            try
            {
                ZipArchiveEntry part = zArchive.Entries.FirstOrDefault(v => v.FullName == archivePart.FullName);
                if (part == null)
                    throw new ArchiveException("This part isn't in archive file.");

                part.ExtractToFile(outFile.FullName, true);
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error getting file from archive", ex);
            }
        }

    }
}

