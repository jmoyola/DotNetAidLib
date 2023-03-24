using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace DotNetAidLib.Core.Network.Client.Core
{


	public class NTPClient
	{
		private int m_TimeOut = 30000;
		private TimeZoneInfo m_timeZone = TimeZoneInfo.Local;

		private string m_NtpServer = "hora.roa.es";

		public NTPClient()
		{
		}

		public NTPClient(string NtpServer)
		{
			this.NtpServer = NtpServer;
		}

		public int TimeOut {
			get { return m_TimeOut; }
			set { m_TimeOut = value; }
		}

		public TimeZoneInfo TimeZone {
			get { return m_timeZone; }
			set { m_timeZone = value; }
		}

		public string NtpServer {
			get { return m_NtpServer; }
			set {
				try {
					Dns.GetHostEntry(value);
					m_NtpServer = value;
				} catch {
					throw new Exception("Invalid value for NtpServer (only valid dns name or Ip are allowed).");
				}

			}
		}

		public DateTime GetLocalNetworkTime()
		{
			return GetLocalNetworkTime(m_NtpServer, m_timeZone, m_TimeOut);
		}

		public DateTime GetUTCNetworkTime()
		{
			return GetUTCNetworkTime(m_NtpServer, m_TimeOut);
		}

		public static DateTime GetUTCNetworkTime(string ntpServer, int timeOut)
		{
			try {
				byte[] requestNtpData = new byte[48];
				byte[] responseNtpData = new byte[1025];

				requestNtpData[0] = 0x1b;
				//LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

				IPAddress[] addresses = Dns.GetHostEntry(ntpServer).AddressList;
				EndPoint ipEndPoint = new IPEndPoint(addresses[0], 123);
				Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

				socket.Connect(ipEndPoint);
				socket.Send(requestNtpData);
				socket.ReceiveTimeout = timeOut;
				int bytesReceived = socket.Receive(responseNtpData);
				if ((bytesReceived != 48)) {
					throw new Exception("Unknow response from NTP Server '");
				}

				socket.Close();

				ulong intPart = Convert.ToUInt64(responseNtpData[40]) << 24 | Convert.ToUInt64(responseNtpData[41]) << 16 | Convert.ToUInt64(responseNtpData[42]) << 8 | Convert.ToUInt64(responseNtpData[43]);
				ulong fractPart = Convert.ToUInt64(responseNtpData[44]) << 24 | Convert.ToUInt64(responseNtpData[45]) << 16 | Convert.ToUInt64(responseNtpData[46]) << 8 | Convert.ToUInt64(responseNtpData[47]);

				decimal milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
				DateTime networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds(Convert.ToInt64(milliseconds));

				return networkDateTime;
			} catch (Exception ex) {
				throw new Exception("Error retrieving UTC Network Datetime from NTP Server '" + ntpServer + "':\r\n" + ex.ToString());
			}

		}

		public static DateTime GetLocalNetworkTime(string ntpServer, int timeOut)
		{
			return GetLocalNetworkTime(ntpServer, TimeZoneInfo.Local, timeOut);
		}

		public static DateTime GetLocalNetworkTime(string ntpServer, TimeZoneInfo timeZone, int timeOut)
		{
			try {
				DateTime networkDateTime = GetUTCNetworkTime(ntpServer, timeOut);

				//Desplazamiento horario
				TimeSpan offspan = timeZone.GetUtcOffset(DateTime.Now);

				return networkDateTime.Add(offspan);
			} catch (Exception ex) {
				throw new Exception("Error retrieving local Network Datetime:\r\n" + ex.ToString());
			}
		}







	}
}