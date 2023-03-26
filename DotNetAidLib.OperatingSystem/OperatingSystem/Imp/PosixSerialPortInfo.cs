using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Streams;
using DotNetAidLib.OperatingSystem.Core;

namespace DotNetAidLib.OperatingSystem.Imp
{
    public class PosixSerialPortInfo : SerialPortInfo
    {
        private static readonly FileInfo udevadmFI = EnvironmentHelper.SearchInPath("udevadm");
        private static readonly Regex UDEVADM_REGEX = new Regex(@"^E:\s+([^=]+)=(.+)$", RegexOptions.Multiline);
        private static readonly FSInfo serialByPathDi = new FSInfo("/dev/serial/by-path");
        private static readonly FSInfo serialByIdDi = new FSInfo("/dev/serial/by-id");

        public static readonly IDictionary<SerialPortType, string> KNOWS_SERIAL_PORTS =
            new Dictionary<SerialPortType, string>
            {
                {SerialPortType.UART, @"ttyS\d+$"},
                {SerialPortType.USB, @"ttyUSB\d+$"},
                {SerialPortType.ACM, @"ttyACM\d+$"},
                {SerialPortType.ARM, @"ttyAMA\d+$"},
                {SerialPortType.UNKNOW, @"ttySAC\d+$"},
                {SerialPortType.BLUETOOTH, @"ttyHS\d+$"},
                {SerialPortType.BLUETOOTHLE, @"ttyHSL\d+$"}
            };

        public static bool INCLUDE_SERIAL_CONSOLE = false;
        private readonly IDictionary<string, object> deviceInfo = new Dictionary<string, object>();

        public PosixSerialPortInfo(string fullName)
            : base(new LabeledValue<string>(fullName, fullName.Split('/').Last()))
        {
            Refresh();
        }

        public override bool Enabled => true;

        public override bool Exists => SerialPort.GetPortNames().Contains(Name.Label);

        public override SerialPortType Type
        {
            get
            {
                var knowType = KNOWS_SERIAL_PORTS
                    .FirstOrDefault(v => Name.Label.RegexIsMatch(v.Value));

                return knowType.Key;
            }
        }

        public override LabeledValue<string> InvariantName
        {
            get
            {
                var ret = Name;

                if (DeviceId != null)
                    ret = DeviceId;
                else if (DevicePath != null)
                    ret = DevicePath;

                return ret;
            }
        }

        public override LabeledValue<string> DeviceId
        {
            get
            {
                if (DescriptorById != null)
                    return new LabeledValue<string>(DescriptorById, Path.GetFileName(DescriptorById));
                return null;
            }
        }

        private string DescriptorById
        {
            get
            {
                if (serialByIdDi.Exists)
                {
                    // Cambiado por error de Mono.Unix en NetStandard2.0
                    //UnixSymbolicLinkInfo p = serialByIdDi.GetFileSystemEntries()
                    //    .Where(v => v.FileType == FileTypes.SymbolicLink)
                    //    .Cast<UnixSymbolicLinkInfo>()
                    //    .Where(v => v.HasContents && v.GetContents().FullName == this.Name)
                    //    .FirstOrDefault();
                    var p = serialByIdDi.Content()
                        .Where(v => v.IsSymbolicLink)
                        .FirstOrDefault(v => v.GetTarget().FullName == Name);
                    if (p != null)
                        return p.FullName;
                }

                return null;
            }
        }

        public override LabeledValue<string> DevicePath
        {
            get
            {
                if (DescriptorByPath != null)
                    return new LabeledValue<string>(DescriptorByPath, Path.GetFileName(DescriptorByPath));
                return null;
            }
        }

        private string DescriptorByPath
        {
            get
            {
                if (serialByPathDi.Exists)
                {
                    var p = serialByPathDi.Content()
                        .Where(v => v.IsSymbolicLink)
                        .FirstOrDefault(v => v.GetTarget().FullName == Name);

                    // Cambiado por error de Mono.Unix en NetStandard2.0
                    //UnixSymbolicLinkInfo p = serialByPathDi.GetFileSystemEntries()
                    //    .Where(v => v.FileType == FileTypes.SymbolicLink)
                    //    .Cast<UnixSymbolicLinkInfo>()
                    //    .Where(v => v.HasContents && v.GetContents().FullName == this.Name)
                    //    .FirstOrDefault();
                    if (p != null)
                        return p.FullName;
                }

                return null;
            }
        }

        public override string DeviceVendorInfo
        {
            get
            {
                string ret = null;

                if (Type == SerialPortType.USB)
                {
                    if (deviceInfo.ContainsKey("ID_VENDOR_FROM_DATABASE"))
                        ret = (string) deviceInfo["ID_VENDOR_FROM_DATABASE"];
                    else if (deviceInfo.ContainsKey("ID_VENDOR"))
                        ret = (string) deviceInfo["ID_VENDOR"];
                }

                return ret;
            }
        }

        public override string DeviceModelInfo
        {
            get
            {
                string ret = null;

                if (Type == SerialPortType.USB)
                {
                    if (deviceInfo.ContainsKey("ID_MODEL_FROM_DATABASE"))
                        ret = (string) deviceInfo["ID_MODEL_FROM_DATABASE"];
                    else if (deviceInfo.ContainsKey("ID_MODEL"))
                        ret = (string) deviceInfo["ID_MODEL"];
                }
                else if (Type == SerialPortType.UART)
                {
                    if (deviceInfo.ContainsKey("ID_MODEL"))
                        ret = (string) deviceInfo["ID_MODEL"];
                }

                return ret;
            }
        }

