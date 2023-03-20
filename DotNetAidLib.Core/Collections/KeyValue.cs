using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace DotNetAidLib.Core.Collections 
{
	public class KeyValue<K, V>
	{
		private K m_Key;
		private V m_Value;

		public KeyValue()
		{
		}

		public KeyValue(K key, V value)
		{
			m_Key = key;
			m_Value = value;
		}

		public K Key {
			get { return m_Key; }
			set { m_Key = value; }
		}

		public V Value {
			get { return m_Value; }
			set { m_Value = value; }
		}

		public override bool Equals(object obj)
		{
			if ((typeof(KeyValue<K, V>).IsAssignableFrom(obj.GetType()))) {
				KeyValue<K, V> kv = (KeyValue<K, V>)obj;
				return this.Key.Equals(kv.Key) && this.Value.Equals(kv.Value);
			} else {
				return base.Equals(obj);
			}
		}

		public override string ToString()
		{
			return m_Key.ToString() + " - " + m_Value.ToString();
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
	}
}

