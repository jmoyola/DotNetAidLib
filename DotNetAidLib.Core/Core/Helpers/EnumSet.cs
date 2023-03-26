using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Introspection;

namespace DotNetAidLib.Core.Helpers
{
    public class EnumItemAttribute : DescriptionAttribute
    {
        public EnumItemAttribute(string description, Type type = null)
            : base(description)
        {
            Assert.NotNull(type, nameof(type));

            Type = type;
        }

        public Type Type { get; }
    }

    public abstract class EnumSet<T> : IEnumerable<EnumItem<T>>
    {
        private static readonly IList<EnumSet<T>> instances = new List<EnumSet<T>>();

        private readonly IDictionary<string, EnumItem<T>> items = new Dictionary<string, EnumItem<T>>();

        protected EnumSet()
        {
            Refresh();
        }

        public T this[string value] => GetByName(value).Value;

        public IEnumerator<EnumItem<T>> GetEnumerator()
        {
            return items.Values.ToList().AsReadOnly().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(string name, T value, Type type = null, string description = null)
        {
            Assert.NotNullOrEmpty(name, nameof(name));

            items.Add(name, new EnumItem<T>(value, name, type, description));
        }

        public bool Exists(string name)
        {
            Assert.NotNullOrEmpty(name, nameof(name));

            return items.ContainsKey(name);
        }

        public T Get(string name)
        {
            return GetByName(name).Value;
        }

        public EnumItem<T> GetByName(string name)
        {
            Assert.NotNullOrEmpty(name, nameof(name));

            if (!Exists(name))
                throw new KeyNotFoundException("Unknow item with name '" + name + "'.");

            return items[name].Value;
        }

        public IEnumerable<EnumItem<T>> GetByValue(T value)
        {
            return items.Where(v => v.Value.Equals(value)).Select(kv => kv.Value);
        }

        public override string ToString()
        {
            return items.Values.ToStringJoin(", ");
        }

        public void Refresh()
        {
            items.Clear();

            var fields = GetType()
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(v => v.IsLiteral && !v.IsInitOnly && v.FieldType.Equals(typeof(T)));

            foreach (var field in fields)
            {
                var enumAtt = field.GetCustomAttribute<EnumItemAttribute>();
                var description = enumAtt == null ? null : enumAtt.Description;
                var type = enumAtt == null ? null : enumAtt.Type;
                try
                {
                    var xmlSummary = field.GetSummary();
                    if (!string.IsNullOrEmpty(xmlSummary))
                        description = xmlSummary;
                }
                catch
                {
                }

                if (description == null)
                    description = field.Name + " " + GetType().Name;

                items.Add(field.Name
                    , new EnumItem<T>((T) field.GetValue(null), field.Name, type, description));
            }
        }

        public static EnumSet<T> Instance<E>() where E : EnumSet<T>
        {
            var ret = instances.FirstOrDefault(v => v is E);
            if (ret == null)
            {
                ret = Activator.CreateInstance<E>();
                instances.Add(ret);
            }

            return ret;
        }
    }
}