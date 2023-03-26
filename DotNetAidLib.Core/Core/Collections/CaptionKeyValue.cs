namespace DotNetAidLib.Core.Collections
{
    public class CaptionKeyValue<K, V> : KeyValue<K, V>
    {
        public CaptionKeyValue()
        {
        }

        public CaptionKeyValue(K key)
        {
            Key = key;
        }

        public CaptionKeyValue(K key, V value)
        {
            Key = key;
            Value = value;
        }

        public CaptionKeyValue(K key, V value, string caption)
            : this(key, value)
        {
            this.Caption = caption;
        }

        public string Caption { get; set; }
    }
}