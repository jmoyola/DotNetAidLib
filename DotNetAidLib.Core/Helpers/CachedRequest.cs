using System;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Helpers
{
	public class CachedRequest<K,R>
    {
		private int cachedTime = 5 * 60 * 1000;
        private IDictionary<K, CachedResponse<R>> cache = new Dictionary<K, CachedResponse<R>>();

        public CachedRequest()
			:this(5 * 60 * 1000){}

		public CachedRequest(int cachedTime)
        {
			this.CachedTime = cachedTime;
        }

		public int CachedTime
        {
            get
            {
                return cachedTime;
            }

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
                foreach (K k in this.cache.Keys)
                {
                    CachedResponse<R> v = this.cache[k];
                    if (DateTime.Now.Subtract(v.LastCachedTime).TotalMilliseconds > this.cachedTime)
                        this.cache.Remove(k);
                }
            }
        }

		private Object oGetValue = new object();
		public R GetValue(K key, Func<K, R> getValueFunction){
			lock (oGetValue)
			{
				R ret = default(R);

				this.PurgeCache();

				if (this.cache.ContainsKey(key))
					ret = this.cache[key].Value;
				else
				{
					ret = getValueFunction.Invoke(key);
					this.cache.Add(key, new CachedResponse<R>(ret, DateTime.Now));
				}

				return ret;
			}
		}
    }
}
