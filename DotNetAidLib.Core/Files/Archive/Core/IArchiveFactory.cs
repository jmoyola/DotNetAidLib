using System;
using System.Collections.Generic;
using System.IO;

namespace DotNetAidLib.Core.IO.Archive.Core
{
	public interface IArchiveFactory
	{
		ArchiveFile NewArchiveInstance (FileInfo archiveFile);
		String DefaultExtension{ get; }
	}
}

