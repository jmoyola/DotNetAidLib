using System;


namespace Library.OperatingSystem.Core
{
    public enum OSShutdownType
    {
        Reboot,
        PowerOff,
        Cancel,
    }

    public class OSScheduleShutdownInfo{
		private OSShutdownType shutdownType;
		private TimeSpan schedule;
        private bool warnWall;
        private String wallMessage;

        public OSScheduleShutdownInfo (OSShutdownType shutdownType)
            : this (shutdownType, TimeSpan.Zero, true, null) { }

        public OSScheduleShutdownInfo (OSShutdownType shutdownType, TimeSpan schedule)
            : this(shutdownType, schedule, true, null) { }

        public OSScheduleShutdownInfo(OSShutdownType shutdownType, TimeSpan schedule, bool warnWall)
            : this(shutdownType, schedule, warnWall, null) { }

        public OSScheduleShutdownInfo(OSShutdownType shutdownType, TimeSpan schedule, bool warnWall, String wallMessage)
        {
			this.shutdownType = shutdownType;
			this.schedule = schedule;
            this.wallMessage = wallMessage;
            this.warnWall = warnWall;

        }

		public OSShutdownType ShutdownType
        {
			get{
				return shutdownType;
			}
		}
        
		public TimeSpan Schedule{
            get {
                return schedule;
			}
		}

        public bool WarnWall
        {
            get
            {
                return warnWall;
            }
        }

        public String WallMessage
        {
            get
            {
                return wallMessage;
            }
        }

        public override string ToString()
        {
            return this.Schedule.ToString() + " " + this.ShutdownType.ToString() + " " + (this.warnWall? "warn":"don't warn") + (String.IsNullOrEmpty(this.WallMessage )? "":" " + this.WallMessage);
        }

        public override bool Equals(object obj)
        {
            if(obj!=null && typeof(OSScheduleShutdownInfo).IsAssignableFrom(obj.GetType())) {
                OSScheduleShutdownInfo osi = (OSScheduleShutdownInfo)obj;
                return this.ShutdownType.Equals(osi.ShutdownType);
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            return this.ShutdownType.GetHashCode();
        }
    }
}

