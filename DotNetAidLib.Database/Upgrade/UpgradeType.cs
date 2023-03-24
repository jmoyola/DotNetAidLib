using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace DotNetAidLib.Database.Upgrade
{
	[DataContract]
	public enum UpgradeType
	{
		[EnumMember]
		Debug,

		[EnumMember]
		Release
	}
}

