using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DotNetAidLib.Database
{
    [DefaultProperty("Value")]
    public class DBNullable<T>
        : IComparable<T>
    {
        private object _Value;

        public DBNullable(object value)
        {
            RawValue = value;
        }

        public DBNullable(T value)
        {
            _Value = value;
        }

        public DBNullable(DBNull value)
        {
            _Value = value;
        }

        public bool HasValue => !DBNull.Value.Equals(_Value);

        public object RawValue
        {
            get => _Value;

            set
            {
                try
                {
                    if (typeof(DBNull).IsInstanceOfType(value))
                        _Value = value;
                    else
                        _Value = Convert.ChangeType(value, typeof(T));
                }
                catch (Exception ex)
                {
                    throw new Exception("Only allowed 'DBNull' or '" + typeof(T).Name + "' types value.", ex);
                }
            }
        }

        public T Value
        {
            get
            {
                if (typeof(T).IsInstanceOfType(_Value))
                    return (T) _Value;
                throw new Exception("Value is 'DBNull'.");
            }
            set => _Value = value;
        }

        public int CompareTo(T n2)
        {
            if (HasValue)
                return Comparer<T>.Default.Compare(Value, n2);
            return 1;
        }

        public static implicit operator DBNullable<T>(T v)
        {
            return new DBNullable<T>(v);
        }

        public static implicit operator DBNullable<T>(DBNull v)
        {
            return new DBNullable<T>(v);
        }

        public static implicit operator T(DBNullable<T> v)
        {
            return v.Value;
        }

        public override string ToString()
        {
            if (HasValue)
                return "" + Value;
            return string.Empty;
        }

        public override bool Equals(object obj)
        {
            if (DBNull.Value.Equals(obj))
                return !HasValue;
            if (!(obj is DBNullable<T>))
                return false;

            return Equals((DBNullable<T>) obj);
        }

        public bool Equals(DBNullable<T> other)
        {
            if (other.HasValue != HasValue)
                return false;

            if (!HasValue)
                return true;

            return other.Value.Equals(Value);
        }

        public int CompareTo(DBNullable<T> n2)
        {
            if (HasValue)
            {
                if (!n2.HasValue)
                    return 1;

                return Comparer<T>.Default.Compare(Value, n2.Value);
            }

            return n2.HasValue ? -1 : 0;
        }

        public override int GetHashCode()
        {
            if (HasValue)
                return Value.GetHashCode();
            return 0;
        }

        public static int Compare(DBNullable<T> n1, DBNullable<T> n2)
        {
            if (n1.HasValue)
            {
                if (!n2.HasValue)
                    return 1;

                return Comparer<T>.Default.Compare(n1.Value, n2.Value);
            }

            return n2.HasValue ? -1 : 0;
        }

        public static bool Equals(DBNullable<T> n1, DBNullable<T> n2)
        {
            if (n1.HasValue != n2.HasValue)
                return false;

            if (!n1.HasValue)
                return true;

            return EqualityComparer<T>.Default.Equals(n1.Value, n2.Value);
        }
    }
}