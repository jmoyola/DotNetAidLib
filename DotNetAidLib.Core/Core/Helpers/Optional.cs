using System;

namespace DotNetAidLib.Core.Helpers
{
    public class Optional<T>
    {
        private T value;

        public Optional()
        {
            value = default;
            HasValue = false;
        }

        public Optional(T value)
        {
            Value = value;
        }

        public T Value
        {
            get
            {
                if (!HasValue)
                    throw new InvalidCastException("Optional<" + typeof(T).Name + "> has not value.");

                return value;
            }

            set
            {
                HasValue = true;
                this.value = value;
            }
        }

        public bool HasValue { get; private set; }

        public void Reset()
        {
            HasValue = false;
            value = default;
        }

        public override string ToString()
        {
            var ret = "Optional<" + typeof(T).Name + ">: ";
            if (!HasValue)
            {
                ret += "(EMPTY)";
            }
            else
            {
                if (value == null)
                    ret += "<NULL>";
                else
                    ret += value.ToString();
            }

            return ret;
        }

        public override bool Equals(object obj)
        {
            if (typeof(Optional<T>).IsAssignableFrom(obj.GetType()))
            {
                var opObj = (Optional<T>) obj;
                if (HasValue && opObj.HasValue)
                    return Equals(value, opObj.value);
                return false;
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (!HasValue) return -1;

            if (Value == null)
                return 0;
            return Value.GetHashCode();
        }

        public static implicit operator Optional<T>(T v)
        {
            return new Optional<T>(v);
        }

        public static implicit operator T(Optional<T> v)
        {
            return v.Value;
        }

        public static Optional<T> FromDefault()
        {
            return new Optional<T>(default);
        }
    }
}