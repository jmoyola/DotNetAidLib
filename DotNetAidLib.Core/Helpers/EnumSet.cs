using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using DotNetAidLib.Core.Develop;
using System.Reflection;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Introspection;

namespace DotNetAidLib.Core.Helpers{
    public class EnumItemAttribute : DescriptionAttribute
    {
        private Type type = null;

        public EnumItemAttribute(String description, Type type=null)
        :base(description)
        {
            Assert.NotNull( type, nameof(type));
            
            this.type = type;
        }
        public Type Type => this.type;
    }

    public abstract class EnumSet<T>:IEnumerable<EnumItem<T>>
    {
        private static IList<EnumSet<T>> instances=new List<EnumSet<T>>() ;
        
        private IDictionary<String, EnumItem<T>> items=new Dictionary<string, EnumItem<T>>();
        
        protected EnumSet()
        {
            this.Refresh();
        }

        public void Add(String name, T value, Type type=null, String description=null)
        {
            Assert.NotNullOrEmpty( name, nameof(name));
            
            items.Add(name, new EnumItem<T>(value, name, type, description));
        }
        
        public T this[String value]
        {
            get
            {
                return this.GetByName(value).Value;
            }
        }

        public bool Exists(String name)
        {
            Assert.NotNullOrEmpty( name, nameof(name));
            
            return this.items.ContainsKey(name);
        }

        public T Get(String name)
        {
            return this.GetByName(name).Value;
        }

        public EnumItem<T> GetByName(String name)
        {
            Assert.NotNullOrEmpty( name, nameof(name));
            
            if (!this.Exists(name))
                throw new KeyNotFoundException("Unknow item with name '" + name + "'.");
            
            return this.items[name].Value;
        }
        
        public IEnumerable<EnumItem<T>> GetByValue(T value)
        {
            return this.items.Where(v=>v.Value.Equals(value)).Select(kv=>kv.Value);
        }

        public IEnumerator<EnumItem<T>> GetEnumerator()
        {
            return items.Values.ToList().AsReadOnly().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString(){
            return items.Values.ToStringJoin(", ");
        }
        
        public void Refresh()
        {
            this.items.Clear();

            var fields = this.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(v => v.IsLiteral && !v.IsInitOnly && v.FieldType.Equals(typeof(T)));
            
            foreach (var field in fields)
            {
                EnumItemAttribute enumAtt = field.GetCustomAttribute<EnumItemAttribute>();
                String description = (enumAtt == null ? null : enumAtt.Description);
                Type type = (enumAtt == null ? null : enumAtt.Type);
                try
                {
                    String xmlSummary = field.GetSummary();
                    if (!String.IsNullOrEmpty(xmlSummary))
                        description = xmlSummary;
                }
                catch { }

                if (description == null)
                    description =field.Name + " " + this.GetType().Name;
                
                this.items.Add(field.Name
                    , new EnumItem<T>((T) field.GetValue(null), field.Name, type, description));
            }
        }

         public static EnumSet<T> Instance<E>() where E:EnumSet<T>
         {
             EnumSet<T> ret = instances.FirstOrDefault(v => v is E);
             if (ret == null) {
                 ret = Activator.CreateInstance<E>();
                 instances.Add(ret);
             }
        
             return ret;
         }
        
    }
}
