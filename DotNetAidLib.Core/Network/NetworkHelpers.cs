using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Net.Sockets;

namespace DotNetAidLib.Core.Network
{
    public static class NetworkHelpers
    {
        public static void CopyTo(this Uri source, FileInfo destination)
        {
            try
            {
                if (source.IsFile)
                {
                    File.Copy(source.AbsolutePath, destination.FullName);
                }
                else
                {
                    HttpClient hc = new HttpClient();

                    using (HttpResponseMessage resp = hc.GetAsync(source).Result)
                    {
                        if (!resp.IsSuccessStatusCode)
                            throw new WebException("Error getting content from source uri '" + source.ToString() + "': " + resp.StatusCode);

                        using (FileStream fs = destination.Create())
                        {
                            resp.Content.CopyToAsync(fs).Wait();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IOException("Error in copy from '" + source.ToString() + "' to '" + destination.ToString() + "'.", ex);
            }
        }

        public static NetworkCredential GetCredentials(this Uri uri, out Uri uriWidthoutCredentials)
        {
            NetworkCredential ret = null;
            uriWidthoutCredentials = uri;

            // Si hay información de usuario en la uri, la establecemos como clientUser y la eliminamos
            if (!String.IsNullOrEmpty(uri.UserInfo))
            {
                UriBuilder ub = new UriBuilder(uri);

                ret = new NetworkCredential(Uri.UnescapeDataString(ub.UserName), Uri.UnescapeDataString(ub.Password));

                // Quitamos los datos de usuario
                ub.UserName = null;
                ub.Password = null;

                // recontruimos la uri sin datos de usuario
                uriWidthoutCredentials = ub.Uri;
            }

            return ret;
        }

        public static Uri GetUriWithCredentials(this Uri uri, NetworkCredential clientUser)
        {
            Uri ret = uri;
            if (clientUser != null)
            {
                UriBuilder ub = new UriBuilder(uri);
                ub.UserName = clientUser.UserName;
                ub.Password = clientUser.Password;
                ret = ub.Uri;
            }
            return ret;
        }

        public static Uri GetUriWithoutCredentials(this Uri uri)
        {
            Uri ret = uri;
            UriBuilder ub = new UriBuilder(uri);
            ub.UserName = null;
            ub.Password = null;
            ret = ub.Uri;
            return ret;
        }

        public static String GetUnscapeString(this Uri uri)
        {
            return Uri.UnescapeDataString(uri.ToString());
        }
        
        public static bool IsRemovableDrive(this Uri v){
            return v.IsFile
                   && DriveInfo.GetDrives().Any(d => v.ToString().StartsWith(d.Name, StringComparison.InvariantCulture)
                                                     && d.DriveType== DriveType.Removable);
        }
        
        public static String ToString(this Uri v, bool showSecrets){
            if (showSecrets)
                return v.ToString();
            else{
                UriBuilder ub = new UriBuilder(v);
                if(!String.IsNullOrEmpty(ub.Password))
                    ub.Password = "****";
                return ub.Uri.ToString();
            }
        }
        
                public enum NetworkAddressClass {
            Public,
            IPv4_A,
            IPv4_B,
            IPv4_C,
            IPv4_D,
            IPv4_E,
            IPv6_Private
        }

        public static IPAddress GetDefaultNetmask(this IPAddress address) {
            IPAddress ret = null;
            switch (address.GetClass()) {
                case NetworkAddressClass.IPv4_A:
                    ret = IPAddress.Parse("255.0.0.0");
                    break;
                case NetworkAddressClass.IPv4_B:
                    ret = IPAddress.Parse("255.255.0.0");
                    break;
                case NetworkAddressClass.IPv4_C:
                    ret = IPAddress.Parse("255.255.255.0");
                    break;
                case NetworkAddressClass.IPv6_Private:
                    ret = new IPAddress(new byte[] { 255,255,255,255,255,255,255,255});
                    break;
            }

            return ret;
        }

        public static NetworkAddressClass GetClass(this IPAddress address)
        {
            NetworkAddressClass ret = NetworkAddressClass.Public;

            byte bAddress = address.GetAddressBytes()[0];
            BitArray ba = new BitArray(new byte[] { bAddress });
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                if (!ba[ba.Length - 1])
                    ret = NetworkAddressClass.IPv4_A;
                else if (ba[ba.Length - 1] && !ba[ba.Length - 2])
                    ret = NetworkAddressClass.IPv4_B;
                else if (ba[ba.Length - 1] && ba[ba.Length - 2] && !ba[ba.Length - 3])
                    ret = NetworkAddressClass.IPv4_C;
                else if (ba[ba.Length - 1] && ba[ba.Length - 2] && ba[ba.Length - 3] && !ba[ba.Length - 4])
                    ret = NetworkAddressClass.IPv4_D;
                else if (ba[ba.Length - 1] && ba[ba.Length - 2] && ba[ba.Length - 3] && ba[ba.Length - 4])
                    ret = NetworkAddressClass.IPv4_E;
            }
            else if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                if (bAddress == 0xfd)
                    ret = NetworkAddressClass.IPv6_Private;
            }

            return ret;
        }
        public static int ToNetworkMaskLength(this IPAddress address){
            int ret = 0;
            BitArray ba = new BitArray(address.GetAddressBytes());
            ret = ba.Cast<bool>().Count(v => v);
            return ret;
        }

        public static IPAddress GetNetworkMask(this IPAddress address, int networkMaskLength) {
            if (networkMaskLength < 2)
                throw new Exception("Network mask must bee greatest than 1.");

            int ipLength = address.GetAddressBytes().Length * 8;

            if (networkMaskLength > ipLength)
                throw new Exception("Network mask can't be greatest address length.");

            BitArray ba = new BitArray(ipLength);

            for (int i = ipLength; i > 0; i--)
                ba[i - 1] = (i > (ipLength - networkMaskLength));
                        
            byte[] aret = new byte[ba.Length / 8];
            ba.CopyTo(aret, 0);
            aret = aret.Reverse().ToArray();
            return new IPAddress(aret);
        }

        public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            
            return new IPAddress(broadcastAddress);
        }

