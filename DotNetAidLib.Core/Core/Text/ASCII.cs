using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DotNetAidLib.Core.Text
{
    public class ASCII
    {
        public enum Control
        {
            NULL = 0,
            SOH = 1,
            STX = 2,
            ETX = 3,
            EOT = 4,
            ENQ = 5,
            ACK = 6,
            BEL = 7,
            BS = 8,
            TAB = 9,
            LF = 10,
            VT = 11,
            FF = 12,
            CR = 13,
            SO = 14,
            SI = 15,
            DLE = 16,
            DC1 = 17,
            DC2 = 18,
            DC3 = 19,
            DC4 = 20,
            NACK = 21,
            SYN = 22,
            ETB = 23,
            CAN = 24,
            EM = 25,
            SUB = 26,
            ESC = 27,
            FS = 28,
            GS = 29,
            RS = 30,
            US = 31,
            DEL = 127,
            NBSP = 255
        }

        static ASCII()
        {
            ControlSet = new Dictionary<byte, string>();
            FullSet = new Dictionary<byte, string>();
            var bControlCode = Enum.GetValues(typeof(Control)).Cast<int>();

            for (var i = 0; i < 256; i++)
                if (bControlCode.Contains(i))
                {
                    ControlSet.Add((byte) i, ((Control) i).ToString());
                    FullSet.Add((byte) i, ((Control) i).ToString());
                }
                else
                {
                    FullSet.Add((byte) i, ((char) i).ToString());
                }

            ControlSet = new ReadOnlyDictionary<byte, string>(ControlSet);
            FullSet = new ReadOnlyDictionary<byte, string>(FullSet);
        }

        public static IDictionary<byte, string> ControlSet { get; }

        public static IDictionary<byte, string> FullSet { get; }
    }
}