using System;
using DotNetAidLib.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Configuration.ApplicationConfig.Mem
{
    public class MemConfig<T> : IConfig<T>
    {
        private DateTime? dateOfCreation;
        private T value;

        protected internal MemConfig(string key, T value)
        {
            Key = key;
            Value = value;
        }

        public DateTime DateOfCreation => dateOfCreation.Value;

        public DateTime DateOfModification { get; private set; }

        public Version Version { get; private set; }

        public Type Type { get; private set; }

        public string Key { get; set; }

        public string Info { get; set; }

        public T Value
        {
            get => value;
            set
            {
                this.value = value;
                if (!Equals(value, null))
                    Type = value.GetType();
                else
                    Type = typeof(T);
                // Se especifica la fecha de creación (si procede)
                if (!dateOfCreation.HasValue)
                    dateOfCreation = DateTime.Now;

                // Se especifica la fecha de modificación
                DateOfModification = DateTime.Now;

                // Se especifica la versión del ensamblado
                Version = typeof(T).Assembly.GetName().Version;
            }
        }
    }
}