        public static IPAddress GetNetworkAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] networkAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < networkAddress.Length; i++)
                networkAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
            
            return new IPAddress(networkAddress);
        }

        public static bool IsInSameSubnet(this IPAddress address2, IPAddress address, IPAddress subnetMask)
        {
            IPAddress network1 = address.GetNetworkAddress(subnetMask);
            IPAddress network2 = address2.GetNetworkAddress(subnetMask);

            return network1.Equals(network2);
        }

        public static UInt32 ToUInt32(this IPAddress ipAddress)
        {
            var addressBytes = ipAddress.GetAddressBytes();
            Array.Reverse(addressBytes);
            return BitConverter.ToUInt32(addressBytes, 0);
        }

        public static BigInteger ToBigInteger(this IPAddress ipAddress)
        {
            var addressBytes = ipAddress.GetAddressBytes();
            Array.Reverse(addressBytes);
            Array.Resize(ref addressBytes, addressBytes.Length + 1);
            return new BigInteger(addressBytes);
        }

        public static IPAddress ToIPv4Address(this UInt32 value)
        {
            var addressBytes = BitConverter.GetBytes(value);
            Array.Reverse(addressBytes);
            return new IPAddress(addressBytes);
        }

        public static IPAddress ToIPv6Address(ref this BigInteger value)
        {
            var addressBytes = value.ToByteArray();
            Array.Resize(ref addressBytes, 16);
            Array.Reverse(addressBytes);
            return new IPAddress(addressBytes);
        }

        public static IPAddress Increment(this IPAddress value) {
            return value.Increment(1);
        }

        public static IPAddress Increment(this IPAddress value, Int32 increment)
        {
            if (value.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ((UInt32)(value.ToUInt32() + increment)).ToIPv4Address();
            }
            else if (value.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                BigInteger ret= (value.ToBigInteger() + increment);
                return ret.ToIPv6Address();
            }
            else
                return value;
        }

        public static int CompareTo(this IPAddress value, IPAddress ipToCompare)
        {
            if (value == ipToCompare)
                return 0;
            else if (value.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                && ipToCompare.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                Int32 ret=(Int32)((Int64)value.ToUInt32() - (Int64)ipToCompare.ToUInt32());
                return (ret > 0 ? 1 : -1);
            }
            else if (value.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
                && ipToCompare.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                BigInteger ret = value.ToBigInteger() - ipToCompare.ToBigInteger();
                return (ret > 0 ? 1 : -1);
            }
            else
                return -1;

        }
    }
}