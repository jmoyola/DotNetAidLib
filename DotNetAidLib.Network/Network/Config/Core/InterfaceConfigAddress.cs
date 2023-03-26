using System.Net.Sockets;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Network.Config.Core
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
        public InterfaceConfigAddress(AddressFamily addressFamily)
            : this(addressFamily, ProvisioningType.Loopback, new DictionaryList<string, string>())
        {
        }

        public InterfaceConfigAddress(AddressFamily addressFamily, ProvisioningType provisioningType,
            DictionaryList<string, string> attributes)
        {
            Assert.NotNullOrEmpty(attributes, nameof(attributes));

            AddressFamily = addressFamily;
            ProvisioningType = provisioningType;
            Attributes = attributes;
        }

        public bool Enabled { get; set; }

        public bool HotPlug { get; set; }

        public AddressFamily AddressFamily { get; }


        public DictionaryList<string, string> Attributes { get; }

        public ProvisioningType ProvisioningType { get; set; }
    }
}