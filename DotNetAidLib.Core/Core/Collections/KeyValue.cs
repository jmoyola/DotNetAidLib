namespace DotNetAidLib.Core.Collections
{
    public class KeyValue<K, V>
    {
        private V m_Value;

        public KeyValue()
        {
        }

        public KeyValue(K key, V value)
        {
            Key = key;
            m_Value = value;
        }

        public K Key { get; set; }

        public V Value
        {
            get => m_Value;
            set => m_Value = value;
        }

        public override bool Equals(object obj)
        {
            if (typeof(KeyValue<K, V>).IsAssignableFrom(obj.GetType()))
            {
                var kv = (KeyValue<K, V>) obj;
                return Key.Equals(kv.Key) && Value.Equals(kv.Value);
            }

            return base.Equals(obj);
        }

        public override string ToString()
        {
            return Key + " - " + m_Value;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}