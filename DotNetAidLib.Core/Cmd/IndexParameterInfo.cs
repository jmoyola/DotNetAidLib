using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Cmd
{
    public class IndexParameterInfo: ParameterInfo
    {
        private int index=-1;

        public IndexParameterInfo(int index, String description)
            :this(index, description, v=>v){ }

        public IndexParameterInfo(int index, String description, Func<String, Object> castFunction)
            :base(description, false, null, castFunction)
        {
            Assert.GreaterThan(index, -1, nameof(index));
            Assert.NotNullOrEmpty(description, nameof(description));

            this.index = index;
        }

        public int Index { get => index;}

        public override string Name => "#" + this.index.ToString("00");

        public override string ToString ()
        {
            return "<" + this.Name + "> " + "\t" + base.ToString();
        }
    }
}
