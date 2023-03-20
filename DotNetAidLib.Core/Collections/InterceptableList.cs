using System;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Collections
{
    public delegate void ItemEventHandler<T>(Object sender, ItemEventArgs<T> args);

    public class ItemEventArgs<T>{
        private T item;
        private int index=-1;

        public ItemEventArgs(T item)
            :this(item, -1){}

        public ItemEventArgs(T item, int index)
        {
            this.item = item;
            this.index = index;
        }

        public T Item
        {
            get
            {
                return item;
            }
        }

        public int Index
        {
            get
            {
                return index;
            }
        }
    }

    public class InterceptableList<T>:List<T>
    {
        public event ItemEventHandler<T> BeforeAdd;
        public event ItemEventHandler<T> AfterAdd;
        public event ItemEventHandler<T> BeforeInsert;
        public event ItemEventHandler<T> AfterInsert;
        public event ItemEventHandler<T> BeforeRemove;
        public event ItemEventHandler<T> AfterRemove;

        public new void Add(T item)
        {
            this.OnBeforeAdd(new ItemEventArgs<T>(item));
            base.Add(item);
            this.OnAfterAdd(new ItemEventArgs<T>(item, this.IndexOf(item)));
        }

        public new void Insert(int index, T item)
        {
            this.OnBeforeInsert(new ItemEventArgs<T>(item));
            base.Insert(index, item);
            this.OnAfterInsert(new ItemEventArgs<T>(item, index));
        }

        public new void InsertRange(int index, IEnumerable<T> collection)
        {
            for (int i = 0; i < collection.Count(); i++)
            {
                T item = collection.ToList()[i];
                base.Insert(index+i, item);
            }
        }

        public new void AddRange(IEnumerable<T> collection)
        {
            collection.ToList().ForEach(v => base.Add(v));
        }

        public new bool Remove(T item)
        {
            bool ret;
            int index = this.IndexOf(item);
            this.OnBeforeRemove(new ItemEventArgs<T>(item, index));
            ret = base.Remove(item);
            this.OnAfterRemove(new ItemEventArgs<T>(item, index));
            return ret;
        }

        public new void RemoveAt(int index)
        {
            T item = this[index];
            this.OnBeforeRemove(new ItemEventArgs<T>(item, index));
            base.RemoveAt(index);
            this.OnAfterRemove(new ItemEventArgs<T>(item, index));
        }

        public new void RemoveRange(int index, int count)
        {
            for (int i = (index + count) - 1; i >= index; i--){
                this.RemoveAt(i);
            }
        }

        public new void RemoveAll(Predicate<T> matches)
        {
            this.Where(v => matches.Invoke(v)).ToList().ForEach(i=>this.Remove(i));
        }

        public new void Clear()
        {
            this.RemoveRange(0, this.Count);
        }

        protected void OnBeforeAdd(ItemEventArgs<T> args)
        {
            if (this.BeforeAdd != null)
                this.BeforeAdd(this, args);
        }

        protected void OnAfterAdd(ItemEventArgs<T> args)
        {
            if (this.AfterAdd != null)
                this.AfterAdd(this, args);
        }

        protected void OnBeforeInsert(ItemEventArgs<T> args)
        {
            if (this.BeforeInsert != null)
                this.BeforeInsert(this, args);
        }

        protected void OnAfterInsert(ItemEventArgs<T> args)
        {
            if (this.AfterInsert != null)
                this.AfterInsert(this, args);
        }

        protected void OnBeforeRemove(ItemEventArgs<T> args)
        {
            if (this.BeforeRemove != null)
                this.BeforeRemove(this, args);
        }

        protected void OnAfterRemove(ItemEventArgs<T> args)
        {
            if (this.AfterRemove != null)
                this.AfterRemove(this, args);
        }

    }
}
