using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Network.Config.Core;
using DotNetAidLib.Network.Config.TcpIp.Core;

namespace DotNetAidLib.Network.Config.Imp
{
    public class DebianInterfaceConfigIPAddress : InterfaceConfigIPAddress
    {
        public DebianInterfaceConfigIPAddress(AddressFamily addressFamily)
            : base(addressFamily)
        {
        }

        public DebianInterfaceConfigIPAddress(AddressFamily addressFamily, ProvisioningType type,
            DictionaryList<string, string> attributes)
            : base(addressFamily, type, attributes)
        {
        }

        public override IPAddress IP
        {
            get => Get("address");
            set => Set("address", value);
        }

        public override IPAddress NetMask
        {
            get => Get("netmask");
            set => Set("netmask", value);
        }

        public override IPAddress Network
        {
            get => Get("network");
            set => Set("network", value);
        }

        public override IPAddress Gateway
        {
            get => Get("gateway");
            set => Set("gateway", value);
        }

        public override IPAddress Broadcast
        {
            get => Get("broadcast");
            set => Set("broadcast", value);
        }

        public override IList<IPAddress> Dns
        {
            get
            {
                var sdns = Attributes.Get("dns", null);
                if (string.IsNullOrEmpty(sdns))
                    return null;
                return sdns.Split(' ').Select(v => IPAddress.Parse(v)).ToList();
            }
            set
            {
                if (value != null && !value.All(v => v.AddressFamily.Equals(AddressFamily)))
                    throw new NetworkingException("Address family is wrong.");

                if (value == null)
                    Attributes.Remove("dns");
                else
                    Attributes.Set("dns", value.Select(v => v.ToString()).ToStringJoin(" "), true);
            }
        }

        private IPAddress Get(string key)
        {
            IPAddress v;
            IPAddress.TryParse(Attributes.Get(key, null), out v);
            return v;
        }

        private void Set(string key, IPAddress value)
        {
            if (value != null && !value.AddressFamily.Equals(AddressFamily))
                throw new NetworkingException("Address family is wrong.");

            if (value == null)
                Attributes.Remove(key);
            else
                Attributes.Set(key, value.ToString(), true);
        }
    }
}