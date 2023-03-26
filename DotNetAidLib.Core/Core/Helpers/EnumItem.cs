using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Helpers
{
    public class EnumItem<T>
    {
        private readonly T value;

        public EnumItem(T value, string name, Type type = null, string description = null)
        {
            Assert.NotNullOrEmpty(name, nameof(name));

            Type = type;
            this.value = value;
            Name = name;
            Description = description;
        }

        public Type Type { get; }

        public T Value => value;
        public string Name { get; }

        public string Description { get; }

        public override string ToString()
        {
            return Name + (Type == null ? "" : "(" + Type.Name + ")") + ": " + value +
                   (Description == null ? "" : " (" + Description + ")");
        }

        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                if (obj is T)
                    return Equals(value, (T) obj);
                if (GetType().IsAssignableFrom(obj.GetType()))
                    return Equals(value, ((EnumItem<T>) obj).value);
                return false;
            }

            return false;
        }

        public static bool operator ==(EnumItem<T> a, EnumItem<T> b)
        {
            if (a is null || b is null)
                return false;
            return a.value.Equals(b.value);
        }

        public static bool operator !=(EnumItem<T> a, EnumItem<T> b)
        {
            if (a is null || b is null)
                return false;
            return !a.value.Equals(b.value);
        }

        public static bool operator ==(T a, EnumItem<T> b)
        {
            if (b is null)
                return false;
            return a.Equals(b.value);
        }

        public static bool operator !=(T a, EnumItem<T> b)
        {
            if (b is null)
                return false;
            return !a.Equals(b.value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public static implicit operator EnumItem<T>(T v)
        {
            var vs = v.ToString();
            return new EnumItem<T>(v, "value_" + vs, null, "description_" + vs);
        }

        public static implicit operator T(EnumItem<T> v)
        {
            return v.value;
        }

        public static bool Equals(EnumItem<T> a, EnumItem<T> b)
        {
            if (a is null && b is null)
                return false;
            if (!(a is null))
                return a.Equals(b);
            if (!(b is null))
                return b.Equals(a);
            return a.Equals(b);
        }

        public static bool Equals(T a, T b)
        {
            if (a == null && b == null)
                return true;
            if (a != null)
                return a.Equals(b);
            if (b != null)
                return b.Equals(a);
            return a.Equals(b);
        }
    }
}