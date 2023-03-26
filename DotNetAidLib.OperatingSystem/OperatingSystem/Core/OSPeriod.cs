using System;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.OperatingSystem.Core
{
    public class OSPeriod
    {
        private DateTime? to;

        public OSPeriod(string type, string kernel, DateTime from, DateTime? to)
        {
            Assert.NotNullOrEmpty(type, nameof(type));
            Type = type;
            Kernel = kernel;
            From = from;
            this.to = to;
        }

        public string Type { get; }

        public string Kernel { get; }

        public DateTime From { get; }

        public DateTime? To => to;

        public override string ToString()
        {
            return Type
                   + (string.IsNullOrEmpty(Kernel) ? "" : " " + Kernel)
                   + " " + From + " - " + (to.HasValue ? to.ToString() : "now");
        }
    }
}