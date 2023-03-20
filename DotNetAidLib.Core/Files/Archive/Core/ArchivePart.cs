using System;

namespace DotNetAidLib.Core.IO.Archive.Core
{
    [Flags]
    public enum ArchivePartAttributes {
        None = 0,
        Archive = 1,
        System = 2,
        Hidden = 4,
        ReadOnly = 8,
        Directory = 16,
    }

    public class ArchivePart
	{
        public ArchivePart(String name, String fullName)
            :this(name, fullName, -1, -1, default(DateTimeOffset)) {}

        public ArchivePart (String name, String fullName, long compressedLength, long length, DateTimeOffset lastWriteTime)
		{
			this.Name = name;
			this.FullName = fullName;
			this.CompressedLength = compressedLength;
			this.Length = length;
			this.LastWriteTime = lastWriteTime;
		}

		public long CompressedLength{ get; set;}
		public long Length{ get; set;}
		public DateTimeOffset LastWriteTime{ get; set;}
		public String Name{ get; set;}
		public String FullName{ get; set;}
        public String Password{ get; set; }
        public String Owner { get; set; }
        public String Group { get; set; }
        public String Permissions { get; set; }
        public ArchivePartAttributes Attributes { get; set; } = ArchivePartAttributes.None;
        public override string ToString ()
		{
            return string.Format ("[ArchivePart: CompressedLength={0}, Length={1}, LastWriteTime={2}, Name={3}, FullName={4}, Owner={5}, Group={6}, Permissions={7}, Attributes={8}]", CompressedLength, Length, LastWriteTime, Name, FullName, Owner, Group, Permissions, Attributes.ToStringFlags());
		}
	}
}

