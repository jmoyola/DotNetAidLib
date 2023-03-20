using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using DotNetAidLib.Core.Cryptography.Encrypt;
using DotNetAidLib.Core.Data;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Database.DAO.Core;

namespace DotNetAidLib.Database.DAO.PropertyParsers
{

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DaoPropertyParserStringCryptAttribute : DaoPropertyParserAttribute
    {
        private static SymmetricEncriptAlgorithm encryptAlgorithm = null;
        private String salt = null;

        private String passPhrase;

        public DaoPropertyParserStringCryptAttribute(String passPhrase)
            : this(passPhrase, "dsfPOASTdg54whdykt_dwFD") { }

        static DaoPropertyParserStringCryptAttribute() {
            encryptAlgorithm = new SymmetricEncriptAlgorithm(SymmetricEncriptAlgorithmType.AES);
        }

        public DaoPropertyParserStringCryptAttribute(String passPhrase, String salt)
        {
            Assert.NotNullOrEmpty( passPhrase, nameof(passPhrase));
            this.passPhrase = passPhrase;

            
            this.salt = salt;
        }

        public override Object Deserialize (DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, IDataReader row)
        {
            Object ret = null;

            Object databaseValue = row.GetValue(propertyAttribute.ColumnName);
            if (databaseValue != DBNull.Value)
                ret = Decrypt(databaseValue.ToString(), this.passPhrase, this.salt);

            return ret;
        }

        public override Object SerializeBeforeInsert (DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity)
        {
            return this.Serialize(daoSession, entityAttribute, propertyAttribute, entity);
        }
        public override Object SerializeBeforeUpdate (DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity)
        {
            return this.Serialize(daoSession, entityAttribute, propertyAttribute, entity);
        }

        private Object Serialize (DaoSession daoSession, DaoEntityAttribute entityAttribute, DaoPropertyAttribute propertyAttribute, Object entity)
        {
            Object objectValue = propertyAttribute.PropertyInfo.GetValue(entity);

            if (objectValue != null)
                objectValue = Encrypt(objectValue.ToString(), this.passPhrase, this.salt);

            return objectValue;
        }

        public static String Encrypt(String value, String passPhrase, String salt)
        {
            if (value != null)
            {
                byte[] bSalt = Encoding.ASCII.GetBytes(salt);
                byte[] aUnEncryptValue = Encoding.Unicode.GetBytes(value);
                byte[] key = encryptAlgorithm.KeyFromPassphrase(passPhrase, bSalt);

                byte[] encrypt = encryptAlgorithm.Encrypt(aUnEncryptValue, key);

                value = Convert.ToBase64String(encrypt);
            }
            return value;
        }

        public static String Decrypt(String value, String passPhrase, String salt)
        {
            String ret = null;

            if (value != null)
            {
                byte[] bSalt = Encoding.ASCII.GetBytes(salt);
                byte[] aLastEncryptValue = Convert.FromBase64String(value);

                byte[] key = encryptAlgorithm.KeyFromPassphrase(passPhrase, bSalt);
                byte[] decrypt = encryptAlgorithm.Decrypt(aLastEncryptValue, key);

                ret = Encoding.Unicode.GetString(decrypt);
            }
            return ret;
        }
    }
}
