using System;
using System.Collections;
using System.Collections.Generic;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Collections {
    public class TypedValue {
        private Type type;
        private Object value;

        public TypedValue (Object value) {
            this.Value = value;
        }

        public Type Type {
            get => type;
        }

        public object Value {
            get => value;
            set {
                Assert.NotNull( value, nameof(value));
                this.type = value.GetType ();
                this.value = value;
            }
        }

        public T Get<T> () {
            return (T)Convert.ChangeType (this.value, typeof (T));
        }

        public bool Is<T> () {
            return this.type.Equals (nameof (T));
        }
    }

    public class NameTypedDictionary : IEnumerable<KeyValuePair<String, TypedValue>> {
        private IDictionary<String, TypedValue> content = null;
        public NameTypedDictionary () {
            this.content = new Dictionary<string, TypedValue> ();
        }

        public Object this [String key]{
            get{
                return this.Get<Object>(key, null);
            }
            set{
                this.Add (key, value, true);
            }
        }

        public void Add<T>(String key, T o, bool updateIfExists=false) {
            if (this.ContainsKey<T>(key))
                this.content[key] = new TypedValue(o);
            else
                this.content.Add(key, new TypedValue(o));
        }

        public T Get<T>(String key){
            return (T)this.content[key].Value;
        }

        public T Get<T>(String key, T valueIfNotExists){
            if (!this.ContainsKey<T>(key))
                return valueIfNotExists;
            else
                return (T)this.content[key].Value;
        }

        public void Remove(String key){
            this.content.Remove(key);
        }

        public Type Typeof(String key){
            return this.content[key].Type;
        }

        public bool ContainsKey<T>(String key){
            return this.content.ContainsKey(key) && this.content[key].Type.Equals(typeof(T));
        }

        public bool ContainsKey(String key)
        {
            return this.content.ContainsKey (key);
        }

        public int Count {
            get { return this.content.Count; }
        }

        public IEnumerator<KeyValuePair<String, TypedValue>> GetEnumerator(){
            return this.content.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.content.GetEnumerator();
        }


    }
}