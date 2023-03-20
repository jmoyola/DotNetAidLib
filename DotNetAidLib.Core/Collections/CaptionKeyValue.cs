using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace DotNetAidLib.Core.Collections
{
	public class CaptionKeyValue<K, V>:KeyValue<K, V>
	{
		private String caption;

		public CaptionKeyValue()
		{
		}

		public CaptionKeyValue(K key)
		{
			this.Key = key;
		}

		public CaptionKeyValue(K key, V value)
		{
			this.Key = key;
			this.Value = value;
		}

		public CaptionKeyValue(K key, V value, String caption)
			:this(key, value)
		{
			this.caption = caption;
		}

		public String Caption {
			get { return caption; }
			set { caption = value; }
		}


	}
}

