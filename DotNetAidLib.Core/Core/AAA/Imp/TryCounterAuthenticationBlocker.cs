using System;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.AAA.Core;
using DotNetAidLib.Core.Process;

namespace DotNetAidLib.Core.AAA.Imp
{
    public class TryCounterAuthenticationBlocker : BackgroundTimer, AuthenticationBlocker
    {
        private readonly int _ExpiredEntrySeconds;
        private readonly int _MaxTryTimes;

        private readonly IList<KeyValuePair<DateTime, IIdentity>> authenticationTries =
            new List<KeyValuePair<DateTime, IIdentity>>();

        public TryCounterAuthenticationBlocker()
            : this(60, 3)
        {
        }


        public TryCounterAuthenticationBlocker(int expiredEntrySeconds)
            : this(expiredEntrySeconds, 3)
        {
        }

        public TryCounterAuthenticationBlocker(int expiredEntrySeconds, int maxTryTimes)
            : base(null, 0, 1000)
        {
            Lapse += LapseEvent;
            _ExpiredEntrySeconds = expiredEntrySeconds;
            _MaxTryTimes = maxTryTimes;
        }

        public void UnsuccessfullAuthentication(IIdentity id)
        {
            authenticationTries.Add(new KeyValuePair<DateTime, IIdentity>(DateTime.Now, id));
        }

        public bool IsValid(IIdentity id)
        {
            return authenticationTries.Count(v => v.Value.Equals(id)) < _MaxTryTimes;
        }

        private void LapseEvent(object sender, BackgroundTimerLapseEventArgs args)
        {
            var now = DateTime.Now;
            for (var i = authenticationTries.Count - 1; i > -1; i--)
                if (now.Subtract(authenticationTries[i].Key).TotalSeconds > _ExpiredEntrySeconds)
                    authenticationTries.RemoveAt(i);
        }
    }
}