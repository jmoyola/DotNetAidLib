using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Files;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Streams;
using DotNetAidLib.Network.Config.TcpIp.Core;
using DotNetAidLib.Services.Core;

namespace DotNetAidLib.Network.Config.TcpIp.Imp
{
    public class DebianTCPNetworkingConfig : TCPNetworkingConfig
    {
        private static readonly IDictionary<string, TCPNetworkingConfig> _Instances =
            new Dictionary<string, TCPNetworkingConfig>();

        private static readonly Regex ifaceReg = new Regex(@"^\s*iface\s+([a-zA-Z0-9\.\:]+)\s+([^\s]+)\s+([^\s]+)\s*$");
        private static readonly Regex ifaceProperty = new Regex(@"^(\t|\s)*([a-zA-Z0-9_\-]+)\s+([^\n]+)$");
        private readonly FileInfo _InterfacesFile;

        // Crear el usuario que podrá reiniciar el servicio creando un archivo sudoers '/etc/sudoers.d/<NombreUsuario>':
        // cisco ALL=NOPASSWD: /etc/init.d/networking
        // No olvidar poner permisos al archivo: sudo chmod 440 /etc/sudoers.d/<NombreUsuario>
        //
        private FileInfo _SudoCommandFile = new FileInfo("/usr/bin/sudo");

        private readonly object oLock = new object();

        private DebianTCPNetworkingConfig(FileInfo interfacesFile)
        {
            Assert.NotNull(interfacesFile, nameof(interfacesFile));
            _InterfacesFile = interfacesFile;
        }

        private bool AllowMultiGateway { get; } = false;

        public override void Save()
        {
            lock (oLock)
            {
                StringBuilder sw = null;
                try
                {
                    sw = new StringBuilder();
                    foreach (var iface in this)
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
                                sw.AppendLine("    address " + iface.IP);
                            if (iface.NetMask != null)
                                sw.AppendLine("    netmask " + iface.NetMask);
                            if (iface.Network != null)
                                sw.AppendLine("    network " + iface.Network);
                            if (iface.Broadcast != null)
                                sw.AppendLine("    broadcast " + iface.Broadcast);
                            if (iface.Gateway != null)
                            {
                                if (!AllowMultiGateway)
                                {
                                    sw.AppendLine("    gateway " + iface.Gateway);
                                }
                                else
                                {
                                    sw.AppendLine("    post-up route add default gw " + iface.Gateway + " metric 1");
                                    sw.AppendLine("    pre-down route del default gw " + iface.Gateway);
                                }
                            }

                            if (iface.Dns.Count > 0)
                                sw.AppendLine("    dns-nameservers " + iface.Dns.ToStringJoin(" "));
                        }

                        foreach (var kv in iface.Attributes)
                            if (kv.Value == null)
                                sw.AppendLine("    " + kv.Key);
                            else
                                sw.AppendLine("    " + kv.Key + " " + kv.Value);

                        sw.AppendLine("");
                    }

                    var tmpInterfacesFile = new FileInfo(".").RandomTempFile(".tmp");
                    File.AppendAllText(tmpInterfacesFile.FullName, sw.ToString());
                    EnvironmentHelper.SearchInPath("chown").CmdExecuteSync("root:root " + tmpInterfacesFile.FullName);
                    EnvironmentHelper.SearchInPath("chmod").CmdExecuteSync("775 " + tmpInterfacesFile.FullName);
                    EnvironmentHelper.SearchInPath("mv")
                        .CmdExecuteSync(_InterfacesFile.FullName + " " + _InterfacesFile.FullName + ".tmp");
                    EnvironmentHelper.SearchInPath("mv")
                        .CmdExecuteSync(tmpInterfacesFile.FullName + " " + _InterfacesFile.FullName);

                    //OperatingSystem.Imp.LinuxOperatingSystem.Instance().AdminTextWrite(_InterfacesFile, sw.ToString());

                    _InterfacesFile.Refresh();
                }
                catch (Exception ex)
                {
                    throw new NetworkingException("Error saving interfaces configuration.", ex);
                }
            }
        }

        public override void Load()
        {
            lock (oLock)
            {
                try
                {
                    var sr = _InterfacesFile.OpenText();
                    var intFile = sr.ReadToEnd(true);

                    Clear();

                    DebianTCPInterfaceConfig interf = null;
                    foreach (var line in intFile.GetLines().SlashEndContinueLines()
                                 .Select(v => v.Trim())
                                 .Where(v => !string.IsNullOrEmpty(v)
                                             && !v.StartsWith("#", StringComparison.InvariantCulture)))
                    {
                        var ifaceRegMatch = ifaceReg.Match(line);
                        var ifacePropertyMatch = ifaceProperty.Match(line);

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
                            interf.Type = (TCPInterfaceConfigType) Enum.Parse(typeof(TCPInterfaceConfigType),
                                ifaceRegMatch.Groups[3].Value.Trim(), true);

                            interf.Enabled = Regex.IsMatch(intFile, @"(allow\-)?auto\s+" + interf.Name,
                                RegexOptions.Multiline);
                            interf.HotPlug = Regex.IsMatch(intFile, @"(allow\-)?hotplug\s+" + interf.Name,
                                RegexOptions.Multiline);

                            Add(interf);

                            continue;
                        }

                        if (ifacePropertyMatch.Success && interf != null)
                        {
                            var key = ifacePropertyMatch.Groups[2].Value;
                            var value = ifacePropertyMatch.Groups[3].Value;

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
                                foreach (var v in value.Split(' '))
                                    interf.Dns.Add(IPAddress.Parse(v));
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
                Save();
                tcpInterface.NetworkInterfaceInfo.Enable();
            }
        }

        public new static TCPNetworkingConfig Instance()
        {
            return Instance(new FileInfo("/etc/network/interfaces"));
        }

        public static TCPNetworkingConfig Instance(FileInfo interfacesFile)
        {
            Assert.NotNull(interfacesFile, nameof(interfacesFile));

            if (!_Instances.ContainsKey(interfacesFile.FullName))
                _Instances.Add(interfacesFile.FullName, new DebianTCPNetworkingConfig(interfacesFile));

            return _Instances[interfacesFile.FullName];
        }
    }
}