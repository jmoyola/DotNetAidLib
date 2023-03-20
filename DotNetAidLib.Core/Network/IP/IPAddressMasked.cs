using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Network.IP
{
    public class IPAddressMasked
    {
        private IPAddress ip;
        private byte[] mask;

        private static Regex ipAddressMaskedRegex = new Regex(@"^(([\:\.]?(\d{1,3}|[0-9a-fA-F]{4})?)+)/(([\:\.]?(\d{1,3}|[0-9a-fA-F]{2}))+)$");

        public IPAddressMasked(String ipMaskedString)
        {
            Match m = ipAddressMaskedRegex.Match(ipMaskedString);

            Assert.When(ipMaskedString, v => m.Success, "Value don't match ipaddres masked: 'ip/mask'", nameof(ipMaskedString));
            this.ip = IPAddress.Parse(m.Groups[1].Value);
            if(m.Groups[4].Captures.Count==1)
                this.CIDRMask = Int32.Parse(m.Groups[4].Value);
            else
                this.Mask = IPAddress.Parse(m.Groups[4].Value).GetAddressBytes();
        }

        public IPAddressMasked(byte[] address, byte[] mask)
        {
            this.ip = new IPAddress(address);
            this.Mask = mask;
        }

        public IPAddressMasked(long newAddress, byte[] mask)
        {
            this.ip = new IPAddress(newAddress);
            this.Mask = mask;
        }

        public IPAddressMasked(byte[] address, long scopeid, byte[] mask) 
        {
            this.ip = new IPAddress(address, scopeid);
            this.Mask = mask;
        }

        public IPAddressMasked(byte[] address, int cidrMask)
        {
            this.ip = new IPAddress(address);
            this.CIDRMask = cidrMask;
        }

        public IPAddressMasked(long newAddress, int cidrMask) 
        {
            this.ip = new IPAddress(newAddress);
            this.CIDRMask = cidrMask;
        }

        public IPAddressMasked(byte[] address, long scopeid, int cidrMask)
        {
            this.ip = new IPAddress(address, scopeid);
            this.CIDRMask = cidrMask;
        }

        public IPAddress IP
        {
            get
            {
                return this.ip;
            }
        }

        public byte[] Mask
        {
            get {
                return mask;
            }
            set
            {
                Assert.NotNullOrEmpty(value, nameof (value));
                Assert.When(value, v => value.Length == this.ip.GetAddressBytes().Length, "Mask length dont match address length.", nameof (value));
                mask = value;
            }
        }

        public int CIDRMask {
            get {
                return new BitArray(this.Mask).Cast<bool>().Count(v=>v);
            }
            set {
                Assert.When(value, (v=>v>0 && v<= this.ip.GetAddressBytes().Length * 8), "No valid length for address.", nameof (value));
                Boolean[] ba = new Boolean[this.ip.GetAddressBytes().Length * 8]
                    .Select((v,i)=>i<value?true:false).ToArray();
                byte[] ret = new byte[this.ip.GetAddressBytes().Length];
                new BitArray(ba).CopyTo(ret,0);
                this.Mask = ret;
            }
        }

        public override string ToString()
        {
            if (this.ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                return this.ip.ToString() + "/" + this.CIDRMask;
            else
                return ip.ToString() + "/" + this.Mask.Select(v => v).ToStringJoin(".");
        }

        public string ToCidrString()
        {
            return this.ip.ToString() + "/" + this.CIDRMask;
        }

        public static IPAddressMasked Parse(String value) {
            return new IPAddressMasked(value);
        }

        public static bool TryParse(String ipMaskedString, out IPAddressMasked address)
        {
            address = null;
            try
            {
                address = Parse(ipMaskedString);
                return true;
            }
            catch {
                return false;
            }

        }

        public override bool Equals(object obj)
        {
            if(obj!=null && typeof(IPAddressMasked).IsAssignableFrom(obj.GetType())) {
                IPAddressMasked o = (IPAddressMasked)obj;
                return this.IP.Equals(o.IP) && this.Mask.EqualsAll((IList<byte>)o.Mask);
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            return this.IP.GetHashCode() & this.Mask.GetHashCode();
        }

        public static implicit operator IPAddress(IPAddressMasked v)
        {
            return v.IP;
        }
    }
}
