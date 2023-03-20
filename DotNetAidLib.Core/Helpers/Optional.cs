using System;
namespace DotNetAidLib.Core.Helpers
{
    public class Optional<T>
    {
        private T value;
        private bool hasValue;

        public Optional(){
            value = default(T);
            hasValue = false;
        }

        public Optional(T value)
        {
            this.Value = value;
        }

        public T Value
        {
            get
            {
                if (!this.hasValue)
                    throw new InvalidCastException("Optional<" + typeof(T).Name + "> has not value.");

                return this.value;
            }

            set
            {
                this.hasValue = true;
                this.value = value;
            }
        }

        public bool HasValue
        {
            get
            {
                return hasValue;
            }
        }

        public void Reset()
        {
            this.hasValue = false;
            this.value = default(T);
        }

        public override string ToString()
        {
            String ret = "Optional<" + typeof(T).Name + ">: ";
            if (!this.hasValue)
                ret += "(EMPTY)";
            else
            {
                if ((Object)this.value == null)
                    ret += "<NULL>";
                else
                    ret += this.value.ToString();
            }
            return ret;
        }

        public override bool Equals(object obj)
        {
            if (typeof(Optional<T>).IsAssignableFrom(obj.GetType())){
                Optional<T> opObj = (Optional<T>)obj;
                if (this.hasValue && opObj.hasValue)
                    return Object.Equals(this.value, opObj.value);
                else
                    return false;
            }
            else
                return false;

        }

        public override int GetHashCode()
        {
            if (!this.HasValue)
                return -1;
            else
            {
                if ((Object)this.Value == null)
                    return 0;
                else
                    return this.Value.GetHashCode();
            }
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
            return new Optional<T>(default(T));
        }
    }
}
