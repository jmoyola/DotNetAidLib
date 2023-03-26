using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAidLib.Core.Collections
{
    public delegate void ItemEventHandler<T>(object sender, ItemEventArgs<T> args);

    public class ItemEventArgs<T>
    {
        public ItemEventArgs(T item)
            : this(item, -1)
        {
        }

        public ItemEventArgs(T item, int index)
        {
            Item = item;
            Index = index;
        }

        public T Item { get; }

        public int Index { get; } = -1;
    }

    public class InterceptableList<T> : List<T>
    {
        public event ItemEventHandler<T> BeforeAdd;
        public event ItemEventHandler<T> AfterAdd;
        public event ItemEventHandler<T> BeforeInsert;
        public event ItemEventHandler<T> AfterInsert;
        public event ItemEventHandler<T> BeforeRemove;
        public event ItemEventHandler<T> AfterRemove;

        public new void Add(T item)
        {
            OnBeforeAdd(new ItemEventArgs<T>(item));
            base.Add(item);
            OnAfterAdd(new ItemEventArgs<T>(item, IndexOf(item)));
        }

        public new void Insert(int index, T item)
        {
            OnBeforeInsert(new ItemEventArgs<T>(item));
            base.Insert(index, item);
            OnAfterInsert(new ItemEventArgs<T>(item, index));
        }

        public new void InsertRange(int index, IEnumerable<T> collection)
        {
            for (var i = 0; i < collection.Count(); i++)
            {
                var item = collection.ToList()[i];
                base.Insert(index + i, item);
            }
        }

        public new void AddRange(IEnumerable<T> collection)
        {
            collection.ToList().ForEach(v => base.Add(v));
        }

        public new bool Remove(T item)
        {
            bool ret;
            var index = IndexOf(item);
            OnBeforeRemove(new ItemEventArgs<T>(item, index));
            ret = base.Remove(item);
            OnAfterRemove(new ItemEventArgs<T>(item, index));
            return ret;
        }

        public new void RemoveAt(int index)
        {
            var item = this[index];
            OnBeforeRemove(new ItemEventArgs<T>(item, index));
            base.RemoveAt(index);
            OnAfterRemove(new ItemEventArgs<T>(item, index));
        }

        public new void RemoveRange(int index, int count)
        {
            for (var i = index + count - 1; i >= index; i--) RemoveAt(i);
        }

        public new void RemoveAll(Predicate<T> matches)
        {
            this.Where(v => matches.Invoke(v)).ToList().ForEach(i => Remove(i));
        }

        public new void Clear()
        {
            RemoveRange(0, Count);
        }

        protected void OnBeforeAdd(ItemEventArgs<T> args)
        {
            if (BeforeAdd != null)
                BeforeAdd(this, args);
        }

        protected void OnAfterAdd(ItemEventArgs<T> args)
        {
            if (AfterAdd != null)
                AfterAdd(this, args);
        }

        protected void OnBeforeInsert(ItemEventArgs<T> args)
        {
            if (BeforeInsert != null)
                BeforeInsert(this, args);
        }

        protected void OnAfterInsert(ItemEventArgs<T> args)
        {
            if (AfterInsert != null)
                AfterInsert(this, args);
        }

        protected void OnBeforeRemove(ItemEventArgs<T> args)
        {
            if (BeforeRemove != null)
                BeforeRemove(this, args);
        }

        protected void OnAfterRemove(ItemEventArgs<T> args)
        {
            if (AfterRemove != null)
                AfterRemove(this, args);
        }
    }
}