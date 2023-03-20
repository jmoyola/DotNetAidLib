using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace DotNetAidLib.Core.Xml.Serialization
{
    public class XmlSerializer
    {
        private static IList<Type> knowTypes=new List<Type>();

        public static IList<Type> KnowTypes {
            get { return knowTypes; }
        }

        public static void Serialize<T>(T v, FileInfo outputFile)
        {
            DataContractSerializer dcs = null;
            XmlWriter xw = null;

            try
            {
                xw = XmlWriter.Create(outputFile.FullName);
                xw.Settings.Indent = true;
                dcs = new DataContractSerializer(typeof(T), knowTypes);

                dcs.WriteObject(xw, v);
            }
            catch (Exception ex)
            {
                throw new Exception("Error serializing to file '" + outputFile.FullName + "'.", ex);
            }
            finally
            {
                if (xw != null)
                    xw.Close();
            }
        }

        public static T Deserialize<T>(FileInfo inputFile)
        {
            FileStream fs;

            try
            {
                fs = inputFile.OpenRead();
                return Deserialize<T>(fs, true);
            }
            catch (Exception ex)
            {
                throw new Exception("Error deserializing from file '" + inputFile.FullName + "'.", ex);
            }
        }

        public static void Serialize<T>(T v, Stream s, bool closeStream = false)
        {
            DataContractSerializer dcs = null;

            try
            {
                dcs = new DataContractSerializer(typeof(T), knowTypes);
                dcs.WriteObject(s, v);
            }
            catch (Exception ex)
            {
                throw new Exception("Error serializing to stream.", ex);
            }
            finally
            {
                if (closeStream && s != null)
                    s.Close();
            }
        }

        private static T Deserialize<T>(Stream s, bool closeStream = false)
        {
            DataContractSerializer dcs = null;

            try
            {
                dcs = new DataContractSerializer(typeof(T), knowTypes);
                return (T)dcs.ReadObject(s);
            }
            catch (Exception ex)
            {
                throw new Exception("Error deserializing from stream.", ex);
            }
            finally
            {
                if (closeStream && s != null)
                    s.Close();
            }
        }

    }
}
