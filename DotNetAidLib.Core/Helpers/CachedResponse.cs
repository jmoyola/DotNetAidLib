using System;
namespace DotNetAidLib.Core.Helpers
{
    public class CachedResponse<T>
    {
		private DateTime lastCachedTime = new DateTime(1974, 1, 1, 0, 0, 0);
		private T value=default(T);
        public CachedResponse()
			:this(default(T)){}

		public CachedResponse(T value)
            : this(value, new DateTime(1974, 1, 1, 0, 0, 0)) { }

		public CachedResponse(T value, DateTime lastCachedTime)
        {
			this.value = value;
			this.lastCachedTime = lastCachedTime;
        }

		public DateTime LastCachedTime
		{
			get
			{
				return lastCachedTime;
			}

			set
			{
				lastCachedTime = value;
			}
		}

		public T Value
		{
			get
			{
				return value;
			}

			set
			{
				this.value = value;
			}
		}

		public override bool Equals(object obj)
        {
			if ((Object)this.value != null)
				return this.value.Equals(obj);
			else
				return (obj==null);
        }

        public override int GetHashCode()
        {
			if ((Object)this.value != null)
				return this.value.GetHashCode();
			else
				return 0;
        }

		public static implicit operator CachedResponse<T>(T v)
        {
			return new CachedResponse<T>(v);
        }

		public static implicit operator T(CachedResponse<T> v)
        {
            return v.Value;
        }
	}
}
