using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Network.Info.Imp;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;


namespace DotNetAidLib.Core.Network.Info.Core
{
    public abstract class NetworkInterfaceInfo
    {
        private String name;
        public NetworkInterfaceInfo(String name){
            this.name = name;
        }

        public String Name {
            get { return this.name; }
        }

        public abstract NetworkInterfaceType Type { get; }
        public abstract NetworkInterfaceOperationState OperationalState { get; }
        public abstract byte[] Address { get; }
        public abstract byte[] Broadcast { get; }
        public abstract bool Dormant { get; }
        public abstract bool FullDuplex { get; }
        public abstract String Alias { get; }
        public abstract int Index { get; }
        public abstract decimal MTU { get; }
        public abstract long Speed { get; }
        public abstract bool Linked { get; }
        public abstract void Enable();
        public abstract void Disable();
        public abstract bool IsEnabled();
        public void Restart()
        {
            this.Disable();
            this.Enable();
        }

        public IPInterfaceInfo IPInterfaceInfo {
            get {
                return IPInterfaceInfo.Instance(this);
            }
        }

        public IEEE80211InterfaceInfo IEEE80211Info{
            get{
                if (this.Type == NetworkInterfaceType.IEEE80211)
                    return IEEE80211InterfaceInfo.Instance(this);
                else
                    return null;
            }
        }

        public static IEnumerable<NetworkInterfaceInfo> GetAllNetworkInterfaces(){
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotImplementedException();
            else
                return PosixNetworkInterfaceInfo.GetAllNetworkInterfaces();
        }

    }
}
