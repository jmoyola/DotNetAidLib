using System.Security.Cryptography;

namespace DotNetAidLib.Core.Cryptography.CheckSum
{
    public class XOR8 : HashAlgorithm
    {
        private byte _CRC;

        public override int HashSize => 8;


        public override void Initialize()
        {
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            _CRC = 0;

            for (var i = ibStart; i < cbSize; i++)
                _CRC ^= array[i];
        }

        protected override byte[] HashFinal()
        {
            return new[] {_CRC};
        }
    }
}