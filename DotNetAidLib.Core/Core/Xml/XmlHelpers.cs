using System.Collections.Generic;
using System.Xml;

namespace DotNetAidLib.Core.Xml
{
    public static class XmlHelpers
    {
        public static Dictionary<string, string> XmlToDictionary(this string xml)
        {
            var ret = new Dictionary<string, string>();

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            XmlNode baseNode = null;

            baseNode = xmlDocument.ChildNodes[xmlDocument.ChildNodes.Count - 1];

            StringToXmlToDictionary(ret, baseNode);
            return ret;
        }

        private static void StringToXmlToDictionary(Dictionary<string, string> values, XmlNode node)
        {
            if (node is XmlText || node is XmlCDataSection)
            {
                values.Add(FullPathName(node.ParentNode), node.Value);
                return;
            }

            if (node is XmlComment)
                return;

            if (node.Attributes != null)
                foreach (XmlAttribute att in node.Attributes)
                    values.Add(FullPathName(node) + "." + att.Name, att.Value);

            foreach (XmlNode child in node.ChildNodes)
                StringToXmlToDictionary(values, child);
        }

        private static string FullPathName(XmlNode node)
        {
            var ret = node.Name;
            while (node.ParentNode != node.OwnerDocument)
            {
                node = node.ParentNode;
                ret = node.Name + "." + ret;
            }

            return ret;
        }
    }
}