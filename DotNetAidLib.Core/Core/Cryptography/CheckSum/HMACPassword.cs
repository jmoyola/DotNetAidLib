using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DotNetAidLib.Core.Cryptography.CheckSum
{
    public enum HMacPasswordType
    {
        SHA1,
        SHA256,
        SHA384,
        SHA512
    }

    public class HMACPassword
    {
        private readonly HMAC _HMac;

        public HMACPassword()
            : this(HMacPasswordType.SHA1)
        {
        }

        public HMACPassword(HMacPasswordType type)
        {
            if (type.Equals(HMacPasswordType.SHA1))
                _HMac = HMAC.Create();
            else if (type.Equals(HMacPasswordType.SHA256))
                _HMac = HMAC.Create();
            else if (type.Equals(HMacPasswordType.SHA384))
                _HMac = HMAC.Create();
            else if (type.Equals(HMacPasswordType.SHA512))
                _HMac = HMAC.Create();

            GenerateKey(4);
        }

        public byte[] Key
        {
            get => _HMac.Key;
            set => _HMac.Key = value;
        }

        public void GenerateKey()
        {
            GenerateKey(2);
        }

        public void GenerateKey(int keySize)
        {
            var key = new byte[keySize];
            var rnd = new Random(DateTime.Now.Millisecond);
            rnd.NextBytes(key);
            Key = key;
        }

        public void GenerateKey(string stringKey)
        {
            Key = Encoding.UTF8.GetBytes(stringKey);
        }

        public string GetHPassword(string passPhrase)
        {
            var ret = new MemoryStream();

            var bRealPassword = Encoding.UTF8.GetBytes(passPhrase);
            var bHMACPassword = _HMac.ComputeHash(bRealPassword);

            ret.Write(Key, 0, Key.Length);
            ret.Write(bHMACPassword, 0, bHMACPassword.Length);

            return ret.ToArray().ToHexadecimal("");
        }

        public bool HPasswordMatch(string passPhrase, string hmacPassword)
        {
            var bHMacPassword = hmacPassword.HexToByteArray();
            var key = bHMacPassword.Extract(0, bHMacPassword.Length - _HMac.HashSize / 8);


            bHMacPassword = bHMacPassword.Extract(key.Length, _HMac.HashSize / 8);

            var auxKey = Key;
            Key = key;
            var generatedHMac = GetHPassword(passPhrase);
            Key = auxKey;

            return generatedHMac.Equals(hmacPassword);
        }
    }
}