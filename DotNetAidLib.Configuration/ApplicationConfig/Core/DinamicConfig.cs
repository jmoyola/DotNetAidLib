using System;

namespace DotNetAidLib.Configuration.ApplicationConfig.Core
{
    public class DinamicConfig<T> : IConfig<T>
    {
        private string m_Info;
        private string m_Key;
        private T m_Value;

        public DinamicConfig(string key, T value)
        {
            DateOfCreation = DateTime.Now;
            m_Key = key;
            m_Value = value;
            Type = typeof(T);
            Version = new Version();
        }

        public DateTime DateOfCreation { get; }

        public DateTime DateOfModification { get; private set; }

        public string Key
        {
            get => m_Key;
            set
            {
                m_Key = value;
                DateOfModification = DateTime.Now;
            }
        }

        public string Info
        {
            get => m_Info;
            set
            {
                m_Info = value;
                DateOfModification = DateTime.Now;
            }
        }

        public Type Type { get; }

        public T Value
        {
            get => m_Value;
            set
            {
                m_Value = value;
                DateOfModification = DateTime.Now;
            }
        }

        public Version Version { get; }
    }
}