using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text.RegularExpressions;
using Library.OperatingSystem.Imp;
using System.Management;
using DotNetAidLib.Core.Helpers;
using Microsoft.Win32;
using Library.OperatingSystem.Core;

namespace Library.OperatingSystem.Imp
{
    public class WindowsSerialPortInfo:SerialPortInfo
    {
        private IDictionary<String, Object> deviceInfo = new Dictionary<String, Object>();

        public WindowsSerialPortInfo (String fullName)
            :base(fullName){
            this.Refresh ();
        }

        public override SerialPortType Type {
            get {
                String id = this.DevicePath;
                if (id.IndexOf("ACPI", StringComparison.InvariantCultureIgnoreCase) > -1)
                    return SerialPortType.UART;
                else if (id.IndexOf ("USB", StringComparison.InvariantCultureIgnoreCase) > -1)
                    return SerialPortType.USB;
                else if (id.IndexOf ("BLUETOOTH", StringComparison.InvariantCultureIgnoreCase) > -1)
                    return SerialPortType.BLUETOOTH;
                else if (id.IndexOf ("ACM", StringComparison.InvariantCultureIgnoreCase) > -1)
                    return SerialPortType.ACM;
                else
                    return SerialPortType.UNKNOW;
            }
        }

        public override bool Enabled {
            get {
                if (deviceInfo.ContainsKey ("Active"))
                    return (bool)this.deviceInfo ["Active"];
                return false;
            }
        }

        public override bool Exists
        {
            get
            {
                return System.IO.Ports.SerialPort.GetPortNames().Contains(this.Name.Label);
            }
        }
        public override LabeledValue<String> DevicePath {
            get {
                if (deviceInfo.ContainsKey("InstanceName"))
                    return (string)this.deviceInfo["InstanceName"];
                return null;
            }
        }

        public override string DeviceVendorInfo {
            get {
                if (deviceInfo.ContainsKey ("VendorInfo"))
                    return (string)this.deviceInfo ["VendorInfo"];
                return null;
            }
        }

        public override string DeviceModelInfo {
            get {
                if (deviceInfo.ContainsKey ("ModelInfo"))
                    return (string)this.deviceInfo ["ModelInfo"];
                return null;
            }
        }

        public override LabeledValue<String> DeviceId => null;

        public override void Refresh(){
            IEnumerable<ManagementObject> mos = null;
            try {
                deviceInfo.Clear ();

                mos =WindowsOperatingSystem.GetWMIQueryObjects ("root\\WMI", "SELECT * FROM MSSerial_PortName WHERE PortName=\"" + this.Name + "\"", false);
                if (mos.Count () == 1) {
                    ManagementObject mo = mos.FirstOrDefault ();
                    deviceInfo.Add("PortName", (String)mo ["PortName"]);
                    deviceInfo.Add ("InstanceName", (String)mo ["InstanceName"]);
                    deviceInfo.Add ("Active", (bool)mo ["Active"]);
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
            catch (Exception ex) {
                throw new OperatingSystemException ("Error refreshing serial port info", ex);
            }
        }

        public new static IEnumerable<SerialPortInfo> SerialPorts
        {
            get
            {
                List<SerialPortInfo> serial_ports = new List<SerialPortInfo>();

                using (RegistryKey subkey = Registry.LocalMachine.OpenSubKey("HARDWARE\\DEVICEMAP\\SERIALCOMM"))
                {
                    if (subkey != null)
                    {
                        string[] names = subkey.GetValueNames();
                        foreach (string value in names)
                        {
                            string port = subkey.GetValue(value, "").ToString();
                            if (port != "")
                                serial_ports.Add(new WindowsSerialPortInfo(port));
                        }
                    }
                }

                return serial_ports;
            }
        }
    }
}



