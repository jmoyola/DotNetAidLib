using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using DotNetAidLib.Core.Network.Config.Route.Core;
using DotNetAidLib.Core.Network.Info.Core;
using DotNetAidLib.Core.Network.Client;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Network.Config.Core
{
    public enum ProvisioningType
    {
        Dhcp,
        Static,
        Manual,
        Loopback
    }

    public class InterfaceConfigAddress
    {
        private bool _enabled;
        private bool _hotPlug;
        private AddressFamily addressFamily;
        private ProvisioningType provisioningType;
        private DictionaryList<String, String> attributes = null;

        public InterfaceConfigAddress(AddressFamily addressFamily)
            :this(addressFamily, ProvisioningType.Loopback, new DictionaryList<String, String>()) {}

        public InterfaceConfigAddress(AddressFamily addressFamily, ProvisioningType provisioningType, DictionaryList<String, String> attributes)
        {
            Assert.NotNullOrEmpty( attributes, nameof(attributes));

            this.addressFamily = addressFamily;
            this.provisioningType = provisioningType;
            this.attributes = attributes;
        }

        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
            }
        }

        public bool HotPlug
        {
            get
            {
                return _hotPlug;
            }
            set
            {
                _hotPlug = value;
            }
        }

        public AddressFamily AddressFamily
        {
            get { return addressFamily; }
        }


        public DictionaryList<String, String> Attributes
        {
            get
            {
                return this.attributes;
            }
        }

        public ProvisioningType ProvisioningType
        {
            get
            {
                return provisioningType;
            }

            set
            {
                provisioningType = value;
            }
        }
    }
}

