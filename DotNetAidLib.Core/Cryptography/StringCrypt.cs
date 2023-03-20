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
using DotNetAidLib.Core.Cryptography.Encrypt;
using DotNetAidLib.Core.Context;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Cryptography{
	[DataContract]
	[Serializable]
	public class StringCrypt : IComparable
	{
		//, ISerializable 
		private SymmetricEncriptAlgorithm _EncryptAlgorithm=null;
		private string _Value = null;
		// <DebuggerBrowsable(DebuggerBrowsableState.Never)>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string _LastEncryptValue = null;

		private static string _DefaultSecretAttribute;
		private string _SecretAttribute;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static byte[] _Salt = null;


		protected virtual void OnCreated(){
			_EncryptAlgorithm=new SymmetricEncriptAlgorithm(SymmetricEncriptAlgorithmType.AES);
			_Salt = Encoding.ASCII.GetBytes("o6806642kbM7c5");
		}

		[OnDeserializing]
		private void OnDeserializing(StreamingContext c)
		{
			OnCreated();
		}

		public StringCrypt()
		{
			this.OnCreated ();

		}

		public StringCrypt(string value) : this()
		{
			this.Value = value;
		}

		public StringCrypt(string value, string secretAttribute) : this()
		{
			_SecretAttribute = secretAttribute;
			this.Value = value;
		}

		protected StringCrypt(SerializationInfo info, StreamingContext context):this()
		{
			this.EncryptValue = info.GetString("Value");
		}

		public string Value {
			get { return _Value; }
			set { _Value = value; }
		}

		[DataMember(IsRequired = true, Name = "Value", Order = 1)]
		public string EncryptValue {
			get {
				byte[] aUnEncryptValue = UnicodeEncoding.Unicode.GetBytes (_Value);
				byte[] key=_EncryptAlgorithm.KeyFromPassphrase (this.GetSecret (), _Salt);

				byte[] encrypt = _EncryptAlgorithm.Encrypt (aUnEncryptValue, key);

				return Convert.ToBase64String(encrypt);
			}
			set {
                if (value.RegexIsMatch ("^\"(.*)\"$"))
                    _Value = value.RegexGroupsMatches ("^\"(.*)\"$") [1];
                else {

                    _LastEncryptValue = value;

                    byte [] aLastEncryptValue = Convert.FromBase64String (_LastEncryptValue);

                    byte [] key = _EncryptAlgorithm.KeyFromPassphrase (this.GetSecret (), _Salt);
                    byte [] decrypt = _EncryptAlgorithm.Decrypt (aLastEncryptValue, key);

                    _Value = UnicodeEncoding.Unicode.GetString (decrypt);
                }
			}
		}
		/// <summary>
		/// Variable del contexto 'StringCrypt' donde encontrar la contraseña por defecto para la des/encriptación.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static string DefaultSecretAttribute {
			get { return _DefaultSecretAttribute; }
			set { _DefaultSecretAttribute = value; }
		}
		/// <summary>
		/// Variable del contexto 'StringCrypt' donde encontrar la contraseña para la des/encriptación (por defecto se inicializa al mismo valor que la propiedad compartida 'DefaultSecretAttribute'.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never), DataMember(IsRequired = true, Order = 0)]
		public string SecretAttribute {
			get { return _SecretAttribute; }
			set { _SecretAttribute = value; }
		}

        private string GetSecret()
		{
			Object ret = null;
			string secretAttribute = null;


			secretAttribute = _SecretAttribute;
			if ((string.IsNullOrEmpty(secretAttribute))) {
				secretAttribute = _DefaultSecretAttribute;
				if ((string.IsNullOrEmpty(secretAttribute))) {
					throw new Exception("You must set 'SecretAttribute' or 'DefaultSecretAttribute' property and can't be null or empty.");
				}
			}

			ret = ContextFactory.GetAttribute(secretAttribute);

			if (ret==null) {
				throw new Exception("Secret passphrase in context attribute '" + secretAttribute + "' don't exists or is null or empty.");
			}

			return ret.ToString();
		}

		public static StringCrypt FromValue(string value, string secret)
		{
			StringCrypt ret = new StringCrypt(secret);
			ret.Value = value;
			return ret;
		}

		public static StringCrypt FromEncryptValue(string encryptValue, string secret)
		{
			StringCrypt ret = new StringCrypt(secret);
			ret.EncryptValue = encryptValue;
			return ret;
		}

		public static implicit operator StringCrypt(string v)
		{
			return new StringCrypt(v);
		}

		public static implicit operator String(StringCrypt v)
		{
			return v.Value;
		}
			
		public override bool Equals(object obj)
		{
			if (((obj != null) && typeof(StringCrypt).IsAssignableFrom(obj.GetType()))) {
				return this.Value.Equals(((StringCrypt)obj).Value);
			} else {
				return base.Equals(obj);
			}
		}

		public override int GetHashCode ()
		{
			return this.Value.GetHashCode ();
		}

		public int CompareTo(object obj)
		{
			if (((obj != null) && typeof(StringCrypt).IsAssignableFrom(obj.GetType()))) {
				return this.Value.CompareTo(((StringCrypt)obj).Value);
			} else {
				return -1;
			}
		}

		public override string ToString()
		{
			return _Value;
		}

        public static void Initialize(String sharedSecret) {
            Initialize(sharedSecret, "stringCryptSharedSecret");
        }

        public static void Initialize(String sharedSecret, String secretAttribute)
        {
            Assert.NotNullOrEmpty( sharedSecret, nameof(sharedSecret));
            Assert.NotNullOrEmpty( secretAttribute, nameof(secretAttribute));

            StringCrypt.DefaultSecretAttribute = secretAttribute;
            ContextFactory.SetAttribute(secretAttribute, sharedSecret);
        }
    }
}