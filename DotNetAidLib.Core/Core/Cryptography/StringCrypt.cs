using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using DotNetAidLib.Core.Context;
using DotNetAidLib.Core.Cryptography.Encrypt;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Cryptography
{
    [DataContract]
    [Serializable]
    public class StringCrypt : IComparable
    {
        private static string _DefaultSecretAttribute;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static byte[] _Salt;

        //, ISerializable 
        private SymmetricEncriptAlgorithm _EncryptAlgorithm;

        // <DebuggerBrowsable(DebuggerBrowsableState.Never)>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _LastEncryptValue;

        private string _SecretAttribute;
        private string _Value;

        public StringCrypt()
        {
            OnCreated();
        }

        public StringCrypt(string value) : this()
        {
            Value = value;
        }

        public StringCrypt(string value, string secretAttribute) : this()
        {
            _SecretAttribute = secretAttribute;
            Value = value;
        }

        protected StringCrypt(SerializationInfo info, StreamingContext context) : this()
        {
            EncryptValue = info.GetString("Value");
        }

        public string Value
        {
            get => _Value;
            set => _Value = value;
        }

        [DataMember(IsRequired = true, Name = "Value", Order = 1)]
        public string EncryptValue
        {
            get
            {
                var aUnEncryptValue = Encoding.Unicode.GetBytes(_Value);
                var key = _EncryptAlgorithm.KeyFromPassphrase(GetSecret(), _Salt);

                var encrypt = _EncryptAlgorithm.Encrypt(aUnEncryptValue, key);

                return Convert.ToBase64String(encrypt);
            }
            set
            {
                if (value.RegexIsMatch("^\"(.*)\"$"))
                {
                    _Value = value.RegexGroupsMatches("^\"(.*)\"$")[1];
                }
                else
                {
                    _LastEncryptValue = value;

                    var aLastEncryptValue = Convert.FromBase64String(_LastEncryptValue);

                    var key = _EncryptAlgorithm.KeyFromPassphrase(GetSecret(), _Salt);
                    var decrypt = _EncryptAlgorithm.Decrypt(aLastEncryptValue, key);

                    _Value = Encoding.Unicode.GetString(decrypt);
                }
            }
        }

        /// <summary>
        ///     Variable del contexto 'StringCrypt' donde encontrar la contraseña por defecto para la des/encriptación.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static string DefaultSecretAttribute
        {
            get => _DefaultSecretAttribute;
            set => _DefaultSecretAttribute = value;
        }

        /// <summary>
        ///     Variable del contexto 'StringCrypt' donde encontrar la contraseña para la des/encriptación (por defecto se
        ///     inicializa al mismo valor que la propiedad compartida 'DefaultSecretAttribute'.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [DataMember(IsRequired = true, Order = 0)]
        public string SecretAttribute
        {
            get => _SecretAttribute;
            set => _SecretAttribute = value;
        }

        public int CompareTo(object obj)
        {
            if (obj != null && typeof(StringCrypt).IsAssignableFrom(obj.GetType()))
                return Value.CompareTo(((StringCrypt) obj).Value);
            return -1;
        }


        protected virtual void OnCreated()
        {
            _EncryptAlgorithm = new SymmetricEncriptAlgorithm(SymmetricEncriptAlgorithmType.AES);
            _Salt = Encoding.ASCII.GetBytes("o6806642kbM7c5");
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            OnCreated();
        }

        private string GetSecret()
        {
            object ret = null;
            string secretAttribute = null;


            secretAttribute = _SecretAttribute;
            if (string.IsNullOrEmpty(secretAttribute))
            {
                secretAttribute = _DefaultSecretAttribute;
                if (string.IsNullOrEmpty(secretAttribute))
                    throw new Exception(
                        "You must set 'SecretAttribute' or 'DefaultSecretAttribute' property and can't be null or empty.");
            }

            ret = ContextFactory.GetAttribute(secretAttribute);

            if (ret == null)
                throw new Exception("Secret passphrase in context attribute '" + secretAttribute +
                                    "' don't exists or is null or empty.");

            return ret.ToString();
        }

        public static StringCrypt FromValue(string value, string secret)
        {
            var ret = new StringCrypt(secret);
            ret.Value = value;
            return ret;
        }

        public static StringCrypt FromEncryptValue(string encryptValue, string secret)
        {
            var ret = new StringCrypt(secret);
            ret.EncryptValue = encryptValue;
            return ret;
        }

        public static implicit operator StringCrypt(string v)
        {
            return new StringCrypt(v);
        }

        public static implicit operator string(StringCrypt v)
        {
            return v.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj != null && typeof(StringCrypt).IsAssignableFrom(obj.GetType()))
                return Value.Equals(((StringCrypt) obj).Value);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return _Value;
        }

        public static void Initialize(string sharedSecret)
        {
            Initialize(sharedSecret, "stringCryptSharedSecret");
        }

        public static void Initialize(string sharedSecret, string secretAttribute)
        {
            Assert.NotNullOrEmpty(sharedSecret, nameof(sharedSecret));
            Assert.NotNullOrEmpty(secretAttribute, nameof(secretAttribute));

            DefaultSecretAttribute = secretAttribute;
            ContextFactory.SetAttribute(secretAttribute, sharedSecret);
        }
    }
}