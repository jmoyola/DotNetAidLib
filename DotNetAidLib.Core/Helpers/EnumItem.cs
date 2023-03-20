using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Helpers{
    public class EnumItem<T>
    {
        private readonly Type type;
        private readonly T value;
        private readonly String name;
        private readonly String description;
        
        public EnumItem(T value, String name, Type type=null,  String description=null) {
            Assert.NotNullOrEmpty( name, nameof(name));

            this.type = type;
            this.value = value;
            this.name = name;
            this.description = description;
        }

        public Type Type => type;
        public T Value => value;
        public string Name => name;
        public string Description => description;
        
        public override string ToString(){
            return this.name + (this.type==null?"":"(" + this.type.Name + ")") + ": " + this.value + (this.description==null?"":" (" + this.description + ")");
        }

        public override bool Equals(object obj){
            if (obj != null){
                if (obj is T)
                    return Equals(this.value,(T)obj);
                else if (this.GetType().IsAssignableFrom(obj.GetType()))
                    return Equals(this.value, ((EnumItem<T>)obj).value);
                else
                    return false;
            }
            return false;
        }

        public static bool operator ==(EnumItem<T> a, EnumItem<T> b) {
            if (a is null || b is null)
                return false;
            else
                return a.value.Equals(b.value);
        }

        public static bool operator !=(EnumItem<T> a, EnumItem<T> b){
            if (a is null || b is null)
                return false;
            else
                return !a.value.Equals(b.value);
        }

        public static bool operator ==(T a, EnumItem<T> b)
        {
            if (b is null)
                return false;
            else
                return a.Equals(b.value);
        }

        public static bool operator !=(T a, EnumItem<T> b)
        {
            if (b is null)
                return false;
            else
                return !a.Equals(b.value);
        }

        public override int GetHashCode(){
            return this.value.GetHashCode();
        }

        public static implicit operator EnumItem<T>(T v)
        {
            String vs = v.ToString();
            return new EnumItem<T>(v, "value_" + vs, null, "description_" + vs);
        }

        public static implicit operator T (EnumItem<T> v){
            return v.value;
        }

        public static bool Equals(EnumItem<T> a, EnumItem<T> b)
        {
            if (a is null && b is null)
                return false;
            else if (!(a is null))
                return a.Equals(b);
            else if (!(b is null))
                return b.Equals(a);
            else
                return a.Equals(b);
        }
        
        public static bool Equals(T a, T b)
        {
            if (a is null && b is null)
                return true;
            else if (!(a is null))
                return a.Equals(b);
            else if (!(b is null))
                return b.Equals(a);
            else
                return a.Equals(b);
        }
    }
}
