using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Streams;
using Library.OperatingSystem.Core;

namespace Library.OperatingSystem.Imp
{
    public class PosixSerialPortInfo:SerialPortInfo
    {
        private static FileInfo udevadmFI = EnvironmentHelper.SearchInPath("udevadm");
        private static Regex UDEVADM_REGEX = new Regex(@"^E:\s+([^=]+)=(.+)$", RegexOptions.Multiline);
        private IDictionary<String, Object> deviceInfo = new Dictionary<String, Object>();
        private static FSInfo serialByPathDi = new FSInfo("/dev/serial/by-path");
        private static FSInfo serialByIdDi = new FSInfo("/dev/serial/by-id");

        public PosixSerialPortInfo (String fullName)
            :base(new LabeledValue<string>(fullName, fullName.Split('/').Last())){
            this.Refresh ();
        }

        public override bool Enabled {
            get {
                return true;
            }
        }

        public override bool Exists
        {
            get
            {
                return System.IO.Ports.SerialPort.GetPortNames().Contains(this.Name.Label);
            }
        }
        
        public static readonly IDictionary<SerialPortType, String> KNOWS_SERIAL_PORTS =
            new Dictionary<SerialPortType, String>{
                { SerialPortType.UART, @"ttyS\d+$" },
                { SerialPortType.USB,  @"ttyUSB\d+$" },
                { SerialPortType.ACM,  @"ttyACM\d+$" },
                { SerialPortType.ARM,  @"ttyAMA\d+$" },
                { SerialPortType.UNKNOW, @"ttySAC\d+$"} ,
                { SerialPortType.BLUETOOTH, @"ttyHS\d+$" },
                { SerialPortType.BLUETOOTHLE, @"ttyHSL\d+$" },
            };

        public override SerialPortType Type {
            get {
                KeyValuePair<SerialPortType, String> knowType = KNOWS_SERIAL_PORTS
                    .FirstOrDefault(v => this.Name.Label.RegexIsMatch(v.Value));

                return knowType.Key;
            }
        }

        public override LabeledValue<String> InvariantName {
            get {
                LabeledValue<String> ret = this.Name;

                if (this.DeviceId != null)
                    ret = this.DeviceId;
                else if (this.DevicePath != null)
                    ret = this.DevicePath;

                return ret;
            }
        }

        public override LabeledValue<String> DeviceId {
            get
            {
                if (this.DescriptorById != null)
                    return new LabeledValue<string>(this.DescriptorById, Path.GetFileName(this.DescriptorById));
                else
                    return null;
            }
        }

        private string DescriptorById {
            get {
                if (serialByIdDi.Exists)
                {
                    // Cambiado por error de Mono.Unix en NetStandard2.0
                    //UnixSymbolicLinkInfo p = serialByIdDi.GetFileSystemEntries()
                    //    .Where(v => v.FileType == FileTypes.SymbolicLink)
                    //    .Cast<UnixSymbolicLinkInfo>()
                    //    .Where(v => v.HasContents && v.GetContents().FullName == this.Name)
                    //    .FirstOrDefault();
                    FSInfo p = serialByIdDi.Content()
                        .Where(v => v.IsSymbolicLink)
                        .FirstOrDefault(v => v.GetTarget().FullName == this.Name);
                    if (p != null)
                        return p.FullName;
                }
                        
                return null;
            }
        }

        public override LabeledValue<String> DevicePath
        {
            get
            {
                if (this.DescriptorByPath != null)
                    return new LabeledValue<string>(this.DescriptorByPath, Path.GetFileName(this.DescriptorByPath));
                else
                    return null;
            }
        }

