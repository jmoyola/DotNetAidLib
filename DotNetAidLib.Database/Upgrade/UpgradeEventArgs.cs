using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetAidLib.Database.Upgrade
{
    public delegate void UpgradeEventHandler(Object sender, UpgradeEventArgs args);
    public enum UpgradeEventType{

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

    public class UpgradeEventArgs:EventArgs 
    {
        private UpgradeEventType _EventType;
        private UpgradeGroup _UpgradeGroup;
        private UpgradeItem _UpgradeItem;
        private Exception _Error;
		private bool _Cancel=false;

        public UpgradeEventArgs(UpgradeEventType eventType, UpgradeGroup upgradeGroup, UpgradeItem upgradeItem, Exception error) {
            _EventType = eventType;
            _UpgradeGroup = upgradeGroup;
            _UpgradeItem = upgradeItem;
            _Error = error;
        }
        public UpgradeEventArgs(UpgradeEventType eventType, UpgradeGroup upgradeGroup, UpgradeItem upgradeItem)
            :this(eventType, upgradeGroup, upgradeItem, null)
        {
        }
        public UpgradeEventArgs(UpgradeEventType eventType, UpgradeGroup upgradeGroup)
            : this(eventType, upgradeGroup, null, null)
        {
        }
        public UpgradeEventArgs(UpgradeEventType eventType)
            : this(eventType, null, null, null)
        {
        }        public UpgradeEventType EventType { get { return _EventType; } }
        public UpgradeGroup UpgradeGroup { get { return _UpgradeGroup; } }
        public UpgradeItem UpgradeItem { get { return _UpgradeItem; } }
        public Exception Error { get { return _Error; } }
		public bool Cancel {
			get { return _Cancel; }
			set { _Cancel=value; }
		}
    }
}
