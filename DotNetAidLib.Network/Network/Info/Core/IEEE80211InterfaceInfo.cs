using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Network.Info.Imp;

namespace DotNetAidLib.Core.Network.Info.Core
{
    public abstract class IEEE80211InterfaceInfo
    {
        private static readonly int[] QuadraticPercentOfSignal =
        {
            100, 100, 100, 100, 100, 100, 100, 100, 100, 100,
            100, 100, 100, 100, 100, 100, 100, 100, 100, 100,
            100, 99, 99, 99, 98, 98, 98, 97, 97, 96,
            96, 95, 95, 94, 93, 93, 92, 91, 90, 90,
            89, 88, 87, 86, 85, 84, 83, 82, 81, 80,
            79, 78, 76, 75, 74, 73, 71, 70, 69, 67,
            66, 64, 63, 61, 60, 58, 56, 55, 53, 51,
            50, 48, 46, 44, 42, 40, 38, 36, 34, 32,
            30, 28, 26, 24, 22, 20, 17, 15, 13, 10,
            8, 6, 3, 1, 1, 1, 1, 1, 1, 1, 1
        };

        public IEEE80211InterfaceInfo(NetworkInterfaceInfo networkInterface)
        {
            Assert.When(() => networkInterface.Type == NetworkInterfaceType.IEEE80211,
                "Only wireless interfaces are allowed.");
            NetworkInterface = networkInterface;
        }

        public NetworkInterfaceInfo NetworkInterface { get; }

        public abstract string IEE802X { get; }
        public abstract string ESSID { get; }
        public abstract string Mode { get; }
        public abstract long Frequency { get; }
        public abstract byte[] AccessPoint { get; }
        public abstract long BitRate { get; }
        public abstract int TxPowerDb { get; }
        public abstract int SignalLevelPercent { get; }
        public abstract int LinkQualityPercent { get; }
        public abstract int NoiseLevelPercent { get; }

        public abstract void Refresh();

        public override string ToString()
        {
            return "WLan interface (" + NetworkInterface.Name + "): "
                   + " IEE802X: " + IEE802X
                   + "; ESSID: " + ESSID
                   + "; Mode: " + Mode
                   + "; Frequency: " + Frequency
                   + "; AccessPoint: " + "" + AccessPoint.ToHexadecimal(":")
                   + "; BitRate: " + BitRate
                   + "; TxPowerDb: " + TxPowerDb
                   + "; SignalLevelPercent: " + SignalLevelPercent
                   + "; LinkQualityPercent: " + LinkQualityPercent
                   + "; NoiseLevelPercent: " + NoiseLevelPercent
                ;
        }

        public static int SignalPercentOfdB(int signaldB)
        {
            Assert.BetweenOrEqual(signaldB, -100, 0, nameof(signaldB));
            return QuadraticPercentOfSignal[signaldB * -1];
        }

        public static int SignaldBOfPercent(int signalPercent)
        {
            Assert.BetweenOrEqual(signalPercent, 0, 100, nameof(signalPercent));
            int i;
            for (i = 0; i <= 100; i++)
                if (QuadraticPercentOfSignal[i] < signalPercent)
                    break;
            return i * -1;
        }

        public static int WLanValueToPercent(string value)
        {
            var ret = 0;
            Regex r;
            Match m;

            // Proporcion
            value = value.Trim();
            r = new Regex(@"(\d+)\s?/s?(\d+)");
            m = r.Match(value);
            if (m.Success)
            {
                var a = int.Parse(m.Groups[1].Value);
                var b = int.Parse(m.Groups[2].Value);
                return a * 100 / b;
            }

            // decibelios
            r = new Regex(@"(-?\d+)\s?dbm?", RegexOptions.IgnoreCase);
            m = r.Match(value);
            if (m.Success)
            {
                var a = int.Parse(m.Groups[1].Value);
                return SignalPercentOfdB(a);
            }

            // valor
            r = new Regex(@"(\d+)");
            m = r.Match(value);
            if (m.Success)
            {
                var a = int.Parse(m.Groups[1].Value);
                return a;
            }

            return ret;
        }

        public static IEEE80211InterfaceInfo Instance(NetworkInterfaceInfo networkInterface)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotImplementedException();
            return new PosixIEEE80211InterfaceInfo(networkInterface);
        }
    }
}