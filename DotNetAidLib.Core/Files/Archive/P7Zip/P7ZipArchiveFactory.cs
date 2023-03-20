using System;
using System.Collections.Generic;
using System.IO;
using DotNetAidLib.Core.IO.Archive.Core;

namespace DotNetAidLib.Core.IO.Archive.P7Zip
{
	public class P7ZipArchiveFactory:IArchiveFactory
	{
		
		private static IArchiveFactory _Instance=null;
		private P7ZipArchiveFactory(){}

		public static IArchiveFactory Instance(){
			if (_Instance == null)
				_Instance = new P7ZipArchiveFactory ();
			return _Instance;
		}

		public ArchiveFile NewArchiveInstance (FileInfo archiveFile){
			return new P7ZipCmdArchiveFile (archiveFile);
		}

		public String DefaultExtension{ get{ return "7z"; } }
	}
}

