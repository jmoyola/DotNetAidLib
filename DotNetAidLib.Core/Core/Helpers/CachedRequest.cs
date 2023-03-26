using System;
using System.Collections.Generic;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Helpers
{
    public class CachedRequest<K, R>
    {
        private readonly IDictionary<K, CachedResponse<R>> cache = new Dictionary<K, CachedResponse<R>>();
        private int cachedTime = 5 * 60 * 1000;

        private readonly object oGetValue = new object();

        public CachedRequest()
            : this(5 * 60 * 1000)
        {
        }

        public CachedRequest(int cachedTime)
        {
            CachedTime = cachedTime;
        }

        public int CachedTime
        {
            get => cachedTime;

            set
            {
                Assert.GreaterThan(value, 0, nameof(value));
                cachedTime = value;
            }
        }

        private void PurgeCache()
        {
            lock (oGetValue)
            {
                foreach (var k in cache.Keys)
                {
                    var v = cache[k];
                    if (DateTime.Now.Subtract(v.LastCachedTime).TotalMilliseconds > cachedTime)
                        cache.Remove(k);
                }
            }
        }

        public R GetValue(K key, Func<K, R> getValueFunction)
        {
            lock (oGetValue)
            {
                var ret = default(R);

                PurgeCache();

                if (cache.ContainsKey(key))
                {
                    ret = cache[key].Value;
                }
                else
                {
                    ret = getValueFunction.Invoke(key);
                    cache.Add(key, new CachedResponse<R>(ret, DateTime.Now));
                }

                return ret;
            }
        }
    }
}