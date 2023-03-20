using System;
using System.Linq;

namespace DotNetAidLib.Core.Collections
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data;
	using System.Diagnostics;
    public class CaptionDictionary<K, V> : Dictionary<K, CaptiontItem<V>>
    {
        private String m_Comment = null;

        public String Comment
        {
            get { return m_Comment; }
            set { m_Comment = value; }
        }

        public void Add(K key, V value, String comment){
            this.Add(key, new CaptiontItem<V>(value, comment));
        }
        public void AddOrUpdate(K key, V value, String comment){
            this.AddOrUpdate(key, new CaptiontItem<V>(value, comment));
        }
    }
}

