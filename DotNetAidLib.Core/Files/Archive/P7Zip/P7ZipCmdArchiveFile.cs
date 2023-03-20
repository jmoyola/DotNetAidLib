using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Diagnostics;
using DotNetAidLib.Core.IO.Archive.Core;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;

namespace DotNetAidLib.Core.IO.Archive.P7Zip
{
	public class P7ZipCmdArchiveFile:ArchiveFile 
	{
		private FileInfo _P7ZipCmdFile;
		private static FileInfo _SystemP7ZipCmdFile;

		public P7ZipCmdArchiveFile (FileInfo archiveFile)
			:this(SystemP7ZipCmdFile, archiveFile){
		}

		public P7ZipCmdArchiveFile (FileInfo p7zipCmdFile, FileInfo archiveFile):base(archiveFile){
			if (p7zipCmdFile==null || !p7zipCmdFile.Exists)
				throw new ArchiveException("Can't find 7zip/p7zip" + (p7zipCmdFile==null?"":" in path '" + p7zipCmdFile.FullName  + "'") + ".");
			this._P7ZipCmdFile = p7zipCmdFile;
            this.compressionLevel = 5;
		}

		public static FileInfo SystemP7ZipCmdFile{
			get{
				if (_SystemP7ZipCmdFile == null) {
					// Preparamos la ruta al ejecutable del compresor p7zip
					if (DotNetAidLib.Core.Helpers.Helper.IsWindowsSO ()) {
						_SystemP7ZipCmdFile = new FileInfo (new FileInfo (Assembly.GetExecutingAssembly ().Location).Directory.FullName + Path.DirectorySeparatorChar + "7za.exe");
						if (_SystemP7ZipCmdFile == null)
							_SystemP7ZipCmdFile = new FileInfo (".").FromPathEnvironmentVariable ("7za.exe");
					} else {
						_SystemP7ZipCmdFile = new FileInfo (new FileInfo (Path.DirectorySeparatorChar + "usr" + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "7z").FullName);
						if (_SystemP7ZipCmdFile == null)
							_SystemP7ZipCmdFile = new FileInfo (".").FromPathEnvironmentVariable("7z");
					}
				}

				return _SystemP7ZipCmdFile;
			}
		}

		public override void Open(ArchiveOpenMode openMode){
			base.Open(openMode);
			this._OpenMode = openMode;
		}

		public override void Close(){
			base.Close ();
			this._OpenMode = ArchiveOpenMode.Close;;
		}

        public override void Add(FileInfo fileToAdd, DirectoryInfo relativePath){
			base.Add (fileToAdd, relativePath);
			Process p = null;
			try {
				String relativePathAux=relativePath.FullName;
				if(!relativePathAux.EndsWith("" + Path.DirectorySeparatorChar))
					relativePathAux+=Path.DirectorySeparatorChar;

				p=_P7ZipCmdFile.GetCmdProcess("a -y \"" + _File.FullName + "\"" + (String.IsNullOrEmpty(this.Password)?"":" -p" + this.Password + " -mhc")  + " -mx=" + this.CompressionLevel + " \"" + fileToAdd.FullName.Substring(relativePathAux.Length) + "\"");
				p.StartInfo.WorkingDirectory=relativePath.FullName;
				p.Start();
				p.WaitForExit();
				if(p.ExitCode!=0)
					throw new ArchiveException(p.StandardError.ReadToEnd());
			} catch (Exception ex) {
				throw new ArchiveException("Error adding file to archive", ex);
			}
			finally{
				p.Dispose ();
			}
		}

		public override void Remove(ArchivePart archivePart){
			base.Remove (archivePart);
			Process p = null;
			try {
				p=_P7ZipCmdFile.GetCmdProcess("d -y \"" + _File.FullName + "\"" + (String.IsNullOrEmpty(this.Password)?"":" -p" + this.Password + " -mhc")  + " \"" + archivePart.FullName  + "\"");
				p.Start();
				p.WaitForExit();
				if(p.ExitCode!=0)
					throw new ArchiveException(p.StandardError.ReadToEnd());
			} catch (Exception ex) {
				throw new Exception("Error removing file from archive", ex);
			}
			finally{
				p.Dispose ();
			}
		}

