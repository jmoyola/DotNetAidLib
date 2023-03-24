using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Reflection;
using DotNetAidLib.Configuration.ApplicationConfig.Core;

namespace DotNetAidLib.Configuration.ApplicationConfig.Mem{
	public class MemConfig<T> : IConfig<T>
	{
        private DateTime? dateOfCreation;
        private DateTime dateOfModification;
        private Version version;
        private Type type;
        private string key;
        private string info;
        private T value;

        protected internal MemConfig(string key, T value)
		{
			this.Key = key;
			this.Value = value;
		}

		public DateTime DateOfCreation {
			get { return this.dateOfCreation.Value; }
		}

		public DateTime DateOfModification {
			get { return this.dateOfModification; }
		}

		public Version Version {
			get { return this.version; }
		}

		public Type Type {
			get {
				return this.type;
			}
		}

		public string Key {
			get { return this.key; }
			set { this.key = value; }
		}

        public string Info
        {
            get { return this.info; }
            set { this.info = value; }
        }

        public T Value {
			get { return this.value; }
			set {
                this.value = value;
				if (!Equals(value, null))
                    this.type = value.GetType();
				else
                    this.type = typeof(T);
				// Se especifica la fecha de creación (si procede)
				if (!this.dateOfCreation.HasValue)
					this.dateOfCreation = DateTime.Now;

				// Se especifica la fecha de modificación
				this.dateOfModification = DateTime.Now;

				// Se especifica la versión del ensamblado
                this.version=typeof(T).Assembly.GetName().Version;
			}
		}
	}
}