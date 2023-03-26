using System.Collections.Generic;
using DotNetAidLib.Core.Collections.Generic;

namespace DotNetAidLib.Core.Collections
{
    public delegate void
        DictionaryChangedEventHandler<K, V>(object sender, NotifyDictionaryChangedEventArgs<K, V> args);

    public class ObservableDictionary<K, V> : DictionaryEx<K, V>
    {
        public ObservableDictionary()
        {
        }

        public ObservableDictionary(IDictionary<K, V> dictionary) : base(dictionary)
        {
        }

        public new V this[K key]
        {
            get => base[key];

            set
            {
                var eHandler = default(NotifyDictionaryChangedEventArgs<K, V>);

                eHandler = new NotifyDictionaryChangedEventArgs<K, V>(
                    NotifyDictionaryChangedAction.Replace,
                    new KeyValuePair<K, V>(key, this[key]),
                    new KeyValuePair<K, V>(key, value));

                if (DictionaryChanged != null)
                    DictionaryChanged(this, eHandler);

                if (!eHandler.Cancel)
                    base[key] = value;
            }
        }

        public event DictionaryChangedEventHandler<K, V> DictionaryChanged;


        public new void Add(K key, V value)
        {
            NotifyDictionaryChangedEventArgs<K, V> eHandler;

            eHandler = new NotifyDictionaryChangedEventArgs<K, V>(
                NotifyDictionaryChangedAction.Add,
                new KeyValuePair<K, V>(default, default),
                new KeyValuePair<K, V>(key, value));
            if (DictionaryChanged != null)
                DictionaryChanged(this, eHandler);

            if (!eHandler.Cancel)
                base.Add(key, value);
        }


        public new void Remove(K key)
        {
            var eHandler = default(NotifyDictionaryChangedEventArgs<K, V>);

            eHandler = new NotifyDictionaryChangedEventArgs<K, V>(
                NotifyDictionaryChangedAction.Remove,
                new KeyValuePair<K, V>(key, this[key]),
                new KeyValuePair<K, V>(default, default));
            if (DictionaryChanged != null)
                DictionaryChanged(this, eHandler);

            if (!eHandler.Cancel)
                base.Remove(key);
        }


        public new void Clear()
        {
            var eHandler = default(NotifyDictionaryChangedEventArgs<K, V>);

            eHandler = new NotifyDictionaryChangedEventArgs<K, V>(
                NotifyDictionaryChangedAction.Reset,
                new KeyValuePair<K, V>(default, default),
                new KeyValuePair<K, V>(default, default));

            if (DictionaryChanged != null)
                DictionaryChanged(this, eHandler);

            if (!eHandler.Cancel)
                base.Clear();
        }
    }
}