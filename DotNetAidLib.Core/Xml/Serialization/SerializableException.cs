using System;
using System.Runtime.Serialization;

namespace DotNetAidLib.Core.Xml.Serialization
{
	[DataContract]
	public class SerializableException
	{
		public SerializableException (Exception ex)
		{
			this.ExceptionType = ex.GetType ().FullName;
			this.Message = ex.Message;
			this.StackTrace = ex.StackTrace;
			if (ex.InnerException != null)
				this.InnerException = new SerializableException (ex.InnerException);
			this.HelpLink = ex.HelpLink;
			this.HResult = ex.HResult;
			this.Source = ex.Source;
			if (ex.TargetSite != null)
				this.TargetSite = ex.TargetSite.Name; 
		}

		[DataMember]
		public String ExceptionType{ get; set;}
		[DataMember]
		public String Message{ get; set;}
		[DataMember]
		public string StackTrace{ get; set;}
		[DataMember]
		public SerializableException InnerException{ get; set;}
		[DataMember]
		public String HelpLink{ get; set;}
		[DataMember]
		public int HResult{ get; set;}
		[DataMember]
		public String Source{ get; set;}
		[DataMember]
		public String TargetSite{ get; set;}

		public Exception ToException(){
			Exception innerException = null;
			if (this.InnerException != null)
				innerException = this.InnerException.ToException ();

			Exception ret = new Exception(this.Message, innerException);

			return ret;
		}

		public static SerializableException FromException(Exception ex)
		{
			SerializableException ret = new SerializableException (ex);
			return ret;
		}
	}
}

