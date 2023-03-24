using System;
using System.Runtime.CompilerServices;
using DotNetAidLib.Core.Develop;

namespace Library.OperatingSystem.Core
{
    public class OSDistribution
    {
        private String id;
        private String architecture;
        private String operatingSystem;
        private String release;
        private String revision;
        private String codename;
        private String kernel;

        public OSDistribution(String id, String architecture, String operatingSystem, String release, String revision, String codename, String kernel)
        {
            Assert.NotNullOrEmpty( id, nameof(id));
            Assert.NotNullOrEmpty( architecture, nameof(architecture));
            Assert.NotNullOrEmpty( operatingSystem, nameof(operatingSystem));
            Assert.NotNullOrEmpty( release, nameof(release));
            //Assert.NotNullOrEmpty( revision, nameof(revision));
            Assert.NotNullOrEmpty( codename, nameof(codename));
            Assert.NotNullOrEmpty( kernel, nameof(kernel));

            this.id = id;
            this.architecture = architecture;
            this.operatingSystem = operatingSystem;
            this.release = release;
            this.codename = codename;
            this.kernel = kernel;
            this.revision = revision;
        }

        public string Id { get => id; }
        public string OperatingSystem { get => operatingSystem; }
        public string Release { get => release; }
        public string Revision { get => revision; }
        public string Codename { get => codename; }
        public string Architecture { get => architecture;}
        public string Kernel { get => kernel; }

        public override string ToString()
        {
            return this.id + " " + this.operatingSystem + " " + this.release + (String.IsNullOrEmpty(this.revision)?"": "." + this.revision) + " (" + this.codename + ")" + " " + this.architecture + " (" + this.kernel + ")";
        }
    }

}
