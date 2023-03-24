using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;
using Library.OperatingSystem.Imp;
namespace Library.OperatingSystem.Core
{
    public enum SerialPortType {
        UNKNOW,
        UART,
        USB,
        ACM,
        ARM,
        CONSOLE,
        BLUETOOTH,
        BLUETOOTHLE,
    }

    public abstract class SerialPortInfo
    {
        protected readonly LabeledValue<String> name;

        public SerialPortInfo(LabeledValue<String> fullName)
        {
            Assert.NotNull ( fullName, nameof(fullName));

            this.name = fullName;
        }

        public virtual LabeledValue<String> InvariantName { get => this.Name; }
        public LabeledValue<String> Name { get => name; }
        public abstract bool Enabled { get; }
        public abstract SerialPortType Type { get; }
        public abstract LabeledValue<String> DeviceId { get; }
        public abstract LabeledValue<String> DevicePath { get; }
        public abstract string DeviceVendorInfo { get; }
        public abstract string DeviceModelInfo { get; }
        public abstract void Refresh ();

        public abstract bool Exists { get; }

        public override string ToString() {
            return this.ToString(false);
        }

        public string ToString(bool detailed)
        {
            return this.name + " (" + this.name.Label + ")" +
                (this.DeviceId == null ? "" : ", Id: " + this.DeviceId.ToString(true)) +
                (this.DevicePath == null ? "" : ", Path: " + this.DevicePath.ToString(true)) +
                (this.DeviceModelInfo == null ? "" : " Model: " + this.DeviceModelInfo) +
                (this.DeviceVendorInfo == null ? "" : ", Vendor: " + this.DeviceVendorInfo) +
                (this.InvariantName == null ? "" : ", InvariantName: " + this.InvariantName.ToString(true))
            ;

        }

        public override int GetHashCode()
        {
            return this.name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && typeof(SerialPortInfo).IsAssignableFrom(obj.GetType()))
                return this.name.Equals(((SerialPortInfo)obj).Name);
            else if (obj != null && typeof(String).IsAssignableFrom(obj.GetType()))
                return this.name.Equals(obj.ToString());
            return false;
        }

        public static SerialPortInfo ByName(String name)
        {
            return SerialPorts.FirstOrDefault(v => v.Name.Equals(name));
        }

        public static SerialPortInfo ByInvariantName(String invariantName)
        {
            return SerialPorts.FirstOrDefault(v=>v.InvariantName.Equals(invariantName));
        }


        public static IEnumerable<SerialPortInfo> SerialPorts {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return WindowsSerialPortInfo.SerialPorts;
                else
                    return PosixSerialPortInfo.SerialPorts;
            }
        }
    }
}
