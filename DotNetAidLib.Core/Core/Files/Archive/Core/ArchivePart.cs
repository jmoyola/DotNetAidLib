using System;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.IO.Archive.Core
{
    [Flags]
    public enum ArchivePartAttributes
    {
        None = 0,
        Archive = 1,
        System = 2,
        Hidden = 4,
        ReadOnly = 8,
        Directory = 16
    }

    public class ArchivePart
    {
        public ArchivePart(string name, string fullName)
            : this(name, fullName, -1, -1, default)
        {
        }

        public ArchivePart(string name, string fullName, long compressedLength, long length,
            DateTimeOffset lastWriteTime)
        {
            Name = name;
            FullName = fullName;
            CompressedLength = compressedLength;
            Length = length;
            LastWriteTime = lastWriteTime;
        }

        public long CompressedLength { get; set; }
        public long Length { get; set; }
        public DateTimeOffset LastWriteTime { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string Owner { get; set; }
        public string Group { get; set; }
        public string Permissions { get; set; }
        public ArchivePartAttributes Attributes { get; set; } = ArchivePartAttributes.None;

        public override string ToString()
        {
            return string.Format(
                "[ArchivePart: CompressedLength={0}, Length={1}, LastWriteTime={2}, Name={3}, FullName={4}, Owner={5}, Group={6}, Permissions={7}, Attributes={8}]",
                CompressedLength, Length, LastWriteTime, Name, FullName, Owner, Group, Permissions,
                Attributes.ToStringFlags());
        }
    }
}