        public bool IsConsole
        {
            get
            {
                var ret = false;

                // Cambiado por error de Mono.Unix en NetStandard2.0
                //UnixFileInfo f = new UnixFileInfo("/sys/class/tty/" + this.name.Label + "/port");
                var f = new FSInfo("/sys/class/tty/" + name.Label + "/port");
                if (f.Exists && "0x0".Equals(File.OpenText(f.FullName).ReadToEnd(true).Replace("\n", "")))
                    ret = true;

                return ret;
            }
        }

        public new static IEnumerable<SerialPortInfo> SerialPorts
        {
            get
            {
                IEnumerable<PosixSerialPortInfo> ret;

                IEnumerable<string> ttys;

                // Cambiado por error de Mono.Unix en NetStandard2.0
                //UnixDirectoryInfo d = new UnixDirectoryInfo("/sys/class/tty");
                //if (d.Exists)
                //    ttys = d.GetFileSystemEntries().Select(v => "/dev/" + v.Name);

                var d = new FSInfo("/sys/class/tty");
                if (d.Exists)
                    ttys = d.Content().Select(v => "/dev/" + v.Name);
                else
                    ttys = new DirectoryInfo("/dev").GetFiles("tty*").Select(v => v.FullName);

                ttys = ttys.Where(s => KNOWS_SERIAL_PORTS.Values.Any(v => s.RegexIsMatch(v)));

                ret = ttys.Select(v => new PosixSerialPortInfo(v));

                if (!INCLUDE_SERIAL_CONSOLE)
                    ret = ret.Where(v => !v.IsConsole);

                return ret;
            }
        }

        public override void Refresh()
        {
            try
            {
                deviceInfo.Clear();

                // udevInfo
                if (udevadmFI != null)
                {
                    var ss = udevadmFI.CmdExecuteSync("info --name=" + Name);
                    foreach (Match m in UDEVADM_REGEX.Matches(ss))
                        deviceInfo.Add(m.Groups[1].Value, m.Groups[2].Value);
                }

                if (Type == SerialPortType.UART)
                {
                    var pi = new FileInfo("/sys/class/tty/" + Path.GetFileName(Name) + "/port");
                    var ii = new FileInfo("/sys/class/tty/" + Path.GetFileName(Name) + "/irq");
                    if (pi.Exists && ii.Exists)
                        deviceInfo.AddOrUpdate("ID_PATH",
                            (pi.OpenText().ReadToEnd(true)
                             + ":"
                             + ii.OpenText().ReadToEnd(true))
                            .Replace("\n", "")
                        );
                }
            }
            catch (Exception ex)
            {
                throw new OperatingSystemException("Error refreshing serial port info", ex);
            }
        }

        public static DevicePathInfo GetPathInfo(string portName)
        {
            DevicePathInfo ret = null;

            var r = new Regex(@"\s{2}looking at( parent)? device '([^']+)':\n(\s{4}([^=]+)==""([^""]*)""\n)*",
                RegexOptions.Multiline);
            var udevadmFi = EnvironmentHelper.SearchInPath("udevadm");
            if (udevadmFI == null)
                return ret;

            var s = udevadmFI.CmdExecuteSync("info -a -n " + portName);
            var m = r.Match(s);
            if (m.Success)
            {
                ret = new DevicePathInfo();
                foreach (var v in r.Matches(s).Cast<Match>().Skip(1))
                {
                    IDictionary<string, string> p = v.Groups[4].Captures.Cast<Capture>()
                        .Select((c, i) => new KeyValuePair<string, string>(c.Value, v.Groups[5].Captures[i].Value))
                        .ToDictionary(kv => kv.Key, kv => kv.Value);

                    ret.Add(new DeviceSubPathInfo(v.Groups[2].Value, p));
                }
            }

            return ret;
        }

        public class DevicePathInfo : List<DeviceSubPathInfo>
        {
            public string Path => Count > 0 ? this[0].Path : null;

            public override string ToString()
            {
                return Path;
            }

            public string ToString(bool detailed)
            {
                if (detailed)
                    return Path + Environment.NewLine +
                           this.Select(v => v.ToString()).ToStringJoin(Environment.NewLine);
                return Path;
            }
        }

        public class DeviceSubPathInfo
        {
            private static readonly Regex attRegex = new Regex(@"ATTRS\{([^\}]+)\}");
            private readonly IDictionary<string, string> properties;

            public DeviceSubPathInfo(string path, IDictionary<string, string> properties)
            {
                Assert.NotNullOrEmpty(path, nameof(path));
                Assert.NotNullOrEmpty(properties, nameof(properties));

                Path = path;
                this.properties = properties;
            }

            public string Path { get; }

            public string Name => Helper.IfNull(properties.FirstOrDefault(kv => kv.Key.RegexIsMatch("KERNELS?")),
                v => null, v => v.Value);

            public string SubSystem =>
                Helper.IfNull(properties.FirstOrDefault(kv => kv.Key.RegexIsMatch("SUBSYSTEMS?")), v => null,
                    v => v.Value);

            public string Driver => Helper.IfNull(properties.FirstOrDefault(kv => kv.Key.RegexIsMatch("DRIVERS?")),
                v => null, v => v.Value);

            public IDictionary<string, string> Attributes => properties.Where(kv => attRegex.IsMatch(kv.Key))
                .ToDictionary(v => attRegex.GroupsMatches(v.Key)[1], v => v.Value);

            public override string ToString()
            {
                return "  " + Path + Environment.NewLine +
                       "    Name: " + Name + Environment.NewLine +
                       "    Subsystem: " + SubSystem + Environment.NewLine +
                       "    Driver: " + Driver + Environment.NewLine +
                       Attributes.Select(kv => "    " + kv.Key + ": " + kv.Value)
                           .ToStringJoin(Environment.NewLine);
            }
        }
    }
}