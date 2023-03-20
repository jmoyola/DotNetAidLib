using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace DotNetAidLib.Core.Context{
	public interface IContextSupport
	{
		Context Context { get; }
		void DisposeContext();
	}
}