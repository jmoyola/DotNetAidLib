using System;
using System.ComponentModel;
using System.Threading;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.Process
{
    public delegate void BackgroundTimerLapseEventHandler(object sender, BackgroundTimerLapseEventArgs args);

    public class BackgroundTimerLapseEventArgs : EventArgs
    {
        public BackgroundTimerLapseEventArgs(object state, DateTime? lapseTime, ulong lapseCount)
        {
            State = state;
            LapseTime = lapseTime;
            LapseCount = lapseCount;
        }

        public object State { get; }

        public DateTime? LapseTime { get; }

        public ulong LapseCount { get; }
    }

    public abstract class BackgroundTimerBase : IStartable
    {
        private ulong _DueTime;
        private ulong _Period;
        private readonly object _State;

        private bool inLapse;

        public BackgroundTimerBase(object state, TimeSpan dueTime, TimeSpan period)
            : this(state, (ulong) dueTime.TotalMilliseconds, (ulong) period.TotalMilliseconds)
        {
        }

        public BackgroundTimerBase(object state, int dueTime, int period)
            : this(state, (ulong) dueTime, (ulong) period)
        {
        }

        public BackgroundTimerBase(object state, ulong dueTime, ulong period)
        {
            _State = state;
            DueTime = dueTime;
            Period = period;
        }

        public bool AllowConcurrentLapses { get; set; } = false;

        public ulong DueTime
        {
            get => _DueTime;
            set
            {
                Assert.GreaterOrEqualThan(value, 0UL, nameof(value));
                _DueTime = value;
            }
        }

        public ulong Period
        {
            get => _Period;
            set
            {
                Assert.GreaterOrEqualThan(value, 1UL, nameof(value));
                _Period = value;
            }
        }

        public DateTime? LastLapseTime { get; private set; }

        public ulong LastLapseCount { get; private set; }

        public bool LapseAfterDueTime { get; set; } = false;

        protected BackgroundTimerLapseEventHandler LapseEventHandler { get; set; } = null;

        public bool Started { get; private set; }

        public void Start()
        {
            if (Started)
                throw new Exception("BackgroundTimer is already started.");

            LastLapseTime = null;
            LastLapseCount = 0;

            var bg = new BackgroundWorker();
            bg.DoWork += StartHandled;
            bg.WorkerReportsProgress = false;
            bg.WorkerSupportsCancellation = false;
            Started = true;
            bg.RunWorkerAsync(_State);
        }

        public void Stop()
        {
            if (!Started)
                throw new Exception("BackgroundTimer is not started.");

            Started = false;
        }

        public void ResetLastLapseCount()
        {
            LastLapseCount = 0;
        }

        private void StartHandled(object sender, DoWorkEventArgs args)
        {
            var timePoint = DateTime.Now;
            while (DateTime.Now.Subtract(timePoint).TotalMilliseconds < _DueTime) Thread.Sleep(1);

            if (LapseAfterDueTime)
                OnLapseEventHandler(new BackgroundTimerLapseEventArgs(_State, LastLapseTime, LastLapseCount));

            while (Started)
            {
                timePoint = DateTime.Now;
                while (DateTime.Now.Subtract(timePoint).TotalMilliseconds < _Period) Thread.Sleep(1);
                LastLapseTime = DateTime.Now;
                LastLapseCount++;
                OnLapseEventHandler(new BackgroundTimerLapseEventArgs(_State, LastLapseTime, LastLapseCount));
            }
        }

        private void OnLapseEventHandler(BackgroundTimerLapseEventArgs args)
        {
            if (inLapse && !AllowConcurrentLapses)
                return;

            inLapse = true;
            if (LapseEventHandler != null)
                LapseEventHandler.Invoke(this, args);
            inLapse = false;
        }
    }
}