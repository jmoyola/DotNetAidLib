using System;
using System.Threading;
using System.ComponentModel;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.Proc.Process
{
    public delegate void BackgroundTimerLapseEventHandler (Object sender, BackgroundTimerLapseEventArgs args);
    public class BackgroundTimerLapseEventArgs : EventArgs
    {
        private Object _State;
        private DateTime? _LapseTime;
        private UInt64 _LapseCount;

        public BackgroundTimerLapseEventArgs (Object state, DateTime? lapseTime, UInt64 lapseCount)
        {
            _State = state;
            _LapseTime = lapseTime;
            _LapseCount = lapseCount;
        }

        public Object State { get { return _State; } }
        public DateTime? LapseTime { get { return _LapseTime; } }
        public UInt64 LapseCount { get { return _LapseCount; } }

    }
    public abstract class BackgroundTimerBase: IStartable
	{
		private bool _Started=false;
        private bool _LapseAfterDueTime=false;

        private bool allowConcurrentLapses = false;
        private bool inLapse;
        private UInt64 _DueTime;
		private UInt64 _Period;
		private DateTime? _LastLapseTime=null;
		private UInt64 _LastLapseCount=0;
        private Object _State;
        private BackgroundTimerLapseEventHandler lapseEventHandler = null;

        public BackgroundTimerBase (Object state, TimeSpan dueTime, TimeSpan period)
			:this( state , (UInt64)dueTime.TotalMilliseconds, (UInt64)period.TotalMilliseconds){}

		public BackgroundTimerBase (Object state, int dueTime, int period)
			:this( state , (UInt64)dueTime, (UInt64)period){}
		
		public BackgroundTimerBase (Object state, UInt64 dueTime, UInt64 period)
		{
            this._State = state;
			this.DueTime = dueTime;
			this.Period = period;
		}

        public bool AllowConcurrentLapses
        {
            get{
                return allowConcurrentLapses;
            }

            set{
                allowConcurrentLapses = value;
            }
        }

        public UInt64 DueTime{
			get{ return _DueTime;}
			set{
                Assert.GreaterOrEqualThan(value, 0UL, nameof(value));
                _DueTime = value;
            }
		}

		public UInt64 Period{
			get{ return _Period;}
			set{
				Assert.GreaterOrEqualThan(value, 1UL, nameof(value));
				_Period = value;
			}
		}

		public DateTime? LastLapseTime{
			get{ return _LastLapseTime;}
		}

		public UInt64 LastLapseCount{
			get{ return _LastLapseCount;}
		}

        public bool LapseAfterDueTime
        {
            get {
                return _LapseAfterDueTime;
            }
            set{
                _LapseAfterDueTime = value; ;
            }
        }

        public void ResetLastLapseCount(){
			_LastLapseCount = 0;
		}

		public bool Started{
			get{ return _Started; }
		}

        protected BackgroundTimerLapseEventHandler LapseEventHandler {
            get {
                return lapseEventHandler;
            }

            set {
                lapseEventHandler = value;
            }
        }

        public void Start(){
			if (_Started)
				throw new Exception ("BackgroundTimer is already started.");

            _LastLapseTime = null;
            _LastLapseCount = 0;

            BackgroundWorker bg = new  BackgroundWorker ();
			bg.DoWork += StartHandled;
			bg.WorkerReportsProgress = false;
			bg.WorkerSupportsCancellation = false;
			_Started = true;
			bg.RunWorkerAsync (_State);
		}

		private void StartHandled(Object sender, DoWorkEventArgs args){
			
			DateTime timePoint = DateTime.Now;
			while (DateTime.Now.Subtract (timePoint).TotalMilliseconds < _DueTime) {
				Thread.Sleep (1);
			}

			if(_LapseAfterDueTime)
                this.OnLapseEventHandler(new BackgroundTimerLapseEventArgs(_State, _LastLapseTime, _LastLapseCount));

            while (_Started) {
				timePoint = DateTime.Now;
				while (DateTime.Now.Subtract (timePoint).TotalMilliseconds < _Period) {
					Thread.Sleep (1);
				}
				_LastLapseTime = DateTime.Now;
				_LastLapseCount++;
                this.OnLapseEventHandler(new BackgroundTimerLapseEventArgs (_State, _LastLapseTime, _LastLapseCount));
            }
		}

        private void OnLapseEventHandler(BackgroundTimerLapseEventArgs args)
        {
            if (this.inLapse && !this.allowConcurrentLapses)
                return;

            this.inLapse = true;
            if (this.LapseEventHandler != null)
                this.LapseEventHandler.Invoke(this, args);
            this.inLapse = false;
        }

        public void Stop(){
			if (!_Started)
				throw new Exception ("BackgroundTimer is not started.");
			
			_Started = false;
		}
    }
}

