using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;

namespace Library.AAA
{
    public class SessionFactory:IDisposable{
        private const String GLOBAL_INSTANCE_KEY = "__GLOBAL_INSTANCE__";

        private static IDictionary<String, SessionFactory> instances = new Dictionary<String, SessionFactory> ();
        private TimeSpan sessionTime = new TimeSpan (0, 5, 0);

        private IDictionary<String, Session> tokens = null;
        private Func<IDictionary<String, Session>, String> idGenerator =
            (T) => Guid.NewGuid ().ToString ().Replace ("-", "").ToUpper ();
        private bool disposed = false;

        private Thread thExpire = null;
        private Object oLock = new object ();

        public event TypedCancellableEventHandler<Session> BeforeCreate;
        public event TypedEventHandler<Session> AfterCreate;

        public event TypedCancellableEventHandler<Session> BeforeExpire;
        public event TypedEventHandler<Session> AfterExpire;

        private SessionFactory (){
            this.tokens = new Dictionary<string, Session> ();
            this.SessionTime = new TimeSpan (0, 5, 0);

            this.thExpire = new Thread (new ThreadStart(thExpireHandler));
            this.thExpire.Start ();
        }

        ~SessionFactory (){
            Dispose (false);
        }

        public static SessionFactory Instance (String key=GLOBAL_INSTANCE_KEY) {
            if (!instances.ContainsKey(key))
                instances.Add (key, new SessionFactory ());

            return instances[key];
        }

        private void thExpireHandler ()
        {
            while (!this.disposed) {
                Thread.Sleep (100);
                this.ExpireProcess ();
            }
        }

        private void ExpireProcess () {
            lock (oLock) {
                DateTime now = DateTime.Now;

                foreach (String expireKey in this.tokens
                    .Where (kv => kv.Value.ExpireTime < now)
                    .Select (kv => kv.Key).ToArray()) {
                    Session token = this.tokens[expireKey];
                    TypedCancellableEventArgs<Session> args = new TypedCancellableEventArgs<Session> (token);
                    this.OnBeforeExpire (args);
                    if(!args.Cancel)
                        this.tokens.Remove (expireKey);
                    this.OnAfterExpire (args);
                }
            }

        }

        public TimeSpan SessionTime {
            get {
                return sessionTime;
            }

            set {
                Assert.GreaterThan (value, new TimeSpan (0), nameof (value));
                sessionTime = value;
            }
        }

        public Func<IDictionary<string, Session>, string> IdGenerator {
            get { return idGenerator; }
            set {
                Assert.NotNull (value, nameof (value));
                idGenerator = value;
            }
        }

        protected void OnBeforeCreate (TypedCancellableEventArgs<Session> args) {
            if (this.BeforeCreate != null)
                this.BeforeCreate (this, args);
        }

        protected void OnAfterCreate (TypedEventArgs<Session> args)
        {
            if (this.AfterCreate != null)
                this.AfterCreate (this, args);
        }

        protected void OnBeforeExpire (TypedCancellableEventArgs<Session> args)
        {
            if (this.BeforeExpire != null)
                this.BeforeExpire (this, args);
        }

        protected void OnAfterExpire (TypedEventArgs<Session> args)
        {
            if (this.AfterExpire != null)
                this.AfterExpire (this, args);
        }

        public bool Exists (String id)
        {
            lock (oLock) {
                return this.tokens.ContainsKey (id);
            }
        }

        public Session Get (String id) {
            lock (oLock) {
                if (!tokens.ContainsKey (id))
                    throw new SessionException ("No valid session.");

                return tokens [id];
            }
        }

        public Session New (){
            lock (oLock) {
                Session token = new Session (this
                    , this.idGenerator.Invoke (this.tokens)
                    , DateTime.Now.Add (this.sessionTime)
                    );

                TypedCancellableEventArgs<Session> args = new TypedCancellableEventArgs<Session> (token);
                this.OnBeforeCreate (args);
                if(!args.Cancel)
                    this.tokens.Add (token.Id, token);
                this.OnAfterCreate (args);

                return token;
            }
        }

        protected virtual void Dispose (bool disposing)
        {
            if (this.disposed)
                return;

            lock (oLock) {
                foreach (Session token in this.tokens.Values)
                    token.Expire ();
                this.ExpireProcess ();
            }

            this.disposed = true;
        }

        public void Dispose ()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
