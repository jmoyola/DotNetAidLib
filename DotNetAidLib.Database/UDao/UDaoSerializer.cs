using System;
namespace DotNetAidLib.Database.UDao
{
    public interface UDaoSerializer
    {
        bool Match (Type type);
        Object Serialize (object value, Type fromType);
        Object Deserialize (object value, Type toType);
    }

    public class UDaoGUIDSerializer : UDaoSerializer
    {
        private bool asString;
        public UDaoGUIDSerializer (bool asString){
            this.asString = asString;
        }

        public bool Match (Type type) => typeof (Guid).Equals(type) || typeof (Guid).Equals (Nullable.GetUnderlyingType (type));
        public Object Serialize (object value, Type fromType)
        {
            if (value == null)
                return Guid.Empty;
            else {
                if (this.asString)
                    return ((Guid)value).ToString();
                else
                    return ((Guid)value).ToByteArray();
            }
        }

        public Object Deserialize (object value, Type toType)
        {
            if (DBNull.Value.Equals(value))
                if (typeof (Nullable<Guid>).Equals (toType))
                    return null;
                else
                    return Guid.Empty;
            else {
                if (this.asString)
                    return new Guid (value.ToString());
                else
                    return new Guid ((byte[])value);
            }
        }
    }

    public class UDaoEnumSerializer : UDaoSerializer
    {
        private bool asInteger;

        public UDaoEnumSerializer (bool asInteger) {
            this.asInteger = asInteger;
        }

        public bool Match (Type type) => type.IsEnum || (Nullable.GetUnderlyingType (type)!=null && Nullable.GetUnderlyingType (type).IsEnum);
        public Object Serialize (object value, Type fromType){
            if (value == null)
                return DBNull.Value;
            else {
                if (this.asInteger)
                    return (int)value;
                else
                    return value.ToString ();
            }
        }

        public Object Deserialize (object value, Type toType)
        {
            Object ret = null;

            if (!DBNull.Value.Equals (value)) {
                Type enumType = toType;
                if (Nullable.GetUnderlyingType (enumType) != null)
                    enumType = enumType.GenericTypeArguments [0];

                if (value is String)
                    ret = Enum.Parse (enumType, value.ToString (), true);
                else
                    ret = Enum.ToObject (enumType, value);
            }
            return ret;
        }
    }

    public class UDaoDefaultSerializer : UDaoSerializer
    {
        private static UDaoSerializer instance = null;

        private UDaoDefaultSerializer () { }

        public bool Match (Type type)=>true;
        public Object Serialize (object value, Type fromType) => (value == null ? DBNull.Value : value);
        public Object Deserialize (object value, Type toType){
            Object ret = null;

            if (!DBNull.Value.Equals (value)) {
                if (Nullable.GetUnderlyingType (toType) != null)
                    ret = Activator.CreateInstance (toType, Convert.ChangeType (value, toType.GetGenericArguments () [0]));
                else
                    ret = Convert.ChangeType (value, toType);
            }
            return ret; 
        }

        public static UDaoSerializer Instance {
            get {
                if (instance == null)
                    instance = new UDaoDefaultSerializer ();
                return instance;
            }
        }
    }

}
