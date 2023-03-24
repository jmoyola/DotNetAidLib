using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Library.AAA.Core;
using Library.AAA.Imp;

namespace Library.OperatingSystem.Core
{
    public enum OSRunLevel{
		PowerOff=0,
        Rescue=1,
        MultiUser = 3,
        Graphical=5,
		Reboot=6,
	}
}

