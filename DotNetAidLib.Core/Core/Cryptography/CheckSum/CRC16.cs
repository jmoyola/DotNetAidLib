using System.Security.Cryptography;

namespace DotNetAidLib.Core.Cryptography.CheckSum
{
    public class CRC16 : HashAlgorithm
    {
        private ushort crc;
        private readonly ushort initialValue;
        private ushort polynomial;
        private ushort[] polynomialTable = new ushort [256];

        public CRC16()
            : this(0x1021, 0xffff)
        {
        }

        public CRC16(ushort polynomial, ushort initialValue)
        {
            this.initialValue = initialValue;
            Polynomial = polynomial;

            Initialize();
        }

        public override int HashSize => 16;

        public ushort Polynomial
        {
            get => polynomial;

            set
            {
                polynomial = value;
                polynomialTable = GeneratePolynomialTable(polynomial);
            }
        }

        public override void Initialize()
        {
            crc = initialValue;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            for (var i = ibStart; i < ibStart + cbSize; ++i)
                ComputeByte(polynomialTable, ref crc, array[i]);
        }

        protected override byte[] HashFinal()
        {
            //augment_message_for_good_crc ();
            return new[] {(byte) (crc >> 8), (byte) (crc & 0xff)};
        }

        public static void ComputeByte(ushort[] polynomialTable, ref ushort crc, byte value)
        {
            crc = (ushort) ((crc << 8) ^ polynomialTable[(crc >> 8) ^ (0xff & value)]);
        }

        public static ushort[] GeneratePolynomialTable(ushort polynomial)
        {
            var ret = new ushort [256];
            ushort temp;
            ushort a;

            for (var i = 0; i < ret.Length; ++i)
            {
                temp = 0;
                a = (ushort) (i << 8);
                for (var j = 0; j < 8; ++j)
                {
                    if (((temp ^ a) & 0x8000) != 0)
                        temp = (ushort) ((temp << 1) ^ polynomial);
                    else
                        temp <<= 1;
                    a <<= 1;
                }

                ret[i] = temp;
            }

            return ret;
        }

        private void augment_message_for_good_crc()
        {
            ushort i;
            var xor_flag = false;

            for (i = 0; i < 16; i++)
            {
                if ((crc & 0x8000) > 0)
                    xor_flag = true;
                else
                    xor_flag = false;
                crc = (ushort) (crc << 1);

                if (xor_flag) crc = (ushort) (crc ^ Polynomial);
            }
        }
    }
}