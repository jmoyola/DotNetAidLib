using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;

namespace DotNetAidLib.Core.Collections.Generic{
	public class DictionaryEx<K, V> : Dictionary<K, V>
	{

		public DictionaryEx() : base()
		{
		}

		public DictionaryEx(IDictionary<K, V> dictionary) : base(dictionary)
		{
		}


		public void Add(K key, V value, bool updateIfExists)
		{
			if ((!base.ContainsKey(key))) {
				this.Add(key, value);
			} else {
				if ((updateIfExists)) {
					this[key] = value;
				} else {
					throw new KeyNotFoundException("Key already exists.");
				}
			}
		}

		public V GetValue(K key)
		{
			V ret=default(V);

			if ((base.ContainsKey(key))) {
				ret = this[key];
			} else {
				throw new KeyNotFoundException("Key doin't exists.");
			}

			return ret;
		}

		public V GetValue(K key, V defaultValue)
		{
			V ret = defaultValue;

			if ((base.ContainsKey(key))) {
				ret = this[key];
			}

			return ret;
		}

		public V GetValue(K key, V defaultValue, bool createIfNotExists)
		{
			V ret = defaultValue;

			if ((!base.ContainsKey(key))) {
				if ((createIfNotExists)) {
					this.Add(key, defaultValue);
				}
			} else {
				ret = this[key];
			}

			return this[key];
		}


		public void SetValue(K key, V value)
		{
			if ((base.ContainsKey(key))) {
				this[key] = value;
			} else {
				throw new KeyNotFoundException("Key doin't exists.");
			}

		}

		public void SetValue(K key, V value, bool createIfNotExists)
		{
			if ((base.ContainsKey(key))) {
				this[key] = value;
			} else {
				if ((createIfNotExists)) {
					this.Add(key, value);
				} else {
					throw new KeyNotFoundException("Key doin't exists.");
				}

			}
		}

		public override string ToString()
		{
			string ret = "";

			foreach (KeyValuePair<K, V> kv in this) {
				ret = ret + ", (";
				if ((kv.Key == null)) {
					ret = ret + "null";
				} else {
					ret = ret + kv.Key.ToString();
				}
				ret = ret + ", ";
				if ((kv.Value == null)) {
					ret = ret + "null";
				} else {
					ret = ret + kv.Value.ToString();
				}
				ret = ret + ")";
			}
			if ((ret.Length > 0)) {
				ret = ret.Substring(2);
			}

			return ret;
		}
	}
}