using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System.Net.NetworkInformation;
using System.Text;
using DotNetAidLib.Core.Network.Config.WLan.Client.Core;
using DotNetAidLib.Core.Network.Info.Core;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.Network.Config.WLan.Client.Imp
{
	public class WpaSupplicantClient:WLanClient 
	{
		// 0.- Instalar programa wpa_supplicant:
		//     sudo apt-get install wpa_supplicant
		// 1.- Editar/Crear archivo '/etc/wpa_supplicant/wpa_supplicant.conf' añadiendo:
		//     ctrl_interface=DIR=/run/wpa_supplicant GROUP=adm
		//	   update_config=1
		// 2.- Reiniciar demonio wpa_supplicant (reinicio manual):
		//     sudo wpa_supplicant -B -i<NombreAdaptadorRed> -c/etc/wpa_supplicant/wpa_supplicant.conf

		private FileInfo _WpaSupplicantPath=null;
		private FileInfo _WpaClientPath = null;

        public WpaSupplicantClient (NetworkInterfaceInfo wlanInterface):base(wlanInterface)
		{
			_WpaSupplicantPath = EnvironmentHelper.SearchInPath("wpa_supplicant");
			_WpaClientPath = EnvironmentHelper.SearchInPath("wpa_cli");

			Assert.Exists(_WpaSupplicantPath);
			Assert.Exists(_WpaClientPath);

			Init();
		}

		private void Init() {
			FileInfo wpaSupplicantConfFile = new FileInfo("/etc/wpa_supplicant/wpa_supplicant.conf");
			if (!wpaSupplicantConfFile.Exists) {
				String content = "ctrl_interface=DIR=/run/wpa_supplicant GROUP=adm\nupdate_config=1";
				wpaSupplicantConfFile.WriteText(content, Encoding.Default, true);
			}
		}

		public void StartService()
		{
			_WpaSupplicantPath.CmdExecuteSync(
				                    "-B -i" + this.WlanInterface.Name + " -c /etc/wpa_supplicant/wpa_supplicant.conf"
				                   );
		}

		public override IEnumerable<WLanNetworkScanInfo> ScanList(){
			List<WLanNetworkScanInfo> ret = new List<WLanNetworkScanInfo> ();
			String result;
			Regex regLst = new Regex (@"^(([A-Fa-f0-9]{2}:?){6})\t(\d+)\t(-?\d+)\t(\[[^\s\]]+\])*\t(.*)$",RegexOptions.Multiline);
			result = this.WpaCliCmd ("scan");
			if(!result.Equals ("OK"))
				throw new WLanClientException ("Error request client scan: " + result);
			Thread.Sleep(5000); // Esperamos 5 segundos
			result = this.WpaCliCmd ("scan_result");
			if(result.Equals ("FAIL"))
				throw new WLanClientException ("Error request clietn scan_result: " + result );
			MatchCollection mc = regLst.Matches (result);
			foreach (Match m in mc) {
				

				byte[] aBSSID = m.Groups [1].Value.ToUpper().Replace(":","").HexToByteArray ();
				String sSSID = m.Groups[6].Value;
				int iFrequency=Int32.Parse(m.Groups [3].Value);
				int iSignal=Int32.Parse(m.Groups [4].Value);

				IList<String> lCapabilities = new List<String>();
				foreach (Capture c in m.Groups[5].Captures)
					lCapabilities.Add(c.Value.Replace("[", "").Replace("]", ""));
				
				WLanNetworkScanInfo ci = new WLanNetworkScanInfo(aBSSID, sSSID, iFrequency, iSignal, lCapabilities);
				ret.Add (ci);
			}
			return ret;
		}

		public override IEnumerable<IWLanNetworkInfo> NetworkList(){
			List<IWLanNetworkInfo> ret = new List<IWLanNetworkInfo> ();
			String result;
			Regex regLst = new Regex (@"^(\d+)\t([^\s]+)?\t([^\s]+)\t(\[[^\s\]]+\])+$",RegexOptions.Multiline);
			result = this.WpaCliCmd ("list_networks");
			if(result.Equals ("FAIL"))
				throw new WLanClientException ("Error request network list: " + result);
			MatchCollection mc = regLst.Matches (result);
			foreach (Match m in mc) {
				WpaSupplicantNetworkInfo ci = new WpaSupplicantNetworkInfo (this, Int32.Parse(m.Groups[1].Value));
				
				ret.Add (ci);		
			}

			return ret;
		}

		public override IWLanNetworkInfo AddNetwork(){
			String result;
			result = this.WpaCliCmd ("add_network");
			if (result.Equals ("FAIL"))
				throw new WLanClientException ("Error adding network: " + result);

			return new WpaSupplicantNetworkInfo(this, Int32.Parse (result));
		}

		public override void RemoveNetwork(IWLanNetworkInfo network){
			String result;
			result = this.WpaCliCmd ("remove_network " + network.Index);
			if (!result.Equals ("OK"))
				throw new WLanClientException ("Error removing network '" + network.Index + "': " + result);
		}

		public override void SaveConfig(){
			String result;
			result = this.WpaCliCmd ("save_config");
			if (!result.Equals ("OK"))
				throw new WLanClientException ("Error saving config : " + result);
		}

		public String WpaCliCmd(String cmd){
			


			String ret = null;

			// Necesario para la ejecución en shell de wpasupplicant y las comillas
			cmd = cmd.Replace("\"", "\\\"");
			String arguments = "";
			if(this.WlanInterface!=null)
				arguments="-i" + this.WlanInterface.Name + " " + cmd;
			else
				arguments=cmd;

			ret=_WpaClientPath.CmdExecuteSync(arguments);

			if (ret.EndsWith ("\n"))
				ret = ret.Substring (0,ret.Length-1);
			return ret;
		}
	}
}

