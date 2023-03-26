using System;

namespace DotNetAidLib.Database.Upgrade
{
    public delegate void UpgradeEventHandler(object sender, UpgradeEventArgs args);

    public enum UpgradeEventType
    {
        BeginUpgrade,
        EndUpgrade,

        BeginUpgradeGroup,
        EndUpgradeGroup,

        BeginUpgradeItem,
        EndUpgradeItem,

        BeginDowngradeItem,
        EndDowngradeItem,

        DBAlreadyUpgradeWithMostRecentUpgraderWarning,

        UpgradeError,
        DowngradeError
    }

    public class UpgradeEventArgs : EventArgs
    {
        public UpgradeEventArgs(UpgradeEventType eventType, UpgradeGroup upgradeGroup, UpgradeItem upgradeItem,
            Exception error)
        {
            EventType = eventType;
            UpgradeGroup = upgradeGroup;
            UpgradeItem = upgradeItem;
            Error = error;
        }

        public UpgradeEventArgs(UpgradeEventType eventType, UpgradeGroup upgradeGroup, UpgradeItem upgradeItem)
            : this(eventType, upgradeGroup, upgradeItem, null)
        {
        }

        public UpgradeEventArgs(UpgradeEventType eventType, UpgradeGroup upgradeGroup)
            : this(eventType, upgradeGroup, null, null)
        {
        }

        public UpgradeEventArgs(UpgradeEventType eventType)
            : this(eventType, null, null, null)
        {
        }

        public UpgradeEventType EventType { get; }

        public UpgradeGroup UpgradeGroup { get; }

        public UpgradeItem UpgradeItem { get; }

        public Exception Error { get; }

        public bool Cancel { get; set; } = false;
    }
}