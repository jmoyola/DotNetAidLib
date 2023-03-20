using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DotNetAidLib.Database.Upgrade
{
	[DataContract]
	public class UpgradeGroup
	{
		private List<UpgradeItem> _UpgradeItems;

		public UpgradeGroup (String schema, UpgradeVersion version, DateTime date, String description)
		{
			this.SchemaName = schema;
			this.Version = version;
			this.Date = date;
			this.Description = description;
		}

        public UpgradeGroup(String schema, UpgradeVersion version, DateTime date, String description, List<UpgradeItem> upgradeItems)
            : this(schema, version, date, description)
        {
			this._UpgradeItems = upgradeItems;
		}

		[DataMember(Order=0)]
		public String SchemaName{ get; set;}

        [DataMember(Order = 1)]
		public UpgradeVersion Version{ get; set;}

        [DataMember(Order = 2)]
		public DateTime Date{ get; set;}

        [DataMember(Order = 3)]
		public String Description{ get; set;}

		[DataMember(Order = 4)]
		public UpgradeType Type{ get; set;}

        [DataMember(Order = 5)]
		public List<UpgradeItem> UpgradeItems{
			get{ 
				if (_UpgradeItems == null)
					_UpgradeItems = new List<UpgradeItem> ();
				
				return _UpgradeItems;
			}
			set{
				_UpgradeItems = value;
			}
		}
	}
}

