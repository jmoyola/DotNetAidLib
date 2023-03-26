using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.OperatingSystem.Imp;

namespace DotNetAidLib.OperatingSystem.Core
{
    public enum SerialPortType
    {
        UNKNOW,
        UART,
        USB,
        ACM,
        ARM,
        CONSOLE,
        BLUETOOTH,
        BLUETOOTHLE
    }

    public abstract class SerialPortInfo
    {
        protected readonly LabeledValue<string> name;

        public SerialPortInfo(LabeledValue<string> fullName)
        {
            Assert.NotNull(fullName, nameof(fullName));

            name = fullName;
        }

        public virtual LabeledValue<string> InvariantName => Name;
        public LabeledValue<string> Name => name;
        public abstract bool Enabled { get; }
        public abstract SerialPortType Type { get; }
        public abstract LabeledValue<string> DeviceId { get; }
        public abstract LabeledValue<string> DevicePath { get; }
        public abstract string DeviceVendorInfo { get; }
        public abstract string DeviceModelInfo { get; }

        public abstract bool Exists { get; }


        public static IEnumerable<SerialPortInfo> SerialPorts
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return WindowsSerialPortInfo.SerialPorts;
                return PosixSerialPortInfo.SerialPorts;
            }
        }

        public abstract void Refresh();

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool detailed)
        {
            return name + " (" + name.Label + ")" +
                   (DeviceId == null ? "" : ", Id: " + DeviceId.ToString(true)) +
                   (DevicePath == null ? "" : ", Path: " + DevicePath.ToString(true)) +
                   (DeviceModelInfo == null ? "" : " Model: " + DeviceModelInfo) +
                   (DeviceVendorInfo == null ? "" : ", Vendor: " + DeviceVendorInfo) +
                   (InvariantName == null ? "" : ", InvariantName: " + InvariantName.ToString(true))
                ;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && typeof(SerialPortInfo).IsAssignableFrom(obj.GetType()))
                return name.Equals(((SerialPortInfo) obj).Name);
            if (obj != null && typeof(string).IsAssignableFrom(obj.GetType()))
                return name.Equals(obj.ToString());
            return false;
        }

        public static SerialPortInfo ByName(string name)
        {
            return SerialPorts.FirstOrDefault(v => v.Name.Equals(name));
        }

        public static SerialPortInfo ByInvariantName(string invariantName)
        {
            return SerialPorts.FirstOrDefault(v => v.InvariantName.Equals(invariantName));
        }
    }
}