using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using DotNetAidLib.Core.Context;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Cryptography.Encrypt
{
	
	public enum SymmetricEncriptAlgorithmType{
		RC2,
		DES,
		TRIPPLE_DES,
		AES,
		RIJNDAEL
	}

    public class SymmetricEncriptAlgorithm
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private PaddingMode _PaddingMode = PaddingMode.PKCS7;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CipherMode _CipherMode = CipherMode.CBC;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected SymmetricAlgorithm _CriptoServiceProvider = null;

        private SymmetricEncriptAlgorithmType _AlgorithmType;

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

            _AlgorithmType = algorithmType;

            this.AutoIV = true;
            this.IVIncluding = true;
        }

        ~SymmetricEncriptAlgorithm(){
            if (_CriptoServiceProvider != null)
                _CriptoServiceProvider.Clear();
        }

        public SymmetricEncriptAlgorithmType AlgorithmType {
			get {
				return _AlgorithmType;
			}
		}

		public bool AutoIV {get;set;}
		public bool IVIncluding {get;set;}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public PaddingMode PaddingMode {
			get {
				return _PaddingMode;
			}
			set {
				_PaddingMode = value;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public CipherMode CipherMode {
			get {
				return _CipherMode;
			}
			set {
				_CipherMode = value;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public byte[] IV {
			get {
				return _CriptoServiceProvider.IV;
			}
			set {
				_CriptoServiceProvider.IV = value;
			}
		}
			
		public void Encrypt(Stream inputStream, byte[] key, Stream outputEncryptStream)
		{
			try {
                Assert.NotNull( inputStream, nameof(inputStream));
                Assert.NotNull( outputEncryptStream, nameof(outputEncryptStream));
                Assert.NotNull( key, nameof(key));
                Assert.When(key, (v => key.Length >= _CriptoServiceProvider.KeySize / 8), "key length must be at least '" + _CriptoServiceProvider.KeySize / 8 + "'.", nameof(key));
                
				_CriptoServiceProvider.Key = key.Extract(0, _CriptoServiceProvider.KeySize / 8);

				_CriptoServiceProvider.Mode = this._CipherMode;
				_CriptoServiceProvider.Padding = this._PaddingMode;

				if(this.AutoIV)
					_CriptoServiceProvider.GenerateIV();

				if(this.IVIncluding) // Escribimos el vector de inicialización si se especificó
					outputEncryptStream.Write(_CriptoServiceProvider.IV, 0, _CriptoServiceProvider.IV.Length);

				ICryptoTransform encryptor = _CriptoServiceProvider.CreateEncryptor();
				CryptoStream csOutputEncryptStream = new CryptoStream(outputEncryptStream, encryptor, CryptoStreamMode.Write);

				inputStream.CopyTo(csOutputEncryptStream, 1024);

				csOutputEncryptStream.FlushFinalBlock();
			} catch (Exception ex) {
				throw new Exception("Error encryting stream.\r\n" + ex.ToString(), ex);
			}
		}

		public void Decrypt(Stream inputEncryptStream, byte[] key, Stream outputStream)
		{

			try {
                Assert.NotNull( inputEncryptStream, nameof(inputEncryptStream));
                Assert.NotNull( outputStream, nameof(outputStream));
                Assert.NotNull( key, nameof(key));
                Assert.When(key, (v => key.Length >= _CriptoServiceProvider.KeySize / 8), "key length must be at least '" + _CriptoServiceProvider.KeySize / 8 + "'.", nameof(key));

                _CriptoServiceProvider.Key = key.Extract(0, _CriptoServiceProvider.KeySize / 8);

				_CriptoServiceProvider.Mode = this._CipherMode;
				_CriptoServiceProvider.Padding = this._PaddingMode;

				byte[] iv=new byte[_CriptoServiceProvider.IV.Length];
				if(this.IVIncluding){ // Leemos el vector de inicialización
					inputEncryptStream.Read(iv, 0, _CriptoServiceProvider.IV.Length);
					_CriptoServiceProvider.IV=iv;
				}

				ICryptoTransform decryptor = _CriptoServiceProvider.CreateDecryptor();
				CryptoStream csInputEncryptStream = new CryptoStream(inputEncryptStream, decryptor, CryptoStreamMode.Read);
				csInputEncryptStream.CopyTo(outputStream, 1024);
			} catch (Exception ex) {
				throw new Exception("Error decryting stream.\r\n" + ex.ToString(), ex);
			}
		}

		public byte[] Encrypt(byte[] input, byte[] key)
		{
			MemoryStream inputStream = null;
			MemoryStream outputStream = null;
			try{
				inputStream = new MemoryStream(input);
				outputStream = new MemoryStream();
				this.Encrypt(inputStream, key, outputStream);
				return outputStream.ToArray();
			}
			catch(Exception ex){
				throw new Exception("Error encryting array.\r\n" + ex.ToString(), ex);
			}
			finally{
				inputStream.Close ();
				outputStream.Close ();
			}
		}

		public byte[] Decrypt(byte[] input, byte[] key)
		{
			MemoryStream inputStream = null;
			MemoryStream outputStream = null;
			try{
				inputStream = new MemoryStream(input);
				outputStream = new MemoryStream();
				this.Decrypt(inputStream, key, outputStream);
				return outputStream.ToArray();
			}
			catch(Exception ex){
				throw new Exception("Error decryting array.\r\n" + ex.ToString(), ex);
			}
			finally{
				inputStream.Close ();
				outputStream.Close ();
			}
		}

        public string EncryptString(string inputString, String passphrase)
        {
            return Encrypt(inputString, KeyFromPassphrase(passphrase), Encoding.Default, true);
        }

        public string DecryptString(string inputString, String passphrase)
        {
            return Decrypt(inputString, KeyFromPassphrase(passphrase), Encoding.Default, true);
        }

        public string Encrypt(string inputString, byte[] key, Encoding enc, bool toBase64)
        {
            string ret = null;
            byte[] inBytes = enc.GetBytes(inputString);
            byte[] outBytes = Encrypt(inBytes, key);
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

            byte[] outBytes = Encrypt(inBytes, key);
            ret = enc.GetString(outBytes);
            return ret;
        }

		public byte[] KeyFromPassphrase(String passphrase){
			return KeyFromPassphrase (passphrase, "o6806642kbM7c5");
		}

		public byte[] KeyFromPassphrase(String passphrase, string salt){
			byte[] baSalt = Encoding.ASCII.GetBytes(salt);
			return KeyFromPassphrase (passphrase, baSalt);
		}

		public byte[] KeyFromPassphrase(String passphrase, byte[] salt){
			Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(passphrase, salt);
			return  key.GetBytes(_CriptoServiceProvider.KeySize / 8);
		}

        public T DecryptObject<T>(byte[] inputBytes, byte[] key)
        {
            try
            {
                T ret;
                MemoryStream iSt = new MemoryStream(inputBytes);
                ret = this.DecryptObject<T>(iSt, key);
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
                MemoryStream oSt = new MemoryStream();
                this.EncryptObject(o, key, oSt);
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
                Stream iSt = inputStream;
                MemoryStream oSt = new MemoryStream();
                this.Decrypt(iSt, key, oSt);
                iSt.Close();
                oSt.Seek(0, SeekOrigin.Begin);
                BinaryFormatter bf = new BinaryFormatter();
                ret = ((T)(bf.Deserialize(oSt)));
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
                BinaryFormatter bf = new BinaryFormatter();
                Stream iSt = new MemoryStream();
                bf.Serialize(iSt, o);
                iSt.Seek(0, SeekOrigin.Begin);
                this.Encrypt(iSt, key, outputStream);
                iSt.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error encripting object.", ex);
            }
        }
    }
}