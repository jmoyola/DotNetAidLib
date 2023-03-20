using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAidLib.Core.Collections
{
    public class GroupList<TKey, tElement>: List<Group<TKey, tElement>>
    {
        private List<Group<TKey, tElement>> elements=new List<Group<TKey, tElement>>();
        public GroupList()
        {
        }

        public void Add(TKey group, tElement value){
            if (!this.Any(v=>v.Key.Equals(group)))
                this.Add(new Group<TKey, tElement>(group));
            this.FirstOrDefault(v => v.Key.Equals(group)).Add(value);
        }

        public void Add(TKey group, IEnumerable<tElement> values)
        {
            if (!this.Any(v => v.Key.Equals(group)))
                this.Add(new Group<TKey, tElement>(group));
            this.FirstOrDefault(v => v.Key.Equals(group)).AddRange(values);
        }

        public void Remove(TKey group){
            if (!this.Any(v => v.Key.Equals(group)))
                throw new Exception("Group don't exists.");
            this.Remove(this.FirstOrDefault(v => v.Key.Equals(group)));
        }

        public override string ToString()
        {
            return this.ToStringJoin("; ");
        }
    }
}
