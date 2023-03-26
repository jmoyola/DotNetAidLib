using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Network.Info.Core;
using DotNetAidLib.Network.Config.WLan.Client.Core;

namespace DotNetAidLib.Network.Config.WLan.Client.Imp
{
    public class WpaSupplicantClient : WLanClient
    {
        private readonly FileInfo _WpaClientPath;
        // 0.- Instalar programa wpa_supplicant:
        //     sudo apt-get install wpa_supplicant
        // 1.- Editar/Crear archivo '/etc/wpa_supplicant/wpa_supplicant.conf' añadiendo:
        //     ctrl_interface=DIR=/run/wpa_supplicant GROUP=adm
        //	   update_config=1
        // 2.- Reiniciar demonio wpa_supplicant (reinicio manual):
        //     sudo wpa_supplicant -B -i<NombreAdaptadorRed> -c/etc/wpa_supplicant/wpa_supplicant.conf

        private readonly FileInfo _WpaSupplicantPath;

        public WpaSupplicantClient(NetworkInterfaceInfo wlanInterface) : base(wlanInterface)
        {
            _WpaSupplicantPath = EnvironmentHelper.SearchInPath("wpa_supplicant");
            _WpaClientPath = EnvironmentHelper.SearchInPath("wpa_cli");

            Assert.Exists(_WpaSupplicantPath);
            Assert.Exists(_WpaClientPath);

            Init();
        }

        private void Init()
        {
            var wpaSupplicantConfFile = new FileInfo("/etc/wpa_supplicant/wpa_supplicant.conf");
            if (!wpaSupplicantConfFile.Exists)
            {
                var content = "ctrl_interface=DIR=/run/wpa_supplicant GROUP=adm\nupdate_config=1";
                wpaSupplicantConfFile.WriteText(content, Encoding.Default, true);
            }
        }

        public void StartService()
        {
            _WpaSupplicantPath.CmdExecuteSync(
                "-B -i" + WlanInterface.Name + " -c /etc/wpa_supplicant/wpa_supplicant.conf"
            );
        }

        public override IEnumerable<WLanNetworkScanInfo> ScanList()
        {
            var ret = new List<WLanNetworkScanInfo>();
            string result;
            var regLst = new Regex(@"^(([A-Fa-f0-9]{2}:?){6})\t(\d+)\t(-?\d+)\t(\[[^\s\]]+\])*\t(.*)$",
                RegexOptions.Multiline);
            result = WpaCliCmd("scan");
            if (!result.Equals("OK"))
                throw new WLanClientException("Error request client scan: " + result);
            Thread.Sleep(5000); // Esperamos 5 segundos
            result = WpaCliCmd("scan_result");
            if (result.Equals("FAIL"))
                throw new WLanClientException("Error request clietn scan_result: " + result);
            var mc = regLst.Matches(result);
            foreach (Match m in mc)
            {
                var aBSSID = m.Groups[1].Value.ToUpper().Replace(":", "").HexToByteArray();
                var sSSID = m.Groups[6].Value;
                var iFrequency = int.Parse(m.Groups[3].Value);
                var iSignal = int.Parse(m.Groups[4].Value);

                IList<string> lCapabilities = new List<string>();
                foreach (Capture c in m.Groups[5].Captures)
                    lCapabilities.Add(c.Value.Replace("[", "").Replace("]", ""));

                var ci = new WLanNetworkScanInfo(aBSSID, sSSID, iFrequency, iSignal, lCapabilities);
                ret.Add(ci);
            }

            return ret;
        }

        public override IEnumerable<IWLanNetworkInfo> NetworkList()
        {
            var ret = new List<IWLanNetworkInfo>();
            string result;
            var regLst = new Regex(@"^(\d+)\t([^\s]+)?\t([^\s]+)\t(\[[^\s\]]+\])+$", RegexOptions.Multiline);
            result = WpaCliCmd("list_networks");
            if (result.Equals("FAIL"))
                throw new WLanClientException("Error request network list: " + result);
            var mc = regLst.Matches(result);
            foreach (Match m in mc)
            {
                var ci = new WpaSupplicantNetworkInfo(this, int.Parse(m.Groups[1].Value));

                ret.Add(ci);
            }

            return ret;
        }

        public override IWLanNetworkInfo AddNetwork()
        {
            string result;
            result = WpaCliCmd("add_network");
            if (result.Equals("FAIL"))
                throw new WLanClientException("Error adding network: " + result);

            return new WpaSupplicantNetworkInfo(this, int.Parse(result));
        }

        public override void RemoveNetwork(IWLanNetworkInfo network)
        {
            string result;
            result = WpaCliCmd("remove_network " + network.Index);
            if (!result.Equals("OK"))
                throw new WLanClientException("Error removing network '" + network.Index + "': " + result);
        }

        public override void SaveConfig()
        {
            string result;
            result = WpaCliCmd("save_config");
            if (!result.Equals("OK"))
                throw new WLanClientException("Error saving config : " + result);
        }

        public string WpaCliCmd(string cmd)
        {
            string ret = null;

            // Necesario para la ejecución en shell de wpasupplicant y las comillas
            cmd = cmd.Replace("\"", "\\\"");
            var arguments = "";
            if (WlanInterface != null)
                arguments = "-i" + WlanInterface.Name + " " + cmd;
            else
                arguments = cmd;

            ret = _WpaClientPath.CmdExecuteSync(arguments);

            if (ret.EndsWith("\n"))
                ret = ret.Substring(0, ret.Length - 1);
            return ret;
        }
    }
}