using System.Collections.Generic;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Process.ProcessInfo.Core
{
    public class BaseProcessArgument
    {
        private string name;

        public BaseProcessArgument(string name)
        {
            Name = name;
        }

        public string Name
        {
            get => name;
            set
            {
                Assert.NotNullOrEmpty(value, nameof(value));
                name = value;
            }
        }

        public override string ToString()
        {
            return name;
        }
    }

    public class ProcessArgument : BaseProcessArgument
    {
        public ProcessArgument(string name, object value)
            : base(name)
        {
            Value = value;
        }

        public object Value { get; set; }

        public override string ToString()
        {
            return ToString(":");
        }

        public string ToString(string assign)
        {
            return base.ToString() + assign + (Value == null ? "<null>" : Value.ToString());
        }
    }

    public class ProcessArguments : List<BaseProcessArgument>
    {
        private string assign = "=";
        private string separator = " ";

        public ProcessArguments()
            : this(" ", "-", "=")
        {
        }

        public ProcessArguments(string separator, string prefix, string assign)
        {
            Prefix = prefix;
            Assign = assign;
            Separator = separator;
        }

        public string Prefix { get; set; } = "-";

        public string Assign
        {
            get => assign;
            set
            {
                Assert.NotNullOrEmpty(value, nameof(value));
                assign = value;
            }
        }

        public string Separator
        {
            get => separator;
            set
            {
                Assert.NotNullOrEmpty(value, nameof(value));
                separator = value;
            }
        }

        public void Add(string name)
        {
            Add(new BaseProcessArgument(name));
        }

        public void Add(string name, object value)
        {
            Add(new ProcessArgument(name, value));
        }

        public override string ToString()
        {
            return this.ToStringJoin(separator,
                v => Prefix + (v is ProcessArgument ? ((ProcessArgument) v).ToString(assign) : v.ToString()));
        }
    }
}