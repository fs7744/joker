using System.Net;

namespace Joker.Connections
{
    public class UdpEndPointData : EndPointData
    {
        public IPEndPoint? Point => EndPoint as IPEndPoint;
    }

}
