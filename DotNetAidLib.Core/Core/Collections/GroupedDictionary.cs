using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Collections
{
    public class GroupedDictionary<K, V> : IDictionary<K, V>
    {
        private readonly List<GroupedDictionary<K, V>> subGroups = new List<GroupedDictionary<K, V>>();

        public string Name { get; set; }

        public GroupedDictionary<K, V> ParentGroup { get; private set; }

        public IList<GroupedDictionary<K, V>> SubGroups => subGroups.AsReadOnly();

        public IDictionary<K, V> Items { get; } = new Dictionary<K, V>();

        protected Dictionary<K, V> TotalItems
        {
            get
            {
                var ret = new Dictionary<K, V>();
                ret.AddAll(Items);
                if (ParentGroup != null)
                    ret.AddAll(ParentGroup.Items, true);

                return ret;
            }
        }

        public V this[K key]
        {
            get
            {
                if (Items.ContainsKey(key))
                    return this[key];
                if (ParentGroup != null)
                    return ParentGroup[key];
                throw new Exception("Key don't found");
            }
            set => Items[key] = value;
        }

        public ICollection<K> Keys
        {
            get
            {
                if (ParentGroup == null) return Items.Keys;

                var ret = Items.Keys.ToList();
                ret.AddRange(ParentGroup.Keys);
                return ret;
            }
        }

        public ICollection<V> Values
        {
            get
            {
                if (ParentGroup == null) return Items.Values;

                var ret = Items.Values.ToList();
                ret.AddRange(ParentGroup.Values);
                return ret;
            }
        }

        public int Count =>
            Items.Count
            + (ParentGroup == null ? 0 : ParentGroup.Count);

        public bool IsReadOnly => Items.IsReadOnly;

        public void Add(K key, V value)
        {
            Items.Add(key, value);
        }

        public void Add(KeyValuePair<K, V> item)
        {
            Items.Add(item);
        }

        public void Clear()
        {
            Clear(false);
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            var ret = false;

            ret = Items.Contains(item);
            if (!ret && ParentGroup != null)
                ret = ParentGroup.Contains(item);

            return ret;
        }

        public bool ContainsKey(K key)
        {
            var ret = false;

            ret = Items.ContainsKey(key);
            if (!ret && ParentGroup != null)
                ret = ParentGroup.ContainsKey(key);

            return ret;
        }

        public bool Remove(K key)
        {
            var ret = false;

            ret = Items.Remove(key);
            if (!ret && ParentGroup != null)
                ret = ParentGroup.Remove(key);

            return ret;
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            var ret = false;

            ret = Items.Remove(item);
            if (!ret && ParentGroup != null)
                ret = ParentGroup.Remove(item);

            return ret;
        }

        public bool TryGetValue(K key, out V value)
        {
            var ret = false;

            ret = Items.TryGetValue(key, out value);
            if (!ret && ParentGroup != null)
                ret = ParentGroup.TryGetValue(key, out value);

            return ret;
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            TotalItems.ToList().CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return TotalItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return TotalItems.GetEnumerator();
        }

        public GroupedDictionary<K, V> AddGroup(GroupedDictionary<K, V> group)
        {
            Assert.NotNull(group, nameof(group));

            group.ParentGroup = this;
            subGroups.Add(group);
            return group;
        }

        public GroupedDictionary<K, V> NewGroup(string name)
        {
            var child = new GroupedDictionary<K, V>();
            child.Name = name;

            return AddGroup(child);
        }

        public bool RemoveGroup(GroupedDictionary<K, V> group)
        {
            Assert.NotNull(group, nameof(group));

            return subGroups.Remove(group);
        }

        public bool RemoveGroup(string name)
        {
            var child = subGroups.FirstOrDefault(v => Equals(v.Name, name));
            if (child == null) return false;

            RemoveGroup(child);
            child.ParentGroup = null;
            return true;
        }

        public void Clear(bool includeParents)
        {
            Items.Clear();
            if (includeParents && ParentGroup != null)
                ParentGroup.Clear();
        }
    }
}