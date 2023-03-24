using System;
using System.Collections.Generic;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
namespace DotNetAidLib.Core.Process.ProcessInfo.Core
{
    public class BaseProcessArgument
    {
        private String name;
        public BaseProcessArgument(String name)
        {
            this.Name = name;
        }

        public string Name
        {
            get { return name; }
            set
            {
                Assert.NotNullOrEmpty( value, nameof(value));
                name = value;
            }
        }

        public override string ToString()
        {
            return this.name;
        }
    }

    public class ProcessArgument:BaseProcessArgument
    {
        private Object _value;
        public ProcessArgument(String name, Object value)
           :base(name)
        {
            this.Value = value;
        }

        public Object Value
        {
            get { return _value; }
            set
            {
                _value = value;
            }
        }

        public override string ToString() {
            return this.ToString(":");
        }

        public string ToString(string assign)
        {
            return base.ToString() + assign + (this.Value==null?"<null>":this.Value.ToString());
        }
    }

    public class ProcessArguments:List<BaseProcessArgument>
    {
        private string separator = " ";
        private string prefix = "-";
        private string assign = "=";

        public ProcessArguments()
            :this(" ", "-", "=")
        {
        }

        public ProcessArguments(string separator, string prefix, string assign)
        {
            this.Prefix = prefix;
            this.Assign = assign;
            this.Separator = separator;
        }

        public string Prefix
        {
            get { return prefix; }
            set
            {
                prefix = value;
            }
        }

        public string Assign
        {
            get { return assign; }
            set
            {
                Assert.NotNullOrEmpty( value, nameof(value));
                assign = value;
            }
        }

        public string Separator
        {
            get { return separator; }
            set
            {
                Assert.NotNullOrEmpty( value, nameof(value));
                separator = value;
            }
        }

        public void Add(String name) {
            this.Add(new BaseProcessArgument(name));
        }

        public void Add(String name, Object value)
        {
            this.Add(new ProcessArgument(name, value));
        }

        public override string ToString()
        {
            return this.ToStringJoin(this.separator,
                (v) => this.prefix + (v is ProcessArgument?((ProcessArgument)v).ToString(this.assign):v.ToString()));
        }
    }
}
