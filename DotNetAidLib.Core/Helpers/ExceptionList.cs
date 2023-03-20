using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Core.Helpers
{
    public class ExceptionList:Exception, IList<Exception>{

        private String commonMessage = "Exception list";
        private IList<Exception> exceptions=new List<Exception>();

        public ExceptionList(){

        }

        public ExceptionList(String commonMessage){
            this.commonMessage = commonMessage;
        }

        public ExceptionList(String commonMessage, Exception exception){
            this.commonMessage = commonMessage;
            this.Add(exception);
        }

        public ExceptionList(String commonMessage, IList<Exception> exceptions){
            this.commonMessage = commonMessage;
            this.AddRange(exceptions);
        }

        public string CommonMessage
        {
            set { this.commonMessage = value; }
            get { return this.commonMessage; }
        }

        public override string Message
        {
            get { return (String.IsNullOrEmpty(this.commonMessage)?"":this.commonMessage + ": ") + exceptions.Select(v => v.Message).ToStringJoin("; "); }
        }

        public override string ToString()
        {
            return (String.IsNullOrEmpty(this.commonMessage) ? "" : this.commonMessage + ": ") + exceptions.Select(v=>v.ToString()).ToStringJoin("\r\n");
        }

        public bool HasError {
            get { return this.Count > 0; }
        }

        public Exception this[int index]{
            get { return exceptions[index];}
            set { exceptions[index] = value; }
        }

        public int Count {
            get{
                return exceptions.Count;
            }
        }

        public bool IsReadOnly{
            get { return exceptions.IsReadOnly; }
        }

        public void Clear(){ exceptions.Clear(); }

        public bool Contains(Exception item) { return exceptions.Contains(item); }

        public void CopyTo(Exception[] array, int arrayIndex){ exceptions.CopyTo(array, arrayIndex); }

        public IEnumerator<Exception> GetEnumerator() { return exceptions.GetEnumerator(); }

        public int IndexOf(Exception item) { return exceptions.IndexOf(item); }

        public void Insert(int index, Exception item) { exceptions.Insert(index, item); }

        public void RemoveAt(int index) { exceptions.RemoveAt(index); }

        public void Add(Exception item){ this.exceptions.Add(item); }
		public void Add(ExceptionList exceptions) {
			this.AddRange(exceptions);
		}
        public void AddRange(IList<Exception> items) {
            foreach(Exception item in items)
                this.exceptions.Add(item);
        }

        IEnumerator IEnumerable.GetEnumerator(){ return exceptions.GetEnumerator(); }

        public bool Remove(Exception item){ return this.exceptions.Remove(item); }
    }
}
