using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Network.Config.TcpIp.Imp;

namespace DotNetAidLib.Core.Network.Info.Core
{
    public enum NetworkInterfaceType{
        [Description("Unknow")]
        UNKNOW = 16384,
        [Description("       from KA9Q: NET/ROM pseudo    ")]
        NETROM = 0,
        [Description("       Ethernet 10Mbps      ")]
        ETHER = 1,
        [Description("       Experimental Ethernet    ")]
        EETHER = 2,
        [Description("       AX.25 Level 2        ")]
        AX25 = 3,
        [Description("       PROnet token ring        ")]
        PRONET = 4,
        [Description("       Chaosnet         ")]
        CHAOS = 5,
        [Description("       IEEE 802.2 Ethernet/TR/TB    ")]
        IEEE802 = 6,
        [Description("       ARCnet           ")]
        ARCNET = 7,
        [Description("       APPLEtalk            ")]
        APPLETLK = 8,
        [Description("       Frame Relay DLCI     ")]
        DLCI = 15,
        [Description("       ATM              ")]
        ATM = 19,
        [Description("       Metricom STRIP (new IANA id) ")]
        METRICOM = 23,
        [Description("       IEEE 1394 IPv4 - RFC 2734    ")]
        IEEE1394 = 24,
        [Description("       EUI-64                       ")]
        EUI64 = 27,
        [Description("       InfiniBand           ")]
        INFINIBAND = 32,
        // Dummy types for non ARP hardware
        [Description("")]
        SLIP = 256,
        [Description("")]
        CSLIP = 257,
        [Description("")]
        SLIP6 = 258,
        [Description("")]
        CSLIP6 = 259,
        [Description("       Notional KISS type       ")]
        RSRVD = 260,
        [Description("")]
        ADAPT = 264,
        [Description("")]
        ROSE = 270,
        [Description("       CCITT X.25           ")]
        X25 = 271,
        [Description("       Boards with X.25 in firmware ")]
        HWX25 = 272,
        [Description("       Controller Area Network      ")]
        CAN = 280,
        [Description("")]
        PPP = 512,
        [Description("       Cisco                ")]
        CISCO = 513,
        [Description("       Cisco HDLC           ")]
        HDLC = CISCO,
        [Description("       LAPB             ")]
        LAPB = 516,
        [Description("       Digital's DDCMP protocol     ")]
        DDCMP = 517,
        [Description("       Raw HDLC         ")]
        RAWHDLC = 518,
        [Description("       Raw IP                       ")]
        RAWIP = 519,
        [Description("       IPIP tunnel          ")]
        TUNNEL = 768,
        [Description("       IP6IP6 tunnel            ")]
        TUNNEL6 = 769,
        [Description("              Frame Relay Access Device    ")]
        FRAD = 770,
        [Description("       SKIP vif         ")]
        SKIP = 771,
        [Description("       Loopback device      ")]
        LOOPBACK = 772,
        [Description("       Localtalk device     ")]
        LOCALTLK = 773,
        [Description("       Fiber Distributed Data Interface ")]
        FDDI = 774,
        [Description("              AP1000 BIF                   ")]
        BIF = 775,
        [Description("       sit0 device - IPv6-in-IPv4   ")]
        SIT = 776,
        [Description("       IP over DDP tunneller    ")]
        IPDDP = 777,
        [Description("       GRE over IP          ")]
        IPGRE = 778,
        [Description("       PIMSM register interface ")]
        PIMREG = 779,
        [Description("       High Performance Parallel Interface ")]
        HIPPI = 780,
        [Description("       Nexus 64Mbps Ash     ")]
        ASH = 781,
        [Description("       Acorn Econet         ")]
        ECONET = 782,
        [Description("       Linux-IrDA           ")]
        IRDA = 783,
        // ARP works differently on different FC media .. so  
        [Description("       Point to point fibrechannel  ")]
        FCPP = 784,
        [Description("       Fibrechannel arbitrated loop ")]
        FCAL = 785,
        [Description("       Fibrechannel public loop ")]
        FCPL = 786,
        [Description("       Fibrechannel fabric      ")]
        FCFABRIC = 787,
        // 787->799 reserved for fibrechannel media types
        [Description("       Magic type ident for TR  ")]
        IEEE802_TR = 800,
        [Description("       IEEE 802.11          ")]
        IEEE80211 = 801,
        [Description("   IEEE 802.11 + Prism2 header  ")]
        IEEE80211_PRISM = 802,
        [Description("   IEEE 802.11 + radiotap header ")]
        IEEE80211_RADIOTAP = 803,
        [Description("")]
        IEEE802154 = 804,
        [Description("   IEEE 802.15.4 network monitor ")]
        IEEE802154_MONITOR = 805,
        [Description("       PhoNet media type        ")]
        PHONET = 820,
        [Description("       PhoNet pipe header       ")]
        PHONET_PIPE = 821,
        [Description("       CAIF media type      ")]
        CAIF = 822,
        [Description("       GRE over IPv6        ")]
        IP6GRE = 823,
        [Description("       Netlink header       ")]
        NETLINK = 824,
        [Description("       IPv6 over LoWPAN             ")]
        IP6LOWPAN = 825,
        [Description("   Vsock monitor header     ")]
        VSOCKMON = 826
    }
}
