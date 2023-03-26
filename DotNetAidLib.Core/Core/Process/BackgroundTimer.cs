using System;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.Process
{
    public class BackgroundTimer : BackgroundTimerBase
    {
        public BackgroundTimer(object state, TimeSpan dueTime, TimeSpan period)
            : this(state, (ulong) dueTime.TotalMilliseconds, (ulong) period.TotalMilliseconds)
        {
        }

        public BackgroundTimer(object state, int dueTime, int period)
            : this(state, (ulong) dueTime, (ulong) period)
        {
        }

        public BackgroundTimer(object state, ulong dueTime, ulong period)
            : base(state, dueTime, period)
        {
            LapseEventHandler = HandleBackgroundTimerLapseEventHandler;
        }

        public event BackgroundTimerLapseEventHandler Lapse;
        public event AsyncExceptionEventHandler LapseError;


        private void HandleBackgroundTimerLapseEventHandler(object sender, BackgroundTimerLapseEventArgs args)
        {
            OnLapse(args);
        }


        protected void OnLapse(BackgroundTimerLapseEventArgs args)
        {
            try
            {
                if (Lapse != null)
                    Lapse(this, args);
            }
            catch (Exception ex)
            {
                OnLapseError(new AsyncExceptionEventArgs(ex));
            }
        }

        protected void OnLapseError(AsyncExceptionEventArgs args)
        {
            try
            {
                if (LapseError != null)
                    LapseError(this, args);
            }
            catch
            {
            }
        }

        public static BackgroundTimer NewTimer(object state, TimeSpan dueTime, TimeSpan period)
        {
            return new BackgroundTimer(state, dueTime, period);
        }

        public static BackgroundTimer NewTimer(object state, int dueTime, int period)
        {
            return new BackgroundTimer(state, dueTime, period);
        }

        public static BackgroundTimer NewTimer(object state, ulong dueTime, ulong period)
        {
            return new BackgroundTimer(state, dueTime, period);
        }
    }
}