        private string DescriptorByPath {
            get {

                if (serialByPathDi.Exists){
                    FSInfo p = serialByPathDi.Content()
                        .Where(v => v.IsSymbolicLink)
                        .FirstOrDefault(v => v.GetTarget().FullName == this.Name);

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

        public override string DeviceVendorInfo {
            get {
                String ret = null;

                if (this.Type == SerialPortType.USB) {
                    if (deviceInfo.ContainsKey ("ID_VENDOR_FROM_DATABASE"))
                        ret = (string)deviceInfo ["ID_VENDOR_FROM_DATABASE"];
                    else if (deviceInfo.ContainsKey ("ID_VENDOR"))
                        ret = (string)deviceInfo ["ID_VENDOR"];
                }
                return ret;
            }
        }

        public override string DeviceModelInfo {
            get {
                String ret = null;

                if (this.Type == SerialPortType.USB) {
                    if (deviceInfo.ContainsKey ("ID_MODEL_FROM_DATABASE"))
                        ret = (string)deviceInfo ["ID_MODEL_FROM_DATABASE"];
                    else if (deviceInfo.ContainsKey ("ID_MODEL"))
                        ret = (string)deviceInfo ["ID_MODEL"];
                } else if (this.Type == SerialPortType.UART) {
                    if (deviceInfo.ContainsKey ("ID_MODEL"))
                        ret = (string)deviceInfo ["ID_MODEL"];
                }
                return ret;
            }
        }

        public override void Refresh(){
            try{
                deviceInfo.Clear ();

                // udevInfo
                if (udevadmFI != null) {
                    String ss = udevadmFI.CmdExecuteSync ("info --name=" + this.Name);
                    foreach (Match m in UDEVADM_REGEX.Matches (ss))
                        deviceInfo.Add (m.Groups [1].Value, m.Groups [2].Value);
                }

                if (this.Type == SerialPortType.UART) {
                    FileInfo pi = new FileInfo("/sys/class/tty/" + System.IO.Path.GetFileName(this.Name) + "/port");
                    FileInfo ii = new FileInfo("/sys/class/tty/" + System.IO.Path.GetFileName(this.Name) + "/irq");
                    if (pi.Exists && ii.Exists)
                        deviceInfo.AddOrUpdate("ID_PATH",
                            (pi.OpenText().ReadToEnd(true)
                                 + ":"
                                 + ii.OpenText().ReadToEnd(true))
                                 .Replace("\n", "")
                         );
                }

            } catch (Exception ex) {
                throw new OperatingSystemException ("Error refreshing serial port info", ex);
            }
        }

        public bool IsConsole {
            get
            {
                bool ret = false;

                // Cambiado por error de Mono.Unix en NetStandard2.0
                //UnixFileInfo f = new UnixFileInfo("/sys/class/tty/" + this.name.Label + "/port");
                FSInfo f = new FSInfo("/sys/class/tty/" + this.name.Label + "/port");
                if (f.Exists && "0x0".Equals(File.OpenText(f.FullName).ReadToEnd(true).Replace("\n", "")))
                    ret = true;

                return ret;
            }
        }

        public static bool INCLUDE_SERIAL_CONSOLE = false;

        public new static IEnumerable<SerialPortInfo> SerialPorts
        {
            get
            {
                IEnumerable<PosixSerialPortInfo> ret;

                IEnumerable<String> ttys;
                
                // Cambiado por error de Mono.Unix en NetStandard2.0
                //UnixDirectoryInfo d = new UnixDirectoryInfo("/sys/class/tty");
                //if (d.Exists)
                //    ttys = d.GetFileSystemEntries().Select(v => "/dev/" + v.Name);
                
                FSInfo d = new FSInfo("/sys/class/tty");
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

        public class DevicePathInfo:List<DeviceSubPathInfo>
        {
            public DevicePathInfo()
            {
            }

            public string Path { get => (this.Count>0?this[0].Path:null); }

            public override string ToString()
            {
                return this.Path;
            }

            public string ToString(bool detailed)
            {
                if(detailed)
                    return this.Path + Environment.NewLine + this.Select(v=>v.ToString()).ToStringJoin(Environment.NewLine);
                else
                    return this.Path;
            }
        }

        public class DeviceSubPathInfo
        {
            private static Regex attRegex = new Regex(@"ATTRS\{([^\}]+)\}");
            private String path;
            private IDictionary<String, String> properties;

            public DeviceSubPathInfo(String path, IDictionary<String, String> properties) {
                Assert.NotNullOrEmpty( path, nameof(path));
                Assert.NotNullOrEmpty( properties, nameof(properties));

                this.path = path;
                this.properties = properties;
            }

            public string Path { get => path; }
            public string Name { get => Helper.IfNull(this.properties.FirstOrDefault(kv => kv.Key.RegexIsMatch("KERNELS?")), (v) => null, (v) => v.Value); }
            public string SubSystem { get => Helper.IfNull(this.properties.FirstOrDefault(kv => kv.Key.RegexIsMatch("SUBSYSTEMS?")), (v) => null, (v) => v.Value); }
            public string Driver { get => Helper.IfNull(this.properties.FirstOrDefault(kv => kv.Key.RegexIsMatch("DRIVERS?")), (v) => null, (v) => v.Value); }
            public IDictionary<string, string> Attributes { get => this.properties.Where(kv=> attRegex.IsMatch(kv.Key)).ToDictionary(v=>attRegex.GroupsMatches(v.Key)[1], v=>v.Value); }

            public override string ToString()
            {
                return "  " + this.path + Environment.NewLine +
                    "    Name: " + this.Name + Environment.NewLine +
                    "    Subsystem: " + this.SubSystem + Environment.NewLine +
                    "    Driver: " + this.Driver + Environment.NewLine +
                    this.Attributes.Select(kv=>"    " + kv.Key + ": " + kv.Value)
                    .ToStringJoin(Environment.NewLine);
            }
        }

        public static DevicePathInfo GetPathInfo(String portName) {
            DevicePathInfo ret = null;

            Regex r = new Regex(@"\s{2}looking at( parent)? device '([^']+)':\n(\s{4}([^=]+)==""([^""]*)""\n)*", RegexOptions.Multiline);
            FileInfo udevadmFi = EnvironmentHelper.SearchInPath("udevadm");
            if (udevadmFI == null)
                return ret;

            String s=udevadmFI.CmdExecuteSync("info -a -n " + portName);
            Match m = r.Match(s);
            if (m.Success) {
                ret = new DevicePathInfo();
                foreach (Match v in r.Matches(s).Cast<Match>().Skip(1)) {
                    IDictionary<String, String> p = v.Groups[4].Captures.Cast<Capture>()
                    .Select((c, i) => new KeyValuePair<String, String>(c.Value, v.Groups[5].Captures[i].Value))
                    .ToDictionary(kv => kv.Key, kv => kv.Value);
                    
                    ret.Add(new DeviceSubPathInfo(v.Groups[2].Value, p));
                }
            }

            return ret;
        }
    }
}
