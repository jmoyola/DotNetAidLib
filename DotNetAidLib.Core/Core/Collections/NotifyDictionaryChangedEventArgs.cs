using System;
using System.Collections.Generic;

namespace DotNetAidLib.Core.Collections
{
    public enum NotifyDictionaryChangedAction
    {
        Add,
        Remove,
        Replace,
        Reset
    }

    public class NotifyDictionaryChangedEventArgs<K, V> : EventArgs
    {
        public NotifyDictionaryChangedEventArgs(NotifyDictionaryChangedAction Action, KeyValuePair<K, V> OldKeyValue,
            KeyValuePair<K, V> NewKeyValue)
        {
            this.Action = Action;
            this.OldKeyValue = OldKeyValue;
            this.NewKeyValue = NewKeyValue;
            Cancel = Cancel;
        }

        public NotifyDictionaryChangedAction Action { get; set; }

        public KeyValuePair<K, V> OldKeyValue { get; set; }

        public KeyValuePair<K, V> NewKeyValue { get; set; }

        public bool Cancel { get; set; }
    }
}