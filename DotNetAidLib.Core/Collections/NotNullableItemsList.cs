using System;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Collections
{
    public class NotNullableItemsList<K>:List<K>
    {
        public new void Add(K v)
        {
            Assert.NotNull( v, nameof(v));
            this.Add(v);
        }

        public new void Insert(int index, K v)
        {
            Assert.NotNull( v, nameof(v));
            this.Insert(index, v);
        }

        public new void InsertRange(int index, IEnumerable<K> collection)
        {
            collection.ToList().ForEach(v => Assert.NotNull(v, "collection"));
            this.InsertRange(index, collection);
        }

        public new void AddRange(IEnumerable<K> collection)
        {
            collection.ToList().ForEach(v => Assert.NotNull(v, "collection"));
            this.AddRange(collection);
        }
    }
}
