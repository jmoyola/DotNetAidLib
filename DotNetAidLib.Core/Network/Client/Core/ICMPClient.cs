using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Net.NetworkInformation;
using DotNetAidLib.Core.Context;
using System.Text;

namespace DotNetAidLib.Core.Network.Client.Core{
	
	public class ICMPClient
	{
		private const string Data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

		public static PingReply Ping(string hostNameOrAddress, int ttl=100, bool dontFragment=true, int timeout=5000, byte[] pingContent=null){
			Ping ping = new Ping ();
			PingOptions pingOptions = new PingOptions (ttl, dontFragment);
			if(pingContent==null || pingContent.Length==0)
				pingContent = System.Text.UTF8Encoding.UTF8.GetBytes("PingTest");
			return ping.Send (hostNameOrAddress, timeout, pingContent, pingOptions);
		}
      
        public static IEnumerable<IPAddress> TraceRoute(string hostNameOrAddress)
        {
			return TraceRoute(hostNameOrAddress, 1);
        }

        private static IEnumerable<IPAddress> TraceRoute(string hostNameOrAddress, int ttl)
        {
            Ping pinger = new Ping();
            PingOptions pingerOptions = new PingOptions(ttl, true);
            int timeout = 10000;
            byte[] buffer = Encoding.ASCII.GetBytes(Data);
            PingReply reply = default(PingReply);

            reply = pinger.Send(hostNameOrAddress, timeout, buffer, pingerOptions);

            List<IPAddress> result = new List<IPAddress>();
            if (reply.Status == IPStatus.Success)
            {
                result.Add(reply.Address);
            }
            else if (reply.Status == IPStatus.TtlExpired || reply.Status == IPStatus.TimedOut)
            {
                //add the currently returned address if an address was found with this TTL
                if (reply.Status == IPStatus.TtlExpired) result.Add(reply.Address);
                //recurse to get the next address...
                IEnumerable<IPAddress> tempResult = default(IEnumerable<IPAddress>);
                tempResult = TraceRoute(hostNameOrAddress, ttl + 1);
                result.AddRange(tempResult);
            }
            else
            {
                //failure 
            }

            return result;
        }
	}
}