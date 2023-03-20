using System;


namespace DotNetAidLib.Core.Helpers
{
    public class CachedValue<T> {
        private T value;
        private DateTime time;

        public CachedValue(DateTime time, T value)
        {
            this.time = time;
            this.value = value;
        }

        public T Value
        {
            get
            {
                return value;
            }

        }

        public DateTime Time
        {
            get
            {
                return time;
            }
        }

        public override string ToString()
        {
            return this.value.ToString();
        }

        public override bool Equals(object obj)
        {
            return this.value.Equals(obj);
        }

        public override int GetHashCode()
        {
#pragma warning disable RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
            return this.value.GetHashCode();
#pragma warning restore RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
        }

        public static implicit operator T(CachedValue<T> v)
        {
            return v.Value;
        }
    }
}
