using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.Helpers
{
    public class LabeledValue<T>
    {
        private readonly T value;
        private String label;

        public LabeledValue(T value)
            : this(value, ((Object)value == null ? "": value.ToString())) { }

        public LabeledValue(T value, String label)
        {
            Assert.NotNull(label, nameof(label));

            this.value = value;
            this.label = label;
        }

        public T Value { get { return this.value; } }
        public String Label { get { return this.label; } }

        public override string ToString() {
            return this.ToString(false);
        }

        public string ToString(bool showValue)
        {
            return this.label + (showValue ? " (" + this.value + ") ":"");
        }

        public override bool Equals(object obj)
        {
            return Equals(this, obj);
        }

        public new static bool Equals(object a, object b)
        {
            // Verificación Nula del encapsulado
            if (a is null && b is null)
                return true;
            else if (a is null && !(b is null))
                return false;
            else if (!(a is null) && b is null)
                return false;
            else
            {
                // Verificación de tipos
                Optional<T> ta = new Optional<T>();
                Optional<T> tb = new Optional<T>();

                ta = (a is T ? new Optional<T>((T)a) : (a is LabeledValue<T> ? new Optional<T>(((LabeledValue<T>)a).Value):ta));
                tb = (b is T ? new Optional<T>((T)b) : (b is LabeledValue<T> ? new Optional<T>(((LabeledValue<T>)b).Value):tb));

                if (!ta.HasValue || !tb.HasValue)
                    return false;

                // Verificación de valores
                if ((Object)ta.Value is null && (Object)tb.Value is null)
                    return true;
                else if (!((Object)tb.Value is null))
                    return tb.Value.Equals(ta.Value);
                else
                    return ta.Value.Equals(tb.Value);
            }
        }

        public static bool operator ==(LabeledValue<T> a, LabeledValue<T> b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(LabeledValue<T> a, LabeledValue<T> b)
        {
            return !Equals(a, b);
        }

        public static bool operator ==(T a, LabeledValue<T> b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(T a, LabeledValue<T> b)
        {
            return !Equals(a, b);
        }

        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        public static implicit operator LabeledValue<T>(T v)
        {
            return new LabeledValue<T>(v);
        }

        public static implicit operator T(LabeledValue<T> v)
        {
            return v.value;
        }
    }
}
