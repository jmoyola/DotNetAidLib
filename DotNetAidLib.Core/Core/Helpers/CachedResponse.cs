using System;

namespace DotNetAidLib.Core.Helpers
{
    public class CachedResponse<T>
    {
        private T value;

        public CachedResponse()
            : this(default)
        {
        }

        public CachedResponse(T value)
            : this(value, new DateTime(1974, 1, 1, 0, 0, 0))
        {
        }

        public CachedResponse(T value, DateTime lastCachedTime)
        {
            this.value = value;
            LastCachedTime = lastCachedTime;
        }

        public DateTime LastCachedTime { get; set; } = new DateTime(1974, 1, 1, 0, 0, 0);

        public T Value
        {
            get => value;

            set => this.value = value;
        }

        public override bool Equals(object obj)
        {
            if (value != null)
                return value.Equals(obj);
            return obj == null;
        }

        public override int GetHashCode()
        {
            if (value != null)
                return value.GetHashCode();
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