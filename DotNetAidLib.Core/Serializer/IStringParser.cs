using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAidLib.Core.Serializer
{
    public abstract class IStringParser
    {
        public abstract string Syntax { get; }
        public abstract String Parse(Object value);
        public abstract Object Unparse(String value, Type type=null);
        
        public T Unparse<T>(String value) {
            Object oValue = this.Unparse(value, typeof(T));
            return (T)Convert.ChangeType(oValue, typeof(T));
        }

        public virtual IList<T> UnparseList<T>(String value) {
            IList<Object> oValue = (IList<Object>)this.Unparse(value);
            return oValue.Cast<T>().ToList();
        }

        public bool TryUnparse<T>(String value, out T outValue)
        {
            outValue = default(T);
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