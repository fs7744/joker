using System.Net;

namespace Joker.Connections
{
    public class TcpEndPointData : EndPointData
    {
        public IPEndPoint? Point => EndPoint as IPEndPoint;
    }
}