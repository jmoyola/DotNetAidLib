using System;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Process;

namespace DotNetAidLib.Core.Collections
{
    public class ExpireList<T> : ThreadSafeList<T>
    {
        private readonly BackgroundTimer bt;
        private readonly Func<T, DateTime> expireField;
        private readonly int expireMilliseconds = -1;

        public ExpireList(Func<T, DateTime> expireField)
            : this(expireField, 10 * 1000, 1000)
        {
        }

        public ExpireList(Func<T, DateTime> expireField, int expireMs)
            : this(expireField, expireMs, 1000)
        {
        }

        public ExpireList(Func<T, DateTime> expireField, int expireMs, int timerPeriodMs)
        {
            Assert.NotNull(expireField, nameof(expireField));

            this.expireField = expireField;
            expireMilliseconds = expireMs;

            if (timerPeriodMs > 0)
            {
                bt = new BackgroundTimer(null, 0, timerPeriodMs);
                bt.Lapse += (sender, args) => Expire();
            }
        }

        public bool Started
        {
            get
            {
                if (bt == null)
                    return false;
                return bt.Started;
            }
        }

        public void Start()
        {
            if (bt == null)
                throw new Exception("timerPeriodMs is zero.");

            bt.Start();
        }

        public void Stop()
        {
            if (bt == null)
                throw new Exception("timerPeriodMs is zero.");

            bt.Stop();
        }

        public void Expire()
        {
            if (expireMilliseconds == 0)
                return;

            var now = DateTime.Now;
            foreach (var i in this)
                if (now.Subtract(expireField.Invoke(i)).TotalMilliseconds > expireMilliseconds)
                    try
                    {
                        Remove(i);
                    }
                    catch
                    {
                    }
        }
    }
}