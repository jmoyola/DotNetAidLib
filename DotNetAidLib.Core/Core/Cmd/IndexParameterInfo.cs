using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Cmd
{
    public class IndexParameterInfo : ParameterInfo
    {
        public IndexParameterInfo(int index, string description)
            : this(index, description, v => v)
        {
        }

        public IndexParameterInfo(int index, string description, Func<string, object> castFunction)
            : base(description, false, null, castFunction)
        {
            Assert.GreaterThan(index, -1, nameof(index));
            Assert.NotNullOrEmpty(description, nameof(description));

            Index = index;
        }

        public int Index { get; } = -1;

        public override string Name => "#" + Index.ToString("00");

        public override string ToString()
        {
            return "<" + Name + "> " + "\t" + base.ToString();
        }
    }
}