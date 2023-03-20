using System;
using System.Security.Cryptography;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Cryptography.CheckSum
{
    public class CRCCustom: HashAlgorithm
    {
        private int hashSize;
        private byte [] hash;
        private Action<byte []> initializeHash;
        private Action<byte[], byte> computeDataHash;

        public CRCCustom (int hashSize, Action<byte []> initializeHash,  Action<byte[], byte> computeDataHash)
        {
            Assert.NotNull( initializeHash, nameof(initializeHash));
            Assert.NotNull( computeDataHash, nameof(computeDataHash));

            this.hashSize = hashSize;
            this.hash = new byte [hashSize % 8 > 0? (hashSize / 8) + 1: hashSize / 8];
            this.initializeHash = initializeHash;
            this.computeDataHash = computeDataHash;
        }

        public override int HashSize {
            get {
                return this.hashSize;
            }
        }

        public override void Initialize ()
        {
            this.initializeHash (this.hash);
        }

        protected override void HashCore (byte [] array, int ibStart, int cbSize)
        {
            for (int i = ibStart; i < ibStart + cbSize; i++)
                this.computeDataHash (this.hash, array[i]);
        }

        protected override byte [] HashFinal ()
        {
            return this.hash;
        }
    }
}
