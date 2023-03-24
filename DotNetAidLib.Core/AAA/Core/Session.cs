using System;
using System.Collections.Generic;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using Library.AAA.Core;

namespace Library.AAA
{
    public class Session: NameTypedDictionary, IIdentity
    {
        private readonly String id;
        private SessionFactory sessionFactory = null;
        private DateTime expireTime;
        public Session (SessionFactory sessionFactory, String id, DateTime expireTime)
        {
            Assert.NotNull (sessionFactory, nameof(sessionFactory));
            Assert.NotNullOrEmpty (id, nameof (id));
            Assert.NotNull (expireTime, nameof (expireTime));

            this.sessionFactory = sessionFactory;
            this.id = id;
            this.expireTime = expireTime;
        }

        public string Id { get => id;}
        public DateTime ExpireTime { get => expireTime;}

        public void Expire () {
            this.expireTime = new DateTime ();
        }

        public void Extend (TimeSpan time) {
            this.expireTime = this.expireTime.Add (time);
        }

        public override string ToString (){
            return this.id;
        }

        public override int GetHashCode (){
            return this.id.GetHashCode();
        }

        public override bool Equals (object obj){
            if (obj != null && typeof (Session).IsAssignableFrom (obj.GetType ())) {
                return this.id == ((Session)obj).Id;
            }
            return false;
        }
    }
}
