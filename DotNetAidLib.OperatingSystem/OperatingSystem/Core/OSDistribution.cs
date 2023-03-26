using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.OperatingSystem.Core
{
    public class OSDistribution
    {
        public OSDistribution(string id, string architecture, string operatingSystem, string release, string revision,
            string codename, string kernel)
        {
            Assert.NotNullOrEmpty(id, nameof(id));
            Assert.NotNullOrEmpty(architecture, nameof(architecture));
            Assert.NotNullOrEmpty(operatingSystem, nameof(operatingSystem));
            Assert.NotNullOrEmpty(release, nameof(release));
            //Assert.NotNullOrEmpty( revision, nameof(revision));
            Assert.NotNullOrEmpty(codename, nameof(codename));
            Assert.NotNullOrEmpty(kernel, nameof(kernel));

            Id = id;
            Architecture = architecture;
            OperatingSystem = operatingSystem;
            Release = release;
            Codename = codename;
            Kernel = kernel;
            Revision = revision;
        }

        public string Id { get; }

        public string OperatingSystem { get; }

        public string Release { get; }

        public string Revision { get; }

        public string Codename { get; }

        public string Architecture { get; }

        public string Kernel { get; }

        public override string ToString()
        {
            return Id + " " + OperatingSystem + " " + Release + (string.IsNullOrEmpty(Revision) ? "" : "." + Revision) +
                   " (" + Codename + ")" + " " + Architecture + " (" + Kernel + ")";
        }
    }
}