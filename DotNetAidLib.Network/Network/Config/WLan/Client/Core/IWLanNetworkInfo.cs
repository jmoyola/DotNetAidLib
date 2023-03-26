namespace DotNetAidLib.Network.Config.WLan.Client.Core
{
    public interface IWLanNetworkInfo
    {
        int Index { get; }
        byte Priority { get; set; }
        byte[] BSSID { get; set; }
        string SSID { get; set; }
        bool Enabled { get; set; }
        bool Connected { get; }
        object GetProperty(string key);
        void SetProperty(string key, object value);
        void Enable();
        void Disable();
        void Select();
    }
}