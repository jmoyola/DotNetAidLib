using System.Collections.Generic;

namespace DotNetAidLib.Core.Collections
{
    public class CaptionDictionary<K, V> : Dictionary<K, CaptiontItem<V>>
    {
        public string Comment { get; set; } = null;

        public void Add(K key, V value, string comment)
        {
            Add(key, new CaptiontItem<V>(value, comment));
        }

        public void AddOrUpdate(K key, V value, string comment)
        {
            this.AddOrUpdate(key, new CaptiontItem<V>(value, comment));
        }
    }
}