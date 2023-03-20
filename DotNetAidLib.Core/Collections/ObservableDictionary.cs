using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using DotNetAidLib.Core.Collections.Generic;

namespace DotNetAidLib.Core.Collections{
	public delegate void DictionaryChangedEventHandler<K, V>(object sender, NotifyDictionaryChangedEventArgs<K, V> args);

	public class ObservableDictionary<K, V> : DictionaryEx<K, V>
	{

		public event DictionaryChangedEventHandler<K, V> DictionaryChanged;

		public ObservableDictionary() : base()
		{
		}

		public ObservableDictionary(IDictionary<K, V> dictionary) : base(dictionary)
		{
		}


		public new void Add(K key, V value)
		{

			NotifyDictionaryChangedEventArgs<K, V> eHandler;

			eHandler = new NotifyDictionaryChangedEventArgs<K, V>(
				NotifyDictionaryChangedAction.Add,
				new KeyValuePair<K, V>(default(K), default(V)),
				new KeyValuePair<K, V>(key, value));
			if (DictionaryChanged != null)
				DictionaryChanged(this, eHandler);

			if ((!eHandler.Cancel))
				base.Add(key, value);
		}


		public new void Remove(K key)
		{
			NotifyDictionaryChangedEventArgs<K, V> eHandler = default(NotifyDictionaryChangedEventArgs<K, V>);

			eHandler = new NotifyDictionaryChangedEventArgs<K, V>(
				NotifyDictionaryChangedAction.Remove,
				new KeyValuePair<K, V>(key, this[key]),
				new KeyValuePair<K, V>(default(K), default(V)));
			if (DictionaryChanged != null)
				DictionaryChanged(this, eHandler);

			if ((!eHandler.Cancel))
				base.Remove(key);
		}

		public new V this[K key] {
			get { return base[key]; }

			set {
				NotifyDictionaryChangedEventArgs<K, V> eHandler = default(NotifyDictionaryChangedEventArgs<K, V>);

				eHandler = new NotifyDictionaryChangedEventArgs<K, V>(
					NotifyDictionaryChangedAction.Replace,
					new KeyValuePair<K, V>(key, this[key]),
					new KeyValuePair<K, V>(key, value));

				if (DictionaryChanged != null)
					DictionaryChanged(this, eHandler);

				if ((!eHandler.Cancel))
					base[key] = value;
			}
		}


		public new void Clear()
		{
			NotifyDictionaryChangedEventArgs<K, V> eHandler = default(NotifyDictionaryChangedEventArgs<K, V>);

			eHandler = new NotifyDictionaryChangedEventArgs<K, V>(
				NotifyDictionaryChangedAction.Reset,
				new KeyValuePair<K, V>(default(K), default(V)),
				new KeyValuePair<K, V>(default(K), default(V)));

			if (DictionaryChanged != null)
				DictionaryChanged(this, eHandler);

			if ((!eHandler.Cancel))
				base.Clear();

		}
	}
}