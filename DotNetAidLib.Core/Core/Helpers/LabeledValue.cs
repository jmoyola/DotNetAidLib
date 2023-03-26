using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Helpers
{
    public class LabeledValue<T>
    {
        private readonly T value;

        public LabeledValue(T value)
            : this(value, value == null ? "" : value.ToString())
        {
        }

        public LabeledValue(T value, string label)
        {
            Assert.NotNull(label, nameof(label));

            this.value = value;
            Label = label;
        }

        public T Value => value;
        public string Label { get; }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool showValue)
        {
            return Label + (showValue ? " (" + value + ") " : "");
        }

        public override bool Equals(object obj)
        {
            return Equals(this, obj);
        }

        public new static bool Equals(object a, object b)
        {
            // Verificación Nula del encapsulado
            if (a is null && b is null) return true;

            if (a is null && !(b is null)) return false;

            if (!(a is null) && b is null) return false;

            // Verificación de tipos
            var ta = new Optional<T>();
            var tb = new Optional<T>();

            ta = a is T ? new Optional<T>((T) a) :
                a is LabeledValue<T> ? new Optional<T>(((LabeledValue<T>) a).Value) : ta;
            tb = b is T ? new Optional<T>((T) b) :
                b is LabeledValue<T> ? new Optional<T>(((LabeledValue<T>) b).Value) : tb;

            if (!ta.HasValue || !tb.HasValue)
                return false;

            // Verificación de valores
            if ((object) ta.Value is null && (object) tb.Value is null)
                return true;
            if (!((object) tb.Value is null))
                return tb.Value.Equals(ta.Value);
            return ta.Value.Equals(tb.Value);
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
            return value.GetHashCode();
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