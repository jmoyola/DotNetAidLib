using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Proc.Process;

namespace DotNetAidLib.Core.Collections
{
    public class ExpireList<T>:ThreadSafeList<T>
    {
        private BackgroundTimer bt = null;
        private int expireMilliseconds=-1;
        private Func<T,DateTime> expireField = null;

        public ExpireList (Func<T, DateTime> expireField)
            : this (expireField, 10 * 1000, 1000) { }

        public ExpireList (Func<T, DateTime> expireField, int expireMs)
            : this (expireField, expireMs, 1000) { }
        public ExpireList (Func<T, DateTime> expireField, int expireMs, int timerPeriodMs) {
            Assert.NotNull( expireField, nameof(expireField));

            this.expireField = expireField;
            this.expireMilliseconds = expireMs;

            if (timerPeriodMs > 0) {
                bt = new BackgroundTimer (null, 0, timerPeriodMs);
                bt.Lapse += (sender, args) => this.Expire ();
            }
        }

        public void Start () {
            if (this.bt == null)
                throw new Exception ("timerPeriodMs is zero.");

            this.bt.Start ();
        }

        public void Stop ()
        {
            if (this.bt == null)
                throw new Exception ("timerPeriodMs is zero.");

            this.bt.Stop ();
        }

        public bool Started
        {
            get {
                if (this.bt == null)
                    return false;
                else
                    return this.bt.Started;
            }
        }

        public void Expire () {
            if (expireMilliseconds == 0)
                return;

            DateTime now = DateTime.Now;
            foreach(T i in this) {
                if (now.Subtract (this.expireField.Invoke(i)).TotalMilliseconds > this.expireMilliseconds)
                    try {
                        this.Remove (i);
                    } catch { }
            }
        }
    }
}
