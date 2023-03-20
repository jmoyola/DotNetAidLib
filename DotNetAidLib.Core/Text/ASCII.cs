using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DotNetAidLib.Core.Helpers;

namespace DotNetAidLib.Core.Text
{
    public class ASCII
    {
        private static IDictionary<byte, String> _ControlSet=null;
        private static IDictionary<byte, String> _FullSet=null;

        static ASCII()
        {
            _ControlSet=new Dictionary<byte, String>();
            _FullSet=new Dictionary<byte, String>();
            IEnumerable<int> bControlCode = Enum.GetValues(typeof(Control)).Cast<int>();

            for (int i = 0; i < 256; i++)
            {
                if (bControlCode.Contains(i))
                {
                    _ControlSet.Add((byte) i, ((Control) i).ToString());
                    _FullSet.Add((byte) i, ((Control) i).ToString());
                }
                else
                {
                    _FullSet.Add((byte) i, ((char) i).ToString());
                }
            }
            _ControlSet = new ReadOnlyDictionary<byte, string>(_ControlSet);
            _FullSet = new ReadOnlyDictionary<byte, string>(_FullSet);
        }

        public enum Control
        {
            NULL = 0, SOH = 1, STX = 2, ETX = 3, EOT = 4, ENQ = 5, ACK = 6, BEL = 7, BS = 8, TAB = 9, LF = 10, VT = 11, FF = 12, CR = 13, SO = 14, SI = 15, DLE = 16, DC1 = 17, DC2 = 18, DC3 = 19, DC4 = 20, NACK = 21, SYN = 22, ETB = 23, CAN = 24, EM = 25, SUB = 26, ESC = 27, FS = 28, GS = 29, RS = 30, US = 31,
            DEL = 127, NBSP = 255, 
        }

        public static IDictionary<byte, String> ControlSet=>_ControlSet;
        public static IDictionary<byte, String> FullSet=>_FullSet;
    }
}