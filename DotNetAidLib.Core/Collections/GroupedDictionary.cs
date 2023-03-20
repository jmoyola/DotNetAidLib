using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Collections
{
    public class GroupedDictionary<K,V>: IDictionary<K, V>
    {
        private String name = null;
        private GroupedDictionary<K, V> parentGroup = null;
        private List<GroupedDictionary<K, V>> subGroups = new List<GroupedDictionary<K, V>>();
        private IDictionary<K, V> items = new Dictionary<K, V>();

        public GroupedDictionary()
        {
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
            }
        }

        public GroupedDictionary<K, V> ParentGroup
        {
            get
            {
                return parentGroup;
            }
        }

        public IList<GroupedDictionary<K, V>> SubGroups
        {
            get
            {
                return subGroups.AsReadOnly();
            }
        }

        public GroupedDictionary<K, V> AddGroup(GroupedDictionary<K, V> group)
        {
            Assert.NotNull( group, nameof(group));

            group.parentGroup = this;
            this.subGroups.Add(group);
            return group;
        }

        public GroupedDictionary<K, V> NewGroup(String name)
        {
            GroupedDictionary<K, V> child = new GroupedDictionary<K, V>();
            child.name = name;

            return this.AddGroup(child);
        }

        public bool RemoveGroup(GroupedDictionary<K, V> group)
        {
            Assert.NotNull( group, nameof(group));

            return this.subGroups.Remove(group);
        }

        public bool RemoveGroup(String name)
        {
            GroupedDictionary<K, V> child = this.subGroups.FirstOrDefault(v => Object.Equals(v.name, name));
            if (child == null)
                return false;
            else
            {
                this.RemoveGroup(child);
                child.parentGroup = null;
                return true;
            }
        }

        public IDictionary<K, V> Items
        {
            get
            {
                return this.items;
            }
        }

        public V this[K key]
        {
            get {
                if (this.items.ContainsKey(key))
                    return this[key];
                else if (this.parentGroup != null)
                    return this.parentGroup[key];
                else
                    throw new Exception("Key don't found");
            }
            set
            {
                this.items[key]=value;
            }
        }

        public ICollection<K> Keys
        {
            get
            {
                if (this.parentGroup == null)
                    return items.Keys;
                else
                {
                    List<K> ret = items.Keys.ToList();
                    ret.AddRange(this.parentGroup.Keys);
                    return ret;
                }
            }
        }

        public ICollection<V> Values
        {
            get
            {
                if (this.parentGroup == null)
                    return items.Values;
                else
                {
                    List<V> ret = items.Values.ToList();
                    ret.AddRange(this.parentGroup.Values);
                    return ret;
                }
            }
        }

        public int Count
        {
            get
            {
                return this.items.Count
                    + (this.parentGroup == null ? 0: this.parentGroup.Count);
            }
        }

        public bool IsReadOnly => items.IsReadOnly;

        public void Add(K key, V value)
        {
            this.items.Add(key, value);
        }

        public void Add(KeyValuePair<K, V> item)
        {
            this.items.Add(item);
        }

        public void Clear()
        {
            this.Clear(false);
        }

        public void Clear(bool includeParents)
        {
            this.items.Clear();
            if (includeParents && this.parentGroup != null)
                this.parentGroup.Clear();
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            bool ret = false;

            ret = this.items.Contains(item);
            if (!ret && this.parentGroup != null)
                ret = this.parentGroup.Contains(item);

            return ret;
        }

        public bool ContainsKey(K key)
        {
            bool ret = false;

            ret = this.items.ContainsKey(key);
            if (!ret && this.parentGroup != null)
                ret = this.parentGroup.ContainsKey(key);

            return ret;
        }

        public bool Remove(K key)
        {
            bool ret = false;

            ret = this.items.Remove(key);
            if (!ret && this.parentGroup != null)
                ret = this.parentGroup.Remove(key);

            return ret;
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            bool ret = false;

            ret = this.items.Remove(item);
            if (!ret && this.parentGroup != null)
                ret = this.parentGroup.Remove(item);

            return ret;
        }

        public bool TryGetValue(K key, out V value)
        {
            bool ret = false;

            ret = this.items.TryGetValue(key, out value);
            if (!ret && this.parentGroup != null)
                ret = this.parentGroup.TryGetValue(key, out value);

            return ret;
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            this.TotalItems.ToList().CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return this.TotalItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.TotalItems.GetEnumerator();
        }

        protected Dictionary<K, V> TotalItems
        {
            get
            {
                Dictionary<K, V> ret = new Dictionary<K, V>();
                ret.AddAll(this.items);
                if (this.parentGroup != null)
                    ret.AddAll(this.parentGroup.Items, true);

                return ret;
            }
        }
    }
}
