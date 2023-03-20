using System;
using System.Collections.Generic;
using System.IO;
using DotNetAidLib.Core.IO.Archive.Core;

namespace DotNetAidLib.Core.IO.Archive.Zip
{
	public class ZipArchiveFactory:IArchiveFactory
	{
		private static IArchiveFactory _Instance=null;
		private ZipArchiveFactory(){}

		public static IArchiveFactory Instance(){
			if (_Instance == null)
				_Instance = new ZipArchiveFactory ();
			return _Instance;
		}

		public ArchiveFile NewArchiveInstance (FileInfo archiveFile){
			return new ZipArchiveFile (archiveFile);
		}

		public String DefaultExtension{ get{ return "zip"; } }
	}
}

