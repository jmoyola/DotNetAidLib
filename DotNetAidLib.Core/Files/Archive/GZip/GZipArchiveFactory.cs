using System;
using System.Collections.Generic;
using System.IO;
using DotNetAidLib.Core.IO.Archive.Core;

namespace DotNetAidLib.Core.IO.Archive.Zip
{
	public class GZipArchiveFactory:IArchiveFactory
	{
		private static IArchiveFactory _Instance=null;
		private GZipArchiveFactory(){}

		public static IArchiveFactory Instance(){
			if (_Instance == null)
				_Instance = new GZipArchiveFactory ();
			return _Instance;
		}

		public ArchiveFile NewArchiveInstance (FileInfo archiveFile){
			return new GZipArchiveFile (archiveFile);
		}

		public String DefaultExtension{ get{ return "gz"; } }
	}
}

