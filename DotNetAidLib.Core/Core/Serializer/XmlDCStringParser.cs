using System;
using System.Runtime.Serialization;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Text;

namespace DotNetAidLib.Core.Serializer
{
    public class XmlDCStringParser : IStringParser
    {
        private static IStringParser _Instance;

        private XmlDCStringParser()
        {
        }

        public override string Syntax => "Xml Syntax";

        public override string Parse(object value)
        {
            var ser = new DataContractSerializer(value.GetType());
            using (var ss = new StringStream())
            {
                ser.WriteObject(ss, value);
                return ss.ToString();
            }
        }

        public override object Unparse(string value, Type type = null)
        {
            Assert.NotNull(type, nameof(type));

            using (var ss = new StringStream(value))
            {
                var ser = new DataContractSerializer(type);

                return ser.ReadObject(ss);
            }
        }

        public static IStringParser Instance()
        {
            if (_Instance == null)
                _Instance = new XmlDCStringParser();
            return _Instance;
        }
    }
}