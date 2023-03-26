using System;

namespace DotNetAidLib.Core.Proc
{
    public class TimeOutWatchDog
    {
        private long _TimeOutMs = 5000;
        private DateTime? t1;
        private DateTime t2;

        public TimeOutWatchDog()
        {
        }

        public TimeOutWatchDog(long timeOutMs)
        {
            TimeOutMs = timeOutMs;
        }

        public long TimeOutMs
        {
            get => _TimeOutMs;
            set
            {
                if (_TimeOutMs > 0)
                    _TimeOutMs = value;
                else
                    throw new ArgumentException("No valid value for TimeOutMs.", "TimeOutMs");
            }
        }

        public long TimeOutCounter
        {
            get
            {
                if (t1.HasValue)
                {
                    var ret = Convert.ToInt64(t2.Subtract(t1.Value).TotalMilliseconds);
                    return ret;
                }

                return -1;
            }
        }

        public void Reset()
        {
            t1 = null;
        }

        public void Init()
        {
            t1 = DateTime.Now;
        }

        public void Init(long timeOutMs)
        {
            TimeOutMs = timeOutMs;
            Init();
        }

        public bool IsTimeOut()
        {
            return IsTimeOut(false, true);
        }

        public bool IsTimeOut(bool errorIfTimeOut)
        {
            return IsTimeOut(errorIfTimeOut, true);
        }

        public bool IsTimeOut(bool errorIfTimeOut, bool resetCounter)
        {
            var ret = false;

            if (!t1.HasValue)
            {
                Init();
            }
            else
            {
                t2 = DateTime.Now;

                if (TimeOutCounter > _TimeOutMs)
                    ret = true;

                if (resetCounter)
                    t1 = t2;
            }

            if (ret && errorIfTimeOut)
                throw new TimeoutException("!Time is over '" + TimeOutMs + "' ms: " + TimeOutCounter + " ms");

            return ret;
        }
    }
}