using System;
using System.Threading;
using DotNetAidLib.Core.Develop;
using System.ComponentModel;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.Process
{
    public class BackgroundTimer : BackgroundTimerBase
    {

        public event BackgroundTimerLapseEventHandler Lapse;
        public event AsyncExceptionEventHandler LapseError;

        public BackgroundTimer (Object state, TimeSpan dueTime, TimeSpan period)
            : this (state, (UInt64)dueTime.TotalMilliseconds, (UInt64)period.TotalMilliseconds) { }

        public BackgroundTimer (Object state, int dueTime, int period)
            : this (state, (UInt64)dueTime, (UInt64)period) { }

        public BackgroundTimer (Object state, UInt64 dueTime, UInt64 period)
            : base (state, dueTime, period)
        {
            this.LapseEventHandler = HandleBackgroundTimerLapseEventHandler;
        }
    

        private void HandleBackgroundTimerLapseEventHandler (object sender, BackgroundTimerLapseEventArgs args)
        {
                this.OnLapse (args);
        }


        protected void OnLapse(BackgroundTimerLapseEventArgs args)
        {
            try
            {
                if (this.Lapse != null)
                    this.Lapse(this, args);
            }
            catch(Exception ex) {
                this.OnLapseError(new AsyncExceptionEventArgs(ex));
            }
        }

        protected void OnLapseError(AsyncExceptionEventArgs args)
        {
            try
            {
                if (this.LapseError != null)
                    this.LapseError(this, args);
            }
            catch { }
        }

        public static BackgroundTimer NewTimer(Object state, TimeSpan dueTime, TimeSpan period){
            return new BackgroundTimer(state, dueTime, period);
        }

        public static BackgroundTimer NewTimer(Object state, int dueTime, int period)
        {
            return new BackgroundTimer(state, dueTime, period);
        }

        public static BackgroundTimer NewTimer(Object state, UInt64 dueTime, UInt64 period)
        {
            return new BackgroundTimer(state, dueTime, period);
        }

    }
}

