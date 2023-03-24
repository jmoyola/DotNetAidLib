using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Text;
using DotNetAidLib.Core.Network.Config.Core;
using DotNetAidLib.Core.Network.Config.TcpIp.Core;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Enums;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Process.ServiceDaemon.Core;
using DotNetAidLib.Core.Streams;
using Library.OperatingSystem.Imp;

namespace DotNetAidLib.Core.Network.Config.Imp
{
    public class DebianNetworkConfig : NetworkConfig
    {
        private static IDictionary<String, NetworkConfig> _Instances = new Dictionary<String, NetworkConfig>();

        // Crear el usuario que podrá reiniciar el servicio creando un archivo sudoers '/etc/sudoers.d/<NombreUsuario>':
        // cisco ALL=NOPASSWD: /etc/init.d/networking
        // No olvidar poner permisos al archivo: sudo chmod 440 /etc/sudoers.d/<NombreUsuario>
        //
        private FileInfo _SudoCommandFile = new FileInfo("/usr/bin/sudo");
        private FileInfo _InterfacesFile = null;

        private DebianNetworkConfig(FileInfo interfacesFile)
            : base()
        {
            Assert.NotNull ( interfacesFile, nameof(interfacesFile));
            this._InterfacesFile = interfacesFile;
        }

        private bool AllowMultiGateway { get; set; } = false;

        private Object oLock = new object();
        public override void Save()
        {
            lock (oLock)
            {
                StringBuilder sw = null;
                try
                {
                    sw = new StringBuilder();
                    foreach (InterfaceConfig iface in this)
                    {
                        foreach (InterfaceConfigAddress address in iface.Addresses)
                        {
                            if (address.Enabled)
                                sw.AppendLine("auto " + iface.Name);
                            if (address.HotPlug)
                                sw.AppendLine("allow-hotplug " + iface.Name);

                            sw.AppendLine("iface"
                                + " " + iface.Name
                                + (address.AddressFamily.Equals(AddressFamily.InterNetworkV6) ? " inet6" : " inet")
                                + " " + address.ProvisioningType.ToString().ToLower());

                            foreach (KeyValue<String, String> kv in address.Attributes)
                                if (kv.Value == null)
                                    sw.AppendLine("    " + kv.Key);
                                else
                                    sw.AppendLine("    " + kv.Key + " " + kv.Value);

                            sw.AppendLine("");
                        }
                    }

                    FileInfo tmpInterfacesFile = new FileInfo(".").RandomTempFile(".tmp");
                    System.IO.File.AppendAllText(tmpInterfacesFile.FullName, sw.ToString());
                    LinuxOperatingSystem.Instance().AdminExecute(EnvironmentHelper.SearchInPath("chown"), "root:root " + tmpInterfacesFile.FullName);
                    LinuxOperatingSystem.Instance().AdminExecute(EnvironmentHelper.SearchInPath("chmod"), "775 " + tmpInterfacesFile.FullName);
                    LinuxOperatingSystem.Instance().AdminExecute(EnvironmentHelper.SearchInPath("mv"), _InterfacesFile.FullName + " " + _InterfacesFile.FullName + ".tmp");
                    LinuxOperatingSystem.Instance().AdminExecute(EnvironmentHelper.SearchInPath("mv"), tmpInterfacesFile.FullName + " " + _InterfacesFile.FullName);

                    //OperatingSystem.Imp.LinuxOperatingSystem.Instance().AdminTextWrite(_InterfacesFile, sw.ToString());

                    _InterfacesFile.Refresh();
                }
                catch (Exception ex)
                {
                    throw new NetworkingException("Error saving interfaces configuration.", ex);
                }
            }
        }

        private static Regex ifaceReg = new Regex(@"^\s*iface\s+([a-zA-Z0-9\.\:]+)\s+([^\s]+)\s+([^\s]+)\s*$");
        private static Regex ifaceProperty = new Regex(@"^(\t|\s)*([a-zA-Z0-9_\-]+)\s+([^\n]+)$");

