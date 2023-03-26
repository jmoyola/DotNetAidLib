using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Cryptography.Encrypt
{
    public enum SymmetricEncriptAlgorithmType
    {
        RC2,
        DES,
        TRIPPLE_DES,
        AES,
        RIJNDAEL
    }

    public class SymmetricEncriptAlgorithm
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected SymmetricAlgorithm _CriptoServiceProvider;

        public SymmetricEncriptAlgorithm(SymmetricEncriptAlgorithmType algorithmType)
        {
            if (algorithmType.Equals(SymmetricEncriptAlgorithmType.RC2))
                _CriptoServiceProvider = RC2.Create();
            else if (algorithmType.Equals(SymmetricEncriptAlgorithmType.DES))
                _CriptoServiceProvider = DES.Create();
            else if (algorithmType.Equals(SymmetricEncriptAlgorithmType.TRIPPLE_DES))
                _CriptoServiceProvider = TripleDES.Create();
            else if (algorithmType.Equals(SymmetricEncriptAlgorithmType.AES))
                _CriptoServiceProvider = Aes.Create();
            else if (algorithmType.Equals(SymmetricEncriptAlgorithmType.RIJNDAEL))
                _CriptoServiceProvider = Rijndael.Create();

            _CriptoServiceProvider.GenerateIV();

            AlgorithmType = algorithmType;

            AutoIV = true;
            IVIncluding = true;
        }

        public SymmetricEncriptAlgorithmType AlgorithmType { get; }

        public bool AutoIV { get; set; }
        public bool IVIncluding { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public PaddingMode PaddingMode { get; set; } = PaddingMode.PKCS7;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public CipherMode CipherMode { get; set; } = CipherMode.CBC;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public byte[] IV
        {
            get => _CriptoServiceProvider.IV;
            set => _CriptoServiceProvider.IV = value;
        }

        ~SymmetricEncriptAlgorithm()
        {
            if (_CriptoServiceProvider != null)
                _CriptoServiceProvider.Clear();
        }

        public void Encrypt(Stream inputStream, byte[] key, Stream outputEncryptStream)
        {
            try
            {
                Assert.NotNull(inputStream, nameof(inputStream));
                Assert.NotNull(outputEncryptStream, nameof(outputEncryptStream));
                Assert.NotNull(key, nameof(key));
                Assert.When(key, v => key.Length >= _CriptoServiceProvider.KeySize / 8,
                    "key length must be at least '" + _CriptoServiceProvider.KeySize / 8 + "'.", nameof(key));

                _CriptoServiceProvider.Key = key.Extract(0, _CriptoServiceProvider.KeySize / 8);

                _CriptoServiceProvider.Mode = CipherMode;
                _CriptoServiceProvider.Padding = PaddingMode;

                if (AutoIV)
                    _CriptoServiceProvider.GenerateIV();

                if (IVIncluding) // Escribimos el vector de inicialización si se especificó
                    outputEncryptStream.Write(_CriptoServiceProvider.IV, 0, _CriptoServiceProvider.IV.Length);

                var encryptor = _CriptoServiceProvider.CreateEncryptor();
                var csOutputEncryptStream = new CryptoStream(outputEncryptStream, encryptor, CryptoStreamMode.Write);

                inputStream.CopyTo(csOutputEncryptStream, 1024);

                csOutputEncryptStream.FlushFinalBlock();
            }
            catch (Exception ex)
            {
                throw new Exception("Error encryting stream.\r\n" + ex, ex);
            }
        }

        public void Decrypt(Stream inputEncryptStream, byte[] key, Stream outputStream)
        {
            try
            {
                Assert.NotNull(inputEncryptStream, nameof(inputEncryptStream));
                Assert.NotNull(outputStream, nameof(outputStream));
                Assert.NotNull(key, nameof(key));
                Assert.When(key, v => key.Length >= _CriptoServiceProvider.KeySize / 8,
                    "key length must be at least '" + _CriptoServiceProvider.KeySize / 8 + "'.", nameof(key));

                _CriptoServiceProvider.Key = key.Extract(0, _CriptoServiceProvider.KeySize / 8);

                _CriptoServiceProvider.Mode = CipherMode;
                _CriptoServiceProvider.Padding = PaddingMode;

                var iv = new byte[_CriptoServiceProvider.IV.Length];
                if (IVIncluding)
                {
                    // Leemos el vector de inicialización
                    inputEncryptStream.Read(iv, 0, _CriptoServiceProvider.IV.Length);
                    _CriptoServiceProvider.IV = iv;
                }

                var decryptor = _CriptoServiceProvider.CreateDecryptor();
                var csInputEncryptStream = new CryptoStream(inputEncryptStream, decryptor, CryptoStreamMode.Read);
                csInputEncryptStream.CopyTo(outputStream, 1024);
            }
            catch (Exception ex)
            {
                throw new Exception("Error decryting stream.\r\n" + ex, ex);
            }
        }

        public byte[] Encrypt(byte[] input, byte[] key)
        {
            MemoryStream inputStream = null;
            MemoryStream outputStream = null;
            try
            {
                inputStream = new MemoryStream(input);
                outputStream = new MemoryStream();
                Encrypt(inputStream, key, outputStream);
                return outputStream.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception("Error encryting array.\r\n" + ex, ex);
            }
            finally
            {
                inputStream.Close();
                outputStream.Close();
            }
        }

        public byte[] Decrypt(byte[] input, byte[] key)
        {
            MemoryStream inputStream = null;
            MemoryStream outputStream = null;
            try
            {
                inputStream = new MemoryStream(input);
                outputStream = new MemoryStream();
                Decrypt(inputStream, key, outputStream);
                return outputStream.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception("Error decryting array.\r\n" + ex, ex);
            }
            finally
            {
                inputStream.Close();
                outputStream.Close();
            }
        }

        public string EncryptString(string inputString, string passphrase)
        {
            return Encrypt(inputString, KeyFromPassphrase(passphrase), Encoding.Default, true);
        }

        public string DecryptString(string inputString, string passphrase)
        {
            return Decrypt(inputString, KeyFromPassphrase(passphrase), Encoding.Default, true);
        }

        public string Encrypt(string inputString, byte[] key, Encoding enc, bool toBase64)
        {
            string ret = null;
            var inBytes = enc.GetBytes(inputString);
            var outBytes = Encrypt(inBytes, key);
            if (toBase64)
                ret = Convert.ToBase64String(outBytes);
            else
                ret = enc.GetString(outBytes);

            return ret;
        }

        public string Decrypt(string inputString, byte[] key, Encoding enc, bool fromBase64)
        {
            string ret = null;
            byte[] inBytes;
            if (fromBase64)
                inBytes = Convert.FromBase64String(inputString);
            else
                inBytes = enc.GetBytes(inputString);

            var outBytes = Encrypt(inBytes, key);
            ret = enc.GetString(outBytes);
            return ret;
        }

        public byte[] KeyFromPassphrase(string passphrase)
        {
            return KeyFromPassphrase(passphrase, "o6806642kbM7c5");
        }

        public byte[] KeyFromPassphrase(string passphrase, string salt)
        {
            var baSalt = Encoding.ASCII.GetBytes(salt);
            return KeyFromPassphrase(passphrase, baSalt);
        }

        public byte[] KeyFromPassphrase(string passphrase, byte[] salt)
        {
            var key = new Rfc2898DeriveBytes(passphrase, salt);
            return key.GetBytes(_CriptoServiceProvider.KeySize / 8);
        }

        public T DecryptObject<T>(byte[] inputBytes, byte[] key)
        {
            try
            {
                T ret;
                var iSt = new MemoryStream(inputBytes);
                ret = DecryptObject<T>(iSt, key);
                iSt.Close();
                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception("Error decripting object.", ex);
            }
        }

        public byte[] EncryptObject<T>(T o, byte[] key)
        {
            try
            {
                byte[] ret;
                var oSt = new MemoryStream();
                EncryptObject(o, key, oSt);
                oSt.Seek(0, SeekOrigin.Begin);
                ret = oSt.ToArray();
                oSt.Close();
                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception("Error encripting object.", ex);
            }
        }

        public T DecryptObject<T>(Stream inputStream, byte[] key)
        {
            try
            {
                T ret;
                var iSt = inputStream;
                var oSt = new MemoryStream();
                Decrypt(iSt, key, oSt);
                iSt.Close();
                oSt.Seek(0, SeekOrigin.Begin);
                var bf = new BinaryFormatter();
                ret = (T) bf.Deserialize(oSt);
                oSt.Close();
                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception("Error decripting object.", ex);
            }
        }

        public void EncryptObject<T>(T o, byte[] key, Stream outputStream)
        {
            try
            {
                var bf = new BinaryFormatter();
                Stream iSt = new MemoryStream();
                bf.Serialize(iSt, o);
                iSt.Seek(0, SeekOrigin.Begin);
                Encrypt(iSt, key, outputStream);
                iSt.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error encripting object.", ex);
            }
        }
    }
}