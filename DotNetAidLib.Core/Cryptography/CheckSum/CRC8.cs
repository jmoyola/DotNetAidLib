using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace DotNetAidLib.Core.Cryptography.CheckSum
{
    ///
    /// This enum is used to indicate what kind of checksum you will be calculating.
    /// 
    public enum CRC8Mode
    {
        CRC8 = 0xd5,
        CRC8_CCITT = 0x07,
        CRC8_CCITT21 = 0x21,
        CRC8_DALLAS_MAXIM = 0x31,
        CRC8_SAE_J1850 = 0x1D,
        CRC_8_WCDMA = 0x9b,
    };

    /// 
    /// Class for calculating CRC8 checksums...
    /// 
	public class CRC8:HashAlgorithm
    {
		private byte _CRC;
		private byte[] _Table = new byte[256];
		private CRC8Mode _Mode;

		public CRC8(){
			this.Mode = CRC8Mode.CRC8;
		}

        public CRC8(CRC8Mode mode)
        {
            this.Mode=mode;
        }

		public CRC8Mode Mode {
			get {
				return _Mode;
			}
			set {
				_Mode = value;
				this.GenerateTable (_Mode);
			}
		}
			
		public override void Initialize ()
		{
			
		}

		protected override void HashCore (byte[] array, int ibStart, int cbSize)
		{
			_CRC = 0;

			for (int i=ibStart;i<cbSize; i++)
			{
				_CRC = _Table[_CRC ^ array[i]];
			}
		}

		protected override byte[] HashFinal ()
		{
			return new byte[]{_CRC};
		}

		public override int HashSize {
			get {
				return 8;
			}
		}

        private void GenerateTable(CRC8Mode polynomial)
        {
            for (int i = 0; i < 256; ++i)
            {
                int curr = i;

                for (int j = 0; j < 8; ++j)
                {
                    if ((curr & 0x80) != 0)
                    {
                        curr = (curr << 1) ^ (int)polynomial;
                    }
                    else
                    {
                        curr <<= 1;
                    }
                }

                _Table[i] = (byte)curr;
            }
        }


    }
}
