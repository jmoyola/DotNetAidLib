using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace DotNetAidLib.Configuration.ApplicationConfig.Core{
	public interface IApplicationConfig : IApplicationConfigGroup
	{
        DateTime? LastSavedTime { get; }
        void Load();
		void Save();
		List<Type> KnownTypes{ get;}
	}
}