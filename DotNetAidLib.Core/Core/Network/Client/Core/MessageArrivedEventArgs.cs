using System;
using System.Net;

namespace DotNetAidLib.Core.Network.Client.Core
{
    public class MessageArrivedEventArgs : EventArgs
    {
        public MessageArrivedEventArgs(IPEndPoint ipEndPoint, byte[] message)
        {
            IPEndPoint = ipEndPoint;
            Message = message;
        }

        public byte[] Message { get; }

        public IPEndPoint IPEndPoint { get; }
    }
}