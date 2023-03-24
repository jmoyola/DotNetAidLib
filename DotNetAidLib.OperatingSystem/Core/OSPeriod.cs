using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;
using Library.AAA.Core;
using Library.AAA.Imp;

namespace Library.OperatingSystem.Core
{
    public class OSPeriod {
        private String type;
        private String kernel;
        private DateTime from;
        private DateTime? to;

        public OSPeriod(String type, String kernel, DateTime from, DateTime? to){
            Assert.NotNullOrEmpty( type, nameof(type));
            this.type = type;
            this.kernel = kernel;
            this.from = from;
            this.to = to;
        }

        public string Type { get => type;}
        public string Kernel { get => kernel;}
        public DateTime From { get => from;}
        public DateTime? To { get => to;}

        public override string ToString()
        {
            return this.type
                + (String.IsNullOrEmpty(this.kernel)?"": " " + this.kernel)
                + " " + this.from.ToString() + " - " + (this.to.HasValue?this.to.ToString():"now");
        }
    }
}

