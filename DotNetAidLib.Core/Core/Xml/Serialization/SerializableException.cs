using System;
using System.Runtime.Serialization;

namespace DotNetAidLib.Core.Xml.Serialization
{
    [DataContract]
    public class SerializableException
    {
        public SerializableException(Exception ex)
        {
            ExceptionType = ex.GetType().FullName;
            Message = ex.Message;
            StackTrace = ex.StackTrace;
            if (ex.InnerException != null)
                InnerException = new SerializableException(ex.InnerException);
            HelpLink = ex.HelpLink;
            HResult = ex.HResult;
            Source = ex.Source;
            if (ex.TargetSite != null)
                TargetSite = ex.TargetSite.Name;
        }

        [DataMember] public string ExceptionType { get; set; }

        [DataMember] public string Message { get; set; }

        [DataMember] public string StackTrace { get; set; }

        [DataMember] public SerializableException InnerException { get; set; }

        [DataMember] public string HelpLink { get; set; }

        [DataMember] public int HResult { get; set; }

        [DataMember] public string Source { get; set; }

        [DataMember] public string TargetSite { get; set; }

        public Exception ToException()
        {
            Exception innerException = null;
            if (InnerException != null)
                innerException = InnerException.ToException();

            var ret = new Exception(Message, innerException);

            return ret;
        }

        public static SerializableException FromException(Exception ex)
        {
            var ret = new SerializableException(ex);
            return ret;
        }
    }
}