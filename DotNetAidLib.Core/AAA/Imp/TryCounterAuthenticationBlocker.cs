using System;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Process;
using Library.AAA.Core;

namespace Library.AAA.Imp
{
    public class TryCounterAuthenticationBlocker: BackgroundTimer, AuthenticationBlocker
    {
        private IList<KeyValuePair<DateTime,IIdentity>> authenticationTries = new List<KeyValuePair<DateTime, IIdentity>>();
        private int _ExpiredEntrySeconds;
        private int _MaxTryTimes;

        public TryCounterAuthenticationBlocker()
            : this(60, 3)
        {
        }


        public TryCounterAuthenticationBlocker(int expiredEntrySeconds)
            :this(expiredEntrySeconds, 3){
        }

        public TryCounterAuthenticationBlocker(int expiredEntrySeconds, int maxTryTimes)
            : base(null, 0, 1000)
        {
            this.Lapse += LapseEvent;
            this._ExpiredEntrySeconds = expiredEntrySeconds;
            this._MaxTryTimes = maxTryTimes;
        }

        private void LapseEvent(object sender, BackgroundTimerLapseEventArgs args) {
            DateTime now = DateTime.Now;
            for (int i = authenticationTries.Count - 1; i > -1; i--){
                if (now.Subtract(authenticationTries[i].Key).TotalSeconds > _ExpiredEntrySeconds)
                    authenticationTries.RemoveAt(i);
            }
        }

        public void UnsuccessfullAuthentication(IIdentity id) {
            authenticationTries.Add(new KeyValuePair<DateTime, IIdentity>(DateTime.Now, id));
        }

        public bool IsValid(IIdentity id) {
            return authenticationTries.Count(v => v.Value.Equals(id)) < this._MaxTryTimes;
        }

    }
}
