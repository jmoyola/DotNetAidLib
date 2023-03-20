using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DotNetAidLib.Core.Cryptography.CheckSum
{
	public enum HMacPasswordType{
		SHA1,
		SHA256,
		SHA384,
		SHA512
	}
	public class HMACPassword
	{
		private HMAC _HMac;

		public HMACPassword ()
			:this(HMacPasswordType.SHA1)
		{
		}

		public HMACPassword (HMacPasswordType type)
		{
			if (type.Equals (HMacPasswordType.SHA1))
				_HMac = HMACSHA1.Create ();
			else if (type.Equals (HMacPasswordType.SHA256))
				_HMac = HMACSHA256.Create ();
			else if (type.Equals (HMacPasswordType.SHA384))
				_HMac = HMACSHA384.Create ();
			else if (type.Equals (HMacPasswordType.SHA512))
				_HMac = HMACSHA512.Create ();

			this.GenerateKey (4);
		}

		public byte[] Key{
			get{ 
				return _HMac.Key;
			}
			set{
				_HMac.Key = value;
			}
		}

		public void GenerateKey(){
			this.GenerateKey (2);
		}

		public void GenerateKey(int keySize){
			byte[] key = new byte[keySize];
			Random rnd = new Random(DateTime.Now.Millisecond);
			rnd.NextBytes(key);
			this.Key = key;
		}

		public void GenerateKey(String stringKey){
			this.Key = System.Text.UTF8Encoding.UTF8.GetBytes(stringKey);
		}

		public string GetHPassword(string passPhrase)
		{
			MemoryStream ret = new MemoryStream();

			byte[] bRealPassword = UTF8Encoding.UTF8.GetBytes(passPhrase);
			byte[] bHMACPassword = _HMac.ComputeHash(bRealPassword);

			ret.Write(this.Key, 0, this.Key.Length);
			ret.Write(bHMACPassword, 0, bHMACPassword.Length);

			return ret.ToArray().ToHexadecimal("");
		}

		public bool HPasswordMatch(string passPhrase, string hmacPassword)
		{
			byte[] bHMacPassword = hmacPassword.HexToByteArray();
			byte[] key = bHMacPassword.Extract(0, bHMacPassword.Length - (_HMac.HashSize/8));


			bHMacPassword = bHMacPassword.Extract(key.Length, (_HMac.HashSize/8));

			byte[] auxKey = this.Key;
			this.Key = key;
			String generatedHMac = GetHPassword (passPhrase);
			this.Key = auxKey;

			return generatedHMac.Equals(hmacPassword);
		}

	}
}

