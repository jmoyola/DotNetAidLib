using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DotNetAidLib.Core.Network.Info.Imp;

namespace DotNetAidLib.Core.Network.Info.Core
{
    public abstract class NetworkInterfaceInfo
    {
        public NetworkInterfaceInfo(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public abstract NetworkInterfaceType Type { get; }
        public abstract NetworkInterfaceOperationState OperationalState { get; }
        public abstract byte[] Address { get; }
        public abstract byte[] Broadcast { get; }
        public abstract bool Dormant { get; }
        public abstract bool FullDuplex { get; }
        public abstract string Alias { get; }
        public abstract int Index { get; }
        public abstract decimal MTU { get; }
        public abstract long Speed { get; }
        public abstract bool Linked { get; }

        public IPInterfaceInfo IPInterfaceInfo => IPInterfaceInfo.Instance(this);

        public IEEE80211InterfaceInfo IEEE80211Info
        {
            get
            {
                if (Type == NetworkInterfaceType.IEEE80211)
                    return IEEE80211InterfaceInfo.Instance(this);
                return null;
            }
        }

        public abstract void Enable();
        public abstract void Disable();
        public abstract bool IsEnabled();

        public void Restart()
        {
            Disable();
            Enable();
        }

        public static IEnumerable<NetworkInterfaceInfo> GetAllNetworkInterfaces()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotImplementedException();
            return PosixNetworkInterfaceInfo.GetAllNetworkInterfaces();
        }
    }
}