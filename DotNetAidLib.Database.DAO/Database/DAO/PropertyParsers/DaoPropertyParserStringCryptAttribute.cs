using System;
using System.Data;
using System.Text;
using DotNetAidLib.Core.Cryptography.Encrypt;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Database.DAO.Core;

namespace DotNetAidLib.Database.DAO.PropertyParsers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DaoPropertyParserStringCryptAttribute : DaoPropertyParserAttribute
    {
        private static readonly SymmetricEncriptAlgorithm encryptAlgorithm;

        private readonly string passPhrase;
        private readonly string salt;

        static DaoPropertyParserStringCryptAttribute()
        {
            encryptAlgorithm = new SymmetricEncriptAlgorithm(SymmetricEncriptAlgorithmType.AES);
        }

        public DaoPropertyParserStringCryptAttribute(string passPhrase)
            : this(passPhrase, "dsfPOASTdg54whdykt_dwFD")
        {
        }

        public DaoPropertyParserStringCryptAttribute(string passPhrase, string salt)
        {
            Assert.NotNullOrEmpty(passPhrase, nameof(passPhrase));
            this.passPhrase = passPhrase;


            this.salt = salt;
        }

        public override object Deserialize(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, IDataReader row)
        {
            object ret = null;

            var databaseValue = row.GetValue(propertyAttribute.ColumnName);
            if (databaseValue != DBNull.Value)
                ret = Decrypt(databaseValue.ToString(), passPhrase, salt);

            return ret;
        }

        public override object SerializeBeforeInsert(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, object entity)
        {
            return Serialize(daoSession, entityAttribute, propertyAttribute, entity);
        }

        public override object SerializeBeforeUpdate(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, object entity)
        {
            return Serialize(daoSession, entityAttribute, propertyAttribute, entity);
        }

        private object Serialize(DaoSession daoSession, DaoEntityAttribute entityAttribute,
            DaoPropertyAttribute propertyAttribute, object entity)
        {
            var objectValue = propertyAttribute.PropertyInfo.GetValue(entity);

            if (objectValue != null)
                objectValue = Encrypt(objectValue.ToString(), passPhrase, salt);

            return objectValue;
        }

        public static string Encrypt(string value, string passPhrase, string salt)
        {
            if (value != null)
            {
                var bSalt = Encoding.ASCII.GetBytes(salt);
                var aUnEncryptValue = Encoding.Unicode.GetBytes(value);
                var key = encryptAlgorithm.KeyFromPassphrase(passPhrase, bSalt);

                var encrypt = encryptAlgorithm.Encrypt(aUnEncryptValue, key);

                value = Convert.ToBase64String(encrypt);
            }

            return value;
        }

        public static string Decrypt(string value, string passPhrase, string salt)
        {
            string ret = null;

            if (value != null)
            {
                var bSalt = Encoding.ASCII.GetBytes(salt);
                var aLastEncryptValue = Convert.FromBase64String(value);

                var key = encryptAlgorithm.KeyFromPassphrase(passPhrase, bSalt);
                var decrypt = encryptAlgorithm.Decrypt(aLastEncryptValue, key);

                ret = Encoding.Unicode.GetString(decrypt);
            }

            return ret;
        }
    }
}