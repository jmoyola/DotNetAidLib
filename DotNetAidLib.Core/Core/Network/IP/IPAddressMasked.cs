using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Network.IP
{
    public class IPAddressMasked
    {
        private static readonly Regex ipAddressMaskedRegex =
            new Regex(@"^(([\:\.]?(\d{1,3}|[0-9a-fA-F]{4})?)+)/(([\:\.]?(\d{1,3}|[0-9a-fA-F]{2}))+)$");

        private byte[] mask;

        public IPAddressMasked(string ipMaskedString)
        {
            var m = ipAddressMaskedRegex.Match(ipMaskedString);

            Assert.When(ipMaskedString, v => m.Success, "Value don't match ipaddres masked: 'ip/mask'",
                nameof(ipMaskedString));
            IP = IPAddress.Parse(m.Groups[1].Value);
            if (m.Groups[4].Captures.Count == 1)
                CIDRMask = int.Parse(m.Groups[4].Value);
            else
                Mask = IPAddress.Parse(m.Groups[4].Value).GetAddressBytes();
        }

        public IPAddressMasked(byte[] address, byte[] mask)
        {
            IP = new IPAddress(address);
            Mask = mask;
        }

        public IPAddressMasked(long newAddress, byte[] mask)
        {
            IP = new IPAddress(newAddress);
            Mask = mask;
        }

        public IPAddressMasked(byte[] address, long scopeid, byte[] mask)
        {
            IP = new IPAddress(address, scopeid);
            Mask = mask;
        }

        public IPAddressMasked(byte[] address, int cidrMask)
        {
            IP = new IPAddress(address);
            CIDRMask = cidrMask;
        }

        public IPAddressMasked(long newAddress, int cidrMask)
        {
            IP = new IPAddress(newAddress);
            CIDRMask = cidrMask;
        }

        public IPAddressMasked(byte[] address, long scopeid, int cidrMask)
        {
            IP = new IPAddress(address, scopeid);
            CIDRMask = cidrMask;
        }

        public IPAddress IP { get; }

        public byte[] Mask
        {
            get => mask;
            set
            {
                Assert.NotNullOrEmpty(value, nameof(value));
                Assert.When(value, v => value.Length == IP.GetAddressBytes().Length,
                    "Mask length dont match address length.", nameof(value));
                mask = value;
            }
        }

        public int CIDRMask
        {
            get { return new BitArray(Mask).Cast<bool>().Count(v => v); }
            set
            {
                Assert.When(value, v => v > 0 && v <= IP.GetAddressBytes().Length * 8, "No valid length for address.",
                    nameof(value));
                var ba = new bool[IP.GetAddressBytes().Length * 8]
                    .Select((v, i) => i < value ? true : false).ToArray();
                var ret = new byte[IP.GetAddressBytes().Length];
                new BitArray(ba).CopyTo(ret, 0);
                Mask = ret;
            }
        }

        public override string ToString()
        {
            if (IP.AddressFamily == AddressFamily.InterNetworkV6)
                return IP + "/" + CIDRMask;
            return IP + "/" + Mask.Select(v => v).ToStringJoin(".");
        }

        public string ToCidrString()
        {
            return IP + "/" + CIDRMask;
        }

        public static IPAddressMasked Parse(string value)
        {
            return new IPAddressMasked(value);
        }

        public static bool TryParse(string ipMaskedString, out IPAddressMasked address)
        {
            address = null;
            try
            {
                address = Parse(ipMaskedString);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj != null && typeof(IPAddressMasked).IsAssignableFrom(obj.GetType()))
            {
                var o = (IPAddressMasked) obj;
                return IP.Equals(o.IP) && Mask.EqualsAll(o.Mask);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return IP.GetHashCode() & Mask.GetHashCode();
        }

        public static implicit operator IPAddress(IPAddressMasked v)
        {
            return v.IP;
        }
    }
}