        public override void Load()
        {
            lock (oLock)
            {
                try
                {
                    StreamReader sr = _InterfacesFile.OpenText();
                    String intFile = sr.ReadToEnd(true);

                    this.Clear();

                    InterfaceConfig interf = null;
                    InterfaceConfigAddress interfAddress=null;
                    foreach (String line in intFile.GetLines().SlashEndContinueLines()
                                            .Select(v => v.Trim())
                                            .Where(v => !String.IsNullOrEmpty(v)
                                                 && !v.StartsWith("#", StringComparison.InvariantCulture)))
                    {
                        Match ifaceRegMatch = ifaceReg.Match(line);
                        Match ifacePropertyMatch = ifaceProperty.Match(line);

                        if (ifaceRegMatch.Success)
                        {
                            AddressFamily addressFamily;
                            if (ifaceRegMatch.Groups[2].Value.Equals("inet"))
                                addressFamily = AddressFamily.InterNetwork;
                            else if (ifaceRegMatch.Groups[2].Value.Equals("inet6"))
                                addressFamily = AddressFamily.InterNetworkV6;
                            else
                                addressFamily = AddressFamily.Unspecified;

                            String sInterfaceAddress = ifaceRegMatch.Groups[1].Value;
                            interf = this.FirstOrDefault(v=>v.Name.Equals(sInterfaceAddress));
                            if (interf == null){
                                interf = new InterfaceConfig(sInterfaceAddress);
                                this.Add(interf);
                            }

                            if(addressFamily.IsAnyOf(AddressFamily.InterNetwork, AddressFamily.InterNetworkV6))
                                interfAddress = new DebianInterfaceConfigIPAddress(addressFamily);
                            else
                                interfAddress = new InterfaceConfigAddress(addressFamily);

                            interf.Addresses.Add(interfAddress);

                            interfAddress.ProvisioningType = (ProvisioningType)Enum.Parse(typeof(TCPInterfaceConfigType), ifaceRegMatch.Groups[3].Value.Trim(), true);

                            interfAddress.Enabled = Regex.IsMatch(intFile, @"(allow\-)?auto\s+" + interf.Name, RegexOptions.Multiline);
                            interfAddress.HotPlug = Regex.IsMatch(intFile, @"(allow\-)?hotplug\s+" + interf.Name, RegexOptions.Multiline);

                            continue;
                        }
                        else if (ifacePropertyMatch.Success && interf != null)
                        {
                            String key = ifacePropertyMatch.Groups[2].Value;
                            String value = ifacePropertyMatch.Groups[3].Value;

                            if (Regex.IsMatch(key, @"(allow\-)?auto")
                                || Regex.IsMatch(key, @"(allow\-)?hotplug"))
                                continue;

                            interfAddress.Attributes.Add(key, value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new NetworkingException("Error loading interfaces configuration.", ex);
                }
            }
        }

        public override void ApplyAll()
        {
            lock (oLock)
            {
                ServiceHelper.Instance().GetService("networking").Restart(30000);
            }
        }

        public override void Apply(InterfaceConfig networkInterface)
        {
            lock (oLock)
            {
                networkInterface.NetworkInterfaceInfo.Disable();
                this.Save();
                networkInterface.NetworkInterfaceInfo.Enable();
            }
        }

        public new static NetworkConfig Instance()
        {
            return Instance(new FileInfo("/etc/network/interfaces"));
        }

        public static NetworkConfig Instance(FileInfo interfacesFile)
        {
            Assert.NotNull ( interfacesFile, nameof(interfacesFile));

            if (!_Instances.ContainsKey(interfacesFile.FullName))
                _Instances.Add(interfacesFile.FullName, new DebianNetworkConfig(interfacesFile));

            return _Instances[interfacesFile.FullName];
        }
    }
}

