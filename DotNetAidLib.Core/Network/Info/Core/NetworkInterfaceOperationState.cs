using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Network.Config.TcpIp.Imp;

namespace DotNetAidLib.Core.Network.Info.Core
{
    public enum NetworkInterfaceOperationState{
        UNKNOWN,
        NOTPRESENT,
        DOWN,
        LOWERLAYERDOWN,
        TESTING,
        DORMANT,
        UP
    }
}
