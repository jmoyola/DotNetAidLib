using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using DotNetAidLib.Core.Network.Config.TcpIp.Core;
using DotNetAidLib.Core.Network.Config.Route.Core;
using DotNetAidLib.Core.Network.Info.Core;
using DotNetAidLib.Core.Network.Client;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Network.Config.Core
{
    public class DebianInterfaceConfigIPAddress : InterfaceConfigIPAddress
    {
        public DebianInterfaceConfigIPAddress(AddressFamily addressFamily)
            :base(addressFamily)
        {}

        public DebianInterfaceConfigIPAddress(AddressFamily addressFamily, ProvisioningType type, DictionaryList<String, String> attributes)
        : base(addressFamily, type, attributes) { }

        public override IPAddress IP
        {
            get {return this.Get("address");}
            set {this.Set("address", value);}
        }

        public override IPAddress NetMask
        {
            get { return this.Get("netmask"); }
            set { this.Set("netmask", value); }

        }

        public override IPAddress Network
        {
            get { return this.Get("network"); }
            set { this.Set("network", value); }

        }

        public override IPAddress Gateway
        {
            get { return this.Get("gateway"); }
            set { this.Set("gateway", value); }

        }

        public override IPAddress Broadcast
        {
            get { return this.Get("broadcast"); }
            set { this.Set("broadcast", value); }
        }

        public override IList<IPAddress> Dns
        {
            get
            {
                String sdns = this.Attributes.Get("dns", null);
                if (String.IsNullOrEmpty(sdns))
                    return null;
                else {
                    return sdns.Split(' ').Select(v => IPAddress.Parse(v)).ToList();
                }
            }
            set
            {
                if (value != null && !value.All(v=>v.AddressFamily.Equals(this.AddressFamily)))
                    throw new NetworkingException("Address family is wrong.");

                if (value == null)
                    this.Attributes.Remove("dns");
                else
                    this.Attributes.Set("dns", value.Select(v => v.ToString()).ToStringJoin(" "), true);
            }
        }

        private IPAddress Get(String key) {
            IPAddress v;
            IPAddress.TryParse(this.Attributes.Get(key, null), out v);
            return v;
        }

        private void Set(String key, IPAddress value)
        {
            if (value != null && !value.AddressFamily.Equals(this.AddressFamily))
                throw new NetworkingException("Address family is wrong.");

            if (value == null)
                this.Attributes.Remove(key);
            else
                this.Attributes.Set(key, value.ToString(), true);
        }
    }
}

