using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace DotNetAidLib.Core.Configuration.ApplicationConfig.Core{
	public interface IConfig<T>
	{
		DateTime DateOfCreation { get; }
		DateTime DateOfModification { get; }
		Version Version { get; }
		Type Type { get; }
		string Key { get; set; }
        string Info { get; set; }
        T Value { get; set; }
	}
}