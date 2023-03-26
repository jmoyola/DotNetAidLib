using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.OperatingSystem.Core;
using Microsoft.Win32;

namespace DotNetAidLib.OperatingSystem.Imp
{
    public class WindowsSerialPortInfo : SerialPortInfo
    {
        private readonly IDictionary<string, object> deviceInfo = new Dictionary<string, object>();

        public WindowsSerialPortInfo(string fullName)
            : base(fullName)
        {
            Refresh();
        }

        public override SerialPortType Type
        {
            get
            {
                string id = DevicePath;
                if (id.IndexOf("ACPI", StringComparison.InvariantCultureIgnoreCase) > -1)
                    return SerialPortType.UART;
                if (id.IndexOf("USB", StringComparison.InvariantCultureIgnoreCase) > -1)
                    return SerialPortType.USB;
                if (id.IndexOf("BLUETOOTH", StringComparison.InvariantCultureIgnoreCase) > -1)
                    return SerialPortType.BLUETOOTH;
                if (id.IndexOf("ACM", StringComparison.InvariantCultureIgnoreCase) > -1)
                    return SerialPortType.ACM;
                return SerialPortType.UNKNOW;
            }
        }

        public override bool Enabled
        {
            get
            {
                if (deviceInfo.ContainsKey("Active"))
                    return (bool) deviceInfo["Active"];
                return false;
            }
        }

        public override bool Exists => SerialPort.GetPortNames().Contains(Name.Label);

        public override LabeledValue<string> DevicePath
        {
            get
            {
                if (deviceInfo.ContainsKey("InstanceName"))
                    return (string) deviceInfo["InstanceName"];
                return null;
            }
        }

        public override string DeviceVendorInfo
        {
            get
            {
                if (deviceInfo.ContainsKey("VendorInfo"))
                    return (string) deviceInfo["VendorInfo"];
                return null;
            }
        }

        public override string DeviceModelInfo
        {
            get
            {
                if (deviceInfo.ContainsKey("ModelInfo"))
                    return (string) deviceInfo["ModelInfo"];
                return null;
            }
        }

        public override LabeledValue<string> DeviceId => null;

        public new static IEnumerable<SerialPortInfo> SerialPorts
        {
            get
            {
                var serial_ports = new List<SerialPortInfo>();

                using (var subkey = Registry.LocalMachine.OpenSubKey("HARDWARE\\DEVICEMAP\\SERIALCOMM"))
                {
                    if (subkey != null)
                    {
                        var names = subkey.GetValueNames();
                        foreach (var value in names)
                        {
                            var port = subkey.GetValue(value, "").ToString();
                            if (port != "")
                                serial_ports.Add(new WindowsSerialPortInfo(port));
                        }
                    }
                }

                return serial_ports;
            }
        }

        public override void Refresh()
        {
            IEnumerable<ManagementObject> mos = null;
            try
            {
                deviceInfo.Clear();

                mos = WindowsOperatingSystem.GetWMIQueryObjects("root\\WMI",
                    "SELECT * FROM MSSerial_PortName WHERE PortName=\"" + Name + "\"", false);
                if (mos.Count() == 1)
                {
                    var mo = mos.FirstOrDefault();
                    deviceInfo.Add("PortName", (string) mo["PortName"]);
                    deviceInfo.Add("InstanceName", (string) mo["InstanceName"]);
                    deviceInfo.Add("Active", (bool) mo["Active"]);
                }

                /*
                if (this.Type == SerialPortType.USB) {
                    mos = WindowsOperatingSystem.GetWMIQueryObjects("root\\WMI", "SELECT * FROM Win32_PNPEntity WHERE PNPDeviceID like '" + this.Id + "'", false);
                    Console.WriteLine("------------------------------");
                    foreach(ManagementObject mo in mos) {
                        Console.WriteLine("-------");
                        Console.WriteLine(mo.PropertiesToDictionary().ToStringJoin("\r\n"));
                        Console.WriteLine("-------");
                    }
                    Console.WriteLine("------------------------------");
                }
                */
            }
            catch (Exception ex)
            {
                throw new OperatingSystemException("Error refreshing serial port info", ex);
            }
        }
    }
}