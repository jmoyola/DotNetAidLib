using System.Collections.Generic;
using System.Linq;

namespace DotNetAidLib.Core.Collections
{
    public class Group<TKey, tElement> : List<tElement>, IGrouping<TKey, tElement>
    {
        private TKey key;

        public Group(TKey key)
        {
            this.key = key;
        }

        public TKey Key => key;

        TKey IGrouping<TKey, tElement>.Key => key;

        public override string ToString()
        {
            if (key == null)
                return "<null> [" + this.ToStringJoin(", ") + "]";
            return key + " [" + this.ToStringJoin(", ") + "]";
        }

        public override int GetHashCode()
        {
            if (Key == null)
                return 0;
            return Key.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && typeof(Group<TKey, tElement>).IsAssignableFrom(obj.GetType()))
                return key.Equals(((Group<TKey, tElement>) obj).key);
            return false;
        }
    }
}