		public override IList<ArchivePart> Content(){
			Process p = null;
			IList<ArchivePart> ret = base.Content();
			Regex p7zOutputListRegex=new Regex(@"^(\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2})\s([^\s]+)\s([\s\d]{12})\s([\s\d]{12})\s(.+)$", RegexOptions.Multiline);
			try {
				p = _P7ZipCmdFile.GetCmdProcess("l \"" + _File.FullName + "\"" + (String.IsNullOrEmpty(this.Password) ? "" : " -p" + this.Password));// + " -mhc"));
				p.Start();
				p.WaitForExit();
				if(p.ExitCode!=0)
					throw new ArchiveException(p.StandardError.ReadToEnd());
				String contentlist=p.StandardOutput.ReadToEnd();
				
				foreach (Match m in p7zOutputListRegex.Matches(contentlist)){
					ArchivePart ap=new ArchivePart(
						Path.GetFileName(m.Groups[5].Value.Trim()),
						m.Groups[5].Value.Trim(),
						0,
						Int64.Parse(m.Groups[3].Value.Trim()),
						DateTime.ParseExact(m.Groups[1].Value.Trim(),"yyyy-MM-dd HH:mm:ss",System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat)
					);
					Int64 compressedLength=0;
					Int64.TryParse(m.Groups[4].Value.Trim(), out compressedLength);
					ap.CompressedLength=compressedLength;

                    String attrs= m.Groups[2].Value.Trim();

                    ap.Attributes = attrs.CharPositionToEnumFlag<ArchivePartAttributes>('.');
					ret.Add(ap);
				}
				return ret;
			} catch (Exception ex) {
				throw new Exception("Error getting content from archive", ex);
			}
			finally{
				p.Dispose ();
			}
		}

        public override void Get(ArchivePart archivePart, DirectoryInfo outDirectory, bool recursive=false)
        {
            Process p = null;
            base.Get(archivePart, outDirectory, recursive);
            try
            {
                if ((archivePart.Attributes & ArchivePartAttributes.Directory) == ArchivePartAttributes.Directory)
                {
                    if (recursive)
                    {
                        p = _P7ZipCmdFile.GetCmdProcess("x" + " -y -o\"" + outDirectory.FullName + "\"" + " \"" + _File.FullName + "\"" + (String.IsNullOrEmpty(this.Password) ? "" : " -p" + this.Password + " -mhc") + " \"" + archivePart.FullName + "\"");
                        p.Start();
                        p.WaitForExit();
                        if (p.ExitCode != 0)
                            throw new ArchiveException(p.StandardError.ReadToEnd());
                    }
                    else
                        new DirectoryInfo(Path.Combine(outDirectory.FullName, archivePart.FullName)).Create();
                }
                else
                    this.Get(archivePart, new FileInfo(Path.Combine(outDirectory.FullName, archivePart.FullName)));
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error getting file from archive", ex);
            }
            finally
            {
                if(p!=null)
                    p.Dispose();
            }
        }

        public override void Get(ArchivePart archivePart, FileInfo outFile){
			Process p = null;
			base.Get (archivePart, outFile);
			try {

                p = _P7ZipCmdFile.GetCmdProcess("e" + " -y -o\"" + outFile.Directory.FullName + "\"" + " \"" + _File.FullName + "\"" + (String.IsNullOrEmpty(this.Password) ? "" : " -p" + this.Password + " -mhc") + " \"" + archivePart.FullName + "\"");
                p.Start();
                p.WaitForExit();
                if (p.ExitCode != 0)
                    throw new ArchiveException(p.StandardError.ReadToEnd());

                // Renombramos el archivo
                FileInfo fo = new FileInfo(outFile.Directory.FullName + Path.DirectorySeparatorChar + archivePart.Name);
                fo.MoveTo(outFile.FullName);

			} catch (Exception ex) {
				throw new ArchiveException("Error getting file from archive", ex);
			}
			finally{
				p.Dispose ();
			}
		}

        public override void GetAll(DirectoryInfo outDirectory)
        {
            if (this.OpenMode.Equals(ArchiveOpenMode.Close))
                throw new ArchiveException("Archive is already closed.");
            else if (!this.OpenMode.Equals(ArchiveOpenMode.OpenRead))
                throw new ArchiveException("Archive must be opened in read mode.");

            Process p = null;

            try
            {
                p = _P7ZipCmdFile.GetCmdProcess("x" + " -y -o\"" + outDirectory.FullName + "\"" + " \"" + _File.FullName + "\"" + (String.IsNullOrEmpty(this.Password) ? "" : " -p" + this.Password + " -mhc"));
                p.Start();
                p.WaitForExit();
                if (p.ExitCode != 0)
                    throw new ArchiveException(p.StandardError.ReadToEnd());
            }
            catch (Exception ex)
            {
                throw new ArchiveException("Error getting all files from archive", ex);
            }
            finally
            {
                p.Dispose();
            }

        }

    }
}

