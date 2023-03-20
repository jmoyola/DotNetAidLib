using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAidLib.Core.Collections
{
    public class Group<TKey, tElement>:List<tElement>, IGrouping<TKey, tElement>
    {
        private TKey key;
        public Group(TKey key)
        {
            this.key = key;
        }

        TKey IGrouping<TKey, tElement>.Key{
            get{
                return this.key;
            }
        }

        public TKey Key
        {
            get
            {
                return this.key;
            }
        }

        public override string ToString()
        {
            if(this.key == null)
                return "<null> [" + this.ToStringJoin(", ") + "]";
            else
                return this.key.ToString() + " [" + this.ToStringJoin(", ") + "]";
        }

        public override int GetHashCode(){
            if (this.Key == null)
                return 0;
            else
                return this.Key.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && typeof(Group<TKey, tElement>).IsAssignableFrom(obj.GetType()))
                return this.key.Equals(((Group<TKey, tElement>)obj).key);
            else
                return false;
        }
    }
}
