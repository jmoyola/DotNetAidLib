using System;
using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Core.Context
{
    public class Context
    {
        private TimeSpan expirationTime;

        internal Context(string name)
        {
            Name = name;
            CreationTime = DateTime.Now;
            expirationTime = new TimeSpan(0);
        }

        public string Name { get; }

        public DateTime CreationTime { get; }

        public TimeSpan ExpirationTime
        {
            get => expirationTime;
            set
            {
                if (value.Ticks > 0 && Name.StartsWith("_GLOBAL", StringComparison.InvariantCulture))
                    throw new ContextException("Global contexts can't expire.");
                expirationTime = value;
            }
        }

        public ObservableDictionary<string, object> Attributes { get; } = new ObservableDictionary<string, object>();

        public object this[string key]
        {
            get
            {
                if (Attributes.ContainsKey(key))
                    return Attributes[key];
                return null;
            }
            set
            {
                if (value == null)
                {
                    if (Attributes.ContainsKey(key))
                        Attributes.Remove(key);
                }
                else
                {
                    if (Attributes.ContainsKey(key))
                        Attributes[key] = value;
                    else
                        Attributes.Add(key, value);
                }
            }
        }

        public void Renove(TimeSpan time)
        {
            expirationTime = expirationTime.Add(time);
        }
    }
}