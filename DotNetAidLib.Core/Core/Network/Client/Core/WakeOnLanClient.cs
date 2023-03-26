using System.IO;
using System.Net;
using System.Net.Sockets;
using DotNetAidLib.Core.Streams;

namespace DotNetAidLib.Core.Network.Client.Core
{
    public class WakeOnLanClient
    {
        public static void WakeUp(byte[] deviceMAC)
        {
            WakeUp(deviceMAC, null, null);
        }

        public static void WakeUp(byte[] deviceMAC, IPAddress host)
        {
            WakeUp(deviceMAC, host, null);
        }

        public static void WakeUp(byte[] deviceMAC, IPAddress host, byte[] password)
        {
            if (host == null)
                host = IPAddress.Broadcast;

            var udpClient = new UdpClient();
            var msMagicPacket = new MemoryStream();

            // MagicPacketPreamble
            for (var n = 0; n < 6; n++)
                msMagicPacket.WriteByte(0xFF);
            for (var n = 0; n < 16; n++)
                msMagicPacket.Write(deviceMAC, deviceMAC.Length);

            if (password != null)
                msMagicPacket.Write(password, password.Length);

            var magicPacket = msMagicPacket.ToArray();
            udpClient.Send(magicPacket, magicPacket.Length, new IPEndPoint(host, 9));
        }
    }
}