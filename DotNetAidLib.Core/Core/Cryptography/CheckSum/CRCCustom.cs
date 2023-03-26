using System;
using System.Security.Cryptography;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Cryptography.CheckSum
{
    public class CRCCustom : HashAlgorithm
    {
        private readonly Action<byte[], byte> computeDataHash;
        private readonly byte[] hash;
        private readonly Action<byte[]> initializeHash;

        public CRCCustom(int hashSize, Action<byte[]> initializeHash, Action<byte[], byte> computeDataHash)
        {
            Assert.NotNull(initializeHash, nameof(initializeHash));
            Assert.NotNull(computeDataHash, nameof(computeDataHash));

            HashSize = hashSize;
            hash = new byte [hashSize % 8 > 0 ? hashSize / 8 + 1 : hashSize / 8];
            this.initializeHash = initializeHash;
            this.computeDataHash = computeDataHash;
        }

        public override int HashSize { get; }

        public override void Initialize()
        {
            initializeHash(hash);
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            for (var i = ibStart; i < ibStart + cbSize; i++)
                computeDataHash(hash, array[i]);
        }

        protected override byte[] HashFinal()
        {
            return hash;
        }
    }
}