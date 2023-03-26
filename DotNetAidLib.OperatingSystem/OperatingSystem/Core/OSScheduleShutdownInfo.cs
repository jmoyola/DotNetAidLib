using System;

namespace DotNetAidLib.OperatingSystem.Core
{
    public enum OSShutdownType
    {
        Reboot,
        PowerOff,
        Cancel
    }

    public class OSScheduleShutdownInfo
    {
        public OSScheduleShutdownInfo(OSShutdownType shutdownType)
            : this(shutdownType, TimeSpan.Zero, true, null)
        {
        }

        public OSScheduleShutdownInfo(OSShutdownType shutdownType, TimeSpan schedule)
            : this(shutdownType, schedule, true, null)
        {
        }

        public OSScheduleShutdownInfo(OSShutdownType shutdownType, TimeSpan schedule, bool warnWall)
            : this(shutdownType, schedule, warnWall, null)
        {
        }

        public OSScheduleShutdownInfo(OSShutdownType shutdownType, TimeSpan schedule, bool warnWall, string wallMessage)
        {
            ShutdownType = shutdownType;
            Schedule = schedule;
            WallMessage = wallMessage;
            WarnWall = warnWall;
        }

        public OSShutdownType ShutdownType { get; }

        public TimeSpan Schedule { get; }

        public bool WarnWall { get; }

        public string WallMessage { get; }

        public override string ToString()
        {
            return Schedule + " " + ShutdownType + " " + (WarnWall ? "warn" : "don't warn") +
                   (string.IsNullOrEmpty(WallMessage) ? "" : " " + WallMessage);
        }

        public override bool Equals(object obj)
        {
            if (obj != null && typeof(OSScheduleShutdownInfo).IsAssignableFrom(obj.GetType()))
            {
                var osi = (OSScheduleShutdownInfo) obj;
                return ShutdownType.Equals(osi.ShutdownType);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ShutdownType.GetHashCode();
        }
    }
}