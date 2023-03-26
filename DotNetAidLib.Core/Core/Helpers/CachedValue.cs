using System;

namespace DotNetAidLib.Core.Helpers
{
    public class CachedValue<T>
    {
        private T value;

        public CachedValue(DateTime time, T value)
        {
            Time = time;
            this.value = value;
        }

        public T Value => value;

        public DateTime Time { get; }

        public override string ToString()
        {
            return value.ToString();
        }

        public override bool Equals(object obj)
        {
            return value.Equals(obj);
        }

        public override int GetHashCode()
        {
#pragma warning disable RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
            return value.GetHashCode();
#pragma warning restore RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
        }

        public static implicit operator T(CachedValue<T> v)
        {
            return v.Value;
        }
    }
}