using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Core.Context{
	public class Context
	{

		private ObservableDictionary<string, object> m_Attributes = new ObservableDictionary<string, object>();
        private String name;
        private DateTime creationTime;
        private TimeSpan expirationTime;

        internal Context (String name)
        {
            this.name = name;
            this.creationTime = DateTime.Now;
            this.expirationTime = new TimeSpan (0);
        }

        public String Name => this.name;
        public DateTime CreationTime => this.creationTime;
        public TimeSpan ExpirationTime {
            get { return this.expirationTime; }
            set {
                if(value.Ticks > 0 && this.name.StartsWith("_GLOBAL", StringComparison.InvariantCulture))
                    throw new ContextException ("Global contexts can't expire.");
                this.expirationTime = value;
            }
        } 

        public void Renove (TimeSpan time) {
            this.expirationTime = this.expirationTime.Add (time);
        }

        public ObservableDictionary<string, object> Attributes {
			get { return m_Attributes; }
		}

		public object this[string key] {
			get {
				if (m_Attributes.ContainsKey(key)) {
					return m_Attributes[key];
				} else {
					return null;
				}
			}
			set {
                if (value == null) {
                    if (m_Attributes.ContainsKey (key))
                        m_Attributes.Remove (key);
                } else {
                    if (m_Attributes.ContainsKey (key)) {
                        m_Attributes [key] = value;
                    } else {
                        m_Attributes.Add (key, value);
                    }
                }
			}
		}
	}
}