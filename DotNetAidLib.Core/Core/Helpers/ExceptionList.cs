using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetAidLib.Core.Collections;

namespace DotNetAidLib.Core.Helpers
{
    public class ExceptionList : Exception, IList<Exception>
    {
        private readonly IList<Exception> exceptions = new List<Exception>();

        public ExceptionList()
        {
        }

        public ExceptionList(string commonMessage)
        {
            this.CommonMessage = commonMessage;
        }

        public ExceptionList(string commonMessage, Exception exception)
        {
            this.CommonMessage = commonMessage;
            Add(exception);
        }

        public ExceptionList(string commonMessage, IList<Exception> exceptions)
        {
            this.CommonMessage = commonMessage;
            AddRange(exceptions);
        }

        public string CommonMessage { set; get; } = "Exception list";

        public override string Message
        {
            get
            {
                return (string.IsNullOrEmpty(CommonMessage) ? "" : CommonMessage + ": ") +
                       exceptions.Select(v => v.Message).ToStringJoin("; ");
            }
        }

        public bool HasError => Count > 0;

        public Exception this[int index]
        {
            get => exceptions[index];
            set => exceptions[index] = value;
        }

        public int Count => exceptions.Count;

        public bool IsReadOnly => exceptions.IsReadOnly;

        public void Clear()
        {
            exceptions.Clear();
        }

        public bool Contains(Exception item)
        {
            return exceptions.Contains(item);
        }

        public void CopyTo(Exception[] array, int arrayIndex)
        {
            exceptions.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Exception> GetEnumerator()
        {
            return exceptions.GetEnumerator();
        }

        public int IndexOf(Exception item)
        {
            return exceptions.IndexOf(item);
        }

        public void Insert(int index, Exception item)
        {
            exceptions.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            exceptions.RemoveAt(index);
        }

        public void Add(Exception item)
        {
            exceptions.Add(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return exceptions.GetEnumerator();
        }

        public bool Remove(Exception item)
        {
            return exceptions.Remove(item);
        }

        public override string ToString()
        {
            return (string.IsNullOrEmpty(CommonMessage) ? "" : CommonMessage + ": ") +
                   exceptions.Select(v => v.ToString()).ToStringJoin("\r\n");
        }

        public void Add(ExceptionList exceptions)
        {
            AddRange(exceptions);
        }

        public void AddRange(IList<Exception> items)
        {
            foreach (var item in items)
                exceptions.Add(item);
        }
    }
}