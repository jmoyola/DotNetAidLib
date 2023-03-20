using System;

namespace DotNetAidLib.Core.Proc 
{
	public class TimeOutWatchDog
	{
		private Nullable<DateTime> t1=null;
		private DateTime t2;

		private long _TimeOutMs=5000;

		public TimeOutWatchDog (){}
		public TimeOutWatchDog (long timeOutMs)
		{
			this.TimeOutMs = timeOutMs;
		}

		public long TimeOutMs{
			get{ 
				return _TimeOutMs;
			}
			set{ 
				if (_TimeOutMs > 0)
					_TimeOutMs = value;
				else
					throw new ArgumentException ("No valid value for TimeOutMs.", "TimeOutMs");
			}
		}

		public long TimeOutCounter{
			get{
				if (t1.HasValue) {
					long ret = Convert.ToInt64 (t2.Subtract (t1.Value).TotalMilliseconds);
					return ret;
				}
				else
					return -1;
			}
		}

		public void Reset(){
			t1=null;
		}

		public void Init(){
			t1 = DateTime.Now;
		}

		public void Init(long timeOutMs){
			this.TimeOutMs=timeOutMs;
			this.Init ();
		}

		public bool IsTimeOut(){
			return this.IsTimeOut (false, true);
		}

		public bool IsTimeOut(bool errorIfTimeOut){
			return this.IsTimeOut (errorIfTimeOut, true);
		}

		public bool IsTimeOut(bool errorIfTimeOut, bool resetCounter){

			bool ret = false;

			if (!t1.HasValue)
				this.Init ();
			else {
				t2 = DateTime.Now;

				if (this.TimeOutCounter > this._TimeOutMs)
					ret = true;

				if(resetCounter)
					t1 = t2;
			}

			if(ret && errorIfTimeOut)
				throw new TimeoutException ("!Time is over '" + this.TimeOutMs +"' ms: " + this.TimeOutCounter + " ms");

			return ret;

		}
			
	}
}

