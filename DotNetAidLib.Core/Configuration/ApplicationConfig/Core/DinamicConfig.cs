using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace DotNetAidLib.Core.Configuration.ApplicationConfig.Core{
	public class DinamicConfig<T> : IConfig<T>
	{

		private DateTime m_DateOfCreation;
		private DateTime m_DateOfModification;
		private string m_Key;
        private string m_Info;
        private T m_Value;
		private Type m_Type;

		private Version m_Version;
		public DinamicConfig(string key, T value)
		{
			m_DateOfCreation = DateTime.Now;
			m_Key = key;
			m_Value = value;
			m_Type = typeof(T);
			m_Version = new Version();
		}

		public System.DateTime DateOfCreation {
			get { return m_DateOfCreation; }
		}

		public System.DateTime DateOfModification {
			get { return m_DateOfModification; }
		}

		public string Key {
			get { return m_Key; }
			set {
				m_Key = value;
				m_DateOfModification = DateTime.Now;
			}
		}

        public string Info
        {
            get { return m_Info; }
            set
            {
                m_Info = value;
                m_DateOfModification = DateTime.Now;
            }
        }

        public System.Type Type {
			get { return m_Type; }
		}

		public T Value {
			get { return m_Value; }
			set {
				m_Value = value;
				m_DateOfModification = DateTime.Now;
			}
		}

		public System.Version Version {
			get { return m_Version; }
		}
	}
}