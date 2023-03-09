using System.Net.Sockets;

namespace Joker.Connections
{
    public class UnixDomainSocketEndPointData : EndPointData
    {
        public UnixDomainSocketEndPoint? Point => EndPoint as UnixDomainSocketEndPoint;
    }

}
