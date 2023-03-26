using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAidLib.Core.Serializer
{
    public abstract class IStringParser
    {
        public abstract string Syntax { get; }
        public abstract string Parse(object value);
        public abstract object Unparse(string value, Type type = null);

        public T Unparse<T>(string value)
        {
            var oValue = Unparse(value, typeof(T));
            return (T) Convert.ChangeType(oValue, typeof(T));
        }

        public virtual IList<T> UnparseList<T>(string value)
        {
            var oValue = (IList<object>) Unparse(value);
            return oValue.Cast<T>().ToList();
        }

        public bool TryUnparse<T>(string value, out T outValue)
        {
            outValue = default;
            try
            {
                outValue = Unparse<T>(value);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}