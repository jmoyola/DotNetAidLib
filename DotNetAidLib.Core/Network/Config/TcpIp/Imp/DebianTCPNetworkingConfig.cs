using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using DotNetAidLib.Core.Proc.ServiceDaemon;
using System.Text;
using DotNetAidLib.Core.Network.Config.TcpIp.Core;
using DotNetAidLib.Core.Proc.ServiceDaemon.Core;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Network.Config.TcpIp.Imp
{
    public class DebianTCPNetworkingConfig : TCPNetworkingConfig
    {
        private static IDictionary<String, TCPNetworkingConfig> _Instances = new Dictionary<String, TCPNetworkingConfig>();

        // Crear el usuario que podrá reiniciar el servicio creando un archivo sudoers '/etc/sudoers.d/<NombreUsuario>':
        // cisco ALL=NOPASSWD: /etc/init.d/networking
        // No olvidar poner permisos al archivo: sudo chmod 440 /etc/sudoers.d/<NombreUsuario>
        //
        private FileInfo _SudoCommandFile = new FileInfo("/usr/bin/sudo");
        private FileInfo _InterfacesFile = null;

        private DebianTCPNetworkingConfig(FileInfo interfacesFile)
            : base()
        {
            Assert.NotNull(nameof(interfacesFile), interfacesFile);
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
                    foreach (TCPInterfaceConfig iface in this)
                    {
                        if (iface.Enabled)
                            sw.AppendLine("auto " + iface.Name);
                        if (iface.HotPlug)
                            sw.AppendLine("allow-hotplug " + iface.Name);

                        sw.AppendLine("iface"
                            + " " + iface.Name
                            + (iface.AddressFamily.Equals(AddressFamily.InterNetworkV6) ? " inet6" : " inet")
                            + " " + iface.Type.ToString().ToLower());

                        if (!iface.Type.Equals(TCPInterfaceConfigType.Dhcp))
                        {
                            if (iface.IP != null)
                                sw.AppendLine("    address " + iface.IP.ToString());
                            if (iface.NetMask != null)
                                sw.AppendLine("    netmask " + iface.NetMask.ToString());
                            if (iface.Network != null)
                                sw.AppendLine("    network " + iface.Network.ToString());
                            if (iface.Broadcast != null)
                                sw.AppendLine("    broadcast " + iface.Broadcast.ToString());
                            if (iface.Gateway != null)
                            {
                                if (!this.AllowMultiGateway)
                                    sw.AppendLine("    gateway " + iface.Gateway.ToString());
                                else
                                {
                                    sw.AppendLine("    post-up route add default gw " + iface.Gateway.ToString() + " metric 1");
                                    sw.AppendLine("    pre-down route del default gw " + iface.Gateway.ToString());
                                }
                            }
                            if (iface.Dns.Count > 0)
                            {
                                sw.AppendLine("    dns-nameservers " + iface.Dns.ToStringJoin(" "));
                            }
                        }

                        foreach (KeyValue<String, Object> kv in iface.Attributes)
                            if (kv.Value == null)
                                sw.AppendLine("    " + kv.Key);
                            else
                                sw.AppendLine("    " + kv.Key + " " + kv.Value.ToString());

                        sw.AppendLine("");
                    }

                    FileInfo tmpInterfacesFile = new FileInfo(".").RandomTempFile(".tmp");
                    System.IO.File.AppendAllText(tmpInterfacesFile.FullName, sw.ToString());
                    OperatingSystem.Imp.LinuxOperatingSystem.Instance().AdminExecute(new FileInfo(".").FromPathEnvironmentVariable("chown"), "root:root " + tmpInterfacesFile.FullName);
                    OperatingSystem.Imp.LinuxOperatingSystem.Instance().AdminExecute(new FileInfo(".").FromPathEnvironmentVariable("chmod"), "775 " + tmpInterfacesFile.FullName);
                    OperatingSystem.Imp.LinuxOperatingSystem.Instance().AdminExecute(new FileInfo(".").FromPathEnvironmentVariable("mv"), _InterfacesFile.FullName + " " + _InterfacesFile.FullName + ".tmp");
                    OperatingSystem.Imp.LinuxOperatingSystem.Instance().AdminExecute(new FileInfo(".").FromPathEnvironmentVariable("mv"), tmpInterfacesFile.FullName + " " + _InterfacesFile.FullName);

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

                    DebianTCPInterfaceConfig interf = null;
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

                            interf = new DebianTCPInterfaceConfig(addressFamily);

                            interf.Name = ifaceRegMatch.Groups[1].Value;
                            interf.Type = (TCPInterfaceConfigType)Enum.Parse(typeof(TCPInterfaceConfigType), ifaceRegMatch.Groups[3].Value.Trim(), true);

                            interf.Enabled = Regex.IsMatch(intFile, @"(allow\-)?auto\s+" + interf.Name, RegexOptions.Multiline);
                            interf.HotPlug = Regex.IsMatch(intFile, @"(allow\-)?hotplug\s+" + interf.Name, RegexOptions.Multiline);

                            this.Add(interf);

                            continue;
                        }
                        else if (ifacePropertyMatch.Success && interf != null)
                        {
                            String key = ifacePropertyMatch.Groups[2].Value;
                            String value = ifacePropertyMatch.Groups[3].Value;

                            if (Regex.IsMatch(key, @"(allow\-)?auto")
                                || Regex.IsMatch(key, @"(allow\-)?hotplug"))
                                continue;

                            if (key.Equals("address"))
                                interf.IP = IPAddress.Parse(value);
                            else if (key.Equals("netmask"))
                                interf.NetMask = IPAddress.Parse(value);
                            else if (key.Equals("network"))
                                interf.Network = IPAddress.Parse(value);
                            else if (key.Equals("broadcast"))
                                interf.Broadcast = IPAddress.Parse(value);
                            else if (key.Equals("gateway"))
                                interf.Gateway = IPAddress.Parse(value);
                            else if (key.Equals("dns-nameservers"))
                            {
                                foreach (String v in value.Split(' '))
                                    interf.Dns.Add(IPAddress.Parse(v));
                            }
                            else
                                interf.Attributes.Add(new KeyValue<string, object>(key, value));
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

        public override void Apply(TCPInterfaceConfig tcpInterface)
        {
            lock (oLock)
            {
                tcpInterface.NetworkInterfaceInfo.Disable();
                this.Save();
                tcpInterface.NetworkInterfaceInfo.Enable();
            }
        }

        public new static TCPNetworkingConfig Instance()
        {
            return Instance(new FileInfo("/etc/network/interfaces"));
        }

        public static TCPNetworkingConfig Instance(FileInfo interfacesFile)
        {
            Assert.NotNull(nameof(interfacesFile), interfacesFile);

            if (!_Instances.ContainsKey(interfacesFile.FullName))
                _Instances.Add(interfacesFile.FullName, new DebianTCPNetworkingConfig(interfacesFile));

            return _Instances[interfacesFile.FullName];
        }
    }
}

