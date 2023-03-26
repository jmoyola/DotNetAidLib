using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DotNetAidLib.Database.Upgrade
{
    [DataContract]
    public class UpgradeGroup
    {
        private List<UpgradeItem> _UpgradeItems;

        public UpgradeGroup(string schema, UpgradeVersion version, DateTime date, string description)
        {
            SchemaName = schema;
            Version = version;
            Date = date;
            Description = description;
        }

        public UpgradeGroup(string schema, UpgradeVersion version, DateTime date, string description,
            List<UpgradeItem> upgradeItems)
            : this(schema, version, date, description)
        {
            _UpgradeItems = upgradeItems;
        }

        [DataMember(Order = 0)] public string SchemaName { get; set; }

        [DataMember(Order = 1)] public UpgradeVersion Version { get; set; }

        [DataMember(Order = 2)] public DateTime Date { get; set; }

        [DataMember(Order = 3)] public string Description { get; set; }

        [DataMember(Order = 4)] public UpgradeType Type { get; set; }

        [DataMember(Order = 5)]
        public List<UpgradeItem> UpgradeItems
        {
            get
            {
                if (_UpgradeItems == null)
                    _UpgradeItems = new List<UpgradeItem>();

                return _UpgradeItems;
            }
            set => _UpgradeItems = value;
        }
    }
}