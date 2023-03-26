using System;
using DotNetAidLib.Core.AAA.Core;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.AAA
{
    public class Session : NameTypedDictionary, IIdentity
    {
        private DateTime expireTime;
        private SessionFactory sessionFactory;

        public Session(SessionFactory sessionFactory, string id, DateTime expireTime)
        {
            Assert.NotNull(sessionFactory, nameof(sessionFactory));
            Assert.NotNullOrEmpty(id, nameof(id));
            Assert.NotNull(expireTime, nameof(expireTime));

            this.sessionFactory = sessionFactory;
            this.Id = id;
            this.expireTime = expireTime;
        }

        public DateTime ExpireTime => expireTime;

        public string Id { get; }

        public void Expire()
        {
            expireTime = new DateTime();
        }

        public void Extend(TimeSpan time)
        {
            expireTime = expireTime.Add(time);
        }

        public override string ToString()
        {
            return Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && typeof(Session).IsAssignableFrom(obj.GetType())) return Id == ((Session) obj).Id;
            return false;
        }
    }
}