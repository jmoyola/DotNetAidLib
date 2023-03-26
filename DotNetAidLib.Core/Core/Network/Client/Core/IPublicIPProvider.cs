using System.Net;

namespace DotNetAidLib.Core.Network.Client.Core
{
    public interface IPublicIPProvider
    {
        int PreferentOrder { get; }
        IPAddress Request();
    }
}