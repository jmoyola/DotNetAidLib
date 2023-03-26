using System.Collections.Generic;
using DotNetAidLib.Core.Network.Info.Core;

namespace DotNetAidLib.Network.Config.WLan.Client.Core
{
    public interface IWLanClient
    {
        NetworkInterfaceInfo WlanInterface { get; }
        IEnumerable<WLanNetworkScanInfo> ScanList();
        IEnumerable<IWLanNetworkInfo> NetworkList();
        IWLanNetworkInfo AddNetwork();
        void RemoveNetwork(IWLanNetworkInfo network);
        void SaveConfig();
    }
}