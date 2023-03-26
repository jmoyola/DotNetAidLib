namespace DotNetAidLib.Core.Collections
{
    public class CaptiontItem<T>
    {
        public CaptiontItem()
            : this(default, null)
        {
        }

        public CaptiontItem(T value)
            : this(value, null)
        {
        }

        public CaptiontItem(T value, string caption)
        {
            Value = value;
            Caption = caption;
        }

        public string Caption { get; set; }
        public T Value { get; set; }

        public static implicit operator T(CaptiontItem<T> commentLine)
        {
            return commentLine.Value;
        }

        public static implicit operator CaptiontItem<T>(T value)
        {
            return new CaptiontItem<T>(value);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            return typeof(CaptiontItem<T>).IsAssignableFrom(obj.GetType())
                   && ((CaptiontItem<T>) obj).Value.Equals(Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return (string.IsNullOrEmpty(Caption) ? "" : Caption + ": ")
                   + ((object) Value is null ? "" : Value.ToString());
        }
    }
}