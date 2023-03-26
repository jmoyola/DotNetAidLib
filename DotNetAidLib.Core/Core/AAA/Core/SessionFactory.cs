using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.AAA
{
    public class SessionFactory : IDisposable
    {
        private const string GLOBAL_INSTANCE_KEY = "__GLOBAL_INSTANCE__";

        private static readonly IDictionary<string, SessionFactory>
            instances = new Dictionary<string, SessionFactory>();

        private bool disposed;

        private Func<IDictionary<string, Session>, string> idGenerator =
            T => Guid.NewGuid().ToString().Replace("-", "").ToUpper();

        private readonly object oLock = new object();
        private TimeSpan sessionTime = new TimeSpan(0, 5, 0);

        private readonly Thread thExpire;

        private readonly IDictionary<string, Session> tokens;

        private SessionFactory()
        {
            tokens = new Dictionary<string, Session>();
            SessionTime = new TimeSpan(0, 5, 0);

            thExpire = new Thread(thExpireHandler);
            thExpire.Start();
        }

        public TimeSpan SessionTime
        {
            get => sessionTime;

            set
            {
                Assert.GreaterThan(value, new TimeSpan(0), nameof(value));
                sessionTime = value;
            }
        }

        public Func<IDictionary<string, Session>, string> IdGenerator
        {
            get => idGenerator;
            set
            {
                Assert.NotNull(value, nameof(value));
                idGenerator = value;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public event TypedCancellableEventHandler<Session> BeforeCreate;
        public event TypedEventHandler<Session> AfterCreate;

        public event TypedCancellableEventHandler<Session> BeforeExpire;
        public event TypedEventHandler<Session> AfterExpire;

        ~SessionFactory()
        {
            Dispose(false);
        }

        public static SessionFactory Instance(string key = GLOBAL_INSTANCE_KEY)
        {
            if (!instances.ContainsKey(key))
                instances.Add(key, new SessionFactory());

            return instances[key];
        }

        private void thExpireHandler()
        {
            while (!disposed)
            {
                Thread.Sleep(100);
                ExpireProcess();
            }
        }

        private void ExpireProcess()
        {
            lock (oLock)
            {
                var now = DateTime.Now;

                foreach (var expireKey in tokens
                             .Where(kv => kv.Value.ExpireTime < now)
                             .Select(kv => kv.Key).ToArray())
                {
                    var token = tokens[expireKey];
                    var args = new TypedCancellableEventArgs<Session>(token);
                    OnBeforeExpire(args);
                    if (!args.Cancel)
                        tokens.Remove(expireKey);
                    OnAfterExpire(args);
                }
            }
        }

        protected void OnBeforeCreate(TypedCancellableEventArgs<Session> args)
        {
            if (BeforeCreate != null)
                BeforeCreate(this, args);
        }

        protected void OnAfterCreate(TypedEventArgs<Session> args)
        {
            if (AfterCreate != null)
                AfterCreate(this, args);
        }

        protected void OnBeforeExpire(TypedCancellableEventArgs<Session> args)
        {
            if (BeforeExpire != null)
                BeforeExpire(this, args);
        }

        protected void OnAfterExpire(TypedEventArgs<Session> args)
        {
            if (AfterExpire != null)
                AfterExpire(this, args);
        }

        public bool Exists(string id)
        {
            lock (oLock)
            {
                return tokens.ContainsKey(id);
            }
        }

        public Session Get(string id)
        {
            lock (oLock)
            {
                if (!tokens.ContainsKey(id))
                    throw new SessionException("No valid session.");

                return tokens[id];
            }
        }

        public Session New()
        {
            lock (oLock)
            {
                var token = new Session(this
                    , idGenerator.Invoke(tokens)
                    , DateTime.Now.Add(sessionTime)
                );

                var args = new TypedCancellableEventArgs<Session>(token);
                OnBeforeCreate(args);
                if (!args.Cancel)
                    tokens.Add(token.Id, token);
                OnAfterCreate(args);

                return token;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            lock (oLock)
            {
                foreach (var token in tokens.Values)
                    token.Expire();
                ExpireProcess();
            }

            disposed = true;
        }
    }
}