using System;

namespace DotNetAidLib.Database.UDao
{
    public interface UDaoSerializer
    {
        bool Match(Type type);
        object Serialize(object value, Type fromType);
        object Deserialize(object value, Type toType);
    }

    public class UDaoGUIDSerializer : UDaoSerializer
    {
        private readonly bool asString;

        public UDaoGUIDSerializer(bool asString)
        {
            this.asString = asString;
        }

        public bool Match(Type type)
        {
            return typeof(Guid).Equals(type) || typeof(Guid).Equals(Nullable.GetUnderlyingType(type));
        }

        public object Serialize(object value, Type fromType)
        {
            if (value == null) return Guid.Empty;

            if (asString)
                return ((Guid) value).ToString();
            return ((Guid) value).ToByteArray();
        }

        public object Deserialize(object value, Type toType)
        {
            if (DBNull.Value.Equals(value))
            {
                if (typeof(Guid?).Equals(toType))
                    return null;
                return Guid.Empty;
            }

            if (asString)
                return new Guid(value.ToString());
            return new Guid((byte[]) value);
        }
    }

    public class UDaoEnumSerializer : UDaoSerializer
    {
        private readonly bool asInteger;

        public UDaoEnumSerializer(bool asInteger)
        {
            this.asInteger = asInteger;
        }

        public bool Match(Type type)
        {
            return type.IsEnum || (Nullable.GetUnderlyingType(type) != null && Nullable.GetUnderlyingType(type).IsEnum);
        }

        public object Serialize(object value, Type fromType)
        {
            if (value == null) return DBNull.Value;

            if (asInteger)
                return (int) value;
            return value.ToString();
        }

        public object Deserialize(object value, Type toType)
        {
            object ret = null;

            if (!DBNull.Value.Equals(value))
            {
                var enumType = toType;
                if (Nullable.GetUnderlyingType(enumType) != null)
                    enumType = enumType.GenericTypeArguments[0];

                if (value is string)
                    ret = Enum.Parse(enumType, value.ToString(), true);
                else
                    ret = Enum.ToObject(enumType, value);
            }

            return ret;
        }
    }

    public class UDaoDefaultSerializer : UDaoSerializer
    {
        private static UDaoSerializer instance;

        private UDaoDefaultSerializer()
        {
        }

        public static UDaoSerializer Instance
        {
            get
            {
                if (instance == null)
                    instance = new UDaoDefaultSerializer();
                return instance;
            }
        }

        public bool Match(Type type)
        {
            return true;
        }

        public object Serialize(object value, Type fromType)
        {
            return value == null ? DBNull.Value : value;
        }

        public object Deserialize(object value, Type toType)
        {
            object ret = null;

            if (!DBNull.Value.Equals(value))
            {
                if (Nullable.GetUnderlyingType(toType) != null)
                    ret = Activator.CreateInstance(toType, Convert.ChangeType(value, toType.GetGenericArguments()[0]));
                else
                    ret = Convert.ChangeType(value, toType);
            }

            return ret;
        }
    }
}