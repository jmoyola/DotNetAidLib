using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace DotNetAidLib.Core.Cryptography.CheckSum
{
	public class XOR8:HashAlgorithm
    {
		private byte _CRC;

		public XOR8(){
			
		}
		
			
		public override void Initialize ()
		{
			
		}

		protected override void HashCore (byte[] array, int ibStart, int cbSize)
		{
			_CRC = 0;

			for (int i=ibStart;i<cbSize; i++)
				_CRC ^= array[i];
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
    }
}
