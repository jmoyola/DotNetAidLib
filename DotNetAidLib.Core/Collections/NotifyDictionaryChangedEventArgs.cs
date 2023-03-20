using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace DotNetAidLib.Core.Collections{
	public enum NotifyDictionaryChangedAction
	{
		Add,
		Remove,
		Replace,
		Reset
	}

	public class NotifyDictionaryChangedEventArgs<K, V> : EventArgs
	{

		private NotifyDictionaryChangedAction m_Action;
		private KeyValuePair<K, V> m_OldKeyValue;
		private KeyValuePair<K, V> m_NewKeyValue;

		private bool m_Cancel;
		public NotifyDictionaryChangedEventArgs(NotifyDictionaryChangedAction Action, KeyValuePair<K, V> OldKeyValue, KeyValuePair<K, V> NewKeyValue)
		{
			m_Action = Action;
			m_OldKeyValue = OldKeyValue;
			m_NewKeyValue = NewKeyValue;
			m_Cancel = Cancel;
		}

		public NotifyDictionaryChangedAction Action {
			get { return m_Action; }
			set { m_Action = value; }
		}

		public KeyValuePair<K, V> OldKeyValue {
			get { return m_OldKeyValue; }
			set { m_OldKeyValue = value; }
		}

		public KeyValuePair<K, V> NewKeyValue {
			get { return m_NewKeyValue; }
			set { m_NewKeyValue = value; }
		}

		public bool Cancel {
			get { return m_Cancel; }
			set { m_Cancel = value; }
		}
	}
}