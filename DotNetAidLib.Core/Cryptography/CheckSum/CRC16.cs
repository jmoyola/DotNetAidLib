using System;
using System.Security.Cryptography;

namespace DotNetAidLib.Core.Cryptography.CheckSum
{
    public class CRC16 : HashAlgorithm
    {
        private UInt16 [] polynomialTable = new UInt16 [256];
        private UInt16 initialValue;
        private UInt16 polynomial;
        private UInt16 crc;

        public CRC16 ()
            : this (0x1021, 0xffff) { }

        public CRC16 (UInt16 polynomial, UInt16 initialValue)
        {
            this.initialValue = initialValue;
            this.Polynomial = polynomial;

            this.Initialize ();
        }

        public override int HashSize {
            get {
                return 16;
            }
        }

        public UInt16 Polynomial {
            get {
                return polynomial;
            }

            set {
                this.polynomial = value;
                this.polynomialTable = GeneratePolynomialTable (this.polynomial);
            }
        }

        public override void Initialize ()
        {
            this.crc = initialValue;
        }

        protected override void HashCore (byte [] array, int ibStart, int cbSize)
        {
            for (int i = ibStart; i < (ibStart + cbSize); ++i)
                ComputeByte (this.polynomialTable, ref this.crc, array [i]);
        }

        protected override byte [] HashFinal ()
        {
            //augment_message_for_good_crc ();
            return new byte [] { (byte)(this.crc >> 8), (byte)(this.crc & 0xff) };
        }

        public static void ComputeByte (UInt16 [] polynomialTable, ref UInt16 crc, byte value) {
            crc = (UInt16)((crc << 8) ^ polynomialTable [((crc >> 8) ^ (0xff & value))]);
        }

        public static UInt16[] GeneratePolynomialTable (UInt16 polynomial)
        {
            UInt16 [] ret = new UInt16 [256];
            UInt16 temp;
            UInt16 a;

            for (int i = 0; i < ret.Length; ++i) {
                temp = 0;
                a = (UInt16)(i << 8);
                for (int j = 0; j < 8; ++j) {
                    if (((temp ^ a) & 0x8000) != 0)
                        temp = (UInt16)((temp << 1) ^ polynomial);
                    else
                        temp <<= 1;
                    a <<= 1;
                }
                ret [i] = temp;
            }

            return ret;
        }

        private void augment_message_for_good_crc ()
        {
            UInt16 i;
            bool xor_flag=false;

            for (i = 0; i < 16; i++) {
                if ((this.crc & 0x8000) > 0) {
                    xor_flag = true;
                } else {
                    xor_flag = false;
                }
                this.crc = (UInt16)(this.crc << 1);

                if (xor_flag) {
                    this.crc = (UInt16)(this.crc ^ this.Polynomial);
                }
            }
        }